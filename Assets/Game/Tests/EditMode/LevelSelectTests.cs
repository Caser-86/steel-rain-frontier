using NUnit.Framework;
using SteelRain.UI;

public sealed class LevelSelectTests
{
    [Test]
    public void MainMenu_LevelSelectTargetsLevelSelectScene()
    {
        Assert.AreEqual("LevelSelect", MainMenuController.LevelSelectSceneName);
    }

    [Test]
    public void LevelSelect_OnlyLevelOneUnlockedInVerticalSlice()
    {
        Assert.IsTrue(LevelSelectController.IsLevelUnlocked("Level01", false));
        Assert.IsFalse(LevelSelectController.IsLevelUnlocked("Level02", false));
        Assert.IsFalse(LevelSelectController.IsLevelUnlocked("Level02", true));
    }
}
