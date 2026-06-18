using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Enemies;
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
            AudioManager.Play("sfx_checkpoint", 0.8f);
            GameEvents.RaiseCheckpointReached();
        }
    }
}
