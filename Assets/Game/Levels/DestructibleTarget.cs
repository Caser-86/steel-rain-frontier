using SteelRain.Audio;
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Levels
{
    /// <summary>
    /// 可破坏目标：木箱、军火箱等。被击破后掉落奖励。
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class DestructibleTarget : MonoBehaviour
    {
        [SerializeField] private GameObject dropPrefab;
        [SerializeField] private int maxHealth = 2;

        private Health health;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            health = GetComponent<Health>();
            if (health == null) health = gameObject.AddComponent<Health>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            health.Initialize(maxHealth, Team.Neutral);
            health.Died += Break;
            health.Damaged += OnDamaged;
        }

        private void OnDamaged(DamageInfo info)
        {
            AudioManager.Play("sfx_boss_hit", 0.5f);
        }

        private void Break()
        {
            AudioManager.Play("sfx_explosion", 0.4f);

            if (dropPrefab != null)
                Instantiate(dropPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);

            gameObject.SetActive(false);
        }
    }
}
