using UnityEngine;

namespace SteelRain.Pickups
{
    public sealed class PickupMotion : MonoBehaviour
    {
        [SerializeField] private float bobHeight = 0.12f;
        [SerializeField] private float bobSpeed = 3.2f;
        [SerializeField] private float pulseScale = 0.08f;

        private Vector3 origin;
        private Vector3 baseScale;
        private float phase;

        private void OnEnable()
        {
            origin = transform.localPosition;
            baseScale = transform.localScale;
            phase = Random.value * Mathf.PI * 2f;
        }

        private void Update()
        {
            var wave = Mathf.Sin(Time.time * bobSpeed + phase);
            transform.localPosition = origin + Vector3.up * (wave * bobHeight);
            transform.localScale = baseScale * (1f + (0.5f + wave * 0.5f) * pulseScale);
        }
    }
}
