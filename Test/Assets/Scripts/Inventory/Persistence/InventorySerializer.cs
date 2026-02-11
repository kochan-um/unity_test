using InventorySystem.Core;

namespace InventorySystem.Persistence
{
    public static class InventorySerializer
    {
        public static InventorySaveData ToSaveData(Inventory inventory)
        {
            var data = new InventorySaveData();
            if (inventory == null)
            {
                return data;
            }

            for (int i = 0; i < inventory.Slots.Count; i++)
            {
                var slot = inventory.Slots[i];
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                data.Slots.Add(new InventorySlotData
                {
                    SlotIndex = i,
                    ItemId = slot.Item.Id,
                    Quantity = slot.Quantity
                });
            }

            return data;
        }
    }
}
