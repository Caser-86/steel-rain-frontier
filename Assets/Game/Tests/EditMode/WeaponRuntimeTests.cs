using NUnit.Framework;
using SteelRain.Weapons;

public sealed class WeaponRuntimeTests
{
    private WeaponDefinition CreateTestWeapon()
    {
        var weapon = UnityEngine.ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.id = "test";
        weapon.displayName = "Test";
        weapon.startingAmmo = -1;
        weapon.baseAmmoInfinite = true;

        var form = UnityEngine.ScriptableObject.CreateInstance<WeaponFormDefinition>();
        form.damage = 2;
        form.ammoCost = 1;
        form.fireRate = 10f;
        form.projectileSpeed = 20f;
        form.levelOneDamageMultiplier = 1.5f;
        form.levelTwoDamageMultiplier = 2.0f;
        form.levelThreeDamageMultiplier = 3.0f;
        form.levelOneFireRateMultiplier = 1.1f;
        form.levelTwoFireRateMultiplier = 1.2f;
        form.levelThreeFireRateMultiplier = 1.3f;
        weapon.forms = new[] { form };

        return weapon;
    }

    [Test]
    public void Constructor_InitializesCorrectly()
    {
        var weapon = CreateTestWeapon();
        var runtime = new WeaponRuntime(weapon, 50);

        Assert.AreEqual(weapon, runtime.Definition);
        Assert.AreEqual(50, runtime.Ammo);
        Assert.AreEqual(0, runtime.Level);
    }

    [Test]
    public void Constructor_NullWeapon_Throws()
    {
        Assert.Throws<System.ArgumentNullException>(() => new WeaponRuntime(null, 0));
    }

    [Test]
    public void Upgrade_IncreasesLevel()
    {
        var weapon = CreateTestWeapon();
        var runtime = new WeaponRuntime(weapon, -1);

        Assert.AreEqual(1, runtime.Upgrade());
        Assert.AreEqual(2, runtime.Upgrade());
        Assert.AreEqual(3, runtime.Upgrade());
    }

    [Test]
    public void Upgrade_CapsAtThree()
    {
        var weapon = CreateTestWeapon();
        var runtime = new WeaponRuntime(weapon, -1);

        runtime.Upgrade();
        runtime.Upgrade();
        runtime.Upgrade();
        Assert.AreEqual(3, runtime.Upgrade());
        Assert.AreEqual(3, runtime.Level);
    }

    [Test]
    public void ResetUpgrades_SetsLevelToZero()
    {
        var weapon = CreateTestWeapon();
        var runtime = new WeaponRuntime(weapon, -1);

        runtime.Upgrade();
        runtime.Upgrade();
        runtime.ResetUpgrades();
        Assert.AreEqual(0, runtime.Level);
    }

    [Test]
    public void ConsumeAmmo_InfiniteWeapon_KeepsInfinite()
    {
        var weapon = CreateTestWeapon();
        var runtime = new WeaponRuntime(weapon, -1);

        Assert.IsTrue(runtime.ConsumeAmmo());
        // 无限弹药使用int.MaxValue表示，避免UI显示-1
        Assert.AreEqual(int.MaxValue, runtime.Ammo);
    }

    [Test]
    public void ConsumeAmmo_FiniteWeapon_ConsumesAmmo()
    {
        var weapon = CreateTestWeapon();
        weapon.baseAmmoInfinite = false;
        var runtime = new WeaponRuntime(weapon, 10);

        Assert.IsTrue(runtime.ConsumeAmmo());
        Assert.AreEqual(9, runtime.Ammo);
    }

    [Test]
    public void ConsumeAmmo_FiniteWeapon_NoAmmo_ReturnsFalse()
    {
        var weapon = CreateTestWeapon();
        weapon.baseAmmoInfinite = false;
        var runtime = new WeaponRuntime(weapon, 0);

        Assert.IsFalse(runtime.ConsumeAmmo());
        Assert.AreEqual(0, runtime.Ammo);
    }

    [Test]
    public void GetDamage_BaseLevel_ReturnsBaseDamage()
    {
        var weapon = CreateTestWeapon();
        var runtime = new WeaponRuntime(weapon, -1);

        Assert.AreEqual(2, runtime.GetDamage());
    }

    [Test]
    public void GetDamage_Level3_ReturnsScaledDamage()
    {
        var weapon = CreateTestWeapon();
        var runtime = new WeaponRuntime(weapon, -1);

        runtime.Upgrade();
        runtime.Upgrade();
        runtime.Upgrade();
        Assert.AreEqual(6, runtime.GetDamage());
    }

    [Test]
    public void GetFireRate_BaseLevel_ReturnsBaseRate()
    {
        var weapon = CreateTestWeapon();
        var runtime = new WeaponRuntime(weapon, -1);

        Assert.AreEqual(10f, runtime.GetFireRate(), 0.01f);
    }

    [Test]
    public void CycleForm_WrapsAround()
    {
        var weapon = CreateTestWeapon();
        var form2 = UnityEngine.ScriptableObject.CreateInstance<WeaponFormDefinition>();
        form2.damage = 5;
        form2.ammoCost = 2;
        form2.fireRate = 5f;
        form2.projectileSpeed = 15f;
        weapon.forms = new[] { weapon.forms[0], form2 };

        var runtime = new WeaponRuntime(weapon, -1);
        Assert.AreEqual("Auto", runtime.CurrentForm.displayName);

        runtime.CycleForm();
        Assert.AreEqual(5, runtime.CurrentForm.damage);

        runtime.CycleForm();
        Assert.AreEqual("Auto", runtime.CurrentForm.displayName);
    }

    [Test]
    public void CanFire_InfiniteWeapon_AlwaysTrue()
    {
        var weapon = CreateTestWeapon();
        var runtime = new WeaponRuntime(weapon, -1);

        Assert.IsTrue(runtime.CanFire());
    }

    [Test]
    public void CanFire_FiniteWeapon_WithAmmo_True()
    {
        var weapon = CreateTestWeapon();
        weapon.baseAmmoInfinite = false;
        var runtime = new WeaponRuntime(weapon, 5);

        Assert.IsTrue(runtime.CanFire());
    }

    [Test]
    public void CanFire_FiniteWeapon_NoAmmo_False()
    {
        var weapon = CreateTestWeapon();
        weapon.baseAmmoInfinite = false;
        var runtime = new WeaponRuntime(weapon, 0);

        Assert.IsFalse(runtime.CanFire());
    }
}
