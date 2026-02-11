"use client";

import { useRouter } from "next/navigation";
import { useState, useEffect, useRef } from "react";

interface Message {
  id: string;
  role: "user" | "assistant";
  content: string;
  timestamp: Date;
}

export default function ChatPage() {
  const router = useRouter();
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [pageLoading, setPageLoading] = useState(true);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  useEffect(() => {
    checkAuthAndCreateSession();
  }, []);

  async function checkAuthAndCreateSession() {
    try {
      // Check auth
      const authRes = await fetch("/api/auth/session", {
        credentials: "include", // クッキーを含める
      });
      console.log("Auth session response:", { status: authRes.status, ok: authRes.ok });
      if (!authRes.ok) {
        console.error("Auth failed with status:", authRes.status);
        router.push("/");
        return;
      }

      setIsAuthenticated(true);

      // Create new session
      const sessionRes = await fetch("/api/chat/sessions", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include", // クッキーを含める
        body: JSON.stringify({ title: `Chat - ${new Date().toLocaleString()}` }),
      });

      console.log("Session creation response:", { status: sessionRes.status, ok: sessionRes.ok });
      if (sessionRes.ok) {
        const data = await sessionRes.json();
        console.log("Full session response:", JSON.stringify(data, null, 2));
        const newSessionId = data.data?.data?.id || data.data?.id || data?.id;
        console.log("Setting sessionId to:", newSessionId);
        setSessionId(newSessionId);
      } else {
        const errorData = await sessionRes.text();
        console.error("Session creation failed:", { status: sessionRes.status, error: errorData });
      }
    } catch (err) {
      console.error("Auth check failed:", err);
      router.push("/");
    } finally {
      setPageLoading(false);
    }
  }

  async function handleSendMessage() {
    console.log("handleSendMessage called:", { input: input.trim(), sessionId, loading });
    if (!input.trim() || !sessionId || loading) {
      console.log("Message sending blocked:", { emptyInput: !input.trim(), noSession: !sessionId, isLoading: loading });
      return;
    }

    const messageContent = input;
    const userMessage: Message = {
      id: `user-${Date.now()}`,
      role: "user",
      content: messageContent,
      timestamp: new Date(),
    };

    setMessages((prev) => [...prev, userMessage]);
    setInput("");
    setLoading(true);

    try {
      console.log("Sending message to session:", sessionId);
      const response = await fetch(`/api/chat/sessions/${sessionId}/messages`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include", // クッキーを含める
        body: JSON.stringify({ content: messageContent }),
      });

      console.log("Message API response:", { status: response.status, ok: response.ok });
      if (!response.ok) {
        throw new Error(`Failed to send message: ${response.status}`);
      }

      let assistantContent = "";
      const reader = response.body?.getReader();
      const decoder = new TextDecoder();

      if (reader) {
        while (true) {
          const { done, value } = await reader.read();
          if (done) break;

          const text = decoder.decode(value);
          const lines = text.split("\n");

          for (const line of lines) {
            if (line.startsWith("data: ")) {
              try {
                const json = JSON.parse(line.slice(6));
                if (json.type === "text") {
                  assistantContent += json.content;
                } else if (json.type === "tool_call") {
                  assistantContent += `\n🔧 ツール実行: ${json.name}(${JSON.stringify(json.args)})\n`;
                } else if (json.type === "tool_result") {
                  assistantContent += `✅ 結果: ${JSON.stringify(json.content)}\n`;
                } else if (json.type === "done") {
                  // Streaming complete
                }
              } catch (e) {
                // JSON parse error
              }
            }
          }
        }
      }

      if (assistantContent) {
        const assistantMessage: Message = {
          id: `assistant-${Date.now()}`,
          role: "assistant",
          content: assistantContent,
          timestamp: new Date(),
        };
        setMessages((prev) => [...prev, assistantMessage]);
      }
    } catch (err) {
      console.error("Error sending message:", err);
      const errorMessage: Message = {
        id: `error-${Date.now()}`,
        role: "assistant",
        content: "エラーが発生しました。もう一度試してください。",
        timestamp: new Date(),
      };
      setMessages((prev) => [...prev, errorMessage]);
    } finally {
      setLoading(false);
    }
  }

  async function handleLogout() {
    try {
      await fetch("/api/auth/logout", {
        method: "POST",
        credentials: "include", // クッキーを含める
      });
      router.push("/");
    } catch (err) {
      console.error("Logout failed:", err);
    }
  }

  if (pageLoading || !isAuthenticated) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto mb-4"></div>
          <p className="text-gray-600">読み込み中...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="h-screen bg-gray-50 flex flex-col">
      {/* ヘッダー */}
      <header className="bg-gradient-to-r from-purple-600 to-blue-600 text-white shadow">
        <div className="max-w-4xl mx-auto px-4 py-4 flex justify-between items-center">
          <div className="flex items-center gap-3">
            <button
              onClick={() => router.push("/dashboard")}
              className="hover:opacity-80 transition-opacity"
            >
              ← 戻る
            </button>
            <h1 className="text-2xl font-bold">🤖 Gemini AIチャットbot</h1>
          </div>
          <button
            onClick={handleLogout}
            className="px-4 py-2 bg-white bg-opacity-20 text-white rounded-lg hover:bg-opacity-30 transition-colors"
          >
            ログアウト
          </button>
        </div>
      </header>

      {/* メッセージエリア */}
      <div className="flex-1 max-w-4xl mx-auto w-full overflow-y-auto p-4 space-y-4">
        {messages.length === 0 && (
          <div className="h-full flex flex-col items-center justify-center text-center">
            <div className="text-6xl mb-4">💬</div>
            <h2 className="text-2xl font-semibold text-gray-900 mb-2">
              何かお手伝いできることはありますか？
            </h2>
            <p className="text-gray-600 mb-6 max-w-md">
              ゲームデータについて質問してください。AIが自動的にデータを検索して回答します。
            </p>
            <div className="space-y-2 text-sm text-gray-600">
              <p>💡 例えば：</p>
              <ul className="list-disc list-inside">
                <li>「レジェンドレアリティの武器を表示してください」</li>
                <li>「プレイヤーAAAのインベントリは？」</li>
                <li>「アクティブなプレイヤー数を教えてください」</li>
              </ul>
            </div>
          </div>
        )}

        {messages.map((msg) => (
          <div key={msg.id} className={`flex ${msg.role === "user" ? "justify-end" : "justify-start"}`}>
            <div
              className={`max-w-xl px-4 py-3 rounded-lg ${
                msg.role === "user"
                  ? "bg-indigo-600 text-white"
                  : "bg-white text-gray-900 border border-gray-200"
              }`}
            >
              <p className="whitespace-pre-wrap break-words text-sm">{msg.content}</p>
              <p className="text-xs mt-1 opacity-60">
                {msg.timestamp.toLocaleTimeString()}
              </p>
            </div>
          </div>
        ))}

        {loading && (
          <div className="flex justify-start">
            <div className="bg-white text-gray-900 border border-gray-200 px-4 py-3 rounded-lg">
              <div className="flex items-center gap-2">
                <div className="animate-bounce w-2 h-2 bg-indigo-600 rounded-full"></div>
                <div className="animate-bounce w-2 h-2 bg-indigo-600 rounded-full animation-delay-200"></div>
                <div className="animate-bounce w-2 h-2 bg-indigo-600 rounded-full animation-delay-400"></div>
              </div>
            </div>
          </div>
        )}

        <div ref={messagesEndRef} />
      </div>

      {/* 入力エリア */}
      <div className="border-t border-gray-200 bg-white p-4">
        <div className="max-w-4xl mx-auto">
          <div className="flex gap-2">
            <input
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyPress={(e) => {
                if (e.key === "Enter" && !e.shiftKey) {
                  e.preventDefault();
                  handleSendMessage();
                }
              }}
              placeholder="メッセージを入力してください..."
              disabled={loading}
              className="flex-1 px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-600 disabled:bg-gray-100"
            />
            <button
              onClick={handleSendMessage}
              disabled={loading || !input.trim()}
              className="px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:bg-gray-400 transition-colors font-semibold"
            >
              送信
            </button>
          </div>
          <p className="text-xs text-gray-500 mt-2">
            💡 ヒント: Shift + Enter で改行、Enter で送信
          </p>
        </div>
      </div>
    </div>
  );
}
