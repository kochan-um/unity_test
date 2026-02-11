## Context
Unity WebGLゲームプロジェクトに管理画面APIを追加する。
ゲームクライアント（Unity）はSupabaseと直接通信するが、管理画面は安全にservice_role keyを使用する必要があるため、サーバーサイドAPI層が必要。
既存のVercelデプロイ基盤を活用し、Next.js App RouterのRoute Handlersで実装する。

### ステークホルダー
- ゲーム運用管理者（アイテム管理、プレイヤーサポート）
- 開発者（デバッグ、データ確認）
- Unity WebGLゲームクライアント（間接的にマスタデータを参照）

## Goals / Non-Goals

### Goals
- Next.js App Router（TypeScript）による型安全なREST APIの構築
- 管理者認証・認可の実装（Supabase Authベース、ロール制御）
- アイテムマスタ・プレイヤーデータのCRUD操作
- SSEによるリアルタイムイベント配信（プレイヤーオンライン状態、ゲーム統計）
- Vercelへのデプロイ（既存WebGLデプロイと共存）
- Zod によるリクエストバリデーション

### Non-Goals
- 管理画面のフロントエンド（UIコンポーネント）の実装（別proposalで対応）
- ゲームクライアントから直接呼び出すAPIの実装（ゲームクライアントはSupabase直接接続を維持）
- GraphQL対応（RESTで十分な規模）
- マイクロサービス分割（モノリスで開始）

## Decisions

### フレームワーク: Next.js App Router
- **決定:** Next.js 15 App RouterのRoute Handlersを使用
- **代替案:**
  - Express.js単体 → Vercelとの統合が複雑、サーバーレス非対応
  - Hono → 軽量だがNext.jsエコシステムの恩恵を受けられない
  - tRPC → 管理画面フロントエンドとの型共有に有利だが、REST APIとしての汎用性が下がる
- **理由:** Vercelネイティブサポート、App Routerの規約ベースルーティング、将来的な管理画面フロントエンドとの統合が容易

### ディレクトリ構成: monorepo内の別ディレクトリ
- **決定:** `admin/` ディレクトリにNext.jsプロジェクトを配置
- **代替案:**
  - 別リポジトリ → 管理が煩雑、Supabase型定義の共有が困難
  - リポジトリルート直下 → Unityプロジェクトとの混在で混乱
- **理由:** 同一リポジトリ内で型定義やスキーマを共有しつつ、ビルド・デプロイを分離できる

### 認証方式: Supabase Auth + サーバーサイドセッション
- **決定:** Supabase Authでログイン後、HTTPOnly Cookieでセッション管理
- **代替案:**
  - JWT Bearer Token → クライアントサイドでのトークン管理が必要、XSS脆弱性リスク
  - NextAuth.js → 追加依存が増え、Supabase Authとの二重管理になる
- **理由:** Supabase Authの認証基盤を再利用し、HTTPOnly Cookieで安全なセッション管理。管理者ロールはSupabaseのカスタムクレームで制御

### データアクセス: Supabase Admin Client
- **決定:** サーバーサイドでservice_role keyを使用したSupabase Admin Clientでデータアクセス
- **理由:** RLSをバイパスし、管理者として全データにアクセス可能。service_role keyはサーバーサイドのみで使用し、クライアントに露出しない

### リアルタイム通信: Server-Sent Events (SSE)
- **決定:** SSEでサーバーからクライアントへの一方向リアルタイム配信
- **代替案:**
  - WebSocket → Vercel Serverless Functionsでは永続接続が困難
  - Polling → リアルタイム性が低く、無駄なリクエストが発生
  - Supabase Realtime → クライアントサイドでの直接利用は可能だが、管理画面のサーバーサイド集約に適さない
- **理由:** Vercelのストリーミングレスポンスで実現可能、HTTPベースで追加インフラ不要

### バリデーション: Zod
- **決定:** ZodでAPIリクエスト/レスポンスのスキーマバリデーション
- **理由:** TypeScriptとの親和性が高く、型推論が自動的に効く。Next.js App Routerとの組み合わせ実績が豊富

## API設計

### エンドポイント一覧

