using System.Collections;
using System;
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.VFX
{
    [RequireComponent(typeof(Health))]
    public sealed class HitFlash : MonoBehaviour
    {
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float duration = 0.06f;

        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock block;
        private Renderer[] renderers;
        private Health health;
        private Coroutine routine;
        private Action<DamageInfo> damageHandler;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            renderers = GetComponentsInChildren<Renderer>();
            health = GetComponent<Health>();
            damageHandler = _ => Flash();
            health.Damaged += damageHandler;
        }

        private void OnDestroy()
        {
            if (health != null)
                health.Damaged -= damageHandler;
        }

        private void Flash()
        {
            if (routine != null)
                StopCoroutine(routine);

            routine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            block.SetColor(ColorId, flashColor);
            foreach (var targetRenderer in renderers)
                targetRenderer.SetPropertyBlock(block);

            yield return new WaitForSeconds(duration);

            foreach (var targetRenderer in renderers)
                targetRenderer.SetPropertyBlock(null);

            routine = null;
        }
    }
}
