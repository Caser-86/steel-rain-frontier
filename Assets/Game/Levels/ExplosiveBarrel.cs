using SteelRain.Core;
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
