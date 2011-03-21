using Minecraft.Net;
using Minecraft.Packet;

namespace Minecraft.Handlers
{
    class PlayerPositionHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 33)
            {
                double x = stream.ReadDouble();
                double y = stream.ReadDouble();
                double stance = stream.ReadDouble();
                double z = stream.ReadDouble();
                client.Player.X = x;
                client.Player.Y = y;
                client.Player.Stance = stance;
                client.Player.Z = z;
                client.Player.OnGround = stream.ReadBool();
                client.Player.Update();
                return true;
            }
            return false;
        }
    }
}
