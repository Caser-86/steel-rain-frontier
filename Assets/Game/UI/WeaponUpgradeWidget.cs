using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class WeaponUpgradeWidget : MonoBehaviour
    {
        [SerializeField] private Text label;

        private string weaponName = "Assault Rifle";
        private int ammo = -1;
        private string formName = "Auto";
        private int weaponLevel;

        private void Start()
        {
            Refresh();
        }

        public void SetAmmo(string newWeaponName, int newAmmo)
        {
            weaponName = newWeaponName;
            ammo = newAmmo;
            Refresh();
        }

        public void SetForm(string newFormName)
        {
            formName = newFormName;
            Refresh();
        }

        public void SetWeaponLevel(int newWeaponLevel)
        {
            weaponLevel = newWeaponLevel;
            Refresh();
        }

        private void Refresh()
        {
            if (label == null)
                return;

            var ammoText = ammo < 0 ? "INF" : ammo.ToString();
            label.text = $"{weaponName} [{formName}] {ammoText} | {CombatHudText.FormatWeaponLevel(weaponLevel)}";
        }
    }
}
