using System.IO;
using Minecraft.Entities;
using Minecraft.Map;
using Minecraft.Net;
using zlib;

namespace Minecraft.Packet
{
    class MinecraftPacketCreator
    {
        public static byte[] GetKeepAlive()
        {
            return new byte[] { (byte)MinecraftOpcode.KeepAlive };
        }

        public static byte[] GetDisconnect(string message)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.Disconnect);
                stream.WriteString(message);
                return stream.ToArray();
            }
        }

        public static byte[] GetHandshake(string hash)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.Handshake);
                stream.WriteString(hash);
                return stream.ToArray();
            }
        }

        public static byte[] GetLoginRequest()
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.LoginRequest);
                stream.WriteUint(MinecraftServer.Instance.Entity++);
                stream.WriteString("");
                stream.WriteString("");
                stream.WriteLong(MinecraftServer.Instance.RandomSeed);
                stream.WriteByte(MinecraftServer.Instance.Dimension);
                return stream.ToArray();
            }
        }

        public static byte[] GetPreChunk(Chunk c)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.PreChunk);
                stream.WriteInt(c.Position.X);
                stream.WriteInt(c.Position.Z);
                stream.WriteBool(true);
                return stream.ToArray();
            }
        }

        public static byte[] GetMapChunk(Chunk c)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.MapChunk);
                stream.WriteInt(c.Position.X);
                stream.WriteShort(c.Position.Y);
                stream.WriteInt(c.Position.Z);
                stream.WriteByte(15);
                stream.WriteByte(127);
                stream.WriteByte(15);

                using (MemoryStream mStream = new MemoryStream())
                {
                    using (ZOutputStream zStream = new ZOutputStream(mStream, zlibConst.Z_BEST_COMPRESSION))
                    {
                        zStream.Write(c.Data, 0, c.Data.Length);
                        stream.WriteInt((int)mStream.Length);
                        mStream.Position = 0;
                        mStream.CopyTo(stream);
                    }
                }
                return stream.ToArray();
            }
        }

        public static byte[] GetSpawnPosition(PointInt position)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.SpawnPosition);
                stream.WriteInt(position.X);
                stream.WriteInt(position.Y);
                stream.WriteInt(position.Z);
                return stream.ToArray();
            }
        }

        public static byte[] GetPositionLook(PointDouble position, Rotation rotation, bool onGround)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.PlayerPositionLook);
                stream.WriteDouble(position.X);
                stream.WriteDouble(position.Y);
                stream.WriteDouble(position.Y + 1.62); // dunno what 1.62 means...
                stream.WriteDouble(position.Z);
                stream.WriteFloat(rotation.Yaw);
                stream.WriteFloat(rotation.Pitch);
                stream.WriteBool(onGround);
                return stream.ToArray();
            }
        }
    }
}
