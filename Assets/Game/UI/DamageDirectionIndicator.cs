using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// 受击方向提示：玩家受伤时屏幕边缘出现红色箭头指示伤害来源。
    /// </summary>
    public sealed class DamageDirectionIndicator : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Image arrowPrefab;
        [SerializeField] private float displayDuration = 1.5f;
        [SerializeField] private Color arrowColor = new Color(1f, 0.2f, 0.2f, 0.8f);

        private void OnEnable()
        {
            GameEvents.PlayerDamaged += OnPlayerDamaged;
            GameEvents.PlayerDied += OnPlayerDied;
        }

        private void OnDisable()
        {
            GameEvents.PlayerDamaged -= OnPlayerDamaged;
            GameEvents.PlayerDied -= OnPlayerDied;
        }

        private void OnPlayerDamaged(Vector2 damageDirection)
        {
            if (player == null || arrowPrefab == null) return;

            // damageDirection 是伤害的方向向量，箭头应指向来源（反方向）
            var sourceDir = -damageDirection.normalized;
            if (sourceDir == Vector2.zero) sourceDir = Vector2.down;
            var angle = Mathf.Atan2(sourceDir.y, sourceDir.x) * Mathf.Rad2Deg;

            var arrow = Instantiate(arrowPrefab, transform);
            arrow.color = arrowColor;
            arrow.rectTransform.anchoredPosition = sourceDir * 280f;
            arrow.rectTransform.rotation = Quaternion.Euler(0, 0, angle);

            StartCoroutine(FadeAndDestroy(arrow, displayDuration));
        }

        private System.Collections.IEnumerator FadeAndDestroy(Image arrow, float duration)
        {
            float elapsed = 0f;
            var startColor = arrow.color;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var c = startColor;
                c.a = startColor.a * (1f - elapsed / duration);
                arrow.color = c;
                yield return null;
            }
            if (arrow != null) Destroy(arrow.gameObject);
        }

        private void OnPlayerDied()
        {
            // 清除所有箭头
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
        }
    }
}
