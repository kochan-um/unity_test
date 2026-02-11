using UnityEngine;

namespace Supabase
{
    /// <summary>
    /// Supabase接続設定をScriptableObjectで管理するクラス
    /// APIキーなどの秘匿情報をハードコード防止
    /// </summary>
    [CreateAssetMenu(fileName = "SupabaseSettings", menuName = "Supabase/Settings")]
    public class SupabaseSettings : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Supabase プロジェクトURL（例: https://xxxxx.supabase.co）")]
        private string supabaseUrl = "";

        [SerializeField]
        [Tooltip("Supabase Anon Key（公開可能なクライアント向けキー）")]
        private string anonKey = "";

        [SerializeField]
        [Tooltip("API通信タイムアウト（秒）")]
        private float requestTimeout = 30f;

        [SerializeField]
        [Tooltip("リトライの最大試行回数")]
        private int maxRetryAttempts = 3;

        [SerializeField]
        [Tooltip("ファイルアップロード最大サイズ（MB）")]
        private long maxUploadSizeMB = 50;

        public string SupabaseUrl => supabaseUrl;
        public string AnonKey => anonKey;
        public float RequestTimeout => requestTimeout;
        public int MaxRetryAttempts => maxRetryAttempts;
        public long MaxUploadSizeBytes => maxUploadSizeMB * 1024 * 1024;

        /// <summary>
        /// 設定が有効か検証
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(supabaseUrl) && !string.IsNullOrEmpty(anonKey);
        }

        /// <summary>
        /// 検証エラーメッセージを取得
        /// </summary>
        public string GetValidationError()
        {
            if (string.IsNullOrEmpty(supabaseUrl))
                return "SupabaseUrl が設定されていません";
            if (string.IsNullOrEmpty(anonKey))
                return "AnonKey が設定されていません";
            return "";
        }
    }
}
