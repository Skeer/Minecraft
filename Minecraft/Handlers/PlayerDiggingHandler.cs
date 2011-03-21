using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minecraft.Net;
using Minecraft.Packet;
using Minecraft.Entities;
using Minecraft.Map;

namespace Minecraft.Handlers
{
    class PlayerDiggingHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 11)
            {
                //byte status = stream.ReadByte();
                PlayerDiggingStatus status = (PlayerDiggingStatus)stream.ReadByte();

                int x = stream.ReadInt();
                byte y = stream.ReadByte();
                int z = stream.ReadInt();
                byte face = stream.ReadByte();
                if (status != PlayerDiggingStatus.Dropped && Math.Pow(Math.Abs(x - client.Player.X), 2) + Math.Pow(Math.Abs(y - client.Player.Y), 2) + Math.Pow(Math.Abs(z - client.Player.Z), 2) > 216)
                {
                    // validation failed, skipping packet
                    return true;
                }
                Chunk c = MinecraftServer.Instance.ChunkManager.GetChunkFromBlockCoords(x, z);
                switch (status)
                {
                    case PlayerDiggingStatus.Finished:
                        //REMOVE BLOCK
                        byte b = c.GetBlockAt(x, y, z);
                        c.SetBlockAt(x, y, z, 0);
                        Drop d = new Drop() {ID = b, EID = MinecraftServer.Instance.Entity++, X = x, Y = y, Z = z };
                        c.Entities.Add(d);
                        MinecraftServer.Instance.Entities.Add(d.EID, d);
                        foreach (Player p in MinecraftServer.Instance.Players.Values)
                        {
                            if (p.IsInRange(c.X, c.Z))
                            {
                                if (p != client.Player)
                                {
                                    p.Client.Send(MinecraftPacketCreator.GetPlayerDigging(status, x, y, z, face));
                                }
                                p.Client.Send(MinecraftPacketCreator.GetBlockChange(x, y, z, c.GetBlockAt(x, y, z), 0x00));
                                p.Client.Send(MinecraftPacketCreator.GetEntity(d.EID));
                                p.Client.Send(MinecraftPacketCreator.GetPickupSpawn(d.EID, d.ID, 1, 0, (int)d.X, (int)d.Y, (int)d.Z, (byte)d.Yaw, (byte)d.Pitch, (byte)12));
                            }
                        }
                        break;
                    case PlayerDiggingStatus.Started:
                        // Send packets to clients in range
                        foreach (Player p in MinecraftServer.Instance.Players.Values)
                        {
                            if (p!=client.Player && p.IsInRange(c.X, c.Z))
                            {
                                p.Client.Send(MinecraftPacketCreator.GetPlayerDigging(status, x, y, z, face));
                            }
                        }
                        break;
                    case PlayerDiggingStatus.Dropped:
                        break;
                }
                return true;
            }
            return false;
        }
    }
}
