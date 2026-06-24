using System.Collections;
using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Player;
using UnityEngine;

namespace SteelRain.Enemies
{
    /// <summary>
    /// 区域触发式敌人波次生成器。
    /// 玩家进入触发区域后开始分波生成敌人，清除所有波次后可选触发事件。
    /// 支持指定敌人类型、每波数量、波次间隔、精英/稀有敌人。
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class EnemySpawner : MonoBehaviour
    {
        [System.Serializable]
        public struct Wave
        {
            public int enemyCount;
            [Tooltip("0=Rifle, 1=Shield, 2=Grenadier, 3=Drone, 4=Sniper, 5=Heavy")]
            public int[] enemyIndices;
            public float spawnDelayBetweenEnemies;
            [Tooltip("是否包含精英敌人（更强数值）")]
            public bool hasElite;
            [Tooltip("精英敌人类型索引")]
            public int eliteIndex;
        }

        [Header("Spawn Config")]
        [SerializeField] private Wave[] waves;
        [SerializeField] private float waveDelay = 2f;
        [SerializeField] private float spawnRadius = 2f;
        [SerializeField] private float spawnMinY = 0f;
        [SerializeField] private float spawnMaxY = 3f;

        [Header("References")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private EnemyDefinition[] enemyDefs;
        [SerializeField] private Transform target;

        [Header("Completion")]
        [Tooltip("所有波次清除后激活的物体（如门、桥、剧情触发器）")]
        [SerializeField] private GameObject[] activateOnComplete;
        [Tooltip("所有波次清除后禁用的物体（如路障）")]
        [SerializeField] private GameObject[] deactivateOnComplete;

        private int currentWaveIndex;
        private int enemiesAliveInWave;
        private int totalEnemiesSpawned;
        private bool triggered;
        private bool completed;

        private void Awake()
        {
            var box = GetComponent<BoxCollider2D>();
            box.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered || completed) return;
            if (!other.CompareTag("Player")) return;
            triggered = true;

            // 自动寻找目标
            if (target == null)
                target = other.transform;

            StartCoroutine(RunWaves());
        }

        private IEnumerator RunWaves()
        {
            for (currentWaveIndex = 0; currentWaveIndex < waves.Length; currentWaveIndex++)
            {
                var wave = waves[currentWaveIndex];
                if (wave.enemyIndices == null || wave.enemyIndices.Length == 0) continue;

                enemiesAliveInWave = 0;

                // 生成普通敌人
                for (int i = 0; i < wave.enemyIndices.Length; i++)
                {
                    SpawnEnemy(wave.enemyIndices[i], false);
                    var delay = wave.spawnDelayBetweenEnemies > 0 ? wave.spawnDelayBetweenEnemies : 0.5f;
                    yield return new WaitForSeconds(delay);
                }

                // 生成精英
                if (wave.hasElite && wave.eliteIndex >= 0 && wave.eliteIndex < enemyPrefabs.Length)
                {
                    SpawnEnemy(wave.eliteIndex, true);
                }

                // 等待本波敌人全部阵亡
                while (enemiesAliveInWave > 0)
                    yield return null;

                // 波次间隔
                if (currentWaveIndex < waves.Length - 1 && waveDelay > 0)
                    yield return new WaitForSeconds(waveDelay);
            }

            // 全部波次完成
            completed = true;

            if (activateOnComplete != null)
                foreach (var go in activateOnComplete)
                    if (go != null) go.SetActive(true);

            if (deactivateOnComplete != null)
                foreach (var go in deactivateOnComplete)
                    if (go != null) go.SetActive(false);

            AudioManager.Play("sfx_checkpoint", 0.8f);
        }

        private void SpawnEnemy(int prefabIndex, bool isElite)
        {
            if (enemyPrefabs == null || prefabIndex >= enemyPrefabs.Length) return;
            var prefab = enemyPrefabs[prefabIndex];
            if (prefab == null) return;

            // 生成位置：在生成器前方随机偏移
            var offsetX = Random.Range(-spawnRadius, spawnRadius);
            var offsetY = Random.Range(spawnMinY, spawnMaxY);
            var pos = transform.position + new Vector3(offsetX + spawnRadius, offsetY, 0);

            var go = Instantiate(prefab, pos, Quaternion.identity);
            var health = go.GetComponent<Health>();
            if (health != null)
            {
                enemiesAliveInWave++;
                totalEnemiesSpawned++;
                health.Died += OnEnemyDied;
            }

            // 初始化敌人定义
            var controller = go.GetComponent<EnemyController>();
            if (controller != null)
            {
                var defIndex = Mathf.Min(prefabIndex, enemyDefs != null ? enemyDefs.Length - 1 : 0);
                if (enemyDefs != null && defIndex >= 0 && defIndex < enemyDefs.Length && enemyDefs[defIndex] != null)
                    controller.Initialize(enemyDefs[defIndex], target);
            }

            // 精英敌人视觉标识和数值增强
            if (isElite)
            {
                var sr = go.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color(1f, 0.3f, 0.3f);
                    go.transform.localScale *= 1.3f;
                }
                // 精英双倍血量
                if (health != null)
                {
                    var extra = health.Max;
                    health.Initialize(extra * 2, health.Team);
                }
            }
        }

        private void OnEnemyDied()
        {
            enemiesAliveInWave = Mathf.Max(0, enemiesAliveInWave - 1);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            var center = transform.position + new Vector3(spawnRadius, (spawnMinY + spawnMaxY) * 0.5f, 0);
            var size = new Vector3(spawnRadius * 2, spawnMaxY - spawnMinY, 0.1f);
            Gizmos.DrawCube(center, size);
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
