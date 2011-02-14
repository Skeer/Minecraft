using Minecraft.Net;
using NBTLibrary;
using System.Collections.Generic;
using Minecraft.Map;
using System;

namespace Minecraft.Entities
{
    public class Player : IEntity
    {
        public Rotation Rotation { get; set; }
        public bool OnGround { get; set; }
        public double Stance { get; set; }

        public Player(string username)
        {
            //TODO: Store username?

            using (NBTFile file = NBTFile.Open(MinecraftServer.Instance.Path + "Players/" + username + ".dat"))
            {
                Tag[] pos = (Tag[])file.FindPayload("Pos");
                Position = new PointDouble() { X = (double)pos[0].Payload, Y = (double)pos[1].Payload, Z = (double)pos[2].Payload };

                Tag[] rotation = (Tag[])file.FindPayload("Rotation");
                Rotation = new Rotation() { Yaw = (float)rotation[0].Payload, Pitch = (float)rotation[1].Payload };

                OnGround = BitConverter.ToBoolean(new byte[] { (byte)file.FindPayload("OnGround") }, 0);
            }
        }
    }
}
