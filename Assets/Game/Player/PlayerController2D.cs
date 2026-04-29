using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public sealed class PlayerController2D : MonoBehaviour
    {
        [SerializeField] private CharacterDefinition character;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundRadius = 0.12f;

        private Rigidbody2D body;
        private Health health;
        private Vector2 moveInput;
        private bool jumpQueued;
        private bool crouching;

        public bool IsGrounded { get; private set; }
        public Vector2 AimDirection { get; private set; } = Vector2.right;
        public CharacterDefinition Character => character;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            health.Initialize(character.maxHealth, Team.Player);
            health.Changed += GameEvents.RaisePlayerHealthChanged;
            health.Died += GameEvents.RaisePlayerDied;
        }

        private void OnDestroy()
        {
            if (health == null)
                return;

            health.Changed -= GameEvents.RaisePlayerHealthChanged;
            health.Died -= GameEvents.RaisePlayerDied;
        }

        private void Update()
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            crouching = Input.GetAxisRaw("Vertical") < -0.5f;

            if (Input.GetButtonDown("Jump"))
                jumpQueued = true;

            var aim = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (aim.sqrMagnitude > 0.1f)
                AimDirection = aim.normalized;
        }

        private void FixedUpdate()
        {
            IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

            var velocity = body.linearVelocity;
            velocity.x = moveInput.x * character.moveSpeed;

            if (jumpQueued && IsGrounded && !crouching)
                velocity.y = character.jumpVelocity;

            body.linearVelocity = velocity;
            jumpQueued = false;
        }
    }
}
