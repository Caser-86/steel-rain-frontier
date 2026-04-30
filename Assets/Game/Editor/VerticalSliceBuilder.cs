using System.IO;
using SteelRain.Core;
using SteelRain.Enemies;
using SteelRain.Levels;
using SteelRain.Player;
using SteelRain.Weapons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SteelRain.EditorTools
{
    public static class VerticalSliceBuilder
    {
        private const string DataRoot = "Assets/Game/Data";
        private const string PrefabRoot = "Assets/Game/Prefabs";
        private const string SceneRoot = "Assets/Scenes";

        [MenuItem("Steel Rain/Build Vertical Slice Assets")]
        public static void BuildAssets()
        {
            EnsureFolders();
            var sprite = EnsureWhiteSprite();
            EnsureMaterials();
            var aila = CreateAila();
            var projectile = CreateProjectilePrefab(sprite);
            var enemyProjectile = CreateEnemyProjectilePrefab(sprite);
            var assaultRifle = CreateAssaultRifle(projectile);
            CreateShotgun(projectile);
            CreateRocketLauncher(projectile);
            CreatePlayerPrefab(sprite, aila, assaultRifle);
            var enemies = CreateEnemyDefinitions(enemyProjectile);
            CreateEnemyPrefabs(sprite, enemies);
            CreateMiniBossPrefab(sprite, enemyProjectile);
            CreateWaveAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Steel Rain vertical slice assets built.");
        }

        [MenuItem("Steel Rain/Build Level 01 Graybox")]
        public static void BuildLevel01Graybox()
        {
            BuildAssets();
            EnsureFolders();
            EnsureMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level01_VerticalSlice";

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Generated/white-square.png");
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/Player/Player_Aila.prefab");
            var miniBossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/Enemies/MiniBoss_Walker.prefab");
            var player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            player.transform.position = new Vector3(0f, 1.5f, 0f);

            CreateCamera(player.transform);
            CreateGround(sprite);
            CreateHud();
            CreateCheckpointManager(player.transform);
            CreateSegmentTrigger("BeachWaveTrigger", 12f, "BeachWave.asset", player.transform);
            CreateSegmentTrigger("VillageWaveTrigger", 55f, "VillageWave.asset", player.transform);
            CreateSegmentTrigger("TrenchWaveTrigger", 105f, "TrenchWave.asset", player.transform);
            CreateCheckpoint("Checkpoint_A", 45f);
            CreateCheckpoint("Checkpoint_B", 95f);
            CreateCheckpoint("Checkpoint_C", 150f);
            CreateRescueNpc(sprite, 72f);

            var miniBoss = PrefabUtility.InstantiatePrefab(miniBossPrefab) as GameObject;
            miniBoss.transform.position = new Vector3(166f, 2.2f, 0f);
            miniBoss.GetComponent<MiniBossWalker>().AssignTarget(player.transform);

            CreateBlock(sprite, "MiniBossArenaWallLeft", new Vector2(150f, 3f), new Vector2(1f, 6f), "Mat_Wall");
            CreateBlock(sprite, "MiniBossArenaWallRight", new Vector2(185f, 3f), new Vector2(1f, 6f), "Mat_Wall");

            EditorSceneManager.SaveScene(scene, $"{SceneRoot}/Level01_VerticalSlice.unity");
            Debug.Log("Steel Rain Level01 vertical slice graybox built.");
        }

        private static void EnsureFolders()
        {
            CreateFolder("Assets/Game", "Generated");
            CreateFolder("Assets/Game", "Data");
            CreateFolder(DataRoot, "Characters");
            CreateFolder(DataRoot, "Weapons");
            CreateFolder(DataRoot, "Enemies");
            CreateFolder(DataRoot, "Levels");
            CreateFolder($"{DataRoot}/Levels", "Level01");
            CreateFolder($"{DataRoot}/Levels/Level01", "Waves");
            CreateFolder("Assets/Game", "Prefabs");
            CreateFolder(PrefabRoot, "Player");
            CreateFolder(PrefabRoot, "Weapons");
            CreateFolder(PrefabRoot, "Enemies");
            CreateFolder(PrefabRoot, "Pickups");
            CreateFolder("Assets", "Scenes");
        }

        private static void CreateFolder(string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static Sprite EnsureWhiteSprite()
        {
            const string path = "Assets/Game/Generated/white-square.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(16, 16);
                var pixels = new Color[16 * 16];
                for (var i = 0; i < pixels.Length; i++)
                    pixels[i] = Color.white;

                texture.SetPixels(pixels);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureMaterials()
        {
            CreateMaterial("Mat_Player", new Color(0.05f, 0.95f, 1f));
            CreateMaterial("Mat_Enemy", new Color(1f, 0.08f, 0.04f));
            CreateMaterial("Mat_Drone", new Color(1f, 0f, 1f));
            CreateMaterial("Mat_Boss", new Color(1f, 0.55f, 0.05f));
            CreateMaterial("Mat_PlayerProjectile", Color.yellow);
            CreateMaterial("Mat_EnemyProjectile", new Color(1f, 0.25f, 0.1f));
            CreateMaterial("Mat_Beach", new Color(0.95f, 0.82f, 0.28f));
            CreateMaterial("Mat_Village", new Color(0.75f, 0.44f, 0.18f));
            CreateMaterial("Mat_Roof", new Color(0.65f, 0.08f, 0.08f));
            CreateMaterial("Mat_Trench", new Color(0.22f, 0.78f, 0.22f));
            CreateMaterial("Mat_Arena", new Color(0.42f, 0.42f, 0.46f));
            CreateMaterial("Mat_Wall", new Color(0.25f, 0.25f, 0.25f));
            CreateMaterial("Mat_Rescue", Color.green);
        }

        private static void CreateMaterial(string name, Color color)
        {
            var path = $"Assets/Game/Generated/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Sprites/Default"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
        }

        private static CharacterDefinition CreateAila()
        {
            var aila = LoadOrCreate<CharacterDefinition>($"{DataRoot}/Characters/Aila.asset");
            aila.id = "aila";
            aila.displayName = "Aila";
            aila.maxHealth = 6;
            aila.moveSpeed = 7.5f;
            aila.jumpVelocity = 9.5f;
            aila.gravityScale = 3.2f;
            aila.fallGravityMultiplier = 1.35f;
            aila.dodgeSpeed = 12f;
            aila.dodgeDuration = 0.16f;
            aila.dodgeCooldown = 0.65f;
            aila.crouchSpeedMultiplier = 0.45f;
            aila.crouchColliderHeightMultiplier = 0.6f;
            EditorUtility.SetDirty(aila);
            return aila;
        }

        private static Projectile CreateProjectilePrefab(Sprite sprite)
        {
            var go = new GameObject("Projectile_Bullet");
            AddVisualQuad(go, "Mat_PlayerProjectile");
            go.transform.localScale = new Vector3(0.22f, 0.08f, 1f);
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            go.AddComponent<Projectile>();
            var prefab = SavePrefab<Projectile>(go, $"{PrefabRoot}/Weapons/Projectile_Bullet.prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static EnemyProjectile CreateEnemyProjectilePrefab(Sprite sprite)
        {
            var go = new GameObject("Projectile_Enemy");
            AddVisualQuad(go, "Mat_EnemyProjectile");
            go.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            go.AddComponent<EnemyProjectile>();
            var prefab = SavePrefab<EnemyProjectile>(go, $"{PrefabRoot}/Weapons/Projectile_Enemy.prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static WeaponDefinition CreateAssaultRifle(Projectile projectile)
        {
            var auto = CreateWeaponForm("AssaultRifle_Auto", "auto", "Auto", ProjectilePattern.Single, 1, 1, 9f, 18f, 1, 0f, 0, 0f);
            var pierce = CreateWeaponForm("AssaultRifle_Pierce", "pierce", "Pierce", ProjectilePattern.Piercing, 2, 2, 4f, 22f, 1, 0f, 2, 0f);
            var grenade = CreateWeaponForm("AssaultRifle_Grenade", "grenade", "Grenade", ProjectilePattern.LobbedExplosive, 4, 5, 1.2f, 12f, 1, 0f, 0, 2f);
            return CreateWeapon("AssaultRifle", "assault_rifle", "Assault Rifle", 90, projectile, auto, pierce, grenade);
        }

        private static WeaponDefinition CreateShotgun(Projectile projectile)
        {
            var scatter = CreateWeaponForm("Shotgun_Scatter", "scatter", "Scatter", ProjectilePattern.Spread, 1, 2, 1.8f, 15f, 6, 35f, 0, 0f);
            var slug = CreateWeaponForm("Shotgun_Slug", "slug", "Slug", ProjectilePattern.Single, 5, 3, 1.1f, 24f, 1, 0f, 0, 0f);
            var burning = CreateWeaponForm("Shotgun_Burning", "burning", "Burning", ProjectilePattern.Spread, 2, 4, 1f, 14f, 3, 20f, 0, 0f);
            return CreateWeapon("Shotgun", "shotgun", "Shotgun", 36, projectile, scatter, slug, burning);
        }

        private static WeaponDefinition CreateRocketLauncher(Projectile projectile)
        {
            var direct = CreateWeaponForm("RocketLauncher_Direct", "direct", "Direct", ProjectilePattern.Single, 8, 1, 0.7f, 14f, 1, 0f, 0, 2.4f);
            var split = CreateWeaponForm("RocketLauncher_Split", "split", "Split", ProjectilePattern.SplitRocket, 4, 2, 0.6f, 13f, 1, 0f, 0, 1.8f);
            var seeker = CreateWeaponForm("RocketLauncher_Seeker", "seeker", "Seeker", ProjectilePattern.Single, 5, 2, 0.8f, 15f, 1, 0f, 0, 1.6f);
            return CreateWeapon("RocketLauncher", "rocket_launcher", "Rocket Launcher", 12, projectile, direct, split, seeker);
        }

        private static WeaponFormDefinition CreateWeaponForm(
            string assetName,
            string id,
            string displayName,
            ProjectilePattern pattern,
            int damage,
            int ammoCost,
            float fireRate,
            float projectileSpeed,
            int projectileCount,
            float spreadAngle,
            int pierceCount,
            float explosionRadius)
        {
            var form = LoadOrCreate<WeaponFormDefinition>($"{DataRoot}/Weapons/{assetName}.asset");
            form.id = id;
            form.displayName = displayName;
            form.pattern = pattern;
            form.damage = damage;
            form.ammoCost = ammoCost;
            form.fireRate = fireRate;
            form.projectileSpeed = projectileSpeed;
            form.projectileCount = projectileCount;
            form.spreadAngle = spreadAngle;
            form.pierceCount = pierceCount;
            form.explosionRadius = explosionRadius;
            EditorUtility.SetDirty(form);
            return form;
        }

        private static WeaponDefinition CreateWeapon(string assetName, string id, string displayName, int ammo, Projectile projectile, params WeaponFormDefinition[] forms)
        {
            var weapon = LoadOrCreate<WeaponDefinition>($"{DataRoot}/Weapons/{assetName}.asset");
            weapon.id = id;
            weapon.displayName = displayName;
            weapon.startingAmmo = ammo;
            weapon.projectilePrefab = projectile;
            weapon.forms = forms;
            EditorUtility.SetDirty(weapon);
            return weapon;
        }

        private static void CreatePlayerPrefab(Sprite sprite, CharacterDefinition aila, WeaponDefinition startingWeapon)
        {
            var go = new GameObject("Player_Aila");
            go.tag = "Player";
            go.transform.position = new Vector3(0f, 1.5f, 0f);
            AddVisualQuad(go, "Mat_Player");
            go.transform.localScale = new Vector3(1.8f, 2.8f, 1f);

            var body = go.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            go.AddComponent<Health>();
            var controller = go.AddComponent<PlayerController2D>();
            var dodge = go.AddComponent<PlayerDodge>();
            var combat = go.AddComponent<PlayerCombat>();

            var groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(go.transform);
            groundCheck.localPosition = new Vector3(0f, -0.55f, 0f);

            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(go.transform);
            muzzle.localPosition = new Vector3(0.65f, 0.05f, 0f);

            SetObject(controller, "character", aila);
            SetObject(controller, "groundCheck", groundCheck);
            SetInt(controller, "groundMask", 1);
            SetObject(dodge, "controller", controller);
            SetObject(combat, "controller", controller);
            SetObject(combat, "muzzle", muzzle);
            SetObject(combat, "startingWeapon", startingWeapon);

            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabRoot}/Player/Player_Aila.prefab");
            Object.DestroyImmediate(go);
        }

        private static EnemyDefinition[] CreateEnemyDefinitions(EnemyProjectile projectile)
        {
            return new[]
            {
                CreateEnemy("RifleSoldier", "rifle_soldier", "Rifle Soldier", 3, 2.5f, 9f, 6f, 1.4f, EnemyAttackPattern.RifleBurst, projectile, 1, 9f),
                CreateEnemy("Grenadier", "grenadier", "Grenadier", 3, 2f, 9f, 7f, 1.8f, EnemyAttackPattern.GrenadeArc, projectile, 2, 7f),
                CreateEnemy("ShieldSoldier", "shield_soldier", "Shield Soldier", 6, 1.6f, 8f, 2f, 1.2f, EnemyAttackPattern.ShieldAdvance, null, 1, 0f),
                CreateEnemy("Sniper", "sniper", "Sniper", 2, 1.2f, 12f, 10f, 2.2f, EnemyAttackPattern.RifleBurst, projectile, 2, 13f),
                CreateEnemy("Drone", "drone", "Drone", 2, 4.2f, 9f, 5f, 1.1f, EnemyAttackPattern.DroneDive, projectile, 1, 11f),
                CreateEnemy("Flamer", "flamer", "Flamer", 5, 1.9f, 8f, 3f, 1.5f, EnemyAttackPattern.FlamethrowerCone, projectile, 1, 6f),
                CreateEnemy("MortarSoldier", "mortar_soldier", "Mortar Soldier", 4, 1f, 11f, 9f, 2.4f, EnemyAttackPattern.MortarMarker, projectile, 2, 5f),
                CreateEnemy("CrawlerMutant", "crawler_mutant", "Crawler Mutant", 3, 3.6f, 7f, 1.4f, 1f, EnemyAttackPattern.ShieldAdvance, null, 1, 0f)
            };
        }

        private static EnemyDefinition CreateEnemy(
            string assetName,
            string id,
            string displayName,
            int health,
            float speed,
            float detectRange,
            float attackRange,
            float cooldown,
            EnemyAttackPattern pattern,
            EnemyProjectile projectile,
            int projectileDamage,
            float projectileSpeed)
        {
            var enemy = LoadOrCreate<EnemyDefinition>($"{DataRoot}/Enemies/{assetName}.asset");
            enemy.id = id;
            enemy.displayName = displayName;
            enemy.maxHealth = health;
            enemy.moveSpeed = speed;
            enemy.detectRange = detectRange;
            enemy.attackRange = attackRange;
            enemy.attackCooldown = cooldown;
            enemy.attackPattern = pattern;
            enemy.projectilePrefab = projectile;
            enemy.projectileDamage = projectileDamage;
            enemy.projectileSpeed = projectileSpeed;
            EditorUtility.SetDirty(enemy);
            return enemy;
        }

        private static void CreateEnemyPrefabs(Sprite sprite, EnemyDefinition[] definitions)
        {
            foreach (var definition in definitions)
            {
                var go = new GameObject(definition.displayName.Replace(" ", "_"));
                AddVisualQuad(go, definition.attackPattern == EnemyAttackPattern.DroneDive ? "Mat_Drone" : "Mat_Enemy");
                go.transform.localScale = definition.attackPattern == EnemyAttackPattern.DroneDive
                    ? new Vector3(0.8f, 0.45f, 1f)
                    : new Vector3(0.8f, 1.35f, 1f);
                var body = go.AddComponent<Rigidbody2D>();
                body.freezeRotation = true;
                body.gravityScale = definition.attackPattern == EnemyAttackPattern.DroneDive ? 0f : 1f;
                go.AddComponent<BoxCollider2D>();
                go.AddComponent<Health>();
                var controller = go.AddComponent<EnemyController>();
                var attackOrigin = new GameObject("AttackOrigin").transform;
                attackOrigin.SetParent(go.transform);
                attackOrigin.localPosition = new Vector3(0.45f, 0.2f, 0f);
                SetObject(controller, "definition", definition);
                SetObject(controller, "attackOrigin", attackOrigin);
                PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabRoot}/Enemies/{definition.displayName.Replace(" ", "_")}.prefab");
                Object.DestroyImmediate(go);
            }
        }

        private static void CreateMiniBossPrefab(Sprite sprite, EnemyProjectile projectile)
        {
            var go = new GameObject("MiniBoss_Walker");
            AddVisualQuad(go, "Mat_Boss");
            go.transform.localScale = new Vector3(3.2f, 2.4f, 1f);
            var body = go.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<Health>();
            var boss = go.AddComponent<MiniBossWalker>();
            var attackOrigin = new GameObject("AttackOrigin").transform;
            attackOrigin.SetParent(go.transform);
            attackOrigin.localPosition = new Vector3(-0.65f, 0.2f, 0f);
            SetObject(boss, "attackOrigin", attackOrigin);
            SetObject(boss, "projectilePrefab", projectile);
            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabRoot}/Enemies/MiniBoss_Walker.prefab");
            Object.DestroyImmediate(go);
        }

        private static void CreateWaveAssets()
        {
            var rifle = LoadPrefabComponent<EnemyController>($"{PrefabRoot}/Enemies/Rifle_Soldier.prefab");
            var grenadier = LoadPrefabComponent<EnemyController>($"{PrefabRoot}/Enemies/Grenadier.prefab");
            var shield = LoadPrefabComponent<EnemyController>($"{PrefabRoot}/Enemies/Shield_Soldier.prefab");
            var drone = LoadPrefabComponent<EnemyController>($"{PrefabRoot}/Enemies/Drone.prefab");
            var mortar = LoadPrefabComponent<EnemyController>($"{PrefabRoot}/Enemies/Mortar_Soldier.prefab");
            var crawler = LoadPrefabComponent<EnemyController>($"{PrefabRoot}/Enemies/Crawler_Mutant.prefab");

            CreateWave("BeachWave", new[] { rifle, rifle, grenadier }, new[] { new Vector2(0f, 1f), new Vector2(4f, 1f), new Vector2(8f, 1f) });
            CreateWave("VillageWave", new[] { rifle, shield, grenadier }, new[] { new Vector2(0f, 1f), new Vector2(5f, 1f), new Vector2(8f, 3.5f) });
            CreateWave("TrenchWave", new[] { mortar, drone, crawler, crawler }, new[] { new Vector2(0f, 1f), new Vector2(3f, 4f), new Vector2(6f, 1f), new Vector2(10f, 1f) });
        }

        private static void CreateWave(string assetName, EnemyController[] enemies, Vector2[] offsets)
        {
            var wave = LoadOrCreate<WaveDefinition>($"{DataRoot}/Levels/Level01/Waves/{assetName}.asset");
            wave.enemyPrefabs = enemies;
            wave.spawnOffsets = offsets;
            EditorUtility.SetDirty(wave);
        }

        private static void CreateCamera(Transform player)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            cameraObject.transform.position = new Vector3(4f, 3f, -10f);
            var follow = cameraObject.AddComponent<CameraFollow2D>();
            SetObject(follow, "target", player);
            cameraObject.AddComponent<DebugHotkeys>();
        }

        private static void CreateGround(Sprite sprite)
        {
            CreateBlock(sprite, "BeachGround", new Vector2(22f, -0.5f), new Vector2(50f, 1f), "Mat_Beach");
            CreateBlock(sprite, "VillageGround", new Vector2(70f, -0.5f), new Vector2(55f, 1f), "Mat_Village");
            CreateBlock(sprite, "VillageRoofA", new Vector2(65f, 3f), new Vector2(10f, 0.5f), "Mat_Roof");
            CreateBlock(sprite, "VillageRoofB", new Vector2(82f, 4.2f), new Vector2(12f, 0.5f), "Mat_Roof");
            CreateBlock(sprite, "TrenchGround", new Vector2(123f, -0.5f), new Vector2(60f, 1f), "Mat_Trench");
            CreateBlock(sprite, "MiniBossArenaGround", new Vector2(167f, -0.5f), new Vector2(38f, 1f), "Mat_Arena");
        }

        private static void CreateBlock(Sprite sprite, string name, Vector2 position, Vector2 size, string materialName)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            AddVisualQuad(go, materialName);
            go.AddComponent<BoxCollider2D>();
        }

        private static void CreateHud()
        {
            var canvas = new GameObject("HUD Canvas");
            canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            var presenter = canvas.AddComponent<SteelRain.UI.HudPresenter>();

            var healthObject = CreateText("HealthLabel", canvas.transform, new Vector2(24f, -24f), "6/6");
            var ammoObject = CreateText("AmmoLabel", canvas.transform, new Vector2(-24f, -24f), "Assault Rifle 90 [Auto]");
            var helpObject = CreateText("HelpLabel", canvas.transform, new Vector2(24f, 24f), "Move: A/D or Arrows  Jump: Space  Fire: Ctrl/Mouse  Switch: E  Quit: Esc/Q");
            SetAnchor(healthObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            SetAnchor(ammoObject.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            SetAnchor(helpObject.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));

            var healthWidget = healthObject.AddComponent<SteelRain.UI.HealthWidget>();
            var ammoWidget = ammoObject.AddComponent<SteelRain.UI.AmmoWidget>();
            SetObject(healthWidget, "label", healthObject.GetComponent<UnityEngine.UI.Text>());
            SetObject(ammoWidget, "label", ammoObject.GetComponent<UnityEngine.UI.Text>());
            SetObject(presenter, "healthWidget", healthWidget);
            SetObject(presenter, "ammoWidget", ammoWidget);
        }

        private static GameObject CreateText(string name, Transform parent, Vector2 anchoredPosition, string text)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(420f, 48f);
            rect.anchoredPosition = anchoredPosition;
            var label = go.AddComponent<UnityEngine.UI.Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 28;
            label.color = Color.white;
            return go;
        }

        private static void SetAnchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
        }

        private static void CreateCheckpointManager(Transform player)
        {
            var managerObject = new GameObject("CheckpointManager");
            var manager = managerObject.AddComponent<CheckpointManager>();
            SetObject(manager, "player", player);
            SetVector3(manager, "fallbackSpawn", new Vector3(0f, 1.5f, 0f));
        }

        private static void CreateSegmentTrigger(string name, float x, string waveAsset, Transform player)
        {
            var go = new GameObject(name);
            go.transform.position = new Vector3(x, 1.5f, 0f);
            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1f, 5f);
            var trigger = go.AddComponent<LevelSegmentTrigger>();
            var wave = AssetDatabase.LoadAssetAtPath<WaveDefinition>($"{DataRoot}/Levels/Level01/Waves/{waveAsset}");
            SetObject(trigger, "wave", wave);
            SetObject(trigger, "player", player);
        }

        private static void CreateCheckpoint(string name, float x)
        {
            var go = new GameObject(name);
            go.transform.position = new Vector3(x, 1.5f, 0f);
            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1f, 5f);
            var checkpoint = go.AddComponent<Checkpoint>();
            var manager = Object.FindFirstObjectByType<CheckpointManager>();
            SetObject(checkpoint, "manager", manager);
        }

        private static void CreateRescueNpc(Sprite sprite, float x)
        {
            var go = new GameObject("RescueNpc_Village");
            go.transform.position = new Vector3(x, 1.5f, 0f);
            AddVisualQuad(go, "Mat_Rescue");
            go.transform.localScale = new Vector3(0.7f, 1.2f, 1f);
            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            go.AddComponent<RescueNpc>();
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static T SavePrefab<T>(GameObject go, string path) where T : Component
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            return prefab.GetComponent<T>();
        }

        private static T LoadPrefabComponent<T>(string path) where T : Component
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return prefab.GetComponent<T>();
        }

        private static void AddVisualQuad(GameObject parent, string materialName)
        {
            var visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
            visual.name = "Visual";
            visual.transform.SetParent(parent.transform, false);

            var collider = visual.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);

            var renderer = visual.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>($"Assets/Game/Generated/{materialName}.mat");
        }

        private static void SetObject(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(Object target, string propertyName, int value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetVector3(Object target, string propertyName, Vector3 value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).vector3Value = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
