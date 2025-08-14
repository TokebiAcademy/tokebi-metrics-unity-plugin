using UnityEngine;
using UnityEditor;
using System.IO;

namespace Tokebi
{
    public class TokebiInstaller : EditorWindow
    {
        private string apiKey = "";
        private bool isInstalling = false;
        private string statusMessage = "";

        [MenuItem("Tools/Install Tokebi Analytics SDK")]
        public static void ShowWindow()
        {
            TokebiInstaller window = GetWindow<TokebiInstaller>("Tokebi SDK Installer");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("🚀 Tokebi Analytics SDK Installer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            GUILayout.Label("Game: " + PlayerSettings.productName, EditorStyles.helpBox);
            EditorGUILayout.Space();

            GUILayout.Label("API Key:");
            apiKey = EditorGUILayout.PasswordField(apiKey);
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(isInstalling || string.IsNullOrEmpty(apiKey.Trim()));
            if (GUILayout.Button("Install SDK", GUILayout.Height(30)))
            {
                InstallSDK();
            }
            EditorGUI.EndDisabledGroup();

            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
            }

            if (isInstalling)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Installing SDK...", MessageType.Info);
            }
        }

        private void InstallSDK()
        {
            if (string.IsNullOrEmpty(apiKey.Trim()))
            {
                statusMessage = "ERROR: API Key is required";
                return;
            }

            isInstalling = true;
            statusMessage = "Installing Tokebi Analytics SDK...";

            try
            {
                CreateDirectories();
                CreateSDKScript();
                CreateEditorScript();
                AssetDatabase.Refresh();

                statusMessage = "✅ SDK installed successfully!\nAdd TokebiAnalytics prefab to your scene or call TokebiSDK.Instance from code.";
                isInstalling = false;

                Debug.Log("[Tokebi] SDK installed successfully!");
            }
            catch (System.Exception e)
            {
                statusMessage = "❌ Installation failed: " + e.Message;
                isInstalling = false;
                Debug.LogError("[Tokebi] Installation failed: " + e);
            }
        }

        private void CreateDirectories()
        {
            string scriptsPath = "Assets/Tokebi";
            string editorPath = "Assets/Tokebi/Editor";

            if (!Directory.Exists(scriptsPath))
                Directory.CreateDirectory(scriptsPath);

            if (!Directory.Exists(editorPath))
                Directory.CreateDirectory(editorPath);
        }

        private void CreateSDKScript()
        {
            string sdkContent = GetSDKContent();
            string path = "Assets/Tokebi/TokebiSDK.cs";
            File.WriteAllText(path, sdkContent);
            Debug.Log("📁 Created: " + path);
        }

        private void CreateEditorScript()
        {
            string editorContent = GetEditorContent();
            string path = "Assets/Tokebi/Editor/TokebiMenu.cs";
            File.WriteAllText(path, editorContent);
            Debug.Log("📁 Created: " + path);
        }

        private string GetSDKContent()
        {
            string content = "";
            content += "using UnityEngine;\n";
            content += "using UnityEngine.Networking;\n";
            content += "using System.Collections;\n";
            content += "using System.Collections.Generic;\n";
            content += "using System.IO;\n";
            content += "using System.Text;\n\n";
            content += "namespace Tokebi\n";
            content += "{\n";
            content += "    public class TokebiSDK : MonoBehaviour\n";
            content += "    {\n";
            content += "        public static TokebiSDK Instance { get; private set; }\n\n";
            content += "        private const string API_KEY = \"" + apiKey.Trim() + "\";\n";
            content += "        private const string ENDPOINT = \"https://tokebi-api.vercel.app\";\n\n";
            content += "        private string gameName;\n";
            content += "        private string gameId = \"\";\n";
            content += "        private string playerId = \"\";\n";
            content += "        private bool isMultiplayerClient = false;\n\n";
            content += GetAwakeMethod();
            content += GetInitializeMethod();
            content += GetMultiplayerMethods();
            content += GetPlayerIdMethod();
            content += GetRegistrationMethod();
            content += GetTrackingMethods();
            content += GetConvenienceMethods();
            content += "    }\n";
            content += "}\n";
            return content;
        }

