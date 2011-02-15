using System.IO;
using System.Text;
using Minecraft.Entities;
using Minecraft.Map;
using Minecraft.Net;
using zlib;

namespace Minecraft.Packet
{
    public class MinecraftPacketCreator
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

        public static byte[] GetPreChunk(int x, int z, bool initialize)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.PreChunk);
                stream.WriteInt(x);
                stream.WriteInt(z);
                stream.WriteBool(initialize);
                return stream.ToArray();
            }
        }

        public static byte[] GetMapChunk(Chunk c)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.MapChunk);
                stream.WriteInt(c.X * 16);
                stream.WriteShort(0);
                stream.WriteInt(c.Z * 16);
                stream.WriteByte(15);
                stream.WriteByte(127);
                stream.WriteByte(15);

                using (MemoryStream mStream = new MemoryStream())
                {
                    using (ZOutputStream zStream = new ZOutputStream(mStream, zlibConst.Z_BEST_COMPRESSION))
                    {
                        zStream.Write(c.Blocks, 0, c.Blocks.Length);
                        zStream.Write(c.Data, 0, c.Data.Length);
                        zStream.Write(c.BlockLight, 0, c.BlockLight.Length);
                        zStream.Write(c.SkyLight, 0, c.SkyLight.Length);
                    }
                    byte[] data = mStream.ToArray();
                    stream.WriteInt(data.Length);
                    stream.Write(data, 0, data.Length);
                }
                return stream.ToArray();
            }
        }

        public static byte[] GetSpawnPosition(int x, int y, int z)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.SpawnPosition);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(z);
                return stream.ToArray();
            }
        }

        public static byte[] GetPositionLook(double x, double y, double z, Rotation rotation, bool onGround)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.PlayerPositionLook);
                stream.WriteDouble(x);
                stream.WriteDouble(y);
                stream.WriteDouble(y + 1.62); // dunno what 1.62 means...
                stream.WriteDouble(z);
                stream.WriteFloat(rotation.Yaw);
                stream.WriteFloat(rotation.Pitch);
                stream.WriteBool(onGround);
                return stream.ToArray();
            }
        }

        public static byte[] GetChatMessage(string message)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.ChatMessage);
                stream.WriteString(message);
                return stream.ToArray();
            }
        }

        public static byte[] GetChatMessage(string username, string message)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.ChatMessage);
                StringBuilder builder = new StringBuilder();
                builder.Append("<");
                builder.Append(username);
                builder.Append("> ");
                builder.Append(message);
                stream.WriteString(builder.ToString());
                return stream.ToArray();
            }
        }
    }
}
