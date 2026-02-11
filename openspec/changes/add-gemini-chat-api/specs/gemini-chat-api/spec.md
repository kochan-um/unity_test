## ADDED Requirements

### Requirement: Gemini クライアント初期化
システムは Google Generative AI SDK を使用して Gemini API に接続できなければならない（MUST）。
API キーは環境変数（`GEMINI_API_KEY`）で管理し、ソースコードにハードコードしてはならない（MUST NOT）。
使用モデルは環境変数（`GEMINI_MODEL`）で変更可能でなければならない（MUST）。デフォルトは `gemini-2.0-flash` とする。

#### Scenario: Gemini クライアントの正常初期化
- **WHEN** 有効な `GEMINI_API_KEY` が環境変数に設定されている
- **THEN** Gemini クライアントが正常に初期化される
- **AND** 指定されたモデル（デフォルト: gemini-2.0-flash）でコンテンツ生成が可能になる

#### Scenario: API キー未設定時のエラー
- **WHEN** `GEMINI_API_KEY` が環境変数に設定されていない
- **THEN** アプリケーション起動時に明確なエラーメッセージが出力される
- **AND** チャット関連の API エンドポイントは 503 Service Unavailable を返却する

### Requirement: チャットセッション管理
認証済みの管理者はチャットセッションを作成・一覧取得・詳細取得・削除できなければならない（MUST）。
管理者は自分自身のセッションのみアクセスできなければならない（MUST）。

#### Scenario: チャットセッション作成
- **WHEN** 認証済みの管理者が `POST /api/chat/sessions` を実行する
- **THEN** 新しいチャットセッションが作成される
- **AND** 201レスポンスでセッション情報（id, title, created_at）が返却される

#### Scenario: チャットセッション一覧取得
- **WHEN** 認証済みの管理者が `GET /api/chat/sessions` を実行する
- **THEN** 当該管理者が所有するセッションのみが一覧で返却される
- **AND** 作成日時の降順でソートされる

#### Scenario: チャットセッション詳細取得
- **WHEN** 認証済みの管理者が `GET /api/chat/sessions/:id` を実行する
- **AND** 指定されたセッションが当該管理者のものである
- **THEN** セッション情報とメッセージ履歴が返却される

#### Scenario: 他の管理者のセッションへのアクセス拒否
- **WHEN** 認証済みの管理者が他の管理者のセッション ID でアクセスする
- **THEN** 404 Not Found レスポンスが返却される

#### Scenario: チャットセッション削除
- **WHEN** 認証済みの管理者が `DELETE /api/chat/sessions/:id` を実行する
- **AND** 指定されたセッションが当該管理者のものである
- **THEN** セッションと関連するメッセージが削除される
- **AND** 200レスポンスが返却される

### Requirement: テキストメッセージ送信とストリーミング応答
認証済みの管理者はテキストメッセージを送信し、AIからのストリーミング応答を受信できなければならない（MUST）。
レスポンスは Server-Sent Events 形式でストリーミングされなければならない（MUST）。

#### Scenario: テキストメッセージ送信と応答
- **WHEN** 認証済みの管理者がテキストメッセージを `POST /api/chat/sessions/:id/messages` に送信する
- **THEN** `Content-Type: text/event-stream` のストリーミングレスポンスが返却される
- **AND** AIの応答がリアルタイムでチャンクとして配信される
- **AND** 配信完了時に `{"type":"done","usage":{...}}` イベントが送信される

#### Scenario: 会話コンテキストの維持
- **WHEN** 同一セッション内で複数回メッセージを送信する
- **THEN** AIは過去のメッセージ履歴を参照して文脈を踏まえた回答を生成する

#### Scenario: 空メッセージの拒否
- **WHEN** テキストが空のメッセージを送信する
- **THEN** 400 Bad Request レスポンスが返却される

### Requirement: マルチモーダル入力（画像添付）
認証済みの管理者はテキストと共に画像を添付してメッセージを送信できなければならない（MUST）。
サポートする画像形式は PNG、JPEG、WebP でなければならない（MUST）。

#### Scenario: 画像付きメッセージの送信
- **WHEN** 認証済みの管理者がテキストと base64 エンコードされた画像を含むメッセージを送信する
- **THEN** Gemini がテキストと画像の両方を解析して回答を生成する
- **AND** ストリーミング形式で応答が返却される

#### Scenario: 複数画像の添付
- **WHEN** 認証済みの管理者が複数の画像（最大4枚）を添付してメッセージを送信する
- **THEN** Gemini がすべての画像とテキストを解析して回答を生成する

