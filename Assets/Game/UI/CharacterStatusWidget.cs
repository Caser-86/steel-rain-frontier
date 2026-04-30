using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class CharacterStatusWidget : MonoBehaviour
    {
        [SerializeField] private Text label;

        private string characterName = "Aila";
        private string skillStatus = "Skill Locked";

        private void Start()
        {
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
            if (label != null)
                label.text = CombatHudText.FormatCharacter(characterName, skillStatus);
        }
    }
}
