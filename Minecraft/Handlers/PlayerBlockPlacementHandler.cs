using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minecraft.Net;
using Minecraft.Packet;
using Minecraft.Map;
using Minecraft.Entities;
using Minecraft.Items;

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
                short id = stream.ReadShort(); // not needed :D just take holding item...
                if (client.Player.Inventory.ContainsKey((byte)(client.Player.HoldingSlot + 36)))
                {
                    Item i = client.Player.Inventory[(byte)(client.Player.HoldingSlot + 36)];
                    if (i.Count > 0 && i.ID > 0)
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
                                case 255:
                                    //Special case
                                    return true;
                            }

                            // TODO: Need validatation
                            // ex: Check whether player is in the way...

                            Chunk c = MinecraftServer.Instance.ChunkManager.GetChunkFromBlockCoords(bx, bz);
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
                return true;
            }
            return false;
        }
    }
}
