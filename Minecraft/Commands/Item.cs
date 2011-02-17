using System;
using Minecraft.Command;
using Minecraft.Net;
using Minecraft.Packet;

namespace Minecraft.Commands
{
    class Item : MarshalByRefObject, ICommand
    {
        public void Run(MinecraftServer server, MinecraftClient client, string[] args)
        {
            short id;
            if (args.Length < 2 || !short.TryParse(args[1], out id))
            {
                client.Send(MinecraftPacketCreator.GetChatMessage("Invalid argument provided."));
                return;
            }


            int count;
            if (args.Length < 3 || !int.TryParse(args[2], out count))
            {
                count = 1;
            }


            // Syntax: /Item <id> [count = 1]
            //         <id> = int or string (to be implemented later)
            if (client.Player.Inventory.Count >= 36)
            {
                //Spawn item as if dropped
            }
            else
            {
                byte b = 36;
                do
                {
                    if (b >= 45)
                    {
                        b = 9;
                    }

                    if (!client.Player.Inventory.ContainsKey(b))
                    {
                        Items.Item i = new Items.Item();
                        i.Count = (byte)count;
                        i.ID = id;
                        i.Slot = b;
                        i.Uses = 0;
                        client.Player.AddInventoryItem(i);
                        client.Player.Save();
                        client.Send(MinecraftPacketCreator.GetSetSlot(0, i.Slot, i.ID, i.Count, i.Uses));
                        break;
                    }

                    ++b;
                }
                while (b != 36);
            }
        }

        public MinecraftRank Rank
        {
            get { return MinecraftRank.Admin; }
        }
    }
}
