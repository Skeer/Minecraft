using Minecraft.Net;
using Minecraft.Packet;

namespace Minecraft.Handlers
{
    class PlayerLookHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 4)
            {
                float yaw = stream.ReadFloat();
                if (stream.Length - stream.Position >= 4)
                {
                    float pitch = stream.ReadFloat();
                    if (stream.Length - stream.Position >= 1)
                    {
                        client.Player.Yaw = yaw;
                        client.Player.Pitch = pitch;
                        client.Player.OnGround = stream.ReadBool();
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
