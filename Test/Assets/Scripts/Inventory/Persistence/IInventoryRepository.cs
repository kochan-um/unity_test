using System.Threading.Tasks;
using InventorySystem.Core;

namespace InventorySystem.Persistence
{
    public interface IInventoryRepository
    {
        Task SaveAsync(Inventory inventory);
        Task<InventorySaveData> LoadAsync();
        Task DeleteAsync();
    }
}
