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
        // 爆炸伤害从5降至3：玩家最大HP为5，原值会导致满血被一击必杀。
        // 3点伤害仍具威胁（60%血量），但给玩家反应和回血的机会。
        [SerializeField] private int explosionDamage = 3;
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
                if (hit.transform == transform) continue; // 不伤害自己
                if (hit.TryGetComponent(out Health hp))
                {
                    // 使用Neutral作为来源，这样爆炸会伤害玩家和敌人（爆炸桶是中立物体）
                    var dir = (hp.transform.position - transform.position).normalized;
                    if (dir == Vector3.zero) dir = Vector2.up;
                    hp.ApplyDamage(new DamageInfo(explosionDamage, Team.Neutral, dir));
                }
            }

            Destroy(gameObject);
        }
    }
}
