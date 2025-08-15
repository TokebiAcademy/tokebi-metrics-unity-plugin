using UnityEngine;
using UnityEditor;

namespace Tokebi
{
    public class TokebiInstaller : EditorWindow
    {
        private string apiKey = "";
        private bool showApiKey = false;

        [MenuItem("Tools/Tokebi/Setup Analytics")]
        public static void ShowWindow()
        {
            TokebiInstaller window = GetWindow<TokebiInstaller>("Tokebi Analytics Setup");
            window.minSize = new Vector2(450, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("🚀 Tokebi Analytics SDK v2.0", EditorStyles.boldLabel);
            GUILayout.Label("Production-ready batched analytics", EditorStyles.helpBox);
            EditorGUILayout.Space();

            // Features info
            EditorGUILayout.HelpBox("✅ 60-second batching (like Unity Analytics)\n✅ Minimal memory allocations\n✅ Object pooling\n✅ Console/mobile optimized", MessageType.Info);
            EditorGUILayout.Space();

            // Game info
            GUILayout.Label("Game: " + PlayerSettings.productName, EditorStyles.helpBox);
            EditorGUILayout.Space();

            // API Key setup
            GUILayout.Label("API Key Configuration:", EditorStyles.boldLabel);
            
            // Toggle to show/hide API key
            showApiKey = EditorGUILayout.Toggle("Show API Key", showApiKey);
            
            if (showApiKey)
            {
                apiKey = EditorGUILayout.TextField("API Key", apiKey);
            }
            else
            {
                apiKey = EditorGUILayout.PasswordField("API Key", apiKey);
            }
            
            EditorGUILayout.Space();

            // Setup button
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(apiKey.Trim()));
            if (GUILayout.Button("Create Analytics GameObject", GUILayout.Height(30)))
            {
                CreateTokebiAnalytics();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            
            // Manual setup info
            GUILayout.Label("Manual Setup:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("1. Create empty GameObject\n2. Add TokebiSDK component\n3. Set API Key in inspector\n4. Done!", MessageType.Info);
            
            EditorGUILayout.Space();
            
            // Installation via Package Manager
            GUILayout.Label("Package Manager Installation:", EditorStyles.boldLabel);
            if (GUILayout.Button("📋 Copy Git URL", GUILayout.Height(25)))
            {
                EditorGUIUtility.systemCopyBuffer = "https://github.com/TokebiAcademy/tokebi-metrics-unity-plugin.git";
                ShowNotification(new GUIContent("Git URL copied to clipboard!"));
            }
            
            EditorGUILayout.HelpBox("Use: Window → Package Manager → + → Add package from git URL", MessageType.Info);
        }

        private void CreateTokebiAnalytics()
        {
            if (string.IsNullOrEmpty(apiKey.Trim()))
            {
                EditorUtility.DisplayDialog("Error", "Please enter your API key", "OK");
                return;
            }

            // Check if one already exists
            TokebiSDK existing = FindObjectOfType<TokebiSDK>();
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("TokebiSDK Found", 
                    "A TokebiSDK already exists in the scene. Replace it?", 
                    "Yes", "Cancel"))
                {
                    return;
                }
                DestroyImmediate(existing.gameObject);
            }

            // Create new GameObject
            GameObject go = new GameObject("TokebiAnalytics");
            TokebiSDK sdk = go.AddComponent<TokebiSDK>();
            
            // Set the API key
            sdk.SetApiKey(apiKey.Trim());
            
            // Mark scene as dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            
            // Select the new object
            Selection.activeGameObject = go;
            
            Debug.Log("[Tokebi] Analytics SDK GameObject created with API key!");
            ShowNotification(new GUIContent("TokebiAnalytics created successfully!"));
            
            Close();
        }
    }
}
