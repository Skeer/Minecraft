using System;

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

    }
}
