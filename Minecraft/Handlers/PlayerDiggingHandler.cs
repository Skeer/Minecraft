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
                        if (b != 0 || true)
                        {
                            c.SetBlockAt(x, y, z, 0);
                            Random r = new Random();
                            Drop d = new Drop() { ID = b, EID = MinecraftServer.Instance.Entity++, X = x + r.NextDouble(), Y = y + 1, Z = z + r.NextDouble() };
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
                                    // explictly update players within range
                                    p.Client.Player.Update();
                                }
                            }
                        }
                        break;
                    case PlayerDiggingStatus.Started:
                        // Send packets to clients in range
                        foreach (Player p in MinecraftServer.Instance.Players.Values)
                        {
                            if (p != client.Player && p.IsInRange(c.X, c.Z))
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
