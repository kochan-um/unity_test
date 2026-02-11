# Supabase サンプルスクリプト ガイド

このフォルダには、Supabase 統合機能をテストするための 4 つのサンプルスクリプトが含まれています。

---

## 📝 サンプルスクリプト一覧

### 1. SupabaseAuthSample.cs
**用途**: ユーザー認証機能のテスト

**機能**:
- メール/パスワード でのサインアップ
- メール/パスワード でのログイン
- ログアウト
- セッション状態の表示

**セットアップ手順**:
1. シーンに新しい Canvas を作成
2. Canvas の下に以下の UI 要素を作成：
   - **InputField** (名前: EmailInput) - メールアドレス入力用
   - **InputField** (名前: PasswordInput) - パスワード入力用
   - **Button** (名前: SignUpButton) - サインアップ
   - **Button** (名前: SignInButton) - ログイン
   - **Button** (名前: SignOutButton) - ログアウト
   - **Text** (名前: StatusText) - ステータス表示
   - **Text** (名前: UserInfoText) - ユーザー情報表示

3. GameObject を作成し、SupabaseAuthSample スクリプトを追加
4. Inspector でそれぞれの UI 要素を割り当て
5. ゲームを実行

**テスト手順**:
```
1. メールアドレスを入力（例: test@example.com）
2. パスワードを入力（例: Password123!）
3. [SignUp] ボタンをクリック
4. サインアップ成功後、別のメールアドレスで再度 [SignUp] をテスト
5. 登録済みのメールアドレスで [SignIn] をテスト
6. ログイン成功を確認
7. [SignOut] をクリック
8. 未認証状態に戻ることを確認
```

**期待される結果**:
- ✓ サインアップ成功時：StatusText が「サインアップ成功」と表示
- ✓ ログイン成功時：ユーザー情報が表示
- ✓ ログアウト後：未認証状態に戻る

---

### 2. SupabasePlayerDataSample.cs
**用途**: プレイヤーデータ（CRUD操作）のテスト

**機能**:
- プレイヤープロフィール作成
- プレイヤープロフィール取得
- プレイヤープロフィール更新
- プロフィール情報の表示

**前提条件**:
- SupabaseAuthSample でログイン済みの状態

**セットアップ手順**:
1. シーンに新しい Canvas を作成
2. Canvas の下に以下の UI 要素を作成：
   - **InputField** (名前: DisplayNameInput) - 表示名入力用
   - **InputField** (名前: ScoreInput) - スコア入力用
   - **InputField** (名前: LevelInput) - レベル入力用
   - **Button** (名前: CreateProfileButton) - プロフィール作成
   - **Button** (名前: GetProfileButton) - プロフィール取得
   - **Button** (名前: UpdateProfileButton) - プロフィール更新
   - **Text** (名前: StatusText) - ステータス表示
   - **Text** (名前: ProfileDataText) - プロフィール情報表示

3. GameObject を作成し、SupabasePlayerDataSample スクリプトを追加
4. Inspector でそれぞれの UI 要素を割り当て

**テスト手順**:
```
1. SupabaseAuthSample でログイン
2. DisplayNameInput に「Player001」と入力
3. [Create Profile] をクリック
4. [Get Profile] をクリック
5. プロフィール情報が表示されることを確認
6. ScoreInput に「500」、LevelInput に「10」と入力
7. [Update Profile] をクリック
8. [Get Profile] で更新内容を確認
```

**期待される結果**:
- ✓ プロフィール作成時：StatusText が「プロフィール作成成功」と表示
- ✓ 取得時：表示名・スコア・レベルが表示
- ✓ 更新後：新しい値が反映

---

### 3. SupabaseStorageSample.cs
**用途**: ファイルストレージ操作のテスト

**機能**:
- ファイルアップロード
- ファイルダウンロード
- ファイル削除
- ファイル一覧取得
- 署名付きURL生成

**前提条件**:
- SupabaseAuthSample でログイン済みの状態
- Supabase ダッシュボードで `avatars` バケット (public) を作成

**セットアップ手順**:
1. シーンに新しい Canvas を作成
2. Canvas の下に以下の UI 要素を作成：
   - **InputField** (名前: BucketNameInput) - バケット名入力用
   - **InputField** (名前: FilePathInput) - ファイルパス入力用
   - **InputField** (名前: FileContentInput) - ファイル内容入力用
   - **InputField** (名前: ExpirationSecondsInput) - URL有効期限(秒)入力用
   - **Button** (名前: UploadButton) - アップロード
   - **Button** (名前: DownloadButton) - ダウンロード
   - **Button** (名前: DeleteButton) - 削除
   - **Button** (名前: ListButton) - 一覧取得
   - **Button** (名前: GenerateSignedUrlButton) - 署名付きURL生成
   - **Text** (名前: StatusText) - ステータス表示
   - **Text** (名前: FileListText) - ファイル一覧表示
   - **Text** (名前: DownloadedContentText) - ダウンロード内容/URL表示

3. GameObject を作成し、SupabaseStorageSample スクリプトを追加
4. Inspector でそれぞれの UI 要素を割り当て

**テスト手順**:
```
1. SupabaseAuthSample でログイン
2. BucketNameInput: "avatars"、FilePathInput: "test_001.txt"、FileContentInput: "Hello, Supabase!" と入力
3. [Upload] をクリック
4. [List] でアップロードされたファイルを確認
5. [Download] でダウンロード内容を確認
6. [Generate Signed URL] で署名付きURL を生成
7. [Delete] でファイルを削除
8. [List] で削除を確認
```

