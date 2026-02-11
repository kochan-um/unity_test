using System.Collections;
using UnityEngine;

namespace WorldStreaming
{
    /// <summary>
    /// チャンク内の全レンダラーをフェードイン演出する。
    /// </summary>
    public class ChunkFadeInEffect : MonoBehaviour
    {
        /// <summary>
        /// チャンクをフェードイン演出で表示する
        /// </summary>
        public static void PlayFadeIn(GameObject chunkRoot, float duration, bool useRGB = true)
        {
            if (chunkRoot == null || duration <= 0)
            {
                return;
            }

            // コルーチン実行用のMonoBehaviourを取得またはアタッチ
            var monoBehaviour = chunkRoot.GetComponent<ChunkFadeInEffect>();
            if (monoBehaviour == null)
            {
                monoBehaviour = chunkRoot.AddComponent<ChunkFadeInEffect>();
            }

            var renderers = chunkRoot.GetComponentsInChildren<Renderer>(includeInactive: false);
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    monoBehaviour.StartCoroutine(FadeInCoroutine(renderer, duration, useRGB));
                }
            }
        }

        private static IEnumerator FadeInCoroutine(Renderer renderer, float duration, bool useRGB)
        {
            float elapsedTime = 0f;

            // 初期状態を透明に
            foreach (var material in renderer.materials)
            {
                SetMaterialAlpha(material, 0f, useRGB);
            }

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / duration);

                foreach (var material in renderer.materials)
                {
                    SetMaterialAlpha(material, alpha, useRGB);
                }

                yield return null;
            }

            // 最終状態を不透明に
            foreach (var material in renderer.materials)
            {
                SetMaterialAlpha(material, 1f, useRGB);
            }
        }

        private static void SetMaterialAlpha(Material material, float alpha, bool useRGB)
        {
            if (material == null)
            {
                return;
            }

            // 複数のシェーダーに対応
            if (material.HasProperty("_Color"))
            {
                var color = material.GetColor("_Color");
                color.a = alpha;
                material.SetColor("_Color", color);
            }
            else if (material.HasProperty("_BaseColor"))
            {
                var color = material.GetColor("_BaseColor");
                color.a = alpha;
                material.SetColor("_BaseColor", color);
            }

            // RGBフェード（不透明度0でオブジェクトを非表示にする）
            if (useRGB && alpha < 0.01f)
            {
                material.renderQueue = -1; // レンダーキュー操作で高速非表示
            }
            else if (useRGB && alpha > 0.99f)
            {
                material.renderQueue = -1; // デフォルトに戻す
            }
        }
    }
}
