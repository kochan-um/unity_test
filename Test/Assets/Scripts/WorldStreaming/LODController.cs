using UnityEngine;

namespace WorldStreaming
{
    /// <summary>
    /// プレイヤーからの距離に基づいてチャンクのLODレベルを判定する。
    /// </summary>
    public class LODController
    {
        private readonly WorldStreamerSettings _settings;
        private readonly ChunkGrid _grid;

        public LODController(WorldStreamerSettings settings, ChunkGrid grid)
        {
            _settings = settings;
            _grid = grid;
        }

        /// <summary>
        /// プレイヤーとチャンク座標の距離に基づいてLODレベルを決定する
        /// </summary>
        public LODLevel GetLODLevel(Vector3 playerPosition, ChunkCoord chunkCoord)
        {
            var playerChunkCoord = _grid.GetChunkCoord(playerPosition);
            int distance = playerChunkCoord.GetDistance(chunkCoord);

            if (distance <= _settings.LoadRadius)
            {
                return LODLevel.High;
            }
            else if (distance <= _settings.LODMediumRadius)
            {
                return LODLevel.Medium;
            }
            else
            {
                return LODLevel.Low;
            }
        }

        /// <summary>
        /// チャンク内のレンダラーのLODレベルに応じた表示/非表示を制御する。
        /// 現在はHigh LODのみ表示とする（Medium/Lowは別途実装）
        /// </summary>
        public void ApplyLOD(GameObject chunkRoot, LODLevel lodLevel)
        {
            if (chunkRoot == null)
            {
                return;
            }

            var renderers = chunkRoot.GetComponentsInChildren<Renderer>(includeInactive: false);
            foreach (var renderer in renderers)
            {
                // 簡易実装：LODレベルに応じてコンポーネント有効/無効を制御
                // 本来は、オブジェクトの詳細度別にプレハブが分けられていることを想定
                switch (lodLevel)
                {
                    case LODLevel.High:
                        renderer.enabled = true;
                        break;
                    case LODLevel.Medium:
                    case LODLevel.Low:
                        // Medium/Lowでも表示するが、将来的にメッシュをスワップする
                        renderer.enabled = true;
                        break;
                }
            }
        }
    }
}
