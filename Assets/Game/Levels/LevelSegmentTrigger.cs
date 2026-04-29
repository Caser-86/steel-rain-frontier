using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class LevelSegmentTrigger : MonoBehaviour
    {
        [SerializeField] private WaveDefinition wave;
        [SerializeField] private Transform player;
        [SerializeField] private bool triggerOnce = true;

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
            for (var i = 0; i < wave.enemyPrefabs.Length; i++)
            {
                var offset = i < wave.spawnOffsets.Length ? wave.spawnOffsets[i] : Vector2.zero;
                var enemy = Instantiate(wave.enemyPrefabs[i], (Vector2)transform.position + offset, Quaternion.identity);
                enemy.AssignTarget(player);
            }
        }
    }
}
