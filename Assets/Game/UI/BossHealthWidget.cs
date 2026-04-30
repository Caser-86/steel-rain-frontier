using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class BossHealthWidget : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private Text label;
        [SerializeField] private Text phaseLabel;

        private CanvasGroup group;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            if (group == null)
                group = gameObject.AddComponent<CanvasGroup>();

            SetVisible(false);
        }

        private void OnEnable()
        {
            GameEvents.BossHealthChanged += SetHealth;
            GameEvents.BossPhaseChanged += SetPhase;
        }

        private void OnDisable()
        {
            GameEvents.BossHealthChanged -= SetHealth;
            GameEvents.BossPhaseChanged -= SetPhase;
        }

        private void SetHealth(string bossName, int current, int max)
        {
            SetVisible(true);

            if (fill != null)
                fill.fillAmount = max <= 0 ? 0f : Mathf.Clamp01((float)current / max);

            if (label != null)
                label.text = $"{bossName} {current}/{max}";
        }

        private void SetPhase(string phaseName)
        {
            if (phaseLabel != null)
                phaseLabel.text = phaseName;

            if (phaseName == "DEFEATED")
                Invoke(nameof(HideAfterDefeat), 1.6f);
        }

        private void HideAfterDefeat()
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
