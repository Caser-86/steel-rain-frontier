using NUnit.Framework;
using SteelRain.Save;
using SteelRain.UI;

public sealed class MenuFlowTests
{
    [Test]
    public void MainMenu_NewGameTargetsCharacterSelect()
    {
        Assert.AreEqual("CharacterSelect", MainMenuController.NewGameSceneName);
    }

    [Test]
    public void CharacterSelect_CreatesFreshLevelOneSave()
    {
        var save = CharacterSelectController.CreateNewGameSave(2);

        Assert.AreEqual("Level01", save.levelId);
        Assert.AreEqual(2, save.selectedCharacterIndex);
        Assert.AreEqual("assault_rifle", save.weaponId);
        Assert.AreEqual(0, save.weaponLevel);
        Assert.IsFalse(save.level01Cleared);
    }
}
