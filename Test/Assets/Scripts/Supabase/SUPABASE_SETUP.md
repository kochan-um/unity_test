# Supabase Unity統合 - セットアップガイド

このガイドでは、Unity プロジェクトに Supabase バックエンドを統合する手順を説明します。
認証、データベース、ストレージの全機能を利用できます。

---

## 目次

1. [Supabase プロジェクトの準備](#supabase-プロジェクトの準備)
2. [Unity プロジェクトへの統合](#unity-プロジェクトへの統合)
3. [認証機能の実装](#認証機能の実装)
4. [データベース操作](#データベース操作)
5. [ストレージ操作](#ストレージ操作)
6. [エラーハンドリング](#エラーハンドリング)
7. [トラブルシューティング](#トラブルシューティング)

---

## Supabase プロジェクトの準備

### 1. Supabase アカウント作成と プロジェクト作成

1. [Supabase 公式サイト](https://supabase.com) にアクセス
2. GitHub または Google アカウントでサインアップ
3. 新しいプロジェクトを作成
   - 地理的に最も近いリージョンを選択（レイテンシ最適化のため）
4. プロジェクト作成完了後、ダッシュボードで以下を確認：
   - **Project URL** : `https://[project-id].supabase.co`
   - **Anon Key** : `eyJhbGc...` で始まるキー

### 2. プレイヤープロフィールテーブルの作成

Supabase ダッシュボードの **SQL Editor** で以下を実行：

```sql
-- プレイヤープロフィールテーブル
CREATE TABLE public.player_profiles (
  id BIGSERIAL PRIMARY KEY,
  user_id UUID NOT NULL UNIQUE REFERENCES auth.users(id) ON DELETE CASCADE,
  display_name TEXT NOT NULL,
  score INTEGER DEFAULT 0,
  level INTEGER DEFAULT 1,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Row Level Security 有効化
ALTER TABLE public.player_profiles ENABLE ROW LEVEL SECURITY;

-- ポリシー：ユーザーは自分のデータのみアクセス可能
CREATE POLICY "Users can view their own profile"
  ON public.player_profiles
  FOR SELECT
  USING (auth.uid() = user_id);

CREATE POLICY "Users can update their own profile"
  ON public.player_profiles
  FOR UPDATE
  USING (auth.uid() = user_id);

CREATE POLICY "Users can insert their own profile"
  ON public.player_profiles
  FOR INSERT
  WITH CHECK (auth.uid() = user_id);
```

### 3. ストレージバケットの作成

Supabase ダッシュボードの **Storage** セクションで以下を作成：

#### バケット 1: `avatars` (Public)
- **バケット名** : `avatars`
- **公開設定** : Public
- **用途** : ユーザーアバター画像の保存

#### バケット 2: `user-data` (Private)
- **バケット名** : `user-data`
- **公開設定** : Private
- **用途** : ユーザー固有のプライベートデータ保存

---

## Unity プロジェクトへの統合

### 1. SupabaseSettings の作成

1. Project パネルで `Assets/Resources` フォルダを作成（既にあれば スキップ）
2. フォルダを右クリック → **Create** → **Supabase** → **Settings**
3. 作成された `SupabaseSettings` アセットをクリック
4. Inspector で以下を入力：
   - **Supabase Url** : Supabase ダッシュボードから取得したプロジェクト URL
   - **Anon Key** : Supabase ダッシュボードから取得した Anon Key
   - **Request Timeout** : 30（秒）
   - **Max Retry Attempts** : 3
   - **Max Upload Size MB** : 50

**⚠️ セキュリティ注意：**
- `SupabaseSettings.asset` を `.gitignore` に追加して、Git にコミットしないこと
- 本番環境では環境変数やセキュアなキー管理を使用すること

### 2. SupabaseManager の配置

1. Hierarchy パネルで新規 GameObject を作成
2. 名前を `SupabaseManager` に設定
3. **Add Component** → **SupabaseManager** を検索して追加
4. Inspector で Settings フィールドに先ほど作成した `SupabaseSettings` をドラッグ＆ドロップ
5. このGameObject は全シーンで必要なため、DontDestroyOnLoad により 削除されません

---

## 認証機能の実装

### ユーザーサインアップ

```csharp
using UnityEngine;
using Supabase;

public class AuthExample : MonoBehaviour
{
    public void OnSignUpButtonClicked(string email, string password)
    {
        SupabaseManager.Instance.Auth.SignUp(email, password, (success) =>
        {
            if (success)
            {
                Debug.Log("サインアップ成功");
                // サインアップ成功後の処理
            }
            else
            {
                Debug.Log("サインアップ失敗");
            }
        });
    }
}
```

### ユーザーログイン

```csharp
public void OnSignInButtonClicked(string email, string password)
{
    SupabaseManager.Instance.Auth.SignIn(email, password, (success) =>
    {
        if (success)
        {
            Debug.Log($"ログイン成功: {SupabaseManager.Instance.Auth.UserEmail}");
            // ログイン後の画面遷移など
        }
        else
        {
            Debug.Log("ログイン失敗");
        }
    });
}
```

### ユーザーログアウト

```csharp
public void OnLogoutButtonClicked()
{
    SupabaseManager.Instance.Auth.SignOut();
    Debug.Log("ログアウト完了");
}
```

### セッション状態の確認

```csharp
void Update()
{
    if (SupabaseManager.Instance.Auth.IsAuthenticated)
    {
        Debug.Log($"ユーザー: {SupabaseManager.Instance.Auth.UserEmail}");
    }
    else
    {
        Debug.Log("未認証");
    }
}
```

### 認証イベントのリスニング

```csharp
private void OnEnable()
{
    SupabaseManager.Instance.Auth.OnAuthSuccess += HandleAuthSuccess;
    SupabaseManager.Instance.Auth.OnAuthError += HandleAuthError;
    SupabaseManager.Instance.Auth.OnLogout += HandleLogout;
}

private void OnDisable()
{
    SupabaseManager.Instance.Auth.OnAuthSuccess -= HandleAuthSuccess;
    SupabaseManager.Instance.Auth.OnAuthError -= HandleAuthError;
    SupabaseManager.Instance.Auth.OnLogout -= HandleLogout;
}

private void HandleAuthSuccess()
{
    Debug.Log("認証成功");
}

private void HandleAuthError(string error)
{
    Debug.LogError($"認証エラー: {error}");
}

private void HandleLogout()
{
    Debug.Log("ログアウト完了");
}
```

---

## データベース操作

### プレイヤープロフィールの作成

```csharp
public void CreatePlayerProfile(string displayName)
{
    SupabaseManager.Instance.PlayerData.CreateProfile(displayName, (success) =>
    {
        if (success)
        {
            Debug.Log("プロフィール作成成功");
        }
        else
        {
            Debug.Log("プロフィール作成失敗");
        }
    });
}
```

### プレイヤープロフィールの取得

```csharp
public void GetPlayerProfile()
{
    SupabaseManager.Instance.PlayerData.GetProfile((profile) =>
    {
        if (profile != null)
        {
            Debug.Log($"プロフィール取得: {profile.display_name}");
            Debug.Log($"スコア: {profile.score}");
            Debug.Log($"レベル: {profile.level}");
        }
        else
        {
            Debug.Log("プロフィール取得失敗");
        }
    });
}
```

### プレイヤープロフィールの更新

```csharp
public void UpdatePlayerProfile(string newDisplayName, int newScore, int newLevel)
{
    var profile = new PlayerDataService.PlayerProfile
    {
        user_id = SupabaseManager.Instance.Auth.UserId,
        display_name = newDisplayName,
        score = newScore,
        level = newLevel
    };

    SupabaseManager.Instance.PlayerData.UpdateProfile(profile, (success) =>
    {
        if (success)
        {
            Debug.Log("プロフィール更新成功");
        }
        else
        {
            Debug.Log("プロフィール更新失敗");
        }
    });
}
```

### データベースイベントのリスニング

```csharp
private void OnEnable()
{
    SupabaseManager.Instance.PlayerData.OnProfileLoaded += HandleProfileLoaded;
    SupabaseManager.Instance.PlayerData.OnProfileUpdated += HandleProfileUpdated;
    SupabaseManager.Instance.PlayerData.OnDataError += HandleDataError;
}

private void OnDisable()
{
    SupabaseManager.Instance.PlayerData.OnProfileLoaded -= HandleProfileLoaded;
    SupabaseManager.Instance.PlayerData.OnProfileUpdated -= HandleProfileUpdated;
    SupabaseManager.Instance.PlayerData.OnDataError -= HandleDataError;
}

private void HandleProfileLoaded(PlayerDataService.PlayerProfile profile)
{
    Debug.Log($"プロフィールロード: {profile.display_name}");
}

private void HandleProfileUpdated(PlayerDataService.PlayerProfile profile)
{
    Debug.Log($"プロフィール更新: {profile.display_name}");
}

private void HandleDataError(string error)
{
    Debug.LogError($"データベースエラー: {error}");
}
```

---

## ストレージ操作

### ファイルのアップロード

```csharp
public void UploadAvatar(byte[] imageData, string fileName)
{
    string bucketName = "avatars";
    string filePath = $"{SupabaseManager.Instance.Auth.UserId}/{fileName}";

    SupabaseManager.Instance.Storage.UploadFile(
        bucketName, filePath, imageData,
        (success, message) =>
        {
            if (success)
            {
                Debug.Log($"アップロード成功: {message}");
            }
            else
            {
                Debug.Log($"アップロード失敗: {message}");
            }
        });
}
```

### ファイルのダウンロード

```csharp
public void DownloadFile(string bucketName, string filePath)
{
    SupabaseManager.Instance.Storage.DownloadFile(
        bucketName, filePath,
        (success, data) =>
        {
            if (success)
            {
                Debug.Log($"ダウンロード成功: {data.Length} バイト");
                // ダウンロードしたバイナリデータを処理
            }
            else
            {
                Debug.Log("ダウンロード失敗");
            }
        });
}
```

### ファイルの削除

```csharp
public void DeleteFile(string bucketName, string filePath)
{
    SupabaseManager.Instance.Storage.DeleteFile(
        bucketName, filePath,
        (success) =>
        {
            if (success)
            {
                Debug.Log("削除成功");
            }
            else
            {
                Debug.Log("削除失敗");
            }
        });
}
```

### ファイル一覧の取得

```csharp
public void ListFiles(string bucketName, string folderPath = "")
{
    SupabaseManager.Instance.Storage.ListFiles(
        bucketName, folderPath,
        (success, files) =>
        {
            if (success && files != null)
            {
                foreach (var file in files)
                {
                    Debug.Log($"ファイル: {file.name}, サイズ: {file.size} バイト");
                }
            }
            else
            {
                Debug.Log("ファイル一覧取得失敗");
            }
        });
}
```

### 署名付きURL の生成（Private バケット用）

```csharp
public void GenerateSignedUrl(string bucketName, string filePath, int expirationSeconds = 3600)
{
    SupabaseManager.Instance.Storage.GenerateSignedUrl(
        bucketName, filePath, expirationSeconds,
        (success, signedUrl) =>
        {
            if (success)
            {
                Debug.Log($"署名付きURL: {signedUrl}");
                // このURLを外部に共有可能（期間限定）
            }
            else
            {
                Debug.Log("署名付きURL生成失敗");
            }
        });
}
```

### ストレージイベントのリスニング

```csharp
private void OnEnable()
{
    SupabaseManager.Instance.Storage.OnFileUploaded += HandleFileUploaded;
    SupabaseManager.Instance.Storage.OnFileDownloaded += HandleFileDownloaded;
    SupabaseManager.Instance.Storage.OnFilesListed += HandleFilesListed;
    SupabaseManager.Instance.Storage.OnStorageError += HandleStorageError;
}

private void OnDisable()
{
    SupabaseManager.Instance.Storage.OnFileUploaded -= HandleFileUploaded;
    SupabaseManager.Instance.Storage.OnFileDownloaded -= HandleFileDownloaded;
    SupabaseManager.Instance.Storage.OnFilesListed -= HandleFilesListed;
    SupabaseManager.Instance.Storage.OnStorageError -= HandleStorageError;
}

private void HandleFileUploaded(string filePath)
{
    Debug.Log($"アップロード完了: {filePath}");
}

private void HandleFileDownloaded(byte[] data)
{
    Debug.Log($"ダウンロード完了: {data.Length} バイト");
}

private void HandleFilesListed(StorageService.StorageFile[] files)
{
    Debug.Log($"ファイル一覧取得: {files.Length} 個");
}

private void HandleStorageError(string error)
{
    Debug.LogError($"ストレージエラー: {error}");
}
```

---

## エラーハンドリング

### ネットワークエラーへの対応

システムはネットワーク障害時に自動的にリトライ（最大3回）を実行します：

- **5xx系エラー（サーバー障害）** → 自動リトライ
- **4xx系エラー（クライアント不正）** → リトライせず即座に失敗を返す
- **ネットワークタイムアウト** → 自動リトライ（デフォルト：30秒）

### コールバックでのエラー処理

```csharp
SupabaseManager.Instance.Auth.SignIn(email, password, (success) =>
{
    if (!success)
    {
        // エラー内容はOnAuthErrorイベントで通知されます
        Debug.LogError("ログイン失敗");
    }
});
```

### エラーイベントでの詳細情報取得

```csharp
SupabaseManager.Instance.Auth.OnAuthError += (errorMessage) =>
{
    // errorMessage には詳細なエラー内容が含まれます
    Debug.LogError($"認証エラー詳細: {errorMessage}");
};

SupabaseManager.Instance.PlayerData.OnDataError += (errorMessage) =>
{
    Debug.LogError($"データベースエラー詳細: {errorMessage}");
};

SupabaseManager.Instance.Storage.OnStorageError += (errorMessage) =>
{
    Debug.LogError($"ストレージエラー詳細: {errorMessage}");
};
```

---

## トラブルシューティング

### 「SupabaseSettings が見つかりません」というエラー

**原因**: SupabaseSettings アセットが `Assets/Resources/` フォルダにない

**解決方法**:
1. `Assets/Resources/` フォルダを作成（なければ）
2. **Create** → **Supabase** → **Settings** でアセットを作成
3. 必要な値（URL、Anon Key）を入力

### ログイン失敗エラー

**原因1**: メールアドレスまたはパスワードが誤っている
- Supabase ダッシュボードの **Authentication** で登録ユーザーを確認

**原因2**: Anon Key が間違っている
- Supabase ダッシュボードから正しいキーをコピーして再設定

**原因3**: CORS エラー（WebGL環境）
- Supabase はデフォルトで CORS 対応。カスタムドメイン使用時は CORS 設定を確認

### アップロード/ダウンロード失敗

**原因1**: ファイルサイズが大きすぎる
- デフォルトの最大サイズは 50MB。超過時はエラーで通知

**原因2**: バケット名やパスが誤っている
- ダッシュボードで確認し、正確なパスを指定

**原因3**: RLS ポリシーが設定されていない
- ダッシュボードの **Storage** セクションで RLS を確認

### WebGL ビルド時の通信エラー

**原因**: ネイティブ環境とは異なり、UnityWebRequest がブラウザの制限を受ける

**解決方法**:
1. Supabase URL が HTTPS であることを確認
2. ブラウザのコンソール（F12）で CORS エラーを確認
3. Supabase ダッシュボードで CORS 設定を確認

---

## セキュリティベストプラクティス

1. **Anon Key の保護**
   - SourceControl にコミットしない
   - 本番環境では環境変数から読み込む

2. **Row Level Security (RLS) の有効化**
   - データベース側でユーザーがアクセスできるレコードを制限

3. **ストレージバケットの公開設定**
   - 秘密情報は Private バケットに保存
   - 署名付きURL で期限付きアクセスを提供

4. **リトライロジック**
   - 自動リトライにより、一時的な障害に耐性を持たせる

---

## サポート・参考資料

- [Supabase 公式ドキュメント](https://supabase.com/docs)
- [Supabase Auth](https://supabase.com/docs/guides/auth)
- [Supabase Database](https://supabase.com/docs/guides/database)
- [Supabase Storage](https://supabase.com/docs/guides/storage)
- [Unity WebRequest ドキュメント](https://docs.unity3d.com/Manual/UnityWebRequest.html)

---

**最終更新**: 2026年2月

質問や問題がある場合は、プロジェクトのissueを作成してください。
