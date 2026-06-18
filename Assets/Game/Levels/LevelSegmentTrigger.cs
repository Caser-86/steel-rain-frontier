using SteelRain.Enemies;
using SteelRain.Weapons;
using UnityEngine;

namespace SteelRain.Levels
{
    /// <summary>
    /// 关卡段落触发器：玩家进入时按偏移生成敌人波次。
    /// 支持 EnemyController、ShieldSoldierAI、DroneAI、GrenadierAI 等不同敌人类型。
    /// </summary>
    public sealed class LevelSegmentTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Vector2[] spawnOffsets;
        [SerializeField] private Transform player;
        [SerializeField] private bool triggerOnce = true;
        [SerializeField] private int spawnCount = 3;
        [SerializeField] private Projectile enemyProjectilePrefab;

        private bool triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered && triggerOnce)
                return;

            if (!other.CompareTag("Player"))
                return;

            triggered = true;
            SpawnWave();
        }

        private void SpawnWave()
        {
            if (enemyPrefab == null) return;

            var defHolder = GetComponent<EnemyDefinitionHolder>();
            var count = Mathf.Max(1, spawnCount);

            for (var i = 0; i < count; i++)
            {
                var offset = i < spawnOffsets.Length ? spawnOffsets[i] : Vector2.zero;
                var enemy = Instantiate(enemyPrefab, (Vector2)transform.position + offset, Quaternion.identity);

                if (defHolder != null && defHolder.definition != null && player != null)
                    InitializeEnemy(enemy, defHolder.definition, player);
                else if (player != null)
                    AssignTarget(enemy, player);
            }
        }

        private void InitializeEnemy(GameObject enemy, EnemyDefinition def, Transform player)
        {
            if (enemy.TryGetComponent(out EnemyController ec))
            {
                ec.Initialize(def, player);
                return;
            }
            if (enemy.TryGetComponent(out ShieldSoldierAI ss))
            {
                ss.Initialize(def, player);
                return;
            }
            if (enemy.TryGetComponent(out DroneAI dr))
            {
                dr.Initialize(def, player, enemyProjectilePrefab);
                return;
            }
            if (enemy.TryGetComponent(out GrenadierAI gr))
            {
                gr.Initialize(def, player, enemyProjectilePrefab);
                return;
            }
            AssignTarget(enemy, player);
        }

        private void AssignTarget(GameObject enemy, Transform player)
        {
            if (enemy.TryGetComponent(out EnemyController ec)) ec.AssignTarget(player);
            else if (enemy.TryGetComponent(out ShieldSoldierAI ss)) ss.AssignTarget(player);
            else if (enemy.TryGetComponent(out DroneAI dr)) dr.AssignTarget(player);
            else if (enemy.TryGetComponent(out GrenadierAI gr)) gr.AssignTarget(player);
        }
    }
}
