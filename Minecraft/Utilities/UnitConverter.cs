using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minecraft.Utilities
{
    class UnitConverter
    {
        public static double FromBlockCoordToChunkCoord(double d)
        {
            return Math.Floor(d / 16);
        }

        public static double FromChunkCoordToRegionCoord(double d)
        {
            return Math.Floor(d / 32);
        }
    }
}
