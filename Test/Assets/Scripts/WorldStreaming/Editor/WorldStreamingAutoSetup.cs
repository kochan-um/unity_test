#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace WorldStreaming.Editor
{
    [InitializeOnLoad]
    public static class WorldStreamingAutoSetup
    {
        private const string SessionKey = "WorldStreaming.AutoSetup.Completed";
        private const string SettingsAssetPath = "Assets/Resources/WorldStreamerSettings.asset";
        private const string WorldStreamerObjectName = "WorldStreamer";

        static WorldStreamingAutoSetup()
        {
            EditorApplication.delayCall += RunOncePerEditorSession;
        }

        private static void RunOncePerEditorSession()
        {
            if (SessionState.GetBool(SessionKey, false))
            {
                return;
            }

            SessionState.SetBool(SessionKey, true);
            TryAutoSetup();
        }

        [MenuItem("Tools/WorldStreaming/Run Auto Setup")]
        public static void RunFromMenu()
        {
            TryAutoSetup();
        }

        private static void TryAutoSetup()
        {
            EnsureAddressablesSettings();
            var settings = EnsureWorldStreamerSettingsAsset();
            EnsureWorldStreamerInActiveScene(settings);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureAddressablesSettings()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null)
            {
                Debug.LogError("[WorldStreaming] Failed to create Addressables settings.");
                return;
            }

            if (settings.FindGroup("Chunks") == null)
            {
                settings.CreateGroup("Chunks", false, false, false, null);
                EditorUtility.SetDirty(settings);
            }
        }

        private static WorldStreamerSettings EnsureWorldStreamerSettingsAsset()
        {
            var settings = AssetDatabase.LoadAssetAtPath<WorldStreamerSettings>(SettingsAssetPath);
            if (settings != null)
            {
                return settings;
            }

            var directory = Path.GetDirectoryName(SettingsAssetPath);
            if (!string.IsNullOrEmpty(directory) && !AssetDatabase.IsValidFolder(directory))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            settings = ScriptableObject.CreateInstance<WorldStreamerSettings>();
            AssetDatabase.CreateAsset(settings, SettingsAssetPath);
            EditorUtility.SetDirty(settings);
            Debug.Log($"[WorldStreaming] Created {SettingsAssetPath}");
            return settings;
        }

        private static void EnsureWorldStreamerInActiveScene(WorldStreamerSettings settings)
        {
            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.isLoaded)
            {
                return;
            }

            var existing = Object.FindFirstObjectByType<WorldStreamer>();
            if (existing == null)
            {
                var go = new GameObject(WorldStreamerObjectName);
                existing = go.AddComponent<WorldStreamer>();
                Undo.RegisterCreatedObjectUndo(go, "Create WorldStreamer");
            }

            var serialized = new SerializedObject(existing);
            var settingsProp = serialized.FindProperty("_settings");
            if (settingsProp != null && settingsProp.objectReferenceValue == null)
            {
                settingsProp.objectReferenceValue = settings;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(existing);
            }

            if (activeScene.path == EditorSceneManager.GetActiveScene().path)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }
        }
    }
}
#endif
