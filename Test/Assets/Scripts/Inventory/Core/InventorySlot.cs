using InventorySystem.Data;

namespace InventorySystem.Core
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemDefinition Item;
        public int Quantity;

        public bool IsEmpty => Item == null || Quantity <= 0;

        public void Clear()
        {
            Item = null;
            Quantity = 0;
        }

        public bool CanStackWith(ItemDefinition item)
        {
            return Item != null && item != null && Item.Id == item.Id;
        }

        public int RemainingCapacity()
        {
            if (Item == null)
            {
                return 0;
            }

            return Item.MaxStackSize - Quantity;
        }
    }
}
