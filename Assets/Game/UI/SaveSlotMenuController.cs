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

        private bool pendingDeleteConfirm;

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
            pendingDeleteConfirm = false;
            SetStatus($"Current slot: {SaveService.CurrentSlot}");
            Refresh();
        }

        public void DeleteCurrentSlot()
        {
            if (!SaveService.HasSave)
            {
                pendingDeleteConfirm = false;
                SetStatus($"Slot {SaveService.CurrentSlot} is already empty.");
                return;
            }

            if (!pendingDeleteConfirm)
            {
                pendingDeleteConfirm = true;
                SetStatus(FormatDeleteConfirmPrompt(SaveService.CurrentSlot));
                return;
            }

            SaveService.Delete();
            pendingDeleteConfirm = false;
            SetStatus($"Slot {SaveService.CurrentSlot} cleared.");
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

        public static string FormatDeleteConfirmPrompt(int slot)
        {
            return $"Press DELETE again to clear Slot {SaveService.NormalizeSlot(slot)}";
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
