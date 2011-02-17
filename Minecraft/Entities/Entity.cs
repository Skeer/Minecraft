using System;

namespace Minecraft.Entities
{
   public class Entity : MarshalByRefObject
   {
       /// <summary>
       /// In blocks (32 px).
       /// </summary>
       public double X { get; set; }
       /// <summary>
       /// In blocks (32 px).
       /// </summary>
       public double Y { get; set; }
       /// <summary>
       /// In blocks (32 px).
       /// </summary>
       public double Z { get; set; }

        // TODO: Height / Widths?
        // Refer to http://mc.kev009.com/Entities

    }
}
