## 1. Implementation
- [x] 1.1 Unity WebGL のビルド対象ディレクトリと出力先を確定する
- [x] 1.2 GitHub Actions で Unity WebGL ビルドジョブを追加する
- [x] 1.3 Vercel CLI で本番デプロイするステップを追加する
- [x] 1.4 `VERCEL_TOKEN` / `VERCEL_ORG_ID` / Unity ライセンス系シークレットの検証を実装する
- [x] 1.5 `vercel.json` に `.wasm/.js/.data` の gzip/brotli ヘッダーを定義する
- [x] 1.6 セットアップ手順（Secrets 設定と運用フロー）を README に記載する

## 2. Validation
- [x] 2.1 `openspec validate add-vercel-cicd-pipeline --strict` を通す
- [ ] 2.2 `workflow_dispatch` による手動実行で build と deploy の成功を確認する