using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Enemies;
using SteelRain.Player;
using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class Checkpoint : MonoBehaviour
    {
        [SerializeField] private CheckpointManager manager;

        private bool used;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (used || !other.CompareTag("Player"))
                return;

            used = true;
            if (manager != null)
                manager.SetCheckpoint(transform.position);
            // 在检查点保存小队状态，支持崩溃/断线恢复
            var squad = other.GetComponent<PlayerSquad>();
            if (squad != null) squad.SaveSquadState();
            AudioManager.Play("sfx_checkpoint", 0.8f);
            GameEvents.RaiseCheckpointReached();
        }
    }
}
