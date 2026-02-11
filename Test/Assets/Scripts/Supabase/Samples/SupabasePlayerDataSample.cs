using UnityEngine;
using UnityEngine.UI;
using Supabase;

namespace SupabaseSamples
{
    /// <summary>
    /// Supabase プレイヤーデータ操作のサンプル実装
    /// プロフィール作成・取得・更新をテスト
    /// </summary>
    public class SupabasePlayerDataSample : MonoBehaviour
    {
        [SerializeField]
        private InputField displayNameInput;

        [SerializeField]
        private InputField scoreInput;

        [SerializeField]
        private InputField levelInput;

        [SerializeField]
        private Button createProfileButton;

        [SerializeField]
        private Button getProfileButton;

        [SerializeField]
        private Button updateProfileButton;

        [SerializeField]
        private Text statusText;

        [SerializeField]
        private Text profileDataText;

        private PlayerDataService.PlayerProfile _currentProfile;

        private void Start()
        {
            // UI ボタンにリスナーを登録
            createProfileButton?.onClick.AddListener(OnCreateProfileClicked);
            getProfileButton?.onClick.AddListener(OnGetProfileClicked);
            updateProfileButton?.onClick.AddListener(OnUpdateProfileClicked);

            // Supabase Manager のイベントをリッスン
            if (SupabaseManager.Instance != null && SupabaseManager.Instance.IsInitialized)
            {
                SupabaseManager.Instance.PlayerData.OnProfileLoaded += HandleProfileLoaded;
                SupabaseManager.Instance.PlayerData.OnProfileUpdated += HandleProfileUpdated;
                SupabaseManager.Instance.PlayerData.OnDataError += HandleDataError;
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
                SupabaseManager.Instance.PlayerData.OnProfileLoaded -= HandleProfileLoaded;
                SupabaseManager.Instance.PlayerData.OnProfileUpdated -= HandleProfileUpdated;
                SupabaseManager.Instance.PlayerData.OnDataError -= HandleDataError;
            }
        }

        /// <summary>
        /// プロフィール作成ボタンクリック
        /// </summary>
        private void OnCreateProfileClicked()
        {
            if (!ValidateAuthentication())
                return;

            string displayName = displayNameInput?.text ?? "";
            if (string.IsNullOrEmpty(displayName))
            {
                SetStatusText("表示名を入力してください", Color.yellow);
                return;
            }

            SetStatusText("プロフィール作成中...", Color.yellow);
            SupabaseManager.Instance.PlayerData.CreateProfile(displayName, (success) =>
            {
                if (success)
                {
                    SetStatusText("プロフィール作成成功", Color.green);
                    displayNameInput.text = "";
                }
                else
                {
                    SetStatusText("プロフィール作成失敗", Color.red);
                }
            });
        }

        /// <summary>
        /// プロフィール取得ボタンクリック
        /// </summary>
        private void OnGetProfileClicked()
        {
            if (!ValidateAuthentication())
                return;

            SetStatusText("プロフィール取得中...", Color.yellow);
            SupabaseManager.Instance.PlayerData.GetProfile((profile) =>
            {
                if (profile != null)
                {
                    _currentProfile = profile;
                    SetStatusText("プロフィール取得成功", Color.green);
                    DisplayProfile(profile);
                }
                else
                {
                    SetStatusText("プロフィール取得失敗", Color.red);
                    profileDataText.text = "プロフィールが見つかりません";
                }
            });
        }

        /// <summary>
        /// プロフィール更新ボタンクリック
        /// </summary>
        private void OnUpdateProfileClicked()
        {
            if (!ValidateAuthentication())
                return;

            if (_currentProfile == null)
            {
                SetStatusText("先にプロフィール取得をしてください", Color.yellow);
                return;
            }

            string displayName = displayNameInput?.text ?? _currentProfile.display_name;
            int score = int.TryParse(scoreInput?.text, out int s) ? s : _currentProfile.score;
            int level = int.TryParse(levelInput?.text, out int l) ? l : _currentProfile.level;

            var updatedProfile = new PlayerDataService.PlayerProfile
            {
                user_id = _currentProfile.user_id,
                display_name = displayName,
                score = score,
                level = level,
                created_at = _currentProfile.created_at
            };

            SetStatusText("プロフィール更新中...", Color.yellow);
            SupabaseManager.Instance.PlayerData.UpdateProfile(updatedProfile, (success) =>
            {
                if (success)
                {
                    SetStatusText("プロフィール更新成功", Color.green);
                    _currentProfile = updatedProfile;
                    DisplayProfile(updatedProfile);
                }
                else
                {
                    SetStatusText("プロフィール更新失敗", Color.red);
                }
            });
        }

        /// <summary>
        /// プロフィールロードイベントハンドラー
        /// </summary>
        private void HandleProfileLoaded(PlayerDataService.PlayerProfile profile)
        {
            Debug.Log($"[PlayerDataSample] プロフィールロード: {profile.display_name}");
            DisplayProfile(profile);
        }

        /// <summary>
        /// プロフィール更新イベントハンドラー
        /// </summary>
        private void HandleProfileUpdated(PlayerDataService.PlayerProfile profile)
        {
            Debug.Log($"[PlayerDataSample] プロフィール更新: {profile.display_name}");
            DisplayProfile(profile);
        }

        /// <summary>
        /// データベースエラーハンドラー
        /// </summary>
        private void HandleDataError(string error)
        {
            Debug.LogError($"[PlayerDataSample] データエラー: {error}");
            SetStatusText($"エラー: {error}", Color.red);
        }

        /// <summary>
        /// プロフィール情報を画面に表示
        /// </summary>
        private void DisplayProfile(PlayerDataService.PlayerProfile profile)
        {
            string profileInfo = $"ユーザーID: {profile.user_id}\n" +
                                $"表示名: {profile.display_name}\n" +
                                $"スコア: {profile.score}\n" +
                                $"レベル: {profile.level}\n" +
                                $"作成日時: {profile.created_at}\n" +
                                $"更新日時: {profile.updated_at ?? "未更新"}";

            profileDataText.text = profileInfo;
            profileDataText.color = Color.white;

            // 入力フィールドに値をセット
            if (displayNameInput != null)
                displayNameInput.text = profile.display_name;
            if (scoreInput != null)
                scoreInput.text = profile.score.ToString();
            if (levelInput != null)
                levelInput.text = profile.level.ToString();
        }

        /// <summary>
        /// 認証状態をチェック
        /// </summary>
        private bool ValidateAuthentication()
        {
            if (!SupabaseManager.Instance.Auth.IsAuthenticated)
            {
                SetStatusText("エラー: 先にログインしてください", Color.red);
                profileDataText.text = "";
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
