using System;
using Minecraft.Net;
using Minecraft.Packet;
using Minecraft.Entities;

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
                        else if (splitted[0].ToLower() == "reload")
                        {
                            MinecraftServer.Instance.CommandManager.ReloadCommands();
                        }
                        else if (splitted[0].ToLower() == "help")
                        {
                            client.Send(MinecraftPacketCreator.GetChatMessage("Availiable commands are:"));
                            foreach (string key in MinecraftServer.Instance.CommandManager.GetCommands())
                            {
                                client.Send(MinecraftPacketCreator.GetChatMessage(key));
                            }
                            client.Send(MinecraftPacketCreator.GetChatMessage("reload"));
                        }
                        else
                        {
                            client.Send(MinecraftPacketCreator.GetChatMessage("Command " + splitted[0] + " does not exist."));
                        }
                    }
                    else
                    {
                        foreach (Player p in MinecraftServer.Instance.Players.Values)
                        {
                            p.Client.Send(MinecraftPacketCreator.GetChatMessage(client.Username, message));
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
