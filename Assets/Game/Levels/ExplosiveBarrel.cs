using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.VFX;
using UnityEngine;

namespace SteelRain.Levels
{
    [RequireComponent(typeof(Health))]
    public sealed class ExplosiveBarrel : MonoBehaviour
    {
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private int explosionDamage = 5;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Initialize(1, Team.Neutral);
            health.Died += Explode;
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Explode()
        {
            AudioManager.Play("sfx_explosion", 0.8f);
            ExplosionEffect.Spawn(transform.position, 2f);

            var hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out Health hp))
                {
                    var dir = (hp.transform.position - transform.position).normalized;
                    hp.ApplyDamage(new DamageInfo(explosionDamage, Team.Neutral, dir));
                }
            }

            Destroy(gameObject);
        }
    }
}
