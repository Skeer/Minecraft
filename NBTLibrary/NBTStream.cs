using System.IO;
using System;
using System.Text;

namespace NBTLibrary
{
    class NBTStream : MemoryStream
    {
        internal new byte ReadByte()
        {
            return (byte)base.ReadByte();
        }

        internal TagType ReadTag()
        {
            return (TagType)ReadByte();
        }

        internal void ReverseBytes(byte[] buffer)
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

        internal short ReadShort()
        {
            byte[] buffer = new byte[2];
            Read(buffer, 0, buffer.Length);
            ReverseBytes(buffer);
            return BitConverter.ToInt16(buffer, 0);
        }

        internal string ReadString()
        {
            byte[] buffer = new byte[ReadShort()];
            Read(buffer, 0, buffer.Length);
            return UTF8Encoding.UTF8.GetString(buffer);
        }

        internal int ReadInt()
        {
            byte[] buffer = new byte[4];
            Read(buffer, 0, buffer.Length);
            ReverseBytes(buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        internal double ReadDouble()
        {
            byte[] buffer = new byte[8];
            Read(buffer, 0, buffer.Length);
            ReverseBytes(buffer);
            return BitConverter.ToDouble(buffer, 0);
        }

        internal float ReadFloat()
        {
            byte[] buffer = new byte[4];
            Read(buffer, 0, buffer.Length);
            ReverseBytes(buffer);
            return BitConverter.ToSingle(buffer, 0);
        }

        internal long ReadLong()
        {
            byte[] buffer = new byte[8];
            Read(buffer, 0, buffer.Length);
            ReverseBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        internal void WriteString(string s)
        {
            byte[] buffer = UTF8Encoding.UTF8.GetBytes(s);
            WriteShort((short)buffer.Length);
            Write(buffer, 0, buffer.Length);
        }

        internal void WriteShort(short s)
        {
            byte[] buffer = BitConverter.GetBytes(s);
            ReverseBytes(buffer);
            Write(buffer, 0, buffer.Length);
        }

        internal void WriteInt(int i)
        {
            byte[] buffer = BitConverter.GetBytes(i);
            ReverseBytes(buffer);
            Write(buffer, 0, buffer.Length);
        }

        internal void WriteTag(TagType type)
        {
            WriteByte((byte)type);
        }

        internal void WriteDouble(double d)
        {
            byte[] buffer = BitConverter.GetBytes(d);
            ReverseBytes(buffer);
            Write(buffer, 0, buffer.Length);
        }

        internal void WriteFloat(float f)
        {
            byte[] buffer = BitConverter.GetBytes(f);
            ReverseBytes(buffer);
            Write(buffer, 0, buffer.Length);
        }

        internal void WriteLong(long l)
        {
            byte[] buffer = BitConverter.GetBytes(l);
            ReverseBytes(buffer);
            Write(buffer, 0, buffer.Length);
        }
    }
}
