using NUnit.Framework;
using SteelRain.Enemies;

public sealed class EnemyTacticsTests
{
    [Test]
    public void MoveDecision_RetreatsWhenTargetTooClose()
    {
        var decision = EnemyTactics.GetMoveDecision(1.2f, 6f, 2f, true);

        Assert.AreEqual(EnemyMoveDecision.Retreat, decision);
    }

    [Test]
    public void MoveDecision_AdvancesWhenOutsideAttackRange()
    {
        var decision = EnemyTactics.GetMoveDecision(7f, 6f, 2f, true);

        Assert.AreEqual(EnemyMoveDecision.Advance, decision);
    }

    [Test]
    public void WarningDuration_IsLongerForSniper()
    {
        Assert.Greater(
            EnemyTactics.GetWarningDuration(EnemyAttackPattern.SniperShot),
            EnemyTactics.GetWarningDuration(EnemyAttackPattern.RifleBurst));
    }
}
