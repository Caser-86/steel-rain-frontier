using UnityEngine;

namespace SteelRain.Core
{
    /// <summary>
    /// 摄像机边界限制器：附加到摄像机上，限制视野不超出关卡边界。
    /// 配合 SimpleCameraFollow 使用。
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraBounds : MonoBehaviour
    {
        [Header("关卡边界（世界坐标）")]
        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 220f;
        [SerializeField] private float minY = -2f;
        [SerializeField] private float maxY = 10f;

        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (cam == null) return;

            var pos = transform.position;
            var halfH = cam.orthographicSize;
            var halfW = halfH * cam.aspect;

            // 限制摄像机位置使视野不超出边界
            pos.x = Mathf.Clamp(pos.x, minX + halfW, maxX - halfW);
            pos.y = Mathf.Clamp(pos.y, minY + halfH, maxY - halfH);
            pos.z = -10f;

            transform.position = pos;
        }

        /// <summary>
        /// 编辑器中绘制边界范围。
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            var center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
            var size = new Vector3(maxX - minX, maxY - minY, 0.1f);
            Gizmos.DrawWireCube(center, size);
        }

        /// <summary>
        /// 动态设置边界（用于关卡生成器）。
        /// </summary>
        public void SetBounds(float minX, float maxX, float minY, float maxY)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }
    }
}
