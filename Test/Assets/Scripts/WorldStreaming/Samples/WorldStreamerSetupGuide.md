# WorldStreamer セットアップガイド

## 概要
WorldStreamerはUnity Addressable Asset Systemを使用したオープンワールドストリーミングシステムです。
プレイヤー位置に基づいてチャンク単位でアセットを自動ロード・アンロードします。

## セットアップ手順

### 1. Addressable Asset Systemのインストール
1. Window → TextMesh Pro → Import TMP Essential Resources
2. Window → Asset Management → Addressables → Groups
3. Addressablesウィンドウで初期設定を行う

### 2. WorldStreamerSettingsの作成
1. Assets/Resources フォルダを作成
2. 右クリック → Create → WorldStreaming → Settings
3. WorldStreamerSettings アセットを作成・配置

### 3. シーンにWorldStreamerを配置
1. 空のGameObjectを作成
2. WorldStreamer MonoBehaviourをアタッチ
3. Settings フィールドに上記で作成したアセットを割り当て
4. Player Transform フィールドにプレイヤーオブジェクトを割り当て（または Camera.main を使用）

### 4. チャンク分割（オプション）
1. Window → WorldStreaming → Chunk Splitter を開く
2. Chunk Size を設定（デフォルト: 100m）
3. Root Object にシーンのルートオブジェクトを割り当て
4. Split & Register をクリック

## 使用方法

### プレイヤースクリプトの実装例
```csharp
using UnityEngine;
using WorldStreaming;

public class MyPlayer : MonoBehaviour
{
    private void Update()
    {
        // プレイヤーの移動処理...
        // WorldStreamerが自動的にチャンクをロード/アンロードする
    }
}
```

### デバッグモードの有効化
WorldStreamerSettings で以下を有効化：
- **Enable Debug Gizmos**: Runtime情報をオーバーレイ表示
- **Enable Debug Logging**: コンソールに詳細ログを出力

## 推奨設定

### モバイル端末向け
- Chunk Size: 100m
- Load Radius: 2チャンク
- Memory Budget: 256MB
- Max Concurrent Loads: 2

### PC/コンソール向け
- Chunk Size: 100m
- Load Radius: 3-4チャンク
- Memory Budget: 512-1024MB
- Max Concurrent Loads: 3-4

## トラブルシューティング

### チャンクがロードされない
- Addressable Asset Settingsが初期化されているか確認
- チャンクアセットに正しいラベル（chunk_x_z形式）が付与されているか確認
- Enable Debug Logging を有効化してログを確認

### メモリ使用量が増え続ける
- Memory Budget を小さくしてテスト
- Min Protected Radius を適切に設定
- キャッシュサイズのバランスを調整

### ポップイン（急激な出現）が目立つ
- Enable Fade In を有効化
- Fade In Duration を増やす（0.5-1.0秒推奨）
- Load Radius を増やす

## パフォーマンス最適化

1. **LODの活用**: Medium/Low LODで遠景の描画負荷を軽減
2. **チャンクサイズの調整**: 大きすぎるとメモリスパイク、小さすぎるとロード頻度増加
3. **並行ロード数の制限**: フレームレート低下を防ぐため maxConcurrentLoads を調整
4. **メモリバジェットの監視**: GUIOverlay でリアルタイム監視

## 既知の制限事項

- 現在はAddressable Local Path のみサポート（リモートCDN対応は今後実装予定）
- LOD Medium/Low の詳細実装は今後の拡張予定
- マルチプレイヤー対応は未実装
