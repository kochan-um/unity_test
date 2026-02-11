using System;
using UnityEngine;

namespace WorldStreaming
{
    /// <summary>
    /// チャンク座標を表す構造体。グリッドベースのチャンク管理で使用される。
    /// </summary>
    [System.Serializable]
    public struct ChunkCoord : IEquatable<ChunkCoord>
    {
        public int X { get; }
        public int Z { get; }

        public ChunkCoord(int x, int z)
        {
            X = x;
            Z = z;
        }

        /// <summary>
        /// チャンクIDを「chunk_x_z」形式で返す
        /// </summary>
        public string ToChunkId()
        {
            return $"chunk_{X}_{Z}";
        }

        /// <summary>
        /// チャンク座標からワールド座標の中心を計算する
        /// </summary>
        public Vector3 GetWorldCenter(float chunkSize)
        {
            float centerX = (X + 0.5f) * chunkSize;
            float centerZ = (Z + 0.5f) * chunkSize;
            return new Vector3(centerX, 0, centerZ);
        }

        /// <summary>
        /// チャンク座標からワールド座標の最小値を計算する
        /// </summary>
        public Vector3 GetWorldMin(float chunkSize)
        {
            return new Vector3(X * chunkSize, 0, Z * chunkSize);
        }

        /// <summary>
        /// チャンク座標からワールド座標の最大値を計算する
        /// </summary>
        public Vector3 GetWorldMax(float chunkSize)
        {
            return new Vector3((X + 1) * chunkSize, 0, (Z + 1) * chunkSize);
        }

        /// <summary>
        /// 2つのチャンク座標間のチェビシェフ距離（Max-norm）を計算する
        /// </summary>
        public int GetDistance(ChunkCoord other)
        {
            return Mathf.Max(Mathf.Abs(X - other.X), Mathf.Abs(Z - other.Z));
        }

        public bool Equals(ChunkCoord other)
        {
            return X == other.X && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkCoord other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Z);
        }

        public override string ToString()
        {
            return ToChunkId();
        }

        public static bool operator ==(ChunkCoord lhs, ChunkCoord rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ChunkCoord lhs, ChunkCoord rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