```
POST   /api/auth/login          管理者ログイン
POST   /api/auth/logout         ログアウト
GET    /api/auth/session        セッション確認

GET    /api/items               アイテム一覧（ページネーション・フィルタ）
GET    /api/items/:id           アイテム詳細
POST   /api/items               アイテム作成
PUT    /api/items/:id           アイテム更新
DELETE /api/items/:id           アイテム削除

GET    /api/players             プレイヤー一覧（ページネーション・検索）
GET    /api/players/:id         プレイヤー詳細
GET    /api/players/:id/inventory  プレイヤーインベントリ
PATCH  /api/players/:id         プレイヤーデータ修正

GET    /api/dashboard/stats     ダッシュボード統計
GET    /api/dashboard/events    SSEリアルタイムイベント
```

### 認証・認可フロー

```
1. POST /api/auth/login (email + password)
2. Supabase Auth でユーザー認証
3. カスタムクレーム (app_metadata.role === 'admin') を検証
4. HTTPOnly Cookie にセッショントークンを設定
5. 以降のリクエストで Cookie からセッションを復元
6. Middleware で全 /api/* ルートを認証保護（/api/auth/login を除く）
```

## プロジェクト構成

```
admin/
├── src/
│   ├── app/
│   │   └── api/
│   │       ├── auth/
│   │       │   ├── login/route.ts
│   │       │   ├── logout/route.ts
│   │       │   └── session/route.ts
│   │       ├── items/
│   │       │   ├── route.ts          # GET (list), POST (create)
│   │       │   └── [id]/route.ts     # GET, PUT, DELETE
│   │       ├── players/
│   │       │   ├── route.ts          # GET (list)
│   │       │   └── [id]/
│   │       │       ├── route.ts      # GET, PATCH
│   │       │       └── inventory/route.ts  # GET
│   │       └── dashboard/
│   │           ├── stats/route.ts    # GET
│   │           └── events/route.ts   # GET (SSE)
│   ├── lib/
│   │   ├── supabase/
│   │   │   ├── admin.ts      # service_role client
│   │   │   └── types.ts      # DB型定義
│   │   ├── auth/
│   │   │   ├── session.ts    # セッション管理
│   │   │   └── middleware.ts # 認証ミドルウェア
│   │   └── validation/
│   │       └── schemas.ts    # Zodスキーマ
│   └── middleware.ts          # Next.js Middleware（認証チェック）
├── package.json
├── tsconfig.json
├── next.config.ts
└── .env.local                 # 環境変数（gitignore対象）
```

## Risks / Trade-offs

- **Vercel Serverless Cold Start** → 初回リクエスト時のレイテンシ増加。管理画面用途であれば許容範囲。必要に応じてVercel Edge Functionsに移行可能
- **SSEの接続制限** → Vercel Serverless Functionsのタイムアウト（デフォルト10秒、Pro: 300秒）。長時間接続にはVercel Pro プラン or ポーリングへのフォールバックが必要
- **service_role keyの管理** → 漏洩時にRLSバイパスされるリスク。Vercel環境変数で厳密に管理し、サーバーサイドのみで使用
- **Unity WebGLとのCORS** → 管理画面APIはゲームクライアントからの直接呼び出しを想定しないため、CORSは管理画面ドメインのみ許可

## Migration Plan
1. `admin/` ディレクトリにNext.jsプロジェクトを初期化（`create-next-app`）
2. Supabase Admin Clientの設定・接続確認
3. 認証API（login/logout/session）の実装
4. Next.js Middlewareによる認証保護の実装
5. アイテムマスタCRUD APIの実装
6. プレイヤーデータ管理APIの実装
7. ダッシュボード統計APIの実装
8. SSEリアルタイムイベント配信の実装
9. Vercelデプロイ設定（`admin/` をルートとする別プロジェクト or monorepo設定）
10. CI/CDパイプラインの拡張

## Open Questions
- Supabaseのテーブルスキーマが未確定（`add-supabase-integration` の進行に依存）
- 管理者ロールの付与方法（Supabaseダッシュボードで手動設定 or 招待フロー）
- Vercel Pro プランの利用可否（SSEのタイムアウト制限に影響）
- 管理画面フロントエンド（React UI）の実装時期と技術選定
