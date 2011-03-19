using System;
using System.Collections.Generic;
using Minecraft.Entities;
using NBTLibrary;
using Minecraft.Utilities;

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
        public int X { get; set; }
        /// <summary>
        /// In chunks (16 blocks).
        /// </summary>
        public int Z { get; set; }
        public long LastUpdate { get; set; }
        public List<Entity> Entities = new List<Entity>();
        public List<Entity> TileEntities = new List<Entity>();
        private RegionFile Region;

        public Chunk(RegionFile region, int x, int z, byte[] data)
        {
            X = x;
            Z = z;
            Region = region;
            Load(data);
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
    }
}
