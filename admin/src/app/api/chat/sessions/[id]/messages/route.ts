import { NextRequest } from "next/server";
import { getSession } from "@/lib/auth/session";
import { unauthorized, notFound } from "@/lib/api/response";

// ローカル開発用デモデータ
const demoMessages: { [sessionId: string]: any[] } = {};
const demoSessions: { [key: string]: any } = {};

export async function POST(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  try {
    const session = await getSession();
    if (!session) {
      return unauthorized();
    }

    // ローカル開発用: ダミーセッション検証
    if (!demoSessions[id]) {
      demoSessions[id] = { id, admin_user_id: session.user.id };
    }
    if (demoSessions[id].admin_user_id !== session.user.id) {
      return notFound("Chat session not found");
    }

    const body = await request.json();
    const { content } = body;

    if (!demoMessages[id]) {
      demoMessages[id] = [];
    }

    // ユーザーメッセージを保存
    demoMessages[id].push({
      id: `msg-${Date.now()}`,
      role: "user",
      content: content,
      timestamp: new Date().toISOString(),
    });

    // ダミーAI応答を生成
    const aiResponse = generateMockResponse(content);

    // AIメッセージを保存
    demoMessages[id].push({
      id: `msg-${Date.now() + 1}`,
      role: "assistant",
      content: aiResponse,
      timestamp: new Date().toISOString(),
    });

    // SSE形式でストリーミング応答を返す
    const encoder = new TextEncoder();
    const stream = new ReadableStream({
      start(controller) {
        // テキストをチャンク分割してストリーミング
        const chunks = aiResponse.match(/.{1,30}/g) || [];
        let index = 0;

        const sendChunk = () => {
          if (index < chunks.length) {
            const chunk = chunks[index];
            controller.enqueue(
              encoder.encode(
                `data: ${JSON.stringify({ type: "text", content: chunk })}\n\n`
              )
            );
            index++;
            setTimeout(sendChunk, 50);
          } else {
            controller.enqueue(
              encoder.encode(
                `data: ${JSON.stringify({ type: "done", usage: { promptTokens: 0, completionTokens: 0 } })}\n\n`
              )
            );
            controller.close();
          }
        };

        sendChunk();
      },
    });

    return new Response(stream, {
      headers: {
        "Content-Type": "text/event-stream",
        "Cache-Control": "no-cache",
        Connection: "keep-alive",
      },
    });
  } catch (error: any) {
    console.error("Error processing message:", error);
    return new Response(
      JSON.stringify({ error: "Internal server error" }),
      { status: 500 }
    );
  }
}

function generateMockResponse(userMessage: string): string {
  const lowerMessage = userMessage.toLowerCase();

  // ユーザーのメッセージに基づいてモック応答を生成
  if (
    lowerMessage.includes("武器") ||
    lowerMessage.includes("weapon") ||
    lowerMessage.includes("アイテム") ||
    lowerMessage.includes("item")
  ) {
    return `ゲーム内のアイテムについてのご質問ですね。🎮

現在のゲームには以下のアイテムカテゴリがあります：
- 武器（Weapon）
- 防具（Armor）
- ポーション（Potion）
- クエストアイテム（Quest Items）

レア度：Common, Uncommon, Rare, Epic, Legendary

もっと詳しく知りたいアイテムの種類はありますか？`;
  } else if (
    lowerMessage.includes("プレイヤー") ||
    lowerMessage.includes("player") ||
    lowerMessage.includes("ユーザー")
  ) {
    return `プレイヤー情報についてのご質問ですね。👥

現在のゲームには以下のプレイヤーデータがあります：
- ユーザーID
- 表示名（Display Name）
- スコア（Score）
- レベル（Level）
- インベントリ（Inventory）

プレイヤーの検索や詳細情報の確認ができます。特定のプレイヤーについてご質問ですか？`;
  } else if (
    lowerMessage.includes("統計") ||
    lowerMessage.includes("stats") ||
    lowerMessage.includes("統計情報")
  ) {
    return `ゲーム統計についてのご質問ですね。📊

現在利用可能な統計情報：
- 総プレイヤー数：1,234名
- 本日のアクティブプレイヤー：456名
- 登録アイテム総数：892個
- 直近7日間の新規登録：78名

リアルタイムで更新されています。他にご質問ありますか？`;
  } else if (
    lowerMessage.includes("こんにちは") ||
    lowerMessage.includes("hello") ||
    lowerMessage.includes("hi")
  ) {
    return `こんにちは！🎉

ゲーム管理アシスタントです。以下のことでお手伝いできます：
- アイテムの検索・管理
- プレイヤー情報の確認
- ゲーム統計の閲覧
- その他ゲーム関連の質問

何かお困りなことがありますか？`;
  } else {
    return `ご質問ありがとうございます。💬

お尋ねの内容について詳しく教えていただければ、より正確な情報を提供できます。

以下の話題についてお答えできます：
- 🎮 アイテム情報
- 👥 プレイヤー管理
- 📊 ゲーム統計
- 🔧 その他のサポート

ご質問をお気軽にどうぞ！`;
  }
}
