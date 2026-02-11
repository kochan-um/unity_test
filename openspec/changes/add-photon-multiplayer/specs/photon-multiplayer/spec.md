## ADDED Requirements

### Requirement: ネットワークモード管理
システムはオフラインモード（シングルプレイヤー）とオンラインモード（マルチプレイヤー）の2つのネットワークモードを管理し、ランタイムで切り替え可能でなければならない（SHALL）。
モード切替はメインメニューまたはロビーシーンからのみ実行可能でなければならない（SHALL）。

#### Scenario: オフラインモードで起動する
- **WHEN** ゲームが起動する
- **AND** NetworkModeManagerのデフォルトモードがOfflineに設定されている
- **THEN** `PhotonNetwork.OfflineMode` が true に設定される
- **AND** Photonサーバーへの接続は行われない
- **AND** プレイヤーはローカルで即座にプレイ可能になる

#### Scenario: オフラインからオンラインに切り替える
- **WHEN** プレイヤーがロビーシーンでオンラインモードを選択する
- **THEN** `PhotonNetwork.OfflineMode` が false に設定される
- **AND** Photonサーバーへの接続が開始される
- **AND** `OnModeChanged(NetworkMode.Online)` イベントが発行される

#### Scenario: オンラインからオフラインに切り替える
- **WHEN** プレイヤーがオンラインモードからオフラインモードに切り替える
- **THEN** 現在のルームから退出する
- **AND** Photonサーバーから切断される
- **AND** `PhotonNetwork.OfflineMode` が true に設定される
- **AND** `OnModeChanged(NetworkMode.Offline)` イベントが発行される

#### Scenario: ゲームプレイ中のモード切替を禁止する
- **WHEN** プレイヤーがゲームプレイシーン内でモード切替を試みる
- **THEN** 切替は実行されない
- **AND** 警告メッセージがログに出力される

---

### Requirement: Photonロビー・ルーム管理
システムはPhotonサーバーへの接続、ロビー参加、ルームの作成・参加・退出を管理しなければならない（SHALL）。
ルームリストの更新をコールバックで通知しなければならない（SHALL）。

#### Scenario: Photonサーバーに接続する
- **WHEN** LobbyManagerのConnectメソッドが呼ばれる
- **AND** ネットワークモードがOnlineである
- **THEN** PhotonNetwork.ConnectUsingSettings() が実行される
- **AND** 接続成功時に OnConnectedToMaster コールバックが発火する

#### Scenario: ルームを作成する
- **WHEN** プレイヤーがルーム名と最大人数を指定してルーム作成を要求する
- **THEN** 指定されたルーム名でルームが作成される
- **AND** 作成したプレイヤーがマスタークライアントとなる
- **AND** OnRoomJoined イベントが発行される

#### Scenario: 既存のルームに参加する
- **WHEN** プレイヤーがルーム名を指定してルーム参加を要求する
- **AND** 指定されたルームが存在し、空きがある
- **THEN** プレイヤーがルームに参加する
- **AND** OnRoomJoined イベントが発行される

#### Scenario: ランダムルームに参加する
- **WHEN** プレイヤーがランダム参加を要求する
- **AND** 参加可能なルームが存在する
- **THEN** 空きのあるルームにランダムに参加する
- **AND** OnRoomJoined イベントが発行される

#### Scenario: ランダム参加で空きルームがない場合
- **WHEN** プレイヤーがランダム参加を要求する
- **AND** 参加可能なルームが存在しない
- **THEN** 新しいルームが自動的に作成される
- **AND** OnRoomJoined イベントが発行される

#### Scenario: ルームから退出する
- **WHEN** プレイヤーがルーム退出を要求する
- **THEN** PhotonNetwork.LeaveRoom() が実行される
- **AND** ローカルプレイヤーオブジェクトが破棄される
- **AND** OnRoomLeft イベントが発行される

#### Scenario: オフラインモードでのルーム参加
- **WHEN** ネットワークモードがOfflineである
- **AND** ルーム参加が要求される
- **THEN** `PhotonNetwork.OfflineMode` のオフラインルームに自動参加する
- **AND** OnRoomJoined イベントが発行される

---

