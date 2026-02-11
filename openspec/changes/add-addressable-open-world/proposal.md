# Change: Addressableベースのオープンワールドストリーミングシステムを追加する

## Why
現在のプロジェクトにはワールドストリーミング機構がなく、大規模なオープンワールドマップを効率的にロード・アンロードする手段がない。
Unity Addressable Asset Systemを活用してワールドをチャンク（セル）単位で分割・非同期ロードすることで、メモリ消費を抑えつつシームレスなオープンワールド体験を実現する。

## What Changes
- ワールドをグリッドベースのチャンク（セル）に分割し、Addressableラベルで管理する仕組みを導入する
- プレイヤー位置に基づいてチャンクの自動ロード・アンロードを制御するWorldStreamerを実装する
- チャンクのロード優先度制御（距離ベース）と非同期ロードキューを実装する
- LOD（Level of Detail）切り替えによる遠景の軽量表示を実装する
- チャンク境界をまたぐオブジェクトの管理機構を実装する
- メモリバジェット監視とチャンクのキャッシュ・パージ制御を実装する
- Addressable Asset Systemのプロファイル設定（ローカル/リモート）に対応する
- エディタ上でのチャンク可視化ギズモとデバッグUIを提供する

## Impact
- Affected specs: `addressable-open-world`（新規）
- Affected code:
  - `Test/Assets/Scripts/WorldStreaming/` — 新規スクリプト群（コアシステム）
  - `Test/Assets/Scripts/WorldStreaming/Editor/` — エディタ拡張（チャンク分割ツール・ギズモ）
  - `Test/Assets/AddressableAssetsData/` — Addressable設定・グループ定義
  - `Test/Packages/manifest.json` — `com.unity.addressables` パッケージ追加
  - `Test/Assets/Resources/` — WorldStreamer設定ファイル（ScriptableObject）