#### Scenario: サポート外の画像形式
- **WHEN** サポート外の画像形式（例: GIF, BMP）を添付してメッセージを送信する
- **THEN** 400 Bad Request レスポンスでサポート対象形式の説明が返却される

#### Scenario: 画像サイズ上限超過
- **WHEN** 1枚あたり4MBを超える画像を添付してメッセージを送信する
- **THEN** 400 Bad Request レスポンスでサイズ制限の説明が返却される

### Requirement: Function Calling によるゲームデータアクセス
AIアシスタントは Function Calling を通じてゲームデータ（アイテム、プレイヤー、統計）にアクセスし、データに基づいた回答を生成できなければならない（MUST）。
Function Calling はデータの読み取りのみを行い、データの更新・削除を行ってはならない（MUST NOT）。

#### Scenario: アイテム検索のFunction Calling
- **WHEN** 管理者が「レア度がレジェンドの武器を教えて」とメッセージを送信する
- **THEN** AIが `search_items` ツールを呼び出す
- **AND** ストリーミングイベントで `{"type":"tool_call","name":"search_items","args":{"query":"武器","rarity":"legendary"}}` が配信される
- **AND** ツール実行結果が `{"type":"tool_result",...}` として配信される
- **AND** AIがツール結果に基づいた回答を生成する

#### Scenario: プレイヤー情報取得のFunction Calling
- **WHEN** 管理者が「プレイヤー○○のインベントリを見せて」とメッセージを送信する
- **THEN** AIが `search_players` と `get_player_inventory` ツールを順次呼び出す
- **AND** 取得したデータに基づいた回答を生成する

#### Scenario: ダッシュボード統計取得のFunction Calling
- **WHEN** 管理者が「今日のアクティブプレイヤー数は？」とメッセージを送信する
- **THEN** AIが `get_dashboard_stats` ツールを呼び出す
- **AND** 統計データに基づいた回答を生成する

#### Scenario: 存在しないデータへの問い合わせ
- **WHEN** 管理者が存在しないアイテムやプレイヤーについて質問する
- **THEN** AIは「該当するデータが見つかりませんでした」と正確に回答する
- **AND** 存在しないデータを捏造してはならない

### Requirement: 個人情報保護
Function Calling でゲームデータを Gemini API に送信する際、プレイヤーのメールアドレス等の個人情報はマスク処理されなければならない（MUST）。

#### Scenario: メールアドレスのマスク
- **WHEN** Function Calling でプレイヤーデータを取得する
- **THEN** メールアドレスがマスクされた状態（例: `u***@example.com`）で Gemini に送信される

#### Scenario: マスク対象外データの通過
- **WHEN** Function Calling でアイテムデータや統計データを取得する
- **THEN** 個人情報を含まないデータはそのまま Gemini に送信される

### Requirement: チャット履歴の永続化
チャットセッションとメッセージは Supabase データベースに永続化されなければならない（MUST）。
ユーザーメッセージは送信時、AI応答はストリーミング完了時に保存されなければならない（MUST）。

#### Scenario: ユーザーメッセージの保存
- **WHEN** 管理者がメッセージを送信する
- **THEN** メッセージが `chat_messages` テーブルに `role: "user"` として保存される
- **AND** 画像添付がある場合、メタデータ（mimeType、サイズ）が記録される

#### Scenario: AI応答の保存
- **WHEN** AIの応答ストリーミングが完了する
- **THEN** 完全な応答テキストが `chat_messages` テーブルに `role: "assistant"` として保存される
- **AND** Function Calling の呼び出し履歴（ツール名、引数、結果）が記録される

#### Scenario: セッション履歴からの会話再開
- **WHEN** 管理者が既存セッションにメッセージを送信する
- **THEN** 保存された過去のメッセージ履歴が Gemini のコンテキストとして復元される
- **AND** 文脈を踏まえた回答が生成される

### Requirement: レート制限
チャットAPIへのリクエストはレート制限されなければならない（MUST）。
制限超過時は 429 Too Many Requests レスポンスを返却しなければならない（MUST）。

#### Scenario: 短期レート制限の適用
- **WHEN** 管理者が1分間に10回を超えるメッセージを送信する
- **THEN** 429 Too Many Requests レスポンスが返却される
- **AND** `Retry-After` ヘッダーで待機時間が通知される

#### Scenario: 日次トークン制限の適用
- **WHEN** 管理者の1日のトークン消費量が上限（環境変数で設定）に達する
- **THEN** 429 Too Many Requests レスポンスが返却される
- **AND** レスポンスボディに日次制限超過の旨が記載される

#### Scenario: レート制限内のリクエスト
- **WHEN** レート制限内でメッセージを送信する
- **THEN** リクエストは正常に処理される
