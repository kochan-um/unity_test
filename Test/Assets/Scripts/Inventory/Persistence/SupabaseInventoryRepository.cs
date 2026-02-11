using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using InventorySystem.Core;
using Supabase;
using UnityEngine;
using UnityEngine.Networking;

namespace InventorySystem.Persistence
{
    public class SupabaseInventoryRepository : IInventoryRepository
    {
        private const string TableName = "inventory_items";
        private readonly SupabaseSettings _settings;
        private readonly LocalInventoryRepository _fallback;

        public SupabaseInventoryRepository(SupabaseSettings settings, LocalInventoryRepository fallback)
        {
            _settings = settings;
            _fallback = fallback;
        }

        public async Task SaveAsync(Inventory inventory)
        {
            if (_settings == null || !_settings.IsValid())
            {
                await _fallback.SaveAsync(inventory);
                return;
            }

            var manager = SupabaseManager.Instance;
            if (manager == null || !manager.IsInitialized || !manager.Auth.IsAuthenticated)
            {
                await _fallback.SaveAsync(inventory);
                return;
            }

            var userId = manager.Auth.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                await _fallback.SaveAsync(inventory);
                return;
            }

            var saveData = InventorySerializer.ToSaveData(inventory);
            await DeleteRemote(userId);
            await InsertRemote(userId, saveData);
        }

        public async Task<InventorySaveData> LoadAsync()
        {
            if (_settings == null || !_settings.IsValid())
            {
                return await _fallback.LoadAsync();
            }

            var manager = SupabaseManager.Instance;
            if (manager == null || !manager.IsInitialized || !manager.Auth.IsAuthenticated)
            {
                return await _fallback.LoadAsync();
            }

            var userId = manager.Auth.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return await _fallback.LoadAsync();
            }

            var url = BuildUrl($"{TableName}?user_id=eq.{userId}&select=slot_index,item_id,quantity");
            var request = UnityWebRequest.Get(url);
            AddHeaders(request);

            var response = await SendRequest(request);
            if (!response.Success)
            {
                return await _fallback.LoadAsync();
            }

            var data = new InventorySaveData();
            try
            {
                var rows = JsonUtility.FromJson<InventoryRowList>(WrapJsonArray(response.Body));
                if (rows != null && rows.items != null)
                {
                    foreach (var row in rows.items)
                    {
                        data.Slots.Add(new InventorySlotData
                        {
                            SlotIndex = row.slot_index,
                            ItemId = row.item_id,
                            Quantity = row.quantity
                        });
                    }
                }
            }
            catch
            {
                return await _fallback.LoadAsync();
            }

            return data;
        }

        public async Task DeleteAsync()
        {
            if (_settings == null || !_settings.IsValid())
            {
                await _fallback.DeleteAsync();
                return;
            }

            var manager = SupabaseManager.Instance;
            if (manager == null || !manager.IsInitialized || !manager.Auth.IsAuthenticated)
            {
                await _fallback.DeleteAsync();
                return;
            }

            var userId = manager.Auth.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                await _fallback.DeleteAsync();
                return;
            }

            await DeleteRemote(userId);
        }

        private async Task DeleteRemote(string userId)
        {
            var url = BuildUrl($"{TableName}?user_id=eq.{userId}");
            var request = UnityWebRequest.Delete(url);
            AddHeaders(request);
            await SendRequest(request);
        }

        private async Task InsertRemote(string userId, InventorySaveData data)
        {
            if (data == null)
            {
                return;
            }

            var rows = new List<InventoryRow>();
            foreach (var slot in data.Slots)
            {
                rows.Add(new InventoryRow
                {
                    user_id = userId,
                    slot_index = slot.SlotIndex,
                    item_id = slot.ItemId,
                    quantity = slot.Quantity
                });
            }

            if (rows.Count == 0)
            {
                return;
            }

            var payload = JsonUtility.ToJson(new InventoryRowList { items = rows });
            var url = BuildUrl(TableName);
            var request = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");
            AddHeaders(request);
            await SendRequest(request);
        }

        private string BuildUrl(string path)
        {
            return _settings.SupabaseUrl.TrimEnd('/') + "/rest/v1/" + path;
        }

        private void AddHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("apikey", _settings.AnonKey);
            request.SetRequestHeader("Authorization", "Bearer " + _settings.AnonKey);
        }

        private static async Task<WebResponse> SendRequest(UnityWebRequest request)
        {
            var op = request.SendWebRequest();
            while (!op.isDone)
            {
                await Task.Yield();
            }

            bool success = request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError && request.responseCode < 400;
            return new WebResponse
            {
                Success = success,
                Body = request.downloadHandler != null ? request.downloadHandler.text : ""
            };
        }

        private static string WrapJsonArray(string jsonArray)
        {
            return "{\"items\":" + jsonArray + "}";
        }

        private class WebResponse
        {
            public bool Success;
            public string Body;
        }

        [System.Serializable]
        private class InventoryRow
        {
            public string user_id;
            public int slot_index;
            public string item_id;
            public int quantity;
        }

        [System.Serializable]
        private class InventoryRowList
        {
            public List<InventoryRow> items;
        }
    }
}
