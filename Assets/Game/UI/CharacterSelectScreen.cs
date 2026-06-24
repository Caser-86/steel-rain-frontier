using SteelRain.Player;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// 角色选择界面（合金弹头简化版）。
    /// 4个角色全部可用，无解锁条件。选择后设置首选角色。
    /// </summary>
    public sealed class CharacterSelectScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Text characterNameText;
        [SerializeField] private Text characterStyleText;
        [SerializeField] private Text characterLoreText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button backButton;

        [Header("Character Slots (4 buttons)")]
        [SerializeField] private Button[] characterButtons;

        [Header("Character Definitions")]
        [SerializeField] private CharacterDefinition[] characters;

        private int selectedIndex;
        private bool shown;

        private const string KeyPreferredCharacter = "Save_PreferredCharacter";

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);

            if (confirmButton != null)
                confirmButton.onClick.AddListener(ConfirmSelection);
            if (backButton != null)
                backButton.onClick.AddListener(Hide);

            if (characterButtons != null)
            {
                for (int i = 0; i < characterButtons.Length; i++)
                {
                    var idx = i;
                    characterButtons[i].onClick.AddListener(() => SelectCharacter(idx));
                }
            }
        }

        public void Show()
        {
            if (panel == null) return;
            shown = true;
            panel.SetActive(true);
            Time.timeScale = 0f;

            var preferred = GetPreferredCharacterId();
            selectedIndex = 0;
            if (characters != null)
            {
                for (int i = 0; i < characters.Length; i++)
                {
                    if (characters[i] != null && characters[i].id == preferred)
                    {
                        selectedIndex = i;
                        break;
                    }
                }
            }

            SelectCharacter(selectedIndex);
        }

        public void Hide()
        {
            shown = false;
            if (panel != null) panel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void SelectCharacter(int index)
        {
            if (characters == null || index < 0 || index >= characters.Length) return;
            if (characters[index] == null) return;

            selectedIndex = index;
            var def = characters[index];

            if (characterNameText != null)
                characterNameText.text = def.displayName;

            if (characterStyleText != null)
                characterStyleText.text = def.combatStyle;

            if (characterLoreText != null)
                characterLoreText.text = def.lore;

            HighlightSelectedButton(index);
        }

        private void ConfirmSelection()
        {
            if (characters == null || selectedIndex >= characters.Length) return;
            var def = characters[selectedIndex];
            if (def == null) return;

            SavePreferredCharacter(def.id);
            Hide();
        }

        private void HighlightSelectedButton(int index)
        {
            if (characterButtons == null) return;
            for (int i = 0; i < characterButtons.Length; i++)
            {
                var img = characterButtons[i]?.GetComponent<Image>();
                if (img != null)
                    img.color = i == index ? new Color(0.3f, 0.6f, 1f) : new Color(0.2f, 0.3f, 0.5f);
            }
        }

        public static string GetPreferredCharacterId()
        {
            return PlayerPrefs.GetString(KeyPreferredCharacter, "aila");
        }

        public static void SavePreferredCharacter(string characterId)
        {
            PlayerPrefs.SetString(KeyPreferredCharacter, characterId);
            PlayerPrefs.Save();
        }
    }
}