**期待される結果**:
- ✓ アップロード成功時：StatusText が成功メッセージを表示
- ✓ 一覧取得時：FileListText にファイル情報が表示
- ✓ ダウンロード時：DownloadedContentText に内容が表示
- ✓ 署名付きURL生成時：DownloadedContentText に URL が表示（Cyan色）
- ✓ 削除後：一覧から削除される

---

### 4. SupabaseIntegrationTest.cs
**用途**: 全機能の統合テスト（自動実行）

**機能**:
- 認証テスト（サインアップ・ログイン・ログアウト）
- データベーステスト（CRUD操作）
- ストレージテスト（アップロード・ダウンロード・削除・一覧取得）
- テスト結果を Console に出力

**セットアップ手順**:
1. GameObject を作成し、SupabaseIntegrationTest スクリプトを追加
2. Inspector の設定：
   - **Test Email** : テスト用メールアドレス（例: test@example.com）
   - **Test Password** : テスト用パスワード（例: password123）
   - **Test Display Name** : テスト用表示名（例: TestPlayer）
   - **Auto Run Tests** : チェック（自動実行）

3. ゲームを実行

**テスト方法**:
- **自動実行**: Auto Run Tests をチェックして実行
- **手動実行**: Inspector で [Run Integration Tests] ボタンをクリック

**期待される結果**:
```
Console に以下のように表示される：
========== Supabase 統合テスト開始 ==========
[Test] ✓ Supabase Manager 初期化完了

--- 認証テスト開始 ---
[Test] ✓ サインアップ成功: test@example.com
[Test] ✓ ユーザーID: xxx-xxx-xxx

--- プレイヤーデータテスト開始 ---
[Test] ✓ プロフィール作成成功: TestPlayer
[Test] ✓ プロフィール取得成功
  表示名: TestPlayer
  スコア: 0
  レベル: 1
[Test] ✓ プロフィール更新成功 (スコア: 1000, レベル: 5)

--- ストレージテスト開始 ---
[Test] ✓ ファイルアップロード成功
[Test] ✓ ファイル一覧取得成功: 1 個
[Test] ✓ ファイルダウンロード成功
[Test] ✓ ファイル削除成功

--- ログアウトテスト開始 ---
[Test] ✓ ログアウト成功

========== Supabase 統合テスト完了 ==========
```

---

## 🔧 セットアップの流れ

### 全体の流れ

```
1. Supabase プロジェクト準備
   ↓
2. SupabaseSettings の作成と設定
   ↓
3. Hierarchy に SupabaseManager を配置
   ↓
4. 各サンプルを個別にテスト
   - AuthSample（認証）
   - PlayerDataSample（データベース）
   - StorageSample（ストレージ）
   ↓
5. IntegrationTest で全機能の統合テスト
```

### チェックリスト

- [ ] Supabase ダッシュボードでプロジェクト作成
- [ ] Project URL と Anon Key をコピー
- [ ] SupabaseSettings.asset を作成し、値を入力
- [ ] Supabase ダッシュボードで player_profiles テーブルを作成
- [ ] Supabase ダッシュボードで avatars バケット (public) を作成
- [ ] SupabaseManager を Hierarchy に配置
- [ ] SupabaseAuthSample でサインアップ・ログインをテスト
- [ ] SupabasePlayerDataSample でプロフィール操作をテスト
- [ ] SupabaseStorageSample でファイル操作をテスト
- [ ] SupabaseIntegrationTest で全機能テスト

---

## 🐛 トラブルシューティング

### 「Supabase Manager が初期化されていません」エラー

**原因**: SupabaseSettings が見つからない、または URL・Anon Key が未入力

**解決方法**:
1. Assets/Resources/SupabaseSettings.asset が存在することを確認
2. Supabase ダッシュボードから正しい URL と Anon Key をコピー
3. Inspector で値を入力

### 「メールアドレスまたはパスワードが正しくありません」エラー

**原因**: ログイン情報が誤っている

**解決方法**:
1. サインアップしたメールアドレスとパスワードを確認
2. Supabase ダッシュボードの Authentication セクションでユーザーを確認
3. 新規メールアドレスで再度サインアップを試行

### ストレージアップロード失敗

**原因**: バケットが存在しない、または権限がない

**解決方法**:
1. Supabase ダッシュボードで avatars バケットが存在することを確認
2. バケット設定で Public に設定されていることを確認
3. バケット名が正確に入力されていることを確認（大文字小文字区別）

### WebGL ビルド時の CORS エラー

**原因**: ブラウザの CORS 制限

**解決方法**:
1. Supabase は CORS 対応。URL が正確に入力されていることを確認
2. ブラウザの Developer Tools (F12) -> Console でエラーメッセージを確認
3. Network タブでリクエスト・レスポンスを確認

---

## 📚 参考資料

- [SUPABASE_SETUP.md](../SUPABASE_SETUP.md) - 詳細セットアップガイド
- [Supabase 公式ドキュメント](https://supabase.com/docs)
- [Unity UI ドキュメント](https://docs.unity3d.com/Manual/UISystem.html)

---

## 💡 ヒント

- **複数シーンでテスト**: SupabaseManager は DontDestroyOnLoad されるため、複数シーンをまたいでテスト可能
- **Console を確認**: 各スクリプトは処理内容を Debug.Log に出力。Console タブで確認可能
- **イベントリスニング**: 各サンプルは OnSuccess・OnError イベントをリッスンして処理を実行

---

質問や問題がある場合は、Console の出力を確認して、エラーメッセージを参考にしてください。
