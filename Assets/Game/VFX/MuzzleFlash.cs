using System.Collections;
using UnityEngine;

namespace SteelRain.VFX
{
    /// <summary>
    /// 枪口火焰特效，射击时短暂闪现。
    /// </summary>
    public sealed class MuzzleFlash : MonoBehaviour
    {
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private float duration = 0.05f;
        [SerializeField] private float scale = 0.4f;

        private SpriteRenderer flashRenderer;
        private Coroutine flashRoutine;
        private static Sprite sharedFlashSprite;

        private void Awake()
        {
            var go = new GameObject("MuzzleFlash");
            go.transform.SetParent(muzzlePoint != null ? muzzlePoint : transform);
            go.transform.localPosition = Vector3.zero;
            flashRenderer = go.AddComponent<SpriteRenderer>();
            flashRenderer.sprite = CreateFlashSprite();
            flashRenderer.color = new Color(1f, 0.8f, 0.2f, 1f);
            flashRenderer.sortingOrder = 15;
            flashRenderer.enabled = false;
        }

        public void Flash(Vector2 direction)
        {
            if (flashRenderer == null) return;
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(direction));
        }

        private IEnumerator FlashRoutine(Vector2 direction)
        {
            flashRenderer.enabled = true;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            flashRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);
            flashRenderer.transform.localScale = Vector3.one * scale;

            yield return new WaitForSeconds(duration);

            flashRenderer.enabled = false;
        }

        private Sprite CreateFlashSprite()
        {
            // 使用静态共享Sprite，避免每个MuzzleFlash实例都创建新Texture2D
            if (sharedFlashSprite != null) return sharedFlashSprite;

            int size = 16;
            var tex = new Texture2D(size, size);
            var center = new Vector2(size / 2f, size / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(1f - dist / (size / 2f));
                    tex.SetPixel(x, y, new Color(1f, 0.9f, 0.5f, alpha));
                }
            }
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            sharedFlashSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0f, 0.5f), 16f);
            sharedFlashSprite.name = "MuzzleFlash_Shared";
            return sharedFlashSprite;
        }
    }
}
