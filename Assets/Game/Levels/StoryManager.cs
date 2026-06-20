using System.Collections;
using SteelRain.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.Levels
{
    /// <summary>
    /// 叙事管理器：在关卡开始、Boss战前、关卡结束时显示文本过场，
    /// 为4人小队提供情感投入和世界观构建。
    /// </summary>
    public sealed class StoryManager : MonoBehaviour
    {
        [System.Serializable]
        public struct StoryBeat
        {
            [TextArea(2, 4)] public string speaker;
            [TextArea(3, 6)] public string text;
            public float displayDuration;
        }

        [Header("UI References")]
        [SerializeField] private GameObject storyPanel;
        [SerializeField] private Text speakerText;
        [SerializeField] private Text bodyText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Level Intro")]
        [SerializeField] private StoryBeat[] introBeats;

        [Header("Boss Warning")]
        [SerializeField] private StoryBeat[] bossWarningBeats;

        [Header("Level Outro")]
        [SerializeField] private StoryBeat[] outroBeats;

        [SerializeField] private float fadeDuration = 0.5f;

        private void Start()
        {
            if (storyPanel != null) storyPanel.SetActive(false);
            if (introBeats != null && introBeats.Length > 0)
                StartCoroutine(PlaySequence(introBeats));
        }

        /// <summary>
        /// Boss战前触发警告过场。
        /// </summary>
        public void PlayBossWarning()
        {
            if (bossWarningBeats != null && bossWarningBeats.Length > 0)
                StartCoroutine(PlaySequence(bossWarningBeats));
        }

        /// <summary>
        /// 关卡结束时播放结局过场。
        /// </summary>
        public void PlayOutro()
        {
            if (outroBeats != null && outroBeats.Length > 0)
                StartCoroutine(PlaySequence(outroBeats));
        }

        private IEnumerator PlaySequence(StoryBeat[] beats)
        {
            // 暂停游戏流程（但不冻结，仅暂停输入相关的timeScale可后续扩展）
            if (storyPanel != null) storyPanel.SetActive(true);

            foreach (var beat in beats)
            {
                yield return StartCoroutine(FadeIn());
                if (speakerText != null) speakerText.text = beat.speaker ?? "";
                if (bodyText != null) bodyText.text = beat.text ?? "";
                AudioManager.Play("sfx_checkpoint", 0.5f);

                var duration = beat.displayDuration > 0 ? beat.displayDuration : 3f;
                yield return new WaitForSecondsRealtime(duration);
                yield return StartCoroutine(FadeOut());
            }

            if (storyPanel != null) storyPanel.SetActive(false);
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
    }
}
