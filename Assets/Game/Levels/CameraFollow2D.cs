using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(6f, 2.5f, -10f);
        [SerializeField] private float smoothTime = 0.18f;

        private Vector3 velocity;

        private void LateUpdate()
        {
            if (target == null)
                return;

            var desired = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }
    }
}
