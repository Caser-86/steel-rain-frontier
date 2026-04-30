using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Levels
{
    [RequireComponent(typeof(Health))]
    public sealed class DestructibleTarget : MonoBehaviour
    {
        [SerializeField] private GameObject dropPrefab;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Died += Break;
        }

        private void OnDestroy()
        {
            if (health != null)
                health.Died -= Break;
        }

        private void Break()
        {
            if (dropPrefab != null)
                Instantiate(dropPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);

            gameObject.SetActive(false);
        }
    }
}
