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
    public void CurrentHealth_CanBeModified()
    {
        var def = CreateDef("aila", 6);
        var runtime = new CharacterRuntime(def);

        runtime.CurrentHealth = 3;

        Assert.AreEqual(3, runtime.CurrentHealth);
    }
}
