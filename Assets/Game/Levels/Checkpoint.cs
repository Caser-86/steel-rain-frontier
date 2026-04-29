using SteelRain.Core;
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
            manager.SetCheckpoint(transform.position);
            GameEvents.RaiseCheckpointReached();
        }
    }
}
