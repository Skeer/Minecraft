using System.Text;
using Minecraft.Utilities;
using System.Collections.Generic;
using Minecraft.Net;

namespace Minecraft.Map
{
    public class ChunkManager
    {
        private Dictionary<PointInt, Chunk> Chunks = new Dictionary<PointInt, Chunk>();

        public string GetChunkPath(PointInt p)
        {
            // TODO: Check path seperators? ROFL

            StringBuilder builder = new StringBuilder();
            builder.Append(MinecraftServer.Instance.Path);
            builder.Append(Base36.Parse(p.X & 63));
            builder.Append("/");
            builder.Append(Base36.Parse(p.Z & 63));
            builder.Append("/c.");
            builder.Append(Base36.Parse(p.X));
            builder.Append(".");
            builder.Append(Base36.Parse(p.Z));
            builder.Append(".dat");
            return builder.ToString();
        }

        public List<Chunk> GetChunks(PointInt p)
        {
            int x = p.X + 3;
            int z = p.Z + 3;
            List<Chunk> chunks = new List<Chunk>();
            for (int a = x - 6; a <= x; ++a)
            {
                for (int b = z - 6; b <= z; ++b)
                {
                    chunks.Add(GetChunk(new PointInt() { X = a, Z = b }));
                }
            }
            return chunks;
        }

        public Chunk GetChunk(PointInt p)
        {
            if (!Chunks.ContainsKey(p))
            {
                Chunks.Add(p, new Chunk(GetChunkPath(p)));
            }
            return Chunks[p];
        }
    }
}
