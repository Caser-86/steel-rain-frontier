using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Image damageFillImage;
        [SerializeField] private Text healthText;

        private float maxDisplayHealth;
        private float currentDisplayHealth;
        private float displayHealth;
        private float damageLagSpeed = 5f;

        private void OnEnable()
        {
            GameEvents.PlayerHealthChanged += OnHealthChanged;
            if (damageFillImage != null)
                damageFillImage.fillAmount = 1f;
        }

        private void OnDisable()
        {
            GameEvents.PlayerHealthChanged -= OnHealthChanged;
        }

        private void Update()
        {
            if (damageFillImage == null) return;
            if (displayHealth > currentDisplayHealth)
            {
                displayHealth = Mathf.Lerp(displayHealth, currentDisplayHealth, Time.deltaTime * damageLagSpeed);
                damageFillImage.fillAmount = maxDisplayHealth > 0 ? displayHealth / maxDisplayHealth : 0f;
            }
            else
            {
                damageFillImage.fillAmount = currentDisplayHealth > 0 ? currentDisplayHealth / maxDisplayHealth : 0f;
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            maxDisplayHealth = max;
            currentDisplayHealth = current;
            if (fillImage != null)
                fillImage.fillAmount = max > 0 ? (float)current / max : 0f;
            if (healthText != null)
                healthText.text = $"{current}/{max}";
        }
    }
}
