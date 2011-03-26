using System.Collections.Generic;
using System.Text;
using Minecraft.Net;
using Minecraft.Utilities;
using System;

namespace Minecraft.Map
{
    public class ChunkManager
    {
        private Dictionary<Point<int, int, int>, Chunk> Chunks = new Dictionary<Point<int, int, int>, Chunk>();
        private Dictionary<Point<int, int, int>, RegionFile> Regions = new Dictionary<Point<int, int, int>, RegionFile>();

        /// <summary>
        /// Get chunks within range.
        /// </summary>
        /// <param name="x">X coordinate in chunks (16 blocks).</param>
        /// <param name="z">Z coordinate in chunks (16 blocks).</param>
        /// <returns></returns>
        public List<Chunk> GetChunks(int x, int z)
        {
            int xf = x + 5;
            int zf = z + 5;
            List<Chunk> chunks = new List<Chunk>();
            for (int a = x - 5; a <= xf; ++a)
            {
                for (int b = z - 5; b <= zf; ++b)
                {
                    if (Math.Abs(a - x) + Math.Abs(b - z) < 8)
                    {
                        chunks.Add(GetChunk(a, b));
                    }
                }
            }
            return chunks;
        }

        public Chunk GetChunkFromBlockCoords(double x, double z)
        {
            return GetChunk((int)UnitConverter.FromBlockCoordToChunkCoord(x), (int)UnitConverter.FromBlockCoordToChunkCoord(z));
        }

        /// <summary>
        /// Get chunk at coordinate.
        /// </summary>
        /// <param name="x">X coordinate in chunks (16 blocks).</param>
        /// <param name="z">Z coordinate in chunks (16 blocks).</param>
        /// <returns></returns>
        public Chunk GetChunk(int x, int z)
        {
            Point<int, int, int> p = new Point<int, int, int>() { X = x, Z = z };
            if (!Chunks.ContainsKey(p))
            {
                Point<int, int, int> rP = new Point<int, int, int>() { X = (int)UnitConverter.FromChunkCoordToRegionCoord(x), Z = (int)UnitConverter.FromChunkCoordToRegionCoord(z) };
                if (!Regions.ContainsKey(rP))
                {
                        Regions.Add(rP, new RegionFile(rP.X, rP.Z));
                }
                Chunks.Add(p, new Chunk(Regions[rP], Regions[rP].GetChunkData(x, z)));
            }
            return Chunks[p];
        }
    }
}