### Requirement: ネットワークプレイヤー生成管理
システムはルーム参加時にネットワーク対応プレイヤーを生成し、退出時にクリーンアップしなければならない（SHALL）。
スポーンポイントの管理を提供しなければならない（SHALL）。
autoSpawnOnJoin設定が有効な場合、ルーム参加時に自動でプレイヤーを生成しなければならない（SHALL）。

#### Scenario: 自動スポーンによるプレイヤー生成
- **WHEN** OnJoinedRoom コールバックが発火する
- **AND** autoSpawnOnJoin 設定が true である
- **THEN** PlayerSpawnManagerがスポーンポイントからNetworkPlayerプレハブを `PhotonNetwork.Instantiate` で自動生成する
- **AND** 生成されたプレイヤーにローカル/リモートの初期化処理が実行される

#### Scenario: 自動スポーンが無効な場合
- **WHEN** OnJoinedRoom コールバックが発火する
- **AND** autoSpawnOnJoin 設定が false である
- **THEN** プレイヤーは自動生成されない
- **AND** 外部から SpawnLocalPlayer() を明示的に呼び出すことで生成できる

#### Scenario: スポーンポイントの選択
- **WHEN** プレイヤーを生成する
- **AND** 複数のスポーンポイントがシーンに配置されている
- **THEN** 他プレイヤーと重複しないスポーンポイントが優先的に選択される

#### Scenario: 他プレイヤーの退出時のクリーンアップ
- **WHEN** リモートプレイヤーがルームから退出する
- **THEN** そのプレイヤーのネットワークオブジェクトが自動的に破棄される（PUN2標準動作）

#### Scenario: リスポーン
- **WHEN** RespawnLocalPlayer() が呼び出される
- **THEN** 既存のローカルプレイヤーが破棄される
- **AND** 新しいスポーンポイントにプレイヤーが再生成される

---

### Requirement: ネットワークプレイヤーコントローラー（ローカル/リモート切り替え）
システムはPhotonViewのオーナーシップに基づいて、ローカルプレイヤーとリモートプレイヤーのコンポーネント有効状態を自動的に制御しなければならない（SHALL）。
既存のThirdPersonControllerを改変してはならない（SHALL）。

#### Scenario: ローカルプレイヤーの初期化
- **WHEN** NetworkPlayerControllerが初期化される
- **AND** `photonView.IsMine` が true である
- **THEN** 以下のコンポーネントが有効化される:
  - ThirdPersonController
  - StarterAssetsInputs
  - PlayerInput
  - CharacterController
  - AudioListener（存在する場合）
- **AND** Cinemachineバーチャルカメラのフォローターゲットがこのプレイヤーに設定される

#### Scenario: リモートプレイヤーの初期化
- **WHEN** NetworkPlayerControllerが初期化される
- **AND** `photonView.IsMine` が false である
- **THEN** 以下のコンポーネントが無効化される:
  - ThirdPersonController
  - StarterAssetsInputs
  - PlayerInput
  - CharacterController
- **AND** AudioListenerが無効化される
- **AND** PhotonTransformViewとPhotonAnimatorViewがネットワーク同期を受信する

#### Scenario: オフラインモードでのローカルプレイヤー
- **WHEN** ネットワークモードがOfflineである
- **AND** NetworkPlayerControllerが初期化される
- **THEN** `photonView.IsMine` は true を返す（Photon OfflineMode動作）
- **AND** ローカルプレイヤーと同様にすべてのコンポーネントが有効化される

---

### Requirement: プレイヤー状態のネットワーク同期
システムはローカルプレイヤーの位置・回転・アニメーション状態をPhotonネットワーク経由でリモートプレイヤーに同期しなければならない（SHALL）。
リモートプレイヤーの表示は補間処理により滑らかでなければならない（SHALL）。

#### Scenario: 位置と回転の同期
- **WHEN** ローカルプレイヤーが移動する
- **THEN** PhotonTransformViewがTransformの位置と回転をネットワーク送信する
- **AND** リモート側でPhotonTransformViewが受信した値を補間して適用する

#### Scenario: アニメーション状態の同期
- **WHEN** ローカルプレイヤーのAnimatorパラメータが変更される
- **THEN** PhotonAnimatorViewが以下のパラメータをネットワーク送信する:
  - Speed (Float)
  - Grounded (Bool)
  - Jump (Bool)
  - FreeFall (Bool)
  - MotionSpeed (Float)
- **AND** リモート側でAnimatorの対応するパラメータが更新される

