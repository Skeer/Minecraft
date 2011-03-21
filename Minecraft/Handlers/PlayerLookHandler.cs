using Minecraft.Net;
using Minecraft.Packet;

namespace Minecraft.Handlers
{
    class PlayerLookHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 9)
            {
                float yaw = stream.ReadFloat();
                float pitch = stream.ReadFloat();
                client.Player.Yaw = yaw;
                client.Player.Pitch = pitch;
                client.Player.OnGround = stream.ReadBool();
                return true;
            }
            return false;
        }
    }
}
