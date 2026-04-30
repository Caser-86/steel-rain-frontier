using NUnit.Framework;
using SteelRain.Levels;

public sealed class ExplosionRadiusPreviewTests
{
    [Test]
    public void DiameterFromRadius_DoublesRadius()
    {
        Assert.AreEqual(5f, ExplosionRadiusPreview.DiameterFromRadius(2.5f));
    }
}
