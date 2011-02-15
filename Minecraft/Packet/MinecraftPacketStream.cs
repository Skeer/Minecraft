using System;
using System.IO;
using System.Text;

namespace Minecraft.Packet
{
    public class MinecraftPacketStream : MemoryStream // Should I replace this with BinaryReader / BinaryWriter?
    {
        private void ReverseBytes(byte[] buffer)
        {
            int index = buffer.Length - 1;
            int limit = buffer.Length / 2;
            for (int i = 0; i < limit; ++i)
            {
                byte t = buffer[i];
                buffer[i] = buffer[index - i];
                buffer[index - i] = t;
            }
        }

        public int ReadInt()
        {
            byte[] buffer = new byte[4];
            Read(buffer, 0, buffer.Length);
            ReverseBytes(buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        public short ReadShort()
        {
            byte[] buffer = new byte[2];
            Read(buffer, 0, buffer.Length);
            ReverseBytes(buffer);
            return BitConverter.ToInt16(buffer, 0);
        }

        public new byte ReadByte()
        {
            return (byte)base.ReadByte();
        }

        public long ReadLong()
        {
            byte[] buffer = new byte[8];
            Read(buffer, 0, buffer.Length);
            ReverseBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        public double ReadDouble()
        {
            byte[] buffer = new byte[8];
            Read(buffer, 0, buffer.Length);
            ReverseBytes(buffer);
            return BitConverter.ToDouble(buffer, 0);
        }

        public float ReadFloat()
        {
            byte[] buffer = new byte[4];
            Read(buffer, 0, buffer.Length);
            ReverseBytes(buffer);
            return BitConverter.ToSingle(buffer, 0);
        }

        public bool ReadBool()
        {
            byte[] buffer = new byte[1];
            Read(buffer, 0, buffer.Length);
            ReverseBytes(buffer);
            return BitConverter.ToBoolean(buffer, 0);
        }

        public string ReadString(short length)
        {
            byte[] buffer = new byte[length];
            Read(buffer, 0, buffer.Length);
            return UTF8Encoding.UTF8.GetString(buffer);
        }

        public void WriteShort(short s)
        {
            byte[] buffer = BitConverter.GetBytes(s);
            ReverseBytes(buffer);
            Write(buffer, 0, buffer.Length);
        }

        public void WriteString(string s)
        {
            byte[] buffer = UTF8Encoding.UTF8.GetBytes(s);
            WriteShort((short)buffer.Length);
            Write(buffer, 0, buffer.Length);
        }

        public void WriteUint(uint u)
        {
            byte[] buffer = BitConverter.GetBytes(u);
            ReverseBytes(buffer);
            Write(buffer, 0, buffer.Length);
        }

        public void WriteLong(long l)
        {
            byte[] buffer = BitConverter.GetBytes(l);
            ReverseBytes(buffer);
            Write(buffer, 0, buffer.Length);
        }

        public void WriteInt(int i)
        {
            byte[] buffer = BitConverter.GetBytes(i);
            ReverseBytes(buffer);
            Write(buffer, 0, buffer.Length);
        }

        public void WriteBool(bool b)
        {
            byte[] buffer = BitConverter.GetBytes(b);
            ReverseBytes(buffer); // LOL
            Write(buffer, 0, buffer.Length);
        }

        public void WriteDouble(double d)
        {
            byte[] buffer = BitConverter.GetBytes(d);
            ReverseBytes(buffer);
            Write(buffer, 0, buffer.Length);
        }

        public void WriteFloat(float f)
        {
            byte[] buffer = BitConverter.GetBytes(f);
            ReverseBytes(buffer);
            Write(buffer, 0, buffer.Length);
        }

    }
}
