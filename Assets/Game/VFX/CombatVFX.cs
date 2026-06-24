using System.Collections;
using UnityEngine;

namespace SteelRain.VFX
{
    /// <summary>
    /// 战斗视觉特效工具：预警区域、命中闪光。
    /// 供敌人AI使用。
    /// </summary>
    public static class CombatVFX
    {
        private static Sprite sharedCircleSprite;

        public static void SpawnWarningZone(Vector3 position, float radius, float duration)
        {
            var go = new GameObject("WarningZone");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * radius;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetSharedCircleSprite();
            sr.color = new Color(1f, 0.2f, 0.2f, 0.6f);
            sr.sortingOrder = 15;
            go.AddComponent<WarningZoneEffect>().StartCoroutine(PlayWarning(go, sr, duration));
        }

        public static void SpawnHitFlash(Vector3 position, float duration)
        {
            var go = new GameObject("HitFlash");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.5f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetSharedCircleSprite();
            sr.color = new Color(1f, 1f, 0.6f, 1f);
            sr.sortingOrder = 20;
            go.AddComponent<WarningZoneEffect>().StartCoroutine(PlayFlash(go, sr, duration));
        }

        private static IEnumerator PlayWarning(GameObject go, SpriteRenderer sr, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                var c = sr.color;
                c.a = 0.6f * (0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 6f));
                sr.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }
            Object.Destroy(go);
        }

        private static IEnumerator PlayFlash(GameObject go, SpriteRenderer sr, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                var c = sr.color;
                c.a = 1f - t;
                sr.color = c;
                go.transform.localScale = Vector3.one * (0.5f + t * 1.5f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Object.Destroy(go);
        }

        private static Sprite GetSharedCircleSprite()
        {
            if (sharedCircleSprite != null) return sharedCircleSprite;

            int size = 32;
            var tex = new Texture2D(size, size);
            var center = new Vector2(size / 2f, size / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(1f - dist / (size / 2f));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            sharedCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
            sharedCircleSprite.name = "CombatVFX_Circle_Shared";
            return sharedCircleSprite;
        }
    }

    /// <summary>
    /// 占位组件，用于启动协程。
    /// </summary>
    internal sealed class WarningZoneEffect : MonoBehaviour { }
}
