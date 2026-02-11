using UnityEngine;
using UnityEngine.UI;
using Supabase;
using System.Text;

namespace SupabaseSamples
{
    /// <summary>
    /// Supabase ストレージ操作のサンプル実装
    /// ファイルアップロード・ダウンロード・削除・一覧取得をテスト
    /// </summary>
    public class SupabaseStorageSample : MonoBehaviour
    {
        [SerializeField]
        private InputField bucketNameInput;

        [SerializeField]
        private InputField filePathInput;

        [SerializeField]
        private InputField fileContentInput;

        [SerializeField]
        private InputField expirationSecondsInput;

        [SerializeField]
        private Button uploadButton;

        [SerializeField]
        private Button downloadButton;

        [SerializeField]
        private Button deleteButton;

        [SerializeField]
        private Button listButton;

        [SerializeField]
        private Button generateSignedUrlButton;

        [SerializeField]
        private Text statusText;

        [SerializeField]
        private Text fileListText;

        [SerializeField]
        private Text downloadedContentText;

        private void Start()
        {
            // デフォルト値を設定
            if (bucketNameInput != null)
                bucketNameInput.text = "avatars";
            if (filePathInput != null)
                filePathInput.text = "test.txt";
            if (expirationSecondsInput != null)
                expirationSecondsInput.text = "3600"; // 1時間

            // UI ボタンにリスナーを登録
            uploadButton?.onClick.AddListener(OnUploadClicked);
            downloadButton?.onClick.AddListener(OnDownloadClicked);
            deleteButton?.onClick.AddListener(OnDeleteClicked);
            listButton?.onClick.AddListener(OnListClicked);
            generateSignedUrlButton?.onClick.AddListener(OnGenerateSignedUrlClicked);

            // Supabase Manager のイベントをリッスン
            if (SupabaseManager.Instance != null && SupabaseManager.Instance.IsInitialized)
            {
                SupabaseManager.Instance.Storage.OnFileUploaded += HandleFileUploaded;
                SupabaseManager.Instance.Storage.OnFileDownloaded += HandleFileDownloaded;
                SupabaseManager.Instance.Storage.OnFilesListed += HandleFilesListed;
                SupabaseManager.Instance.Storage.OnStorageError += HandleStorageError;
            }
            else
            {
                SetStatusText("エラー: Supabase Manager が初期化されていません", Color.red);
            }
        }

        private void OnDestroy()
        {
            if (SupabaseManager.Instance != null)
            {
                SupabaseManager.Instance.Storage.OnFileUploaded -= HandleFileUploaded;
                SupabaseManager.Instance.Storage.OnFileDownloaded -= HandleFileDownloaded;
                SupabaseManager.Instance.Storage.OnFilesListed -= HandleFilesListed;
                SupabaseManager.Instance.Storage.OnStorageError -= HandleStorageError;
            }
        }

        /// <summary>
        /// ファイルアップロードボタンクリック
        /// </summary>
        private void OnUploadClicked()
        {
            if (!ValidateAuthentication())
                return;

            string bucketName = bucketNameInput?.text ?? "";
            string filePath = filePathInput?.text ?? "";
            string content = fileContentInput?.text ?? "テストデータ";

            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(filePath))
            {
                SetStatusText("バケット名とファイルパスを入力してください", Color.yellow);
                return;
            }

            byte[] fileData = Encoding.UTF8.GetBytes(content);
            SetStatusText("ファイルアップロード中...", Color.yellow);

            SupabaseManager.Instance.Storage.UploadFile(
                bucketName, filePath, fileData,
                (success, message) =>
                {
                    if (success)
                    {
                        SetStatusText($"アップロード成功: {message}", Color.green);
                        fileContentInput.text = "";
                    }
                    else
                    {
                        SetStatusText($"アップロード失敗: {message}", Color.red);
                    }
                });
        }

        /// <summary>
        /// ファイルダウンロードボタンクリック
        /// </summary>
        private void OnDownloadClicked()
        {
            if (!ValidateAuthentication())
                return;

            string bucketName = bucketNameInput?.text ?? "";
            string filePath = filePathInput?.text ?? "";

            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(filePath))
            {
                SetStatusText("バケット名とファイルパスを入力してください", Color.yellow);
                return;
            }

            SetStatusText("ファイルダウンロード中...", Color.yellow);

