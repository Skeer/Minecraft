using System;
using Minecraft.Net;
using Minecraft.Packet;

namespace Minecraft.Handlers
{
    class ChatMessageHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 2)
            {
                short length = stream.ReadShort();
                if (stream.Length - stream.Position >= length)
                {
                    string message = stream.ReadString(length);
                    if (message.Substring(0, 1) == "/")
                    {
                        string[] splitted = message.Substring(1).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (MinecraftServer.Instance.CommandManager.CommandExists(splitted[0].ToLower()))
                        {
                            MinecraftServer.Instance.CommandManager.RunCommand(client, splitted[0].ToLower(), splitted);
                        }
                        else if (splitted[0].ToLower() == "help")
                        {
                            client.Send(MinecraftPacketCreator.GetChatMessage("Availiable commands are:"));
                            foreach (string key in MinecraftServer.Instance.CommandManager.GetCommands())
                            {
                                client.Send(MinecraftPacketCreator.GetChatMessage(key));
                            }
                        }
                        else
                        {
                            client.Send(MinecraftPacketCreator.GetChatMessage("Command " + splitted[0] + " does not exist."));
                        }
                    }
                    else
                    {
                        foreach (MinecraftClient c in MinecraftServer.Instance.Clients)
                        {
                            c.Send(MinecraftPacketCreator.GetChatMessage(client.Username, message));
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
