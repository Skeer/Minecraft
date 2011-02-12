using Minecraft.Net;
using System;

namespace Minecraft
{
    class Program
    {
        public static void Main(string[] args)
        {
            MinecraftServer.Instance.Run();
            Console.ReadLine();
        }
    }
}
