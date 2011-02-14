using Minecraft.Packet;
using Minecraft.Net;

namespace Minecraft.Handlers
{
    public interface IPacketHandler
    {
        bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream);
    }
}
