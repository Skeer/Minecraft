using System;
using Minecraft.Command;
using Minecraft.Net;
using Minecraft.Packet;

namespace Minecraft.Commands
{
    class Spawn : MarshalByRefObject, ICommand
    {
        public MinecraftRank Rank
        {
            get { return MinecraftRank.Player; }
        }

        public void Run(MinecraftServer server, MinecraftClient client, string[] args)
        {
            //Warp someone else
            if (args.Length > 1)
            {
                if (server.Players.ContainsKey(args[1].ToLower()))
                {
                    server.Players[args[1].ToLower()].ToSpawn();
                }
                else
                {
                    client.Send(MinecraftPacketCreator.GetChatMessage("Unable to find player " + args[1] + "."));
                }
            }
            else
            {
                client.Player.ToSpawn();
            }
        }
    }
}
