namespace SteelRain.Enemies
{
    public static class EnemyTactics
    {
        public static EnemyMoveDecision GetMoveDecision(float distance, float attackRange, float retreatRange, bool canRetreat)
        {
            if (canRetreat && distance < retreatRange)
                return EnemyMoveDecision.Retreat;

            if (distance > attackRange)
                return EnemyMoveDecision.Advance;

            return EnemyMoveDecision.Hold;
        }

        public static float GetWarningDuration(EnemyAttackPattern pattern)
        {
            return pattern switch
            {
                EnemyAttackPattern.SniperShot => 0.85f,
                EnemyAttackPattern.MortarMarker => 0.65f,
                EnemyAttackPattern.GrenadeArc => 0.45f,
                _ => 0.18f
            };
        }
    }
}
