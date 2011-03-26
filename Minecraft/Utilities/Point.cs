using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minecraft.Utilities
{
    public struct Point<T, U, V> : IEquatable<Point<T, U, V>>
    {
       public T X { get; set; }
       public U Y { get; set; }
       public V Z { get; set; }

       public bool Equals(Point<T, U, V> other)
       {
           return (X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z));
       }

       public override int GetHashCode()
       {
           return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
       } 
    }
}
