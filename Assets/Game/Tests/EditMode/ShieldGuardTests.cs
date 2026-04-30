using NUnit.Framework;
using SteelRain.Enemies;
using UnityEngine;

public sealed class ShieldGuardTests
{
    [Test]
    public void CalculateDamage_BlocksFrontalDamage()
    {
        var damage = ShieldGuard.CalculateDamage(4, Vector2.left, 1f);

        Assert.AreEqual(1, damage);
    }

    [Test]
    public void CalculateDamage_AllowsRearDamage()
    {
        var damage = ShieldGuard.CalculateDamage(4, Vector2.right, 1f);

        Assert.AreEqual(4, damage);
    }
}