        private string GetAwakeMethod()
        {
            string content = "";
            content += "        private void Awake()\n";
            content += "        {\n";
            content += "            if (Instance == null)\n";
            content += "            {\n";
            content += "                Instance = this;\n";
            content += "                DontDestroyOnLoad(gameObject);\n";
            content += "                Initialize();\n";
            content += "            }\n";
            content += "            else\n";
            content += "            {\n";
            content += "                Destroy(gameObject);\n";
            content += "            }\n";
            content += "        }\n\n";
            return content;
        }

        private string GetInitializeMethod()
        {
            string content = "";
            content += "        private void Initialize()\n";
            content += "        {\n";
            content += "            Debug.Log(\"[Tokebi] Initializing SDK...\");\n";
            content += "            gameName = Application.productName;\n";
            content += "            Debug.Log(\"[Tokebi] Game Name: \" + gameName);\n";
            content += "            playerId = GetOrCreatePlayerId();\n";
            content += "            Debug.Log(\"[Tokebi] Player ID: \" + playerId);\n";
            content += "            DetectMultiplayerMode();\n";
            content += "            StartCoroutine(RegisterGame());\n";
            content += "        }\n\n";
            return content;
        }

        private string GetMultiplayerMethods()
        {
            string content = "";
            content += "        private void DetectMultiplayerMode()\n";
            content += "        {\n";
            content += "            Debug.Log(\"[Tokebi] Mode: Single Player (default)\");\n";
            content += "            Debug.Log(\"[Tokebi] Call TokebiSDK.Instance.SetMultiplayerMode(isClient) for multiplayer games\");\n";
            content += "        }\n\n";
            content += "        public void SetMultiplayerMode(bool isClient)\n";
            content += "        {\n";
            content += "            isMultiplayerClient = isClient;\n";
            content += "            if (isClient)\n";
            content += "            {\n";
            content += "                Debug.Log(\"[Tokebi] Mode: Multiplayer Client - Will NOT track\");\n";
            content += "                gameId = \"client_no_track\";\n";
            content += "            }\n";
            content += "            else\n";
            content += "            {\n";
            content += "                Debug.Log(\"[Tokebi] Mode: Multiplayer Host/Server - Will track\");\n";
            content += "                if (gameId == \"client_no_track\")\n";
            content += "                {\n";
            content += "                    gameId = \"\";\n";
            content += "                    StartCoroutine(RegisterGame());\n";
            content += "                }\n";
            content += "            }\n";
            content += "        }\n\n";
            return content;
        }

        private string GetPlayerIdMethod()
        {
            string content = "";
            content += "        private string GetOrCreatePlayerId()\n";
            content += "        {\n";
            content += "            string filePath = Path.Combine(Application.persistentDataPath, \"tokebi_player_id\");\n";
            content += "            if (File.Exists(filePath))\n";
            content += "            {\n";
            content += "                string existingId = File.ReadAllText(filePath).Trim();\n";
            content += "                if (!string.IsNullOrEmpty(existingId))\n";
            content += "                    return existingId;\n";
            content += "            }\n";
            content += "            string newId = \"player_\" + System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + \"_\" + UnityEngine.Random.Range(1000, 9999);\n";
            content += "            File.WriteAllText(filePath, newId);\n";
            content += "            return newId;\n";
            content += "        }\n\n";
            return content;
        }

