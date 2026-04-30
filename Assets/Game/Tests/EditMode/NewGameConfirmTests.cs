using NUnit.Framework;
using SteelRain.UI;

public sealed class NewGameConfirmTests
{
    [Test]
    public void MainMenu_ConfirmPromptNamesCurrentSlot()
    {
        Assert.AreEqual("Slot 2 has save. Press NEW GAME again to overwrite.", MainMenuController.FormatOverwritePrompt(2));
    }
}
