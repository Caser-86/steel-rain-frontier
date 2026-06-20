using System.Collections;
using UnityEngine;

namespace SteelRain.VFX
{
    /// <summary>
    /// 粒子特效生成器：弹壳、脚步尘土、爆炸碎片。
    /// 全部程序化生成，无需粒子系统预制体。
    /// </summary>
    public sealed class ParticleSpawner : MonoBehaviour
    {
        [SerializeField] private int shellPoolSize = 30;
        [SerializeField] private int dustPoolSize = 20;

        private static ParticleSpawner instance;

        private GameObject[] shellPool;
        private GameObject[] dustPool;
        private int shellIndex;
        private int dustIndex;

        private static Sprite sharedDotSprite;

        private void Awake()
        {
            instance = this;
            shellPool = CreatePool("Shell", shellPoolSize, new Color(0.8f, 0.7f, 0.3f), 0.06f);
            dustPool = CreatePool("Dust", dustPoolSize, new Color(0.6f, 0.55f, 0.4f, 0.5f), 0.12f);
        }

        private GameObject[] CreatePool(string name, int size, Color color, float scale)
        {
            var pool = new GameObject[size];
            for (int i = 0; i < size; i++)
            {
                var go = new GameObject($"{name}_{i}");
                go.transform.SetParent(transform);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = CreateDotSprite();
                sr.color = color;
                sr.sortingOrder = 6;
                sr.enabled = false;
                go.transform.localScale = Vector3.one * scale;
                pool[i] = go;
            }
            return pool;
        }

        public static void SpawnShell(Vector3 position, Vector2 direction)
        {
            if (instance == null) return;
            var shell = instance.shellPool[instance.shellIndex];
            instance.shellIndex = (instance.shellIndex + 1) % instance.shellPoolSize;
            instance.StartCoroutine(instance.PlayShell(shell, position, direction));
        }

        public static void SpawnDust(Vector3 position)
        {
            if (instance == null) return;
            var dust = instance.dustPool[instance.dustIndex];
            instance.dustIndex = (instance.dustIndex + 1) % instance.dustPoolSize;
            instance.StartCoroutine(instance.PlayDust(dust, position));
        }

        private IEnumerator PlayShell(GameObject shell, Vector3 pos, Vector2 dir)
        {
            shell.transform.position = pos;
            shell.GetComponent<SpriteRenderer>().enabled = true;

            var vel = new Vector2(-dir.x * 3f + Random.Range(-1f, 1f), 4f);
            float elapsed = 0f;
            float duration = 0.8f;

            while (elapsed < duration)
            {
                vel.y -= 15f * Time.deltaTime;
                shell.transform.position += (Vector3)vel * Time.deltaTime;
                shell.transform.Rotate(0, 0, 720f * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            shell.GetComponent<SpriteRenderer>().enabled = false;
        }

        private IEnumerator PlayDust(GameObject dust, Vector3 pos)
        {
            dust.transform.position = pos;
            var sr = dust.GetComponent<SpriteRenderer>();
            sr.enabled = true;

            float elapsed = 0f;
            float duration = 0.3f;
            var startScale = dust.transform.localScale;
            var startColor = sr.color;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                dust.transform.localScale = startScale * (1f + t * 2f);
                var c = startColor;
                c.a = startColor.a * (1f - t);
                sr.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }

            sr.enabled = false;
            dust.transform.localScale = startScale;
            sr.color = startColor;
        }

        private Sprite CreateDotSprite()
        {
            // 使用静态共享Sprite，避免每次创建新Texture2D导致内存泄漏
            if (sharedDotSprite != null) return sharedDotSprite;

            int size = 8;
            var tex = new Texture2D(size, size);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, Color.white);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            sharedDotSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 8f);
            sharedDotSprite.name = "ParticleDot_Shared";
            return sharedDotSprite;
        }
    }
}
