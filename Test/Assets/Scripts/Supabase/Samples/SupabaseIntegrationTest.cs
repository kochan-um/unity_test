using UnityEngine;
using Supabase;
using System.Collections;

namespace SupabaseSamples
{
    /// <summary>
    /// Supabase 統合テストスクリプト
    /// 全機能を順序立ててテスト（コンソール出力確認用）
    /// </summary>
    public class SupabaseIntegrationTest : MonoBehaviour
    {
        [SerializeField]
        private string testEmail = "test@example.com";

        [SerializeField]
        private string testPassword = "password123";

        [SerializeField]
        private string testDisplayName = "TestPlayer";

        [SerializeField]
        private bool autoRunTests = false;

        private void Start()
        {
            if (autoRunTests)
            {
                StartCoroutine(RunIntegrationTests());
            }
        }

        /// <summary>
        /// 統合テストを順序立てて実行
        /// </summary>
        private IEnumerator RunIntegrationTests()
        {
            Debug.Log("========== Supabase 統合テスト開始 ==========");

            // 初期化確認
            if (!SupabaseManager.Instance.IsInitialized)
            {
                Debug.LogError("[Test] Supabase Manager が初期化されていません");
                yield break;
            }
            Debug.Log("[Test] ✓ Supabase Manager 初期化完了");
            yield return new WaitForSeconds(1);

            // 1. 認証テスト
            Debug.Log("\n--- 認証テスト開始 ---");
            bool signUpSuccess = false;
            SupabaseManager.Instance.Auth.SignUp(testEmail, testPassword, (success) =>
            {
                signUpSuccess = success;
                if (success)
                {
                    Debug.Log($"[Test] ✓ サインアップ成功: {testEmail}");
                }
                else
                {
                    Debug.Log($"[Test] ✗ サインアップ失敗");
                }
            });
            yield return new WaitForSeconds(2);

            if (!signUpSuccess)
            {
                // 既に登録済みの場合はログインする
                Debug.Log("[Test] サインアップ失敗のため、ログインを試行");
                bool signInSuccess = false;
                SupabaseManager.Instance.Auth.SignIn(testEmail, testPassword, (success) =>
                {
                    signInSuccess = success;
                });
                yield return new WaitForSeconds(2);

                if (!signInSuccess)
                {
                    Debug.LogError("[Test] ✗ ログイン失敗。テスト中止");
                    yield break;
                }
            }

            Debug.Log($"[Test] ✓ ユーザーID: {SupabaseManager.Instance.Auth.UserId}");
            yield return new WaitForSeconds(1);

            // 2. プレイヤーデータテスト
            Debug.Log("\n--- プレイヤーデータテスト開始 ---");
            bool profileCreated = false;
            SupabaseManager.Instance.PlayerData.CreateProfile(testDisplayName, (success) =>
            {
                profileCreated = success;
                if (success)
                {
                    Debug.Log($"[Test] ✓ プロフィール作成成功: {testDisplayName}");
                }
                else
                {
                    Debug.Log($"[Test] ✗ プロフィール作成失敗");
                }
            });
            yield return new WaitForSeconds(2);

            // プロフィール取得テスト
            PlayerDataService.PlayerProfile loadedProfile = null;
            SupabaseManager.Instance.PlayerData.GetProfile((profile) =>
            {
                loadedProfile = profile;
                if (profile != null)
                {
                    Debug.Log($"[Test] ✓ プロフィール取得成功");
                    Debug.Log($"  表示名: {profile.display_name}");
                    Debug.Log($"  スコア: {profile.score}");
                    Debug.Log($"  レベル: {profile.level}");
                }
                else
                {
                    Debug.Log($"[Test] ✗ プロフィール取得失敗");
                }
            });
            yield return new WaitForSeconds(2);

            // プロフィール更新テスト
            if (loadedProfile != null)
            {
                var updatedProfile = new PlayerDataService.PlayerProfile
                {
                    user_id = loadedProfile.user_id,
                    display_name = testDisplayName,
                    score = 1000,
                    level = 5,
                    created_at = loadedProfile.created_at
                };

                bool updateSuccess = false;
                SupabaseManager.Instance.PlayerData.UpdateProfile(updatedProfile, (success) =>
                {
                    updateSuccess = success;
                    if (success)
                    {
                        Debug.Log($"[Test] ✓ プロフィール更新成功 (スコア: 1000, レベル: 5)");
                    }
                    else
                    {
                        Debug.Log($"[Test] ✗ プロフィール更新失敗");
                    }
                });
                yield return new WaitForSeconds(2);
            }

            // 3. ストレージテスト
            Debug.Log("\n--- ストレージテスト開始 ---");
            string testFileName = "test_" + System.DateTime.Now.Ticks + ".txt";
            string testContent = "これはテストファイルです。\nSupabase Storage のテストです。";
            byte[] testFileData = System.Text.Encoding.UTF8.GetBytes(testContent);

            bool uploadSuccess = false;
            SupabaseManager.Instance.Storage.UploadFile(
                "avatars", testFileName, testFileData,
                (success, message) =>
                {
                    uploadSuccess = success;
                    if (success)
                    {
                        Debug.Log($"[Test] ✓ ファイルアップロード成功: {message}");
                    }
                    else
                    {
                        Debug.Log($"[Test] ✗ ファイルアップロード失敗: {message}");
                    }
                });
            yield return new WaitForSeconds(2);

            // ファイル一覧取得テスト
            if (uploadSuccess)
            {
                StorageService.StorageFile[] fileList = null;
                SupabaseManager.Instance.Storage.ListFiles(
                    "avatars", "",
                    (success, files) =>
                    {
                        if (success && files != null)
                        {
                            fileList = files;
                            Debug.Log($"[Test] ✓ ファイル一覧取得成功: {files.Length} 個");
                            foreach (var file in files)
                            {
                                Debug.Log($"  - {file.name} ({file.size} bytes)");
                            }
                        }
                        else
                        {
                            Debug.Log($"[Test] ✗ ファイル一覧取得失敗");
                        }
                    });
                yield return new WaitForSeconds(2);

                // ファイルダウンロードテスト
                SupabaseManager.Instance.Storage.DownloadFile(
                    "avatars", testFileName,
                    (success, data) =>
                    {
                        if (success)
                        {
                            string downloadedContent = System.Text.Encoding.UTF8.GetString(data);
                            Debug.Log($"[Test] ✓ ファイルダウンロード成功: {data.Length} bytes");
                            Debug.Log($"  内容: {downloadedContent}");
                        }
                        else
                        {
                            Debug.Log($"[Test] ✗ ファイルダウンロード失敗");
                        }
                    });
                yield return new WaitForSeconds(2);

                // ファイル削除テスト
                bool deleteSuccess = false;
                SupabaseManager.Instance.Storage.DeleteFile(
                    "avatars", testFileName,
                    (success) =>
                    {
                        deleteSuccess = success;
                        if (success)
                        {
                            Debug.Log($"[Test] ✓ ファイル削除成功");
                        }
                        else
                        {
                            Debug.Log($"[Test] ✗ ファイル削除失敗");
                        }
                    });
                yield return new WaitForSeconds(2);
            }

            // 4. ログアウトテスト
            Debug.Log("\n--- ログアウトテスト開始 ---");
            SupabaseManager.Instance.Auth.SignOut();
            if (!SupabaseManager.Instance.Auth.IsAuthenticated)
            {
                Debug.Log("[Test] ✓ ログアウト成功");
            }
            else
            {
                Debug.Log("[Test] ✗ ログアウト失敗");
            }

            Debug.Log("\n========== Supabase 統合テスト完了 ==========");
            Debug.Log("Console の出力を確認して、すべてのテストが成功したか確認してください。");
        }

        /// <summary>
        /// Inspector から手動でテストを開始できるメソッド
        /// </summary>
        [ContextMenu("Run Integration Tests")]
        public void ManualRunTests()
        {
            StartCoroutine(RunIntegrationTests());
        }
    }
}
