# Change: Gemini APIを使用した管理者向けAIチャットbot機能の追加

## Why
管理画面でアイテムデータやプレイヤー情報を確認する際、SQLクエリやAPI操作に慣れていない運用担当者にとって必要な情報へのアクセスが難しい。
Gemini APIのマルチモーダル機能を活用した対話型AIアシスタントを導入することで、自然言語やスクリーンショットでの質問から即座にゲームデータの検索・分析・レポート生成が可能になる。

## What Changes
- Google Generative AI SDK（`@google/generative-ai`）を `admin/` プロジェクトに追加する
- Gemini クライアントの初期化・設定管理を行うライブラリモジュールを実装する
- チャットAPI エンドポイント（メッセージ送信・ストリーミング応答）を実装する
- マルチモーダル入力（テキスト＋画像）に対応したメッセージ処理を実装する
- ゲームデータ（アイテム・プレイヤー）をコンテキストとして Gemini に提供する Function Calling 機能を実装する
- チャット履歴の永続化（Supabase）を実装する
- レート制限を実装する

## Impact
- Affected specs: `gemini-chat-api`（新規）
- Affected code:
  - `admin/src/app/api/chat/` — チャットAPI Route Handlers（新規）
  - `admin/src/lib/gemini/` — Gemini クライアント・ツール定義（新規）
  - `admin/src/lib/validation/schemas.ts` — チャット用 Zod スキーマ追加
  - `admin/src/lib/supabase/types.ts` — チャット履歴テーブル型追加
- 依存する既存変更: `add-nextjs-admin-api`（認証基盤・API構造に依存）
