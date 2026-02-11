## 1. 環境セットアップ
- [x] 1.1 `Test/Assets/Scripts/Networking/` ディレクトリ構成を作成する
- [ ] 1.2 PhotonServerSettingsのApp IDが設定されていることを確認する
- [ ] 1.3 Photon PUN2のScripting Define Symbols（PHOTON_UNITY_NETWORKING, PUN_2_OR_NEWER）を確認する

## 2. 設定システム
- [x] 2.1 NetworkSettings（ScriptableObject）を実装する（全設定項目の定義）
- [x] 2.2 NetworkMode列挙型を定義する（Offline, Online）

## 3. ネットワークモード管理
- [x] 3.1 NetworkModeManagerを実装する（シングルトン、モード切替、イベント通知）
- [x] 3.2 Photon OfflineMode の有効化/無効化ロジックを実装する
- [x] 3.3 ゲームプレイ中のモード切替禁止ロジックを実装する

## 4. ロビー・ルーム管理
- [x] 4.1 LobbyManagerを実装する（MonoBehaviourPunCallbacks継承）
- [x] 4.2 Photonサーバー接続処理を実装する
- [x] 4.3 ルーム作成（CreateRoom）を実装する
- [x] 4.4 ルーム参加（JoinRoom, JoinRandomRoom）を実装する
- [x] 4.5 ルーム退出（LeaveRoom）を実装する
- [x] 4.6 ランダム参加失敗時の自動ルーム作成を実装する
- [x] 4.7 オフラインモードでのルーム参加を実装する
- [x] 4.8 ルームリスト更新コールバックを実装する

## 5. プレイヤー生成管理
- [x] 5.1 SpawnPointコンポーネントを実装する
- [x] 5.2 PlayerSpawnManagerを実装する（PhotonNetwork.Instantiateによる生成）
- [x] 5.3 スポーンポイント選択ロジックを実装する（重複回避）
- [x] 5.4 プレイヤー退出時のクリーンアップを実装する
- [x] 5.5 自動スポーン機能を実装する（autoSpawnOnJoin設定）

## 6. ネットワークプレイヤーコントローラー
- [x] 6.1 NetworkPlayerControllerを実装する（MonoBehaviourPun継承）
- [x] 6.2 ローカルプレイヤーの初期化処理を実装する（コンポーネント有効化、カメラ設定）
- [x] 6.3 リモートプレイヤーの初期化処理を実装する（コンポーネント無効化）
- [x] 6.4 Cinemachineバーチャルカメラのフォローターゲット自動設定を実装する

## 7. ネットワーク同期
- [ ] 7.1 PhotonTransformViewの設定（位置・回転の同期、補間）を実装する — プレハブ作成時に設定
- [ ] 7.2 PhotonAnimatorViewの設定（Speed, Grounded, Jump, FreeFall, MotionSpeed同期）を実装する — プレハブ作成時に設定
- [ ] 7.3 リモートプレイヤーの補間処理を実装する — PhotonTransformView標準の補間を使用

## 8. イベント通知システム
- [x] 8.1 ネットワークイベント定義を実装する（OnRoomJoined, OnRoomLeft, OnPlayerJoined, OnPlayerLeft, OnConnectionFailed）
- [x] 8.2 LobbyManager/PlayerSpawnManagerからのイベント発行を実装する

## 9. プレハブ作成
- [ ] 9.1 NetworkPlayerプレハブを作成する（Resources/フォルダに配置）— Unityエディタで作成
- [ ] 9.2 プレハブにPhotonView, PhotonTransformView, PhotonAnimatorViewをアタッチする — Unityエディタで設定
- [ ] 9.3 プレハブにNetworkPlayerController, ThirdPersonController, StarterAssetsInputs等をアタッチする — Unityエディタで設定

## 10. デバッグUI
- [x] 10.1 ネットワークデバッグオーバーレイを実装する（モード、接続状態、Ping表示）

## 11. クイックスタート
- [x] 11.1 NetworkQuickStartコンポーネントを実装する（ワンクリックで接続〜スポーンまで自動実行）

## 12. テスト
- [ ] 12.1 NetworkModeManagerの単体テストを作成する（モード切替、イベント発行）
- [ ] 12.2 LobbyManagerの統合テストを作成する（接続、ルーム操作）
- [ ] 12.3 NetworkPlayerControllerのローカル/リモート切替テストを作成する
- [ ] 12.4 オフラインモードでの動作確認テストを作成する
