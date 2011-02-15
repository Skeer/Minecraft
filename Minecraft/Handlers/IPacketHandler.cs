using Minecraft.Net;
using Minecraft.Packet;

namespace Minecraft.Handlers
{
    public interface IPacketHandler
    {
        bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream);
    }
}
