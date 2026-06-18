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
        private Color originalColor;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            health = GetComponent<Health>();
            originalColor = spriteRenderer.color;
            health.Damaged += _ => StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(duration);
            spriteRenderer.color = originalColor;
        }
    }
}
