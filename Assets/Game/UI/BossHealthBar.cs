using SteelRain.Core;
using SteelRain.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Image fillImage;
        [SerializeField] private Text nameText;

        private Health bossHealth;
        private bool tracking;

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
            GameEvents.BossDefeated += OnBossDefeated;
        }

        private void OnDestroy()
        {
            GameEvents.BossDefeated -= OnBossDefeated;
        }

        private void Update()
        {
            if (!tracking || bossHealth == null) return;
            if (fillImage != null)
                fillImage.fillAmount = (float)bossHealth.Current / bossHealth.Max;
        }

        public void TrackBoss(Health health, string bossName)
        {
            bossHealth = health;
            tracking = true;
            if (panel != null) panel.SetActive(true);
            if (nameText != null) nameText.text = bossName;
            if (fillImage != null) fillImage.fillAmount = 1f;
        }

        private void OnBossDefeated()
        {
            tracking = false;
            if (panel != null) panel.SetActive(false);
        }
    }
}
