using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.Data
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemDefinition> items = new List<ItemDefinition>();

        private readonly Dictionary<string, ItemDefinition> _byId = new Dictionary<string, ItemDefinition>();
        private bool _initialized;

        public IReadOnlyList<ItemDefinition> Items => items;

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _byId.Clear();
            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    Debug.LogWarning($"[ItemDatabase] Item '{item.name}' has empty ID.");
                    continue;
                }

                if (_byId.ContainsKey(item.Id))
                {
                    Debug.LogError($"[ItemDatabase] Duplicate ID detected: {item.Id}");
                    continue;
                }

                _byId[item.Id] = item;
            }

            _initialized = true;
        }

        public ItemDefinition GetById(string id)
        {
            if (!_initialized)
            {
                Initialize();
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            _byId.TryGetValue(id, out var item);
            return item;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _initialized = false;
        }
#endif
    }
}
