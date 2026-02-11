# Change: UnityとSupabaseの接続機能を追加する

## Why
現在のプロジェクトにはバックエンド永続化層がなく、ユーザー認証・プレイヤーデータの保存・ランキング等のサーバーサイド機能が実装できない。
Supabaseを導入することで、PostgreSQLデータベース・認証・リアルタイム通知・ストレージを統合的に利用でき、バックエンドインフラの構築コストを最小化できる。

## What Changes
- Supabase C#クライアント（supabase-csharp）をUnityプロジェクトに導入する
- Supabase接続の初期化・設定管理を行うマネージャークラスを実装する
- Supabase Authによるユーザー認証（サインアップ・ログイン・ログアウト・セッション管理）機能を実装する
- Supabase Database（PostgREST）によるプレイヤーデータのCRUD操作を実装する
- Supabase Storageによるファイルのアップロード・ダウンロード・削除・一覧取得・署名付きURL生成を実装する
- 環境変数によるSupabase URL・Anon Keyの安全な管理を実装する
- WebGL環境での動作互換性を確保する

## Impact
- Affected specs: `supabase-integration`（新規）
- Affected code:
  - `Test/Assets/Scripts/Supabase/` — 新規スクリプト群
  - `Test/Assets/Resources/` — 設定ファイル（ScriptableObject）
  - `Test/Packages/manifest.json` — NuGet/UPM依存追加
