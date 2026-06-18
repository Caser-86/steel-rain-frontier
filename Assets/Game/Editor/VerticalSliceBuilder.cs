#if UNITY_EDITOR
using System;
using System.IO;
using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Enemies;
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
            CreateMiniBossPrefab();
            var upgradePickupPrefab = CreateUpgradePickupPrefab();
            var healthPickupPrefab = CreateHealthPickupPrefab();
            var cratePrefab = CreateCratePrefab(healthPickupPrefab);
            BuildLevel01Scene(playerPrefab, enemyPrefab, enemyDefs, projectilePrefab,
                upgradePickupPrefab, cratePrefab, healthPickupPrefab, characters,
                shieldPrefab, dronePrefab, grenadierPrefab, weapons);
            BuildLevel02Scene(playerPrefab, enemyPrefab, enemyDefs, projectilePrefab,
                upgradePickupPrefab, cratePrefab, healthPickupPrefab, characters,
                shieldPrefab, dronePrefab, grenadierPrefab);
            CreateBootScene();
            CreateMainMenuScene();
            RegisterBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[VerticalSliceBuilder] Build complete!");
        }

        [MenuItem("Steel Rain/Build Windows")]
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

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
            if (existing != null) return existing;

            var aila = ScriptableObject.CreateInstance<CharacterDefinition>();
            aila.id = "aila";
            aila.displayName = "Aila";
            aila.maxHealth = 6;
            aila.moveSpeed = 7.2f;
            aila.jumpVelocity = 9.5f;
            aila.gravityScale = 3.2f;
            aila.fallGravityMultiplier = 1.35f;
            aila.crouchSpeedMultiplier = 0.45f;
            aila.crouchColliderHeightMultiplier = 0.6f;
            aila.dodgeSpeed = 12f;
            aila.dodgeDuration = 0.16f;
            aila.dodgeCooldown = 0.65f;
            aila.skillId = CharacterSkillId.BreakthroughFire;
            aila.tintColor = new Color(0.7f, 0.85f, 1f, 1f);
            AssetDatabase.CreateAsset(aila, path);
            return aila;
        }

        private static CharacterDefinition[] CreateAllCharacterData()
        {
            var aila = CreateCharacterData();

            var brunoPath = $"{DataDir}/Characters/Bruno.asset";
            var bruno = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(brunoPath);
            if (bruno == null)
            {
                bruno = ScriptableObject.CreateInstance<CharacterDefinition>();
                bruno.id = "bruno";
                bruno.displayName = "Bruno";
                bruno.maxHealth = 8;
                bruno.moveSpeed = 6.0f;
                bruno.jumpVelocity = 8.2f;
                bruno.gravityScale = 3.6f;
                bruno.fallGravityMultiplier = 1.45f;
                bruno.crouchSpeedMultiplier = 0.4f;
                bruno.crouchColliderHeightMultiplier = 0.6f;
                bruno.dodgeSpeed = 10f;
                bruno.dodgeDuration = 0.18f;
                bruno.dodgeCooldown = 0.75f;
                bruno.skillId = CharacterSkillId.TrenchShield;
                bruno.tintColor = new Color(0.8f, 0.6f, 0.4f, 1f);
                AssetDatabase.CreateAsset(bruno, brunoPath);
            }

            var maraPath = $"{DataDir}/Characters/Mara.asset";
            var mara = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(maraPath);
            if (mara == null)
            {
                mara = ScriptableObject.CreateInstance<CharacterDefinition>();
                mara.id = "mara";
                mara.displayName = "Mara";
                mara.maxHealth = 6;
                mara.moveSpeed = 7.0f;
                mara.jumpVelocity = 9.0f;
                mara.gravityScale = 3.3f;
                mara.fallGravityMultiplier = 1.35f;
                mara.crouchSpeedMultiplier = 0.45f;
                mara.crouchColliderHeightMultiplier = 0.6f;
                mara.dodgeSpeed = 11f;
                mara.dodgeDuration = 0.16f;
                mara.dodgeCooldown = 0.65f;
                mara.skillId = CharacterSkillId.BombardmentMatrix;
                mara.tintColor = new Color(0.9f, 0.5f, 0.8f, 1f);
                AssetDatabase.CreateAsset(mara, maraPath);
            }

            var nikoPath = $"{DataDir}/Characters/Niko.asset";
            var niko = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(nikoPath);
            if (niko == null)
            {
                niko = ScriptableObject.CreateInstance<CharacterDefinition>();
                niko.id = "niko";
                niko.displayName = "Niko";
                niko.maxHealth = 5;
                niko.moveSpeed = 7.5f;
                niko.jumpVelocity = 9.8f;
                niko.gravityScale = 3.1f;
                niko.fallGravityMultiplier = 1.25f;
                niko.crouchSpeedMultiplier = 0.45f;
                niko.crouchColliderHeightMultiplier = 0.6f;
                niko.dodgeSpeed = 13f;
                niko.dodgeDuration = 0.15f;
                niko.dodgeCooldown = 0.6f;
                niko.skillId = CharacterSkillId.TimeRift;
                niko.tintColor = new Color(0.5f, 1f, 0.7f, 1f);
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
            var defs = new EnemyDefinition[4];
            defs[0] = MakeEnemy("RifleSoldier", "Rifle Soldier", 3, 2.5f, 6f, 1.4f, EnemyAttackPattern.RifleBurst, new Color(0.8f, 0.4f, 0.3f));
            defs[1] = MakeEnemy("ShieldSoldier", "Shield Soldier", 6, 1.6f, 2f, 2f, EnemyAttackPattern.ShieldAdvance, new Color(0.5f, 0.6f, 0.4f));
            defs[2] = MakeEnemy("Drone", "Drone", 2, 4.2f, 5f, 1.8f, EnemyAttackPattern.DroneDive, new Color(0.4f, 0.5f, 0.6f));
            defs[3] = MakeEnemy("Grenadier", "Grenadier", 3, 2f, 7f, 2.5f, EnemyAttackPattern.GrenadeArc, new Color(0.6f, 0.5f, 0.2f));
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

        // ===== 预制体 =====

        private static Projectile CreateProjectilePrefab()
        {
            var path = $"{PrefabDir}/Projectile.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<Projectile>(path);
            if (existing != null) return existing;

            var go = new GameObject("Projectile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/bullet_player.png");
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
            if (existing != null) return existing;

            var go = new GameObject("Player_Aila");
            go.tag = "Player";

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/player_aila.png");
            sr.sortingOrder = 5;
            sr.color = Color.white;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3.2f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.2f);

            var health = go.AddComponent<Health>();
            var controller = go.AddComponent<PlayerController2D>();
            var combat = go.AddComponent<PlayerCombat>();
            var dodge = go.AddComponent<PlayerDodge>();
            go.AddComponent<HitFlash>();
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
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/enemy_rifle.png");
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
            firePoint.transform.localPosition = new Vector3(0.5f, 0f, 0f);

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
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/enemy_shield.png");
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
            shieldSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/enemy_shield.png");
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
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/enemy_drone.png");
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
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/enemy_rifle.png");
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

        private static void CreateMiniBossPrefab()
        {
            var path = $"{PrefabDir}/MiniBoss_Walker.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            var go = new GameObject("MiniBoss_Walker");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/miniboss_walker.png");
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

        private static GameObject CreateUpgradePickupPrefab()
        {
            var path = $"{PrefabDir}/Pickup_Upgrade.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var go = new GameObject("Pickup_Upgrade");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/upgrade_capsule.png");
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
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/upgrade_capsule.png");
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
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/upgrade_capsule.png");
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
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/crate.png");
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
            WeaponDefinition[] weapons)
        {
            Debug.Log("[VerticalSliceBuilder] Building Level01 scene...");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 摄像机
            var camGo = new GameObject("MainCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(0.12f, 0.1f, 0.18f);
            camGo.tag = "MainCamera";
            camGo.AddComponent<CameraShake>();
            var camFollow = camGo.AddComponent<SimpleCameraFollow>();

            // 地面层
            var groundLayer = 6;
            var groundGo = new GameObject("Ground");
            BuildGround(groundGo, groundLayer);

            // 玩家
            var player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            player.transform.position = new Vector3(0f, 1f, 0f);
            var controller = player.GetComponent<PlayerController2D>();
            var combat = player.GetComponent<PlayerCombat>();

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

            // 段落 A：海滩 0-45 — 步枪兵教学
            CreateSegment("SegmentA1_Trigger", new Vector3(8f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 2);
            CreateSegment("SegmentA2_Trigger", new Vector3(18f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 3);
            CreateSegment("SegmentA3_Trigger", new Vector3(28f, 0f, 0f), grenadierPrefab, enemyDefs[3], player.transform, 2);
            CreateCheckpoint("Checkpoint_A", new Vector3(32f, 1.5f, 0f), cpMgr);

            // 段落 B：村落 45-95 — 盾兵引入
            CreateSegment("SegmentB1_Trigger", new Vector3(48f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 3);
            CreateSegment("SegmentB2_Trigger", new Vector3(58f, 0f, 0f), shieldPrefab, enemyDefs[1], player.transform, 2);
            CreateSegment("SegmentB3_Trigger", new Vector3(72f, 0f, 0f), grenadierPrefab, enemyDefs[3], player.transform, 2);
            CreateCheckpoint("Checkpoint_B", new Vector3(80f, 1.5f, 0f), cpMgr);

            // 段落 C：壕沟 95-150 — 无人机+混合
            CreateSegment("SegmentC1_Trigger", new Vector3(98f, 0f, 0f), enemyPrefab, enemyDefs[0], player.transform, 3);
            CreateSegment("SegmentC2_Trigger", new Vector3(108f, 0f, 0f), dronePrefab, enemyDefs[2], player.transform, 3);
            CreateSegment("SegmentC3_Trigger", new Vector3(118f, 0f, 0f), shieldPrefab, enemyDefs[1], player.transform, 2);
            CreateCheckpoint("Checkpoint_C", new Vector3(124f, 1.5f, 0f), cpMgr);

            // 小 Boss 竞技场
            var bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/MiniBoss_Walker.prefab");
            if (bossPrefab != null)
            {
                var boss = PrefabUtility.InstantiatePrefab(bossPrefab) as GameObject;
                boss.transform.position = new Vector3(140f, 1.5f, 0f);
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

            // 音效管理器
            BuildAudioManager();

            // 背景音乐播放器
            BuildMusicPlayer(player.transform);

            // 升级胶囊：3 个主线路径 + 2 个备用
            var shotgun = weapons[1];
            var rocket = weapons[2];
            var shotgunPickupPrefab = CreateWeaponPickupPrefab(shotgun, new Color(0.9f, 0.4f, 0.1f));
            var rocketPickupPrefab = CreateWeaponPickupPrefab(rocket, new Color(0.9f, 0.1f, 0.1f));

            PlacePickup(shotgunPickupPrefab, new Vector3(35f, 1.5f, 0f), "Weapon_Shotgun");
            PlacePickup(rocketPickupPrefab, new Vector3(105f, 1.5f, 0f), "Weapon_Rocket");
            PlacePickup(upgradePickupPrefab, new Vector3(18f, 1.5f, 0f), "Upgrade_Lv1_Beach");
            PlacePickup(upgradePickupPrefab, new Vector3(48f, 1.5f, 0f), "Upgrade_Lv2_Village");
            PlacePickup(upgradePickupPrefab, new Vector3(112f, 1.5f, 0f), "Upgrade_Lv3_BossPrep");
            PlacePickup(upgradePickupPrefab, new Vector3(54f, 4f, 0f), "Upgrade_Backup_High");
            PlacePickup(upgradePickupPrefab, new Vector3(108f, 1.5f, 0f), "Upgrade_Backup_Armory");

            // 可破坏箱子
            PlacePickup(cratePrefab, new Vector3(8f, 1f, 0f), "Crate_1");
            PlacePickup(cratePrefab, new Vector3(15f, 1f, 0f), "Crate_2");
            PlacePickup(cratePrefab, new Vector3(50f, 1f, 0f), "Crate_3");
            PlacePickup(cratePrefab, new Vector3(100f, 1f, 0f), "Crate_4");

            // 健康药剂
            PlacePickup(healthPickupPrefab, new Vector3(25f, 1.5f, 0f), "Health_1");
            PlacePickup(healthPickupPrefab, new Vector3(75f, 1.5f, 0f), "Health_2");
            PlacePickup(healthPickupPrefab, new Vector3(115f, 1.5f, 0f), "Health_3");

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
            endRt.anchoredPosition = new Vector2(155f, 2f);
            levelEnd.AddComponent<LevelEndTrigger>();
            levelEnd.transform.position = new Vector3(155f, 2f, 0f);

            // 教程提示
            BuildTutorialPrompts();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[VerticalSliceBuilder] Level01 scene saved.");
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
            GameObject shieldPrefab, GameObject dronePrefab, GameObject grenadierPrefab)
        {
            Debug.Log("[VerticalSliceBuilder] Building Level02 scene...");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("MainCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(0.15f, 0.08f, 0.05f);
            camGo.tag = "MainCamera";
            camGo.AddComponent<CameraShake>();
            var camFollow = camGo.AddComponent<SimpleCameraFollow>();

            var groundLayer = 6;
            var groundGo = new GameObject("Ground");
            BuildFactoryGround(groundGo, groundLayer);

            var player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            player.transform.position = new Vector3(0f, 2f, 0f);
            var controller = player.GetComponent<PlayerController2D>();
            var combat = player.GetComponent<PlayerCombat>();

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

            var bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/MiniBoss_Walker.prefab");
            if (bossPrefab != null)
            {
                var boss = PrefabUtility.InstantiatePrefab(bossPrefab) as GameObject;
                boss.transform.position = new Vector3(150f, 1.5f, 0f);
                var bossComp = boss.GetComponent<MiniBossWalker>();
                if (bossComp != null) bossComp.AssignTarget(player.transform);
            }

            var particleGo = new GameObject("ParticleSpawner");
            particleGo.AddComponent<ParticleSpawner>();

            var dmgNumGo = new GameObject("DamageNumberSpawner");
            dmgNumGo.AddComponent<DamageNumberSpawner>();

            BuildPauseMenu();
            BuildAudioManager();
            BuildMusicPlayer(player.transform);
            PlacePickup(upgradePickupPrefab, new Vector3(20f, 1.5f, 0f), "L2_Upgrade_1");
            PlacePickup(upgradePickupPrefab, new Vector3(70f, 1.5f, 0f), "L2_Upgrade_2");
            PlacePickup(cratePrefab, new Vector3(30f, 1f, 0f), "L2_Crate_1");
            PlacePickup(healthPickupPrefab, new Vector3(55f, 1.5f, 0f), "L2_Health_1");
            PlacePickup(healthPickupPrefab, new Vector3(115f, 1.5f, 0f), "L2_Health_2");

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

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Level02_Factory.unity");
            Debug.Log("[VerticalSliceBuilder] Level02 scene saved.");
        }

        private static void CreateHazard(Vector3 pos, string name)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/ground_trench.png");
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
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/crate.png");
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
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/ground_beach.png");
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
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/ground_village.png");
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
            var groundSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/ground_village.png");
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
            var groundSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/ground_beach.png");

            // 主地面：从 x=-5 到 x=150
            float startX = -5f;
            float endX = 150f;
            float groundY = 0f;

            for (float x = startX; x < endX; x += 1f)
            {
                var block = new GameObject($"Ground_{x}");
                block.transform.SetParent(parent.transform);
                block.transform.position = new Vector3(x, groundY - 0.5f, 0f);
                var sr = block.AddComponent<SpriteRenderer>();
                sr.sprite = groundSprite;
                sr.sortingOrder = 0;
                block.layer = layer;

                var col = block.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 1f);
            }

            // 一些平台
            CreatePlatform(parent, 20f, 2.5f, 3f, groundSprite, layer);
            CreatePlatform(parent, 50f, 3f, 4f, groundSprite, layer);
            CreatePlatform(parent, 75f, 2.5f, 3f, groundSprite, layer);
            CreatePlatform(parent, 100f, 3.5f, 5f, groundSprite, layer);
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
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/checkpoint_flag.png");
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

        private static void BuildParallaxBackground(GameObject camera)
        {
            var bgGo = new GameObject("ParallaxBackground");
            var parallax = bgGo.AddComponent<ParallaxBackground>();

            var skySprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/background_sky.png");
            var seaSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/background_sea.png");

            // 天空层
            var skyGo = new GameObject("SkyLayer");
            skyGo.transform.SetParent(bgGo.transform);
            skyGo.transform.position = new Vector3(0f, 8f, 5f);
            var skySr = skyGo.AddComponent<SpriteRenderer>();
            skySr.sprite = skySprite;
            skySr.drawMode = SpriteDrawMode.Sliced;
            skySr.size = new Vector2(200f, 40f);
            skySr.sortingOrder = -10;
            skySr.color = new Color(0.15f, 0.1f, 0.2f, 1f);

            // 海洋层
            var seaGo = new GameObject("SeaLayer");
            seaGo.transform.SetParent(bgGo.transform);
            seaGo.transform.position = new Vector3(0f, 2f, 4f);
            var seaSr = seaGo.AddComponent<SpriteRenderer>();
            seaSr.sprite = seaSprite;
            seaSr.drawMode = SpriteDrawMode.Sliced;
            seaSr.size = new Vector2(200f, 10f);
            seaSr.sortingOrder = -8;
            seaSr.color = new Color(0.1f, 0.2f, 0.35f, 0.8f);

            var so = new SerializedObject(parallax);
            var layersProp = so.FindProperty("layers");
            layersProp.arraySize = 2;

            var layer0 = layersProp.GetArrayElementAtIndex(0);
            layer0.FindPropertyRelative("transform").objectReferenceValue = skyGo.transform;
            layer0.FindPropertyRelative("parallaxFactor").floatValue = 0.1f;
            layer0.FindPropertyRelative("horizontalExtent").floatValue = 100f;

            var layer1 = layersProp.GetArrayElementAtIndex(1);
            layer1.FindPropertyRelative("transform").objectReferenceValue = seaGo.transform;
            layer1.FindPropertyRelative("parallaxFactor").floatValue = 0.3f;
            layer1.FindPropertyRelative("horizontalExtent").floatValue = 100f;

            so.FindProperty("camera").objectReferenceValue = camera.transform;
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

            var titleText = CreateHudText(canvasGo, "PauseTitle", new Vector2(0, 40), "PAUSED", 48);
            var hintText = CreateHudText(canvasGo, "PauseHint", new Vector2(0, -20), "ESC Resume | Q Quit | R Restart", 20);

            var pm = canvasGo.AddComponent<PauseManager>();
            var so = new SerializedObject(pm);
            so.FindProperty("pausePanel").objectReferenceValue = panel;
            so.FindProperty("pauseTitle").objectReferenceValue = titleText;
            so.FindProperty("pauseHint").objectReferenceValue = hintText;
            so.ApplyModifiedProperties();

            panel.SetActive(false);
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
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            var go = new GameObject("Bootstrap");
            go.AddComponent<GameBootstrap>();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Boot.unity");
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

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("MainCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            camGo.tag = "MainCamera";

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
            var settingsBtn = CreateMenuButton(menuContent, "SettingsBtn", new Vector2(0, -225), "SETTINGS", 200, 42, false);
            var quitBtn = CreateMenuButton(menuContent, "QuitBtn", new Vector2(0, -280), "QUIT", 200, 42, false);

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
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

            CreateHudText(canvasGo, "GameOverTitle", new Vector2(0, 80), "GAME OVER", 72, SteelRain.UI.UIPalette.Danger, FontStyle.Bold);
            CreateUnderline(canvasGo, "GameOverUnderline", new Vector2(0, 30), 240, 3, SteelRain.UI.UIPalette.Danger);

            var goScoreText = CreateHudText(canvasGo, "ScoreText", new Vector2(0, -20), "Score: 0\nHigh Score: 0", 20, SteelRain.UI.UIPalette.TextPrimary, FontStyle.Bold);
            goScoreText.alignment = TextAnchor.MiddleCenter;
            goScoreText.rectTransform.anchorMin = new Vector2(0.3f, 0.4f);
            goScoreText.rectTransform.anchorMax = new Vector2(0.7f, 0.5f);
            goScoreText.rectTransform.sizeDelta = Vector2.zero;

            var retryBtn = CreateMenuButton(canvasGo, "RetryBtn", new Vector2(0, -90), "RETRY", 200, 50, true);
            var menuBtn = CreateMenuButton(canvasGo, "MenuBtn", new Vector2(0, -150), "MAIN MENU", 200, 50, false);

            var gameOver = canvasGo.AddComponent<GameOverScreen>();
            var so = new SerializedObject(gameOver);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("retryButton").objectReferenceValue = retryBtn;
            so.FindProperty("menuButton").objectReferenceValue = menuBtn;
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

            CreateHudText(canvasGo, "VictoryTitle", new Vector2(0, 80), "VICTORY!", 72, SteelRain.UI.UIPalette.Success, FontStyle.Bold);
            CreateUnderline(canvasGo, "VictoryUnderline", new Vector2(0, 30), 240, 3, SteelRain.UI.UIPalette.Success);
            CreateHudText(canvasGo, "VictorySub", new Vector2(0, 0), "Mission Complete", 26, SteelRain.UI.UIPalette.TextSecondary, FontStyle.Normal);
            var victoryScoreText = CreateHudText(canvasGo, "ScoreText", new Vector2(0, -40), "Score: 0\nHigh Score: 0", 20, SteelRain.UI.UIPalette.TextPrimary, FontStyle.Bold);
            victoryScoreText.alignment = TextAnchor.MiddleCenter;
            victoryScoreText.rectTransform.anchorMin = new Vector2(0.3f, 0.32f);
            victoryScoreText.rectTransform.anchorMax = new Vector2(0.7f, 0.42f);
            victoryScoreText.rectTransform.sizeDelta = Vector2.zero;
            var nextLevelBtn = CreateMenuButton(canvasGo, "NextLevelBtn", new Vector2(0, -110), "NEXT LEVEL", 200, 50, true);
            var menuBtn = CreateMenuButton(canvasGo, "VictoryMenuBtn", new Vector2(0, -170), "MAIN MENU", 200, 50, false);

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
