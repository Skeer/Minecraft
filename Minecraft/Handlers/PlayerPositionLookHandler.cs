using System;
using Minecraft.Packet;
using Minecraft.Net;

namespace Minecraft.Handlers
{
    class PlayerPositionLookHandler:IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            // NOTE: Maybe bad practice... Probably a better idea to store data after packet 100% received...
            // Although I don't see anything that might go wrong... Since the server and client is essentially "syncronized"
            if (stream.Length - stream.Position >= 8)
            {
                client.Player.Position.X = stream.ReadDouble();
                if (stream.Length - stream.Position >= 8)
                {
                    client.Player.Stance = stream.ReadDouble();
                    if (stream.Length - stream.Position >= 8)
                    {
                        client.Player.Position.Y = stream.ReadDouble();
                        if (stream.Length - stream.Position >= 8)
                        {
                            client.Player.Position.Z = stream.ReadDouble();
                            if (stream.Length - stream.Position >= 4)
                            {
                                client.Player.Rotation.Yaw = stream.ReadFloat();
                                if (stream.Length - stream.Position >= 4)
                                {
                                    client.Player.Rotation.Pitch = stream.ReadFloat();
                                    if (stream.Length - stream.Position >= 1)
                                    {
                                        client.Player.OnGround = stream.ReadBool();
                                        return true;
                                    }
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
