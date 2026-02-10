# Manual Steps (Only)

このファイルは、自動化できない手順だけをまとめたチェックリストです。

## 1. GitHub リポジトリ接続（初回のみ）
ローカルリポジトリに remote がない場合のみ実施します。

```bash
git remote add origin <YOUR_GITHUB_REPO_URL>
git push -u origin <YOUR_BRANCH>
```

## 2. Vercel でトークン発行（手動必須）
1. Vercel Dashboard にログイン
2. Account Settings > Tokens で新規 Token を発行
3. 値を安全に保管

## 3. GitHub Actions Secrets 設定（手動必須）
GitHub リポジトリの `Settings > Secrets and variables > Actions` で以下を登録します。

- 必須: `VERCEL_TOKEN`
- 必須（どちらか）: `UNITY_LICENSE` または `UNITY_SERIAL`
- 任意: `UNITY_EMAIL`
- 任意: `UNITY_PASSWORD`
- 任意（チーム運用）: `VERCEL_ORG_ID`

## 4. Vercel プロジェクト準備（手動必須）
1. Vercel上でプロジェクトを作成する、または既存プロジェクトへの deploy 権限を確認する
2. Organization利用時は `VERCEL_ORG_ID` を控える

## 5. GitHub Actions 実行（手動実行）
1. GitHub リポジトリの `Actions` タブを開く
2. `Build WebGL and Deploy to Vercel` を選択
3. `Run workflow` を実行
4. 成功ログを確認し、Vercelの公開URLへアクセス

## 6. 失敗時の確認ポイント（手動対応）
- `Missing UNITY_LICENSE or UNITY_SERIAL`:
  Unity secrets 未設定
- `Missing VERCEL_TOKEN secret`:
  Vercel token 未設定
- `WebGL output not found under Test/Build`:
  Unity build 失敗または出力先不一致

## 7. ローカルからの検証デプロイ（任意・手動）
ローカルで token を設定済みの場合のみ実施します。

```bash
# PowerShell
$env:VERCEL_TOKEN="<YOUR_TOKEN>"

# 認証確認
vercel whoami
```

`vercel whoami` が成功しない場合、Tokenの再発行が必要です。
