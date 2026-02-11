using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using System;

namespace Supabase
{
    /// <summary>
    /// プレイヤーデータのCRUD操作を管理するサービス
    /// Supabase Database (PostgREST) を使用
    /// </summary>
    public class PlayerDataService
    {
        private SupabaseSettings _settings;
        private AuthService _authService;

        public event System.Action<PlayerProfile> OnProfileLoaded;
        public event System.Action<PlayerProfile> OnProfileUpdated;
        public event System.Action<string> OnDataError;

        public PlayerDataService(SupabaseSettings settings)
        {
            _settings = settings;
            // AuthServiceへの参照が必要な場合はSupabaseManagerから取得
        }

        /// <summary>
        /// 認証サービスを設定（SupabaseManagerから呼び出し）
        /// </summary>
        internal void SetAuthService(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// プレイヤープロフィールを作成
        /// </summary>
        public void CreateProfile(string displayName, System.Action<bool> callback)
        {
            if (!ValidateAuth())
            {
                callback?.Invoke(false);
                return;
            }

            SupabaseCoroutineRunner.Instance.StartCoroutine(
                CreateProfileAsync(displayName, callback));
        }

        private IEnumerator CreateProfileAsync(string displayName, System.Action<bool> callback)
        {
            string url = _settings.SupabaseUrl.TrimEnd('/') +
                        "/rest/v1/player_profiles";

            var profileData = new PlayerProfile
            {
                user_id = _authService.UserId,
                display_name = displayName,
                created_at = System.DateTime.UtcNow.ToString("O")
            };

            string jsonData = JsonUtility.ToJson(profileData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("apikey", _settings.AnonKey);
                request.SetRequestHeader("Authorization", "Bearer " + _authService.AccessToken);
                request.timeout = (int)_settings.RequestTimeout;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[PlayerData] プロフィール作成成功");
                    callback?.Invoke(true);
                }
                else
                {
                    OnDataError?.Invoke($"プロフィール作成失敗: {request.error}");
                    callback?.Invoke(false);
                }
            }
        }

        /// <summary>
        /// 自身のプレイヤープロフィールを取得
        /// </summary>
        public void GetProfile(System.Action<PlayerProfile> callback)
        {
            if (!ValidateAuth())
            {
                callback?.Invoke(null);
                return;
            }

            SupabaseCoroutineRunner.Instance.StartCoroutine(
                GetProfileAsync(callback));
        }

        private IEnumerator GetProfileAsync(System.Action<PlayerProfile> callback)
        {
            string url = _settings.SupabaseUrl.TrimEnd('/') +
                        $"/rest/v1/player_profiles?user_id=eq.{_authService.UserId}&select=*";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("apikey", _settings.AnonKey);
                request.SetRequestHeader("Authorization", "Bearer " + _authService.AccessToken);
                request.timeout = (int)_settings.RequestTimeout;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        // JSON配列をパース
                        string json = "{\"items\":" + request.downloadHandler.text + "}";
                        var wrapper = JsonUtility.FromJson<PlayerProfileArray>(json);

                        if (wrapper.items != null && wrapper.items.Length > 0)
                        {
                            OnProfileLoaded?.Invoke(wrapper.items[0]);
                            callback?.Invoke(wrapper.items[0]);
                        }
                        else
                        {
                            callback?.Invoke(null);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        OnDataError?.Invoke($"プロフィール解析エラー: {ex.Message}");
                        callback?.Invoke(null);
                    }
                }
                else
                {
                    OnDataError?.Invoke($"プロフィール取得失敗: {request.error}");
                    callback?.Invoke(null);
                }
            }
        }

        /// <summary>
        /// プレイヤープロフィールを更新
        /// </summary>
        public void UpdateProfile(PlayerProfile profile, System.Action<bool> callback)
        {
            if (!ValidateAuth())
            {
                callback?.Invoke(false);
                return;
            }

            SupabaseCoroutineRunner.Instance.StartCoroutine(
                UpdateProfileAsync(profile, callback));
        }

        private IEnumerator UpdateProfileAsync(PlayerProfile profile, System.Action<bool> callback)
        {
            string url = _settings.SupabaseUrl.TrimEnd('/') +
                        $"/rest/v1/player_profiles?user_id=eq.{_authService.UserId}";

            string jsonData = JsonUtility.ToJson(profile);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("apikey", _settings.AnonKey);
                request.SetRequestHeader("Authorization", "Bearer " + _authService.AccessToken);
                request.timeout = (int)_settings.RequestTimeout;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    OnProfileUpdated?.Invoke(profile);
                    Debug.Log("[PlayerData] プロフィール更新成功");
                    callback?.Invoke(true);
                }
                else
                {
                    OnDataError?.Invoke($"プロフィール更新失敗: {request.error}");
                    callback?.Invoke(false);
                }
            }
        }

        /// <summary>
        /// 認証状態の検証
        /// </summary>
        private bool ValidateAuth()
        {
            if (_authService == null || !_authService.IsAuthenticated)
            {
                OnDataError?.Invoke("認証されていません。先にログインしてください。");
                return false;
            }
            return true;
        }

        // JSONシリアライズ用クラス
        [System.Serializable]
        public class PlayerProfile
        {
            public string user_id;
            public string display_name;
            public int score = 0;
            public int level = 1;
            public string created_at;
            public string updated_at;
        }

        [System.Serializable]
        private class PlayerProfileArray
        {
            public PlayerProfile[] items;
        }
    }
}
