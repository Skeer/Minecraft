namespace Minecraft.Items
{
    public class Item // Might need rename to InventoryItem
    {
        public short ID { get; set; }
        public short Damage { get; set; } // Maybe a default?
        public byte Count { get; set; }
        public byte Slot { get; set; }
    }
}
