using System;
using SteelRain.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class SaveSlotMenuController : MonoBehaviour
    {
        private const string MainMenuSceneName = "MainMenu";

        [SerializeField] private Text statusLabel;
        [SerializeField] private Text slotOneLabel;
        [SerializeField] private Text slotTwoLabel;
        [SerializeField] private Text slotThreeLabel;

        private void Start()
        {
            Refresh();
        }

        public void SelectSlotOne()
        {
            SelectSlot(1);
        }

        public void SelectSlotTwo()
        {
            SelectSlot(2);
        }

        public void SelectSlotThree()
        {
            SelectSlot(3);
        }

        public void BackToMainMenu()
        {
            SceneManager.LoadScene(MainMenuSceneName);
        }

        public void SelectSlot(int slot)
        {
            SaveService.SetCurrentSlot(slot);
            SetStatus($"Current slot: {SaveService.CurrentSlot}");
            Refresh();
        }

        public static string FormatSlotLabel(int slot, SaveData save)
        {
            var normalizedSlot = SaveService.NormalizeSlot(slot);
            if (save == null)
                return $"Slot {normalizedSlot} - Empty";

            var savedTime = FormatSavedTime(save.savedAtUtc);
            return $"Slot {normalizedSlot} - {save.levelId} - {savedTime}";
        }

        private static string FormatSavedTime(string savedAtUtc)
        {
            if (DateTime.TryParse(savedAtUtc, out var savedAt))
                return savedAt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm");

            return "Unknown time";
        }

        private void Refresh()
        {
            SetSlotLabel(slotOneLabel, 1);
            SetSlotLabel(slotTwoLabel, 2);
            SetSlotLabel(slotThreeLabel, 3);
        }

        private void SetSlotLabel(Text label, int slot)
        {
            if (label != null)
                label.text = FormatSlotLabel(slot, SaveService.LoadFromSlot(slot));
        }

        private void SetStatus(string text)
        {
            if (statusLabel != null)
                statusLabel.text = text;
        }
    }
}
