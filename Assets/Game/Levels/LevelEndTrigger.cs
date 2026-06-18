using SteelRain.Core;
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

            if (nextLevelIndex >= 0)
                LevelManager.LoadLevel(nextLevelIndex);
            else
                LevelManager.LoadNextLevel();
        }
    }
}
