using System.Collections;
using SteelRain.Core;
using SteelRain.VFX;
using UnityEngine;

namespace SteelRain.Enemies
{
    public sealed class EnemyStrikeMarker : MonoBehaviour
    {
        [SerializeField] private float radius = 1.2f;
        [SerializeField] private float delay = 0.65f;
        [SerializeField] private int damage = 2;

        private Renderer targetRenderer;

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.material = new Material(Shader.Find("Sprites/Default"));
                targetRenderer.material.color = new Color(1f, 0.05f, 0.02f, 0.55f);
            }

            transform.localScale = new Vector3(DiameterFromRadius(radius), 0.08f, 1f);
        }

        private void Start()
        {
            StartCoroutine(StrikeRoutine());
        }

        public void Configure(float newRadius, float newDelay, int newDamage)
        {
            radius = newRadius;
            delay = newDelay;
            damage = newDamage;
            transform.localScale = new Vector3(DiameterFromRadius(radius), 0.08f, 1f);
        }

        public static float DiameterFromRadius(float radius)
        {
            return radius * 2f;
        }

        private IEnumerator StrikeRoutine()
        {
            yield return new WaitForSeconds(delay);
            CameraShake.ShakeGlobal(0.12f, 0.12f);
            ImpactBurst.Spawn(transform.position, new Color(1f, 0.18f, 0.03f, 0.85f), 0.45f, radius * 2.4f, 0.22f);

            var hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent(out Health health) || health.Team == Team.Enemy)
                    continue;

                var direction = ((Vector2)health.transform.position - (Vector2)transform.position).normalized;
                health.ApplyDamage(new DamageInfo(damage, Team.Enemy, direction));
            }

            Destroy(gameObject);
        }
    }
}
