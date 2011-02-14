using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBTLibrary;
using Minecraft.Entities;

namespace Minecraft.Map
{
    class Chunk
    {
        public string Path { get; set; }

        #region NBT Data
        public byte[] Data { get; set; }
        public List<IEntity> Entities { get; set; }
        public long LastUpdate { get; set; }
        public PointInt Position { get; set; }
        public List<IEntity> TileEntities { get; set; }
        public byte TerrainPopulated { get; set; }
        public byte[] SkyLight { get; set; }
        public byte[] HeightMap { get; set; }
        public byte[] BlockLight { get; set; }
        public byte[] Blocks { get; set; }
        #endregion

        public Chunk(string path)
        {
            Path = path;

            using (NBTFile file = NBTFile.Open(Path))
            {
                Data = (byte[])file.FindPayload("Data");
                //TODO: Parse Entities
                //Entities = (List<IEntity>)file.FindPayload("Entities");
                LastUpdate = (long)file.FindPayload("LastUpdate");
                Position = new PointInt() { X = (int) file.FindPayload("xPos"), Z = (int) file.FindPayload("zPos")};
                //TODO: Parse more entities
                //TileEntities = (List<IEntity>)file.FindPayload("TileEntities");
                TerrainPopulated = (byte)file.FindPayload("TerrainPopulated");
                SkyLight = (byte[])file.FindPayload("SkyLight");
                HeightMap = (byte[])file.FindPayload("HeightMap");
                BlockLight = (byte[])file.FindPayload("BlockLight");
                Blocks = (byte[])file.FindPayload("Blocks");
            }
        }

        public void Save()
        {
            // TODO: create the NBT File...
            NBTFile file = null;

            file.Save(Path);
        }
    }
}
