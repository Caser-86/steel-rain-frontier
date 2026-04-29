using NUnit.Framework;
using SteelRain.Player;
using UnityEngine;

public sealed class PlayerControllerSmokeTests
{
    [Test]
    public void CharacterDefinition_DefaultAilaValues_ArePlayable()
    {
        var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
        Assert.GreaterOrEqual(definition.maxHealth, 3);
        Assert.Greater(definition.moveSpeed, 0f);
        Assert.Greater(definition.jumpVelocity, 0f);
        Object.DestroyImmediate(definition);
    }
}
