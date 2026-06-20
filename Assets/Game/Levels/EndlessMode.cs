using System.Collections;
using SteelRain.Core;
using SteelRain.Enemies;
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

        private int currentWave = 0;
        private int enemiesAlive = 0;

        public int CurrentWave => currentWave;
        public int EnemiesAlive => enemiesAlive;

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;

            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
                Debug.LogError("[EndlessMode] No enemy prefabs assigned!");

            StartCoroutine(StartWaveSequence());
        }

        private Transform playerTransform;

        private IEnumerator StartWaveSequence()
        {
            yield return new WaitForSecondsRealtime(3f);

            while (true)
            {
                currentWave++;

                if (currentWave % 5 == 0)
                    yield return StartCoroutine(SpawnBossWave());
                else
                    yield return StartCoroutine(SpawnRegularWave());

                while (enemiesAlive > 0)
                    yield return null;

                Debug.Log($"[EndlessMode] Wave {currentWave} cleared!");
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
            Debug.Log($"[EndlessMode] BOSS WAVE {currentWave}!");
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
            var prefabIndex = isElite
                ? Random.Range(1, enemyPrefabs.Length)
                : Random.Range(0, enemyPrefabs.Length);

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
        }
    }
}
