using SteelRain.Core;
using SteelRain.VFX;
using UnityEngine;

namespace SteelRain.Levels
{
    [RequireComponent(typeof(Health))]
    public sealed class ExplosiveBarrel : MonoBehaviour
    {
        [SerializeField] private int damage = 3;
        [SerializeField] private float radius = 2.5f;

        private Health health;
        private bool exploded;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Died += Explode;
        }

        private void OnDestroy()
        {
            if (health != null)
                health.Died -= Explode;
        }

        private void Explode()
        {
            if (exploded)
                return;

            exploded = true;
            CameraShake.ShakeGlobal(0.18f, 0.22f);
            ImpactBurst.Spawn(transform.position, new Color(1f, 0.45f, 0.08f, 0.85f), 0.6f, radius * 2f, 0.28f);
            var hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent(out Health target))
                    continue;

                var direction = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
                target.ApplyDamage(new DamageInfo(damage, Team.Neutral, direction));
            }

            gameObject.SetActive(false);
        }
    }
}
