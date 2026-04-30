using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class CheckpointManager : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Vector3 fallbackSpawn;

        private Vector3 currentSpawn;
        public Vector3 CurrentSpawn => currentSpawn;

        private void Awake()
        {
            currentSpawn = fallbackSpawn;
            GameEvents.PlayerDied += RespawnPlayer;
        }

        private void OnDestroy()
        {
            GameEvents.PlayerDied -= RespawnPlayer;
        }

        public void SetCheckpoint(Vector3 position)
        {
            currentSpawn = position;
        }

        public void RestoreCheckpoint(Vector3 position)
        {
            currentSpawn = position;
            if (player != null)
                player.position = currentSpawn;
        }

        private void RespawnPlayer()
        {
            player.position = currentSpawn;
        }
    }
}
