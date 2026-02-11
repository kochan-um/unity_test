## 1. Gemini クライアント基盤
- [ ] 1.1 `@google/generative-ai` パッケージをインストール
- [ ] 1.2 `.env.local` に `GEMINI_API_KEY` と `GEMINI_MODEL`（デフォルト: gemini-2.0-flash）を追加
- [ ] 1.3 `src/lib/gemini/client.ts` — Gemini クライアントの初期化（シングルトン、環境変数からAPI Key取得）
- [ ] 1.4 `src/lib/gemini/system-prompt.ts` — ゲーム管理アシスタント用のシステムプロンプトを定義

## 2. Function Calling ツール
- [ ] 2.1 `src/lib/gemini/tools.ts` — Function Calling のツール定義（search_items, get_item_detail, search_players, get_player_detail, get_player_inventory, get_dashboard_stats）
- [ ] 2.2 `src/lib/gemini/tool-handlers.ts` — 各ツールの実行ハンドラー実装（既存の Supabase Admin Client を使用してデータ取得）
- [ ] 2.3 個人情報マスク処理（メールアドレス等をGeminiに送信する前にフィルタ）

## 3. チャット履歴永続化
- [ ] 3.1 `src/lib/supabase/types.ts` に `chat_sessions` と `chat_messages` テーブルの型定義を追加
- [ ] 3.2 Supabase で `chat_sessions` テーブル作成（id, admin_user_id, title, created_at, updated_at）
- [ ] 3.3 Supabase で `chat_messages` テーブル作成（id, session_id, role, content, images, tool_calls, tool_results, created_at）

## 4. バリデーションスキーマ
- [ ] 4.1 `src/lib/validation/schemas.ts` にチャット用 Zod スキーマを追加（createSessionSchema, sendMessageSchema, listSessionsQuerySchema）
- [ ] 4.2 画像添付の Zod スキーマ（base64データ、mimeType、サイズ上限チェック）

## 5. チャットセッション API
- [ ] 5.1 `POST /api/chat/sessions` — 新規セッション作成
- [ ] 5.2 `GET /api/chat/sessions` — セッション一覧取得（ページネーション、管理者自身のセッションのみ）
- [ ] 5.3 `GET /api/chat/sessions/[id]` — セッション詳細取得（メッセージ履歴付き）
- [ ] 5.4 `DELETE /api/chat/sessions/[id]` — セッション削除

## 6. メッセージ送信 API（ストリーミング）
- [ ] 6.1 `POST /api/chat/sessions/[id]/messages` — メッセージ送信エンドポイント実装
- [ ] 6.2 マルチモーダル入力処理（テキスト＋base64画像をGemini用パーツに変換）
- [ ] 6.3 Gemini `generateContentStream` によるストリーミングレスポンス実装
- [ ] 6.4 Function Calling のリクエスト・レスポンスループ実装（ツール呼び出し→結果取得→Geminiに返送）
- [ ] 6.5 ストリーミングイベントのフォーマット実装（text, tool_call, tool_result, done イベント）
- [ ] 6.6 チャットメッセージの Supabase 永続化（ユーザーメッセージ送信時＋AI応答完了時）

## 7. レート制限
- [ ] 7.1 インメモリレート制限の実装（1分あたりのリクエスト上限）
- [ ] 7.2 日次トークン消費量の Supabase 記録・制限チェック

## 8. デプロイ・設定
- [ ] 8.1 Vercel 環境変数に `GEMINI_API_KEY` を追加
- [ ] 8.2 `.env.local.example` を更新（Gemini 関連の変数を追加）
- [ ] 8.3 本番環境での動作確認（ストリーミング、Function Calling、画像入力）
