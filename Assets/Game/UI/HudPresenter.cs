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
            if (healthWidget != null)
                GameEvents.PlayerHealthChanged += healthWidget.SetHealth;

            if (ammoWidget != null)
            {
                GameEvents.AmmoChanged += ammoWidget.SetAmmo;
                GameEvents.WeaponFormChanged += ammoWidget.SetForm;
                GameEvents.WeaponLevelChanged += ammoWidget.SetWeaponLevel;
                GameEvents.PlayerCharacterChanged += ammoWidget.SetCharacter;
            }
        }

        private void OnDisable()
        {
            if (healthWidget != null)
                GameEvents.PlayerHealthChanged -= healthWidget.SetHealth;

            if (ammoWidget != null)
            {
                GameEvents.AmmoChanged -= ammoWidget.SetAmmo;
                GameEvents.WeaponFormChanged -= ammoWidget.SetForm;
                GameEvents.WeaponLevelChanged -= ammoWidget.SetWeaponLevel;
                GameEvents.PlayerCharacterChanged -= ammoWidget.SetCharacter;
            }
        }
    }
}
