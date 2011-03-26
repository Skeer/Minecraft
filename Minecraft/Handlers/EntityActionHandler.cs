using System;
using System.Linq;
using Minecraft.Net;
using Minecraft.Packet;
using Minecraft.Entities;
using Minecraft.Utilities;

namespace Minecraft.Handlers
{
    class EntityActionHandler : IPacketHandler
    {
        private static Logger Log = new Logger(typeof(EntityActionHandler));

        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 5)
            {
                uint eid = stream.ReadUint();
                MinecraftEntityAction action = (MinecraftEntityAction)stream.ReadByte();
                if (eid == client.Player.EID)
                {
                    // 1 = crouch
                    // 2 = uncrouch
                    // 3 = leave bed
                    //p.Client.Send(MinecraftPacketCreator.GetEntityAction(eid, action));
                    switch (action)
                    {
                        case MinecraftEntityAction.Crouch:
                            foreach (Player p in from p in MinecraftServer.Instance.Players.Values
                                                 where p != client.Player && p.IsInRange(client.Player.CurrentChunk.Location.X, client.Player.CurrentChunk.Location.Z)
                                                 select p)
                            {
                                p.Client.Send(MinecraftPacketCreator.GetAnimation(eid, MinecraftAnimation.Crouch));
                            }
                            break;
                        case MinecraftEntityAction.Uncrouch:
                            foreach (Player p in from p in MinecraftServer.Instance.Players.Values
                                                 where p != client.Player && p.IsInRange(client.Player.CurrentChunk.Location.X, client.Player.CurrentChunk.Location.Z)
                                                 select p)
                            {
                                p.Client.Send(MinecraftPacketCreator.GetAnimation(eid, MinecraftAnimation.Uncrouch));
                            }
                            break;
                        case MinecraftEntityAction.Bed:
                            // unbed
                            foreach (Player p in from p in MinecraftServer.Instance.Players.Values
                                                 where p.IsInRange(client.Player.CurrentChunk.Location.X, client.Player.CurrentChunk.Location.Z)
                                                 select p)
                            {
                                p.Client.Send(MinecraftPacketCreator.GetAnimation(eid, MinecraftAnimation.Bed));
                            }
                            break;
                    }
                }
                else
                {
                    Log.Warning("EID of {0} does not correspond with player's EID of {1}.", eid, client.Player.EID);
                }
                return true;
            }
            return false;
        }
    }
}
