using SteelRain.Core;
using SteelRain.VFX;
using UnityEngine;

namespace SteelRain.Enemies
{
    [RequireComponent(typeof(Health))]
    public sealed class ShieldGuard : MonoBehaviour
    {
        [SerializeField] private float facingSign = 1f;
        [SerializeField] private float frontalDamageMultiplier = 0.25f;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.DamageFilter += FilterDamage;
        }

        private void OnDestroy()
        {
            if (health != null)
                health.DamageFilter -= FilterDamage;
        }

        public static int CalculateDamage(int amount, Vector2 incomingDirection, float facingSign)
        {
            var hitFromFront = Mathf.Sign(incomingDirection.x) != Mathf.Sign(facingSign);
            return hitFromFront ? Mathf.Max(1, Mathf.RoundToInt(amount * 0.25f)) : amount;
        }

        public void SetFacing(float newFacingSign)
        {
            if (Mathf.Abs(newFacingSign) > 0.01f)
                facingSign = Mathf.Sign(newFacingSign);
        }

        private DamageInfo FilterDamage(DamageInfo info)
        {
            var amount = CalculateDamage(info.Amount, info.Direction, facingSign);
            if (amount < info.Amount)
                ImpactBurst.Spawn(transform.position + Vector3.up * 0.25f, new Color(0.25f, 0.75f, 1f, 0.75f), 0.25f, 1.1f, 0.16f);

            return new DamageInfo(amount, info.SourceTeam, info.Direction);
        }
    }
}
