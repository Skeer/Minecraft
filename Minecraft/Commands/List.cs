using Minecraft.Command;
using Minecraft.Net;
using System.Text;
using Minecraft.Packet;
using System;

namespace Minecraft.Commands
{
    class List : MarshalByRefObject, ICommand
    {
        public void Run(MinecraftServer server, MinecraftClient client, string[] args)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Connected users: ");
            foreach (MinecraftClient c in server.Clients)
            {
                builder.Append(c.Username);
                builder.Append(", ");
            }
            builder.Remove(builder.Length - 2, 2);
            builder.Append(".");
            client.Send(MinecraftPacketCreator.GetChatMessage(builder.ToString()));
        }
    }
}
