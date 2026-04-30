using System.Collections;
using SteelRain.Core;
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
        private bool missionEnded;

        public bool IsDodging => dodging;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            GameEvents.LevelCompleted += EndMissionDodge;
            GameEvents.SquadDefeated += EndMissionDodge;
        }

        private void OnDisable()
        {
            GameEvents.LevelCompleted -= EndMissionDodge;
            GameEvents.SquadDefeated -= EndMissionDodge;
        }

        private void Update()
        {
            if (missionEnded)
                return;

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K))
                TryDodge();
        }

        public void TryDodge()
        {
            if (missionEnded || dodging || Time.time < nextAllowedTime)
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

        private void EndMissionDodge()
        {
            missionEnded = true;
            dodging = false;
        }
    }
}
