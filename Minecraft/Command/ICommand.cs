using Minecraft.Net;

namespace Minecraft.Command
{
    public interface ICommand
    {
        void Run(MinecraftServer server, MinecraftClient client, string[] args);
    }
}
