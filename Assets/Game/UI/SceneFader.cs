using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class SceneFader : MonoBehaviour
    {
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 0.5f;

        private static SceneFader instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            if (fadeImage != null)
            {
                fadeImage.raycastTarget = false;
                SetAlpha(0f);
            }
        }

        public static void FadeToScene(string sceneName)
        {
            if (instance != null)
                instance.StartCoroutine(instance.FadeRoutine(sceneName));
            else
            {
                Debug.LogWarning("[SceneFader] No instance, loading directly: " + sceneName);
                SceneManager.LoadScene(sceneName);
            }
        }

        private IEnumerator FadeRoutine(string sceneName)
        {
            yield return StartCoroutine(Fade(0f, 1f));
            SceneManager.LoadScene(sceneName);
            yield return StartCoroutine(Fade(1f, 0f));
        }

        private IEnumerator Fade(float from, float to)
        {
            if (fadeImage == null) yield break;
            var elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(from, to, elapsed / fadeDuration));
                yield return null;
            }
            SetAlpha(to);
        }

        private void SetAlpha(float alpha)
        {
            if (fadeImage == null) return;
            var c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
    }
}
