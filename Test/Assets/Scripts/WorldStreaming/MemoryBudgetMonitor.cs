using UnityEngine;

namespace WorldStreaming
{
    /// <summary>
    /// ロード済みチャンクのメモリ使用量を監視し、バジェット超過時にアンロードを実行する。
    /// </summary>
    public class MemoryBudgetMonitor
    {
        private readonly WorldStreamerSettings _settings;
        private readonly ChunkLoader _chunkLoader;
        private long _totalMemoryUsage;

        public long TotalMemoryUsage => _totalMemoryUsage;
        public long MemoryBudgetBytes => (long)_settings.MemoryBudgetBytes;
        public bool IsBudgetExceeded => _totalMemoryUsage > MemoryBudgetBytes;

        public MemoryBudgetMonitor(WorldStreamerSettings settings, ChunkLoader chunkLoader)
        {
            _settings = settings;
            _chunkLoader = chunkLoader;
            _totalMemoryUsage = 0;
        }

        /// <summary>
        /// メモリ使用量を更新する（毎フレーム呼び出し推奨）
        /// </summary>
        public void Update()
        {
            _totalMemoryUsage = _chunkLoader.CalculateTotalMemory();
        }

        /// <summary>
        /// メモリバジェット超過時にパージを実行する
        /// </summary>
        public void PurgeIfNeeded(Vector3 playerPosition, ChunkGrid grid)
        {
            if (!IsBudgetExceeded)
            {
                return;
            }

            var playerChunkCoord = grid.GetChunkCoord(playerPosition);
            _totalMemoryUsage = _chunkLoader.PurgeExcessMemory(
                MemoryBudgetBytes,
                _settings.MinProtectedRadius,
                playerChunkCoord);

            if (_settings.EnableDebugLogging)
            {
                Debug.LogWarning($"[MemoryBudgetMonitor] Purged chunks. New memory usage: {_totalMemoryUsage / (1024f * 1024f):.2f}MB");
            }
        }
    }
}
