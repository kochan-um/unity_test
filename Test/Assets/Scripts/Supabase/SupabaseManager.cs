using UnityEngine;
using System.Collections.Generic;

namespace Supabase
{
    /// <summary>
    /// Supabase全体の初期化と各サービスのエントリーポイント
    /// シングルトンパターンで実装
    /// </summary>
    public class SupabaseManager : MonoBehaviour
    {
        private static SupabaseManager _instance;

        [SerializeField]
        private SupabaseSettings settings;

        private AuthService _authService;
        private PlayerDataService _playerDataService;
        private StorageService _storageService;

        private bool _initialized = false;

        public static SupabaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SupabaseManager>();
                    if (_instance == null)
                    {
                        GameObject managerGo = new GameObject("SupabaseManager");
                        _instance = managerGo.AddComponent<SupabaseManager>();
                        DontDestroyOnLoad(managerGo);
                    }
                }
                return _instance;
            }
        }

        public AuthService Auth => _authService;
        public PlayerDataService PlayerData => _playerDataService;
        public StorageService Storage => _storageService;
        public bool IsInitialized => _initialized;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // エディタ内で設定ファイルが指定されない場合は自動ロード
            if (settings == null)
            {
                settings = Resources.Load<SupabaseSettings>("SupabaseSettings");
            }

            Initialize();
        }

        /// <summary>
        /// Supabase接続を初期化
        /// </summary>
        private void Initialize()
        {
            if (settings == null)
            {
                Debug.LogError("[Supabase] SupabaseSettings が見つかりません。Assets/Resources/SupabaseSettings.asset を作成してください。");
                _initialized = false;
                return;
            }

            if (!settings.IsValid())
            {
                Debug.LogError($"[Supabase] 設定エラー: {settings.GetValidationError()}");
                _initialized = false;
                return;
            }

            // 各サービスを初期化
            _authService = new AuthService(settings);
            _playerDataService = new PlayerDataService(settings);
            _storageService = new StorageService(settings);

            // サービス間の依存関係を設定
            _playerDataService.SetAuthService(_authService);
            _storageService.SetAuthService(_authService);

            _initialized = true;
            Debug.Log("[Supabase] 初期化完了: " + settings.SupabaseUrl);
        }

        /// <summary>
        /// アプリケーション終了時のクリーンアップ
        /// </summary>
        private void OnDestroy()
        {
            if (_initialized && _authService != null)
            {
                // セッション情報のクリーンアップ（必要に応じて）
            }
        }
    }
}
