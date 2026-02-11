using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Collections.Generic;

namespace Supabase
{
    /// <summary>
    /// Supabase Storage を管理するサービス
    /// ファイルのアップロード・ダウンロード・削除・一覧取得
    /// </summary>
    public class StorageService
    {
        private SupabaseSettings _settings;
        private AuthService _authService;

        public event System.Action<string> OnFileUploaded;
        public event System.Action<byte[]> OnFileDownloaded;
        public event System.Action<StorageFile[]> OnFilesListed;
        public event System.Action<string> OnStorageError;

        public StorageService(SupabaseSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// 認証サービスを設定（SupabaseManagerから呼び出し）
        /// </summary>
        internal void SetAuthService(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// ファイルをアップロード
        /// </summary>
        public void UploadFile(string bucketName, string filePath, byte[] fileData,
                               System.Action<bool, string> callback)
        {
            if (!ValidateAuth())
            {
                callback?.Invoke(false, "認証が必要です");
                return;
            }

            // ファイルサイズ チェック
            if (fileData.Length > _settings.MaxUploadSizeBytes)
            {
                string error = $"ファイルサイズが大きすぎます（最大: {_settings.MaxUploadSizeBytes / (1024 * 1024)}MB）";
                OnStorageError?.Invoke(error);
                callback?.Invoke(false, error);
                return;
            }

            SupabaseCoroutineRunner.Instance.StartCoroutine(
                UploadFileAsync(bucketName, filePath, fileData, callback));
        }

        private IEnumerator UploadFileAsync(string bucketName, string filePath, byte[] fileData,
                                           System.Action<bool, string> callback)
        {
            string url = _settings.SupabaseUrl.TrimEnd('/') +
                        $"/storage/v1/object/{bucketName}/{filePath}";

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(fileData);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("apikey", _settings.AnonKey);
                request.SetRequestHeader("Authorization", "Bearer " + _authService.AccessToken);
                request.timeout = (int)_settings.RequestTimeout;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    OnFileUploaded?.Invoke(filePath);
                    Debug.Log($"[Storage] ファイルアップロード成功: {filePath}");
                    callback?.Invoke(true, filePath);
                }
                else
                {
                    string error = $"アップロード失敗: {request.error}";
                    OnStorageError?.Invoke(error);
                    callback?.Invoke(false, error);
                }
            }
        }

        /// <summary>
        /// ファイルをダウンロード
        /// </summary>
        public void DownloadFile(string bucketName, string filePath,
                                System.Action<bool, byte[]> callback)
        {
            if (!ValidateAuth())
            {
                callback?.Invoke(false, null);
                return;
            }

            SupabaseCoroutineRunner.Instance.StartCoroutine(
                DownloadFileAsync(bucketName, filePath, callback));
        }

