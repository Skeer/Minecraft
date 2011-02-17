using Minecraft.Net;

namespace Minecraft.Command
{
    public interface ICommand
    {
        MinecraftRank Rank { get; }
        void Run(MinecraftServer server, MinecraftClient client, string[] args);
    }
}
