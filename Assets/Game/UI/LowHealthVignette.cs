using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class LowHealthVignette : MonoBehaviour
    {
        [SerializeField] private Image vignetteImage;
        [SerializeField] private float threshold = 0.3f;

        private void OnEnable()
        {
            GameEvents.PlayerHealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            GameEvents.PlayerHealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(int current, int max)
        {
            if (vignetteImage == null || max <= 0) return;
            var ratio = (float)current / max;
            if (ratio <= threshold)
            {
                var intensity = 1f - (ratio / threshold);
                var c = vignetteImage.color;
                c.a = intensity * 0.5f;
                vignetteImage.color = c;
            }
            else
            {
                var c = vignetteImage.color;
                c.a = 0f;
                vignetteImage.color = c;
            }
        }
    }
}
