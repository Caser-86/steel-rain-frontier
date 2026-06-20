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
        [SerializeField] private float maxX = 470f;

        private Vector3 velocity;
        private CameraShake cameraShake;
        private Vector3 lastShakeOffset;

        private void Awake()
        {
            cameraShake = GetComponent<CameraShake>();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // 先减去上一帧的抖动偏移，得到"纯净"的相机位置
            transform.position -= lastShakeOffset;

            var targetPos = new Vector3(
                Mathf.Clamp(target.position.x + offset.x, minX, maxX),
                target.position.y + offset.y,
                transform.position.z
            );

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

            // 应用当前帧的摄像机抖动偏移
            if (cameraShake != null)
            {
                lastShakeOffset = cameraShake.ShakeOffset;
                transform.position += lastShakeOffset;
            }
            else
            {
                lastShakeOffset = Vector3.zero;
            }
        }
    }
}
