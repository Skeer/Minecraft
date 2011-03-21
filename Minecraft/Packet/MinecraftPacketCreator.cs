using System.IO;
using System.Text;
using Minecraft.Map;
using Minecraft.Net;
using zlib;
using Minecraft.Items;
using System.Collections.Generic;

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

        public static byte[] GetLoginRequest(uint eid)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.LoginRequest);
                stream.WriteUint(eid);
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

        public static byte[] GetPositionLook(double x, double y, double z, float yaw, float pitch, bool onGround)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.PlayerPositionLook);
                stream.WriteDouble(x);
                stream.WriteDouble(y);
                stream.WriteDouble(y + 1.62); // dunno what 1.62 means...
                stream.WriteDouble(z);
                stream.WriteFloat(yaw);
                stream.WriteFloat(pitch);
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

        public static byte[] GetTimeUpdate(long time)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.TimeUpdate);
                stream.WriteLong(time);
                return stream.ToArray();
            }
        }

        private static short ConvertSlot(short slot)
        {
            if (slot < 9)
            {
                return (short)(slot + 36);
            }
            return slot;
        }

        public static byte[] GetSetSlot(byte window, short slot, short id, byte count, short uses)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.SetSlot);
                stream.WriteByte(window);
                stream.WriteShort(ConvertSlot(slot));
                stream.WriteShort(id);
                stream.WriteByte(count);
                stream.WriteShort(uses);
                return stream.ToArray();
            }
        }

        public static byte[] GetEntityEquipment(uint eid, short slot, short itemID, short damage)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.EntityEquipment);
                stream.WriteUint(eid);
                stream.WriteShort(slot);
                stream.WriteShort(itemID);
                stream.WriteShort(damage);
                return stream.ToArray();
            }
        }

        public static byte[] GetNamedEntitySpawn(uint eid, string username, int x, int y, int z, byte yaw, byte pitch, short currentItem)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.NamedEntitySpawn);
                stream.WriteUint(eid);
                stream.WriteString(username);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(z);
                stream.WriteByte(yaw);
                stream.WriteByte(pitch);
                stream.WriteShort(currentItem);
                return stream.ToArray();
            }
        }

        public static byte[] GetPlayerDigging(PlayerDiggingStatus status, int x, byte y, int z, byte face)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.PlayerDigging);
                stream.WriteByte((byte)status);
                stream.WriteInt(x);
                stream.WriteByte(y);
                stream.WriteInt(z);
                stream.WriteByte(face);
                return stream.ToArray();
            }
        }

        public static byte[] GetBlockChange(int x, byte y, int z, byte type, byte metaData)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.BlockChange);
                stream.WriteInt(x);
                stream.WriteByte(y);
                stream.WriteInt(z);
                stream.WriteByte(type);
                stream.WriteByte(metaData);
                return stream.ToArray();
            }
        }

        public static byte[] GetWindowItems(byte windowID, short count, List<Item> Inventory)
        {
            return null;
        }

        public static byte[] GetPickupSpawn(uint eid, short id, byte count, short metaData, int x, int y, int z, byte yaw, byte pitch, byte roll)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.PickupSpawn);
                stream.WriteUint(eid);
                stream.WriteShort(id);
                stream.WriteByte(count);
                stream.WriteShort(metaData);
                stream.WriteInt(x);
                stream.WriteInt(y);
                stream.WriteInt(z);
                stream.WriteByte(yaw);
                stream.WriteByte(pitch);
                stream.WriteByte(roll);
                return stream.ToArray();
            }
        }

        public static byte[] GetEntity(uint eid)
        {
            using (MinecraftPacketStream stream = new MinecraftPacketStream())
            {
                stream.WriteByte((byte)MinecraftOpcode.Entity);
                stream.WriteUint(eid);
                return stream.ToArray();
            }
        }
    }
}
