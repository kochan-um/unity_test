#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;

namespace WorldStreaming.Tests
{
    [TestFixture]
    public class ChunkCoordTests
    {
        [Test]
        public void ChunkCoord_ToChunkId_FormatsCorrectly()
        {
            var coord = new ChunkCoord(1, 2);
            Assert.AreEqual("chunk_1_2", coord.ToChunkId());
        }

        [Test]
        public void ChunkCoord_NegativeCoordinates()
        {
            var coord = new ChunkCoord(-1, -2);
            Assert.AreEqual("chunk_-1_-2", coord.ToChunkId());
        }

        [Test]
        public void ChunkCoord_Equality()
        {
            var coord1 = new ChunkCoord(1, 2);
            var coord2 = new ChunkCoord(1, 2);
            Assert.AreEqual(coord1, coord2);
        }

        [Test]
        public void ChunkCoord_GetDistance()
        {
            var coord1 = new ChunkCoord(0, 0);
            var coord2 = new ChunkCoord(3, 4);
            // チェビシェフ距離（Max-norm）
            Assert.AreEqual(4, coord1.GetDistance(coord2));
        }

        [Test]
        public void ChunkCoord_GetWorldCenter()
        {
            var coord = new ChunkCoord(1, 2);
            var center = coord.GetWorldCenter(100f);
            Assert.AreEqual(new Vector3(150f, 0, 250f), center);
        }

        [Test]
        public void ChunkCoord_HashCode_Consistent()
        {
            var coord1 = new ChunkCoord(5, 10);
            var coord2 = new ChunkCoord(5, 10);
            Assert.AreEqual(coord1.GetHashCode(), coord2.GetHashCode());
        }
    }

    [TestFixture]
    public class ChunkGridTests
    {
        private ChunkGrid _grid;

        [SetUp]
        public void SetUp()
        {
            _grid = new ChunkGrid(100f);
        }

        [Test]
        public void ChunkGrid_GetChunkCoord_SimplePosition()
        {
            var coord = _grid.GetChunkCoord(new Vector3(150f, 0, 250f));
            Assert.AreEqual(new ChunkCoord(1, 2), coord);
        }

        [Test]
        public void ChunkGrid_GetChunkCoord_NegativePosition()
        {
            var coord = _grid.GetChunkCoord(new Vector3(-50f, 0, -150f));
            Assert.AreEqual(new ChunkCoord(-1, -2), coord);
        }

        [Test]
        public void ChunkGrid_GetChunkCoord_BoundaryCondition()
        {
            var coord = _grid.GetChunkCoord(new Vector3(100f, 0, 100f));
            Assert.AreEqual(new ChunkCoord(1, 1), coord);
        }

        [Test]
        public void ChunkGrid_GetSurroundingChunks_Radius1()
        {
            var chunks = _grid.GetSurroundingChunks(new Vector3(150f, 0, 150f), 1);
            Assert.AreEqual(9, chunks.Count); // 3x3グリッド
            Assert.IsTrue(chunks.Contains(new ChunkCoord(1, 1)));
            Assert.IsTrue(chunks.Contains(new ChunkCoord(0, 0)));
            Assert.IsTrue(chunks.Contains(new ChunkCoord(2, 2)));
        }

        [Test]
        public void ChunkGrid_GetSurroundingChunks_Radius0()
        {
            var chunks = _grid.GetSurroundingChunks(new Vector3(150f, 0, 150f), 0);
            Assert.AreEqual(1, chunks.Count);
            Assert.IsTrue(chunks.Contains(new ChunkCoord(1, 1)));
        }
    }
}
#endif
