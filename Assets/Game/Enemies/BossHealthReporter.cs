using SteelRain.Core;
using SteelRain.VFX;
using UnityEngine;

namespace SteelRain.Enemies
{
    [RequireComponent(typeof(Health))]
    public sealed class BossHealthReporter : MonoBehaviour
    {
        [SerializeField] private string displayName = "Siege Walker";
        [SerializeField] private float enragedThreshold = 0.5f;

        private Health health;
        private bool enraged;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Changed += OnHealthChanged;
            health.Died += OnDied;
        }

        private void Start()
        {
            PublishHealth(health.Current, health.Max);
            GameEvents.RaiseBossPhaseChanged("PHASE 1: ADVANCING");
        }

        private void OnDestroy()
        {
            if (health == null)
                return;

            health.Changed -= OnHealthChanged;
            health.Died -= OnDied;
        }

        private void OnHealthChanged(int current, int max)
        {
            PublishHealth(current, max);

            if (!enraged && current > 0 && current <= Mathf.CeilToInt(max * enragedThreshold))
            {
                enraged = true;
                GameEvents.RaiseBossPhaseChanged("PHASE 2: RAGE - CORE EXPOSED");
                CameraShake.ShakeGlobal(0.28f, 0.35f);
                ImpactBurst.Spawn(transform.position + Vector3.up * 0.6f, new Color(1f, 0.2f, 0.05f, 0.85f), 0.9f, 4.2f, 0.32f);
            }
        }

        private void OnDied()
        {
            PublishHealth(0, health.Max);
            GameEvents.RaiseBossPhaseChanged("DEFEATED");
        }

        private void PublishHealth(int current, int max)
        {
            GameEvents.RaiseBossHealthChanged(displayName, current, max);
        }
    }
}
