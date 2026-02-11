## ADDED Requirements

### Requirement: 管理者認証
管理者はメールアドレスとパスワードでログインし、セッションベースの認証でAPIにアクセスできなければならない（MUST）。
認証はSupabase Authを基盤とし、管理者ロール（`app_metadata.role === 'admin'`）を持つユーザーのみがアクセスを許可される。
セッションはHTTPOnly Cookieで管理し、クライアントサイドJavaScriptからトークンにアクセスできてはならない（MUST NOT）。

#### Scenario: 管理者ログイン成功
- **WHEN** 有効なメールアドレスとパスワードを `POST /api/auth/login` に送信する
- **AND** 該当ユーザーが admin ロールを持つ
- **THEN** HTTPOnly Cookie にセッショントークンが設定される
- **AND** 200レスポンスでユーザー情報が返却される

#### Scenario: 管理者ロールなしのユーザーがログインを試みる
- **WHEN** 有効な認証情報を持つが admin ロールを持たないユーザーがログインを試みる
- **THEN** 403 Forbidden レスポンスが返却される
- **AND** セッションは作成されない

#### Scenario: 無効な認証情報でのログイン
- **WHEN** 無効なメールアドレスまたはパスワードで `POST /api/auth/login` に送信する
- **THEN** 401 Unauthorized レスポンスが返却される

#### Scenario: ログアウト
- **WHEN** 認証済みの管理者が `POST /api/auth/logout` を実行する
- **THEN** セッション Cookie が削除される
- **AND** 200レスポンスが返却される

#### Scenario: セッション確認
- **WHEN** 認証済みの管理者が `GET /api/auth/session` を実行する
- **THEN** 現在のユーザー情報とセッション状態が返却される

#### Scenario: 未認証リクエストの拒否
- **WHEN** セッション Cookie なしで保護されたAPIエンドポイントにアクセスする
- **THEN** 401 Unauthorized レスポンスが返却される

### Requirement: アイテムマスタ管理
認証済みの管理者は、ゲーム内アイテムのマスタデータをCRUD操作で管理できなければならない（MUST）。
アイテム一覧はページネーション・カテゴリフィルタ・ソートに対応しなければならない（MUST）。
アイテム削除は論理削除（`is_deleted` フラグ）で実装しなければならない（MUST）。

#### Scenario: アイテム一覧取得
- **WHEN** 認証済みの管理者が `GET /api/items?page=1&limit=20` を実行する
- **THEN** アイテムの一覧が `{ data: [...], meta: { page, limit, totalCount, totalPages } }` 形式で返却される

#### Scenario: カテゴリフィルタ付きアイテム一覧
- **WHEN** 認証済みの管理者が `GET /api/items?category=weapon&page=1&limit=20` を実行する
- **THEN** 指定カテゴリのアイテムのみがフィルタされて返却される

#### Scenario: アイテム作成
- **WHEN** 認証済みの管理者が有効なアイテムデータを `POST /api/items` に送信する
- **THEN** アイテムが作成され、201レスポンスで作成されたアイテムが返却される

#### Scenario: 不正なアイテムデータでの作成
- **WHEN** 必須フィールドが欠けたデータを `POST /api/items` に送信する
- **THEN** 400 Bad Request レスポンスでバリデーションエラーの詳細が返却される

#### Scenario: アイテム詳細取得
- **WHEN** 認証済みの管理者が `GET /api/items/:id` を実行する
- **THEN** 指定IDのアイテム詳細が返却される

#### Scenario: 存在しないアイテムの取得
- **WHEN** 存在しないIDで `GET /api/items/:id` を実行する
- **THEN** 404 Not Found レスポンスが返却される

#### Scenario: アイテム更新
- **WHEN** 認証済みの管理者が有効な更新データを `PUT /api/items/:id` に送信する
- **THEN** アイテムが更新され、200レスポンスで更新されたアイテムが返却される

#### Scenario: アイテム削除（論理削除）
- **WHEN** 認証済みの管理者が `DELETE /api/items/:id` を実行する
- **THEN** アイテムの `is_deleted` フラグが `true` に設定される
- **AND** 200レスポンスが返却される
- **AND** 以降の一覧取得で当該アイテムはデフォルトで非表示になる

