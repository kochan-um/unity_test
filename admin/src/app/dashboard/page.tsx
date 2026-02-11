"use client";

import { useRouter } from "next/navigation";
import { useState, useEffect } from "react";

export default function Dashboard() {
  const router = useRouter();
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [userName, setUserName] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    checkAuth();
  }, []);

  async function checkAuth() {
    try {
      const res = await fetch("/api/auth/session");
      if (res.ok) {
        const data = await res.json();
        setIsAuthenticated(true);
        setUserName(data.data?.user?.email || "Admin");
      } else {
        router.push("/");
      }
    } catch (err) {
      console.error("Auth check failed:", err);
      router.push("/");
    } finally {
      setLoading(false);
    }
  }

  async function handleLogout() {
    try {
      await fetch("/api/auth/logout", { method: "POST" });
      router.push("/");
    } catch (err) {
      console.error("Logout failed:", err);
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto mb-4"></div>
          <p className="text-gray-600">読み込み中...</p>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return null;
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* ヘッダー */}
      <header className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 flex justify-between items-center">
          <h1 className="text-3xl font-bold text-gray-900">ゲーム管理画面</h1>
          <div className="flex items-center gap-4">
            <span className="text-sm text-gray-600">{userName}</span>
            <button
              onClick={handleLogout}
              className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
            >
              ログアウト
            </button>
          </div>
        </div>
      </header>

      {/* メインコンテンツ */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {/* アイテム管理 */}
          <div className="bg-white rounded-lg shadow p-6 hover:shadow-lg transition-shadow cursor-pointer">
            <div className="flex items-center gap-4">
              <div className="text-4xl">📦</div>
              <div>
                <h2 className="text-lg font-semibold text-gray-900">
                  アイテム管理
                </h2>
                <p className="text-sm text-gray-600">
                  ゲーム内アイテムの作成・編集・削除
                </p>
              </div>
            </div>
            <button className="mt-4 w-full px-4 py-2 bg-indigo-100 text-indigo-700 rounded-lg hover:bg-indigo-200 transition-colors">
              管理画面へ
            </button>
          </div>

          {/* プレイヤー管理 */}
          <div className="bg-white rounded-lg shadow p-6 hover:shadow-lg transition-shadow cursor-pointer">
            <div className="flex items-center gap-4">
              <div className="text-4xl">👥</div>
              <div>
                <h2 className="text-lg font-semibold text-gray-900">
                  プレイヤー管理
                </h2>
                <p className="text-sm text-gray-600">
                  プレイヤー情報の検索・確認・修正
                </p>
              </div>
            </div>
            <button className="mt-4 w-full px-4 py-2 bg-indigo-100 text-indigo-700 rounded-lg hover:bg-indigo-200 transition-colors">
              管理画面へ
            </button>
          </div>

          {/* ダッシュボード統計 */}
          <div className="bg-white rounded-lg shadow p-6 hover:shadow-lg transition-shadow cursor-pointer">
            <div className="flex items-center gap-4">
              <div className="text-4xl">📊</div>
              <div>
                <h2 className="text-lg font-semibold text-gray-900">
                  統計ダッシュボード
                </h2>
                <p className="text-sm text-gray-600">
                  ゲーム統計とメトリクスを表示
                </p>
              </div>
            </div>
            <button className="mt-4 w-full px-4 py-2 bg-indigo-100 text-indigo-700 rounded-lg hover:bg-indigo-200 transition-colors">
              管理画面へ
            </button>
          </div>

          {/* Gemini AIチャットbot */}
          <div className="bg-gradient-to-br from-purple-50 to-blue-50 rounded-lg shadow p-6 hover:shadow-lg transition-shadow cursor-pointer border-2 border-purple-200 md:col-span-2 lg:col-span-1">
            <div className="flex items-center gap-4">
              <div className="text-4xl">🤖</div>
              <div>
                <h2 className="text-lg font-semibold text-gray-900">
                  AIチャットbot
                </h2>
                <p className="text-sm text-gray-600">
                  Gemini AI による自然言語データ検索
                </p>
              </div>
            </div>
            <button
              onClick={() => router.push("/chat")}
              className="mt-4 w-full px-4 py-2 bg-gradient-to-r from-purple-600 to-blue-600 text-white rounded-lg hover:from-purple-700 hover:to-blue-700 transition-colors font-semibold"
            >
              チャットを開く →
            </button>
          </div>
        </div>

        {/* 情報セクション */}
        <div className="mt-12 bg-blue-50 border-l-4 border-blue-500 rounded p-6">
          <h3 className="text-lg font-semibold text-blue-900 mb-2">
            ℹ️ Gemini AIチャットbotについて
          </h3>
          <p className="text-blue-800 mb-2">
            自然言語でゲームデータを検索できるAIアシスタントです。以下のような質問が可能です：
          </p>
          <ul className="list-disc list-inside text-blue-700 space-y-1">
            <li>「レア度がレジェンドの武器を教えて」</li>
            <li>「プレイヤーAAAのインベントリを見せて」</li>
            <li>「今日のアクティブプレイヤー数は？」</li>
            <li>画像をアップロードしてアイテム分析も可能</li>
          </ul>
        </div>
      </main>
    </div>
  );
}
