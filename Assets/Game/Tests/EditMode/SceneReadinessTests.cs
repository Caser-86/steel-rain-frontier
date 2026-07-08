using System.IO;
using System.Linq;
using NUnit.Framework;
using SteelRain.Core;
using SteelRain.Enemies;
using SteelRain.Game;
using SteelRain.Levels;
using SteelRain.Player;
using SteelRain.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SteelRain.Tests
{
    [TestFixture]
    public sealed class SceneReadinessTests
    {
        private static readonly string[] ExpectedBuildScenes =
        {
            "Assets/Scenes/Boot.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Level01_VerticalSlice.unity",
            "Assets/Scenes/Level02_Factory.unity",
            "Assets/Scenes/Level03_Warzone.unity",
            "Assets/Scenes/Level04_Bunker.unity",
            "Assets/Scenes/Level05_Citadel.unity",
            "Assets/Scenes/EndlessMode.unity"
        };

        private static readonly string[] GameplayScenes =
        {
            "Assets/Scenes/Level01_VerticalSlice.unity",
            "Assets/Scenes/Level02_Factory.unity",
            "Assets/Scenes/Level03_Warzone.unity",
            "Assets/Scenes/Level04_Bunker.unity",
            "Assets/Scenes/Level05_Citadel.unity"
        };

        [TearDown]
        public void TearDown()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [Test]
        public void BuildSettings_ContainExpectedPlayableScenesInOrder()
        {
            var enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            CollectionAssert.AreEqual(ExpectedBuildScenes, enabledScenes);
            foreach (var scenePath in ExpectedBuildScenes)
                Assert.IsTrue(File.Exists(scenePath), $"Missing build scene file: {scenePath}");
        }

        [Test]
        public void BootScene_HasBootstrapAndBootScreen()
        {
            var scene = OpenScene("Assets/Scenes/Boot.unity");

            AssertHasComponent<GameBootstrap>(scene);
            AssertHasComponent<BootScreen>(scene);
            AssertHasComponent<SceneFader>(scene);
        }

        [Test]
        public void MainMenuScene_HasMenuAndSceneFader()
        {
            var scene = OpenScene("Assets/Scenes/MainMenu.unity");

            AssertHasComponent<MainMenu>(scene);
            AssertHasComponent<SceneFader>(scene);
        }

        [Test]
        public void GameplayScenes_HavePlayerLoopAndProgressionComponents()
        {
            foreach (var scenePath in GameplayScenes)
            {
                var scene = OpenScene(scenePath);

                AssertPlayablePlayerReady(scene);
                AssertHasComponent<GameLoop>(scene);
                AssertHasComponent<AchievementTracker>(scene);
            }
        }

        [Test]
        public void GameplayScenes_HaveCombatAndOutcomeComponents()
        {
            foreach (var scenePath in GameplayScenes)
            {
                var scene = OpenScene(scenePath);

                AssertHasCombatEncounter(scene);
                AssertHasComponent<LevelEndTrigger>(scene);
                AssertHasComponent<VictoryScreen>(scene);
                AssertHasComponent<GameOverScreen>(scene);
                AssertHasComponent<GameCompleteScreen>(scene);
            }
        }

        [Test]
        public void EndlessScene_HasPlayerLoopAndEndlessModeComponents()
        {
            var scene = OpenScene("Assets/Scenes/EndlessMode.unity");

            AssertPlayablePlayerReady(scene);
            AssertHasComponent<GameLoop>(scene);
            AssertHasComponent<AchievementTracker>(scene);
            AssertEndlessModeReady(AssertHasComponent<EndlessMode>(scene));
        }

        private static Scene OpenScene(string scenePath)
        {
            Assert.IsTrue(File.Exists(scenePath), $"Scene file does not exist: {scenePath}");
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Assert.IsTrue(scene.IsValid(), $"Scene did not open: {scenePath}");
            return scene;
        }

        private static T AssertHasComponent<T>(Scene scene) where T : Component
        {
            var component = Resources.FindObjectsOfTypeAll<T>()
                .FirstOrDefault(found => found != null && found.gameObject.scene == scene);

            Assert.IsNotNull(component, $"{scene.path} is missing required component {typeof(T).Name}");
            return component;
        }

        private static void AssertHasCombatEncounter(Scene scene)
        {
            var enemyCount =
                CountComponentsInScene<EnemyController>(scene) +
                CountComponentsInScene<FlyingDroneEnemy>(scene) +
                CountComponentsInScene<TurretBoss>(scene) +
                CountComponentsInScene<MiniBossWalker>(scene);

            Assert.Greater(enemyCount, 0, $"{scene.path} must include at least one combat encounter");
        }

        private static int CountComponentsInScene<T>(Scene scene) where T : Component
        {
            return Resources.FindObjectsOfTypeAll<T>()
                .Count(found => found != null && found.gameObject.scene == scene);
        }

        private static void AssertPlayablePlayerReady(Scene scene)
        {
            var squad = AssertHasComponent<PlayerSquad>(scene);
            AssertHasComponent<PlayerController2D>(scene);
            AssertHasComponent<PlayerCombat>(scene);

            var squadSo = new SerializedObject(squad);
            Assert.IsNotNull(squadSo.FindProperty("controller").objectReferenceValue,
                $"{scene.path} PlayerSquad.controller is not assigned");
            Assert.IsNotNull(squadSo.FindProperty("combat").objectReferenceValue,
                $"{scene.path} PlayerSquad.combat is not assigned");

            var members = squadSo.FindProperty("members");
            Assert.GreaterOrEqual(members.arraySize, 4, $"{scene.path} PlayerSquad must include all 4 characters");
            for (var i = 0; i < members.arraySize; i++)
            {
                Assert.IsNotNull(members.GetArrayElementAtIndex(i).objectReferenceValue,
                    $"{scene.path} PlayerSquad.members[{i}] is not assigned");
            }
        }

        private static void AssertEndlessModeReady(EndlessMode endless)
        {
            var endlessSo = new SerializedObject(endless);
            var enemyPrefabs = endlessSo.FindProperty("enemyPrefabs");
            Assert.Greater(enemyPrefabs.arraySize, 0, "EndlessMode.enemyPrefabs must not be empty");
            for (var i = 0; i < enemyPrefabs.arraySize; i++)
            {
                Assert.IsNotNull(enemyPrefabs.GetArrayElementAtIndex(i).objectReferenceValue,
                    $"EndlessMode.enemyPrefabs[{i}] is not assigned");
            }

            Assert.IsNotNull(endlessSo.FindProperty("bossPrefab").objectReferenceValue,
                "EndlessMode.bossPrefab is not assigned");
            Assert.IsNotNull(endlessSo.FindProperty("healthPickupPrefab").objectReferenceValue,
                "EndlessMode.healthPickupPrefab is not assigned");
            Assert.IsNotNull(endlessSo.FindProperty("weaponPickupPrefab").objectReferenceValue,
                "EndlessMode.weaponPickupPrefab is not assigned");
        }
    }
}
