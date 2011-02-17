using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minecraft.Packet;
using Minecraft.Net;

namespace Minecraft.Handlers
{
    class AnimationHandler :IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 4)
            {
                uint eid = stream.ReadUint();
                if (stream.Length - stream.Position >= 1)
                {
                    byte animate = stream.ReadByte();
                    //TODO
                    return true;
                }
            }
            return false;
        }
    }
}
