using System.Collections;
using UnityEngine;

namespace SteelRain.VFX
{
    public sealed class ImpactBurst : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock block;
        private Renderer targetRenderer;
        private Color color;
        private float duration;
        private Vector3 startScale;
        private Vector3 endScale;

        public static void Spawn(Vector2 position, Color color, float startSize, float endSize, float duration)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "ImpactBurst";
            go.transform.position = new Vector3(position.x, position.y, -0.5f);

            var collider = go.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            var burst = go.AddComponent<ImpactBurst>();
            burst.Initialize(color, startSize, endSize, duration);
        }

        private void Initialize(Color burstColor, float startSize, float endSize, float burstDuration)
        {
            targetRenderer = GetComponent<Renderer>();
            targetRenderer.material = new Material(Shader.Find("Sprites/Default"));
            block = new MaterialPropertyBlock();
            color = burstColor;
            duration = Mathf.Max(0.01f, burstDuration);
            startScale = Vector3.one * startSize;
            endScale = Vector3.one * endSize;
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                var t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
                var faded = color;
                faded.a *= 1f - t;
                block.SetColor(ColorId, faded);
                targetRenderer.SetPropertyBlock(block);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
