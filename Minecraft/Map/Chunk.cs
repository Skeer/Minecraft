using System;
using System.Collections.Generic;
using Minecraft.Entities;
using NBTLibrary;
using Minecraft.Utilities;
using NBTLibrary.Tags;
using Minecraft.Net;

namespace Minecraft.Map
{
    public class Chunk
    {
        private Logger Log = new Logger(typeof(Chunk));
        private NBTFile Config;

        public byte TerrainPopulated
        {
            get { return (byte)Config["Level"]["TerrainPopulated"].Payload; }
            set { Config["Level"]["TerrainPopulated"].Payload = value; }
        }
        public byte[] BlockLight
        {
            get { return (byte[])Config["Level"]["BlockLight"].Payload; }
            set { Config["Level"]["BlockLight"].Payload = value; }
        }
        public byte[] Blocks
        {
            get { return (byte[])Config["Level"]["Blocks"].Payload; }
            set { Config["Level"]["Blocks"].Payload = value; }
        }
        public byte[] MetaData
        {
            get { return (byte[])Config["Level"]["Data"].Payload; }
            set { Config["Level"]["Data"].Payload = value; }
        }
        public byte[] HeightMap
        {
            get { return (byte[])Config["Level"]["HeightMap"].Payload; }
            set { Config["Level"]["HeightMap"].Payload = value; }
        }
        public byte[] SkyLight
        {
            get { return (byte[])Config["Level"]["SkyLight"].Payload; }
            set { Config["Level"]["SkyLight"].Payload = value; }
        }
        public long LastUpdate { get; set; }
        public List<Entity> Entities = new List<Entity>();
        public List<Entity> TileEntities = new List<Entity>();
        private RegionFile Region;
        public readonly Point<int, int, int> Location;

        public Chunk(RegionFile region, byte[] data)
        {
            Region = region;
            Load(data);

            //x, y positions
            Location = new Point<int, int, int>() { X = (int)Config["Level"]["xPos"].Payload, Z = (int)Config["Level"]["zPos"].Payload };

            List<Tag> entities = (List<Tag>)Config["Level"]["Entities"].Payload;
            foreach (Tag t in entities)
            {
                switch (Entity.GetTypeFromTag(t))
                {
                    case "Mob":
                        //var m = Mob.Load(MinecraftServer.Instance.Entity++, t);
                        //Entities.Add(m);
                        //MinecraftServer.Instance.Entities.Add(m.EID, m);
                        break;
                    case "Painting":
                        Painting p = Painting.Load(MinecraftServer.Instance.Entity++, t);
                        Entities.Add(p);
                        MinecraftServer.Instance.Entities.Add(p.EID, p);
                        break;
                    default:
                        break;
                }
            }

            List<Tag> tileEntities = (List<Tag>)Config["Level"]["TileEntities"].Payload;
            foreach (Tag t in tileEntities)
            {
                switch (Entity.GetTypeFromTag(t))
                {
                    case "Sign":
                        Sign s = Sign.Load(MinecraftServer.Instance.Entity++, t);
                        Entities.Add(s);
                        MinecraftServer.Instance.Entities.Add(s.EID, s);
                        break;
                    case "Chest":
                        break;
                    case "Furnace":
                        break;
                    case "MobSpawner":
                        break;
                    default:
                        break;
                }
            }

        }

        public void Load(byte[] data)
        {
            try
            {
                Config = NBTFile.Load(data);
            }
            catch
            {
                Log.Warning("Unable to load Chunk. Possibly need of generation.");
            }
        }

        public void Save()
        {
            Region.SetChunkData(Location.X, Location.Z, Config.GetBytes(), DateTime.Now.Ticks);
        }

        public override string ToString()
        {
            return "(" + Location.X + ", " + Location.Z + ")";
        }

        public byte GetBlockAt(int x, int y, int z)
        {
            return Blocks[GetIndexFromCoords(x, y, z)];
        }
        public byte GetMetaDataAt(int x, int y, int z)
        {
            return MetaData[GetIndexFromCoords(x, y, z) / 2];
        }

        public void SetBlockAt(int x, int y, int z, byte block)
        {
            Blocks[GetIndexFromCoords(x, y, z)] = block;
        }

        private int GetIndexFromCoords(int x, int y, int z)
        {
            int ix = x % 16;
            if (ix < 0)
            {
                ix += 16;
            }

            int iz = z % 16;
            if (iz < 0)
            {
                iz += 16;
            }
            return y + iz * 128 + ix * 2048;
        }
    }
}
