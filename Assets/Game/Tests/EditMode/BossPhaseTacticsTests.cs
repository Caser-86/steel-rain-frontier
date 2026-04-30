using NUnit.Framework;
using SteelRain.Enemies;

public sealed class BossPhaseTacticsTests
{
    [Test]
    public void GetPhase_UsesThreeHealthBands()
    {
        Assert.AreEqual(BossPhase.Advancing, BossPhaseTactics.GetPhase(30, 35));
        Assert.AreEqual(BossPhase.Enraged, BossPhaseTactics.GetPhase(17, 35));
        Assert.AreEqual(BossPhase.CoreExposed, BossPhaseTactics.GetPhase(10, 35));
    }

    [Test]
    public void CoreExposed_HasLongerWeakPointWindow()
    {
        Assert.Greater(
            BossPhaseTactics.GetWeakPointWindow(BossPhase.CoreExposed),
            BossPhaseTactics.GetWeakPointWindow(BossPhase.Enraged));
    }

    [Test]
    public void CoreExposed_FiresFewerRandomBullets()
    {
        Assert.Less(
            BossPhaseTactics.GetBurstProjectileCount(BossPhase.CoreExposed),
            BossPhaseTactics.GetBurstProjectileCount(BossPhase.Enraged));
    }
}
