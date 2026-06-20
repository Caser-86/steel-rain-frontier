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
        [SerializeField] private float proneColliderHeightMultiplier = 0.35f;
        [SerializeField] private KeyCode proneKey = KeyCode.Z;

        private Rigidbody2D body;
        private BoxCollider2D boxCollider;
        private Health health;
        private PlayerDodge dodge;
        private Vector2 moveInput;
        private bool crouching;
        private bool prone;
        private float jumpBufferCounter;
        private float coyoteCounter;
        private float dustTimer;
        private Vector2 originalColliderSize;
        private SpriteRenderer spriteRenderer;
        private System.Action<DamageInfo> onDamagedHandler;

        public bool IsGrounded { get; private set; }
        public bool IsCrouching => crouching;
        public bool IsProne => prone;
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
            spriteRenderer = GetComponent<SpriteRenderer>();

            // 确保Rigidbody2D配置正确
            if (body != null)
            {
                body.bodyType = RigidbodyType2D.Dynamic;
                body.simulated = true;
            }

            // 确保BoxCollider2D启用且非Trigger
            if (boxCollider != null)
            {
                boxCollider.enabled = true;
                boxCollider.isTrigger = false;
                if (boxCollider.size == Vector2.zero)
                    boxCollider.size = new Vector2(0.8f, 1.2f);
                originalColliderSize = boxCollider.size;
            }

            if (character != null)
            {
                health.Initialize(character.maxHealth, Team.Player);
                body.gravityScale = character.gravityScale;
                UpdateVisualSprite();
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

            prone = Input.GetKey(proneKey) && IsGrounded;
            crouching = moveInput.y < -0.5f && IsGrounded && !prone;

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

            // 使用多种方法检测地面，确保可靠性
            IsGrounded = CheckGrounded();

            // 防止掉出场景边界
            if (transform.position.y < -10f)
            {
                transform.position = new Vector3(transform.position.x, 2f, 0f);
                body.linearVelocity = Vector2.zero;
            }

            if (IsGrounded)
                coyoteCounter = coyoteTime;

            var velocity = body.linearVelocity;

            // 速度：prone 最慢，crouch 中等，正常最快
            float speed;
            if (prone)
                speed = character.moveSpeed * 0.2f;
            else if (crouching)
                speed = character.moveSpeed * character.crouchSpeedMultiplier;
            else
                speed = character.moveSpeed;

            if (!MovementLocked && (dodge == null || !dodge.IsDodging))
                velocity.x = moveInput.x * speed;

            // Collider 高度：prone 最低，crouch 中等，正常最高
            if (originalColliderSize != Vector2.zero)
            {
                if (prone)
                {
                    var proneSize = originalColliderSize;
                    proneSize.y *= proneColliderHeightMultiplier;
                    boxCollider.size = proneSize;
                }
                else if (crouching)
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

            // 跳跃（prone 状态下不能跳，需先起来）
            if (jumpBufferCounter > 0 && coyoteCounter > 0 && !crouching && !prone)
            {
                velocity.y = character.jumpVelocity;
                jumpBufferCounter = 0;
                coyoteCounter = 0;
                AudioManager.Play("sfx_jump", 0.5f);
            }

            if (velocity.y < 0f)
                velocity.y += Physics2D.gravity.y * (character.fallGravityMultiplier - 1f) * Time.fixedDeltaTime;

            body.linearVelocity = velocity;

            // 更新视觉精灵（根据状态切换）
            UpdateVisualSprite();

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

        /// <summary>
        /// 根据当前状态（prone/crouch/jump/normal）和角色定义切换精灵图。
        /// 这是让 5 个角色看起来有差别的关键。
        /// </summary>
        private void UpdateVisualSprite()
        {
            if (spriteRenderer == null || character == null) return;

            Sprite target = null;
            if (prone && character.proneSprite != null)
                target = character.proneSprite;
            else if (crouching && character.crouchSprite != null)
                target = character.crouchSprite;
            else if (!IsGrounded && character.jumpSprite != null)
                target = character.jumpSprite;
            else if (character.portraitSprite != null)
                target = character.portraitSprite;

            if (target != null && spriteRenderer.sprite != target)
                spriteRenderer.sprite = target;
        }

        /// <summary>
        /// 检测Player是否在地面上。使用OverlapCircle确保可靠性。
        /// </summary>
        private bool CheckGrounded()
        {
            if (boxCollider == null) return false;

            // 使用groundCheck位置进行OverlapCircle检测，这是最可靠的方法
            if (groundCheck != null)
            {
                var hit = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);
                if (hit != null && hit.gameObject != gameObject)
                {
                    return true;
                }
            }

            // 备用方法：使用OverlapBox检测底部
            var colliderBounds = boxCollider.bounds;
            var checkCenter = new Vector2(colliderBounds.center.x, colliderBounds.min.y - 0.1f);
            var checkSize = new Vector2(colliderBounds.size.x * 0.8f, 0.2f);
            var hit2 = Physics2D.OverlapBox(checkCenter, checkSize, 0f, groundMask);
            if (hit2 != null && hit2.gameObject != gameObject)
            {
                return true;
            }

            return false;
        }

        public void AssignCharacter(CharacterDefinition definition, int currentHealth)
        {
            if (definition == null) return;
            character = definition;
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
            if (health == null) health = GetComponent<Health>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            body.gravityScale = character.gravityScale;
            if (originalColliderSize == Vector2.zero)
                originalColliderSize = boxCollider.size;

            if (spriteRenderer != null) spriteRenderer.color = character.tintColor;

            // 切换角色时立即更换精灵图（这是让 5 个角色看起来有差别的关键）
            UpdateVisualSprite();

            // 应用商店购买的最大血量加成
            var hpBonus = SaveSystem.LoadMaxHealthBonus();
            var maxHealth = character.maxHealth + hpBonus;
            health.InitializeWithCurrent(maxHealth, Team.Player, currentHealth);
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
