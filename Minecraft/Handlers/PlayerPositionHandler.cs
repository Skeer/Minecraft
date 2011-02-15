using Minecraft.Net;
using Minecraft.Packet;

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
                                client.Player.X = x;
                                client.Player.Y = y;
                                client.Player.Stance = stance;
                                client.Player.Z = z;
                                client.Player.OnGround = stream.ReadBool();
                                client.Player.Update();
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
