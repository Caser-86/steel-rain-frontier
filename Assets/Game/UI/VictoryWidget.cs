using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class VictoryWidget : MonoBehaviour
    {
        [SerializeField] private Text label;

        private void Awake()
        {
            if (label != null)
                label.enabled = false;
        }

        private void OnEnable()
        {
            GameEvents.LevelCompleted += Show;
        }

        private void OnDisable()
        {
            GameEvents.LevelCompleted -= Show;
        }

        private void Show()
        {
            if (label == null)
                return;

            label.text = "MISSION CLEAR\nR: Restart   Esc: Quit";
            label.enabled = true;
        }
    }
}
