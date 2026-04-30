using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class MissionStatusWidget : MonoBehaviour
    {
        [SerializeField] private Text label;

        private void Awake()
        {
            Hide();
        }

        private void OnEnable()
        {
            GameEvents.LevelCompleted += ShowVictory;
            GameEvents.SquadDefeated += ShowDefeat;
        }

        private void OnDisable()
        {
            GameEvents.LevelCompleted -= ShowVictory;
            GameEvents.SquadDefeated -= ShowDefeat;
        }

        private void ShowVictory()
        {
            Show("MISSION CLEAR\nR: Restart   Esc: Quit", new Color(1f, 0.92f, 0.2f));
        }

        private void ShowDefeat()
        {
            Show("MISSION FAILED\nR: Restart   Esc: Quit", new Color(1f, 0.18f, 0.12f));
        }

        private void Show(string text, Color color)
        {
            if (label == null)
                return;

            label.text = text;
            label.color = color;
            label.enabled = true;
        }

        private void Hide()
        {
            if (label != null)
                label.enabled = false;
        }
    }
}
