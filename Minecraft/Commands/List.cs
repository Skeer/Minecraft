using System;
using System.Text;
using Minecraft.Command;
using Minecraft.Net;
using Minecraft.Packet;
using Minecraft.Entities;

namespace Minecraft.Commands
{
    class List : MarshalByRefObject, ICommand
    {

        public void Run(MinecraftServer server, MinecraftClient client, string[] args)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Connected users: ");
            foreach (Player p in server.Players.Values)
            {
                builder.Append(p.Username);
                builder.Append(", ");
            }
            builder.Remove(builder.Length - 2, 2);
            builder.Append(".");
            client.Send(MinecraftPacketCreator.GetChatMessage(builder.ToString()));
        }

        public MinecraftRank Rank
        {
            get { return MinecraftRank.Player; }
        }
    }
}
