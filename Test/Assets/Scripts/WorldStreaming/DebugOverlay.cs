using UnityEngine;

namespace WorldStreaming
{
    /// <summary>
    /// ランタイムデバッグオーバーレイUIを表示する。
    /// </summary>
    public class DebugOverlay : MonoBehaviour
    {
        private WorldStreamer _streamer;
        private GUIStyle _style;

        public void Initialize(WorldStreamer streamer)
        {
            _streamer = streamer;
            _style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }

        private void OnGUI()
        {
            if (_streamer == null)
            {
                return;
            }

            var settings = _streamer.GetSettings();
            if (!settings.EnableDebugGizmos)
            {
                return;
            }

            GUI.Box(new Rect(10, 10, 400, 200), "", _style);

            string debugText = $@"
[WorldStreamer Debug Info]
Player Chunk: {_streamer.CurrentPlayerChunk}
Loaded Chunks: {_streamer.LoadedChunkCount}
Pending Loads: {_streamer.PendingLoadCount}
Currently Loading: {_streamer.CurrentlyLoadingCount}
Memory Usage: {_streamer.MemoryUsageMB}MB / {_streamer.MemoryBudgetMB}MB
";
            GUI.Label(new Rect(15, 15, 390, 190), debugText, _style);
        }
    }
}
