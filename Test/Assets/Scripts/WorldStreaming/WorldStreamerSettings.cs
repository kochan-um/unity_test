using UnityEngine;

namespace WorldStreaming
{
    /// <summary>
    /// ワールドストリーミングシステムの設定をまとめたScriptableObject。
    /// エディタから全パラメータを編集可能。
    /// </summary>
    [CreateAssetMenu(fileName = "WorldStreamerSettings", menuName = "WorldStreaming/Settings")]
    public class WorldStreamerSettings : ScriptableObject
    {
        [Header("Grid Configuration")]
        [Tooltip("チャンクのサイズ（メートル）")]
        [SerializeField] private float chunkSize = 100f;

        [Header("Loading")]
        [Tooltip("ロード対象の半径（チャンク数）")]
        [SerializeField] private int loadRadius = 3;

        [Tooltip("アンロード対象の半径（チャンク数）")]
        [SerializeField] private int unloadRadius = 4;

        [Tooltip("最大同時ロード数")]
        [SerializeField] private int maxConcurrentLoads = 3;

        [Tooltip("ロード失敗時の最大リトライ回数")]
        [SerializeField] private int maxRetries = 3;

        [Tooltip("リトライ前の基本待機時間（秒）")]
        [SerializeField] private float retryBaseDelaySeconds = 0.5f;

        [Header("LOD")]
        [Tooltip("Medium LODに切り替わる半径（チャンク数）")]
        [SerializeField] private int lodMediumRadius = 5;

        [Tooltip("Low LODに切り替わる半径（チャンク数）")]
        [SerializeField] private int lodLowRadius = 8;

        [Header("Memory Management")]
        [Tooltip("メモリバジェット上限（MB）")]
        [SerializeField] private float memoryBudgetMB = 512f;

        [Tooltip("最小保護半径（チャンク数。この半径内は強制アンロード対象外）")]
        [SerializeField] private int minProtectedRadius = 1;

        [Header("Effects")]
        [Tooltip("チャンクフェードイン時間（秒）")]
        [SerializeField] private float fadeInDuration = 0.5f;

        [Tooltip("チャンクロード時のフェードイン演出を有効にするか")]
        [SerializeField] private bool enableFadeIn = true;

        [Header("Debug")]
        [Tooltip("デバッグギズモを表示するか")]
        [SerializeField] private bool enableDebugGizmos = false;

        [Tooltip("デバッグログを出力するか")]
        [SerializeField] private bool enableDebugLogging = false;

        public float ChunkSize => chunkSize;
        public int LoadRadius => loadRadius;
        public int UnloadRadius => unloadRadius;
        public int MaxConcurrentLoads => maxConcurrentLoads;
        public int MaxRetries => maxRetries;
        public float RetryBaseDelaySeconds => retryBaseDelaySeconds;
        public int LODMediumRadius => lodMediumRadius;
        public int LODLowRadius => lodLowRadius;
        public float MemoryBudgetBytes => memoryBudgetMB * 1024f * 1024f;
        public int MinProtectedRadius => minProtectedRadius;
        public float FadeInDuration => fadeInDuration;
        public bool EnableFadeIn => enableFadeIn;
        public bool EnableDebugGizmos => enableDebugGizmos;
        public bool EnableDebugLogging => enableDebugLogging;

        private void OnValidate()
        {
            chunkSize = Mathf.Max(1f, chunkSize);
            loadRadius = Mathf.Max(1, loadRadius);
            unloadRadius = Mathf.Max(loadRadius, unloadRadius);
            maxConcurrentLoads = Mathf.Max(1, maxConcurrentLoads);
            maxRetries = Mathf.Max(0, maxRetries);
            retryBaseDelaySeconds = Mathf.Max(0.1f, retryBaseDelaySeconds);
            lodMediumRadius = Mathf.Max(loadRadius, lodMediumRadius);
            lodLowRadius = Mathf.Max(lodMediumRadius, lodLowRadius);
            memoryBudgetMB = Mathf.Max(100f, memoryBudgetMB);
            minProtectedRadius = Mathf.Max(0, minProtectedRadius);
            fadeInDuration = Mathf.Max(0f, fadeInDuration);
        }
    }
}
