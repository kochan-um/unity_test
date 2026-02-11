## Context
プロジェクトには以下が既に存在する：
- **StarterAssets ThirdPersonController**: CharacterController + Input System ベースの三人称コントローラー（`StarterAssets/ThirdPersonController/Scripts/ThirdPersonController.cs`）
- **StarterAssetsInputs**: move, look, jump, sprint の入力ハンドラー（`StarterAssets/InputSystem/StarterAssetsInputs.cs`）
- **Photon PUN2**: PhotonNetwork, PhotonView, PhotonTransformView, PhotonAnimatorView 等が導入済み（`Assets/Photon/`配下）

ThirdPersonControllerは `CharacterController` と `PlayerInput` を `RequireComponent` で依存しており、直接編集せずにネットワーク対応させる必要がある。

## Goals / Non-Goals

### Goals
- シングルプレイヤー（Offline）とマルチプレイヤー（Online）をランタイムで切り替え可能にする
- 既存のThirdPersonControllerを一切改変しない（ラッパーパターンで対応）
- ローカルプレイヤーのみが入力・カメラを制御する（オーナーシップ制御）
- リモートプレイヤーの位置・回転・アニメーション状態をスムーズに同期する
- ロビー接続・ルーム管理の基本フローを提供する
- オフライン時もオンライン時と同じプレハブ・同じシーン構成で動作する

### Non-Goals
- ボイスチャット（Photon Voice は別途対応）
- マッチメイキングの高度なカスタマイズ（ELO、スキルベース等）
- サーバーサイド権威型の物理同期（PUN2はクライアント権威モデル）
- Supabase認証との統合（別提案で対応）

## Decisions

### Decision 1: ラッパーパターンによるThirdPersonController対応
- **選択**: ThirdPersonControllerを改変せず、NetworkPlayerControllerでラップする
- **理由**: StarterAssetsはUnity公式アセットであり、直接改変するとアップデート時に差分管理が困難になる。ラッパーパターンなら元コードを保持したまま拡張可能
- **実装方針**:
  - ローカルプレイヤー: ThirdPersonController + StarterAssetsInputs をそのまま有効化
  - リモートプレイヤー: ThirdPersonController + StarterAssetsInputs + PlayerInput を無効化し、PhotonTransformView/PhotonAnimatorViewで状態を反映

### Decision 2: Photon Offlineモードの活用
- **選択**: シングルプレイヤー時は `PhotonNetwork.OfflineMode = true` を使用する
- **理由**: オフラインモードを使うことで、`PhotonNetwork.Instantiate` や `photonView.IsMine` 等のPhoton APIがオフラインでもそのまま動作する。モード切り替え時のコード分岐を最小化できる
- **代替案**:
  - 完全な分岐（if online / if offline）: コード全体にモード分岐が散在し保守性低下
  - 独立した2つのプレハブ: 重複が多く、変更時の同期コスト大

### Decision 3: プレイヤープレハブの構成
- **選択**: 1つのNetworkPlayerプレハブに全コンポーネントを配置し、ローカル/リモートで有効・無効を切り替える
- **構成**:
  ```
  NetworkPlayer (Prefab, Resources/)
  ├── PhotonView
  ├── PhotonTransformView
  ├── PhotonAnimatorView
  ├── NetworkPlayerController (ラッパー)
  ├── ThirdPersonController (ローカルのみ有効)
  ├── StarterAssetsInputs (ローカルのみ有効)
  ├── PlayerInput (ローカルのみ有効)
  ├── CharacterController
  ├── Animator
  └── CinemachineCameraTarget (子オブジェクト)
  ```

### Decision 4: 同期対象と方式
- **選択**:
  - **位置・回転**: PhotonTransformView（補間あり）
  - **アニメーション**: PhotonAnimatorView（Speed, Grounded, Jump, FreeFall, MotionSpeed パラメータ）
  - **カスタム状態**: IPunObservable で追加データ送信（将来拡張用）
- **理由**: PUN2標準のViewコンポーネントを最大限活用し、実装量を最小化

### Decision 5: ロビー・ルーム管理の設計
- **選択**: MonoBehaviourPunCallbacksを継承したLobbyManagerで接続フローを管理
- **フロー**:
  ```
  ConnectToPhoton → OnConnectedToMaster → JoinLobby → OnJoinedLobby
  → CreateRoom / JoinRoom / JoinRandomRoom
  → OnJoinedRoom → SpawnPlayer
  → OnLeftRoom → Cleanup
  ```

## Architecture

```
NetworkModeManager (シングルトン, DontDestroyOnLoad)
├── 現在のモード管理 (Offline / Online)
├── モード切替メソッド
└── イベント通知 (OnModeChanged)

LobbyManager (MonoBehaviourPunCallbacks)
├── Photon接続管理
├── ルーム作成/参加/退出
├── ルームリスト更新通知
└── イベント通知 (OnRoomJoined, OnRoomLeft, etc.)

PlayerSpawnManager (MonoBehaviourPunCallbacks)
├── ローカルプレイヤー生成 (PhotonNetwork.Instantiate)
├── スポーンポイント管理
└── プレイヤー退出時のクリーンアップ

NetworkPlayerController (MonoBehaviourPun, IPunObservable)
├── ローカル/リモート判定 (photonView.IsMine)
├── コンポーネント有効/無効の切り替え
├── リモートプレイヤーの補間処理
└── カスタム状態同期
```

## Data Flow

```
[ローカルプレイヤー]
1. Input System → StarterAssetsInputs → ThirdPersonController
2. ThirdPersonController → CharacterController.Move() → Transform更新
3. PhotonTransformView → Transform変更をネットワーク送信
4. PhotonAnimatorView → Animatorパラメータをネットワーク送信

[リモートプレイヤー]
1. ネットワーク受信 → PhotonTransformView → Transform補間適用
2. ネットワーク受信 → PhotonAnimatorView → Animatorパラメータ適用
3. ThirdPersonController/StarterAssetsInputs/PlayerInput は無効化済み
```

## Risks / Trade-offs

- **CharacterControllerとPhotonTransformViewの競合**: ローカルプレイヤーではCharacterControllerが位置を制御するが、リモートプレイヤーではPhotonTransformViewが位置を設定する必要がある
  → 緩和策: リモートプレイヤーではCharacterControllerを無効化し、Transform直接更新に切り替え
- **ネットワーク遅延によるワープ**: 高レイテンシ環境でプレイヤーが瞬間移動して見える
  → 緩和策: PhotonTransformViewの補間設定を調整、移動予測の追加
- **オフラインモードからオンラインへのホットスイッチ**: シーン途中でのモード切替は状態リセットが必要
  → 緩和策: モード切替はメインメニュー/ロビーシーンでのみ許可する設計

## Open Questions
- 最大プレイヤー数のデフォルト値（4〜20人を想定）
- リモートプレイヤーのネームプレート表示の要否
- ホストマイグレーション（マスタークライアント切替）のハンドリング方針
