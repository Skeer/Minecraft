using System.IO.Compression;
using System.IO;
using System.Collections.Generic;
using System;

namespace NBTLibrary
{
    public class NBTFile : IDisposable
    {
        private NBTStream Stream = new NBTStream();
        public Tag Root { get; set; }
        private bool Disposed = false;
        public string Path { get; set; }

        public static NBTFile Open(string path)
        {
            NBTFile file = new NBTFile(path);
            return file;
        }

        public NBTFile()
        { }

        private NBTFile(string path)
        {
            Path = path;
            using (GZipStream gStream = new GZipStream(new FileStream(path, FileMode.Open), CompressionMode.Decompress))
            {
                gStream.CopyTo(Stream);
            }
            Stream.Position = 0;

            //start
            if (Stream.ReadTag() == TagType.Compound)
            {
                Root = new Tag();
                Root.Type = TagType.Compound;
                ParseTag(Root);
            }
        }

        public void Save()
        {
            try
            {
                Save(Path);
            }
            catch
            {
                throw new NotSupportedException();
            }
        }

        public void Save(string path)
        {
            Stream.SetLength(0);
            SaveTag(Root);
            Stream.Position = 0;
            using (GZipStream gStream = new GZipStream(new FileStream(path, FileMode.Create), CompressionMode.Compress))
            {
                Stream.CopyTo(gStream);
                gStream.Flush();
            }
        }

        private void SaveTag(Tag tag, bool IsList = false)
        {
            if (tag.Type != TagType.End)
            {
                if (!IsList)
                {
                    Stream.WriteTag(tag.Type);
                    Stream.WriteString(tag.Name);
                }
                switch (tag.Type)
                {
                    case TagType.Byte:
                        Stream.WriteByte((byte)tag.Payload);
                        break;
                    case TagType.ByteArray:
                        byte[] payload = (byte[])tag.Payload;
                        Stream.WriteInt(payload.Length);
                        foreach (byte b in payload)
                        {
                            Stream.WriteByte(b);
                        }
                        break;
                    case TagType.Compound:
                        List<Tag> pload = (List<Tag>)tag.Payload;
                        foreach (Tag t in pload)
                        {
                            SaveTag(t);
                        }
                        Stream.WriteTag(TagType.End);
                        break;
                    case TagType.Double:
                        Stream.WriteDouble((double)tag.Payload);
                        break;
                    case TagType.Float:
                        Stream.WriteFloat((float)tag.Payload);
                        break;
                    case TagType.Int:
                        Stream.WriteInt((int)tag.Payload);
                        break;
                    case TagType.List:
                        List<Tag> load = (List<Tag>)tag.Payload;
                        if (load.Count > 0)
                        {
                            Stream.WriteTag(load[0].Type);
                        }
                        else
                        {
                            // Dummy tag lol
                            Stream.WriteTag(TagType.Byte);
                        }
                        Stream.WriteInt(load.Count);
                        foreach (Tag t in load)
                        {
                            SaveTag(t, true);
                        }
                        break;
                    case TagType.Long:
                        Stream.WriteLong((long)tag.Payload);
                        break;
                    case TagType.Short:
                        Stream.WriteShort((short)tag.Payload);
                        break;
                    case TagType.String:
                        Stream.WriteString((string)tag.Payload);
                        break;
                }
            }
        }

        public Tag this[string index]
        {
            get
            {
                return Root[index];
            }
            set
            {
                Root[index] = value;
            }
        }

        private void ParseTag(Tag tag, bool IsList = false)
        {
            if (tag.Type != TagType.End && (byte)tag.Type != 0xff)
            {
                if (!IsList)
                {
                    tag.Name = Stream.ReadString();
                }
                switch (tag.Type)
                {
                    case TagType.Byte:
                        tag.Payload = Stream.ReadByte();
                        break;
                    case TagType.ByteArray:
                        int length = Stream.ReadInt();
                        byte[] payload = new byte[length];
                        for (int i = 0; i < length; ++i)
                        {
                            payload[i] = Stream.ReadByte();
                        }
                        tag.Payload = payload;
                        break;
                    case TagType.Compound:
                        List<Tag> pload = new List<Tag>();
                        while (true)
                        {
                            TagType tt = Stream.ReadTag();
                            if (tt == TagType.End || (byte)tt == 0xff)
                            {
                                break;
                            }
                            else
                            {
                                Tag t = new Tag();
                                t.Type = tt;
                                ParseTag(t);
                                pload.Add(t);
                            }
                        }
                        tag.Payload = pload;
                        break;
                    case TagType.Double:
                        tag.Payload = Stream.ReadDouble();
                        break;
                    case TagType.Float:
                        tag.Payload = Stream.ReadFloat();
                        break;
                    case TagType.Int:
                        tag.Payload = Stream.ReadInt();
                        break;
                    case TagType.List:
                        TagType type = Stream.ReadTag();
                        length = Stream.ReadInt();
                        List<Tag> load = new List<Tag>();
                        for (int i = 0; i < length; ++i)
                        {
                            Tag t = new Tag();
                            t.Type = type;
                            ParseTag(t, true);
                            load.Add(t);
                        }
                        tag.Payload = load;
                        break;
                    case TagType.Long:
                        tag.Payload = Stream.ReadLong();
                        break;
                    case TagType.Short:
                        tag.Payload = Stream.ReadShort();
                        break;
                    case TagType.String:
                        tag.Payload = Stream.ReadString();
                        break;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    if (Stream != null)
                    {
                        Stream.Dispose();
                    }
                }
                Stream = null;
                Disposed = true;
            }
        }

        public object FindPayload(string name, Tag tag = null, bool IsList = false)
        {
            if (tag == null)
            {
                tag = Root;
            }

            if (!IsList && tag.Name == name)
            {
                return tag.Payload;
            }

            if (tag.Type == TagType.Compound)
            {
                foreach (Tag t in (List<Tag>)tag.Payload)
                {
                    object p = FindPayload(name, t);

                    if (p != null)
                    {
                        return p;
                    }
                }
            }
            else if (tag.Type == TagType.List)
            {
                foreach (Tag t in (List<Tag>)tag.Payload)
                {
                    object p = FindPayload(name, t, true);

                    if (p != null)
                    {
                        return p;
                    }
                }
            }

            return null;
        }

        ~NBTFile()
        {
            Dispose(false);
        }

        public static NBTFile Load(byte[] data)
        {
            NBTFile file = new NBTFile(data);
            return file;
        }

        private NBTFile(byte[] data)
        {
            Stream.Write(data, 0, data.Length);
            Stream.Position = 0;

            //start
            if (Stream.ReadTag() == TagType.Compound)
            {
                Root = new Tag();
                Root.Type = TagType.Compound;
                ParseTag(Root);
            }
        }

        public byte[] GetBytes()
        {
            Stream.SetLength(0);
            SaveTag(Root);
            return Stream.ToArray();
        }
    }
}
