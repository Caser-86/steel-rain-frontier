using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.VFX;
using UnityEngine;

namespace SteelRain.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class PlayerController2D : MonoBehaviour
    {
        [SerializeField] private CharacterDefinition character;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundRadius = 0.15f;
        [SerializeField] private float jumpBufferTime = 0.12f;
        [SerializeField] private float coyoteTime = 0.10f;
        [SerializeField] private float dustInterval = 0.15f;

        private Rigidbody2D body;
        private BoxCollider2D boxCollider;
        private Health health;
        private PlayerDodge dodge;
        private Vector2 moveInput;
        private bool crouching;
        private float jumpBufferCounter;
        private float coyoteCounter;
        private float dustTimer;
        private Vector2 originalColliderSize;
        private System.Action<DamageInfo> onDamagedHandler;

        public bool IsGrounded { get; private set; }
        public bool IsCrouching => crouching;
        public Vector2 AimDirection { get; private set; } = Vector2.right;
        public CharacterDefinition Character => character;
        public Health Health => health;
        public bool MovementLocked => dodgeLock || skillLock;
        private bool dodgeLock;
        private bool skillLock;

        public void SetDodgeLock(bool locked) => dodgeLock = locked;
        public void SetSkillLock(bool locked) => skillLock = locked;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            health = GetComponent<Health>();
            dodge = GetComponent<PlayerDodge>();

            originalColliderSize = boxCollider.size;

            if (character != null)
            {
                health.Initialize(character.maxHealth, Team.Player);
                body.gravityScale = character.gravityScale;
            }

            health.Changed += GameEvents.RaisePlayerHealthChanged;
            health.Died += GameEvents.RaisePlayerDied;
            onDamagedHandler = info =>
            {
                GameEvents.RaisePlayerDamaged(info.Direction);
                var shake = Camera.main?.GetComponent<CameraShake>();
                if (shake != null) shake.Shake(0.15f, 0.1f);
            };
            health.Damaged += onDamagedHandler;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Changed -= GameEvents.RaisePlayerHealthChanged;
                health.Died -= GameEvents.RaisePlayerDied;
                if (onDamagedHandler != null)
                    health.Damaged -= onDamagedHandler;
            }
        }

        private void Update()
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");

            crouching = moveInput.y < -0.5f && IsGrounded;

            if (Input.GetButtonDown("Jump"))
                jumpBufferCounter = jumpBufferTime;

            var aim = new Vector2(moveInput.x, moveInput.y);
            if (aim.sqrMagnitude > 0.1f)
                AimDirection = aim.normalized;

            if (jumpBufferCounter > 0)
                jumpBufferCounter -= Time.deltaTime;
            if (coyoteCounter > 0)
                coyoteCounter -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (character == null) return;

            IsGrounded = groundCheck != null &&
                Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

            if (IsGrounded)
                coyoteCounter = coyoteTime;

            var velocity = body.linearVelocity;

            var speed = crouching
                ? character.moveSpeed * character.crouchSpeedMultiplier
                : character.moveSpeed;

            if (!MovementLocked && (dodge == null || !dodge.IsDodging))
                velocity.x = moveInput.x * speed;

            if (originalColliderSize != Vector2.zero)
            {
                if (crouching)
                {
                    var crouchSize = originalColliderSize;
                    crouchSize.y *= character.crouchColliderHeightMultiplier;
                    boxCollider.size = crouchSize;
                }
                else
                {
                    boxCollider.size = originalColliderSize;
                }
            }

            if (jumpBufferCounter > 0 && coyoteCounter > 0 && !crouching)
            {
                velocity.y = character.jumpVelocity;
                jumpBufferCounter = 0;
                coyoteCounter = 0;
                AudioManager.Play("sfx_jump", 0.5f);
            }

            if (velocity.y < 0f)
                velocity.y += Physics2D.gravity.y * (character.fallGravityMultiplier - 1f) * Time.fixedDeltaTime;

            body.linearVelocity = velocity;

            // 脚步尘土
            if (IsGrounded && Mathf.Abs(velocity.x) > 1f)
            {
                dustTimer -= Time.fixedDeltaTime;
                if (dustTimer <= 0f)
                {
                    dustTimer = dustInterval;
                    if (groundCheck != null)
                        ParticleSpawner.SpawnDust(groundCheck.position);
                }
            }
        }

        public void AssignCharacter(CharacterDefinition definition, int currentHealth)
        {
            if (definition == null) return;
            character = definition;
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
            if (health == null) health = GetComponent<Health>();

            body.gravityScale = character.gravityScale;
            if (originalColliderSize == Vector2.zero)
                originalColliderSize = boxCollider.size;

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = character.tintColor;

            health.InitializeWithCurrent(character.maxHealth, Team.Player, currentHealth);
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
            }
        }
    }
}
