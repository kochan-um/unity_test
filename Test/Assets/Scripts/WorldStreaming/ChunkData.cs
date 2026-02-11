using System;
using UnityEngine;

namespace WorldStreaming
{
    /// <summary>
    /// ロード済みチャンクの状態を管理する。
    /// </summary>
    public class ChunkData
    {
        public ChunkCoord Coord { get; }
        public LoadState State { get; set; }
        public LODLevel CurrentLOD { get; set; }
        public long LastAccessTicks { get; set; }
        public long EstimatedMemoryBytes { get; set; }
        public GameObject RootObject { get; set; }

        public ChunkData(ChunkCoord coord)
        {
            Coord = coord;
            State = LoadState.Unloaded;
            CurrentLOD = LODLevel.Low;
            LastAccessTicks = System.DateTime.UtcNow.Ticks;
            EstimatedMemoryBytes = 0;
            RootObject = null;
        }

        public void UpdateLastAccess()
        {
            LastAccessTicks = System.DateTime.UtcNow.Ticks;
        }
    }

    public enum LoadState
    {
        Unloaded,
        Loading,
        Loaded,
        Unloading,
        Failed
    }

    public enum LODLevel
    {
        High = 0,
        Medium = 1,
        Low = 2
    }
}
