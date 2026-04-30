using SteelRain.Core;
using SteelRain.VFX;
using UnityEngine;

namespace SteelRain.Enemies
{
    [RequireComponent(typeof(Renderer))]
    public sealed class BossWeakPointVisual : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Color normalColor = new(1f, 0.9f, 0.05f, 1f);
        [SerializeField] private Color exposedColor = new(1f, 0.05f, 0.02f, 1f);
        [SerializeField] private float pulseSpeed = 7f;
        [SerializeField] private float pulseScale = 0.18f;

        private Renderer targetRenderer;
        private MaterialPropertyBlock block;
        private Vector3 baseScale;
        private bool exposed;

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            block = new MaterialPropertyBlock();
            baseScale = transform.localScale;
            ApplyColor(normalColor);
        }

        private void OnEnable()
        {
            GameEvents.BossPhaseChanged += OnBossPhaseChanged;
        }

        private void OnDisable()
        {
            GameEvents.BossPhaseChanged -= OnBossPhaseChanged;
        }

        private void Update()
        {
            var amount = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            transform.localScale = baseScale * (exposed ? amount * 1.25f : amount);
        }

        private void OnBossPhaseChanged(string phaseName)
        {
            if (phaseName.Contains("CORE EXPOSED"))
            {
                exposed = true;
                ApplyColor(exposedColor);
                CameraShake.ShakeGlobal(0.16f, 0.18f);
                ImpactBurst.Spawn(transform.position, exposedColor, 0.35f, 2f, 0.2f);
            }
            else if (phaseName.Contains("DEFEATED"))
            {
                gameObject.SetActive(false);
            }
        }

        private void ApplyColor(Color color)
        {
            block.SetColor(ColorId, color);
            targetRenderer.SetPropertyBlock(block);
        }
    }
}
