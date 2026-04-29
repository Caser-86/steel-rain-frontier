using SteelRain.Core;
using UnityEngine;

namespace SteelRain.UI
{
    public sealed class HudPresenter : MonoBehaviour
    {
        [SerializeField] private HealthWidget healthWidget;
        [SerializeField] private AmmoWidget ammoWidget;

        private void OnEnable()
        {
            GameEvents.PlayerHealthChanged += healthWidget.SetHealth;
            GameEvents.AmmoChanged += ammoWidget.SetAmmo;
            GameEvents.WeaponFormChanged += ammoWidget.SetForm;
        }

        private void OnDisable()
        {
            GameEvents.PlayerHealthChanged -= healthWidget.SetHealth;
            GameEvents.AmmoChanged -= ammoWidget.SetAmmo;
            GameEvents.WeaponFormChanged -= ammoWidget.SetForm;
        }
    }
}
