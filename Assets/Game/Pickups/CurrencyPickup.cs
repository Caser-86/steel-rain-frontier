using SteelRain.Audio;
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Pickups
{
    /// <summary>
    /// 军票拾取物：击杀敌人掉落，玩家拾取后增加军票余额。
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class CurrencyPickup : MonoBehaviour
    {
        [SerializeField] private int amount = 10;
        [SerializeField] private float lifetime = 12f;

        private float despawnAt;

        private void Awake()
        {
            despawnAt = Time.time + lifetime;
        }

        private void Update()
        {
            if (Time.time >= despawnAt)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            CurrencyManager.Add(amount);
            AudioManager.Play("sfx_pickup", 0.6f);
            Destroy(gameObject);
        }

        /// <summary>
        /// 设置掉落金额（由敌人死亡时调用）。
        /// </summary>
        public void SetAmount(int value)
        {
            amount = Mathf.Max(1, value);
        }
    }
}
