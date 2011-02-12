﻿using System.IO.Compression;
using System.IO;
using System.Collections.Generic;
using System;

namespace NBTLibrary
{
    public class NBTFile : IDisposable
    {
        private NBTStream Stream = new NBTStream();
        public Tag File;

        public static NBTFile Open(string path)
        {
            NBTFile file = new NBTFile(path);
            //parse
            return file;
        }

        private NBTFile(string path)
        {
            using (GZipStream gStream = new GZipStream(new FileStream(path, FileMode.Open), CompressionMode.Decompress))
            {
                gStream.CopyTo(Stream);
            }
            Stream.Position = 0;

            //start
            if (Stream.ReadTag() == TagType.Compound)
            {
                File = new Tag();
                File.Type = TagType.Compound;
                ParseTag(File);
            }
        }

        private void ParseTag(Tag tag, bool IsList = false)
        {
            if (tag.Type != TagType.End || (byte)tag.Type == 0xff)
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
                        Tag[] load = new Tag[length];
                        for (int i = 0; i < length; ++i)
                        {
                            load[i] = new Tag();
                            load[i].Type = type;
                            ParseTag(load[i], true);
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

        public void Save(string path)
        {
        }

        public void Dispose()
        {
            Stream.Dispose();
        }

        public object FindPayload(string name, Tag tag = null, bool IsList = false)
        {
            if (tag == null)
            {
                tag = File;
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
                foreach (Tag t in (Tag[])tag.Payload)
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
    }
}