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
        public bool IsCrouching => crouching;
        public Vector2 AimDirection { get; private set; } = Vector2.right;
        public CharacterDefinition Character => character;
        public int CurrentHealth => health != null ? health.Current : 0;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = character.gravityScale;
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

        public void AssignCharacter(CharacterDefinition definition, int currentHealth)
        {
            character = definition;
            if (body != null)
                body.gravityScale = character.gravityScale;

            if (health != null)
                health.Initialize(character.maxHealth, Team.Player, currentHealth);
        }

        private void Update()
        {
            moveInput.x = ReadHorizontal();
            crouching = Input.GetAxisRaw("Vertical") < -0.5f || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
                jumpQueued = true;

            var aim = new Vector2(ReadHorizontal(), ReadVertical());
            if (aim.sqrMagnitude > 0.1f)
                AimDirection = aim.normalized;
        }

        private static float ReadHorizontal()
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                return -1f;

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                return 1f;

            return Input.GetAxisRaw("Horizontal");
        }

        private static float ReadVertical()
        {
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                return -1f;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                return 1f;

            return Input.GetAxisRaw("Vertical");
        }

        private void FixedUpdate()
        {
            IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

            var velocity = body.linearVelocity;
            var speed = crouching ? character.moveSpeed * character.crouchSpeedMultiplier : character.moveSpeed;
            velocity.x = moveInput.x * speed;

            if (jumpQueued && IsGrounded && !crouching)
                velocity.y = character.jumpVelocity;

            if (velocity.y < 0f)
                velocity.y += Physics2D.gravity.y * (character.fallGravityMultiplier - 1f) * Time.fixedDeltaTime;

            body.linearVelocity = velocity;
            jumpQueued = false;
        }
    }
}
