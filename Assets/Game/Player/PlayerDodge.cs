using System.Collections;
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
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K))
                TryDodge();
        }

        public void TryDodge()
        {
            if (dodging || Time.time < nextAllowedTime)
                return;

            StartCoroutine(DodgeRoutine());
        }

        private IEnumerator DodgeRoutine()
        {
            dodging = true;
            nextAllowedTime = Time.time + controller.Character.dodgeCooldown;

            var direction = controller.AimDirection.x < 0f ? -1f : 1f;
            body.linearVelocity = new Vector2(direction * controller.Character.dodgeSpeed, 0f);

            yield return new WaitForSeconds(controller.Character.dodgeDuration);
            dodging = false;
        }
    }
}
