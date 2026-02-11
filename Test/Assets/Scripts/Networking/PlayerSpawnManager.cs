using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Networking
{
    /// <summary>
    /// ルーム参加時のプレイヤー生成、退出時のクリーンアップ、
    /// スポーンポイント管理を担当する。
    /// autoSpawnOnJoinが有効な場合、ルーム参加時に自動でプレイヤーをスポーンする。
    /// </summary>
    public class PlayerSpawnManager : MonoBehaviourPunCallbacks
    {
        private static PlayerSpawnManager _instance;
        public static PlayerSpawnManager Instance => _instance;

        [SerializeField] private NetworkSettings _settings;

        private GameObject _localPlayerInstance;
        private readonly HashSet<int> _occupiedSpawnIndices = new HashSet<int>();

        public GameObject LocalPlayerInstance => _localPlayerInstance;
        public bool HasLocalPlayer => _localPlayerInstance != null;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            if (_settings == null && NetworkModeManager.Instance != null)
            {
                _settings = NetworkModeManager.Instance.Settings;
            }
        }

        public override void OnJoinedRoom()
        {
            if (_settings != null && _settings.AutoSpawnOnJoin)
            {
                SpawnLocalPlayer();
            }
        }

        public override void OnLeftRoom()
        {
            CleanupLocalPlayer();
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            // PUN2が自動でリモートプレイヤーを破棄するため追加処理不要
            // スポーンポイントの解放のみ
            _occupiedSpawnIndices.Clear();
        }

        /// <summary>
        /// ローカルプレイヤーをスポーンする
        /// </summary>
        public GameObject SpawnLocalPlayer()
        {
            if (_localPlayerInstance != null)
            {
                Log("Local player already spawned");
                return _localPlayerInstance;
            }

            string prefabName = _settings != null ? _settings.PlayerPrefabName : "NetworkPlayer";

            GetSpawnTransform(out Vector3 position, out Quaternion rotation);

            _localPlayerInstance = PhotonNetwork.Instantiate(
                prefabName,
                position,
                rotation
            );

            Log($"Spawned local player at {position}");
            return _localPlayerInstance;
        }

        /// <summary>
        /// ローカルプレイヤーを破棄する
        /// </summary>
        public void CleanupLocalPlayer()
        {
            if (_localPlayerInstance != null)
            {
                PhotonNetwork.Destroy(_localPlayerInstance);
                _localPlayerInstance = null;
                _occupiedSpawnIndices.Clear();
                Log("Cleaned up local player");
            }
        }

        /// <summary>
        /// ローカルプレイヤーを指定位置にリスポーンする
        /// </summary>
        public void RespawnLocalPlayer()
        {
            CleanupLocalPlayer();
            SpawnLocalPlayer();
        }

        /// <summary>
        /// スポーンポイントから重複しない位置を取得する
        /// </summary>
        private void GetSpawnTransform(out Vector3 position, out Quaternion rotation)
        {
            var spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("[PlayerSpawnManager] No SpawnPoints found in scene. Using origin.");
                position = Vector3.zero;
                rotation = Quaternion.identity;
                return;
            }

            // 未使用のスポーンポイントを優先的に選択
            int selectedIndex = -1;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (!_occupiedSpawnIndices.Contains(i))
                {
                    selectedIndex = i;
                    break;
                }
            }

            // 全て使用済みならPhoton ActorNumberでインデックスを決定
            if (selectedIndex == -1)
            {
                selectedIndex = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
            }

            _occupiedSpawnIndices.Add(selectedIndex);

            position = spawnPoints[selectedIndex].Position;
            rotation = spawnPoints[selectedIndex].Rotation;
        }

        private void Log(string message)
        {
            if (_settings != null && _settings.EnableDebugLogging)
            {
                Debug.Log($"[PlayerSpawnManager] {message}");
            }
        }
    }
}