            SupabaseManager.Instance.Storage.DownloadFile(
                bucketName, filePath,
                (success, data) =>
                {
                    if (success)
                    {
                        SetStatusText($"ダウンロード成功: {data.Length} バイト", Color.green);
                        string content = Encoding.UTF8.GetString(data);
                        downloadedContentText.text = $"ダウンロード内容:\n{content}";
                        downloadedContentText.color = Color.white;
                    }
                    else
                    {
                        SetStatusText("ダウンロード失敗", Color.red);
                    }
                });
        }

        /// <summary>
        /// ファイル削除ボタンクリック
        /// </summary>
        private void OnDeleteClicked()
        {
            if (!ValidateAuthentication())
                return;

            string bucketName = bucketNameInput?.text ?? "";
            string filePath = filePathInput?.text ?? "";

            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(filePath))
            {
                SetStatusText("バケット名とファイルパスを入力してください", Color.yellow);
                return;
            }

            SetStatusText("ファイル削除中...", Color.yellow);

            SupabaseManager.Instance.Storage.DeleteFile(
                bucketName, filePath,
                (success) =>
                {
                    if (success)
                    {
                        SetStatusText("ファイル削除成功", Color.green);
                    }
                    else
                    {
                        SetStatusText("ファイル削除失敗", Color.red);
                    }
                });
        }

        /// <summary>
        /// ファイル一覧取得ボタンクリック
        /// </summary>
        private void OnListClicked()
        {
            if (!ValidateAuthentication())
                return;

            string bucketName = bucketNameInput?.text ?? "";
            if (string.IsNullOrEmpty(bucketName))
            {
                SetStatusText("バケット名を入力してください", Color.yellow);
                return;
            }

            SetStatusText("ファイル一覧取得中...", Color.yellow);

            SupabaseManager.Instance.Storage.ListFiles(
                bucketName, "",
                (success, files) =>
                {
                    if (success && files != null)
                    {
                        SetStatusText($"ファイル一覧取得成功: {files.Length} 個", Color.green);
                        DisplayFileList(files);
                    }
                    else
                    {
                        SetStatusText("ファイル一覧取得失敗", Color.red);
                    }
                });
        }

        /// <summary>
        /// 署名付きURL生成ボタンクリック
        /// </summary>
        private void OnGenerateSignedUrlClicked()
        {
            if (!ValidateAuthentication())
                return;

            string bucketName = bucketNameInput?.text ?? "";
            string filePath = filePathInput?.text ?? "";

            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(filePath))
            {
                SetStatusText("バケット名とファイルパスを入力してください", Color.yellow);
                return;
            }

            int expirationSeconds = int.TryParse(expirationSecondsInput?.text, out int seconds)
                ? seconds : 3600;

            SetStatusText("署名付きURL生成中...", Color.yellow);

            SupabaseManager.Instance.Storage.GenerateSignedUrl(
                bucketName, filePath, expirationSeconds,
                (success, signedUrl) =>
                {
                    if (success)
                    {
                        SetStatusText("署名付きURL生成成功", Color.green);
                        downloadedContentText.text = $"署名付きURL:\n{signedUrl}";
                        downloadedContentText.color = Color.cyan;
                    }
                    else
                    {
                        SetStatusText("署名付きURL生成失敗", Color.red);
                    }
                });
        }

        /// <summary>
        /// ファイルアップロードイベントハンドラー
        /// </summary>
        private void HandleFileUploaded(string filePath)
        {
            Debug.Log($"[StorageSample] ファイルアップロード: {filePath}");
        }

        /// <summary>
        /// ファイルダウンロードイベントハンドラー
        /// </summary>
        private void HandleFileDownloaded(byte[] data)
        {
            Debug.Log($"[StorageSample] ファイルダウンロード: {data.Length} バイト");
        }

        /// <summary>
        /// ファイル一覧取得イベントハンドラー
        /// </summary>
        private void HandleFilesListed(StorageService.StorageFile[] files)
        {
            Debug.Log($"[StorageSample] ファイル一覧取得: {files.Length} 個");
        }

        /// <summary>
        /// ストレージエラーハンドラー
        /// </summary>
        private void HandleStorageError(string error)
        {
            Debug.LogError($"[StorageSample] ストレージエラー: {error}");
            SetStatusText($"エラー: {error}", Color.red);
        }

        /// <summary>
        /// ファイル一覧を画面に表示
        /// </summary>
        private void DisplayFileList(StorageService.StorageFile[] files)
        {
            StringBuilder sb = new StringBuilder("ファイル一覧:\n");
            foreach (var file in files)
            {
                sb.AppendLine($"名前: {file.name}");
                sb.AppendLine($"  サイズ: {file.size} バイト");
                sb.AppendLine($"  更新日時: {file.updated_at}");
                sb.AppendLine();
            }

            fileListText.text = sb.ToString();
            fileListText.color = Color.white;
        }

        /// <summary>
        /// 認証状態をチェック
        /// </summary>
        private bool ValidateAuthentication()
        {
            if (!SupabaseManager.Instance.Auth.IsAuthenticated)
            {
                SetStatusText("エラー: 先にログインしてください", Color.red);
                return false;
            }
            return true;
        }

        /// <summary>
        /// ステータステキストを更新
        /// </summary>
        private void SetStatusText(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }
        }
    }
}
