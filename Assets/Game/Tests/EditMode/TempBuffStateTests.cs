using NUnit.Framework;
using SteelRain.Core;

public sealed class TempBuffStateTests
{
    [SetUp]
    public void SetUp()
    {
        TempBuffState.Reset();
    }

    [Test]
    public void Reset_ClearsAllBuffs()
    {
        TempBuffState.ShieldActive = true;
        TempBuffState.ShieldTimer = 5f;
        TempBuffState.SpeedBoostActive = true;
        TempBuffState.SpeedBoostTimer = 3f;
        TempBuffState.SpeedBoostMultiplier = 1.5f;

        TempBuffState.Reset();

        Assert.IsFalse(TempBuffState.ShieldActive);
        Assert.AreEqual(0f, TempBuffState.ShieldTimer);
        Assert.IsFalse(TempBuffState.SpeedBoostActive);
        Assert.AreEqual(0f, TempBuffState.SpeedBoostTimer);
        Assert.AreEqual(1f, TempBuffState.SpeedBoostMultiplier);
    }

    [Test]
    public void ShieldActive_DefaultFalse()
    {
        Assert.IsFalse(TempBuffState.ShieldActive);
    }

    [Test]
    public void SpeedBoostActive_DefaultFalse()
    {
        Assert.IsFalse(TempBuffState.SpeedBoostActive);
    }

    [Test]
    public void SpeedBoostMultiplier_DefaultOne()
    {
        Assert.AreEqual(1f, TempBuffState.SpeedBoostMultiplier);
    }
}
