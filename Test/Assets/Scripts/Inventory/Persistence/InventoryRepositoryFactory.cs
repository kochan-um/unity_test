using Supabase;
using UnityEngine;
using InventorySystem.Data;

namespace InventorySystem.Persistence
{
    public static class InventoryRepositoryFactory
    {
        public static IInventoryRepository Create(ItemDatabase database)
        {
            var fallback = new LocalInventoryRepository();

            var settings = Resources.Load<SupabaseSettings>("SupabaseSettings");
            if (settings == null || !settings.IsValid())
            {
                return fallback;
            }

            return new SupabaseInventoryRepository(settings, fallback);
        }
    }
}
