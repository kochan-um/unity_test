using System.IO;
using System.Threading.Tasks;
using InventorySystem.Core;
using UnityEngine;

namespace InventorySystem.Persistence
{
    public class LocalInventoryRepository : IInventoryRepository
    {
        private readonly string _filePath;

        public LocalInventoryRepository(string fileName = "inventory.json")
        {
            _filePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        public Task SaveAsync(Inventory inventory)
        {
            if (inventory == null)
            {
                return Task.CompletedTask;
            }

            var data = InventorySerializer.ToSaveData(inventory);
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_filePath, json);
            return Task.CompletedTask;
        }

        public Task<InventorySaveData> LoadAsync()
        {
            if (!File.Exists(_filePath))
            {
                return Task.FromResult<InventorySaveData>(null);
            }

            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return Task.FromResult<InventorySaveData>(null);
            }

            var data = JsonUtility.FromJson<InventorySaveData>(json);
            return Task.FromResult(data);
        }

        public Task DeleteAsync()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }

            return Task.CompletedTask;
        }
    }
}
