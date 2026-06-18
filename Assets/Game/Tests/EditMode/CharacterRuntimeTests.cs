using NUnit.Framework;
using SteelRain.Player;
using UnityEngine;

public sealed class CharacterRuntimeTests
{
    private CharacterDefinition CreateDef(string id, int maxHp)
    {
        var def = ScriptableObject.CreateInstance<CharacterDefinition>();
        def.id = id;
        def.displayName = id.ToUpper();
        def.maxHealth = maxHp;
        return def;
    }

    [Test]
    public void Constructor_InitializesHealthToMax()
    {
        var def = CreateDef("aila", 6);
        var runtime = new CharacterRuntime(def);

        Assert.AreEqual(def, runtime.Definition);
        Assert.AreEqual(6, runtime.CurrentHealth);
    }

    [Test]
    public void Constructor_DefaultWeapon_IsAssaultRifle()
    {
        var def = CreateDef("bruno", 8);
        var runtime = new CharacterRuntime(def);

        Assert.AreEqual("assault_rifle", runtime.SelectedWeaponId);
    }

    [Test]
    public void GetWeaponLevel_UnknownWeapon_ReturnsZero()
    {
        var def = CreateDef("mara", 5);
        var runtime = new CharacterRuntime(def);

        Assert.AreEqual(0, runtime.GetWeaponLevel("shotgun"));
    }

    [Test]
    public void SetWeaponLevel_StoresValue()
    {
        var def = CreateDef("niko", 4);
        var runtime = new CharacterRuntime(def);

        runtime.SetWeaponLevel("shotgun", 2);

        Assert.AreEqual(2, runtime.GetWeaponLevel("shotgun"));
    }

    [Test]
    public void SetWeaponLevel_ClampsToZeroThree()
    {
        var def = CreateDef("aila", 6);
        var runtime = new CharacterRuntime(def);

        runtime.SetWeaponLevel("rifle", 5);
        Assert.AreEqual(3, runtime.GetWeaponLevel("rifle"));

        runtime.SetWeaponLevel("rifle", -1);
        Assert.AreEqual(0, runtime.GetWeaponLevel("rifle"));
    }

    [Test]
    public void SetWeaponLevel_DifferentWeapons_TrackedSeparately()
    {
        var def = CreateDef("bruno", 6);
        var runtime = new CharacterRuntime(def);

        runtime.SetWeaponLevel("rifle", 2);
        runtime.SetWeaponLevel("shotgun", 1);

        Assert.AreEqual(2, runtime.GetWeaponLevel("rifle"));
        Assert.AreEqual(1, runtime.GetWeaponLevel("shotgun"));
    }

    [Test]
    public void ClearCurrentWeaponUpgradeOnDeath_ResetsSelectedWeapon()
    {
        var def = CreateDef("mara", 6);
        var runtime = new CharacterRuntime(def);
        runtime.SetWeaponLevel("assault_rifle", 3);

        runtime.ClearCurrentWeaponUpgradeOnDeath();

        Assert.AreEqual(0, runtime.GetWeaponLevel("assault_rifle"));
    }

    [Test]
    public void ClearCurrentWeaponUpgradeOnDeath_KeepsOtherWeapons()
    {
        var def = CreateDef("niko", 4);
        var runtime = new CharacterRuntime(def);
        runtime.SetWeaponLevel("assault_rifle", 3);
        runtime.SetWeaponLevel("shotgun", 2);

        runtime.ClearCurrentWeaponUpgradeOnDeath();

        Assert.AreEqual(0, runtime.GetWeaponLevel("assault_rifle"));
        Assert.AreEqual(2, runtime.GetWeaponLevel("shotgun"));
    }

    [Test]
    public void CurrentHealth_CanBeModified()
    {
        var def = CreateDef("aila", 6);
        var runtime = new CharacterRuntime(def);

        runtime.CurrentHealth = 3;

        Assert.AreEqual(3, runtime.CurrentHealth);
    }
}
