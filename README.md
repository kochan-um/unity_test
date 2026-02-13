# Unity WebGL CI/CD (Vercel)

## 概要
このリポジトリは Unity プロジェクト (`Test/`) の WebGL ビルドを GitHub Actions で実行し、Vercel へ本番デプロイします。

- Workflow: `.github/workflows/webgl-vercel.yml`
- Vercelヘッダー設定: `vercel.json`

## 事前準備
1. GitHub リポジトリの `Settings > Secrets and variables > Actions` に以下を登録する
- `UNITY_LICENSE` または `UNITY_SERIAL`（いずれか必須）
  - 互換: `UNITY_LISENCE` / `UNITY_LICENCE` でも受け付けます
- `UNITY_EMAIL`（`UNITY_LICENSE` / `UNITY_SERIAL` 利用時は必須）
- `UNITY_PASSWORD`（`UNITY_LICENSE` / `UNITY_SERIAL` 利用時は必須）
- `VERCEL_TOKEN`（必須）
- `VERCEL_ORG_ID`（チーム運用時のみ任意）

2. Unity ライセンス方式の注意
- Unity Personal は手動有効化（`.alf` -> `.ulf`）を前提にしない
- `license.unity3d.com/manual` の UI を検証ツールで書き換える方法は運用に採用しない
- CI で Unity ビルドする場合は、`UNITY_SERIAL`（有償ライセンス）または正規に取得した `UNITY_LICENSE` を使う

3. Vercel 側でプロジェクト作成済み、または Token にデプロイ権限があることを確認する

## デプロイ方法
1. `main` または `master` へ push する
2. もしくは GitHub Actions の `Build WebGL and Deploy to Vercel` を `workflow_dispatch` で手動実行する
3. ワークフローが `Test/Build` 配下から `index.html` を含む WebGL 出力を検出して本番デプロイする

## 失敗時チェック
- `Missing UNITY_LICENSE or UNITY_SERIAL`:
  Unity ライセンス系シークレット未設定
- `UNITY_LICENSE mode requires UNITY_EMAIL and UNITY_PASSWORD`:
  `UNITY_LICENSE` はあるが認証情報不足
- `UNITY_LICENSE does not look like a valid .ulf XML content`:
  `.ulf` 形式でない値が設定されている
- `Missing VERCEL_TOKEN secret`:
  Vercelトークン未設定
- `WebGL output not found under Test/Build`:
  Unity ビルド失敗、または出力先が想定外

## 手動手順の一覧
手動でしか実施できない作業は `MANUAL_STEPS.md` に集約しています。
