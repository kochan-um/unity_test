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
  - 互換: `UNITY_LISENCE` / `UNITY_LICENCE` でも可
- 必須（`UNITY_LICENSE` / `UNITY_SERIAL` 利用時）: `UNITY_EMAIL`
- 必須（`UNITY_LICENSE` / `UNITY_SERIAL` 利用時）: `UNITY_PASSWORD`
- 任意（チーム運用）: `VERCEL_ORG_ID`
- 管理API用: `SUPABASE_URL`
- 管理API用: `SUPABASE_SERVICE_ROLE_KEY`
- 管理API用: `SESSION_SECRET`

### Unity ライセンス運用ポリシー
- Unity Personal の手動有効化（`.alf` -> `.ulf`）は前提にしない
- `license.unity3d.com/manual` で非表示オプションをブラウザ検証ツールで表示する方法は採用しない
- CI で Unity ビルドが必要な場合は、`UNITY_SERIAL`（有償）または正規に取得した `UNITY_LICENSE` を登録する

## 4. Vercel プロジェクト準備（手動必須）
1. Vercel上でプロジェクトを作成する、または既存プロジェクトへの deploy 権限を確認する
2. Organization利用時は `VERCEL_ORG_ID` を控える
3. 管理API用に別プロジェクトを作成する場合は、Root Directory を `admin` に設定する

## 5. GitHub Actions 実行（手動実行）
1. GitHub リポジトリの `Actions` タブを開く
2. `Build WebGL and Deploy to Vercel` を選択
3. `Run workflow` を実行
4. 成功ログを確認し、Vercelの公開URLへアクセス
5. 管理APIをデプロイする場合は `Deploy Admin API to Vercel` を選択する

## 6. 失敗時の確認ポイント（手動対応）
- `Missing UNITY_LICENSE or UNITY_SERIAL`:
  Unity secrets 未設定
- `UNITY_LICENSE mode requires UNITY_EMAIL and UNITY_PASSWORD`:
  `UNITY_LICENSE` はあるが認証情報不足
- `UNITY_LICENSE does not look like a valid .ulf XML content`:
  `.ulf` 形式でない値が設定されている
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