### Requirement: プレイヤーデータ管理
認証済みの管理者は、プレイヤーの情報・インベントリを閲覧し、必要に応じてデータを修正できなければならない（MUST）。
プレイヤーデータの修正は変更ログとして記録されなければならない（MUST）。

#### Scenario: プレイヤー一覧取得
- **WHEN** 認証済みの管理者が `GET /api/players?page=1&limit=20` を実行する
- **THEN** プレイヤーの一覧が `{ data: [...], meta: { page, limit, totalCount, totalPages } }` 形式で返却される

#### Scenario: プレイヤー検索
- **WHEN** 認証済みの管理者が `GET /api/players?search=username` を実行する
- **THEN** ユーザー名またはメールアドレスに部分一致するプレイヤーがフィルタされて返却される

#### Scenario: プレイヤー詳細取得
- **WHEN** 認証済みの管理者が `GET /api/players/:id` を実行する
- **THEN** プレイヤーのプロフィールと統計情報が返却される

#### Scenario: プレイヤーインベントリ取得
- **WHEN** 認証済みの管理者が `GET /api/players/:id/inventory` を実行する
- **THEN** プレイヤーが所有するアイテムの一覧がアイテム名付きで返却される

#### Scenario: プレイヤーデータ修正
- **WHEN** 認証済みの管理者が `PATCH /api/players/:id` で修正データを送信する
- **THEN** プレイヤーデータが更新される
- **AND** 変更内容が監査ログとして記録される（変更者、変更日時、変更前後の値）

### Requirement: ダッシュボード統計
認証済みの管理者は、ゲームの運用状況を把握するための統計データを取得できなければならない（MUST）。

#### Scenario: ダッシュボード統計取得
- **WHEN** 認証済みの管理者が `GET /api/dashboard/stats` を実行する
- **THEN** 以下の統計データが返却される:
  - 総プレイヤー数
  - 本日のアクティブプレイヤー数
  - 登録アイテム総数
  - 直近7日間の新規登録プレイヤー数

### Requirement: リアルタイムイベント配信
管理画面はServer-Sent Events（SSE）を通じて、ゲーム内のリアルタイムイベントを受信できなければならない（MUST）。

#### Scenario: SSE接続確立
- **WHEN** 認証済みの管理者が `GET /api/dashboard/events` に接続する
- **THEN** `text/event-stream` レスポンスでSSE接続が確立される
- **AND** 接続中はSupabase Realtimeからのイベント（プレイヤーログイン/ログアウト、アイテム変更）がリアルタイムで配信される

#### Scenario: SSE未認証接続の拒否
- **WHEN** 未認証のクライアントが `GET /api/dashboard/events` に接続しようとする
- **THEN** 401 Unauthorized レスポンスが返却される

### Requirement: APIレスポンス形式
すべてのAPIレスポンスは統一されたJSON形式で返却されなければならない（MUST）。

#### Scenario: 成功レスポンス
- **WHEN** APIリクエストが正常に処理される
- **THEN** `{ data: <結果>, meta?: <ページネーション等> }` 形式で返却される

#### Scenario: エラーレスポンス
- **WHEN** APIリクエストがエラーとなる
- **THEN** `{ error: { code: <HTTPステータスコード>, message: <エラーメッセージ>, details?: <バリデーションエラー等> } }` 形式で返却される

### Requirement: リクエストバリデーション
すべてのAPIリクエストのボディとクエリパラメータはZodスキーマでバリデーションされなければならない（MUST）。
バリデーションエラーは具体的なフィールド名とエラー理由を含まなければならない（MUST）。

#### Scenario: バリデーション成功
- **WHEN** Zodスキーマに適合するリクエストが送信される
- **THEN** リクエストは正常に処理される

#### Scenario: バリデーション失敗
- **WHEN** Zodスキーマに適合しないリクエストが送信される
- **THEN** 400 Bad Request レスポンスが返却される
- **AND** エラー詳細にフィールド名とエラー理由が含まれる（例: `{ details: [{ field: "name", message: "必須フィールドです" }] }`）
