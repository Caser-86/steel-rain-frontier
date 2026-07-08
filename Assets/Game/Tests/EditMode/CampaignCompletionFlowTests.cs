using System.Reflection;
using NUnit.Framework;
using SteelRain.Core;
using SteelRain.Levels;
using SteelRain.UI;
using UnityEditor;
using UnityEngine;

namespace SteelRain.Tests
{
    [TestFixture]
    public sealed class CampaignCompletionFlowTests
    {
        private static readonly FieldInfo CurrentLevelIndexField =
            typeof(LevelManager).GetField("currentLevelIndex", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly FieldInfo InEndlessModeField =
            typeof(LevelManager).GetField("inEndlessMode", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly FieldInfo IsQuittingField =
            typeof(SaveSystem).GetField("isQuitting", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly MethodInfo LevelEndTriggerEnterMethod =
            typeof(LevelEndTrigger).GetMethod("OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance);

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();
            AchievementManager.ResetAll();
            SetLevelState(0, false);
            IsQuittingField.SetValue(null, false);
            Time.timeScale = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            Object.DestroyImmediate(GameObject.Find("Tracker"));
            Object.DestroyImmediate(GameObject.Find("Victory"));
            Object.DestroyImmediate(GameObject.Find("Complete"));
            Object.DestroyImmediate(GameObject.Find("CompletePanel"));
            Object.DestroyImmediate(GameObject.Find("LevelEnd"));
            Object.DestroyImmediate(GameObject.Find("Player"));
            SetLevelState(0, false);
            IsQuittingField.SetValue(null, false);
            AchievementManager.ResetAll();
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void OnLevelComplete_CalledTwice_CountsCurrentLevelOnce()
        {
            var tracker = new GameObject("Tracker").AddComponent<AchievementTracker>();

            tracker.OnLevelComplete();
            tracker.OnLevelComplete();

            Assert.AreEqual(1, AchievementManager.GetStat(AchievementManager.StatId.LevelsCompleted));
        }

        [Test]
        public void FinalVictory_RecordsLevelCompletionAndUnlocksEndlessMode()
        {
            AchievementManager.AddStat(AchievementManager.StatId.LevelsCompleted, LevelManager.TotalLevels - 1);
            SetLevelState(LevelManager.TotalLevels - 1, false);

            new GameObject("Tracker").AddComponent<AchievementTracker>();
            var complete = new GameObject("Complete").AddComponent<GameCompleteScreen>();
            var completePanel = new GameObject("CompletePanel");
            AssignSerializedField(complete, "panel", completePanel);

            var victory = new GameObject("Victory").AddComponent<VictoryScreen>();
            AssignSerializedField(victory, "gameCompleteScreen", complete);

            victory.Show();

            Assert.AreEqual(LevelManager.TotalLevels,
                AchievementManager.GetStat(AchievementManager.StatId.LevelsCompleted));
            Assert.IsTrue(AchievementManager.IsUnlocked(AchievementManager.AchievementId.GameComplete));
            Assert.IsTrue(SaveSystem.IsEndlessUnlocked());
        }

        [Test]
        public void FinalLevelEndTrigger_ShowsCompletionAndUnlocksEndlessMode()
        {
            AchievementManager.AddStat(AchievementManager.StatId.LevelsCompleted, LevelManager.TotalLevels - 1);
            SetLevelState(LevelManager.TotalLevels - 1, false);

            new GameObject("Tracker").AddComponent<AchievementTracker>();
            var complete = new GameObject("Complete").AddComponent<GameCompleteScreen>();
            var completePanel = new GameObject("CompletePanel");
            AssignSerializedField(complete, "panel", completePanel);

            var victory = new GameObject("Victory").AddComponent<VictoryScreen>();
            AssignSerializedField(victory, "gameCompleteScreen", complete);

            var trigger = new GameObject("LevelEnd").AddComponent<LevelEndTrigger>();
            var player = new GameObject("Player");
            player.tag = "Player";
            var collider = player.AddComponent<BoxCollider2D>();

            LevelEndTriggerEnterMethod.Invoke(trigger, new object[] { collider });

            Assert.IsTrue(completePanel.activeSelf);
            Assert.AreEqual(LevelManager.TotalLevels,
                AchievementManager.GetStat(AchievementManager.StatId.LevelsCompleted));
            Assert.IsTrue(AchievementManager.IsUnlocked(AchievementManager.AchievementId.GameComplete));
            Assert.IsTrue(SaveSystem.IsEndlessUnlocked());
        }

        private static void SetLevelState(int levelIndex, bool inEndlessMode)
        {
            CurrentLevelIndexField.SetValue(null, levelIndex);
            InEndlessModeField.SetValue(null, inEndlessMode);
        }

        private static void AssignSerializedField(Object target, string fieldName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(fieldName).objectReferenceValue = value;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
