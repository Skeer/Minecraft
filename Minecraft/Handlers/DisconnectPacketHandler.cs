using Minecraft.Net;
using Minecraft.Packet;

namespace Minecraft.Handlers
{
    class DisconnectPacketHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 2)
            {
                short length = stream.ReadShort();
                if (stream.Length - stream.Position >= length)
                {
                    string message = stream.ReadString(length);
                    client.Disconnected(message);
                    return true;
                }
            }
            return false;
        }
    }
}
