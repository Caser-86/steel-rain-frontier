using System.Reflection;
using NUnit.Framework;
using SteelRain.Core;
using SteelRain.Levels;
using UnityEngine;

namespace SteelRain.Tests
{
    [TestFixture]
    public sealed class CheckpointManagerTests
    {
        private static readonly FieldInfo CurrentLevelIndexField =
            typeof(LevelManager).GetField("currentLevelIndex", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly FieldInfo InEndlessModeField =
            typeof(LevelManager).GetField("inEndlessMode", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly FieldInfo CurrentSpawnField =
            typeof(CheckpointManager).GetField("currentSpawn", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo AwakeMethod =
            typeof(CheckpointManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo IsQuittingField =
            typeof(SaveSystem).GetField("isQuitting", BindingFlags.NonPublic | BindingFlags.Static);

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();
            SetQuitting(false);
            SetLevelState(0, false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(GameObject.Find("Player"));
            Object.DestroyImmediate(GameObject.Find("CheckpointManager"));
            SetLevelState(0, false);
            SetQuitting(false);
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void Awake_IgnoresCheckpointSavedForDifferentLevel()
        {
            SaveSystem.SaveCheckpoint(new Vector3(99f, 4f, 0f));
            SaveSystem.SaveLevelIndex(0);
            SetLevelState(1, false);

            var player = CreatePlayer(new Vector3(0f, 2f, 0f));
            var manager = CreateInitializedManager();

            Assert.AreEqual(player.transform.position, CurrentSpawn(manager));
        }

        [Test]
        public void Awake_UsesCheckpointSavedForCurrentLevel()
        {
            var savedCheckpoint = new Vector3(42f, 3f, 0f);
            SaveSystem.SaveCheckpoint(savedCheckpoint);
            SaveSystem.SaveLevelIndex(2);
            SetLevelState(2, false);

            CreatePlayer(new Vector3(0f, 2f, 0f));
            var manager = CreateInitializedManager();

            Assert.AreEqual(savedCheckpoint, CurrentSpawn(manager));
        }

        private static GameObject CreatePlayer(Vector3 position)
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = position;
            return player;
        }

        private static Vector3 CurrentSpawn(CheckpointManager manager)
        {
            return (Vector3)CurrentSpawnField.GetValue(manager);
        }

        private static CheckpointManager CreateInitializedManager()
        {
            var manager = new GameObject("CheckpointManager").AddComponent<CheckpointManager>();
            AwakeMethod.Invoke(manager, null);
            return manager;
        }

        private static void SetLevelState(int levelIndex, bool inEndlessMode)
        {
            CurrentLevelIndexField.SetValue(null, levelIndex);
            InEndlessModeField.SetValue(null, inEndlessMode);
        }

        private static void SetQuitting(bool quitting)
        {
            IsQuittingField.SetValue(null, quitting);
        }
    }
}
