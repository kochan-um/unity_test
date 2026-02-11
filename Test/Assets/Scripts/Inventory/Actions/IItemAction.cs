namespace InventorySystem.Actions
{
    public interface IItemAction
    {
        void Execute(Core.InventoryManager inventoryManager, int slotIndex);
    }
}
