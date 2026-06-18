using NUnit.Framework;
using SteelRain.Core;

namespace SteelRain.Tests
{
    [TestFixture]
    public class AchievementManagerTests
    {
        [SetUp]
        public void SetUp()
        {
            AchievementManager.ResetAll();
        }

        [TearDown]
        public void TearDown()
        {
            AchievementManager.ResetAll();
        }

        [Test]
        public void Unlock_NewAchievement_ReturnsTrue()
        {
            // Act
            var result = AchievementManager.Unlock(AchievementManager.AchievementId.FirstBlood);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(AchievementManager.IsUnlocked(AchievementManager.AchievementId.FirstBlood));
        }

        [Test]
        public void Unlock_AlreadyUnlocked_ReturnsFalse()
        {
            // Arrange
            AchievementManager.Unlock(AchievementManager.AchievementId.FirstBlood);

            // Act
            var result = AchievementManager.Unlock(AchievementManager.AchievementId.FirstBlood);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsUnlocked_NotUnlocked_ReturnsFalse()
        {
            // Act
            var result = AchievementManager.IsUnlocked(AchievementManager.AchievementId.FirstBlood);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetAchievementName_ReturnsCorrectName()
        {
            // Act
            var name = AchievementManager.GetAchievementName(AchievementManager.AchievementId.FirstBlood);

            // Assert
            Assert.AreEqual("First Blood", name);
        }

        [Test]
        public void GetAchievementDescription_ReturnsCorrectDescription()
        {
            // Act
            var desc = AchievementManager.GetAchievementDescription(AchievementManager.AchievementId.FirstBlood);

            // Assert
            Assert.AreEqual("Defeat your first enemy", desc);
        }

        [Test]
        public void AddStat_IncrementsValue()
        {
            // Act
            AchievementManager.AddStat(AchievementManager.StatId.TotalKills);
            AchievementManager.AddStat(AchievementManager.StatId.TotalKills);
            AchievementManager.AddStat(AchievementManager.StatId.TotalKills);

            // Assert
            Assert.AreEqual(3, AchievementManager.GetStat(AchievementManager.StatId.TotalKills));
        }

        [Test]
        public void SetStat_SetsExactValue()
        {
            // Act
            AchievementManager.SetStat(AchievementManager.StatId.MaxCombo, 15);

            // Assert
            Assert.AreEqual(15, AchievementManager.GetStat(AchievementManager.StatId.MaxCombo));
        }

        [Test]
        public void AddStat_TotalKills_TriggersFirstBloodAchievement()
        {
            // Act
            AchievementManager.AddStat(AchievementManager.StatId.TotalKills, 1);

            // Assert
            Assert.IsTrue(AchievementManager.IsUnlocked(AchievementManager.AchievementId.FirstBlood));
        }

        [Test]
        public void AddStat_TotalKills10_TriggersSlayerAchievement()
        {
            // Act
            AchievementManager.AddStat(AchievementManager.StatId.TotalKills, 10);

            // Assert
            Assert.IsTrue(AchievementManager.IsUnlocked(AchievementManager.AchievementId.Kill10));
        }

        [Test]
        public void AddStat_TotalKills50_TriggersExterminatorAchievement()
        {
            // Act
            AchievementManager.AddStat(AchievementManager.StatId.TotalKills, 50);

            // Assert
            Assert.IsTrue(AchievementManager.IsUnlocked(AchievementManager.AchievementId.Kill50));
        }

        [Test]
        public void AddStat_TotalKills100_TriggersOneManArmyAchievement()
        {
            // Act
            AchievementManager.AddStat(AchievementManager.StatId.TotalKills, 100);

            // Assert
            Assert.IsTrue(AchievementManager.IsUnlocked(AchievementManager.AchievementId.Kill100));
        }

        [Test]
        public void AddStat_MaxCombo5_TriggersComboStarterAchievement()
        {
            // Act
            AchievementManager.SetStat(AchievementManager.StatId.MaxCombo, 5);

            // Assert
            Assert.IsTrue(AchievementManager.IsUnlocked(AchievementManager.AchievementId.Combo5));
        }

        [Test]
        public void AddStat_MaxCombo10_TriggersComboMasterAchievement()
        {
            // Act
            AchievementManager.SetStat(AchievementManager.StatId.MaxCombo, 10);

            // Assert
            Assert.IsTrue(AchievementManager.IsUnlocked(AchievementManager.AchievementId.Combo10));
        }

        [Test]
        public void AddStat_MaxCombo20_TriggersComboLegendAchievement()
        {
            // Act
            AchievementManager.SetStat(AchievementManager.StatId.MaxCombo, 20);

            // Assert
            Assert.IsTrue(AchievementManager.IsUnlocked(AchievementManager.AchievementId.Combo20));
        }

        [Test]
        public void AddStat_TotalBossKills_TriggersBossSlayerAchievement()
        {
            // Act
            AchievementManager.AddStat(AchievementManager.StatId.TotalBossKills, 1);

            // Assert
            Assert.IsTrue(AchievementManager.IsUnlocked(AchievementManager.AchievementId.FirstBoss));
        }

        [Test]
        public void AddStat_LevelsCompleted1_TriggersLevel1CompleteAchievement()
        {
            // Act
            AchievementManager.AddStat(AchievementManager.StatId.LevelsCompleted, 1);

            // Assert
            Assert.IsTrue(AchievementManager.IsUnlocked(AchievementManager.AchievementId.Level1Complete));
        }

        [Test]
        public void AddStat_LevelsCompleted2_TriggersGameCompleteAchievement()
        {
            // Act
            AchievementManager.AddStat(AchievementManager.StatId.LevelsCompleted, 2);

            // Assert
            Assert.IsTrue(AchievementManager.IsUnlocked(AchievementManager.AchievementId.GameComplete));
        }

        [Test]
        public void GetUnlockedCount_ReturnsCorrectCount()
        {
            // Arrange
            AchievementManager.Unlock(AchievementManager.AchievementId.FirstBlood);
            AchievementManager.Unlock(AchievementManager.AchievementId.Combo5);
            AchievementManager.Unlock(AchievementManager.AchievementId.FirstBoss);

            // Act
            var count = AchievementManager.GetUnlockedCount();

            // Assert
            Assert.AreEqual(3, count);
        }

        [Test]
        public void GetTotalCount_ReturnsCorrectCount()
        {
            // Act
            var count = AchievementManager.GetTotalCount();

            // Assert
            Assert.AreEqual(19, count);
        }

        [Test]
        public void ResetAll_ClearsAllAchievements()
        {
            // Arrange
            AchievementManager.Unlock(AchievementManager.AchievementId.FirstBlood);
            AchievementManager.AddStat(AchievementManager.StatId.TotalKills, 10);

            // Act
            AchievementManager.ResetAll();

            // Assert
            Assert.IsFalse(AchievementManager.IsUnlocked(AchievementManager.AchievementId.FirstBlood));
            Assert.AreEqual(0, AchievementManager.GetStat(AchievementManager.StatId.TotalKills));
        }

        [Test]
        public void AddFloatStat_IncrementsValue()
        {
            // Act
            AchievementManager.AddFloatStat(AchievementManager.StatId.TotalPlayTime, 100f);
            AchievementManager.AddFloatStat(AchievementManager.StatId.TotalPlayTime, 200f);

            // Assert
            Assert.AreEqual(300f, AchievementManager.GetFloatStat(AchievementManager.StatId.TotalPlayTime), 0.01f);
        }
    }
}
