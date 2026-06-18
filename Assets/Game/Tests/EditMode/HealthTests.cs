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

    [Test]
    public void Damage_ToZero_TriggersDied()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(3, Team.Player);

        var diedCalled = false;
        health.Died += () => diedCalled = true;

        health.ApplyDamage(new DamageInfo(3, Team.Enemy, Vector2.right));

        Assert.IsTrue(diedCalled);
        Assert.AreEqual(0, health.Current);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Heal_RestoresHealth()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);
        health.ApplyDamage(new DamageInfo(5, Team.Enemy, Vector2.right));

        health.Heal(3);

        Assert.AreEqual(8, health.Current);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Heal_DoesNotExceedMax()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);
        health.ApplyDamage(new DamageInfo(3, Team.Enemy, Vector2.right));

        health.Heal(100);

        Assert.AreEqual(10, health.Current);
        Object.DestroyImmediate(go);
    }
}
