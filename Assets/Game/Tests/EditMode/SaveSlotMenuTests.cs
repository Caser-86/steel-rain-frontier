using NUnit.Framework;
using SteelRain.Save;
using SteelRain.UI;

public sealed class SaveSlotMenuTests
{
    [Test]
    public void MainMenu_SaveSlotsTargetsSaveSlotScene()
    {
        Assert.AreEqual("SaveSlots", MainMenuController.SaveSlotsSceneName);
    }

    [Test]
    public void SaveSlotMenu_FormatsEmptyAndSavedSlots()
    {
        Assert.AreEqual("Slot 1 - Empty", SaveSlotMenuController.FormatSlotLabel(1, null));

        var save = new SaveData
        {
            levelId = "Level01",
            savedAtUtc = "2026-04-30T08:00:00.0000000Z"
        };

        Assert.AreEqual("Slot 2 - Level01 - 2026-04-30 08:00", SaveSlotMenuController.FormatSlotLabel(2, save));
    }
}
