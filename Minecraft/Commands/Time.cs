using System;
using Minecraft.Command;
using Minecraft.Net;
using Minecraft.Packet;
using Minecraft.Entities;

namespace Minecraft.Commands
{
    class Time : MarshalByRefObject, ICommand
    {
        enum Sections
        {
            Day = 0,
            Noon = 6000,
            Sunset = 12000,
            Night = 13800,
            MidNight = 18000,
            Sunrise = 22200
        }

        public MinecraftRank Rank
        {
            get { return MinecraftRank.Admin; }
        }

        public void Run(MinecraftServer server, MinecraftClient client, string[] args)
        {
            Sections s;
            int t;
            if (Enum.TryParse(args[1], true, out s))
            {
                int difference = (int)s - (int)(server.Time % 24000);
                if (difference < 0)
                {
                    difference += 24000;
                }
                server.Time += difference;

                foreach (Player p in server.Players.Values)
                {
                    p.Client.Send(MinecraftPacketCreator.GetTimeUpdate(server.Time));
                }
            }
            else if (int.TryParse(args[1], out t))
            {
                int difference = Math.Abs((int)t - (int)(server.Time % 24000));
                if (difference < 0)
                {
                    difference += 24000;
                }
                server.Time += difference;

                foreach (Player p in server.Players.Values)
                {
                    p.Client.Send(MinecraftPacketCreator.GetTimeUpdate(server.Time));
                }
            }
            //error output?
        }
    }
}
