using System.Collections;
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.VFX
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Health))]
    public sealed class HitFlash : MonoBehaviour
    {
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float duration = 0.06f;

        private SpriteRenderer spriteRenderer;
        private Health health;
        private Coroutine flashRoutine;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            health = GetComponent<Health>();
            health.Damaged += _ => StartFlash();
        }

        private void StartFlash()
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            var originalColor = spriteRenderer.color;
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(duration);
            // 恢复到flash开始时的颜色，而非缓存的originalColor
            // 这样如果其他脚本在flash期间修改了颜色，不会被覆盖
            if (spriteRenderer.color == flashColor)
                spriteRenderer.color = originalColor;
        }
    }
}
