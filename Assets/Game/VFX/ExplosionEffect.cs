using System.Collections;
using UnityEngine;

namespace SteelRain.VFX
{
    /// <summary>
    /// 爆炸特效：粒子扩散 + 闪光 + 自动销毁。
    /// </summary>
    public sealed class ExplosionEffect : MonoBehaviour
    {
        [SerializeField] private float duration = 0.4f;

        public static void Spawn(Vector3 position, float scale = 1f)
        {
            var go = new GameObject("Explosion");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSpriteStatic();
            sr.color = new Color(1f, 0.6f, 0.1f, 1f);
            sr.sortingOrder = 20;
            var effect = go.AddComponent<ExplosionEffect>();
            effect.StartCoroutine(effect.PlayRoutine(go, sr));
        }

        private static Sprite CreateCircleSpriteStatic()
        {
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
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }

        private IEnumerator PlayRoutine(GameObject go, SpriteRenderer sr)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                go.transform.localScale = Vector3.one * (1f + t * 3f);
                var c = sr.color;
                c.a = 1f - t;
                sr.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(go);
        }
    }
}
