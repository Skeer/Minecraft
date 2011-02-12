using Minecraft.Packet;
using Minecraft.Net;

namespace Minecraft.Handlers
{
    interface IPacketHandler
    {
        bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream);
    }
}
