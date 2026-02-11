"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";

export default function Home() {
  const router = useRouter();
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    checkAuth();
  }, []);

  async function checkAuth() {
    try {
      const res = await fetch("/api/auth/session", {
        credentials: "include", // クッキーを含める
      });
      if (res.ok) {
        setIsAuthenticated(true);
        router.push("/dashboard");
      }
    } catch (err) {
      console.error("Auth check failed:", err);
    }
  }

  async function handleLogin(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      const res = await fetch("/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include", // クッキーを含める
        body: JSON.stringify({ email, password }),
      });

      if (res.ok) {
        setIsAuthenticated(true);
        router.push("/dashboard");
      } else {
        const data = await res.json();
        setError(data.error?.message || "ログインに失敗しました");
      }
    } catch (err) {
      setError("ログインに失敗しました");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-md p-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            ゲーム管理画面
          </h1>
          <p className="text-gray-600">Gemini AIチャットbot対応</p>
        </div>

        <form onSubmit={handleLogin} className="space-y-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              メールアドレス
            </label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="admin@example.com"
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
              disabled={loading}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              パスワード
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
              disabled={loading}
            />
          </div>

          {error && (
            <div className="p-3 bg-red-100 text-red-700 rounded-lg text-sm">
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-indigo-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-indigo-700 disabled:bg-gray-400 transition-colors"
          >
            {loading ? "ログイン中..." : "ログイン"}
          </button>
        </form>

        <div className="mt-8 pt-8 border-t border-gray-200">
          <p className="text-sm text-gray-600 mb-4">
            <strong>テスト用認証情報:</strong>
          </p>
          <p className="text-xs text-gray-500 mb-2">
            Email: admin@example.com
          </p>
          <p className="text-xs text-gray-500">Password: password123</p>
        </div>

        <div className="mt-6 pt-6 border-t border-gray-200">
          <p className="text-sm text-gray-600 mb-2">
            <strong>利用可能な機能:</strong>
          </p>
          <ul className="text-xs text-gray-500 space-y-1">
            <li>• アイテムマスタ管理</li>
            <li>• プレイヤーデータ検索</li>
            <li>• ゲーム統計ダッシュボード</li>
            <li>• 🤖 Gemini AIチャットbot</li>
          </ul>
        </div>
      </div>
    </div>
  );
}
