using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class AmmoWidget : MonoBehaviour
    {
        [SerializeField] private Text label;

        private string weaponName = "";
        private int ammo;
        private string formName = "";
        private int weaponLevel;
        private string characterName = "Aila";
        private string skillStatus = "Skill Locked";

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

        public void SetCharacter(string newCharacterName)
        {
            characterName = newCharacterName;
            Refresh();
        }

        public void SetSkillStatus(string newSkillStatus)
        {
            skillStatus = newSkillStatus;
            Refresh();
        }

        private void Refresh()
        {
            if (label == null)
                return;

            var ammoText = ammo < 0 ? "INF" : ammo.ToString();
            label.text = $"{characterName} | {weaponName} Lv{weaponLevel} {ammoText} [{formName}] | {skillStatus}";
        }
    }
}
