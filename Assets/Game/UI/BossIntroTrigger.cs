using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class BossIntroTrigger : MonoBehaviour
    {
        [SerializeField] private Text warningText;
        [SerializeField] private float displayDuration = 2f;

        private bool triggered;

        private void Start()
        {
            if (warningText != null) warningText.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered || !other.CompareTag("Player")) return;
            triggered = true;
            StartCoroutine(ShowWarning());
        }

        private IEnumerator ShowWarning()
        {
            if (warningText == null) yield break;
            warningText.text = "WARNING";
            warningText.color = new Color(1f, 0.2f, 0.2f, 1f);
            warningText.enabled = true;

            float elapsed = 0f;
            while (elapsed < displayDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var pulse = Mathf.Sin(elapsed * 8f) * 0.3f + 0.7f;
                var c = warningText.color;
                c.a = pulse;
                warningText.color = c;
                yield return null;
            }

            warningText.enabled = false;
        }
    }
}
