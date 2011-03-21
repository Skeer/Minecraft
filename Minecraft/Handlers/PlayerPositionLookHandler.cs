using Minecraft.Net;
using Minecraft.Packet;

namespace Minecraft.Handlers
{
    class PlayerPositionLookHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            //TODO: Static Packet Lengths check.........

            // Ex:
            if (stream.Length - stream.Position >= 41)
            {
                client.Player.X = stream.ReadDouble();
                client.Player.Stance = stream.ReadDouble();
                client.Player.Y = stream.ReadDouble();
                client.Player.Z = stream.ReadDouble();
                client.Player.Yaw = stream.ReadFloat();
                client.Player.Pitch = stream.ReadFloat();
                client.Player.OnGround = stream.ReadBool();
                client.Player.Update();
                return true;
            }
            return false;
        }
    }
}
