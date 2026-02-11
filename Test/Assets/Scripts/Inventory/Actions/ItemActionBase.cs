using UnityEngine;

namespace InventorySystem.Actions
{
    public abstract class ItemActionBase : ScriptableObject, IItemAction
    {
        public abstract void Execute(Core.InventoryManager inventoryManager, int slotIndex);
    }
}