        private IEnumerator DownloadFileAsync(string bucketName, string filePath,
                                             System.Action<bool, byte[]> callback)
        {
            string url = _settings.SupabaseUrl.TrimEnd('/') +
                        $"/storage/v1/object/{bucketName}/{filePath}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("apikey", _settings.AnonKey);
                request.SetRequestHeader("Authorization", "Bearer " + _authService.AccessToken);
                request.timeout = (int)_settings.RequestTimeout;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = request.downloadHandler.data;
                    OnFileDownloaded?.Invoke(data);
                    Debug.Log($"[Storage] ファイルダウンロード成功: {filePath}");
                    callback?.Invoke(true, data);
                }
                else
                {
                    OnStorageError?.Invoke($"ダウンロード失敗: {request.error}");
                    callback?.Invoke(false, null);
                }
            }
        }

        /// <summary>
        /// ファイルを削除
        /// </summary>
        public void DeleteFile(string bucketName, string filePath,
                              System.Action<bool> callback)
        {
            if (!ValidateAuth())
            {
                callback?.Invoke(false);
                return;
            }

            SupabaseCoroutineRunner.Instance.StartCoroutine(
                DeleteFileAsync(bucketName, filePath, callback));
        }

        private IEnumerator DeleteFileAsync(string bucketName, string filePath,
                                           System.Action<bool> callback)
        {
            string url = _settings.SupabaseUrl.TrimEnd('/') +
                        $"/storage/v1/object/{bucketName}/{filePath}";

            using (UnityWebRequest request = UnityWebRequest.Delete(url))
            {
                request.SetRequestHeader("apikey", _settings.AnonKey);
                request.SetRequestHeader("Authorization", "Bearer " + _authService.AccessToken);
                request.timeout = (int)_settings.RequestTimeout;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[Storage] ファイル削除成功: {filePath}");
                    callback?.Invoke(true);
                }
                else
                {
                    OnStorageError?.Invoke($"削除失敗: {request.error}");
                    callback?.Invoke(false);
                }
            }
        }

        /// <summary>
        /// バケット内のファイル一覧を取得
        /// </summary>
        public void ListFiles(string bucketName, string folderPath = "",
                             System.Action<bool, StorageFile[]> callback = null)
        {
            if (!ValidateAuth())
            {
                callback?.Invoke(false, null);
                return;
            }

            SupabaseCoroutineRunner.Instance.StartCoroutine(
                ListFilesAsync(bucketName, folderPath, callback));
        }

        private IEnumerator ListFilesAsync(string bucketName, string folderPath,
                                          System.Action<bool, StorageFile[]> callback)
        {
            string url = _settings.SupabaseUrl.TrimEnd('/') +
                        $"/storage/v1/object/list/{bucketName}";

            if (!string.IsNullOrEmpty(folderPath))
            {
                url += $"?prefix={folderPath}";
            }

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
                        string json = "{\"files\":" + request.downloadHandler.text + "}";
                        var wrapper = JsonUtility.FromJson<StorageFileArray>(json);

                        if (wrapper.files != null)
                        {
                            OnFilesListed?.Invoke(wrapper.files);
                            callback?.Invoke(true, wrapper.files);
                        }
                        else
                        {
                            callback?.Invoke(true, new StorageFile[0]);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        OnStorageError?.Invoke($"ファイル一覧解析エラー: {ex.Message}");
                        callback?.Invoke(false, null);
                    }
                }
                else
                {
                    OnStorageError?.Invoke($"ファイル一覧取得失敗: {request.error}");
                    callback?.Invoke(false, null);
                }
            }
        }

        /// <summary>
        /// 署名付きURL を生成（privateバケット用）
        /// </summary>
        public void GenerateSignedUrl(string bucketName, string filePath, int expirationSeconds,
                                     System.Action<bool, string> callback)
        {
            if (!ValidateAuth())
            {
                callback?.Invoke(false, "");
                return;
            }

            SupabaseCoroutineRunner.Instance.StartCoroutine(
                GenerateSignedUrlAsync(bucketName, filePath, expirationSeconds, callback));
        }

        private IEnumerator GenerateSignedUrlAsync(string bucketName, string filePath,
                                                  int expirationSeconds,
                                                  System.Action<bool, string> callback)
        {
            string url = _settings.SupabaseUrl.TrimEnd('/') +
                        $"/storage/v1/object/signed-url/{bucketName}/{filePath}";

            var requestData = new SignedUrlRequest
            {
                expiresIn = expirationSeconds
            };

            string jsonData = JsonUtility.ToJson(requestData);
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
                    try
                    {
                        var response = JsonUtility.FromJson<SignedUrlResponse>(request.downloadHandler.text);
                        Debug.Log($"[Storage] 署名付きURL生成成功");
                        callback?.Invoke(true, response.signedURL);
                    }
                    catch (System.Exception ex)
                    {
                        OnStorageError?.Invoke($"署名付きURL生成エラー: {ex.Message}");
                        callback?.Invoke(false, "");
                    }
                }
                else
                {
                    OnStorageError?.Invoke($"署名付きURL生成失敗: {request.error}");
                    callback?.Invoke(false, "");
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
                OnStorageError?.Invoke("認証されていません。先にログインしてください。");
                return false;
            }
            return true;
        }

        // JSONシリアライズ用クラス
        [System.Serializable]
        public class StorageFile
        {
            public string name;
            public long size;
            public string updated_at;
            public string id;
        }

        [System.Serializable]
        private class StorageFileArray
        {
            public StorageFile[] files;
        }

        [System.Serializable]
        private class SignedUrlRequest
        {
            public int expiresIn;
        }

        [System.Serializable]
        private class SignedUrlResponse
        {
            public string signedURL;
        }
    }
}