#### Scenario: リモートプレイヤーのスムーズな移動表示
- **WHEN** リモートプレイヤーの位置情報をネットワーク受信する
- **THEN** 前回位置から新しい位置への補間が適用される
- **AND** ネットワーク遅延による瞬間移動（ワープ）が軽減される

---

### Requirement: ネットワーク設定（ScriptableObject）
システムはPhotonマルチプレイヤーの設定をScriptableObjectとして管理し、エディタ上で変更可能でなければならない（SHALL）。

#### Scenario: 設定項目の定義
- **WHEN** NetworkSettingsアセットを作成する
- **THEN** 以下の項目が設定可能である:
  - defaultMode（デフォルトネットワークモード: Offline/Online）
  - maxPlayersPerRoom（ルーム最大人数、デフォルト: 8）
  - playerPrefabName（Resourcesフォルダ内のプレハブ名）
  - autoJoinLobby（接続時に自動でロビーに参加するか、デフォルト: true）
  - enableVoiceChat（ボイスチャット有効化フラグ、デフォルト: false / 将来用）
  - photonSendRate（送信レート、デフォルト: 20）
  - photonSerializationRate（シリアライゼーションレート、デフォルト: 10）

---

### Requirement: ネットワークイベント通知
システムはネットワーク状態の変更をC#イベント（Action）で通知し、UIや他システムが購読可能でなければならない（SHALL）。

#### Scenario: ルーム参加の通知
- **WHEN** プレイヤーがルームに参加する
- **THEN** `OnRoomJoined` イベントが発行される
- **AND** 購読しているUIコンポーネントがルーム情報を表示できる

#### Scenario: 他プレイヤーの入退出通知
- **WHEN** 他のプレイヤーがルームに参加する
- **THEN** `OnPlayerJoined(Photon.Realtime.Player)` イベントが発行される
- **WHEN** 他のプレイヤーがルームから退出する
- **THEN** `OnPlayerLeft(Photon.Realtime.Player)` イベントが発行される

#### Scenario: 接続エラーの通知
- **WHEN** Photonサーバーへの接続に失敗する
- **THEN** `OnConnectionFailed(DisconnectCause)` イベントが発行される
- **AND** エラー理由がログに出力される

---

### Requirement: スポーンポイント
システムはシーン内にスポーンポイントを配置し、プレイヤー生成時の初期位置として使用しなければならない（SHALL）。

#### Scenario: スポーンポイントの配置
- **WHEN** シーンにSpawnPointコンポーネントがアタッチされたGameObjectが配置されている
- **THEN** PlayerSpawnManagerがそのGameObjectの位置と回転をスポーン候補として認識する

#### Scenario: スポーンポイントが未配置の場合
- **WHEN** シーンにスポーンポイントが1つも配置されていない
- **THEN** ワールド原点 (0, 0, 0) にプレイヤーが生成される
- **AND** 警告メッセージがログに出力される

---

### Requirement: ネットワークデバッグUI
システムはランタイムでネットワーク状態を確認できるデバッグUIを提供しなければならない（SHALL）。

#### Scenario: デバッグ情報の表示
- **WHEN** デバッグUIが有効である
- **THEN** 画面上に以下の情報がオーバーレイ表示される:
  - 現在のネットワークモード（Offline/Online）
  - 接続状態（Connected/Disconnected/Connecting）
  - ルーム名と参加人数
  - Ping値（ミリ秒）
  - 自分のPhoton Player ID

---

### Requirement: クイックスタート
システムはシーン配置するだけでネットワーク接続からプレイヤー生成まで自動実行するクイックスタートコンポーネントを提供しなければならない（SHALL）。

#### Scenario: オフラインクイックスタート
- **WHEN** NetworkQuickStartコンポーネントがシーンに配置されている
- **AND** モードがOfflineに設定されている
- **THEN** 起動時にオフラインルームに自動参加する
- **AND** autoSpawnOnJoinにより自動でプレイヤーが生成される

#### Scenario: オンラインクイックスタート
- **WHEN** NetworkQuickStartコンポーネントがシーンに配置されている
- **AND** モードがOnlineに設定されている
- **THEN** 起動時にPhotonサーバーに自動接続する
- **AND** 接続完了後にランダムルームまたは指定ルームに自動参加する
- **AND** autoSpawnOnJoinにより自動でプレイヤーが生成される
