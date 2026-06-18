using UnityEngine;

namespace SteelRain.VFX
{
    /// <summary>
    /// 简单 2D 摄像机跟随，横向卷轴用。
    /// </summary>
    public sealed class SimpleCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private Vector2 offset = new Vector2(3f, 1.5f);
        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 145f;

        private Vector3 velocity;

        private void LateUpdate()
        {
            if (target == null) return;

            var targetPos = new Vector3(
                Mathf.Clamp(target.position.x + offset.x, minX, maxX),
                target.position.y + offset.y,
                transform.position.z
            );

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        }
    }
}
