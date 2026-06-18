using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class TutorialPrompt : MonoBehaviour
    {
        [SerializeField] private Text promptText;
        [SerializeField] private string message;
        [SerializeField] private float displayDuration = 4f;

        private bool triggered;

        private void Start()
        {
            if (promptText != null) promptText.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered || !other.CompareTag("Player")) return;
            triggered = true;
            ShowPrompt();
        }

        private void ShowPrompt()
        {
            if (promptText == null) return;
            promptText.text = message;
            promptText.enabled = true;
            Invoke(nameof(HidePrompt), displayDuration);
        }

        private void HidePrompt()
        {
            if (promptText != null) promptText.enabled = false;
        }
    }
}
