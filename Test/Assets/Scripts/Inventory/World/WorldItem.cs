using InventorySystem.Core;
using InventorySystem.Data;
using UnityEngine;

namespace InventorySystem.World
{
    public class WorldItem : MonoBehaviour
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int quantity = 1;
        [SerializeField] private bool autoPickupOnTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (!autoPickupOnTrigger)
            {
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }

            TryPickup();
        }

        public void TryPickup()
        {
            if (item == null || quantity <= 0)
            {
                return;
            }

            var inventory = InventoryManager.Instance;
            if (inventory == null)
            {
                return;
            }

            int remaining = inventory.Inventory.AddItem(item, quantity);
            if (remaining <= 0)
            {
                if (item.PickupVfxPrefab != null)
                {
                    Instantiate(item.PickupVfxPrefab, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
            }
            else
            {
                quantity = remaining;
            }
        }
    }
}
