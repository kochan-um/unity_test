using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Networking
{
    /// <summary>
    /// Photonサーバー接続、ロビー参加、ルーム管理を担当する。
    /// </summary>
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        private static LobbyManager _instance;
        public static LobbyManager Instance => _instance;

        private NetworkSettings _settings;
        private List<RoomInfo> _cachedRoomList = new List<RoomInfo>();

        public bool IsConnected => PhotonNetwork.IsConnected;
        public bool InRoom => PhotonNetwork.InRoom;
        public string CurrentRoomName => PhotonNetwork.CurrentRoom?.Name ?? "";
        public int CurrentPlayerCount => PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
        public IReadOnlyList<RoomInfo> CachedRoomList => _cachedRoomList;

        // イベント
        public event Action OnConnected;
        public event Action OnRoomJoined;
        public event Action OnRoomLeft;
        public event Action<Photon.Realtime.Player> OnPlayerJoined;
        public event Action<Photon.Realtime.Player> OnPlayerLeft;
        public event Action<DisconnectCause> OnConnectionFailed;
        public event Action<List<RoomInfo>> OnRoomListChanged;

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
            if (NetworkModeManager.Instance != null)
            {
                _settings = NetworkModeManager.Instance.Settings;
            }
        }

        /// <summary>
        /// Photonサーバーに接続する
        /// </summary>
        public void Connect()
        {
            if (PhotonNetwork.IsConnected)
            {
                Log("Already connected");
                return;
            }

            var mode = NetworkModeManager.Instance?.CurrentMode ?? NetworkMode.Offline;
            if (mode == NetworkMode.Offline)
            {
                // オフラインモードではPhoton接続不要、即座にルーム参加可能
                Log("Offline mode: skipping server connection");
                return;
            }

            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
            Log("Connecting to Photon...");
        }

        /// <summary>
        /// ルームを作成する
        /// </summary>
        public void CreateRoom(string roomName = null, byte maxPlayers = 0)
        {
            if (maxPlayers == 0)
            {
                maxPlayers = _settings != null ? _settings.MaxPlayersPerRoom : (byte)8;
            }

            if (string.IsNullOrEmpty(roomName))
            {
                var prefix = _settings != null ? _settings.AutoRoomPrefix : "Room_";
                roomName = prefix + UnityEngine.Random.Range(1000, 9999);
            }

            var options = new RoomOptions
            {
                MaxPlayers = maxPlayers,
                IsVisible = true,
                IsOpen = true
            };

            PhotonNetwork.CreateRoom(roomName, options);
            Log($"Creating room: {roomName} (max: {maxPlayers})");
        }

        /// <summary>
        /// 指定した名前のルームに参加する
        /// </summary>
        public void JoinRoom(string roomName)
        {
            PhotonNetwork.JoinRoom(roomName);
            Log($"Joining room: {roomName}");
        }

        /// <summary>
        /// ランダムなルームに参加する。空きがなければ自動作成する。
        /// </summary>
        public void JoinRandomRoom()
        {
            PhotonNetwork.JoinRandomRoom();
            Log("Joining random room...");
        }

        /// <summary>
        /// オフラインルームに参加する（シングルプレイヤー用）
        /// </summary>
        public void JoinOfflineRoom()
        {
            PhotonNetwork.CreateRoom("OfflineRoom");
            Log("Joining offline room");
        }

        /// <summary>
        /// 現在のルームから退出する
        /// </summary>
        public void LeaveRoom()
        {
            if (!PhotonNetwork.InRoom)
            {
                Log("Not in a room");
                return;
            }

            PhotonNetwork.LeaveRoom();
            Log("Leaving room...");
        }

        /// <summary>
        /// Photonサーバーから切断する
        /// </summary>
        public void Disconnect()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
        }

        // --- Photon Callbacks ---

        public override void OnConnectedToMaster()
        {
            Log("Connected to master server");
            OnConnected?.Invoke();

            if (_settings != null && _settings.AutoJoinLobby)
            {
                PhotonNetwork.JoinLobby();
            }
        }

        public override void OnJoinedLobby()
        {
            Log("Joined lobby");
        }

        public override void OnJoinedRoom()
        {
            Log($"Joined room: {PhotonNetwork.CurrentRoom.Name} ({PhotonNetwork.CurrentRoom.PlayerCount} players)");
            OnRoomJoined?.Invoke();
        }

        public override void OnLeftRoom()
        {
            Log("Left room");
            OnRoomLeft?.Invoke();
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            Log($"Player joined: {newPlayer.NickName} (ID: {newPlayer.ActorNumber})");
            OnPlayerJoined?.Invoke(newPlayer);
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            Log($"Player left: {otherPlayer.NickName} (ID: {otherPlayer.ActorNumber})");
            OnPlayerLeft?.Invoke(otherPlayer);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Log($"Join random room failed: {message}. Creating new room...");
            CreateRoom();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[LobbyManager] Create room failed: {message}");
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[LobbyManager] Join room failed: {message}");
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Log($"Disconnected: {cause}");
            if (cause != DisconnectCause.DisconnectByClientLogic &&
                cause != DisconnectCause.None)
            {
                OnConnectionFailed?.Invoke(cause);
            }
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            _cachedRoomList.Clear();
            foreach (var room in roomList)
            {
                if (!room.RemovedFromList)
                {
                    _cachedRoomList.Add(room);
                }
            }
            OnRoomListChanged?.Invoke(_cachedRoomList);
        }

        private void Log(string message)
        {
            if (_settings != null && _settings.EnableDebugLogging)
            {
                Debug.Log($"[LobbyManager] {message}");
            }
        }
    }
}
