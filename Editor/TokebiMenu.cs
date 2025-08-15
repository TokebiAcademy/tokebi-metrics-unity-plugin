using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Tokebi
{
    public class TokebiMenu
    {
        [MenuItem("GameObject/Tokebi/Create Analytics SDK", false, 10)]
        public static void CreateTokebiSDK()
        {
            GameObject go = new GameObject("TokebiAnalytics");
            go.AddComponent<TokebiSDK>();
            Selection.activeGameObject = go;
            Debug.Log("[Tokebi] Analytics SDK GameObject created! Don't forget to set your API key.");
        }

        [MenuItem("Tools/Tokebi/Test Analytics")]
        public static void TestAnalytics()
        {
            if (Application.isPlaying && TokebiSDK.Instance != null)
            {
                TokebiSDK.Instance.Track("test_event", new Dictionary<string, object> { 
                    ["test_data"] = "Hello from Unity SDK v2.0!",
                    ["batch_test"] = true,
                    ["timestamp"] = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
                Debug.Log("[Tokebi] Test event queued for batching! (Check queue in 60 seconds or force flush)");
            }
            else
            {
                Debug.LogWarning("[Tokebi] SDK not available. Make sure you're in Play Mode and have TokebiAnalytics in your scene.");
            }
        }

        [MenuItem("Tools/Tokebi/Force Flush Events")]
        public static void ForceFlush()
        {
            if (Application.isPlaying && TokebiSDK.Instance != null)
            {
                TokebiSDK.Instance.FlushEvents();
                Debug.Log("[Tokebi] Forced event flush! Check console for batch send results.");
            }
            else
            {
                Debug.LogWarning("[Tokebi] SDK not available. Make sure you're in Play Mode.");
            }
        }

        [MenuItem("Tools/Tokebi/Track Level Events")]
        public static void TestLevelEvents()
        {
            if (Application.isPlaying && TokebiSDK.Instance != null)
            {
                // Track multiple events quickly to test batching
                TokebiSDK.Instance.TrackLevelStart("test-level-1");
                
                TokebiSDK.Instance.Track("player_action", new Dictionary<string, object> {
                    ["action"] = "jump",
                    ["position_x"] = 10.5f,
                    ["position_y"] = 5.2f
                });
                
                TokebiSDK.Instance.Track("player_action", new Dictionary<string, object> {
                    ["action"] = "collect_coin",
                    ["coin_type"] = "gold",
                    ["value"] = 100
                });
                
                TokebiSDK.Instance.TrackLevelComplete("test-level-1", 45.2f);
                
                Debug.Log("[Tokebi] Multiple test events queued! Use 'Force Flush Events' to send immediately.");
            }
            else
            {
                Debug.LogWarning("[Tokebi] SDK not available. Make sure you're in Play Mode.");
            }
        }

        [MenuItem("Tools/Tokebi/Setup Analytics")]
        public static void ShowSetupWindow()
        {
            TokebiInstaller.ShowWindow();
        }

        [MenuItem("Tools/Tokebi/Documentation")]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/TokebiAcademy/tokebi-metrics-unity-plugin#readme");
        }

        [MenuItem("Tools/Tokebi/Package Manager Installation")]
        public static void CopyGitURL()
        {
            EditorGUIUtility.systemCopyBuffer = "https://github.com/TokebiAcademy/tokebi-metrics-unity-plugin.git";
            Debug.Log("[Tokebi] Git URL copied to clipboard! Use Window → Package Manager → + → Add package from git URL");
        }
    }
}
