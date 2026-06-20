#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace SteelRain.Editor
{
    /// <summary>
    /// 命令行自动执行器：用于 batchmode 下自动运行 Sprite Sheet 导入。
    /// 用法：Unity -batchmode -quit -executeMethod SteelRain.Editor.BatchRunner.RunImportSheets -logFile import.log
    /// </summary>
    public static class BatchRunner
    {
        public static void RunImportSheets()
        {
            Debug.Log("[BatchRunner] Starting Sprite Sheet import...");
            try
            {
                SpriteSheetImporter.ImportAllSheets();
                Debug.Log("[BatchRunner] Import completed successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BatchRunner] Import failed: {e.Message}\n{e.StackTrace}");
            }
        }

        public static void RunBuildAll()
        {
            Debug.Log("[BatchRunner] Starting Build All...");
            try
            {
                SteelRain.EditorTools.VerticalSliceBuilder.BuildAll();
                Debug.Log("[BatchRunner] Build All completed successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BatchRunner] Build All failed: {e.Message}\n{e.StackTrace}");
            }
        }

        public static void RunGenerateAndBuild()
        {
            Debug.Log("[BatchRunner] Starting Generate Enhanced Art + Build All...");
            try
            {
                EnhancedArtGenerator.GenerateAll();
                Debug.Log("[BatchRunner] Enhanced Art generated successfully.");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                SteelRain.EditorTools.VerticalSliceBuilder.BuildAll();
                Debug.Log("[BatchRunner] Generate + Build All completed successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BatchRunner] Generate + Build All failed: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}
#endif
