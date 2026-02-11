using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace Networking
{
    /// <summary>
    /// ネットワークモード（Offline/Online）を管理するシングルトン。
    /// Photon OfflineModeを活用し、モード間で共通のコードパスを実現する。
    /// </summary>
    public class NetworkModeManager : MonoBehaviour
    {
        private static NetworkModeManager _instance;
        public static NetworkModeManager Instance => _instance;

        [SerializeField] private NetworkSettings _settings;

        private NetworkMode _currentMode;
        private bool _isInGameplay;

        public NetworkMode CurrentMode => _currentMode;
        public NetworkSettings Settings => _settings;

        /// <summary>モード変更時に発行されるイベント</summary>
        public event Action<NetworkMode> OnModeChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (_settings == null)
            {
                Debug.LogError("[NetworkModeManager] NetworkSettings is not assigned!");
                return;
            }

            // Photon送受信レートの設定
            PhotonNetwork.SendRate = _settings.PhotonSendRate;
            PhotonNetwork.SerializationRate = _settings.PhotonSerializationRate;

            // デフォルトモードで初期化
            ApplyMode(_settings.DefaultMode);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // シーン名にGameplayが含まれるかで判定（カスタマイズ可能）
            _isInGameplay = scene.name.Contains("Gameplay") || scene.name.Contains("Game");
        }

        /// <summary>
        /// ゲームプレイ中フラグを外部から設定する
        /// </summary>
        public void SetInGameplay(bool inGameplay)
        {
            _isInGameplay = inGameplay;
        }

        /// <summary>
        /// ネットワークモードを切り替える。ゲームプレイ中は切り替え不可。
        /// </summary>
        public bool SwitchMode(NetworkMode newMode)
        {
            if (_currentMode == newMode)
            {
                return true;
            }

            if (_isInGameplay)
            {
                Debug.LogWarning("[NetworkModeManager] Cannot switch network mode during gameplay.");
                return false;
            }

            // オンラインからオフラインへの切替時はPhoton切断
            if (_currentMode == NetworkMode.Online && newMode == NetworkMode.Offline)
            {
                if (PhotonNetwork.InRoom)
                {
                    PhotonNetwork.LeaveRoom();
                }
                if (PhotonNetwork.IsConnected)
                {
                    PhotonNetwork.Disconnect();
                }
            }

            ApplyMode(newMode);
            return true;
        }

        private void ApplyMode(NetworkMode mode)
        {
            _currentMode = mode;

            switch (mode)
            {
                case NetworkMode.Offline:
                    PhotonNetwork.OfflineMode = true;
                    Log("Switched to Offline mode");
                    break;

                case NetworkMode.Online:
                    PhotonNetwork.OfflineMode = false;
                    Log("Switched to Online mode");
                    break;
            }

            OnModeChanged?.Invoke(_currentMode);
        }

        private void Log(string message)
        {
            if (_settings != null && _settings.EnableDebugLogging)
            {
                Debug.Log($"[NetworkModeManager] {message}");
            }
        }
    }
}
