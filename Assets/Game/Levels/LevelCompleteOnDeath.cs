using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Levels
{
    [RequireComponent(typeof(Health))]
    public sealed class LevelCompleteOnDeath : MonoBehaviour
    {
        private Health health;
        private bool completed;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Died += CompleteLevel;
        }

        private void OnDestroy()
        {
            if (health != null)
                health.Died -= CompleteLevel;
        }

        private void CompleteLevel()
        {
            if (completed)
                return;

            completed = true;
            GameEvents.RaiseLevelCompleted();
        }
    }
}
