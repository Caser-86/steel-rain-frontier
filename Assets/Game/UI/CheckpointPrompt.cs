using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// 检查点到达提示：屏幕中央显示 2 秒"检查点已到达"。
    /// </summary>
    public sealed class CheckpointPrompt : MonoBehaviour
    {
        [SerializeField] private Text promptText;
        [SerializeField] private float displayDuration = 2f;

        private void OnEnable()
        {
            GameEvents.CheckpointReached += ShowPrompt;
        }

        private void OnDisable()
        {
            GameEvents.CheckpointReached -= ShowPrompt;
        }

        private void ShowPrompt()
        {
            if (promptText != null)
                StartCoroutine(ShowRoutine());
        }

        private System.Collections.IEnumerator ShowRoutine()
        {
            promptText.enabled = true;
            promptText.text = "CHECKPOINT REACHED";
            promptText.color = new Color(0.9f, 0.85f, 0.2f, 1f);

            float elapsed = 0f;
            while (elapsed < displayDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var c = promptText.color;
                c.a = 1f - (elapsed / displayDuration);
                promptText.color = c;
                yield return null;
            }

            promptText.enabled = false;
        }
    }
}
