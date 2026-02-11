using System.Threading.Tasks;
using InventorySystem.Data;
using InventorySystem.Persistence;
using UnityEngine;

namespace InventorySystem.Core
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Data")]
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private int slotCount = 30;

        [Header("Persistence")]
        [SerializeField] private bool autoSaveOnChange = true;

        public Inventory Inventory { get; private set; }
        public ItemDatabase Database => itemDatabase;

        private IInventoryRepository _repository;
        private bool _isInitialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private async void Start()
        {
            await InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized)
            {
                return;
            }

            if (itemDatabase == null)
            {
                itemDatabase = Resources.Load<ItemDatabase>("Items/ItemDatabase");
            }

            if (itemDatabase != null)
            {
                itemDatabase.Initialize();
            }

            Inventory = new Inventory(slotCount);
            Inventory.OnItemAdded += HandleInventoryChanged;
            Inventory.OnItemRemoved += HandleInventoryChanged;
            Inventory.OnItemMoved += HandleInventoryChanged;
            Inventory.OnSlotUpdated += HandleSlotUpdated;

            _repository = InventoryRepositoryFactory.Create(itemDatabase);
            var loaded = await _repository.LoadAsync();
            if (loaded != null)
            {
                ApplyLoadedInventory(loaded);
            }

            _isInitialized = true;
        }

        public int AddItemById(string itemId, int quantity)
        {
            var item = itemDatabase != null ? itemDatabase.GetById(itemId) : null;
            if (item == null)
            {
                Debug.LogWarning($"[InventoryManager] Item not found: {itemId}");
                return quantity;
            }

            int remaining = Inventory.AddItem(item, quantity);
            return remaining;
        }

        public void UseItem(int slotIndex)
        {
            var slot = Inventory.GetSlot(slotIndex);
            if (slot == null || slot.IsEmpty)
            {
                return;
            }

            var action = slot.Item.UseAction;
            if (action != null)
            {
                action.Execute(this, slotIndex);
            }
        }

        public async Task SaveAsync()
        {
            if (_repository == null)
            {
                return;
            }

            await _repository.SaveAsync(Inventory);
        }

        public void SortByName()
        {
            Inventory?.SortByName();
        }

        public void SortByQuantity()
        {
            Inventory?.SortByQuantity();
        }

        public async Task DeleteAsync()
        {
            if (_repository == null)
            {
                return;
            }

            await _repository.DeleteAsync();
        }

        private void HandleInventoryChanged(InventorySystem.Data.ItemDefinition item, int qty)
        {
            if (autoSaveOnChange)
            {
                _ = SaveAsync();
            }
        }

        private void HandleInventoryChanged(int fromIndex, int toIndex)
        {
            if (autoSaveOnChange)
            {
                _ = SaveAsync();
            }
        }

        private void HandleSlotUpdated(int index)
        {
            if (autoSaveOnChange)
            {
                _ = SaveAsync();
            }
        }

        private void ApplyLoadedInventory(InventorySaveData loaded)
        {
            if (loaded == null || Inventory == null)
            {
                return;
            }

            Inventory.Clear();

            foreach (var slot in loaded.Slots)
            {
                if (slot == null || string.IsNullOrWhiteSpace(slot.ItemId))
                {
                    continue;
                }

                var item = itemDatabase != null ? itemDatabase.GetById(slot.ItemId) : null;
                if (item == null)
                {
                    continue;
                }

                int targetIndex = slot.SlotIndex;
                var invSlot = Inventory.GetSlot(targetIndex);
                if (invSlot == null)
                {
                    continue;
                }

                invSlot.Item = item;
                invSlot.Quantity = slot.Quantity;
            }
        }
    }
}
