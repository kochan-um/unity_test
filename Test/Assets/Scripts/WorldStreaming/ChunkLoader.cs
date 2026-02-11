using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace WorldStreaming
{
    /// <summary>
    /// Addressable繧剃ｽｿ逕ｨ縺励◆繝√Ε繝ｳ繧ｯ縺ｮ髱槫酔譛溘Ο繝ｼ繝峨・繧｢繝ｳ繝ｭ繝ｼ繝峨ｒ邂｡逅・☆繧九・    /// 蜆ｪ蜈亥ｺｦ繧ｭ繝･繝ｼ縺ｫ蝓ｺ縺･縺・※繝ｭ繝ｼ繝牙・逅・ｒ螳溯｡後＠縲∽ｸｦ陦梧焚繧貞宛髯舌☆繧九・    /// </summary>
    public class ChunkLoader
    {
        private readonly ChunkGrid _grid;
        private readonly WorldStreamerSettings _settings;
        private readonly LoadQueue _loadQueue;
        private readonly Dictionary<ChunkCoord, ChunkData> _chunkCache;
        private readonly Dictionary<ChunkCoord, AsyncOperationHandle<GameObject>> _pendingLoads;
        private readonly HashSet<ChunkCoord> _currentlyLoading;
        private readonly Dictionary<ChunkCoord, int> _retryCount;
        private readonly HashSet<string> _missingChunkKeys;

        public int PendingLoadCount => _loadQueue.Count;
        public int CurrentlyLoadingCount => _currentlyLoading.Count;
        public Dictionary<ChunkCoord, ChunkData> ChunkCache => _chunkCache;

        public ChunkLoader(ChunkGrid grid, WorldStreamerSettings settings)
        {
            _grid = grid;
            _settings = settings;
            _loadQueue = new LoadQueue();
            _chunkCache = new Dictionary<ChunkCoord, ChunkData>();
            _pendingLoads = new Dictionary<ChunkCoord, AsyncOperationHandle<GameObject>>();
            _currentlyLoading = new HashSet<ChunkCoord>();
            _retryCount = new Dictionary<ChunkCoord, int>();
            _missingChunkKeys = new HashSet<string>();
        }

        /// <summary>
        /// 繝ｭ繝ｼ繝牙ｯｾ雎｡繝√Ε繝ｳ繧ｯ繧偵く繝･繝ｼ縺ｫ霑ｽ蜉縺吶ｋ
        /// </summary>
        public void EnqueueLoad(ChunkCoord coord, float priorityDistance)
        {
            if (_chunkCache.ContainsKey(coord) || _currentlyLoading.Contains(coord) || _pendingLoads.ContainsKey(coord))
            {
                return;
            }

            _loadQueue.Enqueue(coord, priorityDistance);
        }

        /// <summary>
        /// 豈弱ヵ繝ｬ繝ｼ繝蜻ｼ縺ｳ蜃ｺ縺輔ｌ縲・撼蜷梧悄繝ｭ繝ｼ繝峨ｒ蜃ｦ逅・☆繧・        /// </summary>
        public void Update()
        {
            var completedLoads = new List<ChunkCoord>();
            foreach (var kvp in _pendingLoads)
            {
                if (kvp.Value.IsDone)
                {
                    completedLoads.Add(kvp.Key);
                }
            }

            foreach (var coord in completedLoads)
            {
                ProcessLoadCompletion(coord);
            }

            while (_currentlyLoading.Count < _settings.MaxConcurrentLoads && _loadQueue.Count > 0)
            {
                var coord = _loadQueue.Dequeue();
                StartLoad(coord);
            }
        }

        private void StartLoad(ChunkCoord coord)
        {
            string chunkId = coord.ToChunkId();

            if (_missingChunkKeys.Contains(chunkId))
            {
                return;
            }

            if (!HasAddressableLocation(chunkId))
            {
                _missingChunkKeys.Add(chunkId);
                if (_settings.EnableDebugLogging)
                {
                    Debug.LogWarning($"[ChunkLoader] Missing Addressable key: {chunkId}");
                }
                return;
            }
            try
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(chunkId);
                _pendingLoads[coord] = handle;
                _currentlyLoading.Add(coord);

                if (!_chunkCache.ContainsKey(coord))
                {
                    _chunkCache[coord] = new ChunkData(coord);
                }
                _chunkCache[coord].State = LoadState.Loading;

                if (_settings.EnableDebugLogging)
                {
                    Debug.Log($"[ChunkLoader] Loading {chunkId}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChunkLoader] Failed to start loading {chunkId}: {ex.Message}");
                HandleLoadFailure(coord);
            }
        }

        private static bool HasAddressableLocation(string key)
        {
            foreach (var locator in Addressables.ResourceLocators)
            {
                if (locator.Locate(key, typeof(GameObject), out IList<IResourceLocation> locations) &&
                    locations != null &&
                    locations.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }
        private void ProcessLoadCompletion(ChunkCoord coord)
        {
            if (!_pendingLoads.TryGetValue(coord, out var handle))
            {
                return;
            }

            _pendingLoads.Remove(coord);
            _currentlyLoading.Remove(coord);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var loadedObject = handle.Result;
                _chunkCache[coord].RootObject = loadedObject;
                _chunkCache[coord].State = LoadState.Loaded;
                _chunkCache[coord].UpdateLastAccess();

                if (_settings.EnableDebugLogging)
                {
                    Debug.Log($"[ChunkLoader] Loaded {coord.ToChunkId()}");
                }
            }
            else
            {
                HandleLoadFailure(coord);
            }
        }

        private void HandleLoadFailure(ChunkCoord coord)
        {
            if (!_retryCount.ContainsKey(coord))
            {
                _retryCount[coord] = 0;
            }

            _retryCount[coord]++;

            if (_retryCount[coord] < _settings.MaxRetries)
            {
                // 繝ｪ繝医Λ繧､繧偵く繝･繝ｼ縺ｫ謌ｻ縺・                _loadQueue.Enqueue(coord, float.MaxValue);
                if (_settings.EnableDebugLogging)
                {
                    Debug.LogWarning($"[ChunkLoader] Retry loading {coord.ToChunkId()} (attempt {_retryCount[coord]})");
                }
            }
            else
            {
                _chunkCache[coord].State = LoadState.Failed;
                _retryCount.Remove(coord);
                Debug.LogError($"[ChunkLoader] Failed to load {coord.ToChunkId()} after {_settings.MaxRetries} retries");
            }
        }

        /// <summary>
        /// 繝√Ε繝ｳ繧ｯ繧偵い繝ｳ繝ｭ繝ｼ繝峨☆繧・        /// </summary>
        public void Unload(ChunkCoord coord)
        {
            if (!_chunkCache.TryGetValue(coord, out var chunkData))
            {
                return;
            }

            if (chunkData.RootObject != null)
            {
                Addressables.ReleaseInstance(chunkData.RootObject);
                chunkData.RootObject = null;
            }

            _chunkCache.Remove(coord);
            _retryCount.Remove(coord);

            if (_settings.EnableDebugLogging)
            {
                Debug.Log($"[ChunkLoader] Unloaded {coord.ToChunkId()}");
            }
        }

        /// <summary>
        /// 縺吶∋縺ｦ縺ｮ繝ｭ繝ｼ繝画ｸ医∩繝√Ε繝ｳ繧ｯ繧偵い繝ｳ繝ｭ繝ｼ繝峨☆繧・        /// </summary>
        public void UnloadAll()
        {
            var coordsToUnload = new List<ChunkCoord>(_chunkCache.Keys);
            foreach (var coord in coordsToUnload)
            {
                Unload(coord);
            }
            _loadQueue.Clear();
            _pendingLoads.Clear();
            _currentlyLoading.Clear();
            _retryCount.Clear();
            _missingChunkKeys.Clear();
        }

        /// <summary>
        /// 謖・ｮ壹＆繧後◆繝｡繝｢繝ｪ繝舌ず繧ｧ繝・ヨ雜・℃蛻・ｒLRU譁ｹ蠑上〒繧｢繝ｳ繝ｭ繝ｼ繝峨☆繧・        /// </summary>
        public long PurgeExcessMemory(long budgetBytes, int protectionRadius, ChunkCoord centerCoord)
        {
            long totalMemory = CalculateTotalMemory();
            if (totalMemory <= budgetBytes)
            {
                return totalMemory;
            }

            var protectedChunks = new HashSet<ChunkCoord>();
            for (int x = centerCoord.X - protectionRadius; x <= centerCoord.X + protectionRadius; x++)
            {
                for (int z = centerCoord.Z - protectionRadius; z <= centerCoord.Z + protectionRadius; z++)
                {
                    protectedChunks.Add(new ChunkCoord(x, z));
                }
            }

            var sortedByAccess = new List<ChunkData>(_chunkCache.Values);
            sortedByAccess.Sort((a, b) => a.LastAccessTicks.CompareTo(b.LastAccessTicks));

            foreach (var chunkData in sortedByAccess)
            {
                if (totalMemory <= budgetBytes)
                {
                    break;
                }

                if (!protectedChunks.Contains(chunkData.Coord))
                {
                    totalMemory -= chunkData.EstimatedMemoryBytes;
                    Unload(chunkData.Coord);
                }
            }

            return totalMemory;
        }

        /// <summary>
        /// 蜈ｨ繝√Ε繝ｳ繧ｯ縺ｮ蜷郁ｨ医Γ繝｢繝ｪ菴ｿ逕ｨ驥上ｒ險育ｮ励☆繧・        /// </summary>
        public long CalculateTotalMemory()
        {
            long total = 0;
            foreach (var chunkData in _chunkCache.Values)
            {
                total += chunkData.EstimatedMemoryBytes;
            }
            return total;
        }

        /// <summary>
        /// 繝√Ε繝ｳ繧ｯ縺ｮ繧｢繧ｯ繧ｻ繧ｹ繧定ｨ倬鹸縺吶ｋ・域怙蠕後・繧｢繧ｯ繧ｻ繧ｹ譎ょ綾繧呈峩譁ｰ・・        /// </summary>
        public void UpdateChunkAccess(ChunkCoord coord)
        {
            if (_chunkCache.TryGetValue(coord, out var chunkData))
            {
                chunkData.UpdateLastAccess();
            }
        }
    }

    /// <summary>
    /// 霍晞屬繝吶・繧ｹ縺ｮ蜆ｪ蜈亥ｺｦ繧ｭ繝･繝ｼ・域怙蟆上ヲ繝ｼ繝暦ｼ・    /// </summary>
    public class LoadQueue
    {
        private class QueueItem : IComparable<QueueItem>
        {
            public ChunkCoord Coord { get; set; }
            public float Distance { get; set; }

            public int CompareTo(QueueItem other)
            {
                return Distance.CompareTo(other.Distance);
            }
        }

        private readonly List<QueueItem> _heap = new List<QueueItem>();

        public int Count => _heap.Count;

        public void Enqueue(ChunkCoord coord, float distance)
        {
            var item = new QueueItem { Coord = coord, Distance = distance };
            _heap.Add(item);
            HeapifyUp(_heap.Count - 1);
        }

        public ChunkCoord Dequeue()
        {
            if (_heap.Count == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }

            var result = _heap[0].Coord;
            _heap[0] = _heap[_heap.Count - 1];
            _heap.RemoveAt(_heap.Count - 1);

            if (_heap.Count > 0)
            {
                HeapifyDown(0);
            }

            return result;
        }

        public void Clear()
        {
            _heap.Clear();
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (_heap[index].CompareTo(_heap[parentIndex]) >= 0)
                {
                    break;
                }

                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void HeapifyDown(int index)
        {
            while (true)
            {
                int smallest = index;
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;

                if (leftChild < _heap.Count && _heap[leftChild].CompareTo(_heap[smallest]) < 0)
                {
                    smallest = leftChild;
                }

                if (rightChild < _heap.Count && _heap[rightChild].CompareTo(_heap[smallest]) < 0)
                {
                    smallest = rightChild;
                }

                if (smallest == index)
                {
                    break;
                }

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int i, int j)
        {
            (_heap[i], _heap[j]) = (_heap[j], _heap[i]);
        }
    }
}

