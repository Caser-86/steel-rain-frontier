using System.Collections;
using SteelRain.Audio;
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerDodge : MonoBehaviour
    {
        [SerializeField] private PlayerController2D controller;
        [SerializeField] private Health health;

        private Rigidbody2D body;
        private float nextAllowedTime;
        private bool dodging;

        public bool IsDodging => dodging;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            if (health == null)
                health = GetComponent<Health>();
        }

        private void Update()
        {
            // 暂停时不响应闪避输入，避免暂停期间触发闪避导致状态异常
            if (Time.timeScale == 0f) return;
            if (Input.GetKeyDown(KeyCode.LeftShift))
                TryDodge();
        }

        public void TryDodge()
        {
            if (dodging || Time.time < nextAllowedTime)
                return;

            if (controller == null || controller.Character == null)
                return;

            StartCoroutine(DodgeRoutine());
        }

        private IEnumerator DodgeRoutine()
        {
            dodging = true;
            nextAllowedTime = Time.time + controller.Character.dodgeCooldown;
            if (controller != null) controller.SetDodgeLock(true);

            // 闪避期间无敌
            if (health != null) health.SetInvincible(true);

            var direction = controller.AimDirection.x < 0f ? -1f : 1f;
            body.linearVelocity = new Vector2(direction * controller.Character.dodgeSpeed, 0f);
            AudioManager.Play("sfx_dodge", 0.5f);

            // 使用 WaitForSeconds（受 timeScale 影响）替代 WaitForSecondsRealtime，
            // 这样暂停时无敌计时也会暂停，避免恢复游戏后无敌帧已消失
            yield return new WaitForSeconds(controller.Character.dodgeDuration);
            dodging = false;
            if (controller != null) controller.SetDodgeLock(false);
            if (health != null) health.SetInvincible(false);
        }
    }
}
