using System.Collections;
using SteelRain.Audio;
using UnityEngine;

namespace SteelRain.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerDodge : MonoBehaviour
    {
        [SerializeField] private PlayerController2D controller;

        private Rigidbody2D body;
        private float nextAllowedTime;
        private bool dodging;

        public bool IsDodging => dodging;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
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

            var direction = controller.AimDirection.x < 0f ? -1f : 1f;
            body.linearVelocity = new Vector2(direction * controller.Character.dodgeSpeed, 0f);
            AudioManager.Play("sfx_dodge", 0.5f);

            yield return new WaitForSeconds(controller.Character.dodgeDuration);
            dodging = false;
            if (controller != null) controller.SetDodgeLock(false);
        }
    }
}
