using NUnit.Framework;
using SteelRain.Core;
using UnityEngine;

public sealed class DifficultyManagerTests
{
    [SetUp]
    public void Setup()
    {
        // 重置为 Normal，避免上轮测试干扰
        PlayerPrefs.DeleteKey("Settings_Difficulty");
        DifficultyManager.SetDifficulty(Difficulty.Normal);
    }

    [TearDown]
    public void Teardown()
    {
        PlayerPrefs.DeleteKey("Settings_Difficulty");
    }

    [Test]
    public void Default_IsNormal()
    {
        // 静态构造函数从 PlayerPrefs 读取，但 Setup 已经强制设为 Normal
        Assert.AreEqual(Difficulty.Normal, DifficultyManager.Current);
    }

    [Test]
    public void SetDifficulty_UpdatesCurrent()
    {
        DifficultyManager.SetDifficulty(Difficulty.Hard);
        Assert.AreEqual(Difficulty.Hard, DifficultyManager.Current);
    }

    [Test]
    public void SetDifficulty_PersistsToPlayerPrefs()
    {
        DifficultyManager.SetDifficulty(Difficulty.Easy);
        int stored = PlayerPrefs.GetInt("Settings_Difficulty", -1);
        Assert.AreEqual((int)Difficulty.Easy, stored);
    }

    [Test]
    public void GetHealthMultiplier_Easy_IsLower()
    {
        DifficultyManager.SetDifficulty(Difficulty.Easy);
        Assert.AreEqual(0.7f, DifficultyManager.GetHealthMultiplier(), 0.001f);
    }

    [Test]
    public void GetHealthMultiplier_Normal_IsOne()
    {
        DifficultyManager.SetDifficulty(Difficulty.Normal);
        Assert.AreEqual(1f, DifficultyManager.GetHealthMultiplier(), 0.001f);
    }

    [Test]
    public void GetHealthMultiplier_Hard_IsHigher()
    {
        DifficultyManager.SetDifficulty(Difficulty.Hard);
        Assert.AreEqual(1.4f, DifficultyManager.GetHealthMultiplier(), 0.001f);
    }

    [Test]
    public void GetDamageMultiplier_ScalesByDifficulty()
    {
        DifficultyManager.SetDifficulty(Difficulty.Easy);
        Assert.AreEqual(0.6f, DifficultyManager.GetDamageMultiplier(), 0.001f);

        DifficultyManager.SetDifficulty(Difficulty.Hard);
        Assert.AreEqual(1.5f, DifficultyManager.GetDamageMultiplier(), 0.001f);
    }

    [Test]
    public void GetEnemySpeedMultiplier_ScalesByDifficulty()
    {
        DifficultyManager.SetDifficulty(Difficulty.Easy);
        Assert.AreEqual(0.8f, DifficultyManager.GetEnemySpeedMultiplier(), 0.001f);

        DifficultyManager.SetDifficulty(Difficulty.Hard);
        Assert.AreEqual(1.2f, DifficultyManager.GetEnemySpeedMultiplier(), 0.001f);
    }

    [Test]
    public void GetDifficultyName_ReturnsReadableName()
    {
        DifficultyManager.SetDifficulty(Difficulty.Easy);
        Assert.AreEqual("Easy", DifficultyManager.GetDifficultyName());

        DifficultyManager.SetDifficulty(Difficulty.Normal);
        Assert.AreEqual("Normal", DifficultyManager.GetDifficultyName());

        DifficultyManager.SetDifficulty(Difficulty.Hard);
        Assert.AreEqual("Hard", DifficultyManager.GetDifficultyName());
    }
}
