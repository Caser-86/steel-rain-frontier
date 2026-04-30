namespace SteelRain.Levels
{
    public static class LevelPacingRules
    {
        public const int FirstLevelMinimumUpgradePickups = 5;
        public const float MaximumEmptyStretchSeconds = 8f;

        public static bool HasEnoughUpgradePickupsForFirstLevel(int upgradePickupCount)
        {
            return upgradePickupCount >= FirstLevelMinimumUpgradePickups;
        }

        public static bool IsEmptyStretchAcceptable(float seconds)
        {
            return seconds <= MaximumEmptyStretchSeconds;
        }
    }
}
