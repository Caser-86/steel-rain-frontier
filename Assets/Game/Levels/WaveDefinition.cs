using SteelRain.Enemies;
using UnityEngine;

namespace SteelRain.Levels
{
    [CreateAssetMenu(menuName = "Steel Rain/Wave Definition")]
    public sealed class WaveDefinition : ScriptableObject
    {
        public EnemyController[] enemyPrefabs;
        public Vector2[] spawnOffsets;
    }
}
