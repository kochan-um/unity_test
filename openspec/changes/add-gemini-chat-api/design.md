## Context
既存の Next.js 管理画面APIに、Gemini APIを利用した対話型AIアシスタント機能を追加する。
管理者は自然言語でゲームデータに関する質問を行い、AIがSupabase上のデータを検索・分析して回答する。
画像入力にも対応し、スクリーンショットやアイテム画像を送信して分析を依頼できる。

### ステークホルダー
- ゲーム運用管理者（日常的な運用質問、データ調査）
- カスタマーサポート担当者（プレイヤーからの問い合わせ調査）
- 開発者（データ確認、デバッグ支援）

## Goals / Non-Goals

### Goals
- Gemini API（マルチモーダル）を使用した対話型チャットbot APIの構築
- テキスト＋画像入力への対応
- ストリーミングレスポンスによるリアルタイム応答表示
- Function Callingによるゲームデータ（アイテム・プレイヤー）の動的検索
- チャット履歴のSupabase永続化
- 既存の認証基盤による保護

### Non-Goals
- チャットUIフロントエンドの実装（別proposalで対応）
- プレイヤー向け（ゲーム内）チャットbot
- ファインチューニングやカスタムモデルのトレーニング
- 音声・動画入力への対応
- 自動アクション実行（データの更新・削除はAIが直接行わない。読み取り専用）

## Decisions

### SDK選択: @google/generative-ai
- **決定:** Google公式の `@google/generative-ai` TypeScript SDKを使用
- **代替案:**
  - Vertex AI SDK → GCPプロジェクトの設定が必要、Vercel Serverlessとの相性が不明確
  - REST API直接呼び出し → ストリーミング処理やマルチモーダルの実装が煩雑
  - LangChain → 抽象化レイヤーが過剰、依存関係が大幅に増加
- **理由:** 軽量、TypeScript型定義付き、ストリーミング・Function Calling・マルチモーダルをネイティブサポート

### モデル選択: gemini-2.0-flash（デフォルト、設定変更可）
- **決定:** デフォルトで `gemini-2.0-flash` を使用し、環境変数で変更可能にする
- **代替案:**
  - gemini-2.0-pro 固定 → コストが高い、管理用途ではflashで十分
  - モデル選択UIを実装 → 初期フェーズでは不要な複雑さ
- **理由:** Flash は低レイテンシ・低コストでマルチモーダル対応。Pro が必要な場面は環境変数で切り替え

### データアクセス方式: Function Calling
- **決定:** Gemini の Function Calling 機能でゲームデータにアクセス
- **代替案:**
  - プロンプトに全データ埋め込み → トークン制限に抵触、コスト高
  - RAG（Retrieval Augmented Generation）→ ベクトルDB導入が必要、初期フェーズでは過剰
  - 事前定義クエリのみ → 柔軟性が低い
- **理由:** Gemini が必要に応じて関数を呼び出し、最小限のデータを取得。トークン効率が良く、リアルタイムデータを返せる

### Function Calling で提供するツール
| ツール名 | 説明 | パラメータ |
|----------|------|------------|
| `search_items` | アイテム検索 | query, category?, rarity? |
| `get_item_detail` | アイテム詳細取得 | item_id |
| `search_players` | プレイヤー検索 | query |
| `get_player_detail` | プレイヤー詳細取得 | player_id |
| `get_player_inventory` | プレイヤーインベントリ取得 | player_id |
| `get_dashboard_stats` | ダッシュボード統計取得 | なし |

### チャット履歴: Supabase永続化
- **決定:** チャットセッション・メッセージをSupabaseの `chat_sessions` / `chat_messages` テーブルに保存
- **代替案:**
  - メモリ内のみ → サーバーレスではリクエスト間で状態が消える
  - ローカルストレージ（クライアント側）→ API層では制御不能
  - Redis → 追加インフラが必要
- **理由:** 既存のSupabase基盤を活用、会話の継続性とログの保持が可能

### ストリーミング: ReadableStream
- **決定:** Next.js Route HandlerでReadableStreamを返し、Server-Sent Events形式でストリーミング
- **理由:** Gemini SDKの`generateContentStream`と直接統合可能。Vercel Serverless Functionsでストリーミングレスポンスをサポート

### レート制限: インメモリ + Supabase
- **決定:** 短期制限はインメモリカウンター、長期制限（日次）はSupabaseで管理
- **代替案:**
  - Vercel KV（Redis）→ 追加コスト
  - Upstash Rate Limit → 外部依存
- **理由:** 管理画面の利用者数は限定的、インメモリで十分。日次制限のみSupabaseに永続化

## API設計

### エンドポイント一覧

```
POST   /api/chat/sessions              チャットセッション作成
GET    /api/chat/sessions              セッション一覧取得
GET    /api/chat/sessions/:id          セッション詳細（メッセージ履歴付き）
DELETE /api/chat/sessions/:id          セッション削除

POST   /api/chat/sessions/:id/messages メッセージ送信（ストリーミング応答）
```

### メッセージ送信リクエスト形式

```typescript
// POST /api/chat/sessions/:id/messages
{
  content: string;          // テキストメッセージ
  images?: {                // 画像添付（任意）
    data: string;           // base64エンコードデータ
    mimeType: string;       // "image/png" | "image/jpeg" | "image/webp"
  }[];
}
```

### ストリーミングレスポンス形式

```
data: {"type":"text","content":"回答テキストの断片..."}

data: {"type":"tool_call","name":"search_items","args":{"query":"レア武器"}}

data: {"type":"tool_result","name":"search_items","content":[...]}

data: {"type":"text","content":"検索結果によると..."}

data: {"type":"done","usage":{"promptTokens":150,"completionTokens":200}}
```

## プロジェクト構成（追加分）

```
admin/src/
├── app/api/chat/
│   └── sessions/
│       ├── route.ts                    # POST (create), GET (list)
│       └── [id]/
│           ├── route.ts                # GET (detail), DELETE
│           └── messages/route.ts       # POST (send + stream)
├── lib/
│   ├── gemini/
│   │   ├── client.ts           # Gemini クライアント初期化
│   │   ├── tools.ts            # Function Calling ツール定義
│   │   ├── tool-handlers.ts    # ツール実行ハンドラー
│   │   └── system-prompt.ts    # システムプロンプト定義
│   └── validation/
│       └── schemas.ts          # チャット用スキーマ追加
```

## Risks / Trade-offs

- **Gemini API レイテンシ** → ストリーミングレスポンスで体感速度を改善。Function Calling 往復でさらに遅延する可能性あり
- **Vercel Serverless タイムアウト** → Hobby: 10秒、Pro: 300秒。長い会話や複数ツール呼び出しでタイムアウトする可能性。Pro プランへのアップグレードまたはレスポンス分割で対応
- **API コスト** → Gemini Flash は比較的低コストだが、画像入力・長い会話でトークン消費が増加。レート制限で制御
- **データ漏洩リスク** → Function Calling でゲームデータを Gemini に送信する。個人情報（メールアドレス等）をマスクするフィルタの検討が必要
- **ハルシネーション** → AI が存在しないデータを生成する可能性。Function Calling の結果に基づく回答を促すシステムプロンプトで緩和

## Open Questions
- Gemini API キーの取得元（Google AI Studio vs Vertex AI）
- 画像のアップロードサイズ制限（Vercel のリクエストボディサイズ制限: Hobby 4.5MB）
- チャット履歴の保持期間（無期限 vs 自動削除）
- 将来的にプレイヤー向け（ゲーム内）チャットbotへの拡張可能性
