using NUnit.Framework;
using SteelRain.Core;
using UnityEngine;

public sealed class HealthTests
{
    [Test]
    public void Damage_ReducesCurrentHealth()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);

        health.ApplyDamage(new DamageInfo(3, Team.Enemy, Vector2.right));

        Assert.AreEqual(7, health.Current);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Damage_FromSameTeam_IsIgnored()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);

        health.ApplyDamage(new DamageInfo(3, Team.Player, Vector2.right));

        Assert.AreEqual(10, health.Current);
        Object.DestroyImmediate(go);
    }
}
