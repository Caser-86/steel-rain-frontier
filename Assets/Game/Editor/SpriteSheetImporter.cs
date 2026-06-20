#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SteelRain.Editor
{
    /// <summary>
    /// Sprite Sheet 自动切割导入器（v2.0 双 Sheet 版）。
    /// 读取 Assets/Art/AI_Generated/ 下以 _sheet.png / _sheet1.png / _sheet2.png 结尾的图片，
    /// 根据命名规范自动切割成多个子精灵，并生成 AnimationClip。
    ///
    /// 支持的 Sheet 类型：
    /// 1. 角色双 4×4 网格（30有效帧）：player_xxx_sheet1.png + player_xxx_sheet2.png
    ///    - Sheet1: idle/walk/run/jump（15有效帧 + 1占位）
    ///    - Sheet2: crouch/prone/shoot/death/skill/weapon（15有效帧 + 1占位）
    /// 2. 敌人单/双 4×4 网格：enemy_xxx_sheet.png（向后兼容）
    /// 3. Boss 4×4 网格：miniboss_xxx_sheet.png / turret_xxx_sheet.png
    /// 4. 地形 1×8 横排：ground_xxx_sheet.png
    ///
    /// 切割后的子精灵命名与现有 AIArtImporter 规范一致：
    /// player_aila_idle_0, player_aila_walk_0, ...
    /// 因此现有 LoadOrImportSprite 和 AnimationClip 代码无需修改。
    /// </summary>
    public static class SpriteSheetImporter
    {
        private const string SourceDir = "Assets/Art/AI_Generated";
        private const string TargetDir = "Assets/Art/Generated";
        private const string AnimDir = "Assets/Art/Animations";
        private const string ControllerDir = "Assets/Art/Animators";

        [MenuItem("Steel Rain/Import Sprite Sheets")]
        public static void ImportAllSheets()
        {
            if (!Directory.Exists(SourceDir))
            {
                EditorUtility.DisplayDialog("Import Sprite Sheets",
                    $"未找到 AI 生成资源目录：\n{SourceDir}", "OK");
                return;
            }

            if (!Directory.Exists(TargetDir))
                Directory.CreateDirectory(TargetDir);
            if (!Directory.Exists(AnimDir))
                Directory.CreateDirectory(AnimDir);
            if (!Directory.Exists(ControllerDir))
                Directory.CreateDirectory(ControllerDir);

            // 查找所有 sheet 文件（支持 _sheet / _sheet1 / _sheet2）
            var sheetFiles = Directory.GetFiles(SourceDir, "*.png")
                .Where(f => IsSheetFile(Path.GetFileName(f)))
                .Where(f => !Path.GetFileName(f).StartsWith("_"))
                .ToArray();

            if (sheetFiles.Length == 0)
            {
                EditorUtility.DisplayDialog("Import Sprite Sheets",
                    $"目录 {SourceDir} 中没有 Sprite Sheet 图片。\n\n" +
                    "请先生成 Sprite Sheet（如 player_aila_sheet1.png / player_aila_sheet2.png）。\n" +
                    "提示词清单见：Assets/Art/AI_Generated/_PROMPTS_SHEET.md", "OK");
                return;
            }

            int total = sheetFiles.Length;
            int imported = 0;
            int totalSprites = 0;

            for (int i = 0; i < total; i++)
            {
                var file = sheetFiles[i];
                var fileName = Path.GetFileName(file);

                EditorUtility.DisplayProgressBar("Importing Sprite Sheets",
                    $"Processing {fileName}...", (float)i / total);

                try
                {
                    int spriteCount = ImportSingleSheet(file);
                    imported++;
                    totalSprites += spriteCount;
                    Debug.Log($"[SpriteSheetImporter] Imported {fileName}: {spriteCount} sprites");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SpriteSheetImporter] Failed to import {fileName}: {e.Message}");
                }
            }

            EditorUtility.DisplayProgressBar("Importing Sprite Sheets", "Building animations...", 0.95f);

            // 生成 AnimationClip（合并所有 sheet 的同角色子精灵）
            try
            {
                var animGroups = FindAnimationGroupsFromSheets();
                int animCount = 0;
                foreach (var kv in animGroups)
                {
                    BuildAnimationClip(kv.Key, kv.Value);
                    animCount++;
                }
                Debug.Log($"[SpriteSheetImporter] Built {animCount} animation clips");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SpriteSheetImporter] Animation build failed: {e.Message}");
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Import Sprite Sheets - Complete",
                $"Sprite Sheet 导入完成！\n\n" +
                $"处理 Sheet 数: {imported} / {total}\n" +
                $"生成子精灵总数: {totalSprites}\n\n" +
                "下一步：执行 Steel Rain > Build All 重新构建场景。", "OK");
        }

        /// <summary>
        /// 判断文件名是否为 Sprite Sheet。
        /// </summary>
        private static bool IsSheetFile(string fileName)
        {
            string lower = fileName.ToLower();
            return lower.EndsWith("_sheet.png") ||
                   lower.EndsWith("_sheet1.png") ||
                   lower.EndsWith("_sheet2.png");
        }

        /// <summary>
        /// 从 sheet 文件名提取角色/对象基础名。
        /// 例如 player_aila_sheet1.png → player_aila
        /// </summary>
        private static string GetBaseNameFromSheetName(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName).ToLower();
            if (name.EndsWith("_sheet1")) return name.Substring(0, name.Length - 7);
            if (name.EndsWith("_sheet2")) return name.Substring(0, name.Length - 7);
            if (name.EndsWith("_sheet")) return name.Substring(0, name.Length - 6);
            return name;
        }

        /// <summary>
        /// 导入单张 Sprite Sheet，自动切割成子精灵。
        /// </summary>
        private static int ImportSingleSheet(string sourceFile)
        {
            var fileName = Path.GetFileName(sourceFile);
            var targetPath = $"{TargetDir}/{fileName}";

            if (File.Exists(targetPath))
                File.Delete(targetPath);
            File.Copy(sourceFile, targetPath);

            var spec = GetSheetSpec(fileName);
            if (spec == null)
            {
                Debug.LogWarning($"[SpriteSheetImporter] Unknown sheet type: {fileName}");
                return 0;
            }

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(targetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[SpriteSheetImporter] Cannot get importer for {targetPath}");
                return 0;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = spec.PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;

            var spriteRects = GenerateSpriteRects(spec);
            importer.spritesheet = spriteRects;

            importer.SaveAndReimport();
            RenameSubSprites(targetPath, spec);

            return spec.TotalFrames;
        }

        /// <summary>
        /// 根据文件名获取 Sprite Sheet 规格。
        /// </summary>
        private static SheetSpec GetSheetSpec(string fileName)
        {
            string baseName = GetBaseNameFromSheetName(fileName);
            string lower = baseName.ToLower();
            string sheetSuffix = Path.GetFileNameWithoutExtension(fileName).ToLower();
            int sheetIndex = 0;
            if (sheetSuffix.EndsWith("_sheet1")) sheetIndex = 1;
            else if (sheetSuffix.EndsWith("_sheet2")) sheetIndex = 2;

            // 玩家角色 / 敌人：4×4 网格，256×256 单帧
            if (lower.StartsWith("player_") || lower.StartsWith("enemy_"))
            {
                return new SheetSpec
                {
                    BaseName = baseName,
                    SheetIndex = sheetIndex,
                    Cols = 4,
                    Rows = 4,
                    FrameWidth = 256,
                    FrameHeight = 256,
                    PixelsPerUnit = 32f,
                    Layout = SheetLayout.Character4x4
                };
            }

            // Boss：4×4 网格，512×512 单帧
            if (lower.StartsWith("miniboss_") || lower.StartsWith("turret_"))
            {
                return new SheetSpec
                {
                    BaseName = baseName,
                    SheetIndex = sheetIndex,
                    Cols = 4,
                    Rows = 4,
                    FrameWidth = 512,
                    FrameHeight = 512,
                    PixelsPerUnit = 64f,
                    Layout = SheetLayout.Boss4x4
                };
            }

            // 地形：1×8 横排
            if (lower.StartsWith("ground_"))
            {
                return new SheetSpec
                {
                    BaseName = baseName,
                    SheetIndex = sheetIndex,
                    Cols = 8,
                    Rows = 1,
                    FrameWidth = 512,
                    FrameHeight = 512,
                    PixelsPerUnit = 32f,
                    Layout = SheetLayout.Terrain1x8
                };
            }

            return null;
        }

        /// <summary>
        /// 生成切割矩形数组。
        /// </summary>
        private static SpriteMetaData[] GenerateSpriteRects(SheetSpec spec)
        {
            var rects = new List<SpriteMetaData>();
            var names = GetSubSpriteNames(spec);

            for (int row = 0; row < spec.Rows; row++)
            {
                for (int col = 0; col < spec.Cols; col++)
                {
                    int index = row * spec.Cols + col;
                    if (index >= names.Length) break;

                    float x = col * spec.FrameWidth;
                    float y = (spec.Rows - 1 - row) * spec.FrameHeight;

                    rects.Add(new SpriteMetaData
                    {
                        name = names[index],
                        rect = new Rect(x, y, spec.FrameWidth, spec.FrameHeight),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f),
                        border = new Vector4(0, 0, 0, 0)
                    });
                }
            }

            return rects.ToArray();
        }

        /// <summary>
        /// 根据规格生成子精灵命名列表（按网格顺序：左到右、上到下）。
        /// 双 Sheet 角色：Sheet1 移动动作，Sheet2 战斗/技能动作。
        /// </summary>
        private static string[] GetSubSpriteNames(SheetSpec spec)
        {
            var names = new List<string>();
            string baseName = spec.BaseName;

            if (spec.Layout == SheetLayout.Character4x4 || spec.Layout == SheetLayout.Boss4x4)
            {
                if (spec.SheetIndex == 2)
                {
                    // Sheet2 - 战斗/技能/武器
                    // Row 0: crouch_0, crouch_1, crouch_2, prone_0
                    // Row 1: shoot_0, shoot_1, shoot_2, shoot_3
                    // Row 2: death_0, death_1, death_2, death_3
                    // Row 3: skill_0, pistol_0, knife_0, _unused
                    string[] actions = {
                        "crouch_0", "crouch_1", "crouch_2", "prone_0",
                        "shoot_0", "shoot_1", "shoot_2", "shoot_3",
                        "death_0", "death_1", "death_2", "death_3",
                        "skill_0", "pistol_0", "knife_0", "_unused"
                    };
                    foreach (var act in actions)
                        names.Add($"{baseName}_{act}");
                }
                else
                {
                    // Sheet1（默认）- 移动动作
                    // Row 0: idle_0, idle_1, walk_0, walk_1
                    // Row 1: walk_2, walk_3, run_0, run_1
                    // Row 2: run_2, run_3, jump_0, jump_1
                    // Row 3: jump_2, jump_3, dash_0, _unused
                    string[] actions = {
                        "idle_0", "idle_1", "walk_0", "walk_1",
                        "walk_2", "walk_3", "run_0", "run_1",
                        "run_2", "run_3", "jump_0", "jump_1",
                        "jump_2", "jump_3", "dash_0", "_unused"
                    };
                    foreach (var act in actions)
                        names.Add($"{baseName}_{act}");
                }
            }
            else if (spec.Layout == SheetLayout.Terrain1x8)
            {
                string[] variants = { "", "_left", "_right", "_mid", "_single", "_corner_left", "_corner_right", "_slope" };
                foreach (var v in variants)
                    names.Add($"{baseName}{v}");
            }

            return names.ToArray();
        }

        /// <summary>
        /// 重命名子精灵为规范命名。
        /// </summary>
        private static void RenameSubSprites(string assetPath, SheetSpec spec)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            var sprites = assets.OfType<Sprite>().ToList();
            var expectedNames = GetSubSpriteNames(spec);

            if (sprites.Count != expectedNames.Length)
            {
                Debug.LogWarning($"[SpriteSheetImporter] Sprite count mismatch: {sprites.Count} vs {expectedNames.Length} for {assetPath}");
                return;
            }

            sprites.Sort((a, b) =>
            {
                int yCompare = b.rect.y.CompareTo(a.rect.y);
                if (yCompare != 0) return yCompare;
                return a.rect.x.CompareTo(b.rect.x);
            });

            for (int i = 0; i < sprites.Count; i++)
            {
                if (i < expectedNames.Length)
                {
                    sprites[i].name = expectedNames[i];
                    EditorUtility.SetDirty(sprites[i]);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        /// <summary>
        /// 扫描所有 sheet 文件，按角色/动作名合并子精灵为动画组。
        /// 支持双 sheet：player_aila_sheet1 和 player_aila_sheet2 的子精灵会合并。
        /// </summary>
        private static Dictionary<string, List<string>> FindAnimationGroupsFromSheets()
        {
            var groups = new Dictionary<string, List<string>>();
            if (!Directory.Exists(TargetDir)) return groups;

            var sheetFiles = Directory.GetFiles(TargetDir, "*.png")
                .Where(f => IsSheetFile(Path.GetFileName(f)))
                .ToArray();

            var regex = new System.Text.RegularExpressions.Regex(@"^(.+?)_(\d+)$");

            foreach (var sheetFile in sheetFiles)
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(sheetFile);
                foreach (var asset in assets)
                {
                    if (!(asset is Sprite sprite)) continue;

                    string name = sprite.name;
                    if (name.EndsWith("_unused")) continue;

                    var match = regex.Match(name);
                    if (!match.Success) continue;

                    string animName = match.Groups[1].Value;
                    int frameIndex = int.Parse(match.Groups[2].Value);

                    if (!groups.ContainsKey(animName))
                        groups[animName] = new List<string>();

                    string spriteRef = $"{sheetFile}|{sprite.name}|{frameIndex:D4}";
                    groups[animName].Add(spriteRef);
                }
            }

            var sorted = new Dictionary<string, List<string>>();
            foreach (var kv in groups)
            {
                var sortedList = kv.Value
                    .OrderBy(s => s.Split('|')[2])
                    .ToList();
                sorted[kv.Key] = sortedList;
            }

            return sorted;
        }

        /// <summary>
        /// 为一个动画组生成 AnimationClip。
        /// </summary>
        private static void BuildAnimationClip(string animName, List<string> spriteRefs)
        {
            if (spriteRefs.Count == 0) return;

            string charName, actionName;
            ParseAnimName(animName, out charName, out actionName);

            string charDir = $"{AnimDir}/{charName}";
            if (!Directory.Exists(charDir))
                Directory.CreateDirectory(charDir);

            string clipPath = $"{charDir}/{actionName}.anim";

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip { name = actionName };
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = IsLoopingAction(actionName);
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var bindings = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            var keyframes = new ObjectReferenceKeyframe[spriteRefs.Count];
            float frameRate = 12f;
            float frameDuration = 1f / frameRate;

            for (int i = 0; i < spriteRefs.Count; i++)
            {
                var parts = spriteRefs[i].Split('|');
                string sheetPath = parts[0].Replace('\\', '/');
                string spriteName = parts[1];

                var targetSprite = AssetDatabase.LoadAllAssetsAtPath(sheetPath)
                    .OfType<Sprite>()
                    .FirstOrDefault(s => s.name == spriteName);

                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * frameDuration,
                    value = targetSprite
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, bindings, keyframes);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();

            Debug.Log($"[SpriteSheetImporter] Built animation: {charName}/{actionName} ({spriteRefs.Count} frames)");
        }

        private static void ParseAnimName(string animName, out string charName, out string actionName)
        {
            string[] actions = { "idle", "walk", "run", "jump", "dash", "crouch", "prone", "shoot", "death", "skill", "pistol", "knife" };
            charName = animName;
            actionName = "idle";

            foreach (var act in actions)
            {
                if (animName.EndsWith("_" + act))
                {
                    actionName = act;
                    charName = animName.Substring(0, animName.Length - act.Length - 1);
                    return;
                }
            }
        }

        private static bool IsLoopingAction(string actionName)
        {
            return actionName == "idle" || actionName == "walk" || actionName == "run" || actionName == "crouch";
        }

        /// <summary>
        /// 兼容性辅助：根据单张图片名（如 player_aila.png）从 Sprite Sheet 中加载对应子精灵。
        /// 优先从 Sheet1 加载 idle_0，找不到则从 Sheet2 查找任意有效帧。
        /// </summary>
        public static Sprite LoadSpriteCompat(string singleImagePath)
        {
            if (File.Exists(singleImagePath))
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(singleImagePath);
                if (assets != null)
                {
                    foreach (var a in assets)
                    {
                        if (a is Sprite s) return s;
                    }
                }
            }

            string fileName = Path.GetFileNameWithoutExtension(singleImagePath);
            string dir = Path.GetDirectoryName(singleImagePath);

            // 优先查找 Sheet1 的 idle_0
            string sheet1Path = $"{dir}/{fileName}_sheet1.png".Replace('\\', '/');
            if (File.Exists(sheet1Path))
            {
                var sheetAssets = AssetDatabase.LoadAllAssetsAtPath(sheet1Path);
                if (sheetAssets != null)
                {
                    foreach (var a in sheetAssets)
                    {
                        if (a is Sprite s && s.name == $"{fileName}_idle_0") return s;
                    }

                    foreach (var a in sheetAssets)
                    {
                        if (a is Sprite s && s.name.StartsWith(fileName) && !s.name.EndsWith("_unused")) return s;
                    }
                }
            }

            // 回退查找 Sheet2
            string sheet2Path = $"{dir}/{fileName}_sheet2.png".Replace('\\', '/');
            if (File.Exists(sheet2Path))
            {
                var sheetAssets = AssetDatabase.LoadAllAssetsAtPath(sheet2Path);
                if (sheetAssets != null)
                {
                    foreach (var a in sheetAssets)
                    {
                        if (a is Sprite s && s.name.StartsWith(fileName) && !s.name.EndsWith("_unused")) return s;
                    }
                }
            }

            // 向后兼容旧版 _sheet.png
            string sheetPath = $"{dir}/{fileName}_sheet.png".Replace('\\', '/');
            if (File.Exists(sheetPath))
            {
                var sheetAssets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
                if (sheetAssets != null)
                {
                    foreach (var a in sheetAssets)
                    {
                        if (a is Sprite s && s.name == $"{fileName}_idle_0") return s;
                    }

                    foreach (var a in sheetAssets)
                    {
                        if (a is Sprite s && s.name.StartsWith(fileName) && !s.name.EndsWith("_unused")) return s;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 检查 Sprite Sheet 导入状态。
        /// </summary>
        [MenuItem("Steel Rain/Check Sprite Sheet Status")]
        public static void CheckStatus()
        {
            if (!Directory.Exists(SourceDir))
            {
                EditorUtility.DisplayDialog("Sprite Sheet Status",
                    $"源目录不存在：{SourceDir}", "OK");
                return;
            }

            var sheetFiles = Directory.GetFiles(SourceDir, "*.png")
                .Where(f => IsSheetFile(Path.GetFileName(f)))
                .Where(f => !Path.GetFileName(f).StartsWith("_"))
                .ToArray();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Sprite Sheet 导入状态 ===\n");
            sb.AppendLine($"源目录: {SourceDir}");
            sb.AppendLine($"目标目录: {TargetDir}\n");
            sb.AppendLine($"待导入 Sheet 数: {sheetFiles.Length}");
            sb.AppendLine();

            if (sheetFiles.Length > 0)
            {
                sb.AppendLine("待导入文件：");
                foreach (var f in sheetFiles)
                {
                    string name = Path.GetFileName(f);
                    var spec = GetSheetSpec(name);
                    if (spec != null)
                    {
                        sb.AppendLine($"  {name} → {spec.Cols}x{spec.Rows} = {spec.TotalFrames} sprites");
                    }
                    else
                    {
                        sb.AppendLine($"  {name} → 未知类型");
                    }
                }
            }

            if (Directory.Exists(TargetDir))
            {
                var importedSheets = Directory.GetFiles(TargetDir, "*.png")
                    .Where(f => IsSheetFile(Path.GetFileName(f)))
                    .ToArray();
                sb.AppendLine($"\n已导入 Sheet 数: {importedSheets.Length}");

                int totalSprites = 0;
                foreach (var f in importedSheets)
                {
                    var assets = AssetDatabase.LoadAllAssetsAtPath(f);
                    int spriteCount = assets.Count(a => a is Sprite);
                    totalSprites += spriteCount;
                }
                sb.AppendLine($"已生成子精灵总数: {totalSprites}");
            }

            EditorUtility.DisplayDialog("Sprite Sheet Status", sb.ToString(), "OK");
            Debug.Log(sb.ToString());
        }

        #region 内部类型

        private enum SheetLayout
        {
            Character4x4,
            Boss4x4,
            Terrain1x8
        }

        private class SheetSpec
        {
            public string BaseName;
            public int SheetIndex;
            public int Cols;
            public int Rows;
            public int FrameWidth;
            public int FrameHeight;
            public float PixelsPerUnit;
            public SheetLayout Layout;

            public int TotalFrames => Cols * Rows;
        }

        #endregion
    }
}
#endif
