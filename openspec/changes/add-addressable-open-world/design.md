## Context
Unityでオープンワールドを実現するには、広大なマップを一度にロードするのではなく、プレイヤー周辺のエリアだけを動的にロード・アンロードする「ワールドストリーミング」が必要となる。Unity Addressable Asset Systemはアセットの非同期ロード・アンロード・メモリ管理を提供しており、この用途に最適である。

### 前提条件
- Unity 2021.3 LTS以降（Addressables 1.19+）
- プロジェクトは既にSupabase連携が進行中（データ永続化に活用可能）
- WebGL対応が求められる場合、リモートアセットバンドルのCDN配信を考慮

## Goals / Non-Goals

### Goals
- プレイヤー位置に応じたシームレスなチャンクロード・アンロード
- メモリ使用量を一定範囲内に収めるバジェット管理
- エディタ上でのチャンク分割・設定の効率的なワークフロー
- ローカルビルドとリモートCDN配信の両対応
- LODによる遠景の軽量描画

### Non-Goals
- プロシージャル地形生成（チャンクの中身は事前に作成済みのアセットを前提）
- マルチプレイヤー同期（別提案で対応）
- ナビメッシュの動的ベイク（静的ナビメッシュをチャンクに含める形で対応）

## Decisions

### Decision 1: グリッドベースのチャンク分割
- **選択**: 固定サイズの正方形グリッドでワールドを分割する
- **理由**: 実装がシンプルで、座標からチャンクIDを O(1) で算出できる。プレイヤー位置からロードすべきチャンクの特定が高速
- **代替案**:
  - 四分木（Quadtree）分割: 密度に応じた可変サイズ。実装が複雑になり、初期段階ではオーバーエンジニアリング
  - シーンベース分割: Unity Sceneを単位にする。Addressableとの統合が煩雑で粒度制御が困難

### Decision 2: Addressableラベルによるチャンク管理
- **選択**: 各チャンクのアセットに `chunk_x_z` 形式のAddressableラベルを付与し、ラベル単位でロード・アンロードする
- **理由**: Addressableのグループ・ラベル機能をそのまま活用でき、バンドルの粒度制御が容易
- **代替案**:
  - Addressable Addressキー直指定: 柔軟だがバッチロードしにくい
  - AssetBundle直接操作: Addressableの利点（参照カウント、依存解決）を失う

### Decision 3: 距離ベースの優先度キュー
- **選択**: プレイヤーからの距離に応じてロード優先度を動的に決定するキューシステム
- **理由**: プレイヤー移動方向の先読みが可能で、ポップイン（突然の出現）を軽減できる
- **実装**: MinHeap（優先度キュー）でチャンク距離をキーとしてロード順序を管理

### Decision 4: LODレベルの構成
- **選択**: 3段階のLOD（High / Medium / Low）をチャンクごとに用意
  - High: プレイヤー周囲（loadRadius内）— フルディテール
  - Medium: 中距離（loadRadius〜lodMediumRadius）— 簡略メッシュ+テクスチャ
  - Low: 遠距離（lodMediumRadius〜lodLowRadius）— インポスター or 最小メッシュ
- **理由**: 3段階が視覚品質とメモリのバランスとして妥当。段階が多すぎるとアセット管理コストが増大

### Decision 5: メモリバジェット管理
- **選択**: ScriptableObjectで設定可能なメモリバジェット上限を定義し、超過時にLRU（Least Recently Used）方式で遠方チャンクからアンロード
- **理由**: プラットフォームごとにバジェットを変更でき、ランタイムでの動的調整も可能

## Architecture

```
WorldStreamer (MonoBehaviour, シングルトン)
├── ChunkGrid — チャンク座標系の管理、座標⇔チャンクID変換
├── ChunkLoader — Addressable非同期ロード/アンロードの実行
│   ├── LoadQueue (Priority Queue) — 距離ベースの優先度キュー
│   └── ChunkCache (Dictionary) — ロード済みチャンクのキャッシュ
├── LODController — 距離に応じたLODレベル切り替え
├── MemoryBudgetMonitor — メモリ使用量の監視とパージトリガー
└── WorldStreamerSettings (ScriptableObject) — 全設定値

Editor拡張:
├── ChunkSplitterWindow — シーン上のオブジェクトをチャンクに自動分割
├── ChunkGizmoDrawer — Sceneビューでチャンク境界を可視化
└── WorldStreamerInspector — カスタムInspector
```

## Data Flow

```
1. Update() → プレイヤー位置取得
2. ChunkGrid.GetSurroundingChunks(position, loadRadius)
   → ロード対象チャンクIDリスト
3. 差分計算: 新規ロード対象 / アンロード対象を算出
4. LoadQueue に新規チャンクを距離順で追加
5. ChunkLoader が毎フレーム maxConcurrentLoads 件まで非同期ロード実行
6. ロード完了 → ChunkCache に登録、LODController で初期LOD設定
7. アンロード対象 → Addressables.ReleaseInstance → ChunkCache から除去
8. MemoryBudgetMonitor が閾値超過検知 → LRUで追加パージ
```

## Risks / Trade-offs

- **ポップイン**: ロードが間に合わない場合にオブジェクトが突然出現する
  → 緩和策: 先読み距離の調整、フェードイン演出、LODによる段階的表示
- **メモリスパイク**: 高速移動時に大量チャンクが同時ロードされる
  → 緩和策: maxConcurrentLoads による並行数制限、メモリバジェットモニター
- **チャンク境界アーティファクト**: チャンク境界でオブジェクトが途切れる
  → 緩和策: 境界オーバーラップ領域の設定、境界オブジェクトの両チャンク参照
- **初回ロード時間**: Addressableカタログの初期化に時間がかかる
  → 緩和策: ローディング画面での事前初期化、カタログキャッシュ

## Migration Plan
- 既存のシーンアセットはそのまま維持し、新規にWorldStreamingシステムを追加する形で導入
- 既存シーンからチャンク分割を行うエディタツールを提供し、段階的に移行可能
- Addressablesパッケージの追加が必要（`com.unity.addressables`）
- ロールバック: WorldStreamerコンポーネントを無効化するだけで従来のシーンロードに戻せる

## Open Questions
- チャンクサイズのデフォルト値（100m × 100m を仮定しているが、ゲーム規模による）
- リモートアセットバンドルのCDN選定（WebGL対応時）
- ナビメッシュのチャンク間接続方法
