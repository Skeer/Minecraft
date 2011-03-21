using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minecraft.Net;
using Minecraft.Packet;
using Minecraft.Map;
using Minecraft.Entities;

namespace Minecraft.Handlers
{
    class PlayerBlockPlacementHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 12)
            {
                int x = stream.ReadInt();
                byte y = stream.ReadByte();
                int z = stream.ReadInt();
                byte direction = stream.ReadByte();
                short id = stream.ReadShort();
                if (id >= 0)
                {
                    if (stream.Length - stream.Position >= 3)
                    {
                        byte amount = stream.ReadByte();
                        short damage = stream.ReadShort();
                        int bx = x;
                        byte by = y;
                        int bz = z;
                        switch (direction)
                        {
                            case 0:
                                --by;
                                break;
                            case 1:
                                ++by;
                                break;
                            case 2:
                                --bz;
                                break;
                            case 3:
                                ++bz;
                                break;
                            case 4:
                                --bx;
                                break;
                            case 5:
                                ++bx;
                                break;
                        }

                        Chunk c = MinecraftServer.Instance.ChunkManager.GetChunkFromBlockCoords(bx, bz);
                        c.SetBlockAt(bx, by, bz, (byte) id);
                        foreach (Player p in MinecraftServer.Instance.Players.Values)
                        {
                            p.Client.Send(MinecraftPacketCreator.GetBlockChange(bx, by, bz, c.GetBlockAt(bx, by, bz), 0x00));
                        }
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }
}
