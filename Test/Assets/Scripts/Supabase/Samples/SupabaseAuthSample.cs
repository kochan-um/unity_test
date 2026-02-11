using UnityEngine;
using UnityEngine.UI;
using Supabase;

namespace SupabaseSamples
{
    /// <summary>
    /// Supabase 認証機能のサンプル実装
    /// UI ボタンとテキスト入力で認証フローをテスト
    /// </summary>
    public class SupabaseAuthSample : MonoBehaviour
    {
        [SerializeField]
        private InputField emailInput;

        [SerializeField]
        private InputField passwordInput;

        [SerializeField]
        private Button signUpButton;

        [SerializeField]
        private Button signInButton;

        [SerializeField]
        private Button signOutButton;

        [SerializeField]
        private Text statusText;

        [SerializeField]
        private Text userInfoText;

        private void Start()
        {
            // UI ボタンにリスナーを登録
            signUpButton?.onClick.AddListener(OnSignUpClicked);
            signInButton?.onClick.AddListener(OnSignInClicked);
            signOutButton?.onClick.AddListener(OnSignOutClicked);

            // Supabase Manager のイベントをリッスン
            if (SupabaseManager.Instance != null && SupabaseManager.Instance.IsInitialized)
            {
                SupabaseManager.Instance.Auth.OnAuthSuccess += HandleAuthSuccess;
                SupabaseManager.Instance.Auth.OnAuthError += HandleAuthError;
                SupabaseManager.Instance.Auth.OnLogout += HandleLogout;

                UpdateUserInfo();
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
                SupabaseManager.Instance.Auth.OnAuthSuccess -= HandleAuthSuccess;
                SupabaseManager.Instance.Auth.OnAuthError -= HandleAuthError;
                SupabaseManager.Instance.Auth.OnLogout -= HandleLogout;
            }
        }

        /// <summary>
        /// サインアップボタンクリック
        /// </summary>
        private void OnSignUpClicked()
        {
            string email = emailInput?.text ?? "";
            string password = passwordInput?.text ?? "";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                SetStatusText("メールアドレスとパスワードを入力してください", Color.yellow);
                return;
            }

            SetStatusText("サインアップ中...", Color.yellow);
            SupabaseManager.Instance.Auth.SignUp(email, password, (success) =>
            {
                if (success)
                {
                    SetStatusText("サインアップ成功", Color.green);
                    ClearInputs();
                }
                else
                {
                    SetStatusText("サインアップ失敗", Color.red);
                }
            });
        }

        /// <summary>
        /// サインインボタンクリック
        /// </summary>
        private void OnSignInClicked()
        {
            string email = emailInput?.text ?? "";
            string password = passwordInput?.text ?? "";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                SetStatusText("メールアドレスとパスワードを入力してください", Color.yellow);
                return;
            }

            SetStatusText("ログイン中...", Color.yellow);
            SupabaseManager.Instance.Auth.SignIn(email, password, (success) =>
            {
                if (success)
                {
                    SetStatusText("ログイン成功", Color.green);
                    ClearInputs();
                    UpdateUserInfo();
                }
                else
                {
                    SetStatusText("ログイン失敗", Color.red);
                }
            });
        }

        /// <summary>
        /// サインアウトボタンクリック
        /// </summary>
        private void OnSignOutClicked()
        {
            SupabaseManager.Instance.Auth.SignOut();
            SetStatusText("ログアウト完了", Color.green);
            UpdateUserInfo();
        }

        /// <summary>
        /// 認証成功イベントハンドラー
        /// </summary>
        private void HandleAuthSuccess()
        {
            Debug.Log("[AuthSample] 認証成功");
            UpdateUserInfo();
        }

        /// <summary>
        /// 認証エラーイベントハンドラー
        /// </summary>
        private void HandleAuthError(string error)
        {
            Debug.LogError($"[AuthSample] 認証エラー: {error}");
            SetStatusText($"エラー: {error}", Color.red);
        }

        /// <summary>
        /// ログアウトイベントハンドラー
        /// </summary>
        private void HandleLogout()
        {
            Debug.Log("[AuthSample] ログアウト");
            ClearInputs();
        }

        /// <summary>
        /// ユーザー情報を更新
        /// </summary>
        private void UpdateUserInfo()
        {
            if (SupabaseManager.Instance.Auth.IsAuthenticated)
            {
                string userInfo = $"ユーザーID: {SupabaseManager.Instance.Auth.UserId}\n" +
                                 $"メール: {SupabaseManager.Instance.Auth.UserEmail}";
                userInfoText.text = userInfo;
                userInfoText.color = Color.green;
            }
            else
            {
                userInfoText.text = "未認証状態";
                userInfoText.color = Color.gray;
            }
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

        /// <summary>
        /// 入力フィールドをクリア
        /// </summary>
        private void ClearInputs()
        {
            if (emailInput != null)
                emailInput.text = "";
            if (passwordInput != null)
                passwordInput.text = "";
        }
    }
}
