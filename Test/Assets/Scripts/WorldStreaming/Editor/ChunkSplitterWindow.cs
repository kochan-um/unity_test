#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;
using System.Linq;

namespace WorldStreaming.Editor
{
    /// <summary>
    /// シーン内のオブジェクトを自動的にチャンクグリッドに分割し、Addressableアセットとして登録するエディタウィンドウ。
    /// </summary>
    public class ChunkSplitterWindow : EditorWindow
    {
        private float _chunkSize = 100f;
        private GameObject _rootObject;

        [MenuItem("Window/WorldStreaming/Chunk Splitter")]
        public static void ShowWindow()
        {
            GetWindow<ChunkSplitterWindow>("Chunk Splitter");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Chunk Splitter", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _chunkSize = EditorGUILayout.FloatField("Chunk Size (meters)", _chunkSize);
            _rootObject = EditorGUILayout.ObjectField("Root Object", _rootObject, typeof(GameObject), true) as GameObject;

            EditorGUILayout.Space();

            if (GUILayout.Button("Split & Register", GUILayout.Height(30)))
            {
                PerformChunkSplit();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("このツールはシーン内のオブジェクトをチャンクグリッドに分割し、Addressable Assetsに登録します。", MessageType.Info);
        }

        private void PerformChunkSplit()
        {
            if (_chunkSize <= 0)
            {
                EditorUtility.DisplayDialog("Error", "Chunk Size must be greater than 0", "OK");
                return;
            }

            var grid = new ChunkGrid(_chunkSize);
            var chunkObjects = new Dictionary<ChunkCoord, List<GameObject>>();

            // ロードするオブジェクトを取得
            List<GameObject> objectsToProcess = new List<GameObject>();
            if (_rootObject != null)
            {
                objectsToProcess.Add(_rootObject);
                foreach (Transform child in _rootObject.GetComponentsInChildren<Transform>())
                {
                    if (child.gameObject != _rootObject)
                    {
                        objectsToProcess.Add(child.gameObject);
                    }
                }
            }
            else
            {
                objectsToProcess = FindObjectsByType<GameObject>(FindObjectsSortMode.None).ToList();
            }

            // オブジェクトをチャンクに分類
            foreach (var obj in objectsToProcess)
            {
                var chunkCoord = grid.GetChunkCoord(obj.transform.position);
                if (!chunkObjects.ContainsKey(chunkCoord))
                {
                    chunkObjects[chunkCoord] = new List<GameObject>();
                }
                chunkObjects[chunkCoord].Add(obj);
            }

            // Addressable Assetsに登録
            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (addressableSettings == null)
            {
                EditorUtility.DisplayDialog("Error", "Addressable Assets settings not found. Please initialize Addressables first.", "OK");
                return;
            }

            int registeredCount = 0;
            foreach (var kvp in chunkObjects)
            {
                var chunkId = kvp.Key.ToChunkId();

                // グループを取得または作成（デフォルトスキーマで）
                var group = addressableSettings.FindGroup("Chunks");
                if (group == null)
                {
                    group = addressableSettings.CreateGroup("Chunks", false, false, false, null);
                }

                foreach (var obj in kvp.Value)
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    if (!string.IsNullOrEmpty(path))
                    {
                        try
                        {
                            var guid = AssetDatabase.AssetPathToGUID(path);
                            var entry = addressableSettings.CreateOrMoveEntry(guid, group);

                            // チャンクラベルを付与
                            entry.SetLabel(chunkId, true);
                            entry.SetLabel($"chunk_{kvp.Key.X}_{kvp.Key.Z}", true);

                            registeredCount++;
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"Failed to register {obj.name}: {ex.Message}");
                        }
                    }
                }
            }

            addressableSettings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Success", $"Registered {registeredCount} assets in {chunkObjects.Count} chunks", "OK");
        }
    }
}
#endif
