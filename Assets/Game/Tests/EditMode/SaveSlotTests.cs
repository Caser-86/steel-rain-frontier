using NUnit.Framework;
using SteelRain.Save;

public sealed class SaveSlotTests
{
    [Test]
    public void NormalizeSlot_ClampsToThreeSlots()
    {
        Assert.AreEqual(1, SaveService.NormalizeSlot(0));
        Assert.AreEqual(1, SaveService.NormalizeSlot(1));
        Assert.AreEqual(2, SaveService.NormalizeSlot(2));
        Assert.AreEqual(3, SaveService.NormalizeSlot(9));
    }

    [Test]
    public void SlotFileName_UsesStableOneBasedNames()
    {
        Assert.AreEqual("steel-rain-save-slot-1.json", SaveService.GetFileNameForSlot(1));
        Assert.AreEqual("steel-rain-save-slot-3.json", SaveService.GetFileNameForSlot(3));
    }
}
