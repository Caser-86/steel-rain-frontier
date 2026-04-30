using NUnit.Framework;
using SteelRain.Player;
using UnityEngine;

public sealed class CharacterRuntimeTests
{
    [Test]
    public void Runtime_PreservesBackgroundUpgradeState()
    {
        var aila = CreateCharacter("aila", "Aila", CharacterSkillId.BreakthroughFire);
        var runtime = new CharacterRuntime(aila);

        runtime.SetWeaponLevel("assault_rifle", 3);
        runtime.CurrentHealth = 4;

        Assert.AreEqual(3, runtime.GetWeaponLevel("assault_rifle"));
        Assert.AreEqual(4, runtime.CurrentHealth);

        Object.DestroyImmediate(aila);
    }

    [Test]
    public void Death_ClearsOnlyCurrentWeaponUpgrade()
    {
        var bruno = CreateCharacter("bruno", "Bruno", CharacterSkillId.TrenchShield);
        var runtime = new CharacterRuntime(bruno);
        runtime.SetWeaponLevel("assault_rifle", 3);
        runtime.SetWeaponLevel("shotgun", 2);
        runtime.SelectedWeaponId = "assault_rifle";

        runtime.ClearCurrentWeaponUpgradeOnDeath();

        Assert.AreEqual(0, runtime.GetWeaponLevel("assault_rifle"));
        Assert.AreEqual(2, runtime.GetWeaponLevel("shotgun"));

        Object.DestroyImmediate(bruno);
    }

    [Test]
    public void WeaponLevel_IsClampedToThree()
    {
        var niko = CreateCharacter("niko", "Niko", CharacterSkillId.TimeRift);
        var runtime = new CharacterRuntime(niko);

        runtime.SetWeaponLevel("energy_gun", 9);

        Assert.AreEqual(3, runtime.GetWeaponLevel("energy_gun"));

        Object.DestroyImmediate(niko);
    }

    private static CharacterDefinition CreateCharacter(string id, string name, CharacterSkillId skill)
    {
        var character = ScriptableObject.CreateInstance<CharacterDefinition>();
        character.id = id;
        character.displayName = name;
        character.maxHealth = 6;
        character.skillId = skill;
        return character;
    }
}
