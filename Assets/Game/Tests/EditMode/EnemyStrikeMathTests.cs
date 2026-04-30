using NUnit.Framework;
using SteelRain.Enemies;
using UnityEngine;

public sealed class EnemyStrikeMathTests
{
    [Test]
    public void ClampLandingPoint_LimitsDistanceFromOrigin()
    {
        var origin = Vector2.zero;
        var target = new Vector2(12f, 0f);

        var landing = EnemyStrikeMath.ClampLandingPoint(origin, target, 5f);

        Assert.AreEqual(5f, landing.x, 0.001f);
    }

    [Test]
    public void DiameterFromRadius_DoublesRadius()
    {
        Assert.AreEqual(2.4f, EnemyStrikeMarker.DiameterFromRadius(1.2f), 0.001f);
    }
}
