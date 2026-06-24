using UnityEngine;

namespace SteelRain.Pickups
{
    /// <summary>
    /// 敌人掉落配置（合金弹头简化版）。
    /// 击杀后按概率掉落血量/护盾/无敌/武器拾取物。无军票。
    /// </summary>
    public sealed class LootDrop : MonoBehaviour
    {
        [Header("Health Drop")]
        [SerializeField] private GameObject healthPrefab;
        [SerializeField] [Range(0f, 1f)] private float healthDropChance = 0.15f;

        [Header("Shield Drop")]
        [SerializeField] private GameObject shieldPrefab;
        [SerializeField] [Range(0f, 1f)] private float shieldDropChance = 0.05f;

        [Header("Invincible Drop")]
        [SerializeField] private GameObject invinciblePrefab;
        [SerializeField] [Range(0f, 1f)] private float invincibleDropChance = 0.03f;

        [Header("Weapon Drop")]
        [SerializeField] private GameObject weaponPrefab;
        [SerializeField] [Range(0f, 1f)] private float weaponDropChance = 0.08f;

        [Header("Drop Physics")]
        [SerializeField] private float popForce = 3f;
        [SerializeField] private float popSpreadAngle = 60f;

        public void SpawnLoot(Vector3 position)
        {
            if (healthPrefab != null && Random.value < healthDropChance)
                SpawnPickup(healthPrefab, position);

            if (shieldPrefab != null && Random.value < shieldDropChance)
                SpawnPickup(shieldPrefab, position);

            if (invinciblePrefab != null && Random.value < invincibleDropChance)
                SpawnPickup(invinciblePrefab, position);

            if (weaponPrefab != null && Random.value < weaponDropChance)
                SpawnPickup(weaponPrefab, position);
        }

        private void SpawnPickup(GameObject prefab, Vector3 position)
        {
            var offset = new Vector3(Random.Range(-0.3f, 0.3f), 0.2f, 0f);
            var pickup = Instantiate(prefab, position + offset, Quaternion.identity);

            var rb = pickup.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                var angle = Random.Range(-popSpreadAngle, popSpreadAngle) * Mathf.Deg2Rad;
                var force = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) + 1f).normalized * popForce;
                rb.AddForce(force, ForceMode2D.Impulse);
            }
        }

        public void SetEliteDropRates()
        {
            healthDropChance = 0.4f;
            shieldDropChance = 0.15f;
            invincibleDropChance = 0.08f;
            weaponDropChance = 0.2f;
        }

        public void SetBossDropRates()
        {
            healthDropChance = 1f;
            shieldDropChance = 0.5f;
            invincibleDropChance = 0.3f;
            weaponDropChance = 0.5f;
        }
    }
}
