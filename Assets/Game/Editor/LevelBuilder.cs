using System.Collections.Generic;
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
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SteelRain.Editor
{
    /// <summary>
    /// 关卡自动搭建工具：一键生成可运行的关卡场景。
    /// 包含玩家、摄像机、敌人、地形、检查点、UI、叙事系统等完整结构。
    /// 菜单：Steel Rain/Build Levels/...
    /// </summary>
    public static class LevelBuilder
    {
        private const string LevelDir = "Assets/Game/Scenes";

        // ─── 预制体目录 ───
        private const string PrefabDir = "Assets/Game/Prefabs";
        private const string EnemyPrefabDir = PrefabDir + "/Enemies";
        private const string PickupPrefabDir = PrefabDir + "/Pickups";

        [MenuItem("Steel Rain/Build Levels/Build All Levels")]
        public static void BuildAllLevels()
        {
            Directory.CreateDirectory(LevelDir);
            BuildMainMenu();
            BuildLevel01_VerticalSlice();
            BuildLevel02_Factory();
            BuildLevel03_Warzone();
            BuildLevel04_Bunker();
            BuildLevel05_Citadel();
            BuildEndlessMode();
            Debug.Log("[LevelBuilder] All 5 levels + Endless + MainMenu built!");
        }

        [MenuItem("Steel Rain/Build Levels/Build Main Menu")]
        public static void BuildMainMenuOnly() => BuildMainMenu();

        [MenuItem("Steel Rain/Build Levels/Build Level 01")]
        public static void BuildLevel01Only() => BuildLevel01_VerticalSlice();

        [MenuItem("Steel Rain/Build Levels/Build Level 02")]
        public static void BuildLevel02Only() => BuildLevel02_Factory();

        [MenuItem("Steel Rain/Build Levels/Build Level 03")]
        public static void BuildLevel03Only() => BuildLevel03_Warzone();

        [MenuItem("Steel Rain/Build Levels/Build Level 04")]
        public static void BuildLevel04Only() => BuildLevel04_Bunker();

        [MenuItem("Steel Rain/Build Levels/Build Level 05")]
        public static void BuildLevel05Only() => BuildLevel05_Citadel();

        [MenuItem("Steel Rain/Build Levels/Build Endless Mode")]
        public static void BuildEndlessOnly() => BuildEndlessMode();

        // ═══════════════════════════════════════════
        //  主菜单场景
        // ═══════════════════════════════════════════
        private static void BuildMainMenu()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 背景色
            Camera.main.backgroundColor = new Color(0.05f, 0.05f, 0.1f);

            // Canvas
            var canvas = CreateCanvas("MainMenuCanvas");
            var canvasGo = canvas.gameObject;

            // 主菜单管理器
            var menuMgr = canvasGo.AddComponent<MainMenu>();
            // 暂存按钮引用（序列化后由 MainMenu 使用）
            var menuContent = CreatePanel(canvasGo.transform, "MenuContent", new Vector2(0, 50), new Vector2(400, 500));
            var titleText = CreateText(menuContent.transform, "TitleText", "STEEL RAIN: FRONTIER", 36, TextAnchor.MiddleCenter, new Vector2(0, 180), new Vector2(380, 60));

            // 按钮
            var startBtn = CreateButton(menuContent.transform, "StartButton", "Start Game", new Vector2(0, 120), new Vector2(280, 50));
            var continueBtn = CreateButton(menuContent.transform, "ContinueButton", "Continue", new Vector2(0, 60), new Vector2(280, 50));
            var newGameBtn = CreateButton(menuContent.transform, "NewGameButton", "New Game", new Vector2(0, 0), new Vector2(280, 50));
            var charSelectBtn = CreateButton(menuContent.transform, "CharacterSelectButton", "Character Select", new Vector2(0, -60), new Vector2(280, 50));
            var endlessBtn = CreateButton(menuContent.transform, "EndlessButton", "Endless Mode", new Vector2(0, -120), new Vector2(280, 50));
            var settingsBtn = CreateButton(menuContent.transform, "SettingsButton", "Settings", new Vector2(0, -180), new Vector2(280, 50));
            var achievementsBtn = CreateButton(menuContent.transform, "AchievementsButton", "Achievements", new Vector2(0, -240), new Vector2(280, 50));
            var quitBtn = CreateButton(menuContent.transform, "QuitButton", "Quit", new Vector2(0, -300), new Vector2(280, 50));

            // 设置面板
            var settingsPanel = CreateSettingsPanel(canvasGo.transform);

            // 角色选择界面
            var charSelectScreen = CreateCharacterSelectScreen(canvasGo.transform);

            // 设置 MainMenu 序列化引用
            SetField(menuMgr, "startButton", startBtn);
            SetField(menuMgr, "continueButton", continueBtn);
            SetField(menuMgr, "newGameButton", newGameBtn);
            SetField(menuMgr, "characterSelectButton", charSelectBtn);
            SetField(menuMgr, "endlessButton", endlessBtn);
            SetField(menuMgr, "settingsButton", settingsBtn);
            SetField(menuMgr, "achievementsButton", achievementsBtn);
            SetField(menuMgr, "quitButton", quitBtn);
            SetField(menuMgr, "settingsPanel", settingsPanel);
            SetField(menuMgr, "menuContent", menuContent);

            // GameBootstrap
            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<GameBootstrap>();

            // MusicPlayer
            var musicGo = new GameObject("MusicPlayer");
            musicGo.AddComponent<MusicPlayer>();

            SaveScene(scene, "MainMenu");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  通用场景创建辅助（避免每个关卡重复初始化代码）
        // ═══════════════════════════════════════════════════════════════════
        private static (Scene scene, Camera cam, GameObject player) InitLevelScene(string bgHex, float orthoSize = 6f)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var cam = Camera.main;
            // 解析颜色
            ColorUtility.TryParseHtmlString(bgHex, out var bgColor);
            cam.backgroundColor = bgColor;
            cam.orthographic = true;
            cam.orthographicSize = orthoSize;
            cam.gameObject.AddComponent<SimpleCameraFollow>();
            cam.gameObject.AddComponent<CameraShake>();
            cam.gameObject.AddComponent<CameraBounds>();

            var managers = new GameObject("--- MANAGERS ---");
            managers.AddComponent<GameBootstrap>();
            managers.AddComponent<GameLoop>();
            var musicGo = new GameObject("MusicPlayer");
            musicGo.AddComponent<MusicPlayer>();

            var player = CreatePlayer(new Vector3(-8, 2, 0));
            return (scene, cam, player);
        }

        private static void FinalizeLevel(Scene scene, string sceneName, float levelEndX, float levelMinX = -12f, float levelMinY = -4f, float levelMaxY = 12f)
        {
            var vfx = new GameObject("--- VFX ---");
            vfx.AddComponent<ParallaxBackground>();
            CreateStoryManager();
            CreateTutorialSystem();
            CreateGameUI();

            var levelEnd = new GameObject("LevelEndTrigger");
            levelEnd.transform.position = new Vector3(levelEndX, 0, 0);
            var box = levelEnd.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(2, 5);
            levelEnd.AddComponent<LevelEndTrigger>();

            // 设置摄像机关卡边界
            var camera = Camera.main;
            if (camera != null)
            {
                var bounds = camera.GetComponent<CameraBounds>();
                if (bounds == null) bounds = camera.gameObject.AddComponent<CameraBounds>();
                bounds.SetBounds(levelMinX, levelEndX + 2f, levelMinY, levelMaxY);
            }

            SaveScene(scene, sceneName);
        }

        private static void CreateEnemySpawnerZone(Transform parent, string name, Vector3 position,
            int[] enemyIndices, int waveCount, float spawnRadius, GameObject[] enemyPrefabsRef)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;

            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(2, 6);

            go.AddComponent<EnemySpawner>();
        }

        private static void CreateFlyingDrone(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.layer = 8;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.8f, 0.8f, 0.9f), 28, 20);
            sr.sortingOrder = 6;

            go.AddComponent<Rigidbody2D>();
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<Health>();
            go.AddComponent<FlyingDroneEnemy>();
            go.AddComponent<LootDrop>();
        }

        private static void CreateSniperEnemy(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.layer = 8;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.3f, 0.5f, 0.3f), 32, 48);
            sr.sortingOrder = 5;

            go.AddComponent<Rigidbody2D>();
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<Health>();
            go.AddComponent<SniperEnemy>();
            go.AddComponent<LootDrop>();

            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0.6f, 0.1f, 0);
        }

        private static void CreateLaserBeam(Transform parent, string name, Vector3 position, float length)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(1f, 0.1f, 0.1f, 0.5f), 64, 8);
            sr.sortingOrder = 4;
            go.transform.localScale = new Vector3(length, 0.5f, 1);

            go.AddComponent<LaserBeam>();
        }

        private static void CreateFallingRock(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.5f, 0.4f, 0.3f), 32, 32);
            sr.sortingOrder = 2;

            go.AddComponent<Rigidbody2D>();
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<FallingRock>();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  关卡 01：海滩前线  (全长 ~220 单位, 35+ 敌人, 5 战斗区)
        // ═══════════════════════════════════════════════════════════════════
        private static void BuildLevel01_VerticalSlice()
        {
            var (_, _, player) = InitLevelScene("#263340");
            var scene = EditorSceneManager.GetActiveScene();

            var terrain = new GameObject("--- TERRAIN ---");

            // ── 区域 1: 起始海滩 (x: -10 ~ 40) ──
            CreateGround(terrain.transform, "Ground_Start", new Vector3(15, -2, 0), new Vector2(50, 1));
            CreateGround(terrain.transform, "Wall_Left", new Vector3(-10, 3, 0), new Vector2(1, 12));
            // 教学平台
            CreateGround(terrain.transform, "Tutorial_Platform", new Vector3(8, 0, 0), new Vector2(5, 0.4f));
            CreateGround(terrain.transform, "Tutorial_High", new Vector3(15, 2, 0), new Vector2(4, 0.4f));
            // 第一次跳跃间隔
            CreateGround(terrain.transform, "Gap_Island1", new Vector3(25, -1, 0), new Vector2(4, 0.4f));
            CreateGround(terrain.transform, "Gap_Island2", new Vector3(32, -0.5f, 0), new Vector2(4, 0.4f));

            // ── 区域 2: 悬崖通道 (x: 40 ~ 80) ──
            CreateGround(terrain.transform, "Cliff_Ground", new Vector3(60, -2, 0), new Vector2(40, 1));
            // 多层平台用于垂直战斗
            CreateGround(terrain.transform, "Cliff_P1", new Vector3(48, 1, 0), new Vector2(5, 0.4f));
            CreateGround(terrain.transform, "Cliff_P2", new Vector3(56, 2.5f, 0), new Vector2(4, 0.4f));
            CreateGround(terrain.transform, "Cliff_P3", new Vector3(65, 4, 0), new Vector2(5, 0.4f));
            CreateGround(terrain.transform, "Cliff_P4", new Vector3(72, 1.5f, 0), new Vector2(4, 0.4f));
            // 狙击手高台
            CreateGround(terrain.transform, "Sniper_Tower", new Vector3(58, 5, 0), new Vector2(3, 0.4f));
            CreateGround(terrain.transform, "Sniper_Wall", new Vector3(56.5f, 3, 0), new Vector2(0.5f, 4));

            // 移动平台
            var mp1 = CreateGround(terrain.transform, "MovPlat_1", new Vector3(42, 0, 0), new Vector2(3, 0.4f));
            var mc1 = mp1.AddComponent<MovingPlatform>();
            SetField(mc1, "pointA", new Vector2(0, -1));
            SetField(mc1, "pointB", new Vector2(0, 3));
            SetField(mc1, "speed", 1.2f);

            // ── 区域 3: 峡谷伏击 (x: 80 ~ 130) ──
            CreateGround(terrain.transform, "Canyon_Floor", new Vector3(105, -2, 0), new Vector2(50, 1));
            // 峡谷墙壁形成狭窄通道
            CreateGround(terrain.transform, "Canyon_Wall_L", new Vector3(85, 3, 0), new Vector2(1, 12));
            CreateGround(terrain.transform, "Canyon_Ceiling", new Vector3(100, 7, 0), new Vector2(30, 0.5f));
            CreateGround(terrain.transform, "Canyon_Wall_R", new Vector3(115, 3, 0), new Vector2(1, 12));
            // 峡谷内跳台
            CreateGround(terrain.transform, "Canyon_P1", new Vector3(92, 0.5f, 0), new Vector2(4, 0.4f));
            CreateGround(terrain.transform, "Canyon_P2", new Vector3(100, 2, 0), new Vector2(3, 0.4f));
            CreateGround(terrain.transform, "Canyon_P3", new Vector3(108, 3.5f, 0), new Vector2(4, 0.4f));

            // 断裂平台
            var crumble1 = CreateGround(terrain.transform, "Crumble_1", new Vector3(96, 1, 0), new Vector2(3, 0.4f));
            crumble1.AddComponent<CrumblingPlatform>();

            // ── 区域 4: 开阔战场 (x: 130 ~ 180) ──
            CreateGround(terrain.transform, "Battlefield_Ground", new Vector3(155, -2, 0), new Vector2(50, 1));
            // 战壕式掩体
            CreateGround(terrain.transform, "Trench_1", new Vector3(138, -0.5f, 0), new Vector2(3, 1.5f));
            CreateGround(terrain.transform, "Trench_2", new Vector3(150, -0.5f, 0), new Vector2(3, 1.5f));
            CreateGround(terrain.transform, "Trench_3", new Vector3(162, -0.5f, 0), new Vector2(3, 1.5f));
            CreateGround(terrain.transform, "Trench_4", new Vector3(174, -0.5f, 0), new Vector2(3, 1.5f));
            // 高架桥
            CreateGround(terrain.transform, "Bridge_Floor", new Vector3(155, 4, 0), new Vector2(30, 0.4f));
            CreateGround(terrain.transform, "Bridge_Support_L", new Vector3(140, 1, 0), new Vector2(0.5f, 6));
            CreateGround(terrain.transform, "Bridge_Support_R", new Vector3(170, 1, 0), new Vector2(0.5f, 6));
            // 垂直移动平台
            var mp2 = CreateGround(terrain.transform, "MovPlat_2", new Vector3(145, 0, 0), new Vector2(3, 0.4f));
            var mc2 = mp2.AddComponent<MovingPlatform>();
            SetField(mc2, "pointA", new Vector2(0, 0));
            SetField(mc2, "pointB", new Vector2(0, 4));
            SetField(mc2, "speed", 0.8f);

            // ── 区域 5: Boss 堡垒 (x: 180 ~ 220) ──
            CreateGround(terrain.transform, "Boss_Ground", new Vector3(200, -2, 0), new Vector2(40, 1));
            CreateGround(terrain.transform, "Boss_Wall_R", new Vector3(220, 3, 0), new Vector2(1, 12));
            // Boss 区域掩体
            CreateGround(terrain.transform, "Boss_Cover_1", new Vector3(190, 0, 0), new Vector2(2, 2));
            CreateGround(terrain.transform, "Boss_Cover_2", new Vector3(210, 0, 0), new Vector2(2, 2));
            // Boss 区高台（用于躲避地面攻击）
            CreateGround(terrain.transform, "Boss_HighP", new Vector3(200, 3, 0), new Vector2(6, 0.4f));

            // ── 危险地形 ──
            var hazards = new GameObject("--- HAZARDS ---");
            CreateSpikeHazard(hazards.transform, "Spikes_A", new Vector3(36, -1.3f, 0), new Vector2(3, 0.5f));
            CreateSpikeHazard(hazards.transform, "Spikes_B", new Vector3(76, -1.3f, 0), new Vector2(4, 0.5f));
            CreateSpikeHazard(hazards.transform, "Spikes_C", new Vector3(130, -1.3f, 0), new Vector2(5, 0.5f));
            CreateSpikeHazard(hazards.transform, "Spikes_D", new Vector3(185, -1.3f, 0), new Vector2(3, 0.5f));
            // 爆炸桶
            CreateExplosiveBarrel(hazards.transform, "Barrel_A", new Vector3(50, -1.2f, 0));
            CreateExplosiveBarrel(hazards.transform, "Barrel_B", new Vector3(68, -1.2f, 0));
            CreateExplosiveBarrel(hazards.transform, "Barrel_C", new Vector3(95, -1.2f, 0));
            CreateExplosiveBarrel(hazards.transform, "Barrel_D", new Vector3(120, -1.2f, 0));
            CreateExplosiveBarrel(hazards.transform, "Barrel_E", new Vector3(155, 4.5f, 0));
            CreateExplosiveBarrel(hazards.transform, "Barrel_F", new Vector3(170, -1.2f, 0));
            // 落石
            CreateFallingRock(hazards.transform, "Rock_A", new Vector3(55, 6, 0));
            CreateFallingRock(hazards.transform, "Rock_B", new Vector3(100, 8, 0));

            // ── 敌人 (静态巡逻 + 波次生成混合) ──
            var enemies = new GameObject("--- ENEMIES ---");

            // 区域1: 教学区 - 少量步兵
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_T1", new Vector3(5, 0.8f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_T2", new Vector3(18, 0.8f, 0));

            // 区域2: 悬崖 - 混合敌人
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_C1", new Vector3(45, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Shield_C1", new Vector3(52, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_C2", new Vector3(60, 3.3f, 0));
            CreateSniperEnemy(enemies.transform, "E_Sniper_Tower", new Vector3(58, 5.8f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Grenadier_C1", new Vector3(70, -1.2f, 0));
            CreateFlyingDrone(enemies.transform, "E_FDrone_C1", new Vector3(65, 5f, 0));

            // 区域3: 峡谷伏击 - 密集步兵+精英
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_G1", new Vector3(88, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_G2", new Vector3(93, 1.3f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Shield_G1", new Vector3(98, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_G3", new Vector3(103, 2.8f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Grenadier_G1", new Vector3(110, -1.2f, 0));
            CreateFlyingDrone(enemies.transform, "E_FDrone_G1", new Vector3(105, 5f, 0));
            CreateSniperEnemy(enemies.transform, "E_Sniper_G1", new Vector3(112, 4.3f, 0));

            // 区域4: 开阔战场 - 大规模战斗
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_B1", new Vector3(135, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_B2", new Vector3(140, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Shield_B1", new Vector3(148, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_B3", new Vector3(155, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Grenadier_B1", new Vector3(160, 4.8f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_B4", new Vector3(165, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Drone_B1", new Vector3(150, 5f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Drone_B2", new Vector3(170, 5f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Sniper_B1", new Vector3(175, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Heavy_B1", new Vector3(158, -1.2f, 0));

            // 区域5: Boss 堡垒
            CreateBossPlaceholder(enemies.transform, "TurretBoss", new Vector3(205, 0, 0));
            // Boss 伴随小兵
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_Boss1", new Vector3(195, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_Boss2", new Vector3(215, -1.2f, 0));

            // ── 检查点 ──
            var cps = new GameObject("--- CHECKPOINTS ---");
            var cpMgr = CreateCheckpointManager(cps.transform);
            CreateCheckpoint(cps.transform, "CP_A", new Vector3(20, 0, 0), cpMgr);
            CreateCheckpoint(cps.transform, "CP_B", new Vector3(60, 0, 0), cpMgr);
            CreateCheckpoint(cps.transform, "CP_C", new Vector3(105, 0, 0), cpMgr);
            CreateCheckpoint(cps.transform, "CP_D", new Vector3(155, 0, 0), cpMgr);

            // ── 拾取物 (分布在各区域) ──
            var pickups = new GameObject("--- PICKUPS ---");
            CreatePickupPlaceholder(pickups.transform, "HP_A", new Vector3(12, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "WU_A", new Vector3(22, 1, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "HP_B", new Vector3(50, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "Shield_B", new Vector3(58, 6, 0), "Shield");
            CreatePickupPlaceholder(pickups.transform, "WU_B", new Vector3(72, 2, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "HP_C", new Vector3(95, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "Inv_C", new Vector3(108, 4, 0), "Invincible");
            CreatePickupPlaceholder(pickups.transform, "WU_C", new Vector3(118, 1, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "HP_D1", new Vector3(142, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "HP_D2", new Vector3(165, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "Shield_D", new Vector3(155, 5, 0), "Shield");
            CreatePickupPlaceholder(pickups.transform, "WU_D", new Vector3(175, 1, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "HP_Boss", new Vector3(190, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "Shield_Boss", new Vector3(200, 4, 0), "Shield");

            FinalizeLevel(scene, "Level01_VerticalSlice", 218);
            ConfigureStoryManagerLevel01();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  关卡 02：工厂  (全长 ~230 单位, 40+ 敌人, 5 战斗区)
        // ═══════════════════════════════════════════════════════════════════
        private static void BuildLevel02_Factory()
        {
            var (_, _, player) = InitLevelScene("#1A1F23");
            var scene = EditorSceneManager.GetActiveScene();

            var terrain = new GameObject("--- TERRAIN ---");

            // ── 区域 1: 入口仓库 (x: -10 ~ 45) ──
            CreateGround(terrain.transform, "Ground_Start", new Vector3(15, -2, 0), new Vector2(50, 1));
            CreateGround(terrain.transform, "Wall_Left", new Vector3(-10, 3, 0), new Vector2(1, 12));
            CreateGround(terrain.transform, "Rack_1", new Vector3(8, -0.5f, 0), new Vector2(3, 1.5f));
            CreateGround(terrain.transform, "Rack_2", new Vector3(18, -0.5f, 0), new Vector2(3, 1.5f));
            CreateGround(terrain.transform, "HighWalk_1", new Vector3(28, 4, 0), new Vector2(14, 0.4f));
            CreateGround(terrain.transform, "HighWalk_Wall", new Vector3(21, 2, 0), new Vector2(0.5f, 4));
            CreateGround(terrain.transform, "Gap_Island", new Vector3(38, -0.5f, 0), new Vector2(4, 0.4f));

            // ── 区域 2: 组装线 (x: 45 ~ 100) ──
            CreateGround(terrain.transform, "Assembly_Ground", new Vector3(72, -2, 0), new Vector2(55, 1));
            for (int i = 0; i < 6; i++)
                CreateGround(terrain.transform, $"Conveyor_{i}", new Vector3(50 + i * 8, i % 2 == 0 ? -0.5f : 1.5f, 0), new Vector2(5, 0.4f));
            CreateGround(terrain.transform, "Assembly_Tower", new Vector3(85, 5, 0), new Vector2(4, 0.4f));
            CreateGround(terrain.transform, "Assembly_TowerWall", new Vector3(83, 3, 0), new Vector2(0.5f, 4));
            var mp1 = CreateGround(terrain.transform, "ForkLiftPlat", new Vector3(60, 0, 0), new Vector2(3, 0.4f));
            var mc1 = mp1.AddComponent<MovingPlatform>();
            SetField(mc1, "pointA", new Vector2(0, 0));
            SetField(mc1, "pointB", new Vector2(0, 5));
            SetField(mc1, "speed", 1f);

            // ── 区域 3: 熔炉房 (x: 100 ~ 150) ──
            CreateGround(terrain.transform, "Furnace_Ground", new Vector3(125, -2, 0), new Vector2(50, 1));
            CreateGround(terrain.transform, "Furnace_Wall_L", new Vector3(105, 3, 0), new Vector2(1, 12));
            CreateGround(terrain.transform, "Furnace_Wall_R", new Vector3(145, 3, 0), new Vector2(1, 12));
            CreateGround(terrain.transform, "Furnace_Ledge_1", new Vector3(112, 2, 0), new Vector2(4, 0.4f));
            CreateGround(terrain.transform, "Furnace_Ledge_2", new Vector3(130, 3, 0), new Vector2(4, 0.4f));
            CreateGround(terrain.transform, "Furnace_Ledge_3", new Vector3(140, 1.5f, 0), new Vector2(4, 0.4f));
            var crumble1 = CreateGround(terrain.transform, "Furnace_Crumble", new Vector3(120, 1, 0), new Vector2(3, 0.4f));
            crumble1.AddComponent<CrumblingPlatform>();
            CreateSpikeHazard(terrain.transform, "Lava_Spikes_A", new Vector3(115, -1.3f, 0), new Vector2(6, 0.5f));
            CreateSpikeHazard(terrain.transform, "Lava_Spikes_B", new Vector3(135, -1.3f, 0), new Vector2(6, 0.5f));

            // ── 区域 4: 仓储大厅 (x: 150 ~ 200) ──
            CreateGround(terrain.transform, "Storage_Ground", new Vector3(175, -2, 0), new Vector2(50, 1));
            CreateGround(terrain.transform, "Shelf_1", new Vector3(158, 0, 0), new Vector2(3, 2));
            CreateGround(terrain.transform, "Shelf_2", new Vector3(170, 0, 0), new Vector2(3, 2));
            CreateGround(terrain.transform, "Shelf_3", new Vector3(182, 0, 0), new Vector2(3, 2));
            CreateGround(terrain.transform, "UpperDeck", new Vector3(175, 4.5f, 0), new Vector2(22, 0.4f));
            var mp2 = CreateGround(terrain.transform, "CargoLift", new Vector3(164, 0, 0), new Vector2(3, 0.4f));
            var mc2 = mp2.AddComponent<MovingPlatform>();
            SetField(mc2, "pointA", new Vector2(0, 0));
            SetField(mc2, "pointB", new Vector2(0, 5));
            SetField(mc2, "speed", 0.8f);

            // ── 区域 5: Boss 铸造井 (x: 200 ~ 230) ──
            CreateGround(terrain.transform, "Boss_Ground", new Vector3(215, -2, 0), new Vector2(30, 1));
            CreateGround(terrain.transform, "Boss_Wall_R", new Vector3(230, 3, 0), new Vector2(1, 12));
            CreateGround(terrain.transform, "Boss_Pillar_L", new Vector3(208, 0, 0), new Vector2(1.2f, 2));
            CreateGround(terrain.transform, "Boss_Pillar_R", new Vector3(222, 0, 0), new Vector2(1.2f, 2));
            CreateGround(terrain.transform, "Boss_Ring", new Vector3(215, 3.5f, 0), new Vector2(14, 0.4f));

            // ── 敌人 ──
            var enemies = new GameObject("--- ENEMIES ---");

            // 区域1 入口
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_W1", new Vector3(6, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_W2", new Vector3(15, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Shield_W1", new Vector3(24, -1.2f, 0));
            CreateFlyingDrone(enemies.transform, "E_Drone_W1", new Vector3(28, 5f, 0));

            // 区域2 组装线
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_A1", new Vector3(48, 0.3f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_A2", new Vector3(56, 2.3f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Shield_A1", new Vector3(64, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Grenadier_A1", new Vector3(72, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_A3", new Vector3(80, -1.2f, 0));
            CreateFlyingDrone(enemies.transform, "E_Drone_A1", new Vector3(85, 6f, 0));
            CreateSniperEnemy(enemies.transform, "E_Sniper_A1", new Vector3(85, 5.8f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_A4", new Vector3(90, -1.2f, 0));

            // 区域3 熔炉
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_F1", new Vector3(110, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_F2", new Vector3(118, 1.8f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Shield_F1", new Vector3(125, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Grenadier_F1", new Vector3(130, 3.8f, 0));
            CreateFlyingDrone(enemies.transform, "E_Drone_F1", new Vector3(135, 5f, 0));
            CreateSniperEnemy(enemies.transform, "E_Sniper_F1", new Vector3(140, 2.3f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_F3", new Vector3(142, -1.2f, 0));

            // 区域4 仓储
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_S1", new Vector3(155, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_S2", new Vector3(162, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Shield_S1", new Vector3(168, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_S3", new Vector3(175, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Grenadier_S1", new Vector3(180, 5.3f, 0));
            CreateFlyingDrone(enemies.transform, "E_Drone_S1", new Vector3(185, 5f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_S4", new Vector3(190, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Heavy_S1", new Vector3(195, -1.2f, 0));

            // 区域5 Boss
            CreateBossPlaceholder(enemies.transform, "MiniBossWalker", new Vector3(218, 0, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_B1", new Vector3(210, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_Rifle_B2", new Vector3(225, -1.2f, 0));

            // ── 检查点 ──
            var cps = new GameObject("--- CHECKPOINTS ---");
            var cpMgr = CreateCheckpointManager(cps.transform);
            CreateCheckpoint(cps.transform, "CP_A", new Vector3(20, 0, 0), cpMgr);
            CreateCheckpoint(cps.transform, "CP_B", new Vector3(72, 0, 0), cpMgr);
            CreateCheckpoint(cps.transform, "CP_C", new Vector3(125, 0, 0), cpMgr);
            CreateCheckpoint(cps.transform, "CP_D", new Vector3(175, 0, 0), cpMgr);

            // ── 拾取物 ──
            var pickups = new GameObject("--- PICKUPS ---");
            CreatePickupPlaceholder(pickups.transform, "HP_A1", new Vector3(12, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "WU_A1", new Vector3(28, 5, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "HP_B1", new Vector3(56, 3, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "Shield_B", new Vector3(85, 6, 0), "Shield");
            CreatePickupPlaceholder(pickups.transform, "WU_B", new Vector3(95, 1, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "HP_C1", new Vector3(115, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "Inv_C", new Vector3(140, 3, 0), "Invincible");
            CreatePickupPlaceholder(pickups.transform, "WU_C", new Vector3(142, 1, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "HP_D1", new Vector3(165, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "HP_D2", new Vector3(185, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "Shield_D", new Vector3(175, 5, 0), "Shield");
            CreatePickupPlaceholder(pickups.transform, "HP_Boss", new Vector3(208, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "Shield_Boss", new Vector3(215, 4, 0), "Shield");

            FinalizeLevel(scene, "Level02_Factory", 228);
            ConfigureStoryManagerLevel02();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  关卡 03：战区（全长 ~200, 35 敌人, 4 区）
        // ═══════════════════════════════════════════════════════════════════
        private static void BuildLevel03_Warzone()
        {
            var (_, _, player) = InitLevelScene("#2B2521");
            var scene = EditorSceneManager.GetActiveScene();
            var terrain = new GameObject("--- TERRAIN ---");

            CreateGround(terrain.transform, "Ground_Start", new Vector3(20, -2, 0), new Vector2(60, 1));
            CreateGround(terrain.transform, "Wall_Left", new Vector3(-10, 3, 0), new Vector2(1, 12));
            CreateGround(terrain.transform, "Ruin_1", new Vector3(10, -0.5f, 0), new Vector2(3, 1.5f));
            CreateGround(terrain.transform, "Ruin_2", new Vector3(22, 0, 0), new Vector2(5, 0.4f));
            CreateGround(terrain.transform, "Ruin_3", new Vector3(35, 1, 0), new Vector2(4, 0.4f));
            CreateGround(terrain.transform, "Street_Gap1", new Vector3(50, -0.5f, 0), new Vector2(4, 0.4f));

            CreateGround(terrain.transform, "Plaza_Ground", new Vector3(80, -2, 0), new Vector2(40, 1));
            CreateGround(terrain.transform, "Fountain", new Vector3(80, -0.5f, 0), new Vector2(5, 1.5f));
            CreateGround(terrain.transform, "Balcony_L", new Vector3(70, 3, 0), new Vector2(6, 0.4f));
            CreateGround(terrain.transform, "Balcony_R", new Vector3(90, 3, 0), new Vector2(6, 0.4f));
            var mp1 = CreateGround(terrain.transform, "TruckPlat", new Vector3(86, 0, 0), new Vector2(4, 0.4f));
            var mc1 = mp1.AddComponent<MovingPlatform>();
            SetField(mc1, "pointA", new Vector2(-5, 0));
            SetField(mc1, "pointB", new Vector2(5, 0));
            SetField(mc1, "speed", 1.2f);

            CreateGround(terrain.transform, "Bridge_Ground", new Vector3(120, -2, 0), new Vector2(20, 1));
            CreateGround(terrain.transform, "Bridge_Top", new Vector3(120, 4, 0), new Vector2(18, 0.4f));
            CreateGround(terrain.transform, "Bridge_Support_L", new Vector3(111, 1, 0), new Vector2(0.5f, 6));
            CreateGround(terrain.transform, "Bridge_Support_R", new Vector3(129, 1, 0), new Vector2(0.5f, 6));

            CreateGround(terrain.transform, "Boss_Ground", new Vector3(160, -2, 0), new Vector2(40, 1));
            CreateGround(terrain.transform, "Boss_Wall_R", new Vector3(180, 3, 0), new Vector2(1, 12));
            CreateGround(terrain.transform, "Boss_Cover_1", new Vector3(155, 0, 0), new Vector2(2, 2));
            CreateGround(terrain.transform, "Boss_Cover_2", new Vector3(170, 0, 0), new Vector2(2, 2));
            CreateGround(terrain.transform, "Boss_HighP", new Vector3(160, 3, 0), new Vector2(8, 0.4f));

            var hazards = new GameObject("--- HAZARDS ---");
            CreateSpikeHazard(hazards.transform, "S_A", new Vector3(40, -1.3f, 0), new Vector2(4, 0.5f));
            CreateSpikeHazard(hazards.transform, "S_B", new Vector3(105, -1.3f, 0), new Vector2(4, 0.5f));
            CreateExplosiveBarrel(hazards.transform, "B_A", new Vector3(25, -1.2f, 0));
            CreateExplosiveBarrel(hazards.transform, "B_B", new Vector3(72, -1.2f, 0));
            CreateExplosiveBarrel(hazards.transform, "B_C", new Vector3(115, -1.2f, 0));
            CreateExplosiveBarrel(hazards.transform, "B_D", new Vector3(150, -1.2f, 0));

            var enemies = new GameObject("--- ENEMIES ---");
            CreateEnemyPlaceholder(enemies.transform, "E_R1", new Vector3(8, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_R2", new Vector3(18, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_R3", new Vector3(30, 1.8f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_R4", new Vector3(45, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_R5", new Vector3(55, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_S1", new Vector3(65, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_S2", new Vector3(78, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_G1", new Vector3(88, -1.2f, 0));
            CreateFlyingDrone(enemies.transform, "E_FDrone_1", new Vector3(90, 4f, 0));
            CreateSniperEnemy(enemies.transform, "E_Sniper_1", new Vector3(70, 3.8f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_R6", new Vector3(95, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_R7", new Vector3(100, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_R8", new Vector3(108, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_S3", new Vector3(112, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_R9", new Vector3(118, -1.2f, 0));
            CreateFlyingDrone(enemies.transform, "E_FDrone_2", new Vector3(120, 5f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_R10", new Vector3(125, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_G2", new Vector3(130, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_S4", new Vector3(135, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_H1", new Vector3(140, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_R11", new Vector3(145, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_R12", new Vector3(150, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_R13", new Vector3(155, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_G3", new Vector3(160, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_S5", new Vector3(165, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_R14", new Vector3(170, -1.2f, 0));
            CreateFlyingDrone(enemies.transform, "E_FDrone_3", new Vector3(175, 5f, 0));
            CreateSniperEnemy(enemies.transform, "E_Sniper_2", new Vector3(178, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_B1", new Vector3(148, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E_B2", new Vector3(152, -1.2f, 0));
            CreateBossPlaceholder(enemies.transform, "TurretBoss", new Vector3(164, 0, 0));

            var cps = new GameObject("--- CHECKPOINTS ---");
            var cpMgr = CreateCheckpointManager(cps.transform);
            CreateCheckpoint(cps.transform, "CP1", new Vector3(20, 0, 0), cpMgr);
            CreateCheckpoint(cps.transform, "CP2", new Vector3(80, 0, 0), cpMgr);
            CreateCheckpoint(cps.transform, "CP3", new Vector3(120, 0, 0), cpMgr);

            var pickups = new GameObject("--- PICKUPS ---");
            CreatePickupPlaceholder(pickups.transform, "HP1", new Vector3(15, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "WU1", new Vector3(35, 2, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "HP2", new Vector3(75, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "SH1", new Vector3(90, 4, 0), "Shield");
            CreatePickupPlaceholder(pickups.transform, "WU2", new Vector3(100, 1, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "INV1", new Vector3(120, 5, 0), "Invincible");
            CreatePickupPlaceholder(pickups.transform, "HP3", new Vector3(140, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "WU3", new Vector3(150, 1, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "SH2", new Vector3(160, 4, 0), "Shield");
            CreatePickupPlaceholder(pickups.transform, "HP4", new Vector3(170, 1, 0), "Health");

            FinalizeLevel(scene, "Level03_Warzone", 178);
            ConfigureStoryManagerLevel03();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  关卡 04：地下掩体（全长 ~180, 30 敌人, 4 区）
        // ═══════════════════════════════════════════════════════════════════
        private static void BuildLevel04_Bunker()
        {
            var (_, _, player) = InitLevelScene("#111317");
            var scene = EditorSceneManager.GetActiveScene();
            var terrain = new GameObject("--- TERRAIN ---");

            CreateGround(terrain.transform, "Ground_Start", new Vector3(15, -2, 0), new Vector2(30, 1));
            CreateGround(terrain.transform, "Wall_Left", new Vector3(-5, 3, 0), new Vector2(1, 12));
            CreateGround(terrain.transform, "Entry_P1", new Vector3(10, 0, 0), new Vector2(4, 0.4f));
            CreateGround(terrain.transform, "Entry_P2", new Vector3(20, 2, 0), new Vector2(4, 0.4f));

            CreateGround(terrain.transform, "Hall_Ground", new Vector3(55, -2, 0), new Vector2(40, 1));
            CreateGround(terrain.transform, "Hall_Wall_R", new Vector3(75, 3, 0), new Vector2(1, 12));
            CreateGround(terrain.transform, "Hall_Ledge_1", new Vector3(45, 1, 0), new Vector2(4, 0.4f));
            CreateGround(terrain.transform, "Hall_Ledge_2", new Vector3(60, 2, 0), new Vector2(4, 0.4f));
            CreateGround(terrain.transform, "Hall_Ledge_3", new Vector3(70, 3, 0), new Vector2(4, 0.4f));
            CreateSpikeHazard(terrain.transform, "Hall_Spikes", new Vector3(50, -1.3f, 0), new Vector2(4, 0.5f));
            var crumble1 = CreateGround(terrain.transform, "Hall_Crumble", new Vector3(65, 0, 0), new Vector2(3, 0.4f));
            crumble1.AddComponent<CrumblingPlatform>();

            CreateGround(terrain.transform, "Shaft_Ground", new Vector3(100, -2, 0), new Vector2(20, 1));
            CreateGround(terrain.transform, "Shaft_Top", new Vector3(100, 8, 0), new Vector2(8, 0.4f));
            var mp1 = CreateGround(terrain.transform, "Elevator", new Vector3(100, 0, 0), new Vector2(3, 0.4f));
            var mc1 = mp1.AddComponent<MovingPlatform>();
            SetField(mc1, "pointA", new Vector2(0, 0));
            SetField(mc1, "pointB", new Vector2(0, 6));
            SetField(mc1, "speed", 1.0f);

            CreateGround(terrain.transform, "Boss_Ground", new Vector3(135, -2, 0), new Vector2(30, 1));
            CreateGround(terrain.transform, "Boss_Wall_R", new Vector3(150, 3, 0), new Vector2(1, 12));
            CreateGround(terrain.transform, "Boss_P1", new Vector3(130, 0, 0), new Vector2(2, 2));
            CreateGround(terrain.transform, "Boss_P2", new Vector3(145, 0, 0), new Vector2(2, 2));
            CreateGround(terrain.transform, "Boss_High", new Vector3(135, 3, 0), new Vector2(6, 0.4f));

            // 激光陷阱（地下掩体特色）
            var hazards = new GameObject("--- HAZARDS ---");
            CreateLaserBeam(hazards.transform, "Laser_A", new Vector3(55, 0, 0), 8f);
            CreateLaserBeam(hazards.transform, "Laser_B", new Vector3(100, 2, 0), 6f);
            CreateFallingRock(hazards.transform, "Rock_A", new Vector3(80, 6, 0));

            var enemies = new GameObject("--- ENEMIES ---");
            CreateEnemyPlaceholder(enemies.transform, "E1", new Vector3(8, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E2", new Vector3(18, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E3", new Vector3(24, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E4", new Vector3(35, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E5", new Vector3(42, -1.2f, 0));
            CreateFlyingDrone(enemies.transform, "E_FDrone_1", new Vector3(48, 3f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E7", new Vector3(55, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E8", new Vector3(62, -1.2f, 0));
            CreateSniperEnemy(enemies.transform, "E_Sniper_1", new Vector3(70, 3.8f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E10", new Vector3(73, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E11", new Vector3(80, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E12", new Vector3(88, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E13", new Vector3(95, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E14", new Vector3(105, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E15", new Vector3(112, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E16", new Vector3(120, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E17", new Vector3(128, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E18", new Vector3(132, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E19", new Vector3(138, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E20", new Vector3(142, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E21", new Vector3(146, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E22", new Vector3(148, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E23", new Vector3(140, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E24", new Vector3(135, -1.2f, 0));
            CreateBossPlaceholder(enemies.transform, "MiniBossWalker", new Vector3(140, 0, 0));

            var cps = new GameObject("--- CHECKPOINTS ---");
            var cpMgr = CreateCheckpointManager(cps.transform);
            CreateCheckpoint(cps.transform, "CP1", new Vector3(20, 0, 0), cpMgr);
            CreateCheckpoint(cps.transform, "CP2", new Vector3(70, 0, 0), cpMgr);
            CreateCheckpoint(cps.transform, "CP3", new Vector3(100, 0, 0), cpMgr);

            var pickups = new GameObject("--- PICKUPS ---");
            CreatePickupPlaceholder(pickups.transform, "HP1", new Vector3(15, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "WU1", new Vector3(25, 3, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "HP2", new Vector3(55, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "SH1", new Vector3(70, 4, 0), "Shield");
            CreatePickupPlaceholder(pickups.transform, "INV1", new Vector3(100, 1, 0), "Invincible");
            CreatePickupPlaceholder(pickups.transform, "WU2", new Vector3(110, 1, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "HP3", new Vector3(130, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "SH2", new Vector3(135, 4, 0), "Shield");

            FinalizeLevel(scene, "Level04_Bunker", 148);
            ConfigureStoryManagerLevel04();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  关卡 05：要塞（全长 ~200, 35 敌人, 4 区）
        // ═══════════════════════════════════════════════════════════════════
        private static void BuildLevel05_Citadel()
        {
            var (_, _, player) = InitLevelScene("#171C22");
            var scene = EditorSceneManager.GetActiveScene();
            var terrain = new GameObject("--- TERRAIN ---");

            CreateGround(terrain.transform, "Ground_Start", new Vector3(20, -2, 0), new Vector2(40, 1));
            CreateGround(terrain.transform, "Wall_Left", new Vector3(-5, 3, 0), new Vector2(1, 12));
            CreateGround(terrain.transform, "Gate_P1", new Vector3(12, 0, 0), new Vector2(5, 0.4f));
            CreateGround(terrain.transform, "Gate_P2", new Vector3(24, 1, 0), new Vector2(4, 0.4f));

            CreateGround(terrain.transform, "Wall_Ground", new Vector3(65, -2, 0), new Vector2(50, 1));
            CreateGround(terrain.transform, "Wall_Tower_L", new Vector3(45, 5, 0), new Vector2(4, 0.4f));
            CreateGround(terrain.transform, "Wall_Tower_R", new Vector3(85, 5, 0), new Vector2(4, 0.4f));
            CreateGround(terrain.transform, "Wall_Ramp_L", new Vector3(50, 0, 0), new Vector2(6, 0.4f));
            CreateGround(terrain.transform, "Wall_Ramp_R", new Vector3(80, 0, 0), new Vector2(6, 0.4f));
            CreateGround(terrain.transform, "Wall_Walk", new Vector3(65, 3, 0), new Vector2(20, 0.4f));

            CreateGround(terrain.transform, "Yard_Ground", new Vector3(120, -2, 0), new Vector2(40, 1));
            CreateGround(terrain.transform, "Yard_Cover_1", new Vector3(110, -0.5f, 0), new Vector2(3, 1.5f));
            CreateGround(terrain.transform, "Yard_Cover_2", new Vector3(125, -0.5f, 0), new Vector2(3, 1.5f));
            CreateGround(terrain.transform, "Yard_Cover_3", new Vector3(140, -0.5f, 0), new Vector2(3, 1.5f));
            CreateGround(terrain.transform, "Yard_Bridge", new Vector3(120, 4, 0), new Vector2(16, 0.4f));
            var mp1 = CreateGround(terrain.transform, "Yard_Lift", new Vector3(105, 0, 0), new Vector2(3, 0.4f));
            var mc1 = mp1.AddComponent<MovingPlatform>();
            SetField(mc1, "pointA", new Vector2(0, 0));
            SetField(mc1, "pointB", new Vector2(0, 4));
            SetField(mc1, "speed", 1.0f);

            CreateGround(terrain.transform, "Throne_Ground", new Vector3(165, -2, 0), new Vector2(30, 1));
            CreateGround(terrain.transform, "Throne_Wall_R", new Vector3(180, 3, 0), new Vector2(1, 12));
            CreateGround(terrain.transform, "Throne_Steps_L", new Vector3(158, 0, 0), new Vector2(3, 2));
            CreateGround(terrain.transform, "Throne_Steps_R", new Vector3(172, 0, 0), new Vector2(3, 2));
            CreateGround(terrain.transform, "Throne_High", new Vector3(165, 3, 0), new Vector2(10, 0.4f));

            // 激光陷阱（要塞入口和王座区）
            var hazards = new GameObject("--- HAZARDS ---");
            CreateLaserBeam(hazards.transform, "Laser_A", new Vector3(65, 1, 0), 10f);
            CreateLaserBeam(hazards.transform, "Laser_B", new Vector3(120, 0.5f, 0), 8f);
            CreateFallingRock(hazards.transform, "Rock_A", new Vector3(90, 7, 0));
            CreateFallingRock(hazards.transform, "Rock_B", new Vector3(140, 7, 0));

            var enemies = new GameObject("--- ENEMIES ---");
            // 大门区
            CreateEnemyPlaceholder(enemies.transform, "E1", new Vector3(10, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E2", new Vector3(22, -1.2f, 0));
            CreateSniperEnemy(enemies.transform, "E_Sniper_1", new Vector3(30, 2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E4", new Vector3(40, -1.2f, 0));
            CreateFlyingDrone(enemies.transform, "E_FDrone_1", new Vector3(48, 3f, 0));
            // 城墙区
            CreateEnemyPlaceholder(enemies.transform, "E6", new Vector3(55, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E7", new Vector3(62, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E8", new Vector3(70, -1.2f, 0));
            CreateSniperEnemy(enemies.transform, "E_Sniper_2", new Vector3(78, 3f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E10", new Vector3(85, -1.2f, 0));
            CreateFlyingDrone(enemies.transform, "E_FDrone_2", new Vector3(90, 3f, 0));
            // 院子区
            CreateEnemyPlaceholder(enemies.transform, "E12", new Vector3(100, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E13", new Vector3(108, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E14", new Vector3(115, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E15", new Vector3(120, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E16", new Vector3(125, -1.2f, 0));
            CreateFlyingDrone(enemies.transform, "E_FDrone_3", new Vector3(130, 5f, 0));
            CreateSniperEnemy(enemies.transform, "E_Sniper_3", new Vector3(140, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E19", new Vector3(145, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E20", new Vector3(150, -1.2f, 0));
            // 王座区
            CreateEnemyPlaceholder(enemies.transform, "E21", new Vector3(155, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E22", new Vector3(160, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E23", new Vector3(165, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E24", new Vector3(170, -1.2f, 0));
            CreateEnemyPlaceholder(enemies.transform, "E25", new Vector3(175, -1.2f, 0));
            CreateBossPlaceholder(enemies.transform, "TurretBoss", new Vector3(166, 0, 0));

            var cps = new GameObject("--- CHECKPOINTS ---");
            var cpMgr = CreateCheckpointManager(cps.transform);
            CreateCheckpoint(cps.transform, "CP1", new Vector3(20, 0, 0), cpMgr);
            CreateCheckpoint(cps.transform, "CP2", new Vector3(65, 0, 0), cpMgr);
            CreateCheckpoint(cps.transform, "CP3", new Vector3(120, 0, 0), cpMgr);

            var pickups = new GameObject("--- PICKUPS ---");
            CreatePickupPlaceholder(pickups.transform, "HP1", new Vector3(15, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "WU1", new Vector3(30, 2, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "HP2", new Vector3(55, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "SH1", new Vector3(65, 4, 0), "Shield");
            CreatePickupPlaceholder(pickups.transform, "INV1", new Vector3(100, 1, 0), "Invincible");
            CreatePickupPlaceholder(pickups.transform, "WU2", new Vector3(110, 1, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "HP3", new Vector3(135, 1, 0), "Health");
            CreatePickupPlaceholder(pickups.transform, "SH2", new Vector3(155, 4, 0), "Shield");
            CreatePickupPlaceholder(pickups.transform, "WU3", new Vector3(160, 1, 0), "Weapon");
            CreatePickupPlaceholder(pickups.transform, "HP4", new Vector3(175, 1, 0), "Health");

            FinalizeLevel(scene, "Level05_Citadel", 178);
            ConfigureStoryManagerLevel05();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  无尽模式场景（波次制生存，通关后解锁）
        // ═══════════════════════════════════════════════════════════════════
        private static void BuildEndlessMode()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Camera.main.backgroundColor = new Color(0.08f, 0.04f, 0.04f);
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 5f;
            Camera.main.gameObject.AddComponent<SimpleCameraFollow>();
            Camera.main.gameObject.AddComponent<CameraShake>();

            // 管理器
            var managers = new GameObject("--- MANAGERS ---");
            managers.AddComponent<GameBootstrap>();
            managers.AddComponent<GameLoop>();
            var musicGo = new GameObject("MusicPlayer");
            musicGo.AddComponent<MusicPlayer>();

            // 地面
            var groundParent = new GameObject("--- Environment ---");
            CreateGround(groundParent.transform, "EndlessGround", new Vector3(0, -2f, 0), new Vector2(200f, 2f));

            // 玩家
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Player_Aila.prefab");
            GameObject player;
            if (playerPrefab != null)
                player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            else
                player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0, 1f, 0);

            // 无尽模式管理器
            var endlessGo = new GameObject("--- EndlessMode ---");
            var endless = endlessGo.AddComponent<EndlessMode>();

            // 设置 EndlessMode 字段
            var so = new SerializedObject(endless);
            so.FindProperty("enemySpawnRadius").floatValue = 12f;
            so.FindProperty("enemySpawnMinY").floatValue = -1f;
            so.FindProperty("enemySpawnMaxY").floatValue = 4f;
            so.FindProperty("waveInterval").floatValue = 5f;

            // 加载敌人预制体（注意：项目未单独制作 Shotgun，复用 Shield 作为近战型）
            var enemyPrefabsList = new List<GameObject>();
            var riflePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Rifle.prefab");
            var shieldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Shield.prefab");
            var grenadierPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Grenadier.prefab");
            var dronePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Drone.prefab");
            var sniperPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Sniper.prefab");
            var heavyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_HeavyGunner.prefab");
            if (riflePrefab != null) enemyPrefabsList.Add(riflePrefab);
            if (shieldPrefab != null) enemyPrefabsList.Add(shieldPrefab);
            if (grenadierPrefab != null) enemyPrefabsList.Add(grenadierPrefab);
            if (dronePrefab != null) enemyPrefabsList.Add(dronePrefab);
            if (sniperPrefab != null) enemyPrefabsList.Add(sniperPrefab);
            if (heavyPrefab != null) enemyPrefabsList.Add(heavyPrefab);

            var bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/MiniBoss_Walker.prefab");
            so.FindProperty("bossPrefab").objectReferenceValue = bossPrefab;

            var enemyPrefabsProp = so.FindProperty("enemyPrefabs");
            enemyPrefabsProp.arraySize = enemyPrefabsList.Count;
            for (int i = 0; i < enemyPrefabsList.Count; i++)
                enemyPrefabsProp.GetArrayElementAtIndex(i).objectReferenceValue = enemyPrefabsList[i];

            // 加载敌人定义
            var enemyDefsList = new List<EnemyDefinition>();
            var defsPath = "Assets/Game/Data/Enemies";
            if (AssetDatabase.IsValidFolder(defsPath))
            {
                var guids = AssetDatabase.FindAssets("t:EnemyDefinition", new[] { defsPath });
                foreach (var guid in guids)
                {
                    var def = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                    if (def != null) enemyDefsList.Add(def);
                }
            }
            var enemyDefsProp = so.FindProperty("enemyDefs");
            enemyDefsProp.arraySize = enemyDefsList.Count;
            for (int i = 0; i < enemyDefsList.Count; i++)
                enemyDefsProp.GetArrayElementAtIndex(i).objectReferenceValue = enemyDefsList[i];

            // 加载拾取物预制体
            var healthPickup = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Pickup_Health.prefab");
            var weaponPickup = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Pickup_Weapon_shotgun.prefab");
            if (weaponPickup == null)
                weaponPickup = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Pickup_Weapon_rocket_launcher.prefab");
            so.FindProperty("healthPickupPrefab").objectReferenceValue = healthPickup;
            so.FindProperty("weaponPickupPrefab").objectReferenceValue = weaponPickup;

            so.ApplyModifiedProperties();

            // 摄像机跟随
            var follow = Camera.main.gameObject.GetComponent<SimpleCameraFollow>();
            if (follow == null)
                follow = Camera.main.gameObject.AddComponent<SimpleCameraFollow>();
            var followSo = new SerializedObject(follow);
            followSo.FindProperty("target").objectReferenceValue = player.transform;
            followSo.FindProperty("minX").floatValue = -100f;
            followSo.FindProperty("maxX").floatValue = 100f;
            followSo.ApplyModifiedProperties();

            // VFX 和 UI
            var vfx = new GameObject("--- VFX ---");
            vfx.AddComponent<ParallaxBackground>();
            CreateGameUI();

            SaveScene(scene, "EndlessMode");
            Debug.Log("[LevelBuilder] EndlessMode scene built!");
        }

        // ═══════════════════════════════════════════
        //  工厂方法
        // ═══════════════════════════════════════════
        private static GameObject CreatePlayer(Vector3 position)
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            player.layer = 7; // Player layer
            player.transform.position = position;

            // 物理组件
            var rb = player.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var box = player.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.8f, 1.2f);

            // 游戏组件
            player.AddComponent<Health>();
            player.AddComponent<PlayerController2D>();
            player.AddComponent<PlayerCombat>();
            player.AddComponent<PlayerDodge>();
            player.AddComponent<PlayerSquad>();

            // 视觉
            var sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.3f, 0.7f, 1f), 32, 48);
            sr.sortingOrder = 10;

            // Muzzle
            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(player.transform);
            muzzle.transform.localPosition = new Vector3(0.6f, 0.1f, 0);

            // GroundCheck
            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(player.transform);
            groundCheck.transform.localPosition = new Vector3(0, -0.7f, 0);

            return player;
        }

        private static GameObject CreateGround(Transform parent, string name, Vector3 position, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.layer = 6; // Ground layer

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.35f, 0.35f, 0.4f), 64, 64);
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            sr.sortingOrder = -1;

            var box = go.AddComponent<BoxCollider2D>();
            return go;
        }

        private static void CreateSpikeHazard(Transform parent, string name, Vector3 position, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.layer = 6;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.8f, 0.2f, 0.2f), 64, 32);
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            sr.sortingOrder = 0;

            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            go.AddComponent<SpikeHazard>();
        }

        private static void CreateExplosiveBarrel(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.6f, 0.4f, 0.1f), 24, 32);
            sr.sortingOrder = 1;

            go.AddComponent<BoxCollider2D>();
            go.AddComponent<Rigidbody2D>().freezeRotation = true;
            go.AddComponent<Health>();
            go.AddComponent<ExplosiveBarrel>();
        }

        private static void CreateEnemyPlaceholder(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.layer = 8; // Enemy layer
            go.tag = "Untagged";

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.8f, 0.3f, 0.3f), 32, 48);
            sr.sortingOrder = 5;

            go.AddComponent<Rigidbody2D>().freezeRotation = true;
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<Health>();
            go.AddComponent<EnemyController>();

            // LootDrop
            var loot = go.AddComponent<LootDrop>();

            // 火力点
            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0.6f, 0.1f, 0);
        }

        private static void CreateBossPlaceholder(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.layer = 8;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.9f, 0.1f, 0.1f), 64, 64);
            sr.sortingOrder = 5;

            go.AddComponent<Rigidbody2D>();
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<Health>();

            if (name.Contains("Turret"))
            {
                go.AddComponent<TurretBoss>();
            }
            else
            {
                go.AddComponent<MiniBossWalker>();
            }

            go.AddComponent<LootDrop>();

            // 火力点
            for (int i = 0; i < 3; i++)
            {
                var fp = new GameObject($"FirePoint_{i}");
                fp.transform.SetParent(go.transform);
                fp.transform.localPosition = new Vector3(1f, 0.5f - i * 0.5f, 0);
            }
        }

        private static void CreateCheckpoint(Transform parent, string name, Vector3 position, CheckpointManager mgr)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.2f, 0.9f, 0.3f), 16, 48);
            sr.sortingOrder = 2;

            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(2, 3);

            var cp = go.AddComponent<Checkpoint>();
            SetField(cp, "manager", mgr);
        }

        private static CheckpointManager CreateCheckpointManager(Transform parent)
        {
            var go = new GameObject("CheckpointManager");
            go.transform.SetParent(parent);
            go.transform.position = Vector3.zero;
            return go.AddComponent<CheckpointManager>();
        }

        private static void CreatePickupPlaceholder(Transform parent, string name, Vector3 position, string type)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;

            Color color = type switch
            {
                "Health" => new Color(0.2f, 0.9f, 0.3f),
                "Shield" => new Color(0.3f, 0.5f, 1f),
                "Weapon" => new Color(1f, 0.8f, 0.1f),
                "Invincible" => new Color(1f, 1f, 0.3f),
                _ => Color.white
            };

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(color, 20, 20);
            sr.sortingOrder = 3;

            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;

            switch (type)
            {
                case "Health":
                    go.AddComponent<HealthPickup>();
                    break;
                case "Shield":
                    go.AddComponent<ShieldPickup>();
                    break;
                case "Weapon":
                    go.AddComponent<WeaponPickup>();
                    break;
                case "Invincible":
                    go.AddComponent<InvinciblePickup>();
                    break;
            }
        }

        // ═══════════════════════════════════════════
        //  叙事系统
        // ═══════════════════════════════════════════
        private static GameObject CreateStoryManager()
        {
            var canvas = CreateCanvas("StoryCanvas");
            var panel = CreatePanel(canvas.transform, "StoryPanel", Vector2.zero, new Vector2(800, 200));
            var speakerText = CreateText(panel.transform, "SpeakerText", "", 24, TextAnchor.MiddleLeft, new Vector2(0, 60), new Vector2(760, 40));
            var bodyText = CreateText(panel.transform, "BodyText", "", 18, TextAnchor.UpperLeft, new Vector2(0, -20), new Vector2(760, 120));
            var cg = panel.AddComponent<CanvasGroup>();

            var go = canvas.gameObject;
            var story = go.AddComponent<StoryManager>();

            // 设置 StoryManager 的 UI 引用
            var so = new SerializedObject(story);
            so.FindProperty("storyPanel").objectReferenceValue = panel;
            so.FindProperty("speakerText").objectReferenceValue = speakerText;
            so.FindProperty("bodyText").objectReferenceValue = bodyText;
            so.FindProperty("canvasGroup").objectReferenceValue = cg;
            so.ApplyModifiedProperties();

            return go;
        }

        private static void ConfigureStoryManagerLevel01()
        {
            var story = FindFirstObjectByType<StoryManager>();
            if (story == null) return;
            var so = new SerializedObject(story);

            var introProp = so.FindProperty("introBeats");
            introProp.arraySize = 2;
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Aila";
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "铁雨事变第37天。我们四人小队终于推进到了敌方海岸线。";
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 4f;
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Aila";
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "目标：穿越五大战区，直捣敌方要塞。全员，准备登陆。";
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            var bossProp = so.FindProperty("bossWarningBeats");
            bossProp.arraySize = 2;
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Mara";
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "前方检测到大型炮塔信号...是敌方海岸炮塔！";
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 3.5f;
            bossProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Bruno";
            bossProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "注意它的散射弹幕和追踪导弹！准备战斗！";
            bossProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            var outroProp = so.FindProperty("outroBeats");
            outroProp.arraySize = 2;
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Niko";
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "海岸炮塔已摧毁。通往内陆的道路打开了。";
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 4f;
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Aila";
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "干得漂亮，小队。下一站：工厂内部。";
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            so.ApplyModifiedProperties();
        }

        private static void ConfigureStoryManagerLevel02()
        {
            var story = FindFirstObjectByType<StoryManager>();
            if (story == null) return;
            var so = new SerializedObject(story);

            var introProp = so.FindProperty("introBeats");
            introProp.arraySize = 2;
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Aila";
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "我们进入工厂内部了。这里布满了自动化防御系统。";
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 4f;
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Niko";
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "检测到深处有四足机甲...它的跳跃砸地很危险。";
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            var bossProp = so.FindProperty("bossWarningBeats");
            bossProp.arraySize = 1;
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Mara";
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "四足机甲激活了！等它跳跃砸地时核心暴露！";
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 3.5f;

            var outroProp = so.FindProperty("outroBeats");
            outroProp.arraySize = 2;
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Bruno";
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "机甲核心已摧毁。工厂防御系统瘫痪。";
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 4f;
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Aila";
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "工厂已清理。下一站：前线战区。继续推进。";
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            so.ApplyModifiedProperties();
        }

        private static void ConfigureStoryManagerLevel03()
        {
            var story = FindFirstObjectByType<StoryManager>();
            if (story == null) return;
            var so = new SerializedObject(story);

            var introProp = so.FindProperty("introBeats");
            introProp.arraySize = 2;
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Aila";
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "我们进入了前线战区。这里到处都是废墟和弹坑。";
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 4f;
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Niko";
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "检测到狙击手和重机枪手...保持移动，不要暴露。";
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            var bossProp = so.FindProperty("bossWarningBeats");
            bossProp.arraySize = 1;
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Mara";
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "炮塔Boss在前方！注意掩体后的散射弹幕！";
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 3.5f;

            var outroProp = so.FindProperty("outroBeats");
            outroProp.arraySize = 2;
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Bruno";
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "战区炮塔已摧毁。敌方防线被撕开了一个口子。";
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 4f;
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Aila";
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "下一站：地下掩体。那里是敌方的指挥中心。";
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            so.ApplyModifiedProperties();
        }

        private static void ConfigureStoryManagerLevel04()
        {
            var story = FindFirstObjectByType<StoryManager>();
            if (story == null) return;
            var so = new SerializedObject(story);

            var introProp = so.FindProperty("introBeats");
            introProp.arraySize = 2;
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Aila";
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "我们进入了地下掩体。空间狭窄，注意近战敌人。";
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 4f;
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Bruno";
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "盾牌兵和投弹手在这里很危险。用手雷和霰弹枪开路。";
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            var bossProp = so.FindProperty("bossWarningBeats");
            bossProp.arraySize = 1;
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Niko";
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "四足机甲激活了！它会跳跃砸地，核心在背部暴露时攻击！";
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 3.5f;

            var outroProp = so.FindProperty("outroBeats");
            outroProp.arraySize = 2;
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Mara";
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "机甲核心已摧毁。掩体防御系统瘫痪。";
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 4f;
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Aila";
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "最后一站：敌方要塞。全力进攻！";
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            so.ApplyModifiedProperties();
        }

        private static void ConfigureStoryManagerLevel05()
        {
            var story = FindFirstObjectByType<StoryManager>();
            if (story == null) return;
            var so = new SerializedObject(story);

            var introProp = so.FindProperty("introBeats");
            introProp.arraySize = 2;
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Aila";
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "这就是敌方要塞。最后的防线，最后的战斗。";
            introProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 4f;
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Bruno";
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "所有敌人类型都在这里。保持警惕，小队！";
            introProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            var bossProp = so.FindProperty("bossWarningBeats");
            bossProp.arraySize = 1;
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Niko";
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "最终炮塔Boss激活了！这是最危险的一战！";
            bossProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 3.5f;

            var outroProp = so.FindProperty("outroBeats");
            outroProp.arraySize = 2;
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("speaker").stringValue = "Mara";
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue =
                "要塞核心已摧毁。敌方指挥系统彻底瘫痪。";
            outroProp.GetArrayElementAtIndex(0).FindPropertyRelative("displayDuration").floatValue = 4f;
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("speaker").stringValue = "Aila";
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue =
                "任务完成。小队，撤离。我们赢得了这场战争。";
            outroProp.GetArrayElementAtIndex(1).FindPropertyRelative("displayDuration").floatValue = 4f;

            so.ApplyModifiedProperties();
        }

        // ═══════════════════════════════════════════
        //  教程系统
        // ═══════════════════════════════════════════
        private static GameObject CreateTutorialSystem()
        {
            var canvas = CreateCanvas("TutorialCanvas");
            var panel = CreatePanel(canvas.transform, "TutorialPanel", new Vector2(0, 200), new Vector2(500, 100));
            var instructionText = CreateText(panel.transform, "InstructionText", "", 20, TextAnchor.MiddleCenter, new Vector2(0, 10), new Vector2(480, 60));
            var progressText = CreateText(panel.transform, "ProgressText", "", 14, TextAnchor.LowerRight, new Vector2(200, -35), new Vector2(100, 30));
            var cg = panel.AddComponent<CanvasGroup>();

            var go = canvas.gameObject;
            go.AddComponent<TutorialManager>();

            return go;
        }

        // ═══════════════════════════════════════════
        //  游戏 HUD
        // ═══════════════════════════════════════════
        private static Canvas CreateGameUI()
        {
            var canvas = CreateCanvas("GameUICanvas");

            // HUD
            var hud = canvas.gameObject.AddComponent<HudPresenter>();

            // 血条
            var healthBar = CreatePanel(canvas.transform, "HealthBar", new Vector2(-350, 230), new Vector2(200, 20));
            healthBar.AddComponent<HealthBar>();

            // 分数文本
            var scoreText = CreateText(canvas.transform, "ScoreText", "Score: 0", 18, TextAnchor.UpperRight, new Vector2(350, 230), new Vector2(200, 30));

            // 连击文本
            var comboText = CreateText(canvas.transform, "ComboText", "", 24, TextAnchor.UpperCenter, new Vector2(0, 200), new Vector2(200, 40));

            // 波次文本（EndlessMode 使用，默认隐藏）
            var waveText = CreateText(canvas.transform, "WaveText", "", 22, TextAnchor.UpperCenter, new Vector2(0, 170), new Vector2(300, 30));
            waveText.gameObject.SetActive(false);

            // Boss血条
            var bossBar = canvas.gameObject.AddComponent<BossHealthBar>();

            // 游戏结束画面
            var gameOver = CreateGameOverScreen(canvas.transform);

            // 胜利画面
            var gameComplete = CreateGameCompleteScreen(canvas.transform);
            var victory = CreateVictoryScreen(canvas.transform, gameComplete);

            // 暂停菜单
            var pause = CreatePauseMenu(canvas.transform);

            // 低血量特效
            canvas.gameObject.AddComponent<LowHealthVignette>();

            // 伤害方向指示器
            canvas.gameObject.AddComponent<DamageDirectionIndicator>();

            // 角色切换提示
            canvas.gameObject.AddComponent<CharacterSwitchToast>();

            // 成就通知
            canvas.gameObject.AddComponent<AchievementNotification>();

            // 成就跟踪器（确保每个场景都有）
            if (FindFirstObjectByType<SteelRain.UI.AchievementTracker>() == null)
            {
                var trackerGo = new GameObject("AchievementTracker");
                trackerGo.AddComponent<SteelRain.UI.AchievementTracker>();
            }

            // 设置 HUD 字段
            SetField(hud, "scoreText", scoreText);
            SetField(hud, "comboText", comboText);
            SetField(hud, "waveText", waveText);

            return canvas;
        }

        private static GameOverScreen CreateGameOverScreen(Transform parent)
        {
            var panel = CreatePanel(parent, "GameOverPanel", Vector2.zero, new Vector2(400, 300));
            panel.SetActive(false);
            CreateText(panel.transform, "GameOverTitle", "GAME OVER", 36, TextAnchor.MiddleCenter, new Vector2(0, 80), new Vector2(360, 50));
            var restartBtn = CreateButton(panel.transform, "RestartButton", "Restart", new Vector2(-80, -60), new Vector2(140, 45));
            var menuBtn = CreateButton(panel.transform, "MenuButton", "Main Menu", new Vector2(80, -60), new Vector2(140, 45));

            var screen = panel.AddComponent<GameOverScreen>();
            SetField(screen, "panel", panel);
            SetField(screen, "retryButton", restartBtn);
            SetField(screen, "menuButton", menuBtn);

            return screen;
        }

        private static VictoryScreen CreateVictoryScreen(Transform parent, GameCompleteScreen gameCompleteScreen)
        {
            var panel = CreatePanel(parent, "VictoryPanel", Vector2.zero, new Vector2(450, 350));
            panel.SetActive(false);
            CreateText(panel.transform, "VictoryTitle", "VICTORY!", 36, TextAnchor.MiddleCenter, new Vector2(0, 110), new Vector2(400, 50));
            var gradeText = CreateText(panel.transform, "VictoryGrade", "S", 60, TextAnchor.MiddleCenter, new Vector2(0, 50), new Vector2(100, 80));
            var scoreText = CreateText(panel.transform, "VictoryScore", "", 18, TextAnchor.MiddleCenter, new Vector2(0, -10), new Vector2(400, 40));
            var timeText = CreateText(panel.transform, "VictoryTime", "", 16, TextAnchor.MiddleCenter, new Vector2(0, -40), new Vector2(400, 30));
            var menuBtn = CreateButton(panel.transform, "MenuButton", "Main Menu", new Vector2(-80, -100), new Vector2(140, 45));
            var nextBtn = CreateButton(panel.transform, "NextLevelButton", "Next Level", new Vector2(80, -100), new Vector2(140, 45));

            var screen = panel.AddComponent<VictoryScreen>();
            SetField(screen, "panel", panel);
            SetField(screen, "menuButton", menuBtn);
            SetField(screen, "nextLevelButton", nextBtn);
            SetField(screen, "scoreText", scoreText);
            SetField(screen, "gradeText", gradeText);
            SetField(screen, "timeText", timeText);
            SetField(screen, "gameCompleteScreen", gameCompleteScreen);

            return screen;
        }

        private static GameCompleteScreen CreateGameCompleteScreen(Transform parent)
        {
            var panel = CreatePanel(parent, "GameCompletePanel", Vector2.zero, new Vector2(500, 400));
            panel.SetActive(false);
            var titleText = CreateText(panel.transform, "GCTitle", "CAMPAIGN COMPLETE!", 32, TextAnchor.MiddleCenter, new Vector2(0, 140), new Vector2(460, 50));
            var statsText = CreateText(panel.transform, "GCStats", "", 16, TextAnchor.MiddleCenter, new Vector2(0, -20), new Vector2(460, 80));
            var menuBtn = CreateButton(panel.transform, "MenuButton", "Main Menu", new Vector2(0, -130), new Vector2(160, 45));

            var screen = panel.AddComponent<GameCompleteScreen>();
            SetField(screen, "panel", panel);
            SetField(screen, "titleText", titleText);
            SetField(screen, "statsText", statsText);
            SetField(screen, "menuButton", menuBtn);

            return screen;
        }

        private static PauseManager CreatePauseMenu(Transform parent)
        {
            var panel = CreatePanel(parent, "PausePanel", Vector2.zero, new Vector2(300, 350));
            panel.SetActive(false);
            var titleText = CreateText(panel.transform, "PauseTitle", "PAUSED", 32, TextAnchor.MiddleCenter, new Vector2(0, 120), new Vector2(260, 50));
            var hintText = CreateText(panel.transform, "PauseHint", "ESC: Resume | R: Restart | Q: Quit", 14, TextAnchor.MiddleCenter, new Vector2(0, -130), new Vector2(260, 30));

            // 内嵌设置滑块
            CreateText(panel.transform, "MasterLabel", "Master", 14, TextAnchor.MiddleLeft, new Vector2(-80, 50), new Vector2(80, 25));
            var masterSlider = CreateSlider(panel.transform, "MasterSlider", new Vector2(40, 50), new Vector2(140, 20));
            CreateText(panel.transform, "MusicLabel", "Music", 14, TextAnchor.MiddleLeft, new Vector2(-80, 15), new Vector2(80, 25));
            var musicSlider = CreateSlider(panel.transform, "MusicSlider", new Vector2(40, 15), new Vector2(140, 20));
            CreateText(panel.transform, "SfxLabel", "SFX", 14, TextAnchor.MiddleLeft, new Vector2(-80, -20), new Vector2(80, 25));
            var sfxSlider = CreateSlider(panel.transform, "SfxSlider", new Vector2(40, -20), new Vector2(140, 20));

            var pm = panel.AddComponent<PauseManager>();
            SetField(pm, "pausePanel", panel);
            SetField(pm, "pauseTitle", titleText);
            SetField(pm, "pauseHint", hintText);
            SetField(pm, "masterSlider", masterSlider);
            SetField(pm, "musicSlider", musicSlider);
            SetField(pm, "sfxSlider", sfxSlider);

            return pm;
        }

        // ═══════════════════════════════════════════
        //  设置面板（菜单/暂停共用）
        // ═══════════════════════════════════════════
        private static CharacterSelectScreen CreateCharacterSelectScreen(Transform parent)
        {
            var panel = CreatePanel(parent, "CharacterSelectPanel", Vector2.zero, new Vector2(600, 450));
            panel.SetActive(false);

            CreateText(panel.transform, "CSTitle", "CHARACTER SELECT", 28, TextAnchor.MiddleCenter, new Vector2(0, 180), new Vector2(560, 50));
            var nameText = CreateText(panel.transform, "CSName", "", 24, TextAnchor.MiddleCenter, new Vector2(0, 120), new Vector2(560, 40));
            var styleText = CreateText(panel.transform, "CSStyle", "", 16, TextAnchor.MiddleCenter, new Vector2(0, 80), new Vector2(560, 40));
            var loreText = CreateText(panel.transform, "CSLore", "", 14, TextAnchor.MiddleCenter, new Vector2(0, 20), new Vector2(560, 60));

            // 4个角色按钮
            var btn0 = CreateButton(panel.transform, "CharBtn0", "1", new Vector2(-180, -60), new Vector2(60, 60));
            var btn1 = CreateButton(panel.transform, "CharBtn1", "2", new Vector2(-60, -60), new Vector2(60, 60));
            var btn2 = CreateButton(panel.transform, "CharBtn2", "3", new Vector2(60, -60), new Vector2(60, 60));
            var btn3 = CreateButton(panel.transform, "CharBtn3", "4", new Vector2(180, -60), new Vector2(60, 60));
            var charButtons = new[] { btn0, btn1, btn2, btn3 };

            var confirmBtn = CreateButton(panel.transform, "ConfirmButton", "Confirm", new Vector2(-80, -140), new Vector2(140, 45));
            var backBtn = CreateButton(panel.transform, "BackButton", "Back", new Vector2(80, -140), new Vector2(140, 45));

            var screen = panel.AddComponent<CharacterSelectScreen>();
            SetField(screen, "panel", panel);
            SetField(screen, "characterNameText", nameText);
            SetField(screen, "characterStyleText", styleText);
            SetField(screen, "characterLoreText", loreText);
            SetField(screen, "confirmButton", confirmBtn);
            SetField(screen, "backButton", backBtn);
            SetField(screen, "characterButtons", charButtons);

            // 加载角色定义
            var charDefs = new List<CharacterDefinition>();
            var charDefsPath = "Assets/Game/Data/Characters";
            if (AssetDatabase.IsValidFolder(charDefsPath))
            {
                var guids = AssetDatabase.FindAssets("t:CharacterDefinition", new[] { charDefsPath });
                foreach (var guid in guids)
                {
                    var def = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                    if (def != null) charDefs.Add(def);
                }
            }
            // 如果没有找到，尝试从 Resources 或 Prefabs 加载
            if (charDefs.Count == 0)
            {
                var allGuids = AssetDatabase.FindAssets("t:CharacterDefinition");
                foreach (var guid in allGuids)
                {
                    var def = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                    if (def != null && !charDefs.Contains(def)) charDefs.Add(def);
                }
            }
            SetField(screen, "characters", charDefs.ToArray());

            return screen;
        }

        private static GameObject CreateSettingsPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "SettingsPanel", Vector2.zero, new Vector2(400, 350));
            panel.SetActive(false);
            CreateText(panel.transform, "SettingsTitle", "SETTINGS", 28, TextAnchor.MiddleCenter, new Vector2(0, 120), new Vector2(360, 50));

            // 音量滑块
            CreateText(panel.transform, "MasterLabel", "Master Volume", 16, TextAnchor.MiddleLeft, new Vector2(-100, 60), new Vector2(140, 30));
            var masterSlider = CreateSlider(panel.transform, "MasterSlider", new Vector2(60, 60), new Vector2(180, 25));

            CreateText(panel.transform, "MusicLabel", "Music Volume", 16, TextAnchor.MiddleLeft, new Vector2(-100, 20), new Vector2(140, 30));
            var musicSlider = CreateSlider(panel.transform, "MusicSlider", new Vector2(60, 20), new Vector2(180, 25));

            CreateText(panel.transform, "SfxLabel", "SFX Volume", 16, TextAnchor.MiddleLeft, new Vector2(-100, -20), new Vector2(140, 30));
            var sfxSlider = CreateSlider(panel.transform, "SfxSlider", new Vector2(60, -20), new Vector2(180, 25));

            // 全屏开关
            var fullscreenToggle = CreateToggle(panel.transform, "FullscreenToggle", "Fullscreen", new Vector2(0, -70));

            // 按钮
            var applyBtn = CreateButton(panel.transform, "ApplyButton", "Apply", new Vector2(-60, -120), new Vector2(100, 40));
            var backBtn = CreateButton(panel.transform, "BackButton", "Back", new Vector2(60, -120), new Vector2(100, 40));

            var settings = panel.AddComponent<SettingsManager>();
            SetField(settings, "masterSlider", masterSlider);
            SetField(settings, "musicSlider", musicSlider);
            SetField(settings, "sfxSlider", sfxSlider);
            SetField(settings, "fullscreenToggle", fullscreenToggle);
            SetField(settings, "applyButton", applyBtn);
            SetField(settings, "backButton", backBtn);
            SetField(settings, "panel", panel);

            return panel;
        }

        // ═══════════════════════════════════════════
        //  UI 辅助方法
        // ═══════════════════════════════════════════
        private static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            go.AddComponent<GraphicRaycaster>();
            go.AddComponent<RectTransform>();
            return canvas;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            return go;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.3f, 0.5f, 1f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(go.transform, false);
            var txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;
            var txt = txtGo.AddComponent<Text>();
            txt.text = label;
            txt.fontSize = 18;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return btn;
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.25f);

            // Fill Area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0, 0.25f);
            faRt.anchorMax = new Vector2(1, 0.75f);
            faRt.sizeDelta = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRt = fill.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.6f, 1f);

            // Handle
            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(go.transform, false);
            var haRt = handleArea.AddComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.sizeDelta = Vector2.zero;

            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            var hRt = handle.AddComponent<RectTransform>();
            hRt.sizeDelta = new Vector2(20, 0);
            var hImg = handle.AddComponent<Image>();
            hImg.color = Color.white;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = hRt;
            slider.targetGraphic = hImg;
            slider.value = 0.7f;

            return slider;
        }

        private static Toggle CreateToggle(Transform parent, string name, string label, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(200, 30);

            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0);
            bgRt.anchorMax = new Vector2(0, 1);
            bgRt.sizeDelta = new Vector2(25, 0);
            bgRt.anchoredPosition = new Vector2(12.5f, 0);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.3f, 0.3f, 0.4f);

            var check = new GameObject("Checkmark");
            check.transform.SetParent(bg.transform, false);
            var ckRt = check.AddComponent<RectTransform>();
            ckRt.anchorMin = Vector2.zero;
            ckRt.anchorMax = Vector2.one;
            ckRt.sizeDelta = Vector2.zero;
            var ckImg = check.AddComponent<Image>();
            ckImg.color = new Color(0.3f, 0.8f, 0.3f);

            var txtGo = new GameObject("Label");
            txtGo.transform.SetParent(go.transform, false);
            var txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = new Vector2(0, 0);
            txtRt.anchorMax = new Vector2(1, 1);
            txtRt.offsetMin = new Vector2(35, 0);
            txtRt.offsetMax = Vector2.zero;
            var txt = txtGo.AddComponent<Text>();
            txt.text = label;
            txt.fontSize = 16;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.color = Color.white;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = bgImg;
            toggle.graphic = ckImg;
            toggle.isOn = true;

            return toggle;
        }

        // ═══════════════════════════════════════════
        //  Sprite 辅助
        // ═══════════════════════════════════════════
        private static Sprite CreatePlaceholderSprite(Color color, int width, int height)
        {
            var tex = new Texture2D(width, height);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 边框加深
                    bool border = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                    pixels[y * width + x] = border ? color * 0.6f : color;
                    pixels[y * width + x].a = 1f;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32);
        }

        // ═══════════════════════════════════════════
        //  反射辅助
        // ═══════════════════════════════════════════
        private static void SetField(Object target, string fieldName, object value)
        {
            var type = target.GetType();
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            if (field != null)
                field.SetValue(target, value);
        }

        private static void SaveScene(UnityEngine.SceneManagement.Scene scene, string name)
        {
            Directory.CreateDirectory(LevelDir);
            var path = $"{LevelDir}/{name}.unity";
            EditorSceneManager.SaveScene(scene, path);
            AddSceneToBuildSettings(path);
            Debug.Log($"[LevelBuilder] Saved: {path}");
        }

        /// <summary>
        /// 将场景添加到 BuildSettings（如果尚未存在）。
        /// </summary>
        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return;
            }

            var list = new List<EditorBuildSettingsScene>(scenes)
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = list.ToArray();
            Debug.Log($"[LevelBuilder] Added to BuildSettings: {scenePath}");
        }
    }
}
