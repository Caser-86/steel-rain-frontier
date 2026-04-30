namespace SteelRain.UI
{
    public static class CombatHudText
    {
        public static string FormatWeaponLevel(int level)
        {
            return level switch
            {
                <= 0 => "Lv0 Base firepower",
                1 => "Lv1 Damage up",
                2 => "Lv2 Heavy form online",
                _ => "Lv3 Skill unlocked"
            };
        }

        public static string FormatCharacter(string characterName, string skillStatus)
        {
            return $"{characterName} | {skillStatus} | Switch 1-4 / Tab";
        }

        public static string FormatCheckpointToast()
        {
            return "CHECKPOINT SAVED";
        }
    }
}
