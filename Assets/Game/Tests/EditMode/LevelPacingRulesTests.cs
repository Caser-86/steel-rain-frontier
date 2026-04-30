using NUnit.Framework;
using SteelRain.Levels;

public sealed class LevelPacingRulesTests
{
    [Test]
    public void FirstLevel_HasEnoughUpgradePickupsForRecovery()
    {
        Assert.IsTrue(LevelPacingRules.HasEnoughUpgradePickupsForFirstLevel(9));
        Assert.IsFalse(LevelPacingRules.HasEnoughUpgradePickupsForFirstLevel(4));
    }

    [Test]
    public void EmptyStretch_RejectsLongQuietWalks()
    {
        Assert.IsTrue(LevelPacingRules.IsEmptyStretchAcceptable(7.9f));
        Assert.IsFalse(LevelPacingRules.IsEmptyStretchAcceptable(8.1f));
    }
}
