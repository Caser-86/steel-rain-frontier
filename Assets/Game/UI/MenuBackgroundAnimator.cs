using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// 主菜单背景动效：星星/火花浮动 + 偶尔扫过的光带。
    /// 挂在带 RectTransform 的 UI 节点上即可。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class MenuBackgroundAnimator : MonoBehaviour
    {
        [SerializeField] private int starCount = 28;
        [SerializeField] private float driftSpeed = 12f;
        [SerializeField] private float driftRange = 8f;
        [SerializeField] private float pulseSpeed = 1.4f;

        private RectTransform rect;
        private Image[] stars;
        private Vector2[] basePositions;
        private Vector2[] driftDirs;
        private float[] pulseSeeds;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            SpawnStars();
        }

        private void Update()
        {
            if (stars == null) return;
            float t = Time.unscaledTime;
            for (int i = 0; i < stars.Length; i++)
            {
                var img = stars[i];
                if (img == null) continue;
                var rt = img.rectTransform;
                // 漂浮
                Vector2 dir = driftDirs[i];
                float phase = t * driftSpeed + i * 0.7f;
                Vector2 offset = new Vector2(
                    Mathf.Sin(phase) * driftRange,
                    Mathf.Cos(phase * 0.83f) * driftRange);
                rt.anchoredPosition = basePositions[i] + offset;
                // 闪烁
                float pulse = 0.35f + 0.65f * (0.5f + 0.5f * Mathf.Sin(t * pulseSpeed + pulseSeeds[i]));
                var c = img.color; c.a = pulse; img.color = c;
            }
        }

        private void SpawnStars()
        {
            stars = new Image[starCount];
            basePositions = new Vector2[starCount];
            driftDirs = new Vector2[starCount];
            pulseSeeds = new float[starCount];
            for (int i = 0; i < starCount; i++)
            {
                var go = new GameObject($"Star_{i}");
                go.transform.SetParent(transform, false);
                var img = go.AddComponent<Image>();
                img.raycastTarget = false;
                float size = Random.Range(2f, 6f);
                img.rectTransform.sizeDelta = new Vector2(size, size);
                // 颜色：橙/蓝/白 随机
                Color c = Random.value switch
                {
                    < 0.4f => UIPalette.Primary,
                    < 0.7f => UIPalette.Accent,
                    _ => UIPalette.TextPrimary
                };
                c.a = 0.6f;
                img.color = c;
                basePositions[i] = new Vector2(
                    Random.Range(-960f, 960f),
                    Random.Range(-540f, 540f));
                driftDirs[i] = Random.insideUnitCircle.normalized;
                pulseSeeds[i] = Random.Range(0f, 6.28f);
                img.rectTransform.anchoredPosition = basePositions[i];
                stars[i] = img;
            }
        }
    }
}
