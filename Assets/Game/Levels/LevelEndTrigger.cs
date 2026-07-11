using SteelRain.Core;
using SteelRain.UI;
using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class LevelEndTrigger : MonoBehaviour
    {
        [SerializeField] private int nextLevelIndex = -1;

        private bool triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered || !other.CompareTag("Player")) return;
            triggered = true;

            // 通知成就系统关卡完成
            var tracker = FindFirstObjectByType<AchievementTracker>();
            if (tracker != null) tracker.OnLevelComplete();

            if (!LevelManager.InEndlessMode && LevelManager.CurrentLevel >= LevelManager.TotalLevels - 1)
            {
                var victory = FindFirstObjectByType<VictoryScreen>();
                if (victory != null)
                {
                    victory.Show();
                    return;
                }

                var complete = FindFirstObjectByType<GameCompleteScreen>();
                if (complete != null)
                {
                    complete.TryShow();
                    Time.timeScale = 0f;
                    return;
                }
            }

            if (nextLevelIndex >= 0)
                LevelManager.LoadLevel(nextLevelIndex);
            else
                LevelManager.LoadNextLevel();
        }
    }
}
