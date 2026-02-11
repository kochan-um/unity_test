using System.Collections.Generic;
using UnityEngine;

namespace WorldStreaming
{
    /// <summary>
    /// グリッドベースのチャンク座標系を管理する。
    /// ワールド座標からチャンクID、またはその逆の変換を O(1) で実行できる。
    /// </summary>
    public class ChunkGrid
    {
        private readonly float _chunkSize;

        public float ChunkSize => _chunkSize;

        public ChunkGrid(float chunkSize)
        {
            _chunkSize = chunkSize;
        }

        /// <summary>
        /// ワールド座標からチャンク座標を取得する
        /// </summary>
        public ChunkCoord GetChunkCoord(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt(worldPosition.x / _chunkSize);
            int z = Mathf.FloorToInt(worldPosition.z / _chunkSize);
            return new ChunkCoord(x, z);
        }

        /// <summary>
        /// ワールド座標からチャンクIDを直接取得する
        /// </summary>
        public string GetChunkId(Vector3 worldPosition)
        {
            return GetChunkCoord(worldPosition).ToChunkId();
        }

        /// <summary>
        /// 指定された座標から指定半径内のすべてのチャンク座標を取得する。
        /// 半径はチェビシェフ距離（Max-norm）で計算される。
        /// </summary>
        public HashSet<ChunkCoord> GetSurroundingChunks(Vector3 worldPosition, int radius)
        {
            var result = new HashSet<ChunkCoord>();
            var centerCoord = GetChunkCoord(worldPosition);

            for (int x = centerCoord.X - radius; x <= centerCoord.X + radius; x++)
            {
                for (int z = centerCoord.Z - radius; z <= centerCoord.Z + radius; z++)
                {
                    result.Add(new ChunkCoord(x, z));
                }
            }

            return result;
        }

        /// <summary>
        /// 2つのチャンク座標セット間の差分を計算する。
        /// 新しい座標セットで追加されたチャンク、削除されたチャンクを返す。
        /// </summary>
        public void GetChunkDifference(
            HashSet<ChunkCoord> oldChunks,
            HashSet<ChunkCoord> newChunks,
            out HashSet<ChunkCoord> toLoad,
            out HashSet<ChunkCoord> toUnload)
        {
            toLoad = new HashSet<ChunkCoord>(newChunks);
            toLoad.ExceptWith(oldChunks);

            toUnload = new HashSet<ChunkCoord>(oldChunks);
            toUnload.ExceptWith(newChunks);
        }
    }
}
