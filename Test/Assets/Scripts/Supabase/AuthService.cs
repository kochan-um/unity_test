using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

namespace Supabase
{
    /// <summary>
    /// Supabase Auth を管理するサービス
    /// メール/パスワードによるサインアップ・ログイン・ログアウト
    /// セッション自動復元機能付き
    /// </summary>
    public class AuthService
    {
        private SupabaseSettings _settings;
        private string _accessToken;
        private string _refreshToken;
        private string _userId;
        private string _userEmail;

        private const string SESSION_KEY = "supabase_session";
        private const string ACCESS_TOKEN_KEY = "access_token";
        private const string REFRESH_TOKEN_KEY = "refresh_token";
        private const string USER_ID_KEY = "user_id";
        private const string USER_EMAIL_KEY = "user_email";

        public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken);
        public string UserId => _userId;
        public string UserEmail => _userEmail;
        public string AccessToken => _accessToken;

        public event System.Action<string> OnAuthError;
        public event System.Action OnAuthSuccess;
        public event System.Action OnLogout;

        public AuthService(SupabaseSettings settings)
        {
            _settings = settings;
            LoadSessionFromStorage();
        }

        /// <summary>
        /// メール/パスワードでサインアップ
        /// </summary>
        public void SignUp(string email, string password, System.Action<bool> callback)
        {
            SupabaseCoroutineRunner.Instance.StartCoroutine(SignUpAsync(email, password, callback));
        }

        private IEnumerator SignUpAsync(string email, string password, System.Action<bool> callback)
        {
            string url = _settings.SupabaseUrl.TrimEnd('/') + "/auth/v1/signup";

            var requestData = new SignUpRequest
            {
                email = email,
                password = password
            };

            string jsonData = JsonUtility.ToJson(requestData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("apikey", _settings.AnonKey);
                request.timeout = (int)_settings.RequestTimeout;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
                        if (response.user != null && response.session != null)
                        {
                            _accessToken = response.session.access_token;
                            _refreshToken = response.session.refresh_token;
                            _userId = response.user.id;
                            _userEmail = response.user.email;
                            SaveSessionToStorage();
                            OnAuthSuccess?.Invoke();
                            Debug.Log($"[Auth] サインアップ成功: {email}");
                            callback?.Invoke(true);
                        }
                        else
                        {
                            throw new System.Exception("レスポンス形式が不正です");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        OnAuthError?.Invoke($"サインアップ処理エラー: {ex.Message}");
                        callback?.Invoke(false);
                    }
                }
                else
                {
                    string errorMsg = request.downloadHandler.text;
                    OnAuthError?.Invoke($"サインアップ失敗: {errorMsg}");
                    Debug.LogError($"[Auth] サインアップエラー: {request.error}");
                    callback?.Invoke(false);
                }
            }
        }

        /// <summary>
        /// メール/パスワードでログイン
        /// </summary>
        public void SignIn(string email, string password, System.Action<bool> callback)
        {
            SupabaseCoroutineRunner.Instance.StartCoroutine(SignInAsync(email, password, callback));
        }

        private IEnumerator SignInAsync(string email, string password, System.Action<bool> callback)
        {
            string url = _settings.SupabaseUrl.TrimEnd('/') + "/auth/v1/token?grant_type=password";

            var requestData = new SignInRequest
            {
                email = email,
                password = password
            };

            string jsonData = JsonUtility.ToJson(requestData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("apikey", _settings.AnonKey);
                request.timeout = (int)_settings.RequestTimeout;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text);
                        _accessToken = response.access_token;
                        _refreshToken = response.refresh_token;
                        _userId = response.user.id;
                        _userEmail = response.user.email;
                        SaveSessionToStorage();
                        OnAuthSuccess?.Invoke();
                        Debug.Log($"[Auth] ログイン成功: {email}");
                        callback?.Invoke(true);
                    }
                    catch (System.Exception ex)
                    {
                        OnAuthError?.Invoke($"ログイン処理エラー: {ex.Message}");
                        callback?.Invoke(false);
                    }
                }
                else
                {
                    OnAuthError?.Invoke("メールアドレスまたはパスワードが正しくありません");
                    Debug.LogError($"[Auth] ログインエラー: {request.error}");
                    callback?.Invoke(false);
                }
            }
        }

        /// <summary>
        /// ログアウト
        /// </summary>
        public void SignOut()
        {
            _accessToken = null;
            _refreshToken = null;
            _userId = null;
            _userEmail = null;
            ClearSessionStorage();
            OnLogout?.Invoke();
            Debug.Log("[Auth] ログアウト完了");
        }

        /// <summary>
        /// セッション情報をPlayerPrefsに保存
        /// </summary>
        private void SaveSessionToStorage()
        {
            PlayerPrefs.SetString(ACCESS_TOKEN_KEY, _accessToken);
            PlayerPrefs.SetString(REFRESH_TOKEN_KEY, _refreshToken);
            PlayerPrefs.SetString(USER_ID_KEY, _userId);
            PlayerPrefs.SetString(USER_EMAIL_KEY, _userEmail);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// PlayerPrefsからセッション情報を復元
        /// </summary>
        private void LoadSessionFromStorage()
        {
            _accessToken = PlayerPrefs.GetString(ACCESS_TOKEN_KEY, "");
            _refreshToken = PlayerPrefs.GetString(REFRESH_TOKEN_KEY, "");
            _userId = PlayerPrefs.GetString(USER_ID_KEY, "");
            _userEmail = PlayerPrefs.GetString(USER_EMAIL_KEY, "");

            if (IsAuthenticated)
            {
                Debug.Log($"[Auth] セッション復元完了: {_userEmail}");
            }
        }

        /// <summary>
        /// セッション情報をクリア
        /// </summary>
        private void ClearSessionStorage()
        {
            PlayerPrefs.DeleteKey(ACCESS_TOKEN_KEY);
            PlayerPrefs.DeleteKey(REFRESH_TOKEN_KEY);
            PlayerPrefs.DeleteKey(USER_ID_KEY);
            PlayerPrefs.DeleteKey(USER_EMAIL_KEY);
            PlayerPrefs.Save();
        }

        // JSON シリアライズ用のクラス
        [System.Serializable]
        private class SignUpRequest
        {
            public string email;
            public string password;
        }

        [System.Serializable]
        private class SignInRequest
        {
            public string email;
            public string password;
        }

        [System.Serializable]
        private class AuthResponse
        {
            public User user;
            public Session session;
        }

        [System.Serializable]
        private class TokenResponse
        {
            public string access_token;
            public string refresh_token;
            public User user;
        }

        [System.Serializable]
        private class Session
        {
            public string access_token;
            public string refresh_token;
        }

        [System.Serializable]
        private class User
        {
            public string id;
            public string email;
        }
    }
}
