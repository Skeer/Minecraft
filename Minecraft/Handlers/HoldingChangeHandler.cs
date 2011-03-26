using System.Linq;
using Minecraft.Entities;
using Minecraft.Items;
using Minecraft.Net;
using Minecraft.Packet;

namespace Minecraft.Handlers
{
    class HoldingChangeHandler : IPacketHandler
    {
        public bool HandlePacket(MinecraftClient client, MinecraftPacketStream stream)
        {
            if (stream.Length - stream.Position >= 2)
            {
                client.Player.HoldingSlot = (byte)stream.ReadShort();
                byte key = (byte)(client.Player.HoldingSlot + 36);
                if (client.Player.Inventory.ContainsKey(key))
                {
                    Item i = client.Player.Inventory[key];

                    foreach (Player p in from p in MinecraftServer.Instance.Players.Values
                                         where p != client.Player && p.IsInRange(client.Player.CurrentChunk.Location.X, client.Player.CurrentChunk.Location.Z)
                                         select p)
                    {
                        p.Client.Send(MinecraftPacketCreator.GetEntityEquipment(client.Player.EID, 0, i.ID, i.Damage));

                    }
                }
                else
                {
                    foreach (Player p in from p in MinecraftServer.Instance.Players.Values
                                         where p != client.Player && p.IsInRange(client.Player.CurrentChunk.Location.X, client.Player.CurrentChunk.Location.Z)
                                         select p)
                    {
                        p.Client.Send(MinecraftPacketCreator.GetEntityEquipment(client.Player.EID, 0, -1, 0));
                    }

                }
                return true;
            }
            return false;
        }
    }
}
