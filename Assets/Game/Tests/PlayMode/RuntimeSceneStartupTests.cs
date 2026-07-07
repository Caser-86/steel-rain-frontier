using System.Collections;
using System.Linq;
using NUnit.Framework;
using SteelRain.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SteelRain.Tests
{
    public sealed class RuntimeSceneStartupTests
    {
        private static readonly string[] NonBootScenes =
        {
            "MainMenu",
            "Level01_VerticalSlice",
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
    }
}
