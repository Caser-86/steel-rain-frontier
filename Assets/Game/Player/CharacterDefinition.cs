using UnityEngine;

namespace SteelRain.Player
{
    [CreateAssetMenu(menuName = "Steel Rain/Character Definition")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id = "aila";
        public string displayName = "Aila";
        [TextArea(2, 4)]
        [Tooltip("角色背景故事，用于叙事过场和角色选择界面")]
        public string lore = "";
        [Tooltip("角色战斗风格简述（一句话）")]
        public string combatStyle = "";

        [Header("Movement")]
        public int maxHealth = 6;
        public float moveSpeed = 7.2f;
        public float jumpVelocity = 9.5f;
        public float gravityScale = 3.2f;
        public float fallGravityMultiplier = 1.35f;
        public float crouchSpeedMultiplier = 0.45f;
        public float crouchColliderHeightMultiplier = 0.6f;
        public float climbSpeed = 3.5f;

        [Header("Dodge")]
        public float dodgeSpeed = 12f;
        public float dodgeDuration = 0.16f;
        public float dodgeCooldown = 0.65f;

        [Header("Visual")]
        public Color tintColor = Color.white;
        public Sprite portraitSprite;
        public Sprite crouchSprite;
        public Sprite proneSprite;
        public Sprite jumpSprite;

        [Header("Projectile")]
        public Color projectileColor = Color.white;
        public float projectileScale = 1f;
        [Tooltip("角色伤害倍率，影响所有武器伤害")]
        public float damageMultiplier = 1f;
    }
}
