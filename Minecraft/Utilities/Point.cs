using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minecraft.Utilities
{
    public struct Point<T, U, V>
    {
       public T X { get; set; }
       public U Y { get; set; }
       public V Z { get; set; }
    }
}
