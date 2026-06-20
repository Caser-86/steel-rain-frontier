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

    [Test]
    public void Invincible_PreventsDamage()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);
        health.SetInvincible(true);

        health.ApplyDamage(new DamageInfo(5, Team.Enemy, Vector2.right));

        Assert.AreEqual(10, health.Current);
        health.SetInvincible(false);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Invincible_RefCount_AllowsMultipleSources()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);

        // 两个系统同时设置无敌
        health.SetInvincible(true); // refCount = 1
        health.SetInvincible(true); // refCount = 2

        health.ApplyDamage(new DamageInfo(5, Team.Enemy, Vector2.right));
        Assert.AreEqual(10, health.Current);

        // 解除一个，仍然无敌
        health.SetInvincible(false); // refCount = 1
        health.ApplyDamage(new DamageInfo(3, Team.Enemy, Vector2.right));
        Assert.AreEqual(10, health.Current);

        // 解除最后一个，不再无敌
        health.SetInvincible(false); // refCount = 0
        health.ApplyDamage(new DamageInfo(2, Team.Enemy, Vector2.right));
        Assert.AreEqual(8, health.Current);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void Revive_RestoresHealth()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);
        health.ApplyDamage(new DamageInfo(10, Team.Enemy, Vector2.right));
        Assert.IsTrue(health.IsDead);

        health.Revive(5);

        Assert.IsFalse(health.IsDead);
        Assert.AreEqual(5, health.Current);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Revive_ClearsInvincible()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);
        health.SetInvincible(true);
        health.ApplyDamage(new DamageInfo(10, Team.Enemy, Vector2.right));

        health.Revive(5);

        Assert.IsFalse(health.IsDead);
        Assert.AreEqual(5, health.Current);
        // 无敌应被清除，可以再次受伤
        health.ApplyDamage(new DamageInfo(3, Team.Enemy, Vector2.right));
        Assert.AreEqual(2, health.Current);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void ReviveFull_RestoresToMax()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);
        health.ApplyDamage(new DamageInfo(10, Team.Enemy, Vector2.right));

        health.ReviveFull();

        Assert.IsFalse(health.IsDead);
        Assert.AreEqual(10, health.Current);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Damage_ZeroAmount_IsIgnored()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);

        health.ApplyDamage(new DamageInfo(0, Team.Enemy, Vector2.right));

        Assert.AreEqual(10, health.Current);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Heal_ZeroAmount_IsIgnored()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);
        health.ApplyDamage(new DamageInfo(5, Team.Enemy, Vector2.right));

        health.Heal(0);

        Assert.AreEqual(5, health.Current);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void ClearInvincible_ForceClearsAll()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);
        health.SetInvincible(true);
        health.SetInvincible(true); // refCount = 2

        health.ClearInvincible();

        health.ApplyDamage(new DamageInfo(5, Team.Enemy, Vector2.right));
        Assert.AreEqual(5, health.Current);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Heal_DoesNothingWhenDead()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);
        health.ApplyDamage(new DamageInfo(10, Team.Enemy, Vector2.right));
        Assert.IsTrue(health.IsDead);

        health.Heal(5);

        Assert.AreEqual(0, health.Current);
        Object.DestroyImmediate(go);
    }
}
