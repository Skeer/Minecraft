using System;

namespace Minecraft.Items
{
    public class Item : MarshalByRefObject // Might need rename to InventoryItem
    {
        public short ID { get; set; }
        private short _Damage { get; set; } // Maybe a default?
        public short Damage
        {
            get { return _Damage; }
            set { _Damage = value; }
        }
        public byte Count { get; set; }
        public byte Slot { get; set; }
        public short Uses { get; set; }

        public Item()
        {        }

        public Item(Item reference)
        {
            ID = reference.ID;
            _Damage = reference._Damage;
            Count = reference.Count;
            Slot = reference.Slot;
            Uses = reference.Uses;
        }
    }
}
