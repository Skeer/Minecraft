using Minecraft.Packet;
using Minecraft.Net;

namespace Minecraft.Handlers
{
    class KeepAliveHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            //client.ResetConnectionTimer();
            return true;
        }
    }
}
