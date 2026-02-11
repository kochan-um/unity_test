# Change: StarterAssets ThirdPersonControllerにPhotonマルチプレイヤー/シングルプレイヤー切り替え機能を追加する

## Why
現在のThirdPersonControllerはローカル専用で、マルチプレイヤー対応の仕組みがない。
Photon PUN2は既にプロジェクトに導入済みだが、StarterAssetsとの統合が行われていない。
シングルプレイヤーとマルチプレイヤーの両モードに対応し、ランタイムで切り替えられる設計にすることで、オフラインプレイとオンラインプレイの両方をサポートする。

## What Changes
- ネットワークモード（Offline/Online）を管理するNetworkModeManagerを実装する
- ThirdPersonControllerをPhoton対応にラップするNetworkPlayerControllerを実装する（元のThirdPersonControllerは改変しない）
- PhotonView/PhotonTransformView/PhotonAnimatorViewによるプレイヤー状態の同期を実装する
- ロビー接続・ルーム作成/参加・退出を管理するLobbyManagerを実装する
- ネットワークプレイヤーの生成・破棄を管理するPlayerSpawnManagerを実装する
- リモートプレイヤーの入力を無効化し、ローカルプレイヤーのみ操作可能にするオーナーシップ制御を実装する
- オフラインモード時はPhoton接続なしでそのまま動作する互換性を保証する

## Impact
- Affected specs: `photon-multiplayer`（新規）
- Affected code:
  - `Test/Assets/Scripts/Networking/` — 新規スクリプト群
  - `Test/Assets/StarterAssets/ThirdPersonController/Scripts/ThirdPersonController.cs` — **改変なし**（ラッパーで対応）
  - `Test/Assets/Resources/` — ネットワークプレイヤープレハブ（Photon Instantiate用）
  - `Test/Assets/Photon/PhotonUnityNetworking/Resources/` — PhotonServerSettings確認
