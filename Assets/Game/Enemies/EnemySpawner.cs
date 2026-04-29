using UnityEngine;

namespace SteelRain.Enemies
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private Transform player;
        [SerializeField] private Transform[] spawnPoints;

        public void SpawnAll()
        {
            foreach (var point in spawnPoints)
            {
                var enemy = Instantiate(enemyPrefab, point.position, Quaternion.identity);
                enemy.AssignTarget(player);
            }
        }
    }
}
