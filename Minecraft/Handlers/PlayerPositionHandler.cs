using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minecraft.Packet;
using Minecraft.Net;

namespace Minecraft.Handlers
{
    class PlayerPositionHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 8)
            {
                double x = stream.ReadDouble();
                if (stream.Length - stream.Position >= 8)
                {
                    double y = stream.ReadDouble();
                    if (stream.Length - stream.Position >= 8)
                    {
                        double stance = stream.ReadDouble();
                        if (stream.Length - stream.Position >= 8)
                        {
                            double z = stream.ReadDouble();
                            if (stream.Length - stream.Position >= 1)
                            {
                                bool onGround = stream.ReadBool();
                                client.Player.Position.X = x;
                                client.Player.Position.Y = y;
                                client.Player.Stance = stance;
                                client.Player.Position.Z = z;
                                client.Player.OnGround = onGround;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
