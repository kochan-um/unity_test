using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

namespace Supabase
{
    /// <summary>
    /// ネットワーク通信のリトライ・エラーハンドリング をサポートするユーティリティクラス
    /// 指数バックオフによるリトライロジック実装
    /// </summary>
    public class NetworkRetryHelper
    {
        private SupabaseSettings _settings;

        public NetworkRetryHelper(SupabaseSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// リトライロジック付きでUnityWebRequestを実行
        /// </summary>
        public IEnumerator ExecuteWithRetry(UnityWebRequest request,
                                           System.Action<bool, string> onComplete)
        {
            int attemptCount = 0;
            int maxRetries = _settings.MaxRetryAttempts;

            while (attemptCount < maxRetries)
            {
                attemptCount++;
                float timeout = _settings.RequestTimeout;
                request.timeout = (int)timeout;

                yield return request.SendWebRequest();

                // 5xx エラーまたはネットワークエラー → リトライ
                if (ShouldRetry(request))
                {
                    if (attemptCount < maxRetries)
                    {
                        float delaySeconds = GetExponentialBackoffDelay(attemptCount);
                        Debug.LogWarning($"[Retry] {attemptCount}/{maxRetries}回目のリトライ前に{delaySeconds}秒待機します");
                        yield return new WaitForSeconds(delaySeconds);

                        // 次のリトライのためにリセット
                        request.Dispose();
                        request = new UnityWebRequest(request.url, request.method);
                    }
                    else
                    {
                        // 最大リトライ回数に達した
                        onComplete?.Invoke(false, $"リトライ回数超過（{maxRetries}回）");
                        break;
                    }
                }
                else
                {
                    // 成功 or 4xxクライアントエラー（リトライしない）
                    bool success = request.result == UnityWebRequest.Result.Success;
                    string message = request.result == UnityWebRequest.Result.Success ?
                                    "成功" : request.downloadHandler?.text ?? request.error;
                    onComplete?.Invoke(success, message);
                    break;
                }
            }
        }

        /// <summary>
        /// リトライすべきかを判定
        /// 5xx系 or ネットワークエラー → true
        /// 4xx系 or 2xx系 → false
        /// </summary>
        private bool ShouldRetry(UnityWebRequest request)
        {
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                // HTTPステータスコードを確認
                long responseCode = request.responseCode;

                // 5xx系エラー → リトライ
                if (responseCode >= 500 && responseCode < 600)
                    return true;

                // タイムアウト → リトライ
                if (request.error != null && request.error.Contains("timeout"))
                    return true;

                // その他のエラー（4xx）→ リトライしない
                return false;
            }

            // 成功 → リトライしない
            return false;
        }

        /// <summary>
        /// 指数バックオフの遅延時間を計算
        /// 例: 試行1→1秒、試行2→2秒、試行3→4秒
        /// </summary>
        private float GetExponentialBackoffDelay(int attemptCount)
        {
            float baseDelay = 1f;
            float delay = baseDelay * (float)System.Math.Pow(2, attemptCount - 1);
            float maxDelay = 10f;
            return Mathf.Min(delay, maxDelay);
        }
    }
}
