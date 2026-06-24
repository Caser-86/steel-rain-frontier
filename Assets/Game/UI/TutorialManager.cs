using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// 交互式首次用户体验（FTUE）管理器。
    /// 逐步引导玩家完成移动、射击、角色切换、武器形态切换、闪避，
    /// 每步需玩家实际操作确认后才进入下一步，避免"显示4秒就消失"的无效教学。
    /// </summary>
    public sealed class TutorialManager : MonoBehaviour
    {
        [System.Serializable]
        public struct TutorialStep
        {
            public string id;
            [TextArea(2, 3)] public string instruction;
            public KeyCode requiredKey;          // 需要按下的键（KeyCode.None 表示无需按键）
            public int requiredActionCount;       // 需要执行的次数（如射击3次）
            public bool waitForTrigger;           // 是否等待外部触发（如拾取武器）
        }

        [Header("UI References")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private Text instructionText;
        [SerializeField] private Text progressText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Tutorial Steps")]
        [SerializeField] private TutorialStep[] steps;

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.3f;

        private int currentIndex = -1;
        private int currentActionCount = 0;
        private bool stepCompleted = false;
        private static bool hasCompletedTutorial;

        private void Start()
        {
            // 已完成教学的玩家不再触发
            if (hasCompletedTutorial || steps == null || steps.Length == 0)
            {
                if (tutorialPanel != null) tutorialPanel.SetActive(false);
                enabled = false;
                return;
            }
            if (tutorialPanel != null) tutorialPanel.SetActive(true);
            StartCoroutine(ShowNextStep());
        }

        private void Update()
        {
            if (stepCompleted || currentIndex < 0 || currentIndex >= steps.Length) return;

            var step = steps[currentIndex];
            if (step.waitForTrigger) return; // 等待外部调用 CompleteCurrentStep

            if (step.requiredKey != KeyCode.None && Input.GetKeyDown(step.requiredKey))
            {
                currentActionCount++;
                UpdateProgress();
                if (currentActionCount >= Mathf.Max(1, step.requiredActionCount))
                    CompleteCurrentStep();
            }
        }

        private IEnumerator ShowNextStep()
        {
            currentIndex++;
            if (currentIndex >= steps.Length)
            {
                yield return StartCoroutine(FadeOut());
                hasCompletedTutorial = true;
                if (tutorialPanel != null) tutorialPanel.SetActive(false);
                enabled = false;
                yield break;
            }

            currentActionCount = 0;
            stepCompleted = false;

            yield return StartCoroutine(FadeIn());

            var step = steps[currentIndex];
            if (instructionText != null) instructionText.text = step.instruction;
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            if (progressText == null) return;
            var step = steps[currentIndex];
            var required = Mathf.Max(1, step.requiredActionCount);
            progressText.text = $"{currentActionCount}/{required}";
        }

        /// <summary>
        /// 外部系统（如拾取武器）调用以完成当前等待触发器的步骤。
        /// </summary>
        public void CompleteCurrentStep()
        {
            if (stepCompleted || currentIndex < 0 || currentIndex >= steps.Length) return;
            stepCompleted = true;
            StartCoroutine(ShowNextStep());
        }

        /// <summary>
        /// 累计动作计数（用于射击、跳跃等无固定键的步骤）。
        /// </summary>
        public void NotifyAction()
        {
            if (stepCompleted || currentIndex < 0 || currentIndex >= steps.Length) return;
            var step = steps[currentIndex];
            if (step.waitForTrigger) return;
            currentActionCount++;
            UpdateProgress();
            if (currentActionCount >= Mathf.Max(1, step.requiredActionCount))
                CompleteCurrentStep();
        }

        private IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;
            canvasGroup.alpha = 0f;
            var t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            if (canvasGroup == null) yield break;
            canvasGroup.alpha = 1f;
            var t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// 重置教学状态（用于测试或新存档）。
        /// </summary>
        public static void ResetTutorial()
        {
            hasCompletedTutorial = false;
        }
    }
}
