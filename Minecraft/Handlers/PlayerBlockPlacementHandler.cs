using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minecraft.Net;
using Minecraft.Packet;
using Minecraft.Map;
using Minecraft.Entities;
using Minecraft.Items;
using Minecraft.Utilities;

namespace Minecraft.Handlers
{
    class PlayerBlockPlacementHandler : IPacketHandler
    {
        private static Logger Log = new Logger(typeof(PlayerBlockPlacementHandler));

        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 12)
            {
                int x = stream.ReadInt();
                byte y = stream.ReadByte();
                int z = stream.ReadInt();
                byte direction = stream.ReadByte();
                short id = stream.ReadShort();
                byte amount;
                short damage;
                if (id > 0)
                {
                    if (stream.Length - stream.Position >= 3)
                    {
                        amount = stream.ReadByte();
                        damage = stream.ReadShort();
                    }
                    else
                    {
                        return false;
                    }
                }

                if (direction != 255)
                {
                    Chunk c = MinecraftServer.Instance.ChunkManager.GetChunkFromBlockCoords(x, z);
                    if (c.GetBlockAt(x, y, z) == 26)
                    {
                        byte m = c.GetMetaDataAt(x, y, z);
                        if (y % 2 == 0)
                        {
                            m &= 0x0f;
                        }
                        else
                        {
                            m &= 0xf0;
                            m /= 16;
                        }

                        int bx = x;
                        byte by = y;
                        int bz = z;

                        bool pillow = false;
                        if (m > 8)
                        {
                            pillow = true;
                        }

                        switch (m % 8)
                        {
                            case 0:
                                if (pillow)
                                {
                                    --bz;
                                }
                                else
                                {
                                    ++bz;
                                }
                                break;
                            case 1:
                                if (pillow)
                                {
                                    ++bx;
                                }
                                else
                                {
                                    --bx;
                                }
                                break;
                            case 2:
                                if (pillow)
                                {
                                    ++bz;
                                }
                                else
                                {
                                    --bz;
                                }
                                break;
                            case 3:
                                if (pillow)
                                {
                                    --bx;
                                }
                                else
                                {
                                    ++bx;
                                }
                                break;
                        }


                        c = MinecraftServer.Instance.ChunkManager.GetChunkFromBlockCoords(bx, bz);
                        if (c.GetBlockAt(bx, by, bz) == 26)
                        {
                            Point<int, byte, int> p;
                            if (pillow)
                            {
                                p = new Point<int, byte, int>() { X = x, Y = y, Z = z };
                            }
                            else
                            {
                                p = new Point<int, byte, int>() { X = bx, Y = by, Z = bz };
                            }
                            if (!MinecraftServer.Instance.Beds.ContainsKey(p))
                            {
                                // bed not in use
                                MinecraftServer.Instance.Beds.Add(p, client.Player.EID);
                            }
                        }
                        else
                        {
                            Log.Warning("Player {0} failed to use bed.", client.Player.Username);
                        }

                        // 1 1 1 1
                        // at least 2 bits needed for orrientation
                        // at least 2 1 bit needed for section
                        // last 2 bits are orrientation
                        // first bit is section?
                        //bed, sleep
                    }
                    else
                    {

                        if (client.Player.Inventory.ContainsKey((byte)(client.Player.HoldingSlot + 36)))
                        {
                            Item i = client.Player.Inventory[(byte)(client.Player.HoldingSlot + 36)];
                            if (i.Count > 0 && i.ID > 0)
                            {
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
                                    case 255:
                                        //Special case
                                        return true;
                                }

                                // TODO: Need validatation
                                // ex: Check whether player is in the way...

                                c = MinecraftServer.Instance.ChunkManager.GetChunkFromBlockCoords(bx, bz);
                                c.SetBlockAt(bx, by, bz, (byte)i.ID);
                                --i.Count;
                                if (i.Count <= 0)
                                {
                                    client.Player.Inventory.Remove(i.Slot);
                                }
                                foreach (Player p in MinecraftServer.Instance.Players.Values)
                                {
                                    p.Client.Send(MinecraftPacketCreator.GetBlockChange(bx, by, bz, c.GetBlockAt(bx, by, bz), 0x00));
                                }
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}
