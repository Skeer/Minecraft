using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minecraft.Items
{
    class ItemRegistry
    {
        public static short GetDrop(short id, short holding = 0)
        {
            Random r = new Random();
            switch (id)
            {
                case 1:
                    if (holding == 271 || holding == 275 || holding == 258 || holding == 286 || holding == 279)
                    {
                        return 4;
                    }
                    return 0;
                case 2:
                    return 3;
                case 4:
                    if (holding == 271 || holding == 275 || holding == 258 || holding == 286 || holding == 279)
                    {
                        return id;
                    }
                    return 0;
                case 13:
                    if (r.Next(21) <= 3)
                    {
                        return 318;
                    }
                    return id;
                case 14:
                    if (holding == 257 || holding == 278)
                    {
                        return id;
                    }
                    return 0;
                case 15:
                    if (holding == 274 || holding == 257 || holding == 278)
                    {
                        return id;
                    }
                    return 0;
                case 16:
                    if (holding == 271 || holding == 275 || holding == 258 || holding == 286 || holding == 279)
                    {
                        return 263;
                    }
                    return 0;
                case 18:
                   // Random r = new Random();
                    if (r.Next(11) <= 1)
                    {
                        return 6;
                    }
                    return 0;
                case 20:
                    return 0;
                case 21:

                default:
                    return id;
            }
        }
    }
}
