using System.Reflection;
using NUnit.Framework;
using SteelRain.Core;
using UnityEngine;

public sealed class SaveSystemTests
{
    private static readonly FieldInfo IsQuittingField =
        typeof(SaveSystem).GetField("isQuitting", BindingFlags.NonPublic | BindingFlags.Static);

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteAll();
        SetQuitting(false);
    }

    [TearDown]
    public void TearDown()
    {
        SetQuitting(false);
        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void ClearSquadSave_RemovesSavedSquadDataWhenNotQuitting()
    {
        PlayerPrefs.SetInt("Save_SquadAlive", 15);
        PlayerPrefs.SetInt("Save_SquadActiveIndex", 2);
        PlayerPrefs.SetInt("Save_SquadHealth_0", 10);
        PlayerPrefs.Save();

        SaveSystem.ClearSquadSave();

        Assert.IsFalse(PlayerPrefs.HasKey("Save_SquadAlive"));
        Assert.IsFalse(PlayerPrefs.HasKey("Save_SquadActiveIndex"));
        Assert.IsFalse(PlayerPrefs.HasKey("Save_SquadHealth_0"));
    }

    [Test]
    public void ClearSquadSave_DoesNotDeleteDataWhileQuitting()
    {
        PlayerPrefs.SetInt("Save_SquadAlive", 15);
        PlayerPrefs.SetInt("Save_SquadActiveIndex", 2);
        PlayerPrefs.SetInt("Save_SquadHealth_0", 10);
        PlayerPrefs.SetInt("Save_SquadHealth_1", 9);
        PlayerPrefs.Save();

        SetQuitting(true);

        SaveSystem.ClearSquadSave();

        Assert.AreEqual(15, PlayerPrefs.GetInt("Save_SquadAlive", -1));
        Assert.AreEqual(2, PlayerPrefs.GetInt("Save_SquadActiveIndex", -1));
        Assert.AreEqual(10, PlayerPrefs.GetInt("Save_SquadHealth_0", -1));
        Assert.AreEqual(9, PlayerPrefs.GetInt("Save_SquadHealth_1", -1));
    }

    private static void SetQuitting(bool quitting)
    {
        IsQuittingField.SetValue(null, quitting);
    }
}
