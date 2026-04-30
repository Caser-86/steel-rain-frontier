namespace SteelRain.Enemies
{
    public static class BossPhaseTactics
    {
        public static BossPhase GetPhase(int currentHealth, int maxHealth)
        {
            if (maxHealth <= 0)
                return BossPhase.Advancing;

            var ratio = (float)currentHealth / maxHealth;
            if (ratio <= 0.34f)
                return BossPhase.CoreExposed;

            if (ratio <= 0.5f)
                return BossPhase.Enraged;

            return BossPhase.Advancing;
        }

        public static int GetBurstProjectileCount(BossPhase phase)
        {
            return phase switch
            {
                BossPhase.Enraged => 5,
                BossPhase.CoreExposed => 3,
                _ => 3
            };
        }

        public static float GetWeakPointWindow(BossPhase phase)
        {
            return phase switch
            {
                BossPhase.CoreExposed => 4.5f,
                BossPhase.Enraged => 2f,
                _ => 0f
            };
        }

        public static string GetPhaseLabel(BossPhase phase)
        {
            return phase switch
            {
                BossPhase.CoreExposed => "PHASE 3: CORE OVERLOAD",
                BossPhase.Enraged => "PHASE 2: RAGE - CORE EXPOSED",
                _ => "PHASE 1: ADVANCING"
            };
        }
    }
}
