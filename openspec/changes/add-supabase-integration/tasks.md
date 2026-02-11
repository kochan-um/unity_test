## 1. 環境構築
- [ ] 1.1 Suabaseプロジェクトをダッシュボードで作成し、Project URL・Anon Keyを取得する
- [ ] 1.2 NuGetForUnityをUnityプロジェクトに導入する
- [ ] 1.3 supabase-csharp パッケージをNuGetForUnity経由でインストールする
- [ ] 1.4 WebGL互換のHTTPハンドラー（UnityWebRequest）の導入・設定を行う

## 2. 設定管理
- [ ] 2.1 `SupabaseSettings` ScriptableObjectクラスを作成する（URL, Anon Key フィールド）
- [ ] 2.2 `.gitignore`にSupabaseSettings実アセットの除外ルールを追加する
- [ ] 2.3 サンプル設定ファイル（`SupabaseSettings.example.asset`）を作成する

## 3. 接続初期化
- [ ] 3.1 `SupabaseManager` シングルトンクラスを実装する（初期化・クライアント管理）
- [ ] 3.2 設定未入力時のバリデーションとエラーログ出力を実装する
- [ ] 3.3 エディタ上で接続テストを実行し、正常に初期化されることを確認する

## 4. ユーザー認証
- [ ] 4.1 `AuthService`クラスを実装する（サインアップ・ログイン・ログアウト）
- [ ] 4.2 セッショントークンのローカル保存と自動復元機能を実装する
- [ ] 4.3 認証エラーのハンドリングとユーザーフィードバックを実装する
- [ ] 4.4 認証フローのテスト（サインアップ → ログイン → ログアウト → セッション復元）を行う

## 5. データベース操作
- [ ] 5.1 Supabaseダッシュボードで`player_profiles`テーブルを作成する（スキーマ定義）
- [ ] 5.2 `PlayerDataService`クラスを実装する（Create・Read・Update・Delete）
- [ ] 5.3 未認証時のアクセス拒否処理を実装する
- [ ] 5.4 CRUD操作のテストを行う

## 6. ファイルストレージ
- [ ] 6.1 Supabaseダッシュボードでバケットを作成する（`avatars`: public、`user-data`: private）
- [ ] 6.2 `StorageService`クラスを実装する（アップロード・ダウンロード・削除・一覧取得）
- [ ] 6.3 署名付きURL生成機能を実装する
- [ ] 6.4 アップロードサイズ制限（デフォルト50MB）のバリデーションを実装する
- [ ] 6.5 public/privateバケットのアクセス制御が正しく動作することをテストする

## 7. エラーハンドリング
- [ ] 7.1 タイムアウト処理（デフォルト30秒）を実装する
- [ ] 7.2 指数バックオフによるリトライ機構（最大3回）を実装する
- [ ] 7.3 4xx/5xxエラーの分類と適切な通知処理を実装する

## 8. WebGL互換性検証
- [ ] 8.1 WebGLビルドを実行し、Supabase通信（Auth・DB・Storage）が正常に動作することを確認する
- [ ] 8.2 ブラウザでCORSエラーが発生しないことを確認する
- [ ] 8.3 Vercelデプロイ環境でのエンドツーエンドテストを実施する

## 9. バリデーション
- [ ] 9.1 `openspec validate add-supabase-integration --strict` を通す
- [ ] 9.2 セットアップ手順をドキュメントに追記する
