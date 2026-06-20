using System.Collections;
using SteelRain.Audio;
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Pickups
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class InvinciblePickup : MonoBehaviour
    {
        [SerializeField] private float duration = 3f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Health health))
                return;
            if (health.Team != Team.Player)
                return;

            // 在 player 对象上启动协程，避免 pickup 被禁用后协程中断导致永久无敌
            var runner = other.GetComponent<PickupCoroutineRunner>();
            if (runner == null)
                runner = other.gameObject.AddComponent<PickupCoroutineRunner>();
            runner.Run(InvincibilityRoutine(health, runner));

            AudioManager.Play("sfx_upgrade", 0.7f);
            gameObject.SetActive(false);
        }

        private IEnumerator InvincibilityRoutine(Health health, MonoBehaviour runner)
        {
            // 使用invincible标志而非改变team，避免玩家子弹伤害自己
            health.SetInvincible(true);
            yield return new WaitForSeconds(duration);
            if (health != null && !health.IsDead)
                health.SetInvincible(false);
        }
    }

    /// <summary>
    /// 临时协程运行器，挂在 player 上以避免 pickup 禁用导致协程中断。
    /// </summary>
    public sealed class PickupCoroutineRunner : MonoBehaviour
    {
        public void Run(IEnumerator routine) => StartCoroutine(routine);
    }
}
