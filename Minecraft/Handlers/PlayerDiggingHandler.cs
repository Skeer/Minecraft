using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minecraft.Net;
using Minecraft.Packet;

namespace Minecraft.Handlers
{
    //enum PlayerDiggingStatus
    //{
    //    Started,
    //    Digging,
    //    Stopped,
    //    Broken,
    //    Dropped
    //}

    class PlayerDiggingHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 1)
            {
                //PlayerDiggingStatus status = (PlayerDiggingStatus)stream.ReadByte();
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
                                //TODO: Validation
                                //switch(status)
                                //{
                                //    case PlayerDiggingStatus.Started:
                                //        break;
                                //    case PlayerDiggingStatus.Digging:
                                //        break;
                                //    case PlayerDiggingStatus.Stopped:
                                //        break;
                                //    case PlayerDiggingStatus.Broken:
                                //        break;
                                //    case PlayerDiggingStatus.Dropped:
                                //        break;
                                //}

                                if (status == 4)
                                {
                                }

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
