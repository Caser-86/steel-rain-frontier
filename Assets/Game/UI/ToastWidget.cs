using System.Collections;
using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class ToastWidget : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private float visibleSeconds = 1.8f;

        private Coroutine routine;

        private void Awake()
        {
            SetText("");
        }

        private void OnEnable()
        {
            GameEvents.CheckpointReached += ShowCheckpointSaved;
            GameEvents.SaveCompleted += ShowAutosaved;
        }

        private void OnDisable()
        {
            GameEvents.CheckpointReached -= ShowCheckpointSaved;
            GameEvents.SaveCompleted -= ShowAutosaved;
        }

        private void ShowCheckpointSaved()
        {
            Show(CombatHudText.FormatCheckpointToast());
        }

        private void ShowAutosaved()
        {
            Show("AUTOSAVED");
        }

        private void Show(string message)
        {
            if (routine != null)
                StopCoroutine(routine);

            routine = StartCoroutine(ShowRoutine(message));
        }

        private IEnumerator ShowRoutine(string message)
        {
            SetText(message);
            yield return new WaitForSeconds(visibleSeconds);
            SetText("");
            routine = null;
        }

        private void SetText(string text)
        {
            if (label != null)
                label.text = text;
        }
    }
}
