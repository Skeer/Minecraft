using System;
using System.Collections.Generic;
using Minecraft.Entities;
using NBTLibrary;

namespace Minecraft.Map
{
    public class Chunk
    {
        public byte TerrainPopulated { get; set; }
        public byte[] BlockLight { get; set; }
        public byte[] Blocks { get; set; }
        public byte[] Data { get; set; }
        public byte[] HeightMap { get; set; }
        public byte[] SkyLight { get; set; }
        /// <summary>
        /// In chunks (16 blocks).
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// In chunks (16 blocks).
        /// </summary>
        public int Z { get; set; }
        public long LastUpdate { get; set; }
        public string Path { get; set; }
        public List<Entity> Entities = new List<Entity>();
        public List<Entity> TileEntities = new List<Entity>();

        public Chunk(string path)
        {
            Path = path;

            Reload();
        }

        private void Reload()
        {
            Entities.Clear();
            TileEntities.Clear();
            using (NBTFile file = NBTFile.Open(Path))
            {
                Data = (byte[])file.FindPayload("Data");

                Tag[] entities = (Tag[])file.FindPayload("Entities");
                foreach (Tag t in entities)
                {
                    //TODO: Parse Entities
                    Entities.Add(new Entity());
                }

                LastUpdate = (long)file.FindPayload("LastUpdate");
                X = (int)file.FindPayload("xPos");
                Z = (int)file.FindPayload("zPos");

                Tag[] tileEntities = (Tag[])file.FindPayload("TileEntities");
                foreach (Tag t in tileEntities)
                {
                    //TODO: Parse more entities
                    TileEntities.Add(new Entity());
                }

                TerrainPopulated = (byte)file.FindPayload("TerrainPopulated");
                SkyLight = (byte[])file.FindPayload("SkyLight");
                HeightMap = (byte[])file.FindPayload("HeightMap");
                BlockLight = (byte[])file.FindPayload("BlockLight");
                Blocks = (byte[])file.FindPayload("Blocks");
            }
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "(" + X + ", " + Z + ")";
        }
    }
}
