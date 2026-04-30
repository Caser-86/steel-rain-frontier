using SteelRain.Core;
using SteelRain.VFX;
using UnityEngine;

namespace SteelRain.Enemies
{
    [RequireComponent(typeof(Health))]
    public sealed class BossHealthReporter : MonoBehaviour
    {
        [SerializeField] private string displayName = "Siege Walker";
        private Health health;
        private BossPhase currentPhase = BossPhase.Advancing;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Changed += OnHealthChanged;
            health.Died += OnDied;
        }

        private void Start()
        {
            PublishHealth(health.Current, health.Max);
            PublishPhase(BossPhase.Advancing);
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

            var nextPhase = BossPhaseTactics.GetPhase(current, max);
            if (current > 0 && nextPhase != currentPhase)
                PublishPhase(nextPhase);
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

        private void PublishPhase(BossPhase phase)
        {
            currentPhase = phase;
            GameEvents.RaiseBossPhaseChanged(BossPhaseTactics.GetPhaseLabel(phase));
            if (phase == BossPhase.Advancing)
                return;

            var burstColor = phase == BossPhase.CoreExposed
                ? new Color(1f, 0.85f, 0.05f, 0.9f)
                : new Color(1f, 0.2f, 0.05f, 0.85f);
            CameraShake.ShakeGlobal(0.28f, phase == BossPhase.CoreExposed ? 0.42f : 0.35f);
            ImpactBurst.Spawn(transform.position + Vector3.up * 0.6f, burstColor, 0.9f, phase == BossPhase.CoreExposed ? 5f : 4.2f, 0.32f);
        }
    }
}
