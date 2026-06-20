using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.VFX
{
    public sealed class DamageNumberSpawner : MonoBehaviour
    {
        private static DamageNumberSpawner instance;
        private Canvas canvas;

        private void Awake()
        {
            instance = this;
            canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
                canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            if (gameObject.GetComponent<CanvasScaler>() == null)
                gameObject.AddComponent<CanvasScaler>();
        }

        public static void Spawn(Vector3 worldPos, int damage, bool isCrit = false)
        {
            if (instance == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                // fallback: 尝试Arial
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            if (font == null) return; // 无法显示文字，跳过

            var go = new GameObject("DmgNum");
            go.transform.SetParent(instance.transform);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = isCrit ? 28 : 20;
            text.color = isCrit ? Color.yellow : Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 30);

            var screenPos = cam.WorldToScreenPoint(worldPos);
            rt.position = screenPos;

            text.text = damage.ToString();
            instance.StartCoroutine(AnimateNumber(go, rt));
        }

        private static IEnumerator AnimateNumber(GameObject go, RectTransform rect)
        {
            float elapsed = 0f;
            float duration = 0.8f;
            var startPos = rect.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                rect.position = startPos + Vector3.up * (t * 80f);

                var text = go.GetComponent<Text>();
                if (text != null)
                {
                    var c = text.color;
                    c.a = 1f - t;
                    text.color = c;
                }

                yield return null;
            }

            Destroy(go);
        }
    }
}