        private string GetRegistrationMethod()
        {
            string content = "";
            content += "        private IEnumerator RegisterGame()\n";
            content += "        {\n";
            content += "            if (gameId == \"client_no_track\")\n";
            content += "                yield break;\n";
            content += "            string jsonData = \"{\\\"gameName\\\":\\\"\" + EscapeJson(gameName) + \"\\\",\\\"platform\\\":\\\"unity\\\"}\";\n";
            content += "            using (UnityWebRequest request = new UnityWebRequest(ENDPOINT + \"/api/games\", \"POST\"))\n";
            content += "            {\n";
            content += "                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);\n";
            content += "                request.uploadHandler = new UploadHandlerRaw(bodyRaw);\n";
            content += "                request.downloadHandler = new DownloadHandlerBuffer();\n";
            content += "                request.SetRequestHeader(\"Authorization\", API_KEY);\n";
            content += "                request.SetRequestHeader(\"Content-Type\", \"application/json\");\n";
            content += "                yield return request.SendWebRequest();\n";
            content += "                if (request.result == UnityWebRequest.Result.Success)\n";
            content += "                {\n";
            content += "                    string responseText = request.downloadHandler.text;\n";
            content += "                    if (responseText.Contains(\"game_id\"))\n";
            content += "                    {\n";
            content += "                        int startIndex = responseText.IndexOf(\"game_id\") + 10;\n";
            content += "                        int endIndex = responseText.IndexOf(\",\", startIndex);\n";
            content += "                        if (endIndex == -1) endIndex = responseText.IndexOf(\"}\", startIndex);\n";
            content += "                        if (startIndex > 9 && endIndex > startIndex)\n";
            content += "                        {\n";
            content += "                            string gameIdStr = responseText.Substring(startIndex, endIndex - startIndex).Trim().Replace(\"\\\"\", \"\");\n";
            content += "                            gameId = gameIdStr;\n";
            content += "                            Debug.Log(\"[Tokebi] Game registered! ID: \" + gameId);\n";
            content += "                        }\n";
            content += "                    }\n";
            content += "                }\n";
            content += "                else\n";
            content += "                {\n";
            content += "                    Debug.LogWarning(\"[Tokebi] Registration failed: \" + request.error);\n";
            content += "                }\n";
            content += "            }\n";
            content += "        }\n\n";
            return content;
        }

        private string GetTrackingMethods()
        {
            string content = "";
            content += "        public void Track(string eventType, Dictionary<string, object> payload = null, bool forceTrack = false)\n";
            content += "        {\n";
            content += "            if (gameId == \"client_no_track\" && !forceTrack)\n";
            content += "            {\n";
            content += "                Debug.Log(\"[Tokebi] Skipping (client): \" + eventType);\n";
            content += "                return;\n";
            content += "            }\n";
            content += "            if (string.IsNullOrEmpty(gameId) || string.IsNullOrEmpty(playerId))\n";
            content += "                return;\n";
            content += "            if (payload == null)\n";
            content += "                payload = new Dictionary<string, object>();\n";
            content += "            StartCoroutine(SendTrackingEvent(eventType, payload));\n";
            content += "        }\n\n";
            content += "        private IEnumerator SendTrackingEvent(string eventType, Dictionary<string, object> payload)\n";
            content += "        {\n";
            content += "            if (payload.Count == 0)\n";
            content += "                payload[\"timestamp\"] = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();\n";
            content += "            string payloadJson = CreatePayloadJson(payload);\n";
            content += "            string jsonData = \"{\\\"eventType\\\":\\\"\" + EscapeJson(eventType) + \"\\\",\\\"payload\\\":\" + payloadJson + \",\\\"gameId\\\":\\\"\" + EscapeJson(gameId) + \"\\\",\\\"playerId\\\":\\\"\" + EscapeJson(playerId) + \"\\\",\\\"platform\\\":\\\"unity\\\",\\\"environment\\\":\\\"\" + (Application.isEditor ? \"development\" : \"production\") + \"\\\"}\";\n";
            content += "            using (UnityWebRequest request = new UnityWebRequest(ENDPOINT + \"/api/track\", \"POST\"))\n";
            content += "            {\n";
            content += "                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);\n";
            content += "                request.uploadHandler = new UploadHandlerRaw(bodyRaw);\n";
            content += "                request.downloadHandler = new DownloadHandlerBuffer();\n";
            content += "                request.SetRequestHeader(\"Authorization\", API_KEY);\n";
            content += "                request.SetRequestHeader(\"Content-Type\", \"application/json\");\n";
            content += "                Debug.Log(\"[Tokebi] Tracking: \" + eventType + \" PlayerId: \" + playerId);\n";
            content += "                yield return request.SendWebRequest();\n";
            content += "                if (request.result != UnityWebRequest.Result.Success)\n";
            content += "                {\n";
            content += "                    Debug.LogWarning(\"[Tokebi] Tracking failed: \" + request.error);\n";
            content += "                }\n";
            content += "            }\n";
            content += "        }\n\n";
            content += GetHelperMethods();
            return content;
        }

