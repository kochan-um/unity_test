using UnityEngine;
using Photon.Pun;

namespace Networking
{
    /// <summary>
    /// ネットワーク状態を画面にオーバーレイ表示するデバッグUI。
    /// </summary>
    public class NetworkDebugOverlay : MonoBehaviour
    {
        private NetworkSettings _settings;
        private GUIStyle _boxStyle;
        private GUIStyle _labelStyle;
        private bool _stylesInitialized;

        private void Start()
        {
            if (NetworkModeManager.Instance != null)
            {
                _settings = NetworkModeManager.Instance.Settings;
            }
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            _boxStyle = new GUIStyle(GUI.skin.box);
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (_settings != null && !_settings.EnableDebugUI) return;

            InitializeStyles();

            float width = 350f;
            float height = 180f;
            float x = Screen.width - width - 10f;
            float y = 10f;

            GUI.Box(new Rect(x, y, width, height), "", _boxStyle);

            var mode = NetworkModeManager.Instance?.CurrentMode ?? NetworkMode.Offline;
            string connectionState = PhotonNetwork.IsConnected ? "Connected" : "Disconnected";
            if (PhotonNetwork.NetworkClientState.ToString().Contains("Connecting"))
            {
                connectionState = "Connecting...";
            }

            string roomInfo = PhotonNetwork.InRoom
                ? $"{PhotonNetwork.CurrentRoom.Name} ({PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})"
                : "Not in room";

            int ping = PhotonNetwork.IsConnected ? PhotonNetwork.GetPing() : 0;
            int localId = PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;

            string text =
$@"[Network Debug]
Mode: {mode}
Status: {connectionState}
Room: {roomInfo}
Ping: {ping}ms
Player ID: {localId}
Send Rate: {PhotonNetwork.SendRate}/s
State: {PhotonNetwork.NetworkClientState}";

            GUI.Label(new Rect(x + 10f, y + 5f, width - 20f, height - 10f), text, _labelStyle);
        }
    }
}
