using UnityEngine;

namespace SteelRain.Player
{
    public enum CharacterSkillId
    {
        BreakthroughFire,
        TrenchShield,
        BombardmentMatrix,
        TimeRift
    }

    [CreateAssetMenu(menuName = "Steel Rain/Character Definition")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id = "aila";
        public string displayName = "Aila";
        public CharacterSkillId skillId = CharacterSkillId.BreakthroughFire;

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

        [Header("Skill")]
        public float skillCooldown = 10f;
    }
}
