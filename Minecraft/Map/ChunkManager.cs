using System.Collections.Generic;
using System.Text;
using Minecraft.Net;
using Minecraft.Utilities;
using System;

namespace Minecraft.Map
{
    public class ChunkManager
    {
        private Dictionary<int, Dictionary<int, Chunk>> Chunks = new Dictionary<int, Dictionary<int, Chunk>>();
        private Dictionary<int, Dictionary<int, RegionFile>> Regions = new Dictionary<int, Dictionary<int, RegionFile>>();

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

        public Chunk GetChunkFromBlockCoords(int x, int z)
        {
            return GetChunk((x - 1) / 16, (z - 1) / 16);
        }

        /// <summary>
        /// Get chunk at coordinate.
        /// </summary>
        /// <param name="x">X coordinate in chunks (16 blocks).</param>
        /// <param name="z">Z coordinate in chunks (16 blocks).</param>
        /// <returns></returns>
        public Chunk GetChunk(int x, int z)
        {
            if (Chunks.ContainsKey(x))
            {
                if (!Chunks[x].ContainsKey(z))
                {
                    int rx, rz;
                    RegionFile.ChunkCoordToRegionCoord(x, z, out rx, out rz);
                    if (Regions.ContainsKey(rx))
                    {
                        if (!Regions[rx].ContainsKey(rz))
                        {
                            Regions[rx].Add(rz, new RegionFile(rx, rz));
                        }
                    }
                    else
                    {
                        Regions.Add(rx, new Dictionary<int, RegionFile>());
                        Regions[rx].Add(rz, new RegionFile(rx, rz));
                    }
                    Chunks[x].Add(z, new Chunk(Regions[rx][rz], Regions[rx][rz].GetChunkData(x, z)));
                }
            }
            else
            {
                Chunks.Add(x, new Dictionary<int, Chunk>());
                int rx, rz;
                RegionFile.ChunkCoordToRegionCoord(x, z, out rx, out rz);
                if (Regions.ContainsKey(rx))
                {
                    if (!Regions[rx].ContainsKey(rz))
                    {
                        Regions[rx].Add(rz, new RegionFile(rx, rz));
                    }
                }
                else
                {
                    Regions.Add(rx, new Dictionary<int, RegionFile>());
                    Regions[rx].Add(rz, new RegionFile(rx, rz));
                }
                Chunks[x].Add(z, new Chunk(Regions[rx][rz], Regions[rx][rz].GetChunkData(x, z)));
            }
            return Chunks[x][z];
        }
    }
}
