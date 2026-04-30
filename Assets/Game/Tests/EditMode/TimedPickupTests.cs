using NUnit.Framework;
using SteelRain.Pickups;

public sealed class TimedPickupTests
{
    [Test]
    public void UpgradePickup_DoesNotExpire()
    {
        Assert.IsFalse(TimedPickup.ShouldExpire(PickupKind.WeaponUpgrade, 999f));
    }

    [Test]
    public void SmallHealth_ExpiresAfterEighteenSeconds()
    {
        Assert.IsFalse(TimedPickup.ShouldExpire(PickupKind.SmallHealth, 17.9f));
        Assert.IsTrue(TimedPickup.ShouldExpire(PickupKind.SmallHealth, 18.1f));
    }
}
