#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SteelRain.EditorTools
{
    /// <summary>
    /// Windows 构建工具，命令行可调用。
    /// </summary>
    public static class BuildTools
    {
        [MenuItem("Steel Rain/Build Windows Executable")]
        public static void BuildWindows()
        {
            var scenes = new[]
            {
                "Assets/Scenes/Boot.unity",
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Level01_VerticalSlice.unity",
                "Assets/Scenes/Level02_Factory.unity",
                "Assets/Scenes/Level03_Warzone.unity",
                "Assets/Scenes/Level04_Bunker.unity",
                "Assets/Scenes/Level05_Citadel.unity"
            };

            foreach (var s in scenes)
            {
                if (!File.Exists(s))
                {
                    Debug.LogError($"[BuildTools] Scene not found: {s}. Run BuildAll first.");
                    return;
                }
            }

            var outputPath = "Builds/Windows/SteelRainFrontier.exe";
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildTools] Build succeeded: {outputPath}");
            }
            else
            {
                Debug.LogError($"[BuildTools] Build failed. Result: {summary.result}");
            }
        }
    }
}
#endif
