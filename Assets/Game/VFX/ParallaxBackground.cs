using UnityEngine;

namespace SteelRain.VFX
{
    /// <summary>
    /// 多层视差背景，营造横版纵深感。
    /// </summary>
    public sealed class ParallaxBackground : MonoBehaviour
    {
        [System.Serializable]
        public class Layer
        {
            public Transform transform;
            public float parallaxFactor = 0.3f;
            public float horizontalExtent = 50f;
        }

        [SerializeField] private Layer[] layers;
        [SerializeField] private Transform camera;

        private Vector3 lastCameraPos;

        private void Start()
        {
            if (camera == null && Camera.main != null)
                camera = Camera.main.transform;
            if (camera != null)
                lastCameraPos = camera.position;
        }

        private void LateUpdate()
        {
            if (camera == null || layers == null) return;

            var delta = camera.position - lastCameraPos;

            foreach (var layer in layers)
            {
                if (layer.transform == null) continue;
                layer.transform.position += new Vector3(
                    delta.x * layer.parallaxFactor,
                    delta.y * layer.parallaxFactor * 0.5f,
                    0f
                );

                // 无限循环
                var relativeX = layer.transform.position.x - camera.position.x;
                if (Mathf.Abs(relativeX) > layer.horizontalExtent)
                {
                    var offset = layer.horizontalExtent * 2f * Mathf.Sign(relativeX);
                    layer.transform.position -= new Vector3(offset, 0f, 0f);
                }
            }

            lastCameraPos = camera.position;
        }
    }
}
