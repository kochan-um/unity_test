# Change: Next.js TypeScriptによる管理画面APIの追加

## Why
現在のプロジェクトはUnity WebGLゲームとSupabaseバックエンドの直接接続で構成されているが、ゲームデータの管理・運用に必要な管理画面が存在しない。
管理者がアイテムマスタの編集、プレイヤーデータの確認・修正、リアルタイムでのゲーム状態監視を行うためには、サーバーサイドで認証・認可を制御できるAPI層が必要である。
Next.js App RouterのAPI Routes（Route Handlers）を採用することで、既存のVercelデプロイ基盤を活用しながら、型安全なTypeScript APIを構築できる。

## What Changes
- Next.js App Routerプロジェクトをリポジトリルートに新規作成する（`admin/` ディレクトリ）
- 管理者認証API（ログイン・ログアウト・セッション管理）を実装する
- アイテムマスタCRUD APIを実装する（作成・取得・更新・削除・一覧）
- プレイヤーデータ管理API（プレイヤー一覧・詳細・インベントリ確認・データ修正）を実装する
- ゲーム統計・ダッシュボードAPI（アクティブプレイヤー数、アイテム統計等）を実装する
- Server-Sent Events（SSE）によるリアルタイムイベント配信エンドポイントを実装する
- Supabase Admin Client（service_role key）を使用したサーバーサイドデータアクセスを実装する
- Vercelへのデプロイ設定を追加する

## Impact
- Affected specs: `nextjs-admin-api`（新規）
- Affected code:
  - `admin/` — Next.js App Routerプロジェクト（新規）
  - `admin/src/app/api/` — API Route Handlers
  - `admin/src/lib/` — 共通ライブラリ（Supabaseクライアント、認証ユーティリティ）
  - `vercel.json` — マルチプロジェクト対応の設定変更
  - `.github/workflows/` — CI/CDパイプラインの拡張
- 依存する既存変更: `add-supabase-integration`（Supabaseテーブルスキーマに依存）
