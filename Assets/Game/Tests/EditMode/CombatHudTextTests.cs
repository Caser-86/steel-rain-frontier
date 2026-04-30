using NUnit.Framework;
using SteelRain.UI;

public sealed class CombatHudTextTests
{
    [Test]
    public void FormatWeaponLevel_ExplainsEachUpgradeTier()
    {
        Assert.AreEqual("Lv0 Base firepower", CombatHudText.FormatWeaponLevel(0));
        Assert.AreEqual("Lv1 Damage up", CombatHudText.FormatWeaponLevel(1));
        Assert.AreEqual("Lv2 Heavy form online", CombatHudText.FormatWeaponLevel(2));
        Assert.AreEqual("Lv3 Skill unlocked", CombatHudText.FormatWeaponLevel(3));
    }

    [Test]
    public void FormatCharacter_IncludesSwitchHint()
    {
        Assert.AreEqual("Aila | Skill Ready | Switch 1-4 / Tab", CombatHudText.FormatCharacter("Aila", "Skill Ready"));
    }

    [Test]
    public void FormatToast_ShowsCheckpointAndSave()
    {
        Assert.AreEqual("CHECKPOINT SAVED", CombatHudText.FormatCheckpointToast());
    }
}
