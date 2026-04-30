using UnityEngine;
using SteelRain.VFX;

namespace SteelRain.Levels
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(4f, 1.5f, -10f);
        [SerializeField] private float smoothTime = 0.18f;

        private Vector3 velocity;
        private CameraShake shake;

        private void Awake()
        {
            shake = GetComponent<CameraShake>();
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            var shakeOffset = shake != null ? shake.Offset : Vector3.zero;
            var desired = target.position + offset + shakeOffset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }
    }
}
