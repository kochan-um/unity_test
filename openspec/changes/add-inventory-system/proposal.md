# Change: インベントリシステムを追加する

## Why
現在のプロジェクトにはアイテム管理の仕組みがなく、プレイヤーがゲーム内でアイテムを取得・使用・管理する手段がない。
ScriptableObjectベースの柔軟なアイテム定義と、スタック・ドラッグ&ドロップ・ソート対応のフルUIを備えたインベントリシステムを実装することで、ゲームプレイの基盤を構築する。
永続化はSupabase連携で行い、マルチプレイヤー同期は将来対応とする。

## What Changes
- ScriptableObjectベースのアイテム定義システム（カスタムカテゴリ対応）を導入する
- スロットベースのインベントリデータ管理（追加・削除・スタック・移動・ソート）を実装する
- ドラッグ&ドロップ対応のインベントリUIを実装する
- アイテムのツールチップ（詳細表示）UIを実装する
- アイテム使用・破棄のアクションシステムを実装する
- カテゴリフィルタリング・ソート機能を実装する
- Supabaseとの連携によるインベントリデータの永続化（セーブ/ロード）を実装する
- アイテムのドロップ（ワールドへの配置）とピックアップの仕組みを実装する

## Impact
- Affected specs: `inventory-system`（新規）
- Affected code:
  - `Test/Assets/Scripts/Inventory/` — 新規スクリプト群（コアシステム）
  - `Test/Assets/Scripts/Inventory/UI/` — インベントリUI関連スクリプト
  - `Test/Assets/Scripts/Inventory/Data/` — ScriptableObject定義・アイテムデータ
  - `Test/Assets/Scripts/Inventory/Persistence/` — Supabase永続化レイヤー
  - `Test/Assets/Resources/Items/` — アイテム定義ScriptableObject群
  - `Test/Assets/Prefabs/UI/Inventory/` — インベントリUIプレハブ
  - `Test/Assets/Prefabs/Items/` — ワールドアイテムプレハブ
- Affected changes: `add-supabase-integration`（永続化レイヤーが依存）
