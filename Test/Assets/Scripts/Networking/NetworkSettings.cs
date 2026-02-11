using UnityEngine;

namespace Networking
{
    [CreateAssetMenu(fileName = "NetworkSettings", menuName = "Networking/Settings")]
    public class NetworkSettings : ScriptableObject
    {
        [Header("Mode")]
        [Tooltip("起動時のデフォルトネットワークモード")]
        [SerializeField] private NetworkMode defaultMode = NetworkMode.Offline;

        [Header("Room")]
        [Tooltip("ルームの最大プレイヤー数")]
        [SerializeField] private byte maxPlayersPerRoom = 8;

        [Tooltip("自動ルーム名のプレフィックス")]
        [SerializeField] private string autoRoomPrefix = "Room_";

        [Header("Player")]
        [Tooltip("Resourcesフォルダ内のプレイヤープレハブ名")]
        [SerializeField] private string playerPrefabName = "NetworkPlayer";

        [Tooltip("ルーム参加時に自動でプレイヤーをスポーンする")]
        [SerializeField] private bool autoSpawnOnJoin = true;

        [Header("Connection")]
        [Tooltip("接続時に自動でロビーに参加する")]
        [SerializeField] private bool autoJoinLobby = true;

        [Tooltip("Photon送信レート（回/秒）")]
        [SerializeField] private int photonSendRate = 20;

        [Tooltip("Photonシリアライゼーションレート（回/秒）")]
        [SerializeField] private int photonSerializationRate = 10;

        [Header("Debug")]
        [Tooltip("デバッグUIを表示する")]
        [SerializeField] private bool enableDebugUI = false;

        [Tooltip("デバッグログを出力する")]
        [SerializeField] private bool enableDebugLogging = false;

        public NetworkMode DefaultMode => defaultMode;
        public byte MaxPlayersPerRoom => maxPlayersPerRoom;
        public string AutoRoomPrefix => autoRoomPrefix;
        public string PlayerPrefabName => playerPrefabName;
        public bool AutoSpawnOnJoin => autoSpawnOnJoin;
        public bool AutoJoinLobby => autoJoinLobby;
        public int PhotonSendRate => photonSendRate;
        public int PhotonSerializationRate => photonSerializationRate;
        public bool EnableDebugUI => enableDebugUI;
        public bool EnableDebugLogging => enableDebugLogging;

        private void OnValidate()
        {
            maxPlayersPerRoom = (byte)Mathf.Clamp(maxPlayersPerRoom, 1, 20);
            photonSendRate = Mathf.Clamp(photonSendRate, 5, 60);
            photonSerializationRate = Mathf.Clamp(photonSerializationRate, 5, 30);
        }
    }
}