        private string GetHelperMethods()
        {
            string content = "";
            content += "        private string CreatePayloadJson(Dictionary<string, object> payload)\n";
            content += "        {\n";
            content += "            if (payload == null || payload.Count == 0)\n";
            content += "                return \"{}\";\n";
            content += "            StringBuilder sb = new StringBuilder();\n";
            content += "            sb.Append(\"{\");\n";
            content += "            bool first = true;\n";
            content += "            foreach (var kvp in payload)\n";
            content += "            {\n";
            content += "                if (!first) sb.Append(\",\");\n";
            content += "                first = false;\n";
            content += "                sb.Append(\"\\\"\").Append(EscapeJson(kvp.Key)).Append(\"\\\":\");\n";
            content += "                if (kvp.Value == null)\n";
            content += "                    sb.Append(\"null\");\n";
            content += "                else if (kvp.Value is string)\n";
            content += "                    sb.Append(\"\\\"\").Append(EscapeJson(kvp.Value.ToString())).Append(\"\\\"\");\n";
            content += "                else if (kvp.Value is bool)\n";
            content += "                    sb.Append(kvp.Value.ToString().ToLower());\n";
            content += "                else\n";
            content += "                    sb.Append(kvp.Value.ToString());\n";
            content += "            }\n";
            content += "            sb.Append(\"}\");\n";
            content += "            return sb.ToString();\n";
            content += "        }\n\n";
            content += "        private string EscapeJson(string str)\n";
            content += "        {\n";
            content += "            if (string.IsNullOrEmpty(str))\n";
            content += "                return str;\n";
            content += "            return str.Replace(\"\\\\\", \"\\\\\\\\\").Replace(\"\\\"\", \"\\\\\\\"\").Replace(\"\\n\", \"\\\\n\").Replace(\"\\r\", \"\\\\r\").Replace(\"\\t\", \"\\\\t\");\n";
            content += "        }\n\n";
            return content;
        }

        private string GetConvenienceMethods()
        {
            string content = "";
            content += "        public void TrackLevelStart(string level)\n";
            content += "        {\n";
            content += "            var payload = new Dictionary<string, object> { [\"level\"] = level };\n";
            content += "            Track(\"level_start\", payload);\n";
            content += "        }\n\n";
            content += "        public void TrackLevelComplete(string level, float timeTaken)\n";
            content += "        {\n";
            content += "            var payload = new Dictionary<string, object> { [\"level\"] = level, [\"time\"] = timeTaken };\n";
            content += "            Track(\"level_complete\", payload);\n";
            content += "        }\n\n";
            content += "        public void TrackClientEvent(string eventType, Dictionary<string, object> payload = null)\n";
            content += "        {\n";
            content += "            Track(eventType, payload, true);\n";
            content += "        }\n\n";
            return content;
        }

        private string GetEditorContent()
        {
            string content = "";
            content += "using UnityEngine;\n";
            content += "using UnityEditor;\n";
            content += "using System.Collections.Generic;\n\n";
            content += "namespace Tokebi\n";
            content += "{\n";
            content += "    public class TokebiMenu\n";
            content += "    {\n";
            content += "        [MenuItem(\"GameObject/Tokebi/Create Analytics SDK\", false, 10)]\n";
            content += "        public static void CreateTokebiSDK()\n";
            content += "        {\n";
            content += "            GameObject go = new GameObject(\"TokebiAnalytics\");\n";
            content += "            go.AddComponent<TokebiSDK>();\n";
            content += "            Selection.activeGameObject = go;\n";
            content += "            Debug.Log(\"[Tokebi] Analytics SDK GameObject created!\");\n";
            content += "        }\n\n";
            content += "        [MenuItem(\"Tools/Tokebi/Test Analytics\")]\n";
            content += "        public static void TestAnalytics()\n";
            content += "        {\n";
            content += "            if (Application.isPlaying && TokebiSDK.Instance != null)\n";
            content += "            {\n";
            content += "                TokebiSDK.Instance.Track(\"test_event\", new Dictionary<string, object> { [\"test_data\"] = \"Hello from Unity!\" });\n";
            content += "                Debug.Log(\"[Tokebi] Test event sent!\");\n";
            content += "            }\n";
            content += "            else\n";
            content += "            {\n";
            content += "                Debug.LogWarning(\"[Tokebi] SDK not available. Make sure you're in Play Mode.\");\n";
            content += "            }\n";
            content += "        }\n";
            content += "    }\n";
            content += "}\n";
            return content;
        }
    }
}