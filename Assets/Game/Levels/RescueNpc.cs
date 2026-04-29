using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class RescueNpc : MonoBehaviour
    {
        [SerializeField] private GameObject rewardPrefab;
        [SerializeField] private Transform rewardPoint;

        private bool rescued;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (rescued || !other.CompareTag("Player"))
                return;

            rescued = true;
            if (rewardPrefab != null)
                Instantiate(rewardPrefab, rewardPoint.position, Quaternion.identity);

            gameObject.SetActive(false);
        }
    }
}
