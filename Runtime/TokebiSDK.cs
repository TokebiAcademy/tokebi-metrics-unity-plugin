using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tokebi
{
    [System.Serializable]
    public struct EventData
    {
        public string eventType;
        public string payloadJson;
        public long timestamp;
        public string playerId;
        
        public EventData(string eventType, string payloadJson, long timestamp, string playerId)
        {
            this.eventType = eventType;
            this.payloadJson = payloadJson;
            this.timestamp = timestamp;
            this.playerId = playerId;
        }
    }

    public class TokebiSDK : MonoBehaviour
    {
        public static TokebiSDK Instance { get; private set; }

        [Header("Configuration")]
        public string apiKey = ""; // Set this in inspector or via code
        
        private const string ENDPOINT = "https://tokebi-api.vercel.app";
        private const float FLUSH_INTERVAL = 60f; // 1 minute like Unity Analytics
        private const int MAX_BATCH_SIZE = 100;
        private const int MAX_QUEUE_SIZE = 1000; // Prevent memory issues

        private string gameName;
        private string gameId = "";
        private string playerId = "";
        private bool isMultiplayerClient = false;
        
        // Batching system
        private Queue<EventData> eventQueue = new Queue<EventData>();
        private float lastFlushTime;
        private bool isFlushingEvents = false;
        
        // Pre-allocated objects to minimize GC
        private StringBuilder jsonBuilder = new StringBuilder(8192);
        private UnityWebRequest pooledRequest;
        private DownloadHandlerBuffer pooledDownloadHandler;
        
        // Cached strings to avoid allocations
        private readonly Dictionary<string, string> escapedStringCache = new Dictionary<string, string>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            Debug.Log("[Tokebi] Initializing SDK v2.0 (Batched)...");
            gameName = Application.productName;
            Debug.Log("[Tokebi] Game Name: " + gameName);
            playerId = GetOrCreatePlayerId();
            Debug.Log("[Tokebi] Player ID: " + playerId);
            
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("[Tokebi] API Key not set! Please set it in the TokebiSDK component or via code.");
                return;
            }
            
            // Initialize pooled objects
            InitializeNetworkObjects();
            
            DetectMultiplayerMode();
            StartCoroutine(RegisterGame());
            
            // Start the flush timer
            lastFlushTime = Time.time;
            StartCoroutine(FlushTimer());
        }

        public void SetApiKey(string key)
        {
            apiKey = key;
        }

        private void InitializeNetworkObjects()
        {
            // Create reusable network objects
            pooledRequest = new UnityWebRequest();
            pooledDownloadHandler = new DownloadHandlerBuffer();
            
            pooledRequest.downloadHandler = pooledDownloadHandler;
        }

        private void DetectMultiplayerMode()
        {
            Debug.Log("[Tokebi] Mode: Single Player (default)");
            Debug.Log("[Tokebi] Call TokebiSDK.Instance.SetMultiplayerMode(isClient) for multiplayer games");
        }

        public void SetMultiplayerMode(bool isClient)
        {
            isMultiplayerClient = isClient;
            if (isClient)
            {
                Debug.Log("[Tokebi] Mode: Multiplayer Client - Will NOT track");
                gameId = "client_no_track";
            }
            else
            {
                Debug.Log("[Tokebi] Mode: Multiplayer Host/Server - Will track");
                if (gameId == "client_no_track")
                {
                    gameId = "";
                    StartCoroutine(RegisterGame());
                }
            }
        }

        private string GetOrCreatePlayerId()
        {
            string filePath = Path.Combine(Application.persistentDataPath, "tokebi_player_id");
            if (File.Exists(filePath))
            {
                string existingId = File.ReadAllText(filePath).Trim();
                if (!string.IsNullOrEmpty(existingId))
                    return existingId;
            }
            string newId = "player_" + System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "_" + UnityEngine.Random.Range(1000, 9999);
            File.WriteAllText(filePath, newId);
            return newId;
        }

        private IEnumerator RegisterGame()
        {
            if (gameId == "client_no_track")
                yield break;
                
            string jsonData = "{\"gameName\":\"" + EscapeJsonCached(gameName) + "\",\"platform\":\"unity\"}";
            using (UnityWebRequest request = new UnityWebRequest(ENDPOINT + "/api/games", "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Authorization", apiKey);
                request.SetRequestHeader("Content-Type", "application/json");
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    if (responseText.Contains("game_id"))
                    {
                        int startIndex = responseText.IndexOf("game_id") + 10;
                        int endIndex = responseText.IndexOf(",", startIndex);
                        if (endIndex == -1) endIndex = responseText.IndexOf("}", startIndex);
                        if (startIndex > 9 && endIndex > startIndex)
                        {
                            string gameIdStr = responseText.Substring(startIndex, endIndex - startIndex).Trim().Replace("\"", "");
                            gameId = gameIdStr;
                            Debug.Log("[Tokebi] Game registered! ID: " + gameId);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[Tokebi] Registration failed: " + request.error);
                }
            }
        }

        // BATCHED EVENT TRACKING - Main public API
        public void Track(string eventType, Dictionary<string, object> payload = null, bool forceTrack = false)
        {
            if (gameId == "client_no_track" && !forceTrack)
            {
                Debug.Log("[Tokebi] Skipping (client): " + eventType);
                return;
            }
            
            if (string.IsNullOrEmpty(gameId) || string.IsNullOrEmpty(playerId))
                return;

            // Prevent memory issues
            if (eventQueue.Count >= MAX_QUEUE_SIZE)
            {
                Debug.LogWarning("[Tokebi] Event queue full, dropping oldest events");
                for (int i = 0; i < 100; i++) // Drop 100 oldest events
                {
                    if (eventQueue.Count > 0) eventQueue.Dequeue();
                }
            }

            // Create payload JSON efficiently
            string payloadJson = CreatePayloadJsonEfficient(payload);
            long timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            // Add to queue - no network call yet
            eventQueue.Enqueue(new EventData(eventType, payloadJson, timestamp, playerId));
            
            Debug.Log("[Tokebi] Queued: " + eventType + " (Queue: " + eventQueue.Count + ")");
            
            // Force flush if batch is very large
            if (eventQueue.Count >= MAX_BATCH_SIZE)
            {
                StartCoroutine(FlushEventsToServer());
            }
        }

        // Timer-based flushing - runs every minute like Unity Analytics
        private IEnumerator FlushTimer()
        {
            while (true)
            {
                yield return new WaitForSeconds(FLUSH_INTERVAL);
                
                if (eventQueue.Count > 0 && !isFlushingEvents)
                {
                    StartCoroutine(FlushEventsToServer());
                }
            }
        }

        // Flush events to server in batches
        private IEnumerator FlushEventsToServer()
        {
            if (isFlushingEvents || eventQueue.Count == 0)
                yield break;
                
            isFlushingEvents = true;
            lastFlushTime = Time.time;
            
            Debug.Log("[Tokebi] Flushing " + eventQueue.Count + " events...");
            
            // Build batch JSON efficiently
            jsonBuilder.Clear();
            jsonBuilder.Append("{\"events\":[");
            
            bool first = true;
            List<EventData> currentBatch = new List<EventData>();
            
            // Process events in batches
            while (eventQueue.Count > 0 && currentBatch.Count < MAX_BATCH_SIZE)
            {
                EventData eventData = eventQueue.Dequeue();
                currentBatch.Add(eventData);
                
                if (!first) jsonBuilder.Append(",");
                first = false;
                
                // Build individual event JSON
                jsonBuilder.Append("{\"eventType\":\"").Append(EscapeJsonCached(eventData.eventType)).Append("\"");
                jsonBuilder.Append(",\"payload\":").Append(eventData.payloadJson);
                jsonBuilder.Append(",\"gameId\":\"").Append(EscapeJsonCached(gameId)).Append("\"");
                jsonBuilder.Append(",\"playerId\":\"").Append(EscapeJsonCached(eventData.playerId)).Append("\"");
                jsonBuilder.Append(",\"timestamp\":").Append(eventData.timestamp);
                jsonBuilder.Append(",\"platform\":\"unity\"");
                jsonBuilder.Append(",\"environment\":\"").Append(Application.isEditor ? "development" : "production").Append("\"");
                jsonBuilder.Append("}");
            }
            
            jsonBuilder.Append("]}");
            
            // Send batch using pooled objects
            yield return SendBatchedEvents(jsonBuilder.ToString(), currentBatch.Count);
            
            isFlushingEvents = false;
            
            // Continue flushing if more events remain
            if (eventQueue.Count > 0)
            {
                StartCoroutine(FlushEventsToServer());
            }
        }

        private IEnumerator SendBatchedEvents(string batchJson, int eventCount)
        {
            // Use existing /api/track endpoint (now supports both single and batch)
            pooledRequest.url = ENDPOINT + "/api/track";
            pooledRequest.method = "POST";
            
            // Create new upload handler each time (can't reuse)
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(batchJson);
            if (pooledRequest.uploadHandler != null)
            {
                pooledRequest.uploadHandler.Dispose();
            }
            pooledRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            
            pooledRequest.SetRequestHeader("Authorization", apiKey);
            pooledRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return pooledRequest.SendWebRequest();
            
            if (pooledRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[Tokebi] Successfully sent batch of " + eventCount + " events");
            }
            else
            {
                Debug.LogWarning("[Tokebi] Batch send failed: " + pooledRequest.error);
                // TODO: Could implement retry logic here
            }
        }

        // Efficient JSON creation with minimal allocations
        private string CreatePayloadJsonEfficient(Dictionary<string, object> payload)
        {
            if (payload == null || payload.Count == 0)
                return "{}";
            
            // Use a smaller builder for payloads
            var payloadBuilder = new StringBuilder(512);
            payloadBuilder.Append("{");
            
            bool first = true;
            foreach (var kvp in payload)
            {
                if (!first) payloadBuilder.Append(",");
                first = false;
                
                payloadBuilder.Append("\"").Append(EscapeJsonCached(kvp.Key)).Append("\":");
                
                if (kvp.Value == null)
                    payloadBuilder.Append("null");
                else if (kvp.Value is string)
                    payloadBuilder.Append("\"").Append(EscapeJsonCached(kvp.Value.ToString())).Append("\"");
                else if (kvp.Value is bool)
                    payloadBuilder.Append(kvp.Value.ToString().ToLower());
                else
                    payloadBuilder.Append(kvp.Value.ToString());
            }
            
            payloadBuilder.Append("}");
            return payloadBuilder.ToString();
        }

        // Cached string escaping to avoid repeated allocations
        private string EscapeJsonCached(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
                
            if (escapedStringCache.TryGetValue(str, out string cachedResult))
                return cachedResult;
            
            // Only escape if needed
            if (str.IndexOfAny(new char[] { '\\', '"', '\n', '\r', '\t' }) == -1)
            {
                escapedStringCache[str] = str;
                return str;
            }
            
            string escaped = str.Replace("\\", "\\\\")
                               .Replace("\"", "\\\"")
                               .Replace("\n", "\\n")
                               .Replace("\r", "\\r")
                               .Replace("\t", "\\t");
            
            // Cache for future use
            if (escapedStringCache.Count < 1000) // Prevent memory growth
                escapedStringCache[str] = escaped;
                
            return escaped;
        }

        // Force flush - useful for app quit scenarios
        public void FlushEvents()
        {
            if (eventQueue.Count > 0)
            {
                StartCoroutine(FlushEventsToServer());
            }
        }

        // Convenience methods
        public void TrackLevelStart(string level)
        {
            var payload = new Dictionary<string, object> { ["level"] = level };
            Track("level_start", payload);
        }

        public void TrackLevelComplete(string level, float timeTaken)
        {
            var payload = new Dictionary<string, object> { ["level"] = level, ["time"] = timeTaken };
            Track("level_complete", payload);
        }

        public void TrackClientEvent(string eventType, Dictionary<string, object> payload = null)
        {
            Track(eventType, payload, true);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) // App is being paused
            {
                FlushEvents();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) // App lost focus
            {
                FlushEvents();
            }
        }

        private void OnDestroy()
        {
            FlushEvents();
            
            // Clean up pooled objects
            if (pooledRequest != null)
            {
                if (pooledRequest.uploadHandler != null)
                {
                    pooledRequest.uploadHandler.Dispose();
                }
                pooledRequest.Dispose();
            }
        }
    }
}
