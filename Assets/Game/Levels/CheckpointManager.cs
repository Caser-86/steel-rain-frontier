using SteelRain.Core;
using SteelRain.Pickups;
using SteelRain.UI;
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
            SaveSystem.SaveLevelIndex(LevelManager.CurrentLevel);
        }

        private void RespawnPlayer()
        {
            if (player == null) return;

            // 如果GameOverScreen已显示，不复活（由GameOverScreen处理重试）
            var gameOver = FindFirstObjectByType<GameOverScreen>();
            if (gameOver != null && gameOver.IsShown)
                return;

            var health = player.GetComponent<Health>();

            // 如果 PlayerSquad 已经处理了复活（切换角色+Revive），不再移动到检查点
            if (health != null && !health.IsDead)
                return;

            // 全员死亡：移动到检查点并复活
            player.position = currentSpawn;
            if (health != null)
                health.ReviveFull();

            // 重置本关卡内所有已消耗的武器拾取物，避免死亡螺旋
            WeaponPickup.ResetAllOnRespawn();
        }
    }
}
