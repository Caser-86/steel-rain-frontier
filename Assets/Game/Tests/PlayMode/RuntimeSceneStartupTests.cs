using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SteelRain.Core;
using SteelRain.Game;
using SteelRain.Levels;
using SteelRain.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SteelRain.Tests
{
    public sealed class RuntimeSceneStartupTests
    {
        private static readonly string[] NonBootScenes =
        {
            "MainMenu",
            "Level01_VerticalSlice",
            "Level02_Factory",
            "Level03_Warzone",
            "Level04_Bunker",
            "Level05_Citadel",
            "EndlessMode"
        };

        private static readonly string[] GameplayScenes =
        {
            "Level01_VerticalSlice",
            "Level02_Factory",
            "Level03_Warzone",
            "Level04_Bunker",
            "Level05_Citadel"
        };

        private static readonly string[] LateCampaignScenes =
        {
            "Level03_Warzone",
            "Level04_Bunker",
            "Level05_Citadel"
        };

        private static readonly FieldInfo CheckpointPlayerField =
            typeof(CheckpointManager).GetField("player", BindingFlags.NonPublic | BindingFlags.Instance);

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator NonBootScenes_DoNotCreateBootScreenAtRuntime()
        {
            foreach (var sceneName in NonBootScenes)
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                yield return null;

                var bootScreens = Object.FindObjectsByType<BootScreen>(FindObjectsSortMode.None)
                    .Where(screen => screen.gameObject.scene == SceneManager.GetActiveScene())
                    .ToArray();

                Assert.Zero(bootScreens.Length, $"{sceneName} should not create BootScreen at runtime.");
            }
        }

        [UnityTest]
        public IEnumerator NonBootScenes_WithButtonsHaveEventSystemAtRuntime()
        {
            foreach (var sceneName in NonBootScenes)
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                yield return null;

                var scene = SceneManager.GetActiveScene();
                var hasButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .Any(button => button.gameObject.scene == scene);
                var hasEventSystem = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None)
                    .Any(eventSystem => eventSystem.gameObject.scene == scene);

                if (hasButtons)
                    Assert.IsTrue(hasEventSystem, $"{sceneName} has UI buttons but no runtime EventSystem.");
            }
        }

        [UnityTest]
        public IEnumerator GameplayScenes_CheckpointManagerBindsPlayerAtRuntime()
        {
            foreach (var sceneName in GameplayScenes)
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                yield return null;

                var manager = Object.FindFirstObjectByType<CheckpointManager>();
                Assert.IsNotNull(manager, $"{sceneName} must have a CheckpointManager.");

                var player = CheckpointPlayerField.GetValue(manager) as Transform;
                Assert.IsNotNull(player, $"{sceneName} CheckpointManager.player is not assigned at runtime.");
                Assert.AreEqual("Player", player.tag, $"{sceneName} CheckpointManager.player must reference the player.");
            }
        }

        [UnityTest]
        public IEnumerator LateCampaignScenes_RunForSeveralSecondsWithCoreLoop()
        {
            foreach (var sceneName in LateCampaignScenes)
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                yield return RunForSeconds(2f);

                var scene = SceneManager.GetActiveScene();
                Assert.AreEqual(sceneName, scene.name);
                Assert.IsNotNull(Object.FindFirstObjectByType<GameLoop>(), $"{sceneName} must keep GameLoop alive.");
                Assert.IsNotNull(Object.FindFirstObjectByType<PlayerSquad>(), $"{sceneName} must keep PlayerSquad alive.");
                Assert.IsNotNull(Object.FindFirstObjectByType<LevelEndTrigger>(), $"{sceneName} must keep a level end trigger.");
            }
        }

        [UnityTest]
        public IEnumerator EndlessMode_StartsFirstWaveAndSpawnsEnemiesAtRuntime()
        {
            yield return SceneManager.LoadSceneAsync("EndlessMode", LoadSceneMode.Single);
            yield return RunForSeconds(4f);

            var endless = Object.FindFirstObjectByType<EndlessMode>();
            Assert.IsNotNull(endless, "EndlessMode scene must keep EndlessMode component alive.");
            Assert.GreaterOrEqual(endless.CurrentWave, 1, "EndlessMode must start the first wave at runtime.");
            Assert.Greater(endless.EnemiesAlive, 0, "EndlessMode must spawn enemies during the first wave.");
            Assert.IsNotNull(Object.FindFirstObjectByType<PlayerSquad>(), "EndlessMode must keep PlayerSquad alive.");
        }

        private static IEnumerator RunForSeconds(float seconds)
        {
            var endTime = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < endTime)
                yield return null;
        }
    }
}
