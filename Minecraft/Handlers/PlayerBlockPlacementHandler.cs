using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minecraft.Net;
using Minecraft.Packet;
using Minecraft.Map;

namespace Minecraft.Handlers
{
    class PlayerBlockPlacementHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 4)
            {
                int x = stream.ReadInt();
                if (stream.Length - stream.Position >= 1)
                {
                    byte y = stream.ReadByte();
                    if (stream.Length - stream.Position >= 4)
                    {
                        int z = stream.ReadInt();
                        if (stream.Length - stream.Position >= 1)
                        {
                            byte direction = stream.ReadByte();
                            if (stream.Length - stream.Position >= 2)
                            {
                                short id = stream.ReadShort();
                                if (id >= 0)
                                {
                                    if (stream.Length - stream.Position >= 1)
                                    {
                                        byte amount = stream.ReadByte();
                                        if (stream.Length - stream.Position >= 2)
                                        {
                                            short damage = stream.ReadShort();
                                            Chunk c = MinecraftServer.Instance.ChunkManager.GetChunk(x / 16, z / 16);
                                            int i  = Math.Abs(y + z * 128 + x * 256);
                                            c.Blocks[i] = (byte) id;
                                            return true;
                                        }
                                    }
                                }
                                else
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
