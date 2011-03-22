using System;
using NBTLibrary.Tags;

namespace Minecraft.Entities
{
    public abstract class Entity : MarshalByRefObject
    {
        public uint EID { get; set; }
        /// <summary>
        /// In blocks (32 px).
        /// </summary>
        public abstract double X { get; set; }
        /// <summary>
        /// In blocks (32 px).
        /// </summary>
        public abstract double Y { get; set; }
        /// <summary>
        /// In blocks (32 px).
        /// </summary>
        public abstract double Z { get; set; }
        public abstract float Yaw { get; set; }
        public abstract float Pitch { get; set; }


        // TODO: Height / Widths?
        // Refer to http://mc.kev009.com/Entities
        public static string GetTypeFromTag(Tag t)
        {
            switch ((string)t["id"].Payload)
            {
                case "Slime":
                case "Creeper":
                case "Pig":
                case "Squid":
                case "Zombie":
                case "Spider":
                case "Skeleton":
                case "Cow":
                    return "Mob";
                default:
                    return (string)t["id"].Payload;
            }
        }
    }
}
