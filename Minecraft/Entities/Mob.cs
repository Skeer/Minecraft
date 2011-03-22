using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBTLibrary.Tags;

namespace Minecraft.Entities
{
    abstract class Mob : Entity
    {
        public static Mob Load(uint eid, Tag data)
        {
            switch ((string)data["id"].Payload)
            {
                case "Creeper":
                    return Creeper.Load(eid, data);
                case "Slime":
                    return Slime.Load(eid, data);
            }
            return null;
        }

        public abstract void Drop();

        public abstract override double X { get; set; }

        public abstract override double Y { get; set; }

        public abstract override double Z { get; set; }
    }
}
