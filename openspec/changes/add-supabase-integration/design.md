## Context
Unity 6 (6000.3.1f1) WebGLプロジェクトにSupabaseバックエンドを接続する。
WebGL環境ではネイティブソケットが使えないため、すべての通信はHTTP（UnityWebRequest）ベースで行う必要がある。
既にPhoton PUNがリアルタイムマルチプレイヤー通信を担当しているため、Supabaseはデータ永続化と認証に特化する。

## Goals / Non-Goals
- **Goals:**
  - Supabaseへの安全な接続初期化
  - メール/パスワードによるユーザー認証
  - プレイヤーデータ（プロフィール・統計）のCRUD操作
  - Supabase Storageによるファイル操作（アップロード・ダウンロード・削除・一覧・署名付きURL）
  - WebGL・エディタ両環境での動作
  - APIキーのハードコード防止

- **Non-Goals:**
  - Supabase Realtimeのサブスクリプション（Photon PUNがリアルタイム通信を担当）
  - OAuth/ソーシャルログイン（初期フェーズではメール/パスワードのみ）
  - サーバーサイドのRow Level Security (RLS) ポリシー設計（Supabaseダッシュボード側の設定）

## Decisions

### クライアントライブラリの選択
- **決定:** supabase-csharp（公式C#クライアント）を使用する
- **代替案:**
  - UnityWebRequestで直接REST API呼び出し → ボイラープレートが多く、認証トークン管理が煩雑
  - サードパーティラッパー → メンテナンスの信頼性が低い
- **理由:** 公式サポート・型安全・認証フロー組み込み済み

### 設定管理の方式
- **決定:** ScriptableObjectベースの設定ファイル（`SupabaseSettings`）を使用する
- **代替案:**
  - 環境変数直読み → WebGL環境では`Environment.GetEnvironmentVariable`が動作しない
  - JSONファイル → Unity標準の設定管理パターンから外れる
- **理由:** Unityエディタとの親和性が高く、ビルド時に値を埋め込める。`.gitignore`で秘匿値を含むアセットを除外可能

### アーキテクチャパターン
- **決定:** シングルトンのSupabaseManagerをエントリーポイントとし、Auth/Database/Storageの各サービスを公開する
- **理由:** Unity MonoBehaviourのライフサイクルに適合し、シーンをまたいだ状態管理が容易

### ストレージのバケット設計
- **決定:** 用途別にバケットを分離する（例: `avatars`(public)、`user-data`(private)）
- **代替案:**
  - 単一バケットにパスで分離 → アクセス制御が複雑になる
- **理由:** バケット単位でpublic/privateを設定でき、RLSポリシーの管理がシンプル

## Risks / Trade-offs
- **WebGL CORS制約** → Supabaseはデフォルトで適切なCORSヘッダーを返すが、カスタムドメイン使用時は確認が必要
- **supabase-csharpのWebGL互換性** → 一部のSystem.Net依存がWebGLで動作しない可能性あり → UnityWebRequestベースのHTTPハンドラーに差し替える
- **Anon Keyの露出** → クライアントサイドで使用するAnon Keyは公開前提。RLSポリシーでデータ保護を担保する必要あり
- **パッケージサイズ増加** → supabase-csharpとその依存関係がWebGLビルドサイズに影響 → 初期検証で許容範囲か確認

## Migration Plan
1. Supabaseプロジェクトをダッシュボードで作成し、URL・Anon Keyを取得
2. supabase-csharp NuGetパッケージをUnityプロジェクトに導入（NuGetForUnity経由）
3. SupabaseSettings ScriptableObjectを作成し、接続情報を設定
4. SupabaseManagerの実装・テスト（エディタ内）
5. 認証フロー（サインアップ・ログイン・ログアウト）の実装・テスト
6. データベースCRUD操作の実装・テスト
7. StorageServiceの実装（アップロード・ダウンロード・削除・一覧・署名付きURL）
8. Supabaseダッシュボードでバケット作成（avatars: public、user-data: private）
9. WebGLビルドでの動作検証

## Open Questions
- Supabaseプロジェクトのリージョン選択（レイテンシ最適化のため）
- プレイヤーデータのテーブルスキーマ設計（ゲーム仕様に依存）
- 認証トークンのリフレッシュ間隔の最適値
