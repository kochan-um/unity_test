using UnityEngine;

namespace InventorySystem.Actions
{
    [CreateAssetMenu(fileName = "ConsumeAction", menuName = "Inventory/Actions/Consume")]
    public class ConsumeAction : ItemActionBase
    {
        [SerializeField] private string logMessage = "Consumed item.";

        public override void Execute(Core.InventoryManager inventoryManager, int slotIndex)
        {
            if (inventoryManager == null)
            {
                return;
            }

            inventoryManager.Inventory.RemoveAt(slotIndex, 1);
            Debug.Log($"[ConsumeAction] {logMessage}");
        }
    }
}
