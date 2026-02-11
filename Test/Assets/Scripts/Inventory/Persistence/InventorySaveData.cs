using System;
using System.Collections.Generic;

namespace InventorySystem.Persistence
{
    [Serializable]
    public class InventorySaveData
    {
        public List<InventorySlotData> Slots = new List<InventorySlotData>();
    }

    [Serializable]
    public class InventorySlotData
    {
        public int SlotIndex;
        public string ItemId;
        public int Quantity;
    }
}
