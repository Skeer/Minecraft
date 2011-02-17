using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minecraft.Net;
using Minecraft.Packet;

namespace Minecraft.Handlers
{
    class PlayerDiggingHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 1)
            {
                byte status = stream.ReadByte();
                if (stream.Length - stream.Position >= 4)
                {
                    int x = stream.ReadInt();
                    if (stream.Length - stream.Position >= 1)
                    {
                        byte y = stream.ReadByte();
                        if (stream.Length - stream.Position >= 4)
                        {
                            int z = stream.ReadInt();
                            if (stream.Length - stream.Position >= 1)
                            {
                                byte face = stream.ReadByte();
                                //TODO
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
