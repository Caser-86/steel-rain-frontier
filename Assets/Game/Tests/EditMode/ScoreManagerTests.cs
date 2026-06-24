using NUnit.Framework;
using SteelRain.Core;
using UnityEngine;

public sealed class ScoreManagerTests
{
    [SetUp]
    public void Setup()
    {
        PlayerPrefs.DeleteKey("Save_Score");
        PlayerPrefs.DeleteKey("Save_HighScore");
        for (int i = 0; i < 10; i++)
            PlayerPrefs.DeleteKey("Save_Leaderboard_" + i);
        ScoreManager.Reset();
    }

    [TearDown]
    public void Teardown()
    {
        PlayerPrefs.DeleteKey("Save_Score");
        PlayerPrefs.DeleteKey("Save_HighScore");
        for (int i = 0; i < 10; i++)
            PlayerPrefs.DeleteKey("Save_Leaderboard_" + i);
    }

    [Test]
    public void AddKill_IncreasesScore()
    {
        ScoreManager.AddKill(10);
        Assert.AreEqual(10, ScoreManager.Score);
    }

    [Test]
    public void AddKill_FirstKill_NoComboMultiplier()
    {
        ScoreManager.AddKill(10);
        Assert.AreEqual(1, ScoreManager.Combo);
    }

    [Test]
    public void AddKill_ConsecutiveKills_IncreaseCombo()
    {
        ScoreManager.AddKill(10);
        ScoreManager.AddKill(10);
        ScoreManager.AddKill(10);

        Assert.AreEqual(3, ScoreManager.Combo);
        // 10*1 + 10*2 + 10*3 = 60
        Assert.AreEqual(60, ScoreManager.Score);
    }

    [Test]
    public void AddScore_IncreasesScoreWithoutCombo()
    {
        ScoreManager.AddScore(50);
        Assert.AreEqual(50, ScoreManager.Score);
        // AddScore 不触发连击
        Assert.AreEqual(0, ScoreManager.Combo);
    }

    [Test]
    public void AddScore_NegativeOrZero_DoesNothing()
    {
        ScoreManager.AddScore(50);
        ScoreManager.AddScore(0);
        ScoreManager.AddScore(-10);
        Assert.AreEqual(50, ScoreManager.Score);
    }

    [Test]
    public void AddScore_FiresScoreChanged()
    {
        int last = -1;
        ScoreManager.ScoreChanged += s => last = s;
        ScoreManager.AddScore(42);
        Assert.AreEqual(42, last);
    }

    [Test]
    public void Reset_ZeroesScoreAndCombo()
    {
        ScoreManager.AddKill(10);
        ScoreManager.AddKill(10);

        ScoreManager.Reset();

        Assert.AreEqual(0, ScoreManager.Score);
        Assert.AreEqual(0, ScoreManager.Combo);
    }

    [Test]
    public void ScoreChanged_FiresOnAddKill()
    {
        int last = -1;
        ScoreManager.ScoreChanged += s => last = s;
        ScoreManager.AddKill(7);
        Assert.AreEqual(7, last);
    }

    [Test]
    public void ComboChanged_FiresOnAddKill()
    {
        int last = -1;
        ScoreManager.ComboChanged += c => last = c;
        ScoreManager.AddKill(5);
        Assert.AreEqual(1, last);
    }

    [Test]
    public void AddToLeaderboard_InsertsNewHigh()
    {
        ScoreManager.AddToLeaderboard(100);
        var board = ScoreManager.GetLeaderboard();

        Assert.AreEqual(100, board[0]);
        Assert.AreEqual(0, board[9]);
    }

    [Test]
    public void AddToLeaderboard_OrdersDescending()
    {
        ScoreManager.AddToLeaderboard(50);
        ScoreManager.AddToLeaderboard(200);
        ScoreManager.AddToLeaderboard(100);
        var board = ScoreManager.GetLeaderboard();

        Assert.AreEqual(200, board[0]);
        Assert.AreEqual(100, board[1]);
        Assert.AreEqual(50, board[2]);
    }

    [Test]
    public void GetLeaderboard_AlwaysTenSlots()
    {
        var board = ScoreManager.GetLeaderboard();
        Assert.AreEqual(10, board.Length);
    }
}
