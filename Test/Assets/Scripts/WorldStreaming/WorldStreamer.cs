using System.Collections.Generic;
using UnityEngine;

namespace WorldStreaming
{
    /// <summary>
    /// ワールドストリーミングシステムのメインコンポーネント。
    /// プレイヤー位置の監視、チャンクロード/アンロード、LOD管理、メモリ管理を統合する。
    /// </summary>
    public class WorldStreamer : MonoBehaviour
    {
        private static WorldStreamer _instance;
        public static WorldStreamer Instance => _instance;

        [SerializeField] private WorldStreamerSettings _settings;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private bool _usePlayerMainCamera = true;

        private ChunkGrid _grid;
        private ChunkLoader _chunkLoader;
        private LODController _lodController;
        private MemoryBudgetMonitor _memoryBudgetMonitor;
        private HashSet<ChunkCoord> _currentlyLoadedChunks = new HashSet<ChunkCoord>();
        private HashSet<ChunkCoord> _targetChunks = new HashSet<ChunkCoord>();
        private DebugOverlay _debugOverlay;

        private void OnEnable()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDisable()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Start()
        {
            if (_settings == null)
            {
                Debug.LogError("[WorldStreamer] WorldStreamerSettings is not assigned!");
                enabled = false;
                return;
            }

            // プレイヤーTransformの取得
            if (_playerTransform == null)
            {
                if (_usePlayerMainCamera && Camera.main != null)
                {
                    _playerTransform = Camera.main.transform;
                }
                else
                {
                    _playerTransform = transform;
                }
            }

            // システムの初期化
            _grid = new ChunkGrid(_settings.ChunkSize);
            _chunkLoader = new ChunkLoader(_grid, _settings);
            _lodController = new LODController(_settings, _grid);
            _memoryBudgetMonitor = new MemoryBudgetMonitor(_settings, _chunkLoader);

            if (_settings.EnableDebugGizmos)
            {
                _debugOverlay = gameObject.AddComponent<DebugOverlay>();
                _debugOverlay.Initialize(this);
            }

            if (_settings.EnableDebugLogging)
            {
                Debug.Log("[WorldStreamer] Initialized");
            }
        }

        private void Update()
        {
            if (_grid == null || _playerTransform == null)
            {
                return;
            }

            Vector3 playerPos = _playerTransform.position;

            // ロード対象チャンクを計算
            _targetChunks = _grid.GetSurroundingChunks(playerPos, _settings.LoadRadius);

            // 差分を計算
            _grid.GetChunkDifference(_currentlyLoadedChunks, _targetChunks, out var toLoad, out var toUnload);

            // アンロード処理
            foreach (var coord in toUnload)
            {
                _chunkLoader.Unload(coord);
                _currentlyLoadedChunks.Remove(coord);
            }

            // ロード処理（優先度キューに追加）
            foreach (var coord in toLoad)
            {
                float distance = CalculateDistance(playerPos, coord);
                _chunkLoader.EnqueueLoad(coord, distance);
                _currentlyLoadedChunks.Add(coord);
            }

            // 非同期ロード処理
            _chunkLoader.Update();

            // LOD更新
            UpdateLOD(playerPos);

            // メモリ監視
            _memoryBudgetMonitor.Update();
            _memoryBudgetMonitor.PurgeIfNeeded(playerPos, _grid);
        }

        /// <summary>
        /// プレイヤーとチャンク間の距離を計算する
        /// </summary>
        private float CalculateDistance(Vector3 playerPos, ChunkCoord chunkCoord)
        {
            var chunkCenter = chunkCoord.GetWorldCenter(_settings.ChunkSize);
            return Vector3.Distance(playerPos, chunkCenter);
        }

        /// <summary>
        /// ロード済みチャンクのLODレベルを更新する
        /// </summary>
        private void UpdateLOD(Vector3 playerPos)
        {
            foreach (var chunkData in _chunkLoader.ChunkCache.Values)
            {
                if (chunkData.State != LoadState.Loaded)
                {
                    continue;
                }

                LODLevel newLOD = _lodController.GetLODLevel(playerPos, chunkData.Coord);
                if (chunkData.CurrentLOD != newLOD)
                {
                    chunkData.CurrentLOD = newLOD;
                    if (chunkData.RootObject != null)
                    {
                        _lodController.ApplyLOD(chunkData.RootObject, newLOD);
                    }
                }

                _chunkLoader.UpdateChunkAccess(chunkData.Coord);
            }
        }

        /// <summary>
        /// システムをリセットし、全チャンクをアンロードする
        /// </summary>
        public void Reset()
        {
            if (_chunkLoader != null)
            {
                _chunkLoader.UnloadAll();
            }
            _currentlyLoadedChunks.Clear();
            _targetChunks.Clear();
        }

        // 以下、デバッグ・情報取得用のPublicメソッド

        public int LoadedChunkCount => _currentlyLoadedChunks.Count;
        public int PendingLoadCount => _chunkLoader?.PendingLoadCount ?? 0;
        public int CurrentlyLoadingCount => _chunkLoader?.CurrentlyLoadingCount ?? 0;
        public long MemoryUsageMB => _memoryBudgetMonitor?.TotalMemoryUsage / (1024 * 1024) ?? 0;
        public float MemoryBudgetMB => _settings != null ? _settings.MemoryBudgetBytes / (1024f * 1024f) : 512f;
        public ChunkCoord CurrentPlayerChunk => _grid?.GetChunkCoord(_playerTransform.position) ?? default;

        public WorldStreamerSettings GetSettings() => _settings;
        public ChunkLoader GetChunkLoader() => _chunkLoader;
        public ChunkGrid GetGrid() => _grid;
        public LODController GetLODController() => _lodController;
        public MemoryBudgetMonitor GetMemoryBudgetMonitor() => _memoryBudgetMonitor;
    }
}
