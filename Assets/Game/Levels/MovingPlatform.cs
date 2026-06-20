using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector2 pointA;
        [SerializeField] private Vector2 pointB;
        [SerializeField] private float speed = 2f;

        private Vector2 startPos;
        private float t;

        private void Start()
        {
            startPos = transform.position;
        }

        private void Update()
        {
            t += Time.deltaTime * speed;
            var cycle = (Mathf.Sin(t) + 1f) * 0.5f;
            transform.position = Vector2.Lerp(startPos + pointA, startPos + pointB, cycle);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.CompareTag("Player"))
                collision.transform.SetParent(transform, true);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                // 使用worldPositionStays=true保留玩家的全局缩放和位置
                collision.transform.SetParent(null, true);
            }
        }
    }
}
