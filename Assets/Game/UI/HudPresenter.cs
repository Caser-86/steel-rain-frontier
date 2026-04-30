using SteelRain.Core;
using UnityEngine;

namespace SteelRain.UI
{
    public sealed class HudPresenter : MonoBehaviour
    {
        [SerializeField] private HealthWidget healthWidget;
        [SerializeField] private AmmoWidget ammoWidget;
        [SerializeField] private CharacterStatusWidget characterStatusWidget;
        [SerializeField] private WeaponUpgradeWidget weaponUpgradeWidget;

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
                GameEvents.SkillStatusChanged += ammoWidget.SetSkillStatus;
            }

            if (characterStatusWidget != null)
            {
                GameEvents.PlayerCharacterChanged += characterStatusWidget.SetCharacter;
                GameEvents.SkillStatusChanged += characterStatusWidget.SetSkillStatus;
            }

            if (weaponUpgradeWidget != null)
            {
                GameEvents.AmmoChanged += weaponUpgradeWidget.SetAmmo;
                GameEvents.WeaponFormChanged += weaponUpgradeWidget.SetForm;
                GameEvents.WeaponLevelChanged += weaponUpgradeWidget.SetWeaponLevel;
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
                GameEvents.SkillStatusChanged -= ammoWidget.SetSkillStatus;
            }

            if (characterStatusWidget != null)
            {
                GameEvents.PlayerCharacterChanged -= characterStatusWidget.SetCharacter;
                GameEvents.SkillStatusChanged -= characterStatusWidget.SetSkillStatus;
            }

            if (weaponUpgradeWidget != null)
            {
                GameEvents.AmmoChanged -= weaponUpgradeWidget.SetAmmo;
                GameEvents.WeaponFormChanged -= weaponUpgradeWidget.SetForm;
                GameEvents.WeaponLevelChanged -= weaponUpgradeWidget.SetWeaponLevel;
            }
        }
    }
}
