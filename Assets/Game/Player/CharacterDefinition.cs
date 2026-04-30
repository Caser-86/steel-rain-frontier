using UnityEngine;

namespace SteelRain.Player
{
    [CreateAssetMenu(menuName = "Steel Rain/Character Definition")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        public string id = "aila";
        public string displayName = "Aila";
        public int maxHealth = 6;
        public float moveSpeed = 7.5f;
        public float jumpVelocity = 9.5f;
        public float gravityScale = 3.2f;
        public float fallGravityMultiplier = 1.35f;
        public float dodgeSpeed = 13f;
        public float dodgeDuration = 0.18f;
        public float dodgeCooldown = 0.55f;
        public CharacterSkillId skillId = CharacterSkillId.BreakthroughFire;
        public float crouchSpeedMultiplier = 0.45f;
        public float crouchColliderHeightMultiplier = 0.6f;
        public float climbSpeed = 3.5f;
        public float skillCooldown = 10f;
    }
}
