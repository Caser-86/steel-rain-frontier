#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace SteelRain.Editor
{
    /// <summary>
    /// AI 生成美术资源自动导入器（v2 多帧动画版）。
    /// 读取 Assets/Art/AI_Generated/ 下的图片，自动：
    /// 1. 导入并配置为精灵
    /// 2. 识别多帧动画（player_aila_walk_0.png ~ _5.png）
    /// 3. 生成 AnimationClip 和 AnimatorController
    /// 4. 替换占位精灵
    /// </summary>
    public static class AIArtImporter
    {
        private const string SourceDir = "Assets/Art/AI_Generated";
        private const string TargetDir = "Assets/Art/Generated";
        private const string AnimDir = "Assets/Art/Animations";
        private const string ControllerDir = "Assets/Art/Animators";

        [MenuItem("Steel Rain/Import AI Art")]
        public static void ImportAll()
        {
            if (!Directory.Exists(SourceDir))
            {
                EditorUtility.DisplayDialog("Import AI Art",
                    $"未找到 AI 生成资源目录：\n{SourceDir}\n\n" +
                    "请先下载 AI 生成的图片放到该目录。", "OK");
                return;
            }

            if (!Directory.Exists(TargetDir))
                Directory.CreateDirectory(TargetDir);
            if (!Directory.Exists(AnimDir))
                Directory.CreateDirectory(AnimDir);
            if (!Directory.Exists(ControllerDir))
                Directory.CreateDirectory(ControllerDir);

            var files = Directory.GetFiles(SourceDir, "*.png");
            if (files.Length == 0)
            {
                EditorUtility.DisplayDialog("Import AI Art",
                    $"目录 {SourceDir} 中没有 PNG 图片。\n\n" +
                    "请先用 AI 工具生成图片并下载到此目录。\n" +
                    "提示词清单见：Assets/Art/AI_Generated/_PROMPTS.md", "OK");
                return;
            }

            int total = files.Length;
            int imported = 0;
            int skipped = 0;

            // 第一步：导入所有图片
            for (int i = 0; i < total; i++)
            {
                var file = files[i];
                var fileName = Path.GetFileName(file);

                if (fileName.StartsWith("_")) { skipped++; continue; }

                EditorUtility.DisplayProgressBar("Importing AI Art",
                    $"Importing {fileName}...", (float)i / total);

                try
                {
                    ImportSingleSprite(file);
                    imported++;
                    Debug.Log($"[AIArtImporter] Imported {fileName}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[AIArtImporter] Failed to import {fileName}: {e.Message}");
                }
            }

            EditorUtility.DisplayProgressBar("Importing AI Art", "Building animations...", 0.95f);

            // 第二步：识别多帧动画并生成 AnimationClip
            try
            {
                var animGroups = FindAnimationGroups();
                int animCount = 0;
                foreach (var kv in animGroups)
                {
                    BuildAnimationClip(kv.Key, kv.Value);
                    animCount++;
                }
                Debug.Log($"[AIArtImporter] Built {animCount} animation clips");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[AIArtImporter] Animation build failed: {e.Message}");
            }

            EditorUtility.ClearProgressBar();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Import AI Art - Complete",
                $"导入完成！\n\n" +
                $"成功导入: {imported} 个精灵\n" +
                $"跳过: {skipped} 个\n" +
                $"失败: {total - imported - skipped} 个\n\n" +
                "下一步：执行 Steel Rain > Build All 重新构建场景。", "OK");

            Debug.Log($"[AIArtImporter] Import complete. Imported: {imported}, Skipped: {skipped}, Failed: {total - imported - skipped}");
        }

        private static void ImportSingleSprite(string sourceFile)
        {
            var fileName = Path.GetFileName(sourceFile);
            var targetPath = $"{TargetDir}/{fileName}";

            if (File.Exists(targetPath))
                File.Delete(targetPath);
            File.Copy(sourceFile, targetPath);

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(targetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[AIArtImporter] Cannot get importer for {targetPath}");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            // 根据分辨率调整 PixelsPerUnit
            importer.spritePixelsPerUnit = GetPixelsPerUnit(fileName);
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            Debug.Log($"[AIArtImporter] Configured sprite: {fileName} (PPU={GetPixelsPerUnit(fileName)})");
        }

        /// <summary>
        /// 根据文件名前缀决定 PixelsPerUnit，保证不同分辨率资源在场景中显示尺寸一致。
        /// </summary>
        private static float GetPixelsPerUnit(string fileName)
        {
            string lower = fileName.ToLower();
            if (lower.StartsWith("miniboss") || lower.StartsWith("turret_boss"))
                return 64f;   // Boss 2048×2048，显示更大
            if (lower.StartsWith("player_") || lower.StartsWith("enemy_"))
                return 32f;   // 角色 1024×1024
            if (lower.StartsWith("ground_"))
                return 32f;   // 地形 512×512
            if (lower.StartsWith("bg_"))
                return 100f;  // 背景 4096×2048
            if (lower.StartsWith("ui_"))
                return 32f;
            return 32f;
        }

        /// <summary>
        /// 扫描 Generated 目录，识别多帧动画组。
        /// 命名规范：player_aila_walk_0.png, player_aila_walk_1.png, ...
        /// 返回：Dictionary<动画名, 排序后的精灵路径列表>
        /// 动画名格式：player_aila_walk
        /// </summary>
        private static Dictionary<string, List<string>> FindAnimationGroups()
        {
            var groups = new Dictionary<string, List<string>>();
            if (!Directory.Exists(TargetDir)) return groups;

            // 匹配末尾的 _数字
            var regex = new Regex(@"^(.+)_(\d+)$");
            var files = Directory.GetFiles(TargetDir, "*.png");

            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var match = regex.Match(name);
                if (!match.Success) continue;

                string animName = match.Groups[1].Value;
                int frameIndex = int.Parse(match.Groups[2].Value);

                if (!groups.ContainsKey(animName))
                    groups[animName] = new List<string>();

                // 用 frameIndex 作为排序键
                groups[animName].Add(frameIndex.ToString("D4") + "|" + file);
            }

            // 排序每组
            var sorted = new Dictionary<string, List<string>>();
            foreach (var kv in groups)
            {
                var sortedFiles = kv.Value
                    .OrderBy(s => s)
                    .Select(s => s.Split('|')[1])
                    .ToList();
                sorted[kv.Key] = sortedFiles;
            }

            return sorted;
        }

        /// <summary>
        /// 为一个动画组生成 AnimationClip。
        /// </summary>
        private static void BuildAnimationClip(string animName, List<string> framePaths)
        {
            if (framePaths.Count == 0) return;

            // 推断角色/对象名和动作名
            // 例如 player_aila_walk → 角色 player_aila，动作 walk
            string charName, actionName;
            ParseAnimName(animName, out charName, out actionName);

            // 创建角色动画子目录
            string charDir = $"{AnimDir}/{charName}";
            if (!Directory.Exists(charDir))
                Directory.CreateDirectory(charDir);

            string clipPath = $"{charDir}/{actionName}.anim";

            // 创建或加载 AnimationClip
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip { name = actionName };
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            // 设置循环
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            bool loop = IsLoopingAction(actionName);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            // 添加关键帧（sprite swap）
            var bindings = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            var keyframes = new ObjectReferenceKeyframe[framePaths.Count];
            float frameRate = 12f; // 12 FPS
            float frameDuration = 1f / frameRate;

            for (int i = 0; i < framePaths.Count; i++)
            {
                // 转换为 Unity 资产路径（正斜杠，相对路径）
                var assetPath = framePaths[i].Replace('\\', '/');
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * frameDuration,
                    value = sprite
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, bindings, keyframes);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();

            Debug.Log($"[AIArtImporter] Built animation: {charName}/{actionName} ({framePaths.Count} frames, loop={loop})");
        }

        /// <summary>
        /// 解析动画名，分离角色名和动作名。
        /// player_aila_walk → ("player_aila", "walk")
        /// enemy_rifle_idle → ("enemy_rifle", "idle")
        /// miniboss_walker_walk → ("miniboss_walker", "walk")
        /// </summary>
        private static void ParseAnimName(string animName, out string charName, out string actionName)
        {
            // 已知动作列表
            string[] actions = { "idle", "walk", "run", "jump", "crouch", "shoot", "death" };

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
            return actionName == "idle" || actionName == "walk" || actionName == "run";
        }

        /// <summary>
        /// 检查 AI 生成目录中有哪些文件，对照清单显示缺失项。
        /// </summary>
        [MenuItem("Steel Rain/Check AI Art Status")]
        public static void CheckStatus()
        {
            var required = GetRequiredAssetList();
            var existing = new HashSet<string>();

            if (Directory.Exists(SourceDir))
            {
                foreach (var f in Directory.GetFiles(SourceDir, "*.png"))
                    existing.Add(Path.GetFileName(f).ToLower());
            }

            int found = 0;
            var missing = new List<string>();
            var categories = new Dictionary<string, int>();
            var categoriesMissing = new Dictionary<string, int>();

            foreach (var r in required)
            {
                var category = GetCategory(r);
                if (!categories.ContainsKey(category))
                {
                    categories[category] = 0;
                    categoriesMissing[category] = 0;
                }
                categories[category]++;

                if (existing.Contains(r.ToLower()))
                    found++;
                else
                {
                    missing.Add(r);
                    categoriesMissing[category]++;
                }
            }

            var report = new System.Text.StringBuilder();
            report.AppendLine("资源清单状态（v2 多帧动画版）：");
            report.AppendLine();
            report.AppendLine($"已就绪: {found} / {required.Count}");
            report.AppendLine($"缺失: {missing.Count}");
            report.AppendLine();
            report.AppendLine("分类统计：");

            foreach (var kv in categories)
            {
                var ready = kv.Value - categoriesMissing[kv.Key];
                report.Append($"  {kv.Key}: {ready}/{kv.Value}");
                if (categoriesMissing[kv.Key] > 0)
                    report.Append($" (缺 {categoriesMissing[kv.Key]})");
                report.AppendLine();
            }

            // 进度百分比
            float percent = required.Count > 0 ? (float)found / required.Count * 100f : 0f;
            report.AppendLine();
            report.AppendLine($"完成度: {percent:F1}%");

            if (missing.Count > 0 && missing.Count <= 50)
            {
                report.AppendLine();
                report.AppendLine("缺失文件:");
                foreach (var m in missing)
                    report.AppendLine("  " + m);
            }
            else if (missing.Count > 50)
            {
                report.AppendLine();
                report.AppendLine("缺失文件（前 50 个）:");
                foreach (var m in missing.GetRange(0, 50))
                    report.AppendLine("  " + m);
                report.AppendLine($"  ... 还有 {missing.Count - 50} 个");
            }

            EditorUtility.DisplayDialog("AI Art Status", report.ToString(), "OK");
            Debug.Log("[AIArtImporter]\n" + report.ToString());
        }

        private static string GetCategory(string fileName)
        {
            if (fileName.StartsWith("player_")) return "玩家角色";
            if (fileName.StartsWith("enemy_")) return "敌人";
            if (fileName.StartsWith("miniboss") || fileName.StartsWith("turret_boss")) return "Boss";
            if (fileName.StartsWith("ground_")) return "地形";
            if (fileName.StartsWith("bg_")) return "背景";
            if (fileName.StartsWith("ui_")) return "UI 图标";
            return "道具/特效";
        }

        /// <summary>
        /// 完整资源清单（v2.2，约 420 张）。
        /// v2.1 新增补充资源：子弹/旧命名背景/视差层/UI/特效/三角形，共 35 张。
        /// v2.2 新增单张默认帧：角色/敌人/Boss 的回退精灵，共 12 张。
        /// 这些资源在项目代码中被直接引用，必须生成。
        /// </summary>
        private static List<string> GetRequiredAssetList()
        {
            var list = new List<string>();

            // ===== 玩家角色（4角色 × 29帧 = 116张）=====
            // Aila
            list.AddRange(ExpandAnim("player_aila_idle", 2));
            list.AddRange(ExpandAnim("player_aila_walk", 6));
            list.AddRange(ExpandAnim("player_aila_run", 6));
            list.AddRange(ExpandAnim("player_aila_jump", 4));
            list.AddRange(ExpandAnim("player_aila_crouch", 3));
            list.AddRange(ExpandAnim("player_aila_shoot", 4));
            list.AddRange(ExpandAnim("player_aila_death", 4));

            // Bruno
            list.AddRange(ExpandAnim("player_bruno_idle", 2));
            list.AddRange(ExpandAnim("player_bruno_walk", 6));
            list.AddRange(ExpandAnim("player_bruno_run", 6));
            list.AddRange(ExpandAnim("player_bruno_jump", 4));
            list.AddRange(ExpandAnim("player_bruno_crouch", 3));
            list.AddRange(ExpandAnim("player_bruno_shoot", 4));
            list.AddRange(ExpandAnim("player_bruno_death", 4));

            // Mara
            list.AddRange(ExpandAnim("player_mara_idle", 2));
            list.AddRange(ExpandAnim("player_mara_walk", 6));
            list.AddRange(ExpandAnim("player_mara_run", 6));
            list.AddRange(ExpandAnim("player_mara_jump", 4));
            list.AddRange(ExpandAnim("player_mara_crouch", 3));
            list.AddRange(ExpandAnim("player_mara_shoot", 4));
            list.AddRange(ExpandAnim("player_mara_death", 4));

            // Niko
            list.AddRange(ExpandAnim("player_niko_idle", 2));
            list.AddRange(ExpandAnim("player_niko_walk", 6));
            list.AddRange(ExpandAnim("player_niko_run", 6));
            list.AddRange(ExpandAnim("player_niko_jump", 4));
            list.AddRange(ExpandAnim("player_niko_crouch", 3));
            list.AddRange(ExpandAnim("player_niko_shoot", 4));
            list.AddRange(ExpandAnim("player_niko_death", 4));

            // ===== 敌人（6敌人 × 多帧 = 约 78张）=====
            // 步枪兵
            list.AddRange(ExpandAnim("enemy_rifle_idle", 2));
            list.AddRange(ExpandAnim("enemy_rifle_walk", 4));
            list.AddRange(ExpandAnim("enemy_rifle_shoot", 4));
            list.AddRange(ExpandAnim("enemy_rifle_death", 4));

            // 盾牌兵
            list.AddRange(ExpandAnim("enemy_shield_idle", 2));
            list.AddRange(ExpandAnim("enemy_shield_walk", 3));
            list.AddRange(ExpandAnim("enemy_shield_shoot", 3));
            list.AddRange(ExpandAnim("enemy_shield_death", 4));

            // 无人机
            list.AddRange(ExpandAnim("enemy_drone_idle", 2));
            list.AddRange(ExpandAnim("enemy_drone_walk", 4));
            list.AddRange(ExpandAnim("enemy_drone_shoot", 4));
            list.AddRange(ExpandAnim("enemy_drone_death", 4));

            // 掷弹兵
            list.AddRange(ExpandAnim("enemy_grenadier_idle", 2));
            list.AddRange(ExpandAnim("enemy_grenadier_walk", 3));
            list.AddRange(ExpandAnim("enemy_grenadier_shoot", 3));
            list.AddRange(ExpandAnim("enemy_grenadier_death", 4));

            // 火焰兵（新增）
            list.AddRange(ExpandAnim("enemy_flamer_idle", 2));
            list.AddRange(ExpandAnim("enemy_flamer_walk", 3));
            list.AddRange(ExpandAnim("enemy_flamer_shoot", 3));
            list.AddRange(ExpandAnim("enemy_flamer_death", 3));

            // ===== Boss（2Boss × 多帧 = 约 27张）=====
            // 行走Boss
            list.AddRange(ExpandAnim("miniboss_walker_idle", 2));
            list.AddRange(ExpandAnim("miniboss_walker_walk", 4));
            list.AddRange(ExpandAnim("miniboss_walker_shoot", 4));
            list.AddRange(ExpandAnim("miniboss_walker_death", 4));

            // 炮台Boss
            list.AddRange(ExpandAnim("turret_boss_idle", 2));
            list.AddRange(ExpandAnim("turret_boss_walk", 3));
            list.AddRange(ExpandAnim("turret_boss_shoot", 3));
            list.AddRange(ExpandAnim("turret_boss_death", 3));

            // ===== 地形（7地形 × 8变种 = 56张）=====
            string[] terrains = { "beach", "village", "trench", "factory", "city", "industrial", "snow" };
            string[] variants = { "", "_left", "_right", "_mid", "_single", "_corner_left", "_corner_right", "_slope" };
            foreach (var t in terrains)
            {
                foreach (var v in variants)
                {
                    list.Add($"ground_{t}{v}.png");
                }
            }

            // ===== 道具（20张）=====
            list.AddRange(new[] {
                "crate.png", "weapon_pickup.png", "checkpoint_flag.png",
                "health_pickup.png", "explosive_barrel.png",
                "sandbag.png", "barricade.png", "spikes.png", "signpost.png",
                "ammo_box.png", "energy_core.png", "keycard.png", "landmine.png",
                "fuel_barrel.png", "binoculars.png", "radio.png",
                "loot_chest.png", "radar.png", "flag.png"
            });

            // ===== 背景（12张）=====
            list.AddRange(new[] {
                "bg_beach.png", "bg_beach_mid.png", "bg_beach_fg.png",
                "bg_factory.png", "bg_factory_mid.png",
                "bg_city.png", "bg_city_mid.png",
                "bg_snow.png", "bg_snow_mid.png",
                "bg_trench.png", "bg_trench_mid.png",
                "bg_night_sky.png"
            });

            // ===== 特效（15张）=====
            list.AddRange(new[] {
                "explosion.png", "muzzle_flash.png", "spark.png", "smoke.png",
                "blood_splash.png", "shell_casing.png", "hit_effect.png",
                "shield_effect.png", "fire_effect.png", "ice_effect.png",
                "lightning_effect.png", "smoke_grenade_effect.png",
                "heal_effect.png", "upgrade_effect.png", "bullet_trail.png"
            });

            // ===== UI 图标（15张）=====
            list.AddRange(new[] {
                "ui_health.png", "ui_ammo.png", "ui_score.png", "ui_pause.png",
                "ui_play.png", "ui_settings.png", "ui_back.png", "ui_achievement.png",
                "ui_avatar_aila.png", "ui_avatar_bruno.png",
                "ui_avatar_mara.png", "ui_avatar_niko.png",
                "ui_level_select.png", "ui_mission.png",
                "ui_lock.png", "ui_unlock.png"
            });

            // ===== 补充资源（v2.1，代码引用必需，35张）=====
            // 子弹/弹药（代码直接引用）
            list.AddRange(new[] {
                "bullet_player.png", "bullet_enemy.png", "bullet_shell.png"
            });

            // 旧命名背景（VerticalSliceBuilder 引用）
            list.AddRange(new[] {
                "background_sky.png", "background_sea.png", "background_cloud.png",
                "background_mountain.png", "background_factory.png",
                "background_city.png", "background_industrial.png"
            });

            // 视差层（VerticalSliceBuilder 引用）
            list.AddRange(new[] {
                "parallax_far_mountain.png", "parallax_mid_hill.png",
                "parallax_near_tree.png", "parallax_foreground_grass.png"
            });

            // UI 资源（EnhancedArtGenerator 命名）
            list.AddRange(new[] {
                "ui_heart.png", "ui_energy.png", "ui_weapon.png",
                "ui_arrow.png", "ui_frame.png"
            });

            // 特效（EnhancedArtGenerator 命名）
            list.AddRange(new[] {
                "hit_spark.png", "footprint.png", "ring_effect.png",
                "dash_trail.png", "pickup_glow.png"
            });

            // 其他（VerticalSliceBuilder 引用）
            list.AddRange(new[] {
                "triangle.png", "player_aila.png"
            });

            // 单张默认帧（v2.2，代码直接引用的回退精灵，12张）
            // 这些是 EnhancedArtGenerator 生成、VerticalSliceBuilder 引用的单张默认帧
            // 可复制对应的 idle_0 帧作为别名
            list.AddRange(new[] {
                "player_aila.png", "player_bruno.png",
                "player_mara.png", "player_niko.png",
                "enemy_rifle.png", "enemy_shield.png",
                "enemy_drone.png", "enemy_grenadier.png",
                "enemy_flamer.png",
                "miniboss_walker.png", "turret_boss.png"
            });

            return list;
        }

        /// <summary>
        /// 展开动画帧文件名。
        /// 例如 ExpandAnim("player_aila_walk", 6) 返回：
        /// player_aila_walk_0.png ~ player_aila_walk_5.png
        /// </summary>
        private static IEnumerable<string> ExpandAnim(string baseName, int frameCount)
        {
            for (int i = 0; i < frameCount; i++)
            {
                yield return $"{baseName}_{i}.png";
            }
        }
    }
}
#endif
