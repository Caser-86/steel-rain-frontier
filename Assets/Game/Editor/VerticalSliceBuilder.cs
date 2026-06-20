#if UNITY_EDITOR
using System;
using System.IO;
using System.Text.RegularExpressions;
using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Editor;
using SteelRain.Enemies;
using SteelRain.Game;
using SteelRain.Levels;
using SteelRain.Pickups;
using SteelRain.Player;
using SteelRain.UI;
using SteelRain.VFX;
using SteelRain.Weapons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SteelRain.EditorTools
{
    /// <summary>
    /// 一键生成全部游戏资产和可玩场景。
    /// 用法：Unity 批处理 -executeMethod SteelRain.EditorTools.VerticalSliceBuilder.BuildAll
    /// </summary>
    public static class VerticalSliceBuilder
    {
        private const string ArtDir = "Assets/Art/Generated";
        private const string PrefabDir = "Assets/Prefabs";
        private const string DataDir = "Assets/Game/Data";
        private const string ScenePath = "Assets/Scenes/Level01_VerticalSlice.unity";

        // ===== 公开入口 =====

        [MenuItem("Steel Rain/Build All")]
        public static void BuildAll()
        {
            Debug.Log("[VerticalSliceBuilder] Starting full build...");
            EnsureDirectories();
            // 强制Unity刷新资源（首次运行或外部添加的png文件需要）
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            GenerateSprites();
            AudioGenerator.GenerateAll();
            MusicGenerator.GenerateAll();
            var characters = CreateAllCharacterData();
            var ailaAsset = characters[0];
            var weapons = CreateAllWeaponData();
            var rifleWeapon = weapons[0];
            var enemyDefs = CreateEnemyData();
            var projectilePrefab = CreateProjectilePrefab();
            var playerPrefab = CreatePlayerPrefab(ailaAsset, rifleWeapon, projectilePrefab, characters);
            var enemyPrefab = CreateEnemyPrefab(projectilePrefab);
            var shieldPrefab = CreateShieldEnemyPrefab(projectilePrefab);
            var dronePrefab = CreateDronePrefab(projectilePrefab);
            var grenadierPrefab = CreateGrenadierPrefab(projectilePrefab);
            var chargerPrefab = CreateChargerPrefab(projectilePrefab);
            var sniperPrefab = CreateSniperPrefab(projectilePrefab);
            var heavyGunnerPrefab = CreateHeavyGunnerPrefab(projectilePrefab);
            CreateMiniBossPrefab();
            CreateTurretBossPrefab();
            var upgradePickupPrefab = CreateUpgradePickupPrefab();
            var healthPickupPrefab = CreateHealthPickupPrefab();
            var cratePrefab = CreateCratePrefab(healthPickupPrefab);
            BuildLevel01Scene(playerPrefab, enemyPrefab, enemyDefs, projectilePrefab,
                upgradePickupPrefab, cratePrefab, healthPickupPrefab, characters,
                shieldPrefab, dronePrefab, grenadierPrefab, weapons,
                chargerPrefab, sniperPrefab, heavyGunnerPrefab);
            BuildLevel02Scene(playerPrefab, enemyPrefab, enemyDefs, projectilePrefab,
                upgradePickupPrefab, cratePrefab, healthPickupPrefab, characters,
                shieldPrefab, dronePrefab, grenadierPrefab, weapons,
                chargerPrefab, sniperPrefab, heavyGunnerPrefab);
            CreateBootScene();
            CreateMainMenuScene();
            RegisterBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[VerticalSliceBuilder] Build complete!");
        }

        [MenuItem("Steel Rain/Diagnose")]

        /// <summary>
        /// 强制重新导入所有PNG让Unity生成sprite子资源
        /// </summary>
        public static void ReimportAllPNGs()
        {
            Debug.Log("[Reimport] Starting...");
            var artDir = "Assets/Art/Generated";
            if (!System.IO.Directory.Exists(artDir))
            {
                Debug.LogError($"[Reimport] {artDir} does not exist");
                return;
            }
            var files = System.IO.Directory.GetFiles(artDir, "*.png");
            foreach (var f in files)
            {
                var path = f.Replace('\\', '/');
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log($"[Reimport] {files.Length} files reimported");
        }

        /// <summary>
        /// 为所有PNG添加Sprite子资源到Asset Database (解决批处理模式下sprite未生成问题)
        /// </summary>
        public static void InjectSpriteSubAssets()
        {
            Debug.Log("[InjectSprite] Starting...");
            var artDir = "Assets/Art/Generated";
            var files = System.IO.Directory.GetFiles(artDir, "*.png");
            int injected = 0;
            foreach (var f in files)
            {
                var path = f.Replace('\\', '/');
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) { Debug.LogWarning($"[InjectSprite] no tex: {path}"); continue; }

                // 检查是否已有Sprite子资源
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                bool hasSprite = false;
                foreach (var a in allAssets)
                {
                    if (a is Sprite) { hasSprite = true; break; }
                }
                if (hasSprite) continue;

                // 创建一个Sprite并添加到Asset
                var sprite = Sprite.Create(tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    16f);
                sprite.name = System.IO.Path.GetFileNameWithoutExtension(path);
                AssetDatabase.AddObjectToAsset(sprite, tex);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                Debug.Log($"[InjectSprite] injected sprite for {path}");
                injected++;
            }
            Debug.Log($"[InjectSprite] {injected} sprites injected");
        }
        public static void Diagnose()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var player = GameObject.Find("Player_Aila");
            if (player != null)
            {
                Debug.Log($"[Diagnose] Player components: ");
                foreach (var c in player.GetComponents<Component>())
                {
                    Debug.Log($"[Diagnose]   {c.GetType().Name}");
                }
                var sr = player.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    var s = sr.sprite;
                    Debug.Log($"[Diagnose] Player SR: sprite={s?.name ?? "null"} rect={s?.rect} guid={AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(s))}");
                }
            }
            // Ground
            var ground = GameObject.Find("Ground");
            if (ground != null && ground.transform.childCount > 0)
            {
                var childSr = ground.transform.GetChild(0).GetComponent<SpriteRenderer>();
                Debug.Log($"[Diagnose] Ground[0] sprite={childSr?.sprite?.name}");
            }
            // File system check
            Debug.Log($"[Diagnose] ArtDir exists: {System.IO.Directory.Exists(ArtDir)}");
            if (System.IO.Directory.Exists(ArtDir))
            {
                var files = System.IO.Directory.GetFiles(ArtDir, "*.png");
                Debug.Log($"[Diagnose] ArtDir png count: {files.Length}");
            }
        }

        [MenuItem("Steel Rain/Build Windows")]

        /// <summary>
        /// 仅生成PNG和音频资源（批处理模式分两步执行的第一步）
        /// </summary>
        public static void Step1_GenerateAssets()
        {
            Debug.Log("[VerticalSliceBuilder] Step1: Generating assets...");
            EnsureDirectories();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            GenerateSprites();
            AudioGenerator.GenerateAll();
            MusicGenerator.GenerateAll();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log("[VerticalSliceBuilder] Step1 complete!");
        }
        public static void BuildWindows()
        {
            BuildAll();
            BuildTools.BuildWindows();
        }

        // ===== 目录 =====

        private static void EnsureDirectories()
        {
            Directory.CreateDirectory(ArtDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(DataDir + "/Characters");
            Directory.CreateDirectory(DataDir + "/Weapons");
            Directory.CreateDirectory(DataDir + "/Enemies");
            Directory.CreateDirectory("Assets/Scenes");
        }

        // ===== 精灵图生成 =====

        private static Sprite MakeSprite(string name, int w, int h, Color color)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[w * h];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels32(pixels);
            tex.Apply();

            var path = $"{ArtDir}/{name}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            return LoadOrImportSprite(path);
        }

        /// <summary>
        /// 加载或重新导入Sprite，兼容批处理模式
        /// 核心修复：Unity 6批处理模式下不生成Sprite子资源，需要直接修改meta文件
        /// </summary>
        public static Sprite LoadOrImportSprite(string path)
        {
            // 0) 优先尝试从 Sprite Sheet 加载子精灵（v2.2 兼容）
            // 若单张图片不存在但存在对应的 _sheet.png，则从 sheet 中加载 idle_0
            try
            {
                var sheetSprite = SteelRain.Editor.SpriteSheetImporter.LoadSpriteCompat(path);
                if (sheetSprite != null)
                {
                    Debug.Log($"[LoadOrImportSprite] Loaded from sprite sheet: {path} → {sheetSprite.name}");
                    return sheetSprite;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[LoadOrImportSprite] Sprite sheet compat failed for {path}: {e.Message}");
            }

            // 1) 优先尝试 LoadAllAssetsAtPath
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            if (assets != null)
            {
                foreach (var a in assets)
                {
                    if (a is Sprite s) return s;
                }
            }

            // 2) 尝试 LoadAssetAtPath<Sprite>
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;

            // 3) 直接修改meta文件，强制Unity识别为Sprite并生成子资源
            EnsureSpriteMeta(path);

            // 强制重新导入
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            // 4) 再次尝试加载
            assets = AssetDatabase.LoadAllAssetsAtPath(path);
            if (assets != null)
            {
                foreach (var a in assets)
                {
                    if (a is Sprite s) return s;
                }
            }

            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;

            // 5) 最后fallback：使用Texture2D创建并添加到Asset
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                var newSprite = Sprite.Create(tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    16f);
                newSprite.name = System.IO.Path.GetFileNameWithoutExtension(path);
                AssetDatabase.AddObjectToAsset(newSprite, tex);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                Debug.LogWarning($"[LoadOrImportSprite] Created runtime sprite for {path}");
                return newSprite;
            }

            Debug.LogError($"[LoadOrImportSprite] Failed to load sprite at {path}");
            return null;
        }

        /// <summary>
        /// 直接修改PNG的meta文件，确保Unity识别为Sprite类型并生成子资源
        /// 解决Unity 6批处理模式下spriteID为空的问题
        /// </summary>
        private static void EnsureSpriteMeta(string assetPath)
        {
            var metaPath = assetPath + ".meta";
            if (!File.Exists(metaPath)) return;

            var name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            var content = File.ReadAllText(metaPath);
            var changed = false;

            // 生成稳定的spriteID（基于文件名hash）
            long spriteId = 0;
            foreach (char c in name)
                spriteId = spriteId * 31 + c;
            spriteId = Math.Abs(spriteId);
            if (spriteId < 1000000000000000L) spriteId += 1000000000000000L;

            // 1) textureType: X -> textureType: 8 (Sprite)
            if (!Regex.IsMatch(content, @"textureType:\s*8"))
            {
                content = Regex.Replace(content, @"textureType:\s*\d+", "textureType: 8");
                changed = true;
            }

            // 2) spriteMode: X -> spriteMode: 1 (Single)
            if (!Regex.IsMatch(content, @"spriteMode:\s*1\b"))
            {
                if (Regex.IsMatch(content, @"spriteMode:\s*\d+"))
                    content = Regex.Replace(content, @"spriteMode:\s*\d+", "spriteMode: 1");
                changed = true;
            }

            // 3) spriteID: <空> -> spriteID: <id>
            if (Regex.IsMatch(content, @"spriteID:\s*$", RegexOptions.Multiline))
            {
                content = Regex.Replace(content, @"spriteID:\s*$", $"spriteID: {spriteId}", RegexOptions.Multiline);
                changed = true;
            }

            // 4) internalID: 0 -> internalID: <负数>
            if (Regex.IsMatch(content, @"internalID:\s*0\s*$", RegexOptions.Multiline))
            {
                content = Regex.Replace(content, @"internalID:\s*0", $"internalID: -{spriteId}");
                changed = true;
            }

            // 5) nameFileIdTable: {} -> nameFileIdTable:\n      <name>: 21300000
            if (Regex.IsMatch(content, @"nameFileIdTable:\s*\{\}"))
            {
                content = Regex.Replace(content, @"nameFileIdTable:\s*\{\}", $"nameFileIdTable:\n      {name}: 21300000");
                changed = true;
            }

            if (changed)
            {
                File.WriteAllText(metaPath, content);
                Debug.Log($"[EnsureSpriteMeta] Fixed meta for {assetPath}: spriteID={spriteId}");
            }
        }

        private static Sprite MakePlayerSprite()
        {
            int w = 16, h = 24;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
            var px = new Color32[w * h];
            var transparent = new Color32(0, 0, 0, 0);
            var body = new Color32(80, 120, 180, 255);
            var head = new Color32(220, 180, 140, 255);
            var scarf = new Color32(200, 50, 50, 255);
            var gun = new Color32(60, 60, 60, 255);

            for (int i = 0; i < px.Length; i++) px[i] = transparent;

            // 头部
            FillRect(px, w, 5, 17, 6, 22, head);
            // 身体
            FillRect(px, w, 5, 9, 10, 16, body);
            // 围巾
            FillRect(px, w, 5, 14, 10, 15, scarf);
            // 腿
            FillRect(px, w, 5, 3, 7, 8, body);
            FillRect(px, w, 9, 3, 11, 8, body);
            // 枪
            FillRect(px, w, 11, 10, 15, 12, gun);

            tex.SetPixels32(px);
            tex.Apply();
            var path = $"{ArtDir}/player_aila.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            return LoadOrImportSprite(path);
        }

        private static void FillRect(Color32[] px, int w, int x0, int y0, int x1, int y1, Color32 c)
        {
            for (int y = y0; y <= y1 && y < 24; y++)
                for (int x = x0; x <= x1 && x < 16; x++)
                    if (y >= 0 && x >= 0)
                        px[y * w + x] = c;
        }

        private static void GenerateSprites()
        {
            Debug.Log("[VerticalSliceBuilder] Generating sprites...");
            // 优先使用增强版美术资源生成器（精致像素艺术）
            // 它会覆盖同名简单精灵，生成更精致的角色/敌人/环境/UI/特效
            try
            {
                EnhancedArtGenerator.GenerateAll();
                Debug.Log("[VerticalSliceBuilder] Enhanced art generated successfully.");
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[VerticalSliceBuilder] Enhanced art generation failed, falling back to simple sprites: {e.Message}");
            }

            // 回退：简单纯色精灵
            MakePlayerSprite();
            MakeSprite("bullet_player", 4, 2, new Color(1f, 0.9f, 0.2f, 1f));
            MakeSprite("bullet_enemy", 4, 2, new Color(1f, 0.3f, 0.2f, 1f));
            MakeGroundSprite("ground_beach", 16, 16, new Color(0.76f, 0.7f, 0.5f), new Color(0.65f, 0.6f, 0.42f));
            MakeGroundSprite("ground_village", 16, 16, new Color(0.55f, 0.4f, 0.3f), new Color(0.45f, 0.32f, 0.22f));
            MakeGroundSprite("ground_trench", 16, 16, new Color(0.35f, 0.28f, 0.18f), new Color(0.28f, 0.22f, 0.14f));
            MakeEnemySprite("enemy_rifle", 14, 20, new Color(0.8f, 0.4f, 0.3f), new Color(0.6f, 0.3f, 0.2f));
            MakeEnemySprite("enemy_shield", 16, 22, new Color(0.5f, 0.6f, 0.4f), new Color(0.35f, 0.45f, 0.3f));
            MakeDroneSprite("enemy_drone", 18, 12, new Color(0.4f, 0.5f, 0.6f));
            MakeBossSprite("miniboss_walker", 40, 48, new Color(0.3f, 0.3f, 0.35f));
            MakeSprite("crate", 14, 14, new Color(0.6f, 0.45f, 0.25f));
            MakeSprite("upgrade_capsule", 10, 14, new Color(0.2f, 0.6f, 1f));
            MakeSprite("checkpoint_flag", 8, 24, new Color(0.9f, 0.85f, 0.2f));
            MakeSprite("background_sky", 64, 64, new Color(0.15f, 0.12f, 0.2f));
            MakeSprite("background_sea", 64, 32, new Color(0.1f, 0.2f, 0.35f));
        }

        private static void MakeGroundSprite(string name, int w, int h, Color fill, Color outline)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    bool isEdge = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                    px[y * w + x] = isEdge ? outline : fill;
                }
            tex.SetPixels32(px);
            tex.Apply();
            SaveTextureAsSprite(name, tex);
        }

        private static void MakeEnemySprite(string name, int w, int h, Color body, Color outline)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    bool isEdge = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                    bool isHead = y > h * 0.7f && x > w * 0.25f && x < w * 0.75f;
                    bool isGun = y > h * 0.4f && y < h * 0.5f && x > w * 0.6f;
                    Color c = isHead ? Color.Lerp(body, Color.white, 0.3f) :
                              isGun ? new Color(0.3f, 0.3f, 0.3f) :
                              isEdge ? outline : body;
                    px[y * w + x] = c;
                }
            tex.SetPixels32(px);
            tex.Apply();
            SaveTextureAsSprite(name, tex);
        }

        private static void MakeDroneSprite(string name, int w, int h, Color body)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            var outline = Color.Lerp(body, Color.black, 0.3f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    bool isEdge = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                    bool isBody = x > w * 0.2f && x < w * 0.8f && y > h * 0.3f && y < h * 0.7f;
                    bool isEye = Mathf.Abs(x - w * 0.5f) < 1 && Mathf.Abs(y - h * 0.5f) < 1;
                    Color c = isEye ? Color.red : isEdge ? outline : isBody ? body : new Color(0, 0, 0, 0);
                    px[y * w + x] = c;
                }
            tex.SetPixels32(px);
            tex.Apply();
            SaveTextureAsSprite(name, tex);
        }

        private static void MakeBossSprite(string name, int w, int h, Color body)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            var outline = Color.Lerp(body, Color.black, 0.4f);
            var highlight = Color.Lerp(body, Color.white, 0.2f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    bool isEdge = x < 2 || x >= w - 2 || y < 2 || y >= h - 2;
                    bool isBody = x > w * 0.15f && x < w * 0.85f && y > h * 0.15f && y < h * 0.85f;
                    bool isEye = Mathf.Abs(x - w * 0.5f) < 3 && Mathf.Abs(y - h * 0.7f) < 2;
                    bool isGun = y > h * 0.4f && y < h * 0.5f && x > w * 0.6f && x < w * 0.9f;
                    bool isLeg = (x > w * 0.2f && x < w * 0.35f && y < h * 0.25f) ||
                                 (x > w * 0.65f && x < w * 0.8f && y < h * 0.25f);
                    Color c = isEye ? Color.red :
                              isGun ? new Color(0.2f, 0.2f, 0.25f) :
                              isLeg ? outline :
                              isEdge ? outline : isBody ? highlight : body;
                    px[y * w + x] = c;
                }
            tex.SetPixels32(px);
            tex.Apply();
            SaveTextureAsSprite(name, tex);
        }

        private static void SaveTextureAsSprite(string name, Texture2D tex)
        {
            var path = $"{ArtDir}/{name}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }

        // ===== 数据资产 =====

        private static CharacterDefinition CreateCharacterData()
        {
            var path = $"{DataDir}/Characters/Aila.asset";
            var existing = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(path);
            if (existing != null)
            {
                // 强制更新叙事字段（确保新添加的 lore/combatStyle 同步到已存在的资产）
                existing.lore = "前联邦特种部队队长，在\u201C铁雨\u201D事变中失去整个小队。" +
                                "她带着对逝去战友的承诺，重新组建四人小队深入敌后。" +
                                "冷静、果断，是团队的战术核心。";
                existing.combatStyle = "全能型突击手：均衡的火力与机动性，突破火线技能可瞬间穿透敌阵";
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var aila = ScriptableObject.CreateInstance<CharacterDefinition>();
            aila.id = "aila";
            aila.displayName = "Aila";
            aila.maxHealth = 6;
            aila.moveSpeed = 7.2f;
            aila.jumpVelocity = 11.0f;
            aila.gravityScale = 2.5f;
            aila.fallGravityMultiplier = 1.35f;
            aila.crouchSpeedMultiplier = 0.45f;
            aila.crouchColliderHeightMultiplier = 0.6f;
            aila.dodgeSpeed = 12f;
            aila.dodgeDuration = 0.16f;
            aila.dodgeCooldown = 0.65f;
            aila.skillId = CharacterSkillId.BreakthroughFire;
            aila.skillCooldown = 6f;
            aila.tintColor = new Color(0.7f, 0.85f, 1f, 1f);
            aila.projectileColor = new Color(1f, 0.9f, 0.3f, 1f);   // 金黄色子弹
            aila.projectileScale = 1.0f;
            aila.damageMultiplier = 1.0f;   // 标准伤害
            aila.lore = "前联邦特种部队队长，在\u201c铁雨\u201d事变中失去整个小队。" +
                        "她带着对逝去战友的承诺，重新组建四人小队深入敌后。" +
                        "冷静、果断，是团队的战术核心。";
            aila.combatStyle = "全能型突击手：均衡的火力与机动性，突破火线技能可瞬间穿透敌阵";
            AssignCharacterSprites(aila, "aila");
            AssetDatabase.CreateAsset(aila, path);
            return aila;
        }

        /// <summary>
        /// 为角色赋值 portrait/crouch/prone/jump 四种精灵。
        /// 这是让 5 个角色在游戏中看起来有差别的关键。
        /// spriteName 是精灵文件名（如 aila/kael/mira/zen/nova），可能与角色 id 不同。
        /// </summary>
        private static void AssignCharacterSprites(CharacterDefinition def, string spriteName)
        {
            def.portraitSprite = LoadOrImportSprite($"{ArtDir}/player_{spriteName}.png");
            def.crouchSprite = LoadOrImportSprite($"{ArtDir}/player_{spriteName}_crouch.png");
            def.proneSprite = LoadOrImportSprite($"{ArtDir}/player_{spriteName}_prone.png");
            def.jumpSprite = LoadOrImportSprite($"{ArtDir}/player_{spriteName}_jump.png");
            Debug.Log($"[Character] Assigned sprites for {def.id} (sprite={spriteName}): portrait={def.portraitSprite?.name}, crouch={def.crouchSprite?.name}, prone={def.proneSprite?.name}, jump={def.jumpSprite?.name}");
        }

        private static CharacterDefinition[] CreateAllCharacterData()
        {
            var aila = CreateCharacterData();

            var brunoPath = $"{DataDir}/Characters/Bruno.asset";
            var bruno = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(brunoPath);
            if (bruno != null)
            {
                bruno.lore = "前重装步兵，在战场上以\u201c移动堡垒\u201d闻名。" +
                             "他的战壕巨盾曾保护无数战友撤离。" +
                             "沉默寡言，但用行动证明忠诚。";
                bruno.combatStyle = "重装防御者：高血量高伤害，战壕巨盾技能可创造临时掩体";
                EditorUtility.SetDirty(bruno);
            }
            if (bruno == null)
            {
                bruno = ScriptableObject.CreateInstance<CharacterDefinition>();
                bruno.id = "bruno";
                bruno.displayName = "Bruno";
                bruno.maxHealth = 8;
                bruno.moveSpeed = 6.0f;
                bruno.jumpVelocity = 12.0f;
                bruno.gravityScale = 2.7f;
                bruno.fallGravityMultiplier = 1.45f;
                bruno.crouchSpeedMultiplier = 0.4f;
                bruno.crouchColliderHeightMultiplier = 0.6f;
                bruno.dodgeSpeed = 10f;
                bruno.dodgeDuration = 0.18f;
                bruno.dodgeCooldown = 0.75f;
                bruno.skillId = CharacterSkillId.TrenchShield;
                bruno.skillCooldown = 7f;
                bruno.tintColor = new Color(0.8f, 0.6f, 0.4f, 1f);
                bruno.projectileColor = new Color(1f, 0.4f, 0.2f, 1f);  // 橙红色大子弹
                bruno.projectileScale = 1.6f;
                bruno.damageMultiplier = 1.5f;   // 重装高伤
                bruno.lore = "前重装步兵，在战场上以\u201c移动堡垒\u201d闻名。" +
                             "他的战壕巨盾曾保护无数战友撤离。" +
                             "沉默寡言，但用行动证明忠诚。";
                bruno.combatStyle = "重装防御者：高血量高伤害，战壕巨盾技能可创造临时掩体";
                AssignCharacterSprites(bruno, "kael");
                AssetDatabase.CreateAsset(bruno, brunoPath);
            }

            var maraPath = $"{DataDir}/Characters/Mara.asset";
            var mara = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(maraPath);
            if (mara != null)
            {
                mara.lore = "天才狙击手，曾是奥运射击冠军。" +
                            "战争爆发后她放弃了奖牌，拿起了真正的步枪。" +
                            "她的轰炸矩阵技能能呼叫精准的轨道打击。";
                mara.combatStyle = "远程狙击手：超高单体伤害，轰炸矩阵技能可覆盖大片区域";
                EditorUtility.SetDirty(mara);
            }
            if (mara == null)
            {
                mara = ScriptableObject.CreateInstance<CharacterDefinition>();
                mara.id = "mara";
                mara.displayName = "Mara";
                mara.maxHealth = 6;
                mara.moveSpeed = 7.0f;
                mara.jumpVelocity = 11.0f;
                mara.gravityScale = 2.8f;
                mara.fallGravityMultiplier = 1.35f;
                mara.crouchSpeedMultiplier = 0.45f;
                mara.crouchColliderHeightMultiplier = 0.6f;
                mara.dodgeSpeed = 11f;
                mara.dodgeDuration = 0.16f;
                mara.dodgeCooldown = 0.65f;
                mara.skillId = CharacterSkillId.BombardmentMatrix;
                mara.skillCooldown = 8f;
                mara.tintColor = new Color(0.9f, 0.5f, 0.8f, 1f);
                mara.projectileColor = new Color(0.3f, 1f, 0.5f, 1f);   // 绿色细长子弹（狙击）
                mara.projectileScale = 0.7f;
                mara.damageMultiplier = 2.0f;   // 狙击高伤
                mara.lore = "天才狙击手，曾是奥运射击冠军。" +
                            "战争爆发后她放弃了奖牌，拿起了真正的步枪。" +
                            "她的轰炸矩阵技能能呼叫精准的轨道打击。";
                mara.combatStyle = "远程狙击手：超高单体伤害，轰炸矩阵技能可覆盖大片区域";
                AssignCharacterSprites(mara, "mira");
                AssetDatabase.CreateAsset(mara, maraPath);
            }

            var nikoPath = $"{DataDir}/Characters/Niko.asset";
            var niko = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(nikoPath);
            if (niko != null)
            {
                niko.lore = "神秘的少年黑客，能操控时间流速。" +
                            "没人知道他的真实身份，只知道他称这场战争为\u201c既定命运\u201d。" +
                            "他的时间裂隙技能可让整个战场减速。";
                niko.combatStyle = "时空操控者：高频低伤射击，时间裂隙技能可全局减速敌人";
                EditorUtility.SetDirty(niko);
            }
            if (niko == null)
            {
                niko = ScriptableObject.CreateInstance<CharacterDefinition>();
                niko.id = "niko";
                niko.displayName = "Niko";
                niko.maxHealth = 5;
                niko.moveSpeed = 7.5f;
                niko.jumpVelocity = 12.0f;
                niko.gravityScale = 2.5f;
                niko.fallGravityMultiplier = 1.25f;
                niko.crouchSpeedMultiplier = 0.45f;
                niko.crouchColliderHeightMultiplier = 0.6f;
                niko.dodgeSpeed = 13f;
                niko.dodgeDuration = 0.15f;
                niko.dodgeCooldown = 0.6f;
                niko.skillId = CharacterSkillId.TimeRift;
                niko.skillCooldown = 7f;
                niko.tintColor = new Color(0.5f, 1f, 0.7f, 1f);
                niko.projectileColor = new Color(0.6f, 0.3f, 1f, 1f);   // 紫色快速小子弹
                niko.projectileScale = 0.8f;
                niko.damageMultiplier = 0.7f;   // 快速低伤
                niko.lore = "神秘的少年黑客，能操控时间流速。" +
                            "没人知道他的真实身份，只知道他称这场战争为\u201c既定命运\u201d。" +
                            "他的时间裂隙技能可让整个战场减速。";
                niko.combatStyle = "时空操控者：高频低伤射击，时间裂隙技能可全局减速敌人";
                AssignCharacterSprites(niko, "zen");
                AssetDatabase.CreateAsset(niko, nikoPath);
            }

            return new[] { aila, bruno, mara, niko };
        }

        private static WeaponDefinition CreateAssaultRifleData()
        {
            var weaponPath = $"{DataDir}/Weapons/AssaultRifle.asset";
            var existing = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(weaponPath);
            if (existing != null) return existing;

            var auto = MakeForm("Auto", ProjectilePattern.Single, dmg: 1, cost: 1, rate: 9f, speed: 18f);
            var pierce = MakeForm("Pierce", ProjectilePattern.Piercing, dmg: 2, cost: 2, rate: 4f, speed: 20f, pierce: 2);
            var grenade = MakeForm("Grenade", ProjectilePattern.LobbedExplosive, dmg: 4, cost: 5, rate: 1.2f, speed: 10f, radius: 2f);

            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.id = "assault_rifle";
            weapon.displayName = "Assault Rifle";
            weapon.startingAmmo = -1;
            weapon.baseAmmoInfinite = true;
            weapon.forms = new[] { auto, pierce, grenade };
            AssetDatabase.CreateAsset(weapon, weaponPath);
            return weapon;
        }

        private static WeaponDefinition[] CreateAllWeaponData()
        {
            var rifle = CreateAssaultRifleData();

            // 霰弹枪
            var shotgunPath = $"{DataDir}/Weapons/Shotgun.asset";
            var shotgun = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(shotgunPath);
            if (shotgun == null)
            {
                var scatter = MakeForm("Scatter", ProjectilePattern.Spread, dmg: 1, cost: 2, rate: 1.8f, speed: 14f, count: 6, spread: 35f);
                var slug = MakeForm("Slug", ProjectilePattern.Single, dmg: 5, cost: 3, rate: 1.1f, speed: 22f);
                var burning = MakeForm("Burning", ProjectilePattern.Spread, dmg: 2, cost: 4, rate: 1f, speed: 12f, count: 3, spread: 20f);
                shotgun = ScriptableObject.CreateInstance<WeaponDefinition>();
                shotgun.id = "shotgun";
                shotgun.displayName = "Shotgun";
                shotgun.startingAmmo = -1;
                shotgun.baseAmmoInfinite = true;
                shotgun.forms = new[] { scatter, slug, burning };
                AssetDatabase.CreateAsset(shotgun, shotgunPath);
            }

            // 火箭筒
            var rocketPath = $"{DataDir}/Weapons/RocketLauncher.asset";
            var rocket = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(rocketPath);
            if (rocket == null)
            {
                var direct = MakeForm("Direct", ProjectilePattern.Single, dmg: 8, cost: 1, rate: 0.7f, speed: 15f, radius: 2f);
                var split = MakeForm("Split", ProjectilePattern.SplitRocket, dmg: 4, cost: 2, rate: 0.6f, speed: 14f, radius: 1.5f);
                var seeker = MakeForm("Seeker", ProjectilePattern.Single, dmg: 5, cost: 2, rate: 0.8f, speed: 12f);
                rocket = ScriptableObject.CreateInstance<WeaponDefinition>();
                rocket.id = "rocket_launcher";
                rocket.displayName = "Rocket Launcher";
                rocket.startingAmmo = -1;
                rocket.baseAmmoInfinite = true;
                rocket.forms = new[] { direct, split, seeker };
                AssetDatabase.CreateAsset(rocket, rocketPath);
            }

            return new[] { rifle, shotgun, rocket };
        }

        private static WeaponFormDefinition MakeForm(string name, ProjectilePattern pattern,
            int dmg, int cost, float rate, float speed,
            int pierce = 0, float radius = 0f, int count = 1, float spread = 0f)
        {
            var form = ScriptableObject.CreateInstance<WeaponFormDefinition>();
            form.id = name.ToLower();
            form.displayName = name;
            form.pattern = pattern;
            form.damage = dmg;
            form.ammoCost = cost;
            form.fireRate = rate;
            form.projectileSpeed = speed;
            form.projectileCount = count;
            form.spreadAngle = spread;
            form.pierceCount = pierce;
            form.explosionRadius = radius;
            var path = $"{DataDir}/Weapons/Form_{name}.asset";
            AssetDatabase.CreateAsset(form, path);
            return form;
        }

        private static EnemyDefinition[] CreateEnemyData()
        {
            var defs = new EnemyDefinition[7];
            defs[0] = MakeEnemy("RifleSoldier", "Rifle Soldier", 3, 2.5f, 6f, 1.4f, EnemyAttackPattern.RifleBurst, new Color(0.8f, 0.4f, 0.3f));
            defs[1] = MakeEnemy("ShieldSoldier", "Shield Soldier", 6, 1.6f, 2f, 2f, EnemyAttackPattern.ShieldAdvance, new Color(0.5f, 0.6f, 0.4f));
            defs[2] = MakeEnemy("Drone", "Drone", 2, 4.2f, 5f, 1.8f, EnemyAttackPattern.DroneDive, new Color(0.4f, 0.5f, 0.6f));
            defs[3] = MakeEnemy("Grenadier", "Grenadier", 3, 2f, 7f, 2.5f, EnemyAttackPattern.GrenadeArc, new Color(0.6f, 0.5f, 0.2f));
            // 新增 3 种敌人
            defs[4] = MakeEnemyAdvanced("Charger", "Fast Charger", 2, 6.0f, 1.5f, 1.0f, EnemyAttackPattern.RapidCharge,
                new Color(0.9f, 0.2f, 0.5f), contactDamage: 2, rangedDamage: 0);
            defs[5] = MakeEnemyAdvanced("Sniper", "Sniper Enemy", 2, 1.5f, 10f, 3.0f, EnemyAttackPattern.SniperShot,
                new Color(0.2f, 0.9f, 0.3f), contactDamage: 1, rangedDamage: 2, projSpeed: 14f, projColor: new Color(1f, 0.2f, 0.2f), projScale: 2f);
            defs[6] = MakeEnemyAdvanced("HeavyGunner", "Heavy Gunner", 5, 1.8f, 7f, 1.6f, EnemyAttackPattern.HeavyMachineGun,
                new Color(0.3f, 0.3f, 0.4f), contactDamage: 2, rangedDamage: 1, projSpeed: 10f,
                projColor: new Color(1f, 0.6f, 0.1f), projScale: 1.4f, projCount: 3, projSpread: 25f);
            return defs;
        }

        private static EnemyDefinition MakeEnemy(string id, string name, int hp, float speed,
            float atkRange, float atkCd, EnemyAttackPattern pattern, Color color)
        {
            var path = $"{DataDir}/Enemies/{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (existing != null) return existing;

            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.id = id.ToLower();
            def.displayName = name;
            def.maxHealth = hp;
            def.moveSpeed = speed;
            def.attackRange = atkRange;
            def.attackCooldown = atkCd;
            def.attackPattern = pattern;
            def.spriteColor = color;
            def.contactDamage = 1;
            def.rangedDamage = 1;
            def.projectileSpeed = 8f;
            AssetDatabase.CreateAsset(def, path);
            return def;
        }

        private static EnemyDefinition MakeEnemyAdvanced(string id, string name, int hp, float speed,
            float atkRange, float atkCd, EnemyAttackPattern pattern, Color color,
            int contactDamage = 1, int rangedDamage = 1, float projSpeed = 8f,
            Color projColor = default, float projScale = 1f, int projCount = 1, float projSpread = 0f)
        {
            var path = $"{DataDir}/Enemies/{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (existing != null) return existing;

            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.id = id.ToLower();
            def.displayName = name;
            def.maxHealth = hp;
            def.moveSpeed = speed;
            def.attackRange = atkRange;
            def.attackCooldown = atkCd;
            def.attackPattern = pattern;
            def.spriteColor = color;
            def.contactDamage = contactDamage;
            def.rangedDamage = rangedDamage;
            def.projectileSpeed = projSpeed;
            def.projectileColor = projColor == default ? new Color(1f, 0.5f, 0.3f, 1f) : projColor;
            def.projectileScale = projScale;
            def.projectileCount = projCount;
            def.projectileSpread = projSpread;
            def.scoreValue = 150;
            AssetDatabase.CreateAsset(def, path);
            return def;
        }

        // ===== 预制体 =====

        private static Projectile CreateProjectilePrefab()
        {
            var path = $"{PrefabDir}/Projectile.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<Projectile>(path);
            if (existing != null) return existing;

            var go = new GameObject("Projectile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/bullet_player.png");
            sr.sortingOrder = 10;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.3f, 0.15f);
            var proj = go.AddComponent<Projectile>();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<Projectile>(path);
        }

        private static GameObject CreatePlayerPrefab(CharacterDefinition aila, WeaponDefinition weapon,
            Projectile projectile, CharacterDefinition[] allCharacters)
        {
            var path = $"{PrefabDir}/Player_Aila.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject("Player_Aila");
            go.tag = "Player";

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/player_aila.png");
            sr.sortingOrder = 5;
            sr.color = Color.white;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2.6f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.2f);

            var health = go.AddComponent<Health>();
            var controller = go.AddComponent<PlayerController2D>();
            var combat = go.AddComponent<PlayerCombat>();
            var dodge = go.AddComponent<PlayerDodge>();
            go.AddComponent<HitFlash>();
            // Easy 难度自动回血组件（仅在 Easy 难度生效，其他难度自动失效）
            go.AddComponent<AutoHeal>();
            var muzzleFlash = go.AddComponent<MuzzleFlash>();
            var skill = go.AddComponent<CharacterSkill>();

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(go.transform);
            groundCheck.transform.localPosition = new Vector3(0f, -0.65f, 0f);

            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(go.transform);
            muzzle.transform.localPosition = new Vector3(0.6f, 0f, 0f);

            var so = new SerializedObject(controller);
            so.FindProperty("character").objectReferenceValue = aila;
            so.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
            so.FindProperty("groundMask").intValue = 1 << 6;
            so.ApplyModifiedProperties();

            var soCombat = new SerializedObject(combat);
            soCombat.FindProperty("controller").objectReferenceValue = controller;
            soCombat.FindProperty("muzzle").objectReferenceValue = muzzle.transform;
            soCombat.FindProperty("startingWeapon").objectReferenceValue = weapon;
            soCombat.FindProperty("projectilePrefab").objectReferenceValue = projectile;
            soCombat.FindProperty("muzzleFlash").objectReferenceValue = muzzleFlash;
            soCombat.ApplyModifiedProperties();

            var soDodge = new SerializedObject(dodge);
            soDodge.FindProperty("controller").objectReferenceValue = controller;
            soDodge.ApplyModifiedProperties();

            // 技能
            var soSkill = new SerializedObject(skill);
            soSkill.FindProperty("controller").objectReferenceValue = controller;
            soSkill.FindProperty("combat").objectReferenceValue = combat;
            soSkill.FindProperty("skillProjectilePrefab").objectReferenceValue = projectile;
            soSkill.ApplyModifiedProperties();

            // 4 人小队
            if (allCharacters != null && allCharacters.Length >= 2)
            {
                var squad = go.AddComponent<PlayerSquad>();
                var soSquad = new SerializedObject(squad);
                soSquad.FindProperty("controller").objectReferenceValue = controller;
                soSquad.FindProperty("combat").objectReferenceValue = combat;
                soSquad.FindProperty("skill").objectReferenceValue = skill;
                var membersProp = soSquad.FindProperty("members");
                membersProp.arraySize = allCharacters.Length;
                for (int i = 0; i < allCharacters.Length; i++)
                    membersProp.GetArrayElementAtIndex(i).objectReferenceValue = allCharacters[i];
                soSquad.ApplyModifiedProperties();
            }

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject CreateEnemyPrefab(Projectile projectile)
        {
            var path = $"{PrefabDir}/Enemy_Rifle.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("Enemy_Rifle");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/enemy_rifle.png");
            sr.sortingOrder = 4;
            sr.color = new Color(0.8f, 0.4f, 0.3f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.7f, 1f);

            var health = go.AddComponent<Health>();
            var controller = go.AddComponent<EnemyController>();
            go.AddComponent<HitFlash>();

            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            // Y=0.4 胸部高度：站立玩家(顶0.6)会被命中，蹲下(顶0.36)/趴下(顶0.21)能躲过
            firePoint.transform.localPosition = new Vector3(0.5f, 0.4f, 0f);

            var so = new SerializedObject(controller);
            so.FindProperty("enemyProjectilePrefab").objectReferenceValue = projectile;
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject CreateShieldEnemyPrefab(Projectile projectile)
        {
            var path = $"{PrefabDir}/Enemy_Shield.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var go = new GameObject("Enemy_Shield");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/enemy_shield.png");
            sr.sortingOrder = 4;
            sr.color = new Color(0.5f, 0.6f, 0.4f, 1f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1f);

            var health = go.AddComponent<Health>();
            var ai = go.AddComponent<ShieldSoldierAI>();

            // 盾牌子物体
            var shieldGo = new GameObject("Shield");
            shieldGo.transform.SetParent(go.transform);
            shieldGo.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            var shieldSr = shieldGo.AddComponent<SpriteRenderer>();
            shieldSr.sprite = LoadOrImportSprite($"{ArtDir}/enemy_shield.png");
            shieldSr.color = new Color(0.7f, 0.8f, 0.6f, 0.8f);
            shieldSr.sortingOrder = 5;

            var so = new SerializedObject(ai);
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.FindProperty("shieldRenderer").objectReferenceValue = shieldSr;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject CreateDronePrefab(Projectile projectile)
        {
            var path = $"{PrefabDir}/Enemy_Drone.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var go = new GameObject("Enemy_Drone");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/enemy_drone.png");
            sr.sortingOrder = 6;
            sr.color = new Color(0.4f, 0.5f, 0.6f, 1f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.9f, 0.6f);

            var health = go.AddComponent<Health>();
            var ai = go.AddComponent<DroneAI>();

            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = Vector3.zero;

            var so = new SerializedObject(ai);
            so.FindProperty("projectilePrefab").objectReferenceValue = projectile;
            so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject CreateGrenadierPrefab(Projectile projectile)
        {
            var path = $"{PrefabDir}/Enemy_Grenadier.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var go = new GameObject("Enemy_Grenadier");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/enemy_rifle.png");
            sr.sortingOrder = 4;
            sr.color = new Color(0.6f, 0.5f, 0.2f, 1f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.7f, 1f);

            var health = go.AddComponent<Health>();
            var ai = go.AddComponent<GrenadierAI>();

            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0.5f, 0.3f, 0f);

            var so = new SerializedObject(ai);
            so.FindProperty("projectilePrefab").objectReferenceValue = projectile;
            so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        // ===== 新增敌人预制体 =====

        private static GameObject CreateChargerPrefab(Projectile projectile)
        {
            var path = $"{PrefabDir}/Enemy_Charger.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var go = new GameObject("Enemy_Charger");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/enemy_rifle.png");
            sr.sortingOrder = 4;
            sr.color = new Color(0.9f, 0.2f, 0.5f, 1f);  // 粉红色，醒目

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.6f, 0.9f);  // 稍小，显得灵活

            var health = go.AddComponent<Health>();
            var controller = go.AddComponent<EnemyController>();
            go.AddComponent<HitFlash>();

            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0.5f, 0.4f, 0f);

            var so = new SerializedObject(controller);
            so.FindProperty("enemyProjectilePrefab").objectReferenceValue = projectile;
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject CreateSniperPrefab(Projectile projectile)
        {
            var path = $"{PrefabDir}/Enemy_Sniper.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var go = new GameObject("Enemy_Sniper");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/enemy_rifle.png");
            sr.sortingOrder = 4;
            sr.color = new Color(0.2f, 0.9f, 0.3f, 1f);  // 绿色，狙击手

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.7f, 1f);

            var health = go.AddComponent<Health>();
            var controller = go.AddComponent<EnemyController>();
            go.AddComponent<HitFlash>();

            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0.5f, 0.5f, 0f);  // 较高位置

            var so = new SerializedObject(controller);
            so.FindProperty("enemyProjectilePrefab").objectReferenceValue = projectile;
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject CreateHeavyGunnerPrefab(Projectile projectile)
        {
            var path = $"{PrefabDir}/Enemy_HeavyGunner.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var go = new GameObject("Enemy_HeavyGunner");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/enemy_shield.png");  // 用较大的图
            sr.sortingOrder = 4;
            sr.color = new Color(0.3f, 0.3f, 0.4f, 1f);  // 深灰色，重装

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.9f, 1.1f);  // 较大，显得笨重

            var health = go.AddComponent<Health>();
            var controller = go.AddComponent<EnemyController>();
            go.AddComponent<HitFlash>();

            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0.5f, 0.4f, 0f);

            var so = new SerializedObject(controller);
            so.FindProperty("enemyProjectilePrefab").objectReferenceValue = projectile;
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static void CreateMiniBossPrefab()
        {
            var path = $"{PrefabDir}/MiniBoss_Walker.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            var go = new GameObject("MiniBoss_Walker");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/miniboss_walker.png");
            sr.sortingOrder = 3;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2.5f, 3f);

            var health = go.AddComponent<Health>();
            health.Initialize(35, Team.Enemy);

            var boss = go.AddComponent<MiniBossWalker>();
            var projectile = AssetDatabase.LoadAssetAtPath<Projectile>($"{PrefabDir}/Projectile.prefab");

            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(1f, 0.5f, 0f);

            var so = new SerializedObject(boss);
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
            so.FindProperty("enemyProjectilePrefab").objectReferenceValue = projectile;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void CreateTurretBossPrefab()
        {
            var path = $"{PrefabDir}/TurretBoss.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            var go = new GameObject("TurretBoss");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/turret_boss.png");
            sr.sortingOrder = 3;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 2f);

            var health = go.AddComponent<Health>();
            health.Initialize(50, Team.Enemy);

            var boss = go.AddComponent<TurretBoss>();
            var projectile = AssetDatabase.LoadAssetAtPath<Projectile>($"{PrefabDir}/Projectile.prefab");

            // 3个炮管方向
            var fp1 = new GameObject("FirePoint_Left");
            fp1.transform.SetParent(go.transform);
            fp1.transform.localPosition = new Vector3(-0.5f, 0.5f, 0f);
            var fp2 = new GameObject("FirePoint_Center");
            fp2.transform.SetParent(go.transform);
            fp2.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            var fp3 = new GameObject("FirePoint_Right");
            fp3.transform.SetParent(go.transform);
            fp3.transform.localPosition = new Vector3(0.5f, 0.5f, 0f);

            var so = new SerializedObject(boss);
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.FindProperty("projectilePrefab").objectReferenceValue = projectile;
            var firePointsProp = so.FindProperty("firePoints");
            firePointsProp.arraySize = 3;
            firePointsProp.GetArrayElementAtIndex(0).objectReferenceValue = fp1.transform;
            firePointsProp.GetArrayElementAtIndex(1).objectReferenceValue = fp2.transform;
            firePointsProp.GetArrayElementAtIndex(2).objectReferenceValue = fp3.transform;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
        }

        private static GameObject CreateUpgradePickupPrefab()
        {
            var path = $"{PrefabDir}/Pickup_Upgrade.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var go = new GameObject("Pickup_Upgrade");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/upgrade_capsule.png");
            sr.sortingOrder = 8;
            sr.color = new Color(0.2f, 0.6f, 1f, 1f);

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.5f, 0.7f);

            go.AddComponent<WeaponUpgradePickup>();
            var timed = go.AddComponent<TimedPickup>();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject CreateWeaponPickupPrefab(WeaponDefinition weapon, Color tint)
        {
            var path = $"{PrefabDir}/Pickup_Weapon_{weapon.id}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var go = new GameObject($"Pickup_Weapon_{weapon.id}");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/upgrade_capsule.png");
            sr.sortingOrder = 8;
            sr.color = tint;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.6f, 0.8f);

            var pickup = go.AddComponent<WeaponSwapPickup>();
            var so = new SerializedObject(pickup);
            so.FindProperty("weapon").objectReferenceValue = weapon;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject CreateHealthPickupPrefab()
        {
            var path = $"{PrefabDir}/Pickup_Health.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var go = new GameObject("Pickup_Health");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/upgrade_capsule.png");
            sr.sortingOrder = 8;
            sr.color = new Color(0.2f, 0.9f, 0.3f, 1f);

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.5f, 0.7f);

            var hp = go.AddComponent<HealthPickup>();
            var so = new SerializedObject(hp);
            so.FindProperty("healAmount").intValue = 1;
            so.ApplyModifiedProperties();

            go.AddComponent<TimedPickup>();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject CreateCratePrefab(GameObject dropPrefab)
        {
            var path = $"{PrefabDir}/Destructible_Crate.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var go = new GameObject("Destructible_Crate");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/crate.png");
            sr.sortingOrder = 3;
            sr.color = new Color(0.6f, 0.45f, 0.25f, 1f);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.8f);

            var target = go.AddComponent<DestructibleTarget>();
            var so = new SerializedObject(target);
            so.FindProperty("dropPrefab").objectReferenceValue = dropPrefab;
            so.FindProperty("maxHealth").intValue = 2;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        // ===== 场景 =====

        private static void BuildLevel01Scene(GameObject playerPrefab, GameObject enemyPrefab,
            EnemyDefinition[] enemyDefs, Projectile projectile,
            GameObject upgradePickupPrefab, GameObject cratePrefab,
            GameObject healthPickupPrefab, CharacterDefinition[] characters,
            GameObject shieldPrefab, GameObject dronePrefab, GameObject grenadierPrefab,
            WeaponDefinition[] weapons,
            GameObject chargerPrefab = null, GameObject sniperPrefab = null, GameObject heavyGunnerPrefab = null)
        {
            Debug.Log("[VerticalSliceBuilder] Building Level01 scene...");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 关键修复：NewScene后active scene可能未设置，强制重新获取
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene != scene)
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
                activeScene = scene;
            }

            // EventSystem（UI按钮点击必需）
            CreateEventSystem();

            // 摄像机
            var camGo = new GameObject("MainCamera");
            camGo.transform.position = new Vector3(0, 0, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.1f, 0.18f);
            camGo.tag = "MainCamera";
            camGo.AddComponent<CameraShake>();
            var camFollow = camGo.AddComponent<SimpleCameraFollow>();

            // 地面层
            var groundLayer = 6;
            var groundGo = new GameObject("Ground");
            BuildGround(groundGo, groundLayer);

            // 玩家
            // 关键修复：使用EditorSceneManager.MoveGameObjectToScene确保Player被显式加入目标场景
            GameObject player = null;
            if (playerPrefab != null)
            {
                // 优先使用PrefabUtility创建prefab instance
                player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                if (player != null)
                {
                    // 强制使用EditorSceneManager.MoveGameObjectToScene将player移到目标scene
                    EditorSceneManager.MoveGameObjectToScene(player, scene);
                }
            }
            if (player == null)
            {
                Debug.LogWarning("[BuildLevel01] PrefabUtility.InstantiatePrefab returned null, using direct clone");
                player = new GameObject("Player_Aila");
                var srcSr = playerPrefab != null ? playerPrefab.GetComponentInChildren<SpriteRenderer>(true) : null;
                if (srcSr != null && srcSr.sprite != null)
                {
                    var newSr = player.AddComponent<SpriteRenderer>();
                    newSr.sprite = srcSr.sprite;
                    newSr.sortingOrder = 5;
                }
                player.AddComponent<Rigidbody2D>();
                player.AddComponent<BoxCollider2D>();
                player.AddComponent<Health>();
                player.AddComponent<PlayerController2D>();
                player.AddComponent<PlayerCombat>();
                EditorSceneManager.MoveGameObjectToScene(player, scene);
            }
            player.name = "Player_Aila";
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.tag = "Player";
            FixSpriteReferences(player, playerPrefab);
            var controller = player.GetComponent<PlayerController2D>();
            var combat = player.GetComponent<PlayerCombat>();
            Debug.Log($"[BuildLevel01] Player created at {player.transform.position}, scene={player.scene.name}, sceneRootCount={scene.rootCount}, sceneIsValid={scene.IsValid()}, components: ctrl={controller != null}, combat={combat != null}");

            // 摄像机跟随
            var soCam = new SerializedObject(camFollow);
            soCam.FindProperty("target").objectReferenceValue = player.transform;
            soCam.ApplyModifiedProperties();

            // 检查点管理器
            var cpMgrGo = new GameObject("CheckpointManager");
            var cpMgr = cpMgrGo.AddComponent<CheckpointManager>();
            var soCp = new SerializedObject(cpMgr);
            soCp.FindProperty("player").objectReferenceValue = player.transform;
            soCp.FindProperty("fallbackSpawn").vector3Value = new Vector3(0f, 1f, 0f);
            soCp.ApplyModifiedProperties();

            // ===== 成品级长关卡：6 个段落，总长约 460 单位 =====
            // 段落 A：海滩登陆 (0-70) — 步枪兵教学 + 基础战斗
            CreateSegment("A1_BeachIntro", new Vector3(8f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 2);
            CreateSegment("A2_BeachAdvance", new Vector3(20f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 3);
            CreateSegment("A3_BeachGrenadier", new Vector3(35f, 0f, 0f), grenadierPrefab, enemyDefs[3], player.transform, 2);
            CreateSegment("A4_BeachAssault", new Vector3(50f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 3);
            CreateSegment("A5_BeachDrone", new Vector3(62f, 0f, 0f), dronePrefab, enemyDefs[2], player.transform, 2);
            CreateCheckpoint("CP_A_BeachEnd", new Vector3(68f, 1.5f, 0f), cpMgr);

            // 段落 B：海岸村庄 (70-150) — 盾兵引入 + 垂直战斗
            CreateSegment("B1_VillageEntry", new Vector3(78f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 3);
            CreateSegment("B2_VillageShield", new Vector3(90f, 0f, 0f), shieldPrefab, enemyDefs[1], player.transform, 2);
            CreateSegment("B3_VillageMixed", new Vector3(105f, 0f, 0f), grenadierPrefab, enemyDefs[3], player.transform, 2);
            CreateSegment("B4_VillageAmbush", new Vector3(118f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 4);
            CreateSegment("B5_VillageDrone", new Vector3(130f, 0f, 0f), dronePrefab, enemyDefs[2], player.transform, 3);
            CreateSegment("B6_VillageShield", new Vector3(142f, 0f, 0f), shieldPrefab, enemyDefs[1], player.transform, 3);
            CreateCheckpoint("CP_B_VillageEnd", new Vector3(148f, 1.5f, 0f), cpMgr);

            // 段落 C：壕沟网络 (150-230) — 无人机 + 混合敌人 + 紧凑战斗
            CreateSegment("C1_TrenchEntry", new Vector3(158f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 3);
            CreateSegment("C2_TrenchDrone", new Vector3(170f, 0f, 0f), dronePrefab, enemyDefs[2], player.transform, 3);
            CreateSegment("C3_TrenchShield", new Vector3(182f, 0f, 0f), shieldPrefab, enemyDefs[1], player.transform, 2);
            CreateSegment("C4_TrenchGrenadier", new Vector3(195f, 0f, 0f), grenadierPrefab, enemyDefs[3], player.transform, 3);
            CreateSegment("C5_TrenchMixed", new Vector3(208f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 3);
            CreateSegment("C6_TrenchDrone", new Vector3(220f, 0f, 0f), dronePrefab, enemyDefs[2], player.transform, 2);
            CreateCheckpoint("CP_C_TrenchEnd", new Vector3(226f, 1.5f, 0f), cpMgr);

            // 段落 D：废墟城市 (230-310) — 重型混合战斗 + 中场小 Boss
            CreateSegment("D1_CityEntry", new Vector3(238f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 3);
            CreateSegment("D2_CityShield", new Vector3(250f, 0f, 0f), shieldPrefab, enemyDefs[1], player.transform, 3);
            CreateSegment("D3_CityGrenadier", new Vector3(262f, 0f, 0f), grenadierPrefab, enemyDefs[3], player.transform, 2);
            CreateSegment("D4_CityDroneSwarm", new Vector3(275f, 0f, 0f), dronePrefab, enemyDefs[2], player.transform, 4);
            CreateSegment("D5_CityMixed", new Vector3(288f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 4);
            CreateSegment("D6_CityShield", new Vector3(300f, 0f, 0f), shieldPrefab, enemyDefs[1], player.transform, 3);
            CreateCheckpoint("CP_D_CityEnd", new Vector3(306f, 1.5f, 0f), cpMgr);

            // 新敌人混入：快速冲锋兵突袭
            if (chargerPrefab != null)
                CreateSegment("D7_CityCharger", new Vector3(312f, 0f, 0f), chargerPrefab, enemyDefs[4], player.transform, 3);

            // 段落 E：工业郊区 (310-390) — 掷弹兵 + 无人机 + 危险环境
            CreateSegment("E1_IndustrialEntry", new Vector3(318f, 0f, 0f), grenadierPrefab, enemyDefs[3], player.transform, 3);
            CreateSegment("E2_IndustrialDrone", new Vector3(330f, 0f, 0f), dronePrefab, enemyDefs[2], player.transform, 3);
            CreateSegment("E3_IndustrialShield", new Vector3(342f, 0f, 0f), shieldPrefab, enemyDefs[1], player.transform, 3);
            CreateSegment("E4_IndustrialMixed", new Vector3(355f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 4);
            CreateSegment("E5_IndustrialGrenadier", new Vector3(368f, 0f, 0f), grenadierPrefab, enemyDefs[3], player.transform, 3);
            CreateSegment("E6_IndustrialDroneSwarm", new Vector3(380f, 0f, 0f), dronePrefab, enemyDefs[2], player.transform, 4);
            CreateCheckpoint("CP_E_IndustrialEnd", new Vector3(386f, 1.5f, 0f), cpMgr);

            // 新敌人混入：狙击手压制
            if (sniperPrefab != null)
                CreateSegment("E7_IndustrialSniper", new Vector3(392f, 0f, 0f), sniperPrefab, enemyDefs[5], player.transform, 2);

            // 段落 F：工厂入口 (390-460) — 最终冲刺 + Boss 竞技场
            CreateSegment("F1_FactoryGauntlet1", new Vector3(398f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 4);
            CreateSegment("F2_FactoryGauntlet2", new Vector3(410f, 0f, 0f), shieldPrefab, enemyDefs[1], player.transform, 3);
            // 新敌人混入：重机枪手守关
            if (heavyGunnerPrefab != null)
                CreateSegment("F2b_FactoryHeavyGunner", new Vector3(416f, 0f, 0f), heavyGunnerPrefab, enemyDefs[6], player.transform, 2);
            CreateSegment("F3_FactoryGauntlet3", new Vector3(422f, 0f, 0f), grenadierPrefab, enemyDefs[3], player.transform, 3);
            CreateSegment("F4_FactoryGauntlet4", new Vector3(434f, 0f, 0f), dronePrefab, enemyDefs[2], player.transform, 4);
            CreateCheckpoint("CP_F_PreBoss", new Vector3(442f, 1.5f, 0f), cpMgr);

            // 最终 Boss 竞技场
            var bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/MiniBoss_Walker.prefab");
            if (bossPrefab != null)
            {
                var boss = PrefabUtility.InstantiatePrefab(bossPrefab) as GameObject;
                boss.transform.position = new Vector3(455f, 1.5f, 0f);
                FixSpriteReferences(boss, bossPrefab);
                var bossComp = boss.GetComponent<MiniBossWalker>();
                if (bossComp != null) bossComp.AssignTarget(player.transform);
            }

            // 粒子生成器
            var particleGo = new GameObject("ParticleSpawner");
            particleGo.AddComponent<ParticleSpawner>();

            // 伤害数字生成器
            var dmgNumGo = new GameObject("DamageNumberSpawner");
            dmgNumGo.AddComponent<DamageNumberSpawner>();

            // 暂停菜单
            BuildPauseMenu();

            // 全局游戏循环驱动器（连击计时器、游戏时间统计）
            BuildGameLoop();

            // 音效管理器
            BuildAudioManager();

            // 背景音乐播放器
            BuildMusicPlayer(player.transform);

            // 武器与升级：分布在 6 个段落中，鼓励探索
            var shotgun = weapons[1];
            var rocket = weapons[2];
            var shotgunPickupPrefab = CreateWeaponPickupPrefab(shotgun, new Color(0.9f, 0.4f, 0.1f));
            var rocketPickupPrefab = CreateWeaponPickupPrefab(rocket, new Color(0.9f, 0.1f, 0.1f));

            // 武器拾取（关键节点）
            PlacePickup(shotgunPickupPrefab, new Vector3(35f, 1.5f, 0f), "Weapon_Shotgun_Beach");
            PlacePickup(rocketPickupPrefab, new Vector3(265f, 5f, 0f), "Weapon_Rocket_CityHigh");
            // 升级胶囊（每段一个主线 + 隐藏奖励）
            PlacePickup(upgradePickupPrefab, new Vector3(18f, 1.5f, 0f), "Upgrade_A_Beach");
            PlacePickup(upgradePickupPrefab, new Vector3(100f, 4.5f, 0f), "Upgrade_B_VillageHigh");
            PlacePickup(upgradePickupPrefab, new Vector3(180f, 4f, 0f), "Upgrade_C_TrenchHigh");
            PlacePickup(upgradePickupPrefab, new Vector3(285f, 3.5f, 0f), "Upgrade_D_City");
            PlacePickup(upgradePickupPrefab, new Vector3(345f, 4.5f, 0f), "Upgrade_E_IndustrialHigh");
            PlacePickup(upgradePickupPrefab, new Vector3(442f, 1.5f, 0f), "Upgrade_F_PreBoss");
            // 隐藏奖励（高处平台）
            PlacePickup(upgradePickupPrefab, new Vector3(51f, 5.5f, 0f), "Upgrade_Hidden_BeachPlatform");
            PlacePickup(upgradePickupPrefab, new Vector3(104f, 6f, 0f), "Upgrade_Hidden_VillagePlatform");
            PlacePickup(upgradePickupPrefab, new Vector3(201f, 6f, 0f), "Upgrade_Hidden_TrenchPlatform");
            PlacePickup(upgradePickupPrefab, new Vector3(276f, 6.5f, 0f), "Upgrade_Hidden_CityPlatform");
            PlacePickup(upgradePickupPrefab, new Vector3(368f, 6f, 0f), "Upgrade_Hidden_IndustrialPlatform");
            PlacePickup(upgradePickupPrefab, new Vector3(424f, 6.5f, 0f), "Upgrade_Hidden_FactoryPlatform");

            // 可破坏箱子（散布全图，提供掩体和掉落）
            PlacePickup(cratePrefab, new Vector3(8f, 1f, 0f), "Crate_A1");
            PlacePickup(cratePrefab, new Vector3(15f, 1f, 0f), "Crate_A2");
            PlacePickup(cratePrefab, new Vector3(45f, 1f, 0f), "Crate_A3");
            PlacePickup(cratePrefab, new Vector3(85f, 1f, 0f), "Crate_B1");
            PlacePickup(cratePrefab, new Vector3(120f, 1f, 0f), "Crate_B2");
            PlacePickup(cratePrefab, new Vector3(165f, 1f, 0f), "Crate_C1");
            PlacePickup(cratePrefab, new Vector3(200f, 1f, 0f), "Crate_C2");
            PlacePickup(cratePrefab, new Vector3(245f, 1f, 0f), "Crate_D1");
            PlacePickup(cratePrefab, new Vector3(288f, 1f, 0f), "Crate_D2");
            PlacePickup(cratePrefab, new Vector3(325f, 1f, 0f), "Crate_E1");
            PlacePickup(cratePrefab, new Vector3(380f, 1f, 0f), "Crate_E2");
            PlacePickup(cratePrefab, new Vector3(410f, 1f, 0f), "Crate_F1");

            // 健康药剂（每段至少一个，Boss 前密集）
            PlacePickup(healthPickupPrefab, new Vector3(25f, 1.5f, 0f), "Health_A1");
            PlacePickup(healthPickupPrefab, new Vector3(68f, 1.5f, 0f), "Health_A2");
            PlacePickup(healthPickupPrefab, new Vector3(118f, 1.5f, 0f), "Health_B1");
            PlacePickup(healthPickupPrefab, new Vector3(148f, 1.5f, 0f), "Health_B2");
            PlacePickup(healthPickupPrefab, new Vector3(195f, 1.5f, 0f), "Health_C1");
            PlacePickup(healthPickupPrefab, new Vector3(226f, 1.5f, 0f), "Health_C2");
            PlacePickup(healthPickupPrefab, new Vector3(262f, 1.5f, 0f), "Health_D1");
            PlacePickup(healthPickupPrefab, new Vector3(306f, 1.5f, 0f), "Health_D2");
            PlacePickup(healthPickupPrefab, new Vector3(355f, 1.5f, 0f), "Health_E1");
            PlacePickup(healthPickupPrefab, new Vector3(386f, 1.5f, 0f), "Health_E2");
            PlacePickup(healthPickupPrefab, new Vector3(442f, 1.5f, 0f), "Health_F_PreBoss1");
            PlacePickup(healthPickupPrefab, new Vector3(445f, 1.5f, 0f), "Health_F_PreBoss2");

            // 环境物件（掩体/路障/爆炸桶）— 成品游戏必备战术元素
            BuildEnvironmentProps();

            // 背景视差
            BuildParallaxBackground(camGo);

            // HUD
            BuildHud(player);

            // 受击方向指示器（必须在 HUD 之后，因为依赖 HUD_Canvas）
            BuildDamageIndicator(player.transform);

            // Game Over / Victory 屏幕
            BuildGameOverScreen();
            BuildVictoryScreen();

            // Boss 血条
            BuildBossHealthBar();

            // 成就系统
            BuildAchievementSystem();

            // 场景过渡
            BuildSceneFader();

            // 关卡结束触发器（Boss击败后玩家继续前进触发）
            var levelEnd = new GameObject("LevelEnd");
            var endCol = levelEnd.AddComponent<BoxCollider2D>();
            endCol.isTrigger = true;
            endCol.size = new Vector2(2f, 5f);
            var endRt = levelEnd.AddComponent<RectTransform>();
            endRt.anchoredPosition = new Vector2(465f, 2f);
            levelEnd.AddComponent<LevelEndTrigger>();
            levelEnd.transform.position = new Vector3(465f, 2f, 0f);

            // 教程提示
            BuildTutorialPrompts();

            // 叙事管理器（关卡开场/Boss警告/结局过场）
            BuildStoryManager();

            // 交互式 FTUE 教学管理器
            BuildTutorialManager();

            // 对象池（子弹/爆炸特效/技能特效）
            BuildObjectPool(projectile);

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[VerticalSliceBuilder] Level01 scene saved.");
        }

        /// <summary>
        /// 构建叙事管理器：配置关卡开场、Boss警告、结局过场文本。
        /// </summary>
        private static void BuildStoryManager()
        {
            var go = new GameObject("StoryManager");
            var story = go.AddComponent<StoryManager>();

            // 创建叙事 UI 面板
            var canvasGo = new GameObject("StoryCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // 半透明背景面板
            var panelGo = new GameObject("StoryPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            var panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.75f);
            var panelRt = panelImg.rectTransform;
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var canvasGroup = panelGo.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            // 说话者名字
            var speakerText = CreateHudText(panelGo, "SpeakerText", new Vector2(0, 80), "", 28, new Color(1f, 0.85f, 0.2f, 1f), FontStyle.Bold);
            speakerText.rectTransform.anchorMin = new Vector2(0.2f, 0.6f);
            speakerText.rectTransform.anchorMax = new Vector2(0.8f, 0.7f);
            speakerText.alignment = TextAnchor.MiddleCenter;

            // 正文
            var bodyText = CreateHudText(panelGo, "BodyText", new Vector2(0, 0), "", 22, Color.white, FontStyle.Normal);
            bodyText.rectTransform.anchorMin = new Vector2(0.15f, 0.3f);
            bodyText.rectTransform.anchorMax = new Vector2(0.85f, 0.55f);
            bodyText.alignment = TextAnchor.MiddleCenter;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;

            var so = new SerializedObject(story);
            so.FindProperty("storyPanel").objectReferenceValue = panelGo;
            so.FindProperty("speakerText").objectReferenceValue = speakerText;
            so.FindProperty("bodyText").objectReferenceValue = bodyText;
            so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;

            // 关卡开场叙事
            var introProp = so.FindProperty("introBeats");
            introProp.arraySize = 2;
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Aila";
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "铁雨事变第37天。我们四人小队终于推进到了敌方海岸线。";
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 4f;
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Aila";
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "目标：穿越六段战区，摧毁工厂核心。全员，准备登陆。";
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            // Boss 警告叙事
            var bossProp = so.FindProperty("bossWarningBeats");
            bossProp.arraySize = 2;
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Mara";
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "前方检测到大型机甲信号...是四足侦察机甲！";
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 3.5f;
            bossProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Bruno";
            bossProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "它的核心在背部，等它跳跃砸地时暴露！准备战斗！";
            bossProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            // 关卡结局叙事
            var outroProp = so.FindProperty("outroBeats");
            outroProp.arraySize = 2;
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Niko";
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "时间线正在收敛...工厂核心已摧毁。";
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 4f;
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Aila";
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "干得漂亮，小队。但这只是开始。下一站：工厂内部。";
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            so.ApplyModifiedProperties();
        }

        /// <summary>
        /// 构建交互式 FTUE 教学管理器：分步引导玩家完成基础操作。
        /// </summary>
        private static void BuildTutorialManager()
        {
            var go = new GameObject("TutorialManager");
            var tutorial = go.AddComponent<TutorialManager>();

            // 复用 StoryCanvas 的层级创建独立教学面板
            var canvasGo = new GameObject("TutorialCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var panelGo = new GameObject("TutorialPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            var panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.6f);
            var panelRt = panelImg.rectTransform;
            panelRt.anchorMin = new Vector2(0.2f, 0.05f);
            panelRt.anchorMax = new Vector2(0.8f, 0.25f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var canvasGroup = panelGo.AddComponent<CanvasGroup>();

            var instructionText = CreateHudText(panelGo, "InstructionText", new Vector2(0, 15), "", 20, Color.white, FontStyle.Bold);
            instructionText.rectTransform.anchorMin = new Vector2(0.05f, 0.4f);
            instructionText.rectTransform.anchorMax = new Vector2(0.95f, 0.9f);
            instructionText.alignment = TextAnchor.MiddleCenter;
            instructionText.horizontalOverflow = HorizontalWrapMode.Wrap;

            var progressText = CreateHudText(panelGo, "ProgressText", new Vector2(0, -15), "0/1", 16, new Color(1f, 0.85f, 0.2f, 1f), FontStyle.Normal);
            progressText.rectTransform.anchorMin = new Vector2(0.4f, 0.05f);
            progressText.rectTransform.anchorMax = new Vector2(0.6f, 0.35f);
            progressText.alignment = TextAnchor.MiddleCenter;

            var so = new SerializedObject(tutorial);
            so.FindProperty("tutorialPanel").objectReferenceValue = panelGo;
            so.FindProperty("instructionText").objectReferenceValue = instructionText;
            so.FindProperty("progressText").objectReferenceValue = progressText;
            so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;

            // 配置教学步骤：移动→射击→角色切换→武器形态→技能
            var stepsProp = so.FindProperty("steps");
            stepsProp.arraySize = 5;

            // 步骤1：移动（按A/D/方向键3次）
            var s0 = stepsProp.GetArrayElementAtIndex(0);
            s0.FindPropertyRelative("id").stringValue = "move";
            s0.FindPropertyRelative("instruction").stringValue = "按 A/D 或方向键移动（执行3次）";
            s0.FindPropertyRelative("requiredKey").enumValueIndex = (int)KeyCode.D;
            s0.FindPropertyRelative("requiredActionCount").intValue = 3;
            s0.FindPropertyRelative("waitForTrigger").boolValue = false;

            // 步骤2：射击（按J 3次）
            var s1 = stepsProp.GetArrayElementAtIndex(1);
            s1.FindPropertyRelative("id").stringValue = "shoot";
            s1.FindPropertyRelative("instruction").stringValue = "按 J 或鼠标左键射击（射击3次）";
            s1.FindPropertyRelative("requiredKey").enumValueIndex = (int)KeyCode.J;
            s1.FindPropertyRelative("requiredActionCount").intValue = 3;
            s1.FindPropertyRelative("waitForTrigger").boolValue = false;

            // 步骤3：角色切换（按2）
            var s2 = stepsProp.GetArrayElementAtIndex(2);
            s2.FindPropertyRelative("id").stringValue = "switch";
            s2.FindPropertyRelative("instruction").stringValue = "按 2 切换到 Bruno（重装防御者）";
            s2.FindPropertyRelative("requiredKey").enumValueIndex = (int)KeyCode.Alpha2;
            s2.FindPropertyRelative("requiredActionCount").intValue = 1;
            s2.FindPropertyRelative("waitForTrigger").boolValue = false;

            // 步骤4：武器形态切换（按E）
            var s3 = stepsProp.GetArrayElementAtIndex(3);
            s3.FindPropertyRelative("id").stringValue = "weapon_form";
            s3.FindPropertyRelative("instruction").stringValue = "按 E 切换武器形态（每种形态有不同攻击模式）";
            s3.FindPropertyRelative("requiredKey").enumValueIndex = (int)KeyCode.E;
            s3.FindPropertyRelative("requiredActionCount").intValue = 1;
            s3.FindPropertyRelative("waitForTrigger").boolValue = false;

            // 步骤5：闪避（按LeftShift）
            var s4 = stepsProp.GetArrayElementAtIndex(4);
            s4.FindPropertyRelative("id").stringValue = "dodge";
            s4.FindPropertyRelative("instruction").stringValue = "按 Left Shift 闪避（闪避期间无敌）";
            s4.FindPropertyRelative("requiredKey").enumValueIndex = (int)KeyCode.LeftShift;
            s4.FindPropertyRelative("requiredActionCount").intValue = 1;
            s4.FindPropertyRelative("waitForTrigger").boolValue = false;

            so.ApplyModifiedProperties();
        }

        /// <summary>
        /// 构建对象池：预分配子弹、爆炸特效等高频对象。
        /// </summary>
        private static void BuildObjectPool(Projectile projectilePrefab)
        {
            var go = new GameObject("[ObjectPool]");
            var pool = go.AddComponent<ObjectPool>();

            var so = new SerializedObject(pool);
            var poolsProp = so.FindProperty("pools");
            poolsProp.arraySize = 1;

            // 子弹池
            var p0 = poolsProp.GetArrayElementAtIndex(0);
            p0.FindPropertyRelative("id").stringValue = "projectile";
            p0.FindPropertyRelative("prefab").objectReferenceValue = projectilePrefab != null ? projectilePrefab.gameObject : null;
            p0.FindPropertyRelative("initialSize").intValue = 30;
            p0.FindPropertyRelative("maxSize").intValue = 200;

            so.ApplyModifiedProperties();
            // ObjectPool.Awake 会自动初始化池
        }

        private static void BuildTutorialPrompts()
        {
            // 教程提示文本（屏幕中央上方）
            var tutorialCanvasGo = new GameObject("TutorialCanvas");
            var tutorialCanvas = tutorialCanvasGo.AddComponent<Canvas>();
            tutorialCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var tutorialScaler = tutorialCanvasGo.AddComponent<CanvasScaler>();
            tutorialScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            tutorialScaler.referenceResolution = new Vector2(1920, 1080);
            tutorialCanvasGo.AddComponent<GraphicRaycaster>();

            var tutorialText = CreateHudText(tutorialCanvasGo, "TutorialText", new Vector2(0, 100), "", 24, SteelRain.UI.UIPalette.Warning, FontStyle.Bold);
            tutorialText.rectTransform.anchorMin = new Vector2(0.2f, 0.7f);
            tutorialText.rectTransform.anchorMax = new Vector2(0.8f, 0.85f);
            tutorialText.alignment = TextAnchor.MiddleCenter;
            tutorialText.enabled = false;

            // 教程触发器1：移动教学
            CreateTutorialTrigger(new Vector3(3f, 1.5f, 0f), "Tutorial_Move",
                "Use A/D or Arrow Keys to move. Press SPACE to jump.",
                tutorialText);

            // 教程触发器2：射击教学
            CreateTutorialTrigger(new Vector3(6f, 1.5f, 0f), "Tutorial_Shoot",
                "Left Click or J to shoot. Destroy crates for pickups.",
                tutorialText);

            // 教程触发器3：角色切换教学
            CreateTutorialTrigger(new Vector3(12f, 1.5f, 0f), "Tutorial_Switch",
                "Press 1/2/3/4 to switch characters. Each has unique skills.",
                tutorialText);

            // 教程触发器4：技能教学
            CreateTutorialTrigger(new Vector3(20f, 1.5f, 0f), "Tutorial_Skill",
                "Press Q or Right Click to use character skill (requires Lv3 weapon).",
                tutorialText);

            // 教程触发器5：闪避教学
            CreateTutorialTrigger(new Vector3(30f, 1.5f, 0f), "Tutorial_Dodge",
                "Press Left Shift to dodge. Time it right to avoid damage.",
                tutorialText);

            // 教程触发器6：武器形态切换
            CreateTutorialTrigger(new Vector3(45f, 1.5f, 0f), "Tutorial_WeaponForm",
                "Press E to cycle weapon forms. Each form has different attack patterns.",
                tutorialText);
        }

        private static void CreateTutorialTrigger(Vector3 pos, string name, string message, Text targetText)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 5f);
            var prompt = go.AddComponent<TutorialPrompt>();
            var so = new SerializedObject(prompt);
            so.FindProperty("promptText").objectReferenceValue = targetText;
            so.FindProperty("message").stringValue = message;
            so.FindProperty("displayDuration").floatValue = 5f;
            so.ApplyModifiedProperties();
        }

        private static void BuildAchievementSystem()
        {
            // 成就跟踪器
            var trackerGo = new GameObject("AchievementTracker");
            trackerGo.AddComponent<SteelRain.UI.AchievementTracker>();

            // 成就通知UI
            var notifGo = new GameObject("AchievementNotification");
            notifGo.AddComponent<Canvas>();
            notifGo.AddComponent<SteelRain.UI.AchievementNotification>();
        }

        private static void BuildSceneFader()
        {
            var go = new GameObject("SceneFader");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            go.AddComponent<GraphicRaycaster>();

            var imgGo = new GameObject("FadeImage");
            imgGo.transform.SetParent(go.transform);
            var img = imgGo.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
            img.raycastTarget = false;
            var imgRt = imgGo.GetComponent<RectTransform>();
            imgRt.anchorMin = Vector2.zero;
            imgRt.anchorMax = Vector2.one;
            imgRt.offsetMin = Vector2.zero;
            imgRt.offsetMax = Vector2.zero;

            var fader = go.AddComponent<SceneFader>();
            var so = new SerializedObject(fader);
            so.FindProperty("fadeImage").objectReferenceValue = img;
            so.ApplyModifiedProperties();
        }

        private static void BuildLevel02Scene(GameObject playerPrefab, GameObject enemyPrefab,
            EnemyDefinition[] enemyDefs, Projectile projectilePrefab,
            GameObject upgradePickupPrefab, GameObject cratePrefab,
            GameObject healthPickupPrefab, CharacterDefinition[] characters,
            GameObject shieldPrefab, GameObject dronePrefab, GameObject grenadierPrefab,
            WeaponDefinition[] weapons,
            GameObject chargerPrefab = null, GameObject sniperPrefab = null, GameObject heavyGunnerPrefab = null)
        {
            Debug.Log("[VerticalSliceBuilder] Building Level02 scene...");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // EventSystem（UI按钮点击必需）
            CreateEventSystem();

            var camGo = new GameObject("MainCamera");
            camGo.transform.position = new Vector3(0, 0, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.15f, 0.08f, 0.05f);
            camGo.tag = "MainCamera";
            camGo.AddComponent<CameraShake>();
            var camFollow = camGo.AddComponent<SimpleCameraFollow>();
            // Level02 长度约 165 单位，相机限制需匹配关卡长度
            var soCamL2 = new SerializedObject(camFollow);
            soCamL2.FindProperty("maxX").floatValue = 170f;
            soCamL2.ApplyModifiedProperties();

            var groundLayer = 6;
            var groundGo = new GameObject("Ground");
            BuildFactoryGround(groundGo, groundLayer);

            GameObject player = null;
            if (playerPrefab != null)
            {
                player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                if (player != null)
                    EditorSceneManager.MoveGameObjectToScene(player, scene);
            }
            if (player == null)
            {
                Debug.LogWarning("[BuildLevel02] PrefabUtility.InstantiatePrefab returned null, recreating");
                player = new GameObject("Player_Aila");
                var srcSr = playerPrefab != null ? playerPrefab.GetComponentInChildren<SpriteRenderer>(true) : null;
                if (srcSr != null && srcSr.sprite != null)
                {
                    var newSr = player.AddComponent<SpriteRenderer>();
                    newSr.sprite = srcSr.sprite;
                    newSr.sortingOrder = 5;
                }
                player.AddComponent<Rigidbody2D>();
                player.AddComponent<BoxCollider2D>();
                player.AddComponent<Health>();
                player.AddComponent<PlayerController2D>();
                player.AddComponent<PlayerCombat>();
                EditorSceneManager.MoveGameObjectToScene(player, scene);
            }
            player.name = "Player_Aila";
            player.transform.position = new Vector3(0f, 2f, 0f);
            player.tag = "Player";
            FixSpriteReferences(player, playerPrefab);
            var controller = player.GetComponent<PlayerController2D>();
            var combat = player.GetComponent<PlayerCombat>();
            Debug.Log($"[BuildLevel02] Player created at {player.transform.position}, scene={player.scene.name}, sceneRootCount={scene.rootCount}");

            var soCam = new SerializedObject(camFollow);
            soCam.FindProperty("target").objectReferenceValue = player.transform;
            soCam.ApplyModifiedProperties();

            var cpMgrGo = new GameObject("CheckpointManager");
            var cpMgr = cpMgrGo.AddComponent<CheckpointManager>();
            var soCp = new SerializedObject(cpMgr);
            soCp.FindProperty("player").objectReferenceValue = player.transform;
            soCp.FindProperty("fallbackSpawn").vector3Value = new Vector3(0f, 2f, 0f);
            soCp.ApplyModifiedProperties();

            CreateSegment("L2_A1", new Vector3(10f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 3);
            CreateSegment("L2_A2", new Vector3(25f, 0f, 0f), shieldPrefab, enemyDefs[1], player.transform, 2);
            CreateSegment("L2_A3", new Vector3(40f, 0f, 0f), dronePrefab, enemyDefs[2], player.transform, 3);
            CreateCheckpoint("L2_CP_A", new Vector3(50f, 1.5f, 0f), cpMgr);

            CreateSegment("L2_B1", new Vector3(60f, 0f, 0f), grenadierPrefab, enemyDefs[3], player.transform, 2);
            CreateSegment("L2_B2", new Vector3(75f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 4);
            CreateSegment("L2_B3", new Vector3(90f, 0f, 0f), shieldPrefab, enemyDefs[1], player.transform, 3);
            CreateCheckpoint("L2_CP_B", new Vector3(100f, 1.5f, 0f), cpMgr);

            CreateSegment("L2_C1", new Vector3(110f, 0f, 0f), dronePrefab, enemyDefs[2], player.transform, 4);
            CreateSegment("L2_C2", new Vector3(125f, 0f, 0f), grenadierPrefab, enemyDefs[3], player.transform, 3);
            CreateCheckpoint("L2_CP_C", new Vector3(135f, 1.5f, 0f), cpMgr);

            // 环境机关
            CreateHazard(new Vector3(65f, 0.5f, 0f), "Spike_1");
            CreateHazard(new Vector3(95f, 0.5f, 0f), "Spike_2");
            CreateBarrel(new Vector3(45f, 0.5f, 0f), "Barrel_1");
            CreateBarrel(new Vector3(110f, 0.5f, 0f), "Barrel_2");

            // 移动平台
            CreateMovingPlatform(new Vector3(30f, 2.5f, 0f), new Vector2(0, 0), new Vector2(5f, 0), "MovePlat_1");
            CreateMovingPlatform(new Vector3(85f, 3f, 0f), new Vector2(0, 0), new Vector2(4f, 0), "MovePlat_2");

            // 碎裂平台
            CreateCrumblingPlatform(new Vector3(70f, 3f, 0f), "Crumble_1");

            var bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/TurretBoss.prefab");
            if (bossPrefab != null)
            {
                var boss = PrefabUtility.InstantiatePrefab(bossPrefab) as GameObject;
                boss.transform.position = new Vector3(155f, 2f, 0f);
                var bossComp = boss.GetComponent<TurretBoss>();
                if (bossComp != null) bossComp.AssignTarget(player.transform);
            }

            var particleGo = new GameObject("ParticleSpawner");
            particleGo.AddComponent<ParticleSpawner>();

            var dmgNumGo = new GameObject("DamageNumberSpawner");
            dmgNumGo.AddComponent<DamageNumberSpawner>();

            BuildPauseMenu();
            BuildAudioManager();
            BuildMusicPlayerBossOnly(player.transform);
            PlacePickup(upgradePickupPrefab, new Vector3(20f, 1.5f, 0f), "L2_Upgrade_1");
            PlacePickup(upgradePickupPrefab, new Vector3(70f, 1.5f, 0f), "L2_Upgrade_2");
            PlacePickup(cratePrefab, new Vector3(30f, 1f, 0f), "L2_Crate_1");
            PlacePickup(healthPickupPrefab, new Vector3(55f, 1.5f, 0f), "L2_Health_1");
            PlacePickup(healthPickupPrefab, new Vector3(115f, 1.5f, 0f), "L2_Health_2");

            // Level02 武器拾取：火箭筒在工厂深处
            if (weapons != null && weapons.Length >= 3)
            {
                var rocket = weapons[2];
                var rocketPickupPrefab = CreateWeaponPickupPrefab(rocket, new Color(0.9f, 0.1f, 0.1f));
                PlacePickup(rocketPickupPrefab, new Vector3(100f, 1.5f, 0f), "L2_Weapon_Rocket");
            }

            // Level02 环境物件：工厂段掩体和障碍
            BuildL2EnvironmentProps();

            // 背景视差
            BuildParallaxBackground(camGo);

            // HUD
            BuildHud(player);

            // 受击方向指示器（必须在 HUD 之后，因为依赖 HUD_Canvas）
            BuildDamageIndicator(player.transform);

            // Game Over / Victory 屏幕
            BuildGameOverScreen();
            BuildVictoryScreen();

            // Boss 血条
            BuildBossHealthBar();

            // 成就系统
            BuildAchievementSystem();

            // 场景过渡
            BuildSceneFader();

            var levelEnd = new GameObject("LevelEnd");
            var endCol = levelEnd.AddComponent<BoxCollider2D>();
            endCol.isTrigger = true;
            endCol.size = new Vector2(2f, 5f);
            var endRt = levelEnd.AddComponent<RectTransform>();
            endRt.anchoredPosition = new Vector2(165f, 2f);
            levelEnd.AddComponent<LevelEndTrigger>();
            levelEnd.transform.position = new Vector3(165f, 2f, 0f);

            // Level02 也添加对象池和叙事管理器
            BuildObjectPool(projectilePrefab);
            BuildStoryManagerLevel02();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Level02_Factory.unity");
            Debug.Log("[VerticalSliceBuilder] Level02 scene saved.");
        }

        /// <summary>
        /// Level02 专用叙事配置（工厂内部）。
        /// </summary>
        private static void BuildStoryManagerLevel02()
        {
            var go = new GameObject("StoryManager");
            var story = go.AddComponent<StoryManager>();

            var canvasGo = new GameObject("StoryCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            var panelGo = new GameObject("StoryPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            var panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.75f);
            var panelRt = panelImg.rectTransform;
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var canvasGroup = panelGo.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            var speakerText = CreateHudText(panelGo, "SpeakerText", new Vector2(0, 80), "", 28, new Color(1f, 0.85f, 0.2f, 1f), FontStyle.Bold);
            speakerText.rectTransform.anchorMin = new Vector2(0.2f, 0.6f);
            speakerText.rectTransform.anchorMax = new Vector2(0.8f, 0.7f);
            speakerText.alignment = TextAnchor.MiddleCenter;

            var bodyText = CreateHudText(panelGo, "BodyText", new Vector2(0, 0), "", 22, Color.white, FontStyle.Normal);
            bodyText.rectTransform.anchorMin = new Vector2(0.15f, 0.3f);
            bodyText.rectTransform.anchorMax = new Vector2(0.85f, 0.55f);
            bodyText.alignment = TextAnchor.MiddleCenter;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;

            var so = new SerializedObject(story);
            so.FindProperty("storyPanel").objectReferenceValue = panelGo;
            so.FindProperty("speakerText").objectReferenceValue = speakerText;
            so.FindProperty("bodyText").objectReferenceValue = bodyText;
            so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;

            var introProp = so.FindProperty("introBeats");
            introProp.arraySize = 2;
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Aila";
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "我们进入工厂内部了。这里布满了自动化防御系统。";
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 4f;
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Niko";
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "检测到深处有炮塔Boss...它的三段攻击模式很危险。";
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            var bossProp = so.FindProperty("bossWarningBeats");
            bossProp.arraySize = 1;
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Mara";
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "炮塔Boss激活了！注意它的散射和追踪导弹！";
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 3.5f;

            var outroProp = so.FindProperty("outroBeats");
            outroProp.arraySize = 2;
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Bruno";
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "炮塔核心已摧毁。工厂防御系统瘫痪。";
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 4f;
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Aila";
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "任务完成。小队，撤离。我们为这场战争赢得了关键的一步。";
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            so.ApplyModifiedProperties();
        }

        private static void CreateHazard(Vector3 pos, string name)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/ground_trench.png");
            sr.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
            sr.sortingOrder = 2;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 0.3f);
            go.AddComponent<SpikeHazard>();
        }

        private static void CreateBarrel(Vector3 pos, string name)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/crate.png");
            sr.color = new Color(0.9f, 0.3f, 0.1f, 1f);
            sr.sortingOrder = 3;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.7f, 0.9f);
            go.AddComponent<Health>();
            go.AddComponent<ExplosiveBarrel>();
        }

        private static void CreateMovingPlatform(Vector3 pos, Vector2 pointA, Vector2 pointB, string name)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/ground_beach.png");
            sr.color = new Color(0.5f, 0.5f, 0.6f, 1f);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(3f, 0.5f);
            sr.sortingOrder = 1;
            go.layer = 6;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3f, 0.5f);
            var mp = go.AddComponent<MovingPlatform>();
            var so = new SerializedObject(mp);
            so.FindProperty("pointA").vector2Value = pointA;
            so.FindProperty("pointB").vector2Value = pointB;
            so.ApplyModifiedProperties();
        }

        private static void CreateCrumblingPlatform(Vector3 pos, string name)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/ground_village.png");
            sr.color = new Color(0.6f, 0.4f, 0.25f, 1f);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(2f, 0.5f);
            sr.sortingOrder = 1;
            go.layer = 6;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 0.5f);
            go.AddComponent<CrumblingPlatform>();
        }

        private static void BuildFactoryGround(GameObject parent, int layer)
        {
            var groundSprite = LoadOrImportSprite($"{ArtDir}/ground_village.png");
            float startX = -5f;
            float endX = 165f;
            float groundY = 0f;

            for (float x = startX; x < endX; x += 1f)
            {
                var block = new GameObject($"Ground_{x}");
                block.transform.SetParent(parent.transform);
                block.transform.position = new Vector3(x, groundY - 0.5f, 0f);
                var sr = block.AddComponent<SpriteRenderer>();
                sr.sprite = groundSprite;
                sr.sortingOrder = 0;
                sr.color = new Color(0.5f, 0.35f, 0.25f, 1f);
                block.layer = layer;
                var col = block.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 1f);
            }

            CreatePlatform(parent, 15f, 3f, 3f, groundSprite, layer);
            CreatePlatform(parent, 35f, 2.5f, 4f, groundSprite, layer);
            CreatePlatform(parent, 55f, 4f, 3f, groundSprite, layer);
            CreatePlatform(parent, 80f, 3f, 5f, groundSprite, layer);
            CreatePlatform(parent, 105f, 3.5f, 4f, groundSprite, layer);
            CreatePlatform(parent, 130f, 2.5f, 3f, groundSprite, layer);
        }

        private static void BuildGround(GameObject parent, int layer)
        {
            // 成品级长关卡：6 个 biome 段落，总长约 460 单位
            // A: 海滩 (-5 ~ 70)   B: 村庄 (70 ~ 150)   C: 壕沟 (150 ~ 230)
            // D: 城市 (230 ~ 310)  E: 工业 (310 ~ 390)  F: 工厂 (390 ~ 460)
            BuildGroundSegment(parent, layer, -5f, 70f, "ground_beach", "Ground_Beach");
            BuildGroundSegment(parent, layer, 70f, 150f, "ground_village", "Ground_Village");
            BuildGroundSegment(parent, layer, 150f, 230f, "ground_trench", "Ground_Trench");
            BuildGroundSegment(parent, layer, 230f, 310f, "ground_city", "Ground_City");
            BuildGroundSegment(parent, layer, 310f, 390f, "ground_industrial", "Ground_Industrial");
            BuildGroundSegment(parent, layer, 390f, 460f, "ground_factory", "Ground_Factory");

            // 多层平台提供垂直性（合金弹头风格：3层结构）
            // 跳跃水平距离4-7单位，平台间距≤5，高度差≤2.0，确保可达
            // 海滩段 — 阶梯式上升
            CreatePlatform(parent, 10f, 1.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_beach.png"), layer);
            CreatePlatform(parent, 15f, 3.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_beach.png"), layer);
            CreatePlatform(parent, 22f, 1.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_beach.png"), layer);
            CreatePlatform(parent, 27f, 3.5f, 4f, LoadOrImportSprite($"{ArtDir}/ground_beach.png"), layer);
            CreatePlatform(parent, 34f, 2.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_beach.png"), layer);
            CreatePlatform(parent, 39f, 4.0f, 4f, LoadOrImportSprite($"{ArtDir}/ground_beach.png"), layer);
            CreatePlatform(parent, 46f, 2.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_beach.png"), layer);
            CreatePlatform(parent, 51f, 4.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_beach.png"), layer);
            CreatePlatform(parent, 58f, 2.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_beach.png"), layer);
            CreatePlatform(parent, 63f, 3.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_beach.png"), layer);
            // 村庄段 — 屋顶跳跃
            CreatePlatform(parent, 75f, 2.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_village.png"), layer);
            CreatePlatform(parent, 80f, 4.0f, 4f, LoadOrImportSprite($"{ArtDir}/ground_village.png"), layer);
            CreatePlatform(parent, 87f, 2.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_village.png"), layer);
            CreatePlatform(parent, 92f, 4.5f, 4f, LoadOrImportSprite($"{ArtDir}/ground_village.png"), layer);
            CreatePlatform(parent, 99f, 2.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_village.png"), layer);
            CreatePlatform(parent, 104f, 5.0f, 4f, LoadOrImportSprite($"{ArtDir}/ground_village.png"), layer);
            CreatePlatform(parent, 111f, 3.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_village.png"), layer);
            CreatePlatform(parent, 116f, 5.0f, 4f, LoadOrImportSprite($"{ArtDir}/ground_village.png"), layer);
            CreatePlatform(parent, 123f, 2.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_village.png"), layer);
            CreatePlatform(parent, 128f, 4.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_village.png"), layer);
            CreatePlatform(parent, 135f, 2.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_village.png"), layer);
            CreatePlatform(parent, 140f, 4.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_village.png"), layer);
            // 壕沟段 — 上下起伏
            CreatePlatform(parent, 155f, 2.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_trench.png"), layer);
            CreatePlatform(parent, 160f, 1.5f, 4f, LoadOrImportSprite($"{ArtDir}/ground_trench.png"), layer);
            CreatePlatform(parent, 167f, 3.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_trench.png"), layer);
            CreatePlatform(parent, 172f, 1.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_trench.png"), layer);
            CreatePlatform(parent, 177f, 4.0f, 4f, LoadOrImportSprite($"{ArtDir}/ground_trench.png"), layer);
            CreatePlatform(parent, 184f, 2.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_trench.png"), layer);
            CreatePlatform(parent, 189f, 4.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_trench.png"), layer);
            CreatePlatform(parent, 196f, 2.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_trench.png"), layer);
            CreatePlatform(parent, 201f, 5.0f, 4f, LoadOrImportSprite($"{ArtDir}/ground_trench.png"), layer);
            CreatePlatform(parent, 208f, 2.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_trench.png"), layer);
            CreatePlatform(parent, 213f, 4.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_trench.png"), layer);
            CreatePlatform(parent, 220f, 2.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_trench.png"), layer);
            CreatePlatform(parent, 225f, 4.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_trench.png"), layer);
            // 城市段 — 高楼平台
            CreatePlatform(parent, 235f, 2.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_city.png"), layer);
            CreatePlatform(parent, 240f, 4.0f, 4f, LoadOrImportSprite($"{ArtDir}/ground_city.png"), layer);
            CreatePlatform(parent, 247f, 2.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_city.png"), layer);
            CreatePlatform(parent, 252f, 4.5f, 4f, LoadOrImportSprite($"{ArtDir}/ground_city.png"), layer);
            CreatePlatform(parent, 259f, 2.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_city.png"), layer);
            CreatePlatform(parent, 264f, 5.0f, 4f, LoadOrImportSprite($"{ArtDir}/ground_city.png"), layer);
            CreatePlatform(parent, 271f, 3.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_city.png"), layer);
            CreatePlatform(parent, 276f, 5.5f, 4f, LoadOrImportSprite($"{ArtDir}/ground_city.png"), layer);
            CreatePlatform(parent, 283f, 2.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_city.png"), layer);
            CreatePlatform(parent, 288f, 4.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_city.png"), layer);
            CreatePlatform(parent, 295f, 2.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_city.png"), layer);
            CreatePlatform(parent, 300f, 4.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_city.png"), layer);
            // 工业段 — 管道平台
            CreatePlatform(parent, 315f, 2.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_industrial.png"), layer);
            CreatePlatform(parent, 320f, 4.5f, 4f, LoadOrImportSprite($"{ArtDir}/ground_industrial.png"), layer);
            CreatePlatform(parent, 327f, 2.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_industrial.png"), layer);
            CreatePlatform(parent, 332f, 4.0f, 4f, LoadOrImportSprite($"{ArtDir}/ground_industrial.png"), layer);
            CreatePlatform(parent, 339f, 2.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_industrial.png"), layer);
            CreatePlatform(parent, 344f, 5.0f, 4f, LoadOrImportSprite($"{ArtDir}/ground_industrial.png"), layer);
            CreatePlatform(parent, 351f, 2.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_industrial.png"), layer);
            CreatePlatform(parent, 356f, 4.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_industrial.png"), layer);
            CreatePlatform(parent, 363f, 2.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_industrial.png"), layer);
            CreatePlatform(parent, 368f, 5.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_industrial.png"), layer);
            CreatePlatform(parent, 375f, 2.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_industrial.png"), layer);
            CreatePlatform(parent, 380f, 4.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_industrial.png"), layer);
            // 工厂段（Boss 竞技场多层平台）
            CreatePlatform(parent, 395f, 2.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_factory.png"), layer);
            CreatePlatform(parent, 400f, 4.0f, 4f, LoadOrImportSprite($"{ArtDir}/ground_factory.png"), layer);
            CreatePlatform(parent, 407f, 2.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_factory.png"), layer);
            CreatePlatform(parent, 412f, 5.0f, 4f, LoadOrImportSprite($"{ArtDir}/ground_factory.png"), layer);
            CreatePlatform(parent, 419f, 3.0f, 3f, LoadOrImportSprite($"{ArtDir}/ground_factory.png"), layer);
            CreatePlatform(parent, 424f, 5.5f, 5f, LoadOrImportSprite($"{ArtDir}/ground_factory.png"), layer);
            CreatePlatform(parent, 431f, 3.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_factory.png"), layer);
            CreatePlatform(parent, 436f, 5.5f, 4f, LoadOrImportSprite($"{ArtDir}/ground_factory.png"), layer);
            CreatePlatform(parent, 443f, 2.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_factory.png"), layer);
            CreatePlatform(parent, 448f, 4.5f, 3f, LoadOrImportSprite($"{ArtDir}/ground_factory.png"), layer);
        }

        /// <summary>
        /// 构建指定 biome 的地面段
        /// </summary>
        private static void BuildGroundSegment(GameObject parent, int layer, float startX, float endX,
            string spriteName, string segmentName)
        {
            var groundSprite = LoadOrImportSprite($"{ArtDir}/{spriteName}.png");
            float groundY = 0f;

            var segParent = new GameObject(segmentName);
            segParent.transform.SetParent(parent.transform);

            for (float x = startX; x < endX; x += 1f)
            {
                var block = new GameObject($"Tile_{segmentName}_{x}");
                block.transform.SetParent(segParent.transform);
                block.transform.position = new Vector3(x, groundY - 0.5f, 0f);
                var sr = block.AddComponent<SpriteRenderer>();
                sr.sprite = groundSprite;
                sr.sortingOrder = 0;
                block.layer = layer;

                var col = block.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 1f);
            }
        }

        private static void CreatePlatform(GameObject parent, float x, float y, float width,
            Sprite sprite, int layer)
        {
            var platform = new GameObject($"Platform_{x}");
            platform.transform.SetParent(parent.transform);
            platform.transform.position = new Vector3(x, y, 0f);
            var sr = platform.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(width, 1f);
            sr.sortingOrder = 0;
            platform.layer = layer;

            var col = platform.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, 0.5f);
        }

        /// <summary>
        /// 修复SpriteRenderer引用丢失问题（Unity 6+ PrefabUtility.InstantiatePrefab已知问题）
        /// 从prefab重新复制sprite引用
        /// </summary>
        private static void FixSpriteReferences(GameObject instance, GameObject prefab)
        {
            if (instance == null || prefab == null) return;
            var instSrs = instance.GetComponentsInChildren<SpriteRenderer>(true);
            var prefSrs = prefab.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var inst in instSrs)
            {
                if (inst.sprite != null) continue;
                // 按transform路径匹配
                var instPath = GetTransformPath(inst.transform, instance.transform);
                foreach (var pref in prefSrs)
                {
                    if (GetTransformPath(pref.transform, prefab.transform) == instPath)
                    {
                        inst.sprite = pref.sprite;
                        inst.color = pref.color;
                        inst.sortingOrder = pref.sortingOrder;
                        inst.flipX = pref.flipX;
                        inst.flipY = pref.flipY;
                        break;
                    }
                }
            }
        }

        private static string GetTransformPath(Transform t, Transform root)
        {
            if (t == root) return "";
            return GetTransformPath(t.parent, root) + "/" + t.name;
        }

        private static void CreateSegment(string name, Vector3 pos, GameObject enemyPrefab,
            EnemyDefinition def, Transform player, int count)
        {
            var triggerGo = new GameObject(name);
            triggerGo.transform.position = pos;
            var triggerCol = triggerGo.AddComponent<BoxCollider2D>();
            triggerCol.isTrigger = true;
            triggerCol.size = new Vector2(2f, 5f);
            var segment = triggerGo.AddComponent<LevelSegmentTrigger>();

            var so = new SerializedObject(segment);
            so.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;

            var offsetArray = so.FindProperty("spawnOffsets");
            offsetArray.arraySize = count;
            for (int i = 0; i < count; i++)
                offsetArray.GetArrayElementAtIndex(i).vector2Value = new Vector2(5f + i * 2f, 1f);

            so.FindProperty("player").objectReferenceValue = player;
            so.FindProperty("spawnCount").intValue = count;

            // 设置敌人子弹预制体（供无人机/手雷兵使用）
            var proj = AssetDatabase.LoadAssetAtPath<Projectile>($"{PrefabDir}/Projectile.prefab");
            so.FindProperty("enemyProjectilePrefab").objectReferenceValue = proj;

            so.ApplyModifiedProperties();

            // 存储 enemy definition 供运行时使用
            var defHolder = triggerGo.AddComponent<EnemyDefinitionHolder>();
            defHolder.definition = def;
        }

        private static void CreateCheckpoint(string name, Vector3 pos, CheckpointManager manager)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 2f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadOrImportSprite($"{ArtDir}/checkpoint_flag.png");
            sr.sortingOrder = 2;

            var cp = go.AddComponent<Checkpoint>();
            var so = new SerializedObject(cp);
            so.FindProperty("manager").objectReferenceValue = manager;
            so.ApplyModifiedProperties();
        }

        private static void PlacePickup(GameObject prefab, Vector3 pos, string name)
        {
            if (prefab == null) return;
            var go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            go.name = name;
            go.transform.position = pos;
            FixSpriteReferences(go, prefab);
        }

        private static void BuildGameLoop()
        {
            var go = new GameObject("[GameLoop]");
            go.AddComponent<SteelRain.Game.GameLoop>();
        }

        private static void BuildAudioManager()
        {
            var go = new GameObject("AudioManager");
            var am = go.AddComponent<AudioManager>();

            var audioDir = "Assets/Audio/Generated";
            var clips = new System.Collections.Generic.List<AudioClip>();
            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { audioDir });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null) clips.Add(clip);
            }

            var so = new SerializedObject(am);
            var clipsProp = so.FindProperty("clipsToLoad");
            clipsProp.arraySize = clips.Count;
            for (int i = 0; i < clips.Count; i++)
                clipsProp.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            so.ApplyModifiedProperties();
        }

        private static void BuildMusicPlayer(Transform player)
        {
            var go = new GameObject("MusicPlayer");
            var mp = go.AddComponent<MusicPlayer>();

            var beachClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/music_beach.wav");
            var villageClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/music_village.wav");
            var bossClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/music_boss.wav");

            var so = new SerializedObject(mp);
            so.FindProperty("player").objectReferenceValue = player;
            so.FindProperty("beachMusic").objectReferenceValue = beachClip;
            so.FindProperty("villageMusic").objectReferenceValue = villageClip;
            so.FindProperty("bossMusic").objectReferenceValue = bossClip;
            so.ApplyModifiedProperties();
        }

        private static void BuildMusicPlayerBossOnly(Transform player)
        {
            var go = new GameObject("MusicPlayer");
            var mp = go.AddComponent<MusicPlayer>();

            var bossClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/music_boss.wav");

            var so = new SerializedObject(mp);
            so.FindProperty("player").objectReferenceValue = player;
            so.FindProperty("beachMusic").objectReferenceValue = bossClip;
            so.FindProperty("villageMusic").objectReferenceValue = bossClip;
            so.FindProperty("bossMusic").objectReferenceValue = bossClip;
            so.FindProperty("playBossOnly").boolValue = true;
            so.ApplyModifiedProperties();
        }

        /// <summary>
        /// 构建环境物件：沙袋掩体、金属路障、爆炸桶、路标
        /// 提供战术掩体和视觉丰富度（成品游戏必备）
        /// </summary>
        private static void BuildEnvironmentProps()
        {
            var sandbagSprite = LoadOrImportSprite($"{ArtDir}/sandbag.png");
            var barricadeSprite = LoadOrImportSprite($"{ArtDir}/barricade.png");
            var barrelSprite = LoadOrImportSprite($"{ArtDir}/explosive_barrel.png");
            var signSprite = LoadOrImportSprite($"{ArtDir}/signpost.png");
            var spikeSprite = LoadOrImportSprite($"{ArtDir}/spikes.png");

            var propsParent = new GameObject("EnvironmentProps");

            // 沙袋掩体（提供低矮掩体，玩家可蹲伏射击）
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(12f, 0.8f, 0f), "Sandbag_A1", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(30f, 0.8f, 0f), "Sandbag_A2", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(55f, 0.8f, 0f), "Sandbag_A3", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(82f, 0.8f, 0f), "Sandbag_B1", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(110f, 0.8f, 0f), "Sandbag_B2", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(140f, 0.8f, 0f), "Sandbag_B3", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(175f, 0.8f, 0f), "Sandbag_C1", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(215f, 0.8f, 0f), "Sandbag_C2", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(255f, 0.8f, 0f), "Sandbag_D1", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(295f, 0.8f, 0f), "Sandbag_D2", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(335f, 0.8f, 0f), "Sandbag_E1", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(375f, 0.8f, 0f), "Sandbag_E2", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(415f, 0.8f, 0f), "Sandbag_F1", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(440f, 0.8f, 0f), "Sandbag_F2", 1);

            // 金属路障（高掩体，阻挡子弹）
            PlaceEnvProp(propsParent, barricadeSprite, new Vector3(40f, 1.2f, 0f), "Barricade_A1", 2);
            PlaceEnvProp(propsParent, barricadeSprite, new Vector3(95f, 1.2f, 0f), "Barricade_B1", 2);
            PlaceEnvProp(propsParent, barricadeSprite, new Vector3(160f, 1.2f, 0f), "Barricade_C1", 2);
            PlaceEnvProp(propsParent, barricadeSprite, new Vector3(240f, 1.2f, 0f), "Barricade_D1", 2);
            PlaceEnvProp(propsParent, barricadeSprite, new Vector3(270f, 1.2f, 0f), "Barricade_D2", 2);
            PlaceEnvProp(propsParent, barricadeSprite, new Vector3(320f, 1.2f, 0f), "Barricade_E1", 2);
            PlaceEnvProp(propsParent, barricadeSprite, new Vector3(360f, 1.2f, 0f), "Barricade_E2", 2);
            PlaceEnvProp(propsParent, barricadeSprite, new Vector3(420f, 1.2f, 0f), "Barricade_F1", 2);
            PlaceEnvProp(propsParent, barricadeSprite, new Vector3(450f, 1.2f, 0f), "Barricade_F2", 2);

            // 爆炸桶（战术元素，可引爆造成范围伤害）
            PlaceEnvProp(propsParent, barrelSprite, new Vector3(22f, 1f, 0f), "Barrel_A1", 2);
            PlaceEnvProp(propsParent, barrelSprite, new Vector3(48f, 1f, 0f), "Barrel_A2", 2);
            PlaceEnvProp(propsParent, barrelSprite, new Vector3(88f, 1f, 0f), "Barrel_B1", 2);
            PlaceEnvProp(propsParent, barrelSprite, new Vector3(125f, 1f, 0f), "Barrel_B2", 2);
            PlaceEnvProp(propsParent, barrelSprite, new Vector3(185f, 1f, 0f), "Barrel_C1", 2);
            PlaceEnvProp(propsParent, barrelSprite, new Vector3(205f, 1f, 0f), "Barrel_C2", 2);
            PlaceEnvProp(propsParent, barrelSprite, new Vector3(250f, 1f, 0f), "Barrel_D1", 2);
            PlaceEnvProp(propsParent, barrelSprite, new Vector3(290f, 1f, 0f), "Barrel_D2", 2);
            PlaceEnvProp(propsParent, barrelSprite, new Vector3(330f, 1f, 0f), "Barrel_E1", 2);
            PlaceEnvProp(propsParent, barrelSprite, new Vector3(370f, 1f, 0f), "Barrel_E2", 2);
            PlaceEnvProp(propsParent, barrelSprite, new Vector3(405f, 1f, 0f), "Barrel_F1", 2);
            PlaceEnvProp(propsParent, barrelSprite, new Vector3(435f, 1f, 0f), "Barrel_F2", 2);

            // 路标（装饰元素，指示方向）
            PlaceEnvProp(propsParent, signSprite, new Vector3(5f, 1.2f, 0f), "Sign_Start", 3);
            PlaceEnvProp(propsParent, signSprite, new Vector3(70f, 1.2f, 0f), "Sign_AB", 3);
            PlaceEnvProp(propsParent, signSprite, new Vector3(150f, 1.2f, 0f), "Sign_BC", 3);
            PlaceEnvProp(propsParent, signSprite, new Vector3(230f, 1.2f, 0f), "Sign_CD", 3);
            PlaceEnvProp(propsParent, signSprite, new Vector3(310f, 1.2f, 0f), "Sign_DE", 3);
            PlaceEnvProp(propsParent, signSprite, new Vector3(390f, 1.2f, 0f), "Sign_EF", 3);
            PlaceEnvProp(propsParent, signSprite, new Vector3(445f, 1.2f, 0f), "Sign_Boss", 3);

            // 尖刺陷阱（危险元素，工业/城市段）
            PlaceEnvProp(propsParent, spikeSprite, new Vector3(235f, 0.6f, 0f), "Spikes_D1", 1);
            PlaceEnvProp(propsParent, spikeSprite, new Vector3(275f, 0.6f, 0f), "Spikes_D2", 1);
            PlaceEnvProp(propsParent, spikeSprite, new Vector3(315f, 0.6f, 0f), "Spikes_E1", 1);
            PlaceEnvProp(propsParent, spikeSprite, new Vector3(350f, 0.6f, 0f), "Spikes_E2", 1);
            PlaceEnvProp(propsParent, spikeSprite, new Vector3(385f, 0.6f, 0f), "Spikes_E3", 1);
        }

        private static void PlaceEnvProp(GameObject parent, Sprite sprite, Vector3 pos, string name, int sortingOrder)
        {
            if (sprite == null) return;
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
        }

        private static void BuildL2EnvironmentProps()
        {
            var sandbagSprite = LoadOrImportSprite($"{ArtDir}/sandbag.png");
            var barricadeSprite = LoadOrImportSprite($"{ArtDir}/barricade.png");
            var barrelSprite = LoadOrImportSprite($"{ArtDir}/explosive_barrel.png");
            var signSprite = LoadOrImportSprite($"{ArtDir}/signpost.png");
            var spikeSprite = LoadOrImportSprite($"{ArtDir}/spikes.png");

            var propsParent = new GameObject("L2_EnvironmentProps");

            // 工厂段：沙袋掩体
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(15f, 0.8f, 0f), "L2_Sandbag_1", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(50f, 0.8f, 0f), "L2_Sandbag_2", 1);
            PlaceEnvProp(propsParent, sandbagSprite, new Vector3(85f, 0.8f, 0f), "L2_Sandbag_3", 1);
            // 金属路障
            PlaceEnvProp(propsParent, barricadeSprite, new Vector3(30f, 0.8f, 0f), "L2_Barricade_1", 1);
            PlaceEnvProp(propsParent, barricadeSprite, new Vector3(70f, 0.8f, 0f), "L2_Barricade_2", 1);
            PlaceEnvProp(propsParent, barricadeSprite, new Vector3(105f, 0.8f, 0f), "L2_Barricade_3", 1);
            // 爆炸桶
            PlaceEnvProp(propsParent, barrelSprite, new Vector3(45f, 0.7f, 0f), "L2_Barrel_1", 1);
            PlaceEnvProp(propsParent, barrelSprite, new Vector3(110f, 0.7f, 0f), "L2_Barrel_2", 1);
            // 路标
            PlaceEnvProp(propsParent, signSprite, new Vector3(5f, 1.2f, 0f), "L2_Sign_1", 2);
            PlaceEnvProp(propsParent, signSprite, new Vector3(60f, 1.2f, 0f), "L2_Sign_2", 2);
            // 尖刺陷阱
            PlaceEnvProp(propsParent, spikeSprite, new Vector3(65f, 0.3f, 0f), "L2_Spikes_1", 1);
            PlaceEnvProp(propsParent, spikeSprite, new Vector3(95f, 0.3f, 0f), "L2_Spikes_2", 1);
        }

        private static void BuildParallaxBackground(GameObject camera)
        {
            var bgGo = new GameObject("ParallaxBackground");
            var parallax = bgGo.AddComponent<ParallaxBackground>();

            var skySprite = LoadOrImportSprite($"{ArtDir}/background_sky.png");
            var seaSprite = LoadOrImportSprite($"{ArtDir}/background_sea.png");
            var mountainSprite = LoadOrImportSprite($"{ArtDir}/parallax_far_mountain.png");
            var hillSprite = LoadOrImportSprite($"{ArtDir}/parallax_mid_hill.png");
            var citySprite = LoadOrImportSprite($"{ArtDir}/background_city.png");
            var industrialSprite = LoadOrImportSprite($"{ArtDir}/background_industrial.png");

            // 天空层（最远，几乎不动）
            var skyGo = new GameObject("SkyLayer");
            skyGo.transform.SetParent(bgGo.transform);
            skyGo.transform.position = new Vector3(230f, 8f, 5f);
            var skySr = skyGo.AddComponent<SpriteRenderer>();
            skySr.sprite = skySprite;
            skySr.drawMode = SpriteDrawMode.Sliced;
            skySr.size = new Vector2(500f, 40f);
            skySr.sortingOrder = -10;
            skySr.color = new Color(0.15f, 0.1f, 0.2f, 1f);

            // 海洋层（远）
            var seaGo = new GameObject("SeaLayer");
            seaGo.transform.SetParent(bgGo.transform);
            seaGo.transform.position = new Vector3(35f, 2f, 4f);
            var seaSr = seaGo.AddComponent<SpriteRenderer>();
            seaSr.sprite = seaSprite;
            seaSr.drawMode = SpriteDrawMode.Sliced;
            seaSr.size = new Vector2(80f, 10f);
            seaSr.sortingOrder = -8;
            seaSr.color = new Color(0.1f, 0.2f, 0.35f, 0.8f);

            // 远山层（中远视差）
            var mountainGo = new GameObject("FarMountainLayer");
            mountainGo.transform.SetParent(bgGo.transform);
            mountainGo.transform.position = new Vector3(128f, 1f, 3f);
            var mountainSr = mountainGo.AddComponent<SpriteRenderer>();
            mountainSr.sprite = mountainSprite;
            mountainSr.drawMode = SpriteDrawMode.Sliced;
            mountainSr.size = new Vector2(500f, 24f);
            mountainSr.sortingOrder = -7;
            mountainSr.color = new Color(1f, 1f, 1f, 0.7f);

            // 中景丘陵层
            var hillGo = new GameObject("MidHillLayer");
            hillGo.transform.SetParent(bgGo.transform);
            hillGo.transform.position = new Vector3(128f, 0.5f, 2f);
            var hillSr = hillGo.AddComponent<SpriteRenderer>();
            hillSr.sprite = hillSprite;
            hillSr.drawMode = SpriteDrawMode.Sliced;
            hillSr.size = new Vector2(500f, 16f);
            hillSr.sortingOrder = -6;
            hillSr.color = new Color(1f, 1f, 1f, 0.85f);

            // 城市天际线层（城市段背景）
            var cityGo = new GameObject("CityBgLayer");
            cityGo.transform.SetParent(bgGo.transform);
            cityGo.transform.position = new Vector3(270f, 2f, 3f);
            var citySr = cityGo.AddComponent<SpriteRenderer>();
            citySr.sprite = citySprite;
            citySr.drawMode = SpriteDrawMode.Sliced;
            citySr.size = new Vector2(100f, 32f);
            citySr.sortingOrder = -6;
            citySr.color = new Color(1f, 1f, 1f, 0.8f);

            // 工业天际线层（工业段背景）
            var industrialGo = new GameObject("IndustrialBgLayer");
            industrialGo.transform.SetParent(bgGo.transform);
            industrialGo.transform.position = new Vector3(350f, 2f, 3f);
            var industrialSr = industrialGo.AddComponent<SpriteRenderer>();
            industrialSr.sprite = industrialSprite;
            industrialSr.drawMode = SpriteDrawMode.Sliced;
            industrialSr.size = new Vector2(100f, 32f);
            industrialSr.sortingOrder = -6;
            industrialSr.color = new Color(1f, 1f, 1f, 0.8f);

            var so = new SerializedObject(parallax);
            var layersProp = so.FindProperty("layers");
            layersProp.arraySize = 6;

            var layer0 = layersProp.GetArrayElementAtIndex(0);
            layer0.FindPropertyRelative("transform").objectReferenceValue = skyGo.transform;
            layer0.FindPropertyRelative("parallaxFactor").floatValue = 0.05f;
            layer0.FindPropertyRelative("horizontalExtent").floatValue = 250f;

            var layer1 = layersProp.GetArrayElementAtIndex(1);
            layer1.FindPropertyRelative("transform").objectReferenceValue = seaGo.transform;
            layer1.FindPropertyRelative("parallaxFactor").floatValue = 0.2f;
            layer1.FindPropertyRelative("horizontalExtent").floatValue = 250f;

            var layer2 = layersProp.GetArrayElementAtIndex(2);
            layer2.FindPropertyRelative("transform").objectReferenceValue = mountainGo.transform;
            layer2.FindPropertyRelative("parallaxFactor").floatValue = 0.3f;
            layer2.FindPropertyRelative("horizontalExtent").floatValue = 250f;

            var layer3 = layersProp.GetArrayElementAtIndex(3);
            layer3.FindPropertyRelative("transform").objectReferenceValue = hillGo.transform;
            layer3.FindPropertyRelative("parallaxFactor").floatValue = 0.5f;
            layer3.FindPropertyRelative("horizontalExtent").floatValue = 250f;

            var layer4 = layersProp.GetArrayElementAtIndex(4);
            layer4.FindPropertyRelative("transform").objectReferenceValue = cityGo.transform;
            layer4.FindPropertyRelative("parallaxFactor").floatValue = 0.4f;
            layer4.FindPropertyRelative("horizontalExtent").floatValue = 250f;

            var layer5 = layersProp.GetArrayElementAtIndex(5);
            layer5.FindPropertyRelative("transform").objectReferenceValue = industrialGo.transform;
            layer5.FindPropertyRelative("parallaxFactor").floatValue = 0.4f;
            layer5.FindPropertyRelative("horizontalExtent").floatValue = 250f;

            so.FindProperty("cam").objectReferenceValue = camera.transform;
            so.ApplyModifiedProperties();
        }

        private static void BuildPauseMenu()
        {
            var canvasGo = new GameObject("PauseCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("PausePanel");
            panel.transform.SetParent(canvasGo.transform);
            var panelRt = panel.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.7f);

            var titleText = CreateHudText(panel, "PauseTitle", new Vector2(0, 40), "PAUSED", 48);
            var hintText = CreateHudText(panel, "PauseHint", new Vector2(0, -20), "ESC Resume | S Settings | R Restart | Q Quit", 18);

            // ===== 设置面板 =====
            var settingsPanel = new GameObject("SettingsPanel");
            settingsPanel.transform.SetParent(canvasGo.transform);
            var spRt = settingsPanel.AddComponent<RectTransform>();
            spRt.anchorMin = Vector2.zero;
            spRt.anchorMax = Vector2.one;
            spRt.offsetMin = Vector2.zero;
            spRt.offsetMax = Vector2.zero;
            var spImg = settingsPanel.AddComponent<Image>();
            spImg.color = new Color(0.06f, 0.07f, 0.1f, 0.95f);

            var spTitle = CreateHudText(settingsPanel, "SettingsTitle", new Vector2(0, 120), "SETTINGS", 36);

            var masterSlider = CreateVolumeSlider(settingsPanel, "MasterSlider", new Vector2(0, 50), "Master Volume", 1f);
            var musicSlider = CreateVolumeSlider(settingsPanel, "MusicSlider", new Vector2(0, 0), "Music Volume", 0.7f);
            var sfxSlider = CreateVolumeSlider(settingsPanel, "SfxSlider", new Vector2(0, -50), "SFX Volume", 1f);

            var backHint = CreateHudText(settingsPanel, "BackHint", new Vector2(0, -120), "ESC Back", 20);

            settingsPanel.SetActive(false);

            var pm = canvasGo.AddComponent<PauseManager>();
            var so = new SerializedObject(pm);
            so.FindProperty("pausePanel").objectReferenceValue = panel;
            so.FindProperty("pauseTitle").objectReferenceValue = titleText;
            so.FindProperty("pauseHint").objectReferenceValue = hintText;
            so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            so.FindProperty("masterSlider").objectReferenceValue = masterSlider;
            so.FindProperty("musicSlider").objectReferenceValue = musicSlider;
            so.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
            so.ApplyModifiedProperties();

            panel.SetActive(false);
        }

        private static Slider CreateVolumeSlider(GameObject parent, string name, Vector2 pos, string label, float defaultValue)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(400, 50);

            // 标签
            var labelText = CreateHudText(go, "Label", new Vector2(-80, 15), label, 18);

            // 滑块背景
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.18f, 0.25f, 1f);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0.1f, 0f);
            bgRt.anchorMax = new Vector2(0.9f, 0.4f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            // 填充
            var fill = new GameObject("Fill");
            fill.transform.SetParent(bg.transform);
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.95f, 0.55f, 0.15f, 1f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            // 手柄
            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(bg.transform);
            var haRt = handleArea.AddComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.offsetMin = Vector2.zero;
            haRt.offsetMax = Vector2.zero;

            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform);
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;
            var handleRt = handle.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(20, 0);
            handleRt.anchorMin = Vector2.zero;
            handleRt.anchorMax = new Vector2(0, 1);
            handleRt.offsetMin = Vector2.zero;
            handleRt.offsetMax = Vector2.zero;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = defaultValue;
            slider.wholeNumbers = false;

            return slider;
        }

        private static void BuildDamageIndicator(Transform player)
        {
            var canvasGo = GameObject.Find("HUD_Canvas");
            if (canvasGo == null) return;

            var indicatorGo = new GameObject("DamageIndicator");
            indicatorGo.transform.SetParent(canvasGo.transform);
            var rt = indicatorGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            var arrowGo = new GameObject("ArrowTemplate");
            arrowGo.transform.SetParent(indicatorGo.transform);
            var arrowRt = arrowGo.AddComponent<RectTransform>();
            arrowRt.anchoredPosition = Vector2.zero;
            arrowRt.sizeDelta = new Vector2(40, 40);
            var arrowImg = arrowGo.AddComponent<Image>();
            arrowImg.sprite = CreateTriangleSprite();
            arrowImg.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            arrowImg.enabled = false;

            var indicator = indicatorGo.AddComponent<DamageDirectionIndicator>();
            var so = new SerializedObject(indicator);
            so.FindProperty("player").objectReferenceValue = player;
            so.FindProperty("arrowPrefab").objectReferenceValue = arrowImg;
            so.ApplyModifiedProperties();
        }

        private static Sprite CreateTriangleSprite()
        {
            int size = 16;
            var tex = new Texture2D(size, size);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    // 三角形
                    float nx = (float)x / size;
                    float ny = (float)y / size;
                    bool inTri = ny > 0.2f && ny < 0.9f && nx > (0.5f - ny * 0.4f) && nx < (0.5f + ny * 0.4f);
                    tex.SetPixel(x, y, inTri ? Color.white : new Color(0, 0, 0, 0));
                }
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            var path = $"{ArtDir}/triangle.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            return LoadOrImportSprite(path);
        }

        private static void BuildHud(GameObject player)
        {
            var canvasGo = new GameObject("HUD_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 玩家卡片背景（左下）
            CreateHudCard(canvasGo, "PlayerCard", new Vector2(0.0f, 0.0f), new Vector2(0.22f, 0.32f));
            // 武器卡片背景（右下）
            CreateHudCard(canvasGo, "WeaponCard", new Vector2(0.78f, 0.0f), new Vector2(1.0f, 0.32f));
            // 顶部中央细条（分数）
            CreateHudCard(canvasGo, "ScoreCard", new Vector2(0.42f, 0.92f), new Vector2(0.58f, 1.0f));

            // 血条背景
            var healthBarBg = new GameObject("HealthBarBg");
            healthBarBg.transform.SetParent(canvasGo.transform);
            var hbBgRt = healthBarBg.AddComponent<RectTransform>();
            hbBgRt.anchorMin = new Vector2(0.025f, 0.225f);
            hbBgRt.anchorMax = new Vector2(0.195f, 0.245f);
            hbBgRt.offsetMin = Vector2.zero;
            hbBgRt.offsetMax = Vector2.zero;
            var hbBgImg = healthBarBg.AddComponent<Image>();
            hbBgImg.color = new Color(0.06f, 0.07f, 0.10f, 0.95f);

            var healthDamageFill = new GameObject("DamageFill");
            healthDamageFill.transform.SetParent(healthBarBg.transform);
            var hdfRt = healthDamageFill.AddComponent<RectTransform>();
            hdfRt.anchorMin = Vector2.zero;
            hdfRt.anchorMax = Vector2.one;
            hdfRt.offsetMin = Vector2.zero;
            hdfRt.offsetMax = Vector2.zero;
            var hdfImg = healthDamageFill.AddComponent<Image>();
            hdfImg.color = new Color(0.55f, 0.10f, 0.12f, 0.85f);
            hdfImg.type = Image.Type.Filled;
            hdfImg.fillMethod = Image.FillMethod.Horizontal;
            hdfImg.fillAmount = 1f;

            var healthFill = new GameObject("HealthFill");
            healthFill.transform.SetParent(healthBarBg.transform);
            var hfRt = healthFill.AddComponent<RectTransform>();
            hfRt.anchorMin = Vector2.zero;
            hfRt.anchorMax = Vector2.one;
            hfRt.offsetMin = Vector2.zero;
            hfRt.offsetMax = Vector2.zero;
            var hfImg = healthFill.AddComponent<Image>();
            hfImg.color = SteelRain.UI.UIPalette.HealthHigh;
            hfImg.type = Image.Type.Filled;
            hfImg.fillMethod = Image.FillMethod.Horizontal;
            hfImg.fillAmount = 1f;

            // 血条文字
            var healthText = CreateHudText(canvasGo, "HealthText", new Vector2(0, -22), "HP 6/6", 18, SteelRain.UI.UIPalette.TextPrimary, FontStyle.Bold);
            healthText.rectTransform.anchorMin = new Vector2(0.025f, 0.18f);
            healthText.rectTransform.anchorMax = new Vector2(0.195f, 0.22f);
            healthText.alignment = TextAnchor.MiddleCenter;

            // 武器文字
            var ammoText = CreateHudText(canvasGo, "AmmoText", new Vector2(0, -20), "Assault Rifle INF [Auto]", 18, SteelRain.UI.UIPalette.TextPrimary, FontStyle.Bold);
            ammoText.rectTransform.anchorMin = new Vector2(0.78f, 0.18f);
            ammoText.rectTransform.anchorMax = new Vector2(1.0f, 0.22f);
            ammoText.alignment = TextAnchor.MiddleCenter;
            var levelText = CreateHudText(canvasGo, "WeaponLevelText", new Vector2(0, -50), "Weapon Lv0", 14, SteelRain.UI.UIPalette.Warning, FontStyle.Bold);
            levelText.rectTransform.anchorMin = new Vector2(0.78f, 0.13f);
            levelText.rectTransform.anchorMax = new Vector2(1.0f, 0.17f);
            levelText.alignment = TextAnchor.MiddleCenter;
            var charText = CreateHudText(canvasGo, "CharacterText", new Vector2(0, -76), "Aila", 16, SteelRain.UI.UIPalette.Accent, FontStyle.Bold);
            charText.rectTransform.anchorMin = new Vector2(0.78f, 0.09f);
            charText.rectTransform.anchorMax = new Vector2(1.0f, 0.13f);
            charText.alignment = TextAnchor.MiddleCenter;
            var skillText = CreateHudText(canvasGo, "SkillText", new Vector2(0, -100), "Skill: READY", 13, SteelRain.UI.UIPalette.TextSecondary, FontStyle.Normal);
            skillText.rectTransform.anchorMin = new Vector2(0.78f, 0.05f);
            skillText.rectTransform.anchorMax = new Vector2(1.0f, 0.09f);
            skillText.alignment = TextAnchor.MiddleCenter;

            // 小队列表
            var squadText = CreateHudText(canvasGo, "SquadText", new Vector2(0, 0), "", 12, SteelRain.UI.UIPalette.TextSecondary, FontStyle.Normal);
            squadText.rectTransform.anchorMin = new Vector2(0.03f, 0.04f);
            squadText.rectTransform.anchorMax = new Vector2(0.20f, 0.18f);
            squadText.alignment = TextAnchor.UpperLeft;
            squadText.horizontalOverflow = HorizontalWrapMode.Overflow;
            squadText.verticalOverflow = VerticalWrapMode.Overflow;

            // 顶部分数
            var scoreText = CreateHudText(canvasGo, "ScoreText", new Vector2(0, 0), "SCORE: 0", 22, SteelRain.UI.UIPalette.TextPrimary, FontStyle.Bold);
            scoreText.rectTransform.anchorMin = new Vector2(0.42f, 0.93f);
            scoreText.rectTransform.anchorMax = new Vector2(0.58f, 0.99f);
            scoreText.alignment = TextAnchor.MiddleCenter;

            // 连击
            var comboText = CreateHudText(canvasGo, "ComboText", new Vector2(0, -22), "x2 COMBO!", 28, SteelRain.UI.UIPalette.Warning, FontStyle.Bold);
            comboText.enabled = false;
            comboText.rectTransform.anchorMin = new Vector2(0.40f, 0.86f);
            comboText.rectTransform.anchorMax = new Vector2(0.60f, 0.92f);
            comboText.alignment = TextAnchor.MiddleCenter;

            // 军票余额（经济系统）
            var currencyText = CreateHudText(canvasGo, "CurrencyText", new Vector2(0, 0), "军票: 0", 18, new Color(1f, 0.85f, 0.2f, 1f), FontStyle.Bold);
            currencyText.rectTransform.anchorMin = new Vector2(0.42f, 0.87f);
            currencyText.rectTransform.anchorMax = new Vector2(0.58f, 0.92f);
            currencyText.alignment = TextAnchor.MiddleCenter;

            // 检查点提示
            var checkpointText = CreateHudText(canvasGo, "CheckpointText", new Vector2(0, 0), "", 24, SteelRain.UI.UIPalette.Accent, FontStyle.Bold);
            checkpointText.enabled = false;
            checkpointText.rectTransform.anchorMin = new Vector2(0.3f, 0.45f);
            checkpointText.rectTransform.anchorMax = new Vector2(0.7f, 0.55f);
            checkpointText.alignment = TextAnchor.MiddleCenter;

            var skill = player.GetComponent<CharacterSkill>();
            var hud = canvasGo.AddComponent<HudPresenter>();
            var so = new SerializedObject(hud);
            so.FindProperty("healthText").objectReferenceValue = healthText;
            so.FindProperty("ammoText").objectReferenceValue = ammoText;
            so.FindProperty("weaponLevelText").objectReferenceValue = levelText;
            so.FindProperty("characterText").objectReferenceValue = charText;
            so.FindProperty("skillText").objectReferenceValue = skillText;
            so.FindProperty("squadText").objectReferenceValue = squadText;
            so.FindProperty("scoreText").objectReferenceValue = scoreText;
            so.FindProperty("comboText").objectReferenceValue = comboText;
            so.FindProperty("currencyText").objectReferenceValue = currencyText;
            so.FindProperty("skill").objectReferenceValue = skill;
            so.ApplyModifiedProperties();

            // 检查点提示
            var cpPrompt = canvasGo.AddComponent<CheckpointPrompt>();
            var soCp = new SerializedObject(cpPrompt);
            soCp.FindProperty("promptText").objectReferenceValue = checkpointText;
            soCp.ApplyModifiedProperties();

            // 血条组件
            var healthBar = canvasGo.AddComponent<HealthBar>();
            var soHb = new SerializedObject(healthBar);
            soHb.FindProperty("fillImage").objectReferenceValue = hfImg;
            soHb.FindProperty("damageFillImage").objectReferenceValue = hdfImg;
            soHb.FindProperty("healthText").objectReferenceValue = healthText;
            soHb.ApplyModifiedProperties();

            // 低血量暗角效果
            var vignetteGo = new GameObject("VignetteOverlay");
            vignetteGo.transform.SetParent(canvasGo.transform);
            var vignetteImg = vignetteGo.AddComponent<Image>();
            vignetteImg.color = new Color(0.6f, 0f, 0f, 0f);
            vignetteImg.raycastTarget = false;
            vignetteImg.type = Image.Type.Filled;
            vignetteImg.fillMethod = Image.FillMethod.Radial360;
            var vignetteRt = vignetteGo.GetComponent<RectTransform>();
            vignetteRt.anchorMin = Vector2.zero;
            vignetteRt.anchorMax = Vector2.one;
            vignetteRt.offsetMin = Vector2.zero;
            vignetteRt.offsetMax = Vector2.zero;
            var vignette = canvasGo.AddComponent<LowHealthVignette>();
            var soVig = new SerializedObject(vignette);
            soVig.FindProperty("vignetteImage").objectReferenceValue = vignetteImg;
            soVig.ApplyModifiedProperties();

            // 角色切换提示
            canvasGo.AddComponent<SteelRain.UI.CharacterSwitchToast>();
        }

        private static void CreateHudCard(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.SetAsFirstSibling();
            var img = go.AddComponent<Image>();
            img.color = SteelRain.UI.UIPalette.Panel;
            img.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Text CreateHudText(GameObject canvas, string name, Vector2 pos, string content, int fontSize, Color color, FontStyle style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(canvas.transform);
            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = style;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null) text.font = font;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var rt = text.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(300, 40);
            return text;
        }

        private static Text CreateHudText(GameObject canvas, string name, Vector2 pos, string content, int fontSize)
        {
            return CreateHudText(canvas, name, pos, content, fontSize, Color.white, FontStyle.Normal);
        }

        private static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 主相机：Boot 场景需要一个相机来渲染，背景色与 BootScreen 一致
            var camGo = new GameObject("MainCamera");
            camGo.transform.position = new Vector3(0, 0, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.08f, 1f);
            camGo.tag = "MainCamera";

            // Bootstrap 根对象：挂载 GameBootstrap 和 BootScreen
            var go = new GameObject("Bootstrap");
            go.AddComponent<GameBootstrap>();
            // 明确挂载 BootScreen，使其在编辑器中可见、可调整参数
            go.AddComponent<BootScreen>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Boot.unity");
            Debug.Log("[VerticalSliceBuilder] Boot scene created with Camera + GameBootstrap + BootScreen.");
        }

        private static void RegisterBuildScenes()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level01_VerticalSlice.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level02_Factory.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("[VerticalSliceBuilder] Build scenes registered.");
        }

        private static void CreateEventSystem()
        {
            // 检查是否已存在EventSystem
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;

            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("MainCamera");
            camGo.transform.position = new Vector3(0, 0, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            camGo.tag = "MainCamera";

            // EventSystem（UI按钮点击必需）
            CreateEventSystem();

            var canvasGo = new GameObject("MenuCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 装饰背景层（在内容之下）
            BuildMenuBackdrop(canvasGo);

            // 背景动效层
            BuildMenuAnimationLayer(canvasGo);

            // 主菜单内容容器
            var menuContent = new GameObject("MenuContent");
            menuContent.transform.SetParent(canvasGo.transform);
            var mcRt = menuContent.AddComponent<RectTransform>();
            mcRt.anchorMin = Vector2.zero;
            mcRt.anchorMax = Vector2.one;
            mcRt.offsetMin = Vector2.zero;
            mcRt.offsetMax = Vector2.zero;

            // 标题区
            var titleText = CreateMenuText(menuContent, "Title", new Vector2(0, 220), "STEEL RAIN: FRONTIER", 64, SteelRain.UI.UIPalette.TextPrimary, FontStyle.Bold);
            titleText.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 80);
            CreateUnderline(menuContent, "TitleUnderline", new Vector2(0, 180), 240, 3, SteelRain.UI.UIPalette.Primary);
            var subtitleText = CreateMenuText(menuContent, "Subtitle", new Vector2(0, 140), "A 2D Squad Shooter", 22, SteelRain.UI.UIPalette.TextSecondary, FontStyle.Normal);

            // 主按钮（橙色调强调）
            var startBtn = CreateMenuButton(menuContent, "StartBtn", new Vector2(0, 60), "START GAME", 200, 50, true);
            var newGameBtn = CreateMenuButton(menuContent, "NewGameBtn", new Vector2(0, 0), "NEW GAME", 200, 50, false);

            // 难度区
            var diffLabel = CreateMenuText(menuContent, "DiffLabel", new Vector2(0, -65), "DIFFICULTY", 14, SteelRain.UI.UIPalette.TextMuted, FontStyle.Bold);
            diffLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 30);
            var diffText = CreateMenuText(menuContent, "DiffText", new Vector2(0, -90), "Normal", 24, SteelRain.UI.UIPalette.Warning, FontStyle.Bold);
            diffText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 30);
            var easyBtn = CreateMenuButton(menuContent, "EasyBtn", new Vector2(-130, -135), "EASY", 90, 40, false);
            var normalBtn = CreateMenuButton(menuContent, "NormalBtn", new Vector2(0, -135), "NORMAL", 100, 40, false);
            var hardBtn = CreateMenuButton(menuContent, "HardBtn", new Vector2(130, -135), "HARD", 90, 40, false);

            // 底部
            CreateDivider(menuContent, "Divider", new Vector2(0, -190), 360, 1, SteelRain.UI.UIPalette.Divider);
            var achievementsBtn = CreateMenuButton(menuContent, "AchievementsBtn", new Vector2(0, -220), "ACHIEVEMENTS", 200, 42, false);
            var settingsBtn = CreateMenuButton(menuContent, "SettingsBtn", new Vector2(0, -270), "SETTINGS", 200, 42, false);
            var quitBtn = CreateMenuButton(menuContent, "QuitBtn", new Vector2(0, -320), "QUIT", 200, 42, false);

            // 底部版本号
            var versionText = CreateMenuText(menuContent, "Version", new Vector2(0, -340), "v1.0.0  ·  Unity 6 LTS", 12, SteelRain.UI.UIPalette.TextMuted, FontStyle.Normal);
            versionText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 24);

            var menu = canvasGo.AddComponent<MainMenu>();

            // Settings 面板
            var settingsPanel = new GameObject("SettingsPanel");
            settingsPanel.transform.SetParent(canvasGo.transform);
            var panelRt = settingsPanel.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.25f, 0.15f);
            panelRt.anchorMax = new Vector2(0.75f, 0.85f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            var panelImg = settingsPanel.AddComponent<Image>();
            panelImg.color = SteelRain.UI.UIPalette.Panel;

            var panelHeader = CreateMenuText(settingsPanel, "SettingsTitle", new Vector2(0, 200), "SETTINGS", 40, SteelRain.UI.UIPalette.TextPrimary, FontStyle.Bold);
            panelHeader.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 60);
            CreateUnderline(settingsPanel, "SettingsUnderline", new Vector2(0, 165), 120, 3, SteelRain.UI.UIPalette.Accent);

            var masterSlider = CreateMenuSlider(settingsPanel, "MasterSlider", new Vector2(0, 110), "Master Volume");
            var musicSlider = CreateMenuSlider(settingsPanel, "MusicSlider", new Vector2(0, 50), "Music Volume");
            var sfxSlider = CreateMenuSlider(settingsPanel, "SfxSlider", new Vector2(0, -10), "SFX Volume");
            var fullscreenToggle = CreateMenuToggle(settingsPanel, "FullscreenToggle", new Vector2(0, -75), "Fullscreen");
            var applyBtn = CreateMenuButton(settingsPanel, "ApplyBtn", new Vector2(0, -150), "APPLY", 160, 46, true);
            var backBtn = CreateMenuButton(settingsPanel, "BackBtn", new Vector2(0, -210), "BACK", 160, 46, false);

            var settingsMgr = settingsPanel.AddComponent<SettingsManager>();
            var soSettings = new SerializedObject(settingsMgr);
            soSettings.FindProperty("masterSlider").objectReferenceValue = masterSlider;
            soSettings.FindProperty("musicSlider").objectReferenceValue = musicSlider;
            soSettings.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
            soSettings.FindProperty("fullscreenToggle").objectReferenceValue = fullscreenToggle;
            soSettings.FindProperty("applyButton").objectReferenceValue = applyBtn;
            soSettings.FindProperty("backButton").objectReferenceValue = backBtn;
            soSettings.FindProperty("panel").objectReferenceValue = settingsPanel;
            soSettings.ApplyModifiedProperties();

            var so = new SerializedObject(menu);
            so.FindProperty("startButton").objectReferenceValue = startBtn;
            so.FindProperty("newGameButton").objectReferenceValue = newGameBtn;
            so.FindProperty("settingsButton").objectReferenceValue = settingsBtn;
            so.FindProperty("achievementsButton").objectReferenceValue = achievementsBtn;
            so.FindProperty("quitButton").objectReferenceValue = quitBtn;
            so.FindProperty("easyButton").objectReferenceValue = easyBtn;
            so.FindProperty("normalButton").objectReferenceValue = normalBtn;
            so.FindProperty("hardButton").objectReferenceValue = hardBtn;
            so.FindProperty("difficultyText").objectReferenceValue = diffText;
            so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            so.FindProperty("menuContent").objectReferenceValue = menuContent;
            so.ApplyModifiedProperties();

            // 场景过渡
            var faderGo = new GameObject("SceneFader");
            var faderCanvas = faderGo.AddComponent<Canvas>();
            faderCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            faderCanvas.sortingOrder = 999;
            var faderScaler = faderGo.AddComponent<CanvasScaler>();
            faderScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            faderScaler.referenceResolution = new Vector2(1920, 1080);
            faderGo.AddComponent<GraphicRaycaster>();
            var faderImgGo = new GameObject("FadeImage");
            faderImgGo.transform.SetParent(faderGo.transform);
            var faderImg = faderImgGo.AddComponent<Image>();
            faderImg.color = new Color(0, 0, 0, 0);
            faderImg.raycastTarget = false;
            var faderImgRt = faderImgGo.GetComponent<RectTransform>();
            faderImgRt.anchorMin = Vector2.zero;
            faderImgRt.anchorMax = Vector2.one;
            faderImgRt.offsetMin = Vector2.zero;
            faderImgRt.offsetMax = Vector2.zero;
            var fader = faderGo.AddComponent<SceneFader>();
            var soFader = new SerializedObject(fader);
            soFader.FindProperty("fadeImage").objectReferenceValue = faderImg;
            soFader.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        }

        private static void BuildMenuAnimationLayer(GameObject canvas)
        {
            var animGo = new GameObject("MenuAnimationLayer");
            animGo.transform.SetParent(canvas.transform);
            var rt = animGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            animGo.AddComponent<SteelRain.UI.MenuBackgroundAnimator>();
        }

        private static void BuildMenuBackdrop(GameObject canvas)
        {
            // 顶部高光
            var topGlow = new GameObject("TopGlow");
            topGlow.transform.SetParent(canvas.transform);
            var topGlowImg = topGlow.AddComponent<Image>();
            topGlowImg.color = new Color(0.12f, 0.16f, 0.24f, 0.6f);
            topGlowImg.raycastTarget = false;
            var topGlowRt = topGlow.GetComponent<RectTransform>();
            topGlowRt.anchorMin = new Vector2(0, 0.55f);
            topGlowRt.anchorMax = new Vector2(1, 1);
            topGlowRt.offsetMin = Vector2.zero;
            topGlowRt.offsetMax = Vector2.zero;

            // 底部暗影
            var bottomShade = new GameObject("BottomShade");
            bottomShade.transform.SetParent(canvas.transform);
            var bottomShadeImg = bottomShade.AddComponent<Image>();
            bottomShadeImg.color = new Color(0.02f, 0.02f, 0.04f, 0.8f);
            bottomShadeImg.raycastTarget = false;
            var bottomShadeRt = bottomShade.GetComponent<RectTransform>();
            bottomShadeRt.anchorMin = new Vector2(0, 0);
            bottomShadeRt.anchorMax = new Vector2(1, 0.35f);
            bottomShadeRt.offsetMin = Vector2.zero;
            bottomShadeRt.offsetMax = Vector2.zero;

            // 中央地平线（带高光的水平带）
            var horizonBand = new GameObject("HorizonBand");
            horizonBand.transform.SetParent(canvas.transform);
            var horizonImg = horizonBand.AddComponent<Image>();
            horizonImg.color = new Color(0.85f, 0.55f, 0.20f, 0.18f);
            horizonImg.raycastTarget = false;
            var horizonRt = horizonBand.GetComponent<RectTransform>();
            horizonRt.anchorMin = new Vector2(0, 0.46f);
            horizonRt.anchorMax = new Vector2(1, 0.50f);
            horizonRt.offsetMin = Vector2.zero;
            horizonRt.offsetMax = Vector2.zero;

            // 角落装饰条（左上）
            CreateCornerAccent(canvas, new Vector2(0, 0), new Vector2(160, 4), SteelRain.UI.UIPalette.Primary, false, true);
            CreateCornerAccent(canvas, new Vector2(0, 0), new Vector2(4, 160), SteelRain.UI.UIPalette.Primary, false, true);
            // 右上
            CreateCornerAccent(canvas, new Vector2(0, 0), new Vector2(160, 4), SteelRain.UI.UIPalette.Accent, true, true);
            CreateCornerAccent(canvas, new Vector2(0, 0), new Vector2(4, 160), SteelRain.UI.UIPalette.Accent, true, true);
            // 左下
            CreateCornerAccent(canvas, new Vector2(0, 0), new Vector2(160, 4), SteelRain.UI.UIPalette.Accent, false, false);
            CreateCornerAccent(canvas, new Vector2(0, 0), new Vector2(4, 160), SteelRain.UI.UIPalette.Accent, false, false);
            // 右下
            CreateCornerAccent(canvas, new Vector2(0, 0), new Vector2(160, 4), SteelRain.UI.UIPalette.Primary, true, false);
            CreateCornerAccent(canvas, new Vector2(0, 0), new Vector2(4, 160), SteelRain.UI.UIPalette.Primary, true, false);
        }

        private static void CreateCornerAccent(GameObject parent, Vector2 anchoredPos, Vector2 size, Color color, bool mirrorX, bool top)
        {
            var go = new GameObject("CornerAccent");
            go.transform.SetParent(parent.transform);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(mirrorX ? 1 : 0, top ? 1 : 0);
            rt.anchorMax = new Vector2(mirrorX ? 1 : 0, top ? 1 : 0);
            rt.pivot = new Vector2(mirrorX ? 1 : 0, top ? 1 : 0);
            rt.anchoredPosition = new Vector2(mirrorX ? -40 : 40, top ? -40 : 40);
            rt.sizeDelta = size;
            // 水平条：拉伸 X；垂直条：拉伸 Y
            if (size.x > size.y)
                rt.sizeDelta = new Vector2(size.x, size.y);
        }

        private static void CreateUnderline(GameObject parent, string name, Vector2 pos, float width, float height, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, height);
        }

        private static void CreateDivider(GameObject parent, string name, Vector2 pos, float width, float height, Color color)
        {
            CreateUnderline(parent, name, pos, width, height, color);
        }

        private static Text CreateMenuText(GameObject canvas, string name, Vector2 pos, string content, int fontSize, Color color, FontStyle style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(canvas.transform);
            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = style;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null) text.font = font;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var rt = text.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(600, 60);
            return text;
        }

        private static Text CreateMenuText(GameObject canvas, string name, Vector2 pos, string content, int fontSize)
        {
            return CreateMenuText(canvas, name, pos, content, fontSize, Color.white, FontStyle.Normal);
        }

        private static Button CreateMenuButton(GameObject canvas, string name, Vector2 pos, string label, float width, float height, bool primary)
        {
            var go = new GameObject(name);
            go.transform.SetParent(canvas.transform);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, height);
            var img = go.AddComponent<Image>();
            img.color = primary ? SteelRain.UI.UIPalette.ButtonPrimary : SteelRain.UI.UIPalette.ButtonNormal;
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = primary ? SteelRain.UI.UIPalette.ButtonPrimary : SteelRain.UI.UIPalette.ButtonNormal;
            colors.highlightedColor = primary ? SteelRain.UI.UIPalette.ButtonPrimaryHover : SteelRain.UI.UIPalette.ButtonHover;
            colors.pressedColor = SteelRain.UI.UIPalette.ButtonPressed;
            colors.selectedColor = primary ? SteelRain.UI.UIPalette.ButtonPrimaryHover : SteelRain.UI.UIPalette.ButtonHover;
            colors.disabledColor = new Color(0.15f, 0.15f, 0.18f, 0.6f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            btn.colors = colors;

            var highlight = go.AddComponent<SteelRain.UI.ButtonHighlight>();
            var soHl = new SerializedObject(highlight);
            soHl.FindProperty("target").objectReferenceValue = img;
            soHl.FindProperty("normalColor").colorValue = primary ? SteelRain.UI.UIPalette.ButtonPrimary : SteelRain.UI.UIPalette.ButtonNormal;
            soHl.FindProperty("hoverColor").colorValue = primary ? SteelRain.UI.UIPalette.ButtonPrimaryHover : SteelRain.UI.UIPalette.ButtonHover;
            soHl.FindProperty("pressedColor").colorValue = SteelRain.UI.UIPalette.ButtonPressed;
            soHl.ApplyModifiedProperties();

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform);
            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.fontSize = 22;
            text.color = SteelRain.UI.UIPalette.TextPrimary;
            text.fontStyle = FontStyle.Bold;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null) text.font = font;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            return btn;
        }

        private static Button CreateMenuButton(GameObject canvas, string name, Vector2 pos, string label)
        {
            return CreateMenuButton(canvas, name, pos, label, 160, 40, false);
        }

        private static Slider CreateMenuSlider(GameObject parent, string name, Vector2 pos, string label)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(360, 50);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform);
            var labelText = labelGo.AddComponent<Text>();
            labelText.text = label;
            labelText.fontSize = 16;
            labelText.color = SteelRain.UI.UIPalette.TextSecondary;
            labelText.fontStyle = FontStyle.Bold;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null) labelText.font = font;
            labelText.alignment = TextAnchor.MiddleLeft;
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0, 0.5f);
            labelRt.anchorMax = new Vector2(0.4f, 1);
            labelRt.offsetMin = new Vector2(10, 0);
            labelRt.offsetMax = new Vector2(0, 0);

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.10f, 0.12f, 0.18f, 1f);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0.4f, 0.3f);
            bgRt.anchorMax = new Vector2(1, 0.7f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            var fillAreaGo = new GameObject("Fill Area");
            fillAreaGo.transform.SetParent(bgGo.transform);
            var fillAreaRt = fillAreaGo.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0, 0);
            fillAreaRt.anchorMax = new Vector2(1, 1);
            fillAreaRt.offsetMin = new Vector2(2, 2);
            fillAreaRt.offsetMax = new Vector2(-2, -2);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillAreaGo.transform);
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = SteelRain.UI.UIPalette.Accent;
            var fillImgRt = fillGo.GetComponent<RectTransform>();
            fillImgRt.anchorMin = Vector2.zero;
            fillImgRt.anchorMax = new Vector2(0.5f, 1);
            fillImgRt.offsetMin = Vector2.zero;
            fillImgRt.offsetMax = Vector2.zero;

            var handleAreaGo = new GameObject("Handle Slide Area");
            handleAreaGo.transform.SetParent(bgGo.transform);
            var handleAreaRt = handleAreaGo.AddComponent<RectTransform>();
            handleAreaRt.anchorMin = Vector2.zero;
            handleAreaRt.anchorMax = Vector2.one;
            handleAreaRt.offsetMin = new Vector2(5, 0);
            handleAreaRt.offsetMax = new Vector2(-5, 0);

            var handleGo = new GameObject("Handle");
            handleGo.transform.SetParent(handleAreaGo.transform);
            var handleImg = handleGo.AddComponent<Image>();
            handleImg.color = SteelRain.UI.UIPalette.TextPrimary;
            var handleRt = handleGo.GetComponent<RectTransform>();
            handleRt.anchorMin = new Vector2(0, 0);
            handleRt.anchorMax = new Vector2(0, 1);
            handleRt.sizeDelta = new Vector2(14, 0);

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillAreaRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            return slider;
        }

        private static Toggle CreateMenuToggle(GameObject parent, string name, Vector2 pos, string label)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(300, 36);

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.10f, 0.12f, 0.18f, 1f);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0);
            bgRt.anchorMax = new Vector2(0, 1);
            bgRt.sizeDelta = new Vector2(40, 0);
            bgRt.pivot = new Vector2(0, 0.5f);
            bgRt.anchoredPosition = new Vector2(10, 0);

            var checkGo = new GameObject("Checkmark");
            checkGo.transform.SetParent(bgGo.transform);
            var checkImg = checkGo.AddComponent<Image>();
            checkImg.color = SteelRain.UI.UIPalette.Accent;
            var checkRt = checkGo.GetComponent<RectTransform>();
            checkRt.anchorMin = new Vector2(0.2f, 0.2f);
            checkRt.anchorMax = new Vector2(0.8f, 0.8f);
            checkRt.offsetMin = Vector2.zero;
            checkRt.offsetMax = Vector2.zero;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform);
            var labelText = labelGo.AddComponent<Text>();
            labelText.text = label;
            labelText.fontSize = 16;
            labelText.color = SteelRain.UI.UIPalette.TextSecondary;
            labelText.fontStyle = FontStyle.Bold;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null) labelText.font = font;
            labelText.alignment = TextAnchor.MiddleLeft;
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0, 0);
            labelRt.anchorMax = new Vector2(1, 1);
            labelRt.offsetMin = new Vector2(60, 0);
            labelRt.offsetMax = new Vector2(0, 0);

            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = bgImg;
            toggle.graphic = checkImg;
            toggle.isOn = true;

            return toggle;
        }

        private static void BuildGameOverScreen()
        {
            var canvasGo = new GameObject("GameOverCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("GameOverPanel");
            panel.transform.SetParent(canvasGo.transform);
            var panelRt = panel.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.78f);

            CreateHudText(panel, "GameOverTitle", new Vector2(0, 80), "GAME OVER", 72, SteelRain.UI.UIPalette.Danger, FontStyle.Bold);
            CreateUnderline(panel, "GameOverUnderline", new Vector2(0, 30), 240, 3, SteelRain.UI.UIPalette.Danger);

            var goScoreText = CreateHudText(panel, "ScoreText", new Vector2(0, -20), "Score: 0\nHigh Score: 0", 20, SteelRain.UI.UIPalette.TextPrimary, FontStyle.Bold);
            goScoreText.alignment = TextAnchor.MiddleCenter;
            goScoreText.rectTransform.anchorMin = new Vector2(0.3f, 0.4f);
            goScoreText.rectTransform.anchorMax = new Vector2(0.7f, 0.5f);
            goScoreText.rectTransform.sizeDelta = Vector2.zero;

            var retryBtn = CreateMenuButton(panel, "RetryBtn", new Vector2(0, -90), "RETRY", 200, 50, true);
            var menuBtn = CreateMenuButton(panel, "MenuBtn", new Vector2(0, -150), "MAIN MENU", 200, 50, false);

            // 复活信标按钮（默认隐藏，有信标时显示）
            var reviveBtn = CreateMenuButton(panel, "ReviveBtn", new Vector2(0, -30), "使用复活信标", 260, 50, false);
            reviveBtn.gameObject.SetActive(false);
            var reviveTextComp = reviveBtn.GetComponentInChildren<Text>();
            if (reviveTextComp != null)
                reviveTextComp.text = "使用复活信标 (0)";

            var gameOver = canvasGo.AddComponent<GameOverScreen>();
            var so = new SerializedObject(gameOver);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("retryButton").objectReferenceValue = retryBtn;
            so.FindProperty("menuButton").objectReferenceValue = menuBtn;
            so.FindProperty("reviveButton").objectReferenceValue = reviveBtn;
            so.FindProperty("reviveText").objectReferenceValue = reviveTextComp;
            so.FindProperty("scoreText").objectReferenceValue = goScoreText;
            so.ApplyModifiedProperties();
        }

        private static void BuildVictoryScreen()
        {
            var canvasGo = new GameObject("VictoryCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("VictoryPanel");
            panel.transform.SetParent(canvasGo.transform);
            var panelRt = panel.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.78f);

            CreateHudText(panel, "VictoryTitle", new Vector2(0, 80), "VICTORY!", 72, SteelRain.UI.UIPalette.Success, FontStyle.Bold);
            CreateUnderline(panel, "VictoryUnderline", new Vector2(0, 30), 240, 3, SteelRain.UI.UIPalette.Success);
            CreateHudText(panel, "VictorySub", new Vector2(0, 0), "Mission Complete", 26, SteelRain.UI.UIPalette.TextSecondary, FontStyle.Normal);
            var victoryScoreText = CreateHudText(panel, "ScoreText", new Vector2(0, -40), "Score: 0\nHigh Score: 0", 20, SteelRain.UI.UIPalette.TextPrimary, FontStyle.Bold);
            victoryScoreText.alignment = TextAnchor.MiddleCenter;
            victoryScoreText.rectTransform.anchorMin = new Vector2(0.3f, 0.32f);
            victoryScoreText.rectTransform.anchorMax = new Vector2(0.7f, 0.42f);
            victoryScoreText.rectTransform.sizeDelta = Vector2.zero;
            var nextLevelBtn = CreateMenuButton(panel, "NextLevelBtn", new Vector2(0, -110), "NEXT LEVEL", 200, 50, true);
            var menuBtn = CreateMenuButton(panel, "VictoryMenuBtn", new Vector2(0, -170), "MAIN MENU", 200, 50, false);

            var victory = canvasGo.AddComponent<VictoryScreen>();
            var so = new SerializedObject(victory);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("menuButton").objectReferenceValue = menuBtn;
            so.FindProperty("nextLevelButton").objectReferenceValue = nextLevelBtn;
            so.FindProperty("scoreText").objectReferenceValue = victoryScoreText;
            so.ApplyModifiedProperties();
        }

        private static void BuildBossHealthBar()
        {
            var canvasGo = new GameObject("BossHealthCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("BossHealthPanel");
            panel.transform.SetParent(canvasGo.transform);
            var panelRt = panel.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.25f, 0.9f);
            panelRt.anchorMax = new Vector2(0.75f, 0.95f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            var fillBg = new GameObject("FillBackground");
            fillBg.transform.SetParent(panel.transform);
            var fillBgImg = fillBg.AddComponent<Image>();
            fillBgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            var fillBgRt = fillBg.GetComponent<RectTransform>();
            fillBgRt.anchorMin = new Vector2(0.05f, 0.15f);
            fillBgRt.anchorMax = new Vector2(0.95f, 0.85f);
            fillBgRt.offsetMin = Vector2.zero;
            fillBgRt.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillBg.transform);
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.9f, 0.2f, 0.2f, 1f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            var nameText = CreateMenuText(panel, "BossName", new Vector2(0, 12), "BOSS", 14);
            var nameRt = nameText.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0);
            nameRt.anchorMax = new Vector2(1, 0.5f);
            nameRt.offsetMin = Vector2.zero;
            nameRt.offsetMax = Vector2.zero;

            var bossBar = canvasGo.AddComponent<BossHealthBar>();
            var so = new SerializedObject(bossBar);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("fillImage").objectReferenceValue = fillImg;
            so.FindProperty("nameText").objectReferenceValue = nameText;
            so.ApplyModifiedProperties();
        }
    }
}
#endif
