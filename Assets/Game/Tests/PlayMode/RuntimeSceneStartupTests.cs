using System.Collections;
using System.Linq;
using NUnit.Framework;
using SteelRain.Core;
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
    }
}
