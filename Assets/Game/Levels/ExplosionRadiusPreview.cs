using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class ExplosionRadiusPreview : MonoBehaviour
    {
        [SerializeField] private float radius = 2.5f;
        [SerializeField] private Color color = new(1f, 0.2f, 0.05f, 0.18f);

        private Renderer targetRenderer;

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.material = new Material(Shader.Find("Sprites/Default"));
                targetRenderer.material.color = color;
            }

            transform.localScale = Vector3.one * DiameterFromRadius(radius);
        }

        public static float DiameterFromRadius(float radius)
        {
            return radius * 2f;
        }
    }
}
