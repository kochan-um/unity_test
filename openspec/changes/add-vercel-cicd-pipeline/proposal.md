# Change: Add Vercel-based CI/CD pipeline for Unity WebGL

## Why
このプロジェクトには、Unity WebGL のビルドから公開までを自動化する標準パイプラインが未定義である。
手動デプロイをなくし、`main`/`master` への push で安定して公開できる仕組みが必要。

## What Changes
- Unity WebGL を GitHub Actions 上でビルドする CI フローを追加する
- 生成物を Vercel に本番デプロイする CD フローを追加する
- Unity と Vercel の必須シークレット、および失敗時の検知要件を定義する
- WebGL 圧縮アセット配信のため `vercel.json` のヘッダー要件を定義する

## Impact
- Affected specs: `vercel-cicd`
- Affected code: `.github/workflows/webgl-vercel.yml`, `vercel.json`, 運用ドキュメント