using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class CheckpointManager : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Vector3 fallbackSpawn;

        private Vector3 currentSpawn;

        private void Awake()
        {
            currentSpawn = SaveSystem.LoadCheckpoint(fallbackSpawn);
            GameEvents.PlayerDied += RespawnPlayer;
        }

        private void OnDestroy()
        {
            GameEvents.PlayerDied -= RespawnPlayer;
        }

        public void SetCheckpoint(Vector3 position)
        {
            currentSpawn = position;
            SaveSystem.SaveCheckpoint(position);
        }

        private void RespawnPlayer()
        {
            if (player == null) return;

            var health = player.GetComponent<Health>();

            // 如果 PlayerSquad 已经处理了复活（切换角色+Revive），不再移动到检查点
            if (health != null && !health.IsDead)
                return;

            // 全员死亡：移动到检查点并复活
            player.position = currentSpawn;
            if (health != null)
                health.ReviveFull();
        }
    }
}
