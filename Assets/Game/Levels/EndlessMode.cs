using System.Collections;
using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Enemies;
using SteelRain.Player;
using UnityEngine;

namespace SteelRain.Levels
{
    /// <summary>
    /// 无尽模式：波次制生存，每5波一个Boss，难度递增。
    /// 提供 endgame 内容和排行榜驱动的长线留存。
    /// </summary>
    public sealed class EndlessMode : MonoBehaviour
    {
        [Header("Wave Config")]
        [SerializeField] private float waveInterval = 5f;
        [SerializeField] private float enemySpawnRadius = 15f;
        [SerializeField] private float enemySpawnMinY = -1f;
        [SerializeField] private float enemySpawnMaxY = 4f;

        [Header("Prefabs")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private GameObject bossPrefab;

        [Header("Definitions")]
        [SerializeField] private EnemyDefinition[] enemyDefs;

        [Header("Pickups")]
        [SerializeField] private GameObject healthPickupPrefab;
        [SerializeField] private GameObject weaponPickupPrefab;
        [SerializeField] [Range(0f, 1f)] private float healthDropChance = 0.12f;
        [SerializeField] [Range(0f, 1f)] private float weaponDropChance = 0.06f;

        private int currentWave = 0;
        private int enemiesAlive = 0;
        private Transform playerTransform;
        private static EndlessMode instance;

        public int CurrentWave => currentWave;
        public int EnemiesAlive => enemiesAlive;
        public static int CurrentWaveStatic => instance != null ? instance.currentWave : 0;

        private void Start()
        {
            instance = this;
            // 每次进入无尽模式重置波次（避免静态字段残留旧值）
            currentWave = 0;
            enemiesAlive = 0;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;

            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
                Debug.LogError("[EndlessMode] No enemy prefabs assigned!");

            StartCoroutine(StartWaveSequence());
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private IEnumerator StartWaveSequence()
        {
            yield return new WaitForSecondsRealtime(3f);

            while (true)
            {
                // 玩家死亡时停止整个协程（避免 timeScale=0 时 yield return null 死锁）
                if (IsPlayerDead())
                {
                    // 保存最高波次记录
                    if (currentWave > 0)
                        SaveSystem.UpdateEndlessBestWave(currentWave);
                    yield break;
                }

                currentWave++;
                GameEvents.RaiseEndlessWaveChanged(currentWave);

                // 波次开始音效
                if (currentWave % 5 == 0)
                    AudioManager.Play("sfx_boss_hit", 0.8f);
                else
                    AudioManager.Play("sfx_checkpoint", 0.5f);

                // 无尽模式成就
                if (currentWave == 10)
                    AchievementManager.Unlock(AchievementManager.AchievementId.EndlessWave10);
                else if (currentWave == 20)
                    AchievementManager.Unlock(AchievementManager.AchievementId.EndlessWave20);

                if (currentWave % 5 == 0)
                    yield return StartCoroutine(SpawnBossWave());
                else
                    yield return StartCoroutine(SpawnRegularWave());

                // 等待所有敌人死亡或玩家死亡
                while (enemiesAlive > 0 && !IsPlayerDead())
                    yield return null;

                // 玩家死亡时停止协程
                if (IsPlayerDead())
                {
                    if (currentWave > 0)
                        SaveSystem.UpdateEndlessBestWave(currentWave);
                    yield break;
                }

                // 波次清除奖励：分数 + 波次 * 100
                ScoreManager.AddScore(currentWave * 100);
                yield return new WaitForSeconds(waveInterval);
            }
        }

        private IEnumerator SpawnRegularWave()
        {
            var baseCount = Mathf.Clamp(currentWave / 2 + 2, 2, 12);
            var hasElite = currentWave >= 10 && currentWave % 3 == 0;

            for (int i = 0; i < baseCount; i++)
            {
                SpawnRandomEnemy(false);
                yield return new WaitForSeconds(0.3f);
            }

            if (hasElite)
                SpawnRandomEnemy(true);
        }

        private IEnumerator SpawnBossWave()
        {
            SpawnBoss();
            for (int i = 0; i < Mathf.Clamp(currentWave / 5, 1, 4); i++)
            {
                SpawnRandomEnemy(false);
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void SpawnRandomEnemy(bool isElite)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

            var pos = GetRandomSpawnPosition();
            // 精英敌人从索引 1 开始选（如果有多个），否则用 0
            int prefabIndex;
            if (isElite && enemyPrefabs.Length > 1)
                prefabIndex = Random.Range(1, enemyPrefabs.Length);
            else
                prefabIndex = Random.Range(0, enemyPrefabs.Length);

            var prefab = enemyPrefabs[prefabIndex];
            if (prefab == null) return;

            var go = Instantiate(prefab, pos, Quaternion.identity);
            var health = go.GetComponent<Health>();
            if (health != null)
            {
                enemiesAlive++;
                health.Died += OnEnemyDied;
            }

            var controller = go.GetComponent<EnemyController>();
            if (controller != null && enemyDefs != null && prefabIndex < enemyDefs.Length && enemyDefs[prefabIndex] != null)
                controller.Initialize(enemyDefs[prefabIndex], playerTransform);

            // 精英敌人血量翻倍（在 Initialize 之后设置，避免被覆盖）
            if (isElite && health != null)
                health.InitializeWithCurrent(health.Max * 2, health.Team, health.Max * 2);
        }

        private void SpawnBoss()
        {
            if (bossPrefab == null) return;
            var pos = GetRandomSpawnPosition();
            var go = Instantiate(bossPrefab, pos, Quaternion.identity);
            var health = go.GetComponent<Health>();
            if (health != null)
            {
                enemiesAlive++;
                health.Died += OnEnemyDied;
            }
            var boss = go.GetComponent<MiniBossWalker>();
            if (boss != null)
                boss.AssignTarget(playerTransform);
        }

        private Vector3 GetRandomSpawnPosition()
        {
            if (playerTransform == null) return new Vector3(10f, 1f, 0f);
            var dir = Random.value > 0.5f ? 1f : -1f;
            var x = playerTransform.position.x + dir * enemySpawnRadius;
            var y = Random.Range(enemySpawnMinY, enemySpawnMaxY);
            return new Vector3(x, y, 0f);
        }

        private void OnEnemyDied()
        {
            enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
            TrySpawnPickup();
        }

        private void TrySpawnPickup()
        {
            if (healthPickupPrefab != null && Random.value < healthDropChance)
            {
                var pos = GetRandomSpawnPosition();
                Instantiate(healthPickupPrefab, pos, Quaternion.identity);
            }
            if (weaponPickupPrefab != null && Random.value < weaponDropChance)
            {
                var pos = GetRandomSpawnPosition();
                Instantiate(weaponPickupPrefab, pos, Quaternion.identity);
            }
        }

        private bool IsPlayerDead()
        {
            if (playerTransform == null) return true;
            var squad = playerTransform.GetComponent<PlayerSquad>();
            if (squad != null) return squad.AliveCount == 0;
            var health = playerTransform.GetComponent<Health>();
            return health == null || health.Current <= 0;
        }
    }
}
