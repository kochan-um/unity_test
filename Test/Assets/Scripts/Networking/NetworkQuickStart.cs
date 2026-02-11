using UnityEngine;
using Photon.Pun;

namespace Networking
{
    /// <summary>
    /// シーンに配置するだけでネットワーク接続からプレイヤー生成まで自動実行する
    /// クイックスタートコンポーネント。
    /// テストや開発時の素早い確認に使用する。
    /// </summary>
    public class NetworkQuickStart : MonoBehaviour
    {
        [Header("Quick Start Settings")]
        [Tooltip("起動時に自動で接続・ルーム参加を実行する")]
        [SerializeField] private bool _autoStartOnAwake = true;

        [Tooltip("使用するネットワークモード")]
        [SerializeField] private NetworkMode _mode = NetworkMode.Offline;

        [Tooltip("ルームに自動参加する")]
        [SerializeField] private bool _autoJoinRoom = true;

        [Tooltip("自動参加するルーム名（空欄で自動生成）")]
        [SerializeField] private string _roomName = "";

        private bool _started;

        private void Start()
        {
            if (_autoStartOnAwake)
            {
                QuickStart();
            }
        }

        /// <summary>
        /// ネットワーク接続からプレイヤー生成までを自動実行する
        /// </summary>
        public void QuickStart()
        {
            if (_started) return;
            _started = true;

            var modeManager = NetworkModeManager.Instance;
            if (modeManager == null)
            {
                Debug.LogError("[NetworkQuickStart] NetworkModeManager not found in scene!");
                return;
            }

            modeManager.SwitchMode(_mode);

            if (_mode == NetworkMode.Offline)
            {
                StartOffline();
            }
            else
            {
                StartOnline();
            }
        }

        private void StartOffline()
        {
            var lobby = LobbyManager.Instance;
            if (lobby == null)
            {
                Debug.LogError("[NetworkQuickStart] LobbyManager not found in scene!");
                return;
            }

            // オフラインモードでは直接ルーム作成（Photon OfflineModeが自動処理）
            lobby.JoinOfflineRoom();
        }

        private void StartOnline()
        {
            var lobby = LobbyManager.Instance;
            if (lobby == null)
            {
                Debug.LogError("[NetworkQuickStart] LobbyManager not found in scene!");
                return;
            }

            // OnConnectedToMaster後にルーム参加
            if (_autoJoinRoom)
            {
                lobby.OnConnected += OnConnectedHandler;
            }

            lobby.Connect();
        }

        private void OnConnectedHandler()
        {
            var lobby = LobbyManager.Instance;
            if (lobby == null) return;

            lobby.OnConnected -= OnConnectedHandler;

            if (!string.IsNullOrEmpty(_roomName))
            {
                lobby.JoinRoom(_roomName);
            }
            else
            {
                lobby.JoinRandomRoom();
            }
        }

        private void OnDestroy()
        {
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnConnected -= OnConnectedHandler;
            }
        }
    }
}
