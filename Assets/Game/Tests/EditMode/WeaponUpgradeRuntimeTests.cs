using NUnit.Framework;
using SteelRain.Weapons;
using UnityEngine;

public sealed class WeaponUpgradeRuntimeTests
{
    [Test]
    public void BaseAmmo_IsInfinite_WhenDefinitionAllowsIt()
    {
        var weapon = CreateWeapon();
        weapon.baseAmmoInfinite = true;
        var runtime = new WeaponRuntime(weapon, 0);

        Assert.IsTrue(runtime.ConsumeAmmo());
        Assert.AreEqual(-1, runtime.Ammo);

        Object.DestroyImmediate(weapon.forms[0]);
        Object.DestroyImmediate(weapon);
    }

    [Test]
    public void Upgrade_StopsAtLevelThree()
    {
        var weapon = CreateWeapon();
        var runtime = new WeaponRuntime(weapon, 0);

        Assert.AreEqual(1, runtime.Upgrade());
        Assert.AreEqual(2, runtime.Upgrade());
        Assert.AreEqual(3, runtime.Upgrade());
        Assert.AreEqual(3, runtime.Upgrade());
        Assert.AreEqual(3, runtime.Level);

        Object.DestroyImmediate(weapon.forms[0]);
        Object.DestroyImmediate(weapon);
    }

    [Test]
    public void ResetUpgrades_ReturnsToLevelZero()
    {
        var weapon = CreateWeapon();
        var runtime = new WeaponRuntime(weapon, 0);
        runtime.Upgrade();
        runtime.Upgrade();

        runtime.ResetUpgrades();

        Assert.AreEqual(0, runtime.Level);

        Object.DestroyImmediate(weapon.forms[0]);
        Object.DestroyImmediate(weapon);
    }

    [Test]
    public void Upgrades_ScaleDamageAndFireRate()
    {
        var weapon = CreateWeapon();
        var runtime = new WeaponRuntime(weapon, 0);

        Assert.AreEqual(2, runtime.GetDamage());
        Assert.AreEqual(8f, runtime.GetFireRate());

        runtime.Upgrade();
        Assert.AreEqual(3, runtime.GetDamage());
        Assert.AreEqual(8.8f, runtime.GetFireRate(), 0.001f);

        Object.DestroyImmediate(weapon.forms[0]);
        Object.DestroyImmediate(weapon);
    }

    private static WeaponDefinition CreateWeapon()
    {
        var form = ScriptableObject.CreateInstance<WeaponFormDefinition>();
        form.displayName = "Auto";
        form.ammoCost = 1;
        form.damage = 2;
        form.fireRate = 8f;

        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.displayName = "Assault Rifle";
        weapon.forms = new[] { form };
        weapon.startingAmmo = 0;
        return weapon;
    }
}
