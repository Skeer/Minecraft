﻿using System;
using System.Collections.Generic;
using Minecraft.Entities;
using NBTLibrary;
using Minecraft.Utilities;
using NBTLibrary.Tags;

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
        public byte[] Data
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
        /// <summary>
        /// In chunks (16 blocks).
        /// </summary>
        public int X
        {
            get { return (int)Config["Level"]["xPos"].Payload; }
            set { Config["Level"]["xPos"].Payload = value; } 
        }

        /// <summary>
        /// In chunks (16 blocks).
        /// </summary>
        public int Z
        {
            get { return (int)Config["Level"]["zPos"].Payload; }
            set { Config["Level"]["zPos"].Payload = value; }
        }
        public long LastUpdate { get; set; }
        public List<Entity> Entities = new List<Entity>();
        public List<Entity> TileEntities = new List<Entity>();
        private RegionFile Region;

        public Chunk(RegionFile region, byte[] data)
        {
            Region = region;
            Load(data);

            List<Tag> entities = (List<Tag>)Config["Level"]["Entities"].Payload;
            foreach (Tag t in entities)
            {
                Mob.Load(t);
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
            Region.SetChunkData(X, Z, Config.GetBytes(), DateTime.Now.Ticks);
        }

        public override string ToString()
        {
            return "(" + X + ", " + Z + ")";
        }

        public byte GetBlockAt(int x, int y, int z)
        {
            return Blocks[GetIndexFromCoords(x, y, z)]; 
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
