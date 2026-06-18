using System.Collections;
using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class CrumblingPlatform : MonoBehaviour
    {
        [SerializeField] private float crumbleDelay = 0.5f;
        [SerializeField] private float respawnDelay = 3f;

        private SpriteRenderer spriteRenderer;
        private Collider2D col;
        private bool crumbling;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            col = GetComponent<Collider2D>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!crumbling && collision.collider.CompareTag("Player"))
                StartCoroutine(CrumbleRoutine());
        }

        private IEnumerator CrumbleRoutine()
        {
            crumbling = true;
            yield return new WaitForSeconds(crumbleDelay);

            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (col != null) col.enabled = false;

            yield return new WaitForSeconds(respawnDelay);

            if (spriteRenderer != null) spriteRenderer.enabled = true;
            if (col != null) col.enabled = true;
            crumbling = false;
        }
    }
}
