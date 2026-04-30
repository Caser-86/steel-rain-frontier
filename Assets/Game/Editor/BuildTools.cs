using System.IO;
using System;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace SteelRain.EditorTools
{
    public static class BuildTools
    {
        public static void BuildWindowsVerticalSlice()
        {
            const string outputPath = "Builds/Windows/SteelRainFrontier.exe";
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            var options = new BuildPlayerOptions
            {
                scenes = new[]
                {
                    "Assets/Scenes/MainMenu.unity",
                    "Assets/Scenes/CharacterSelect.unity",
                    "Assets/Scenes/LevelSelect.unity",
                    "Assets/Scenes/Level01_VerticalSlice.unity"
                },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception($"Windows build failed: {report.summary.result}");
        }
    }
}
