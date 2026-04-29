using TMPro;
using UnityEngine;

namespace SteelRain.UI
{
    public sealed class AmmoWidget : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        private string weaponName = "";
        private int ammo;
        private string formName = "";

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

        private void Refresh()
        {
            if (label == null)
                return;

            label.text = $"{weaponName} {ammo} [{formName}]";
        }
    }
}
