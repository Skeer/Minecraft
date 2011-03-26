using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minecraft.Packet;
using Minecraft.Net;
using Minecraft.Entities;

namespace Minecraft.Handlers
{
    class AnimationHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 5)
            {
                uint eid = stream.ReadUint();
                MinecraftAnimation animate = (MinecraftAnimation) stream.ReadByte();
                foreach (Player p in from p in MinecraftServer.Instance.Players.Values
                                     where p != client.Player && p.IsInRange(client.Player.CurrentChunk.Location.X, client.Player.CurrentChunk.Location.Z)
                                     select p)
                {
                    p.Client.Send(MinecraftPacketCreator.GetAnimation(eid, animate));
                }
                return true;

            }
            return false;
        }
    }
}
