﻿using System;
using Minecraft.Net;
using Minecraft.Packet;

namespace Minecraft.Handlers
{
    class HandshakePacketHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 2)
            {
                short length = stream.ReadShort();
                if (stream.Length - stream.Position >= length)
                {
                    client.Username = stream.ReadString(length);

                    if (MinecraftServer.Instance.Authentication == MinecraftAuthentication.Online)
                    {
                        Random random = new Random();
                        client.Hash = random.Next().ToString("X");
                        client.Send(MinecraftPacketCreator.GetHandshake(client.Hash));
                    }
                    else if (MinecraftServer.Instance.Authentication == MinecraftAuthentication.Offline)
                    {
                        client.Send(MinecraftPacketCreator.GetHandshake("-"));
                    }
                    else if (MinecraftServer.Instance.Authentication == MinecraftAuthentication.Password)
                    {
                        client.Send(MinecraftPacketCreator.GetHandshake("+"));
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
