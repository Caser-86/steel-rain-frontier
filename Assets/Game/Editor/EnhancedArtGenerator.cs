using System.IO;
using UnityEditor;
using UnityEngine;

namespace SteelRain.Editor
{
    /// <summary>
    /// 增强版美术资源生成器：生成更精致的像素艺术精灵。
    /// </summary>
    public static class EnhancedArtGenerator
    {
        private const string ArtDir = "Assets/Art/Generated";

        [MenuItem("Steel Rain/Generate Enhanced Art")]
        public static void GenerateAll()
        {
            Debug.Log("[EnhancedArt] Generating enhanced art assets...");
            if (!Directory.Exists(ArtDir))
                Directory.CreateDirectory(ArtDir);

            GeneratePlayerSprites();
            GenerateEnemySprites();
            GenerateGroundSprites();
            GeneratePropSprites();
            GenerateProjectileSprites();
            GenerateBackgroundSprites();
            GenerateEffectSprites();
            GenerateUISprites();
            GenerateParallaxSprites();
            GenerateExtraEffectSprites();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EnhancedArt] All enhanced art assets generated!");
        }

        private static void Clear(Color32[] px, int w, int h)
        {
            var transparent = new Color32(0, 0, 0, 0);
            for (int i = 0; i < px.Length; i++) px[i] = transparent;
        }

        private static void FillRect(Color32[] px, int w, int x0, int y0, int x1, int y1, Color32 c)
        {
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    if (y >= 0 && x >= 0 && y * w + x < px.Length)
                        px[y * w + x] = c;
        }

        private static void DrawOutline(Color32[] px, int w, int h, Color32 outline)
        {
            var result = (Color32[])px.Clone();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (px[y * w + x].a == 0) continue;
                    bool needOutline = false;
                    if (x == 0 || x == w - 1 || y == 0 || y == h - 1) needOutline = true;
                    else
                    {
                        if (px[y * w + x - 1].a == 0 || px[y * w + x + 1].a == 0 ||
                            px[(y - 1) * w + x].a == 0 || px[(y + 1) * w + x].a == 0)
                            needOutline = true;
                    }
                    if (needOutline) result[y * w + x] = outline;
                }
            }
            for (int i = 0; i < px.Length; i++) px[i] = result[i];
        }

        private static void SaveSprite(string name, Texture2D tex, Color32[] px)
        {
            tex.SetPixels32(px);
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
            Debug.Log($"[EnhancedArt] Saved {name}.png ({tex.width}x{tex.height})");
        }

        private static void GeneratePlayerSprites()
        {
            MakePlayerAila();
            MakePlayerBruno();
            MakePlayerMara();
            MakePlayerNiko();

            // 为每个角色生成 prone/crouch/jump 三种动作变体
            // 这样在游戏中切换角色或按下蹲下/趴下/跳跃键时能看到明显差别
            MakePlayerActionVariants();
        }

        /// <summary>
        /// 为 4 个角色生成 prone（趴下）、crouch（蹲下）、jump（跳跃）三种动作的精灵。
        /// 每个角色保留其标志性的颜色和武器，但姿势不同：
        /// - prone：身体水平贴地，高度只有原来的 35%
        /// - crouch：单膝跪地，高度 60%
        /// - jump：双腿弯曲腾空，手臂上举
        /// </summary>
        private static void MakePlayerActionVariants()
        {
            MakePlayerProne("aila",   new Color32(60, 100, 165, 255),  new Color32(200, 55, 55, 255),  "rifle");
            MakePlayerProne("bruno",  new Color32(165, 60, 55, 255),   new Color32(220, 180, 80, 255), "shield");
            MakePlayerProne("mara",   new Color32(70, 110, 70, 255),   new Color32(180, 150, 100, 255),"sniper");
            MakePlayerProne("niko",   new Color32(110, 80, 140, 255),  new Color32(240, 200, 80, 255), "wrench");

            MakePlayerCrouch("aila",  new Color32(60, 100, 165, 255),  new Color32(200, 55, 55, 255),  "rifle");
            MakePlayerCrouch("bruno", new Color32(165, 60, 55, 255),   new Color32(220, 180, 80, 255), "shield");
            MakePlayerCrouch("mara",  new Color32(70, 110, 70, 255),   new Color32(180, 150, 100, 255),"sniper");
            MakePlayerCrouch("niko",  new Color32(110, 80, 140, 255),  new Color32(240, 200, 80, 255), "wrench");

            MakePlayerJump("aila",    new Color32(60, 100, 165, 255),  new Color32(200, 55, 55, 255),  "rifle");
            MakePlayerJump("bruno",   new Color32(165, 60, 55, 255),   new Color32(220, 180, 80, 255), "shield");
            MakePlayerJump("mara",    new Color32(70, 110, 70, 255),   new Color32(180, 150, 100, 255),"sniper");
            MakePlayerJump("niko",    new Color32(110, 80, 140, 255),  new Color32(240, 200, 80, 255), "wrench");
        }

        // 通用颜色定义
        private static readonly Color32 SkinTone = new Color32(230, 195, 155, 255);
        private static readonly Color32 HairDark = new Color32(90, 60, 40, 255);
        private static readonly Color32 PantsColor = new Color32(50, 55, 70, 255);
        private static readonly Color32 BootsColor = new Color32(35, 30, 25, 255);
        private static readonly Color32 GunMetal = new Color32(55, 55, 60, 255);
        private static readonly Color32 GunDetail = new Color32(90, 90, 95, 255);
        private static readonly Color32 OutlineDark = new Color32(20, 20, 30, 255);

        /// <summary>
        /// 生成趴下姿势：身体水平贴地，头朝右，腿伸直
        /// </summary>
        private static void MakePlayerProne(string charName, Color32 coat, Color32 accent, string weapon)
        {
            int w = 32, h = 14;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var coatShadow = new Color32((byte)(coat.r * 0.6f), (byte)(coat.g * 0.6f), (byte)(coat.b * 0.6f), 255);
            var coatLight = new Color32((byte)Mathf.Min(coat.r * 1.3f, 255), (byte)Mathf.Min(coat.g * 1.3f, 255), (byte)Mathf.Min(coat.b * 1.3f, 255), 255);

            // 头（右侧）
            FillRect(px, w, 24, 6, 29, 11, SkinTone);
            FillRect(px, w, 24, 6, 24, 11, SkinTone);
            // 头发
            FillRect(px, w, 24, 10, 29, 12, HairDark);
            FillRect(px, w, 23, 7, 23, 10, HairDark);
            // 眼睛
            px[8 * w + 28] = OutlineDark;
            // 身体（水平）
            FillRect(px, w, 10, 5, 23, 10, coat);
            FillRect(px, w, 10, 5, 10, 10, coatShadow);
            FillRect(px, w, 23, 6, 23, 9, coatLight);
            // 腿（伸直向左）
            FillRect(px, w, 2, 5, 10, 8, PantsColor);
            FillRect(px, w, 2, 5, 4, 8, new Color32(35, 40, 55, 255));
            // 靴子（左端）
            FillRect(px, w, 0, 5, 2, 8, BootsColor);
            // 手臂（向前持枪）
            FillRect(px, w, 20, 7, 24, 9, coat);
            // 武器（根据类型）
            DrawWeaponProne(px, w, weapon, coat, accent);

            // 标志性装饰
            if (charName == "aila") FillRect(px, w, 12, 4, 16, 5, accent); // 红围巾
            if (charName == "bruno") FillRect(px, w, 14, 9, 22, 12, accent); // 盾牌在身下
            if (charName == "mara")  FillRect(px, w, 25, 11, 28, 13, accent); // 瞄准镜
            if (charName == "niko")  FillRect(px, w, 18, 3, 21, 5, accent); // 工具腰带

            DrawOutline(px, w, h, OutlineDark);
            SaveSprite($"player_{charName}_prone", tex, px);
        }

        private static void DrawWeaponProne(Color32[] px, int w, string weapon, Color32 coat, Color32 accent)
        {
            switch (weapon)
            {
                case "rifle":
                    FillRect(px, w, 22, 9, 30, 10, GunMetal);
                    FillRect(px, w, 29, 9, 30, 10, GunDetail);
                    break;
                case "shield":
                    FillRect(px, w, 22, 6, 28, 11, new Color32(180, 150, 80, 255));
                    FillRect(px, w, 23, 7, 27, 10, accent);
                    break;
                case "sniper":
                    FillRect(px, w, 20, 8, 31, 9, GunMetal);
                    FillRect(px, w, 24, 10, 27, 11, new Color32(40, 40, 45, 255));
                    break;
                case "wrench":
                    FillRect(px, w, 24, 8, 30, 9, GunMetal);
                    FillRect(px, w, 29, 7, 31, 10, accent);
                    break;
                case "smg":
                    FillRect(px, w, 23, 8, 29, 10, GunMetal);
                    FillRect(px, w, 28, 10, 29, 11, GunDetail);
                    break;
            }
        }

        /// <summary>
        /// 生成蹲下姿势：单膝跪地，身体压低
        /// </summary>
        private static void MakePlayerCrouch(string charName, Color32 coat, Color32 accent, string weapon)
        {
            int w = 24, h = 22;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var coatShadow = new Color32((byte)(coat.r * 0.6f), (byte)(coat.g * 0.6f), (byte)(coat.b * 0.6f), 255);
            var coatLight = new Color32((byte)Mathf.Min(coat.r * 1.3f, 255), (byte)Mathf.Min(coat.g * 1.3f, 255), (byte)Mathf.Min(coat.b * 1.3f, 255), 255);

            // 头
            FillRect(px, w, 9, 16, 14, 21, SkinTone);
            FillRect(px, w, 9, 16, 9, 21, SkinTone);
            FillRect(px, w, 8, 20, 15, 21, HairDark);
            FillRect(px, w, 8, 16, 8, 19, HairDark);
            px[18 * w + 10] = OutlineDark;
            px[18 * w + 13] = OutlineDark;
            // 身体（蹲下，压低）
            FillRect(px, w, 8, 9, 15, 15, coat);
            FillRect(px, w, 8, 9, 8, 15, coatShadow);
            FillRect(px, w, 15, 10, 15, 14, coatLight);
            // 单膝跪地（前腿弯曲）
            FillRect(px, w, 9, 5, 13, 9, PantsColor);
            FillRect(px, w, 10, 3, 13, 5, BootsColor);
            // 后腿
            FillRect(px, w, 13, 5, 15, 9, PantsColor);
            FillRect(px, w, 13, 3, 15, 5, BootsColor);
            // 手臂
            FillRect(px, w, 14, 11, 17, 14, coat);
            // 武器
            DrawWeaponCrouch(px, w, weapon, coat, accent);
            // 标志性装饰
            if (charName == "aila") FillRect(px, w, 9, 14, 14, 15, accent);
            if (charName == "bruno") FillRect(px, w, 16, 8, 21, 16, accent);
            if (charName == "mara")  FillRect(px, w, 12, 16, 14, 18, accent);
            if (charName == "niko")  FillRect(px, w, 10, 8, 13, 10, accent);

            DrawOutline(px, w, h, OutlineDark);
            SaveSprite($"player_{charName}_crouch", tex, px);
        }

        private static void DrawWeaponCrouch(Color32[] px, int w, string weapon, Color32 coat, Color32 accent)
        {
            switch (weapon)
            {
                case "rifle":
                    FillRect(px, w, 17, 12, 23, 13, GunMetal);
                    FillRect(px, w, 22, 12, 23, 13, GunDetail);
                    break;
                case "shield":
                    FillRect(px, w, 16, 8, 21, 16, new Color32(180, 150, 80, 255));
                    FillRect(px, w, 17, 9, 20, 15, accent);
                    break;
                case "sniper":
                    FillRect(px, w, 16, 12, 23, 13, GunMetal);
                    FillRect(px, w, 18, 14, 21, 15, new Color32(40, 40, 45, 255));
                    break;
                case "wrench":
                    FillRect(px, w, 17, 11, 22, 13, GunMetal);
                    FillRect(px, w, 21, 10, 23, 13, accent);
                    break;
                case "smg":
                    FillRect(px, w, 17, 12, 22, 14, GunMetal);
                    FillRect(px, w, 21, 14, 22, 15, GunDetail);
                    break;
            }
        }

        /// <summary>
        /// 生成跳跃姿势：双腿弯曲腾空，手臂上举
        /// </summary>
        private static void MakePlayerJump(string charName, Color32 coat, Color32 accent, string weapon)
        {
            int w = 24, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var coatShadow = new Color32((byte)(coat.r * 0.6f), (byte)(coat.g * 0.6f), (byte)(coat.b * 0.6f), 255);
            var coatLight = new Color32((byte)Mathf.Min(coat.r * 1.3f, 255), (byte)Mathf.Min(coat.g * 1.3f, 255), (byte)Mathf.Min(coat.b * 1.3f, 255), 255);

            // 头（略高）
            FillRect(px, w, 9, 24, 14, 29, SkinTone);
            FillRect(px, w, 9, 24, 9, 29, SkinTone);
            FillRect(px, w, 8, 28, 15, 30, HairDark);
            FillRect(px, w, 8, 24, 8, 27, HairDark);
            px[26 * w + 10] = OutlineDark;
            px[26 * w + 13] = OutlineDark;
            // 身体（腾空，风衣飘起）
            FillRect(px, w, 8, 14, 15, 23, coat);
            FillRect(px, w, 8, 14, 8, 23, coatShadow);
            FillRect(px, w, 15, 15, 15, 22, coatLight);
            // 风衣下摆飘动（跳跃时向后飘）
            FillRect(px, w, 4, 14, 8, 18, coat);
            FillRect(px, w, 2, 15, 5, 17, coatShadow);
            FillRect(px, w, 15, 16, 18, 20, coat);
            // 腿（弯曲，跳跃姿势）
            FillRect(px, w, 9, 10, 11, 14, PantsColor);
            FillRect(px, w, 12, 10, 14, 14, PantsColor);
            FillRect(px, w, 8, 8, 11, 10, PantsColor);
            FillRect(px, w, 12, 8, 15, 10, PantsColor);
            // 靴子
            FillRect(px, w, 8, 6, 11, 9, BootsColor);
            FillRect(px, w, 12, 6, 15, 9, BootsColor);
            // 手臂（上举）
            FillRect(px, w, 5, 18, 8, 22, coat);
            FillRect(px, w, 16, 18, 19, 22, coat);
            // 武器（举高）
            DrawWeaponJump(px, w, weapon, coat, accent);
            // 标志性装饰
            if (charName == "aila") FillRect(px, w, 9, 22, 14, 23, accent);
            if (charName == "bruno") FillRect(px, w, 4, 16, 8, 22, accent);
            if (charName == "mara")  FillRect(px, w, 12, 26, 14, 28, accent);
            if (charName == "niko")  FillRect(px, w, 10, 12, 13, 14, accent);

            DrawOutline(px, w, h, OutlineDark);
            SaveSprite($"player_{charName}_jump", tex, px);
        }

        private static void DrawWeaponJump(Color32[] px, int w, string weapon, Color32 coat, Color32 accent)
        {
            switch (weapon)
            {
                case "rifle":
                    FillRect(px, w, 17, 20, 23, 21, GunMetal);
                    FillRect(px, w, 22, 20, 23, 21, GunDetail);
                    break;
                case "shield":
                    FillRect(px, w, 4, 16, 8, 22, new Color32(180, 150, 80, 255));
                    FillRect(px, w, 5, 17, 7, 21, accent);
                    break;
                case "sniper":
                    FillRect(px, w, 16, 20, 23, 21, GunMetal);
                    FillRect(px, w, 18, 22, 21, 23, new Color32(40, 40, 45, 255));
                    break;
                case "wrench":
                    FillRect(px, w, 17, 19, 22, 21, GunMetal);
                    FillRect(px, w, 21, 18, 23, 21, accent);
                    break;
                case "smg":
                    FillRect(px, w, 17, 20, 22, 22, GunMetal);
                    FillRect(px, w, 21, 22, 22, 23, GunDetail);
                    break;
            }
        }

        private static void MakePlayerAila()
        {
            // Aila - 突击兵：蓝色长风衣 + 红围巾 + 步枪（端枪瞄准姿势）
            int w = 24, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var skin = new Color32(230, 195, 155, 255);
            var skinShadow = new Color32(195, 160, 125, 255);
            var hair = new Color32(90, 60, 40, 255);
            var coat = new Color32(60, 100, 165, 255);
            var coatShadow = new Color32(40, 75, 130, 255);
            var coatLight = new Color32(90, 130, 195, 255);
            var scarf = new Color32(200, 55, 55, 255);
            var scarfLight = new Color32(230, 80, 80, 255);
            var pants = new Color32(50, 55, 70, 255);
            var boots = new Color32(35, 30, 25, 255);
            var gun = new Color32(55, 55, 60, 255);
            var gunDetail = new Color32(90, 90, 95, 255);
            var outline = new Color32(20, 20, 30, 255);

            // 头发（后脑勺，长发）
            FillRect(px, w, 8, 24, 15, 30, hair);
            FillRect(px, w, 9, 23, 14, 30, hair);
            // 脸
            FillRect(px, w, 9, 25, 14, 28, skin);
            FillRect(px, w, 9, 24, 14, 24, skin);
            // 头发刘海
            FillRect(px, w, 8, 28, 15, 30, hair);
            // 眼睛
            px[26 * w + 10] = outline;
            px[26 * w + 13] = outline;
            FillRect(px, w, 9, 25, 9, 25, skinShadow);
            // 红围巾（脖子）
            FillRect(px, w, 9, 21, 14, 23, scarf);
            FillRect(px, w, 10, 22, 13, 22, scarfLight);
            // 围巾飘动（向后）
            FillRect(px, w, 5, 21, 8, 22, scarf);
            FillRect(px, w, 3, 22, 5, 22, scarf);
            // 风衣身体
            FillRect(px, w, 8, 14, 15, 20, coat);
            FillRect(px, w, 8, 14, 8, 20, coatShadow);
            FillRect(px, w, 15, 14, 15, 20, coatLight);
            // 风衣下摆飘动
            FillRect(px, w, 6, 14, 8, 19, coat);
            FillRect(px, w, 15, 15, 17, 19, coat);
            FillRect(px, w, 6, 15, 6, 19, coatShadow);
            // 围巾飘动细节
            px[18 * w + 11] = scarfLight;
            px[16 * w + 11] = scarfLight;
            // 腿
            FillRect(px, w, 9, 8, 11, 13, pants);
            FillRect(px, w, 12, 8, 14, 13, pants);
            // 靴子
            FillRect(px, w, 9, 5, 12, 8, boots);
            FillRect(px, w, 12, 5, 15, 8, boots);
            // 步枪（端枪瞄准，水平向前）
            FillRect(px, w, 17, 17, 23, 18, gun);
            FillRect(px, w, 16, 18, 17, 18, gun);
            FillRect(px, w, 22, 17, 23, 18, gunDetail);
            FillRect(px, w, 18, 19, 19, 20, gun);
            // 枪口
            px[17 * w + 23] = gunDetail;

            DrawOutline(px, w, h, outline);
            SaveSprite("player_aila", tex, px);
        }

        private static void MakePlayerBruno()
        {
            // Bruno - 盾兵：红色重甲 + 大盾牌 + 短剑（举盾防御姿势）
            int w = 24, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var skin = new Color32(220, 180, 140, 255);
            var hair = new Color32(40, 35, 30, 255);
            var armor = new Color32(140, 50, 50, 255);
            var armorLight = new Color32(175, 70, 70, 255);
            var armorShadow = new Color32(100, 35, 35, 255);
            var shield = new Color32(180, 180, 190, 255);
            var shieldLight = new Color32(220, 220, 230, 255);
            var shieldEdge = new Color32(120, 120, 130, 255);
            var shieldEmblem = new Color32(200, 170, 50, 255);
            var pants = new Color32(60, 40, 40, 255);
            var boots = new Color32(30, 25, 20, 255);
            var sword = new Color32(200, 200, 210, 255);
            var swordHilt = new Color32(120, 90, 50, 255);
            var outline = new Color32(20, 15, 15, 255);

            // 头盔（全覆盖，只露脸）
            FillRect(px, w, 8, 24, 15, 30, hair);
            FillRect(px, w, 7, 25, 16, 30, armor);
            FillRect(px, w, 8, 26, 14, 28, skin);
            // 头盔顶饰
            FillRect(px, w, 11, 30, 12, 31, armorLight);
            // 眼睛
            px[26 * w + 10] = outline;
            px[26 * w + 13] = outline;
            // 胸甲（红色重甲）
            FillRect(px, w, 8, 14, 15, 20, armor);
            FillRect(px, w, 8, 14, 8, 20, armorShadow);
            FillRect(px, w, 14, 14, 15, 20, armorLight);
            // 胸甲徽章
            FillRect(px, w, 10, 17, 13, 19, armorLight);
            px[18 * w + 11] = shieldEmblem;
            px[18 * w + 12] = shieldEmblem;
            // 肩甲
            FillRect(px, w, 6, 18, 8, 20, armor);
            FillRect(px, w, 15, 18, 17, 20, armor);
            // 大盾牌（左侧，举起来挡住半身）
            FillRect(px, w, 1, 12, 8, 22, shield);
            FillRect(px, w, 2, 13, 7, 21, shieldLight);
            // 盾牌边框
            FillRect(px, w, 1, 12, 1, 22, shieldEdge);
            FillRect(px, w, 8, 12, 8, 22, shieldEdge);
            FillRect(px, w, 1, 12, 8, 12, shieldEdge);
            FillRect(px, w, 1, 22, 8, 22, shieldEdge);
            // 盾牌十字徽章
            FillRect(px, w, 4, 15, 5, 19, shieldEmblem);
            FillRect(px, w, 3, 16, 6, 18, shieldEmblem);
            // 腿
            FillRect(px, w, 9, 8, 11, 13, pants);
            FillRect(px, w, 12, 8, 14, 13, pants);
            // 靴子（重靴）
            FillRect(px, w, 8, 4, 12, 8, boots);
            FillRect(px, w, 11, 4, 15, 8, boots);
            // 短剑（右手持剑，向下）
            FillRect(px, w, 16, 14, 17, 22, sword);
            FillRect(px, w, 15, 22, 18, 23, swordHilt);
            px[20 * w + 16] = swordHilt;

            DrawOutline(px, w, h, outline);
            SaveSprite("player_bruno", tex, px);
        }

        private static void MakePlayerMara()
        {
            // Mara - 狙击手：绿色迷彩服 + 长狙击枪 + 瞄准镜（半蹲瞄准姿势）
            int w = 24, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var skin = new Color32(215, 175, 135, 255);
            var hair = new Color32(130, 100, 60, 255);
            var suit = new Color32(70, 110, 70, 255);
            var suitLight = new Color32(95, 140, 90, 255);
            var suitShadow = new Color32(50, 85, 50, 255);
            var camo1 = new Color32(90, 120, 60, 255);
            var camo2 = new Color32(60, 90, 50, 255);
            var pants = new Color32(55, 65, 55, 255);
            var boots = new Color32(40, 35, 30, 255);
            var rifle = new Color32(50, 50, 55, 255);
            var rifleDetail = new Color32(80, 80, 85, 255);
            var scope = new Color32(40, 30, 20, 255);
            var scopeLens = new Color32(120, 180, 220, 255);
            var outline = new Color32(15, 20, 15, 255);

            // 头发（马尾）
            FillRect(px, w, 8, 24, 15, 30, hair);
            FillRect(px, w, 9, 25, 14, 28, skin);
            // 马尾辫（向后飘）
            FillRect(px, w, 14, 26, 16, 30, hair);
            FillRect(px, w, 16, 27, 17, 29, hair);
            // 眼睛（眯眼瞄准）
            px[26 * w + 10] = outline;
            px[26 * w + 13] = outline;
            FillRect(px, w, 9, 26, 9, 26, suitShadow);
            FillRect(px, w, 14, 26, 14, 26, suitShadow);
            // 迷彩服身体（半蹲姿势，身体略低）
            FillRect(px, w, 8, 12, 15, 18, suit);
            FillRect(px, w, 8, 12, 8, 18, suitShadow);
            FillRect(px, w, 14, 12, 15, 18, suitLight);
            // 迷彩斑点
            px[15 * w + 10] = camo1;
            px[15 * w + 13] = camo2;
            px[16 * w + 9] = camo2;
            px[16 * w + 14] = camo1;
            px[14 * w + 11] = camo1;
            // 手臂（向前伸，托枪）
            FillRect(px, w, 6, 13, 8, 16, suit);
            FillRect(px, w, 15, 13, 17, 16, suit);
            // 腿（半蹲，膝盖弯曲）
            FillRect(px, w, 8, 6, 11, 12, pants);
            FillRect(px, w, 12, 6, 15, 12, pants);
            FillRect(px, w, 7, 4, 11, 7, pants);
            FillRect(px, w, 11, 4, 15, 7, pants);
            // 靴子
            FillRect(px, w, 7, 3, 11, 5, boots);
            FillRect(px, w, 11, 3, 15, 5, boots);
            // 长狙击枪（水平向前，很长）
            FillRect(px, w, 16, 14, 23, 15, rifle);
            FillRect(px, w, 15, 15, 16, 15, rifle);
            // 瞄准镜（枪身上方）
            FillRect(px, w, 18, 16, 21, 17, scope);
            FillRect(px, w, 19, 17, 20, 17, scopeLens);
            // 枪口
            FillRect(px, w, 23, 14, 23, 15, rifleDetail);
            // 枪托（向后）
            FillRect(px, w, 14, 14, 15, 16, rifle);

            DrawOutline(px, w, h, outline);
            SaveSprite("player_mara", tex, px);
        }

        private static void MakePlayerNiko()
        {
            // Niko - 突击手：紫色风衣 + 护目镜 + 扳手（举工具姿势）
            int w = 24, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var skin = new Color32(200, 165, 130, 255);
            var hair = new Color32(60, 50, 70, 255);
            var coat = new Color32(110, 70, 130, 255);
            var coatLight = new Color32(140, 95, 160, 255);
            var coatShadow = new Color32(80, 50, 100, 255);
            var goggles = new Color32(180, 220, 240, 255);
            var gogglesFrame = new Color32(40, 40, 50, 255);
            var pants = new Color32(55, 45, 65, 255);
            var boots = new Color32(35, 30, 35, 255);
            var tool = new Color32(180, 180, 80, 255);
            var toolHandle = new Color32(120, 80, 40, 255);
            var backpack = new Color32(90, 60, 110, 255);
            var outline = new Color32(20, 15, 25, 255);

            // 头发（蓬乱）
            FillRect(px, w, 8, 24, 15, 30, hair);
            FillRect(px, w, 7, 26, 8, 29, hair);
            FillRect(px, w, 15, 26, 16, 29, hair);
            // 脸
            FillRect(px, w, 9, 25, 14, 28, skin);
            // 护目镜（覆盖眼睛位置）
            FillRect(px, w, 9, 26, 14, 27, goggles);
            FillRect(px, w, 9, 26, 9, 27, gogglesFrame);
            FillRect(px, w, 14, 26, 14, 27, gogglesFrame);
            px[11 * w + 26] = gogglesFrame;
            // 嘴
            px[25 * w + 11] = outline;
            // 紫色风衣身体
            FillRect(px, w, 8, 14, 15, 20, coat);
            FillRect(px, w, 8, 14, 8, 20, coatShadow);
            FillRect(px, w, 14, 14, 15, 20, coatLight);
            // 风衣腰带
            FillRect(px, w, 8, 16, 15, 17, coatShadow);
            // 工具腰带（多个工具）
            px[16 * w + 10] = tool;
            px[16 * w + 13] = tool;
            // 背包（背后鼓起）
            FillRect(px, w, 5, 15, 8, 19, backpack);
            FillRect(px, w, 5, 15, 5, 19, coatShadow);
            // 手臂（右手举工具向上）
            FillRect(px, w, 15, 15, 17, 20, coat);
            FillRect(px, w, 6, 15, 8, 18, coat);
            // 腿
            FillRect(px, w, 9, 8, 11, 13, pants);
            FillRect(px, w, 12, 8, 14, 13, pants);
            // 靴子
            FillRect(px, w, 9, 5, 12, 8, boots);
            FillRect(px, w, 12, 5, 15, 8, boots);
            // 扳手（右手举高）
            FillRect(px, w, 17, 20, 19, 24, toolHandle);
            FillRect(px, w, 16, 24, 20, 25, tool);
            FillRect(px, w, 16, 23, 17, 24, tool);
            FillRect(px, w, 19, 23, 20, 24, tool);

            DrawOutline(px, w, h, outline);
            SaveSprite("player_niko", tex, px);
        }

        private static void GenerateEnemySprites()
        {
            MakeEnemyRifle();
            MakeEnemyShield();
            MakeEnemyDrone();
            MakeEnemyGrenadier();
            MakeEnemyFlamer();
            MakeMiniBossWalker();
            MakeTurretBoss();
        }

        private static void MakeEnemyRifle()
        {
            // 步枪兵：灰绿色军服 + 钢盔 + 步枪（标准站立瞄准）
            int w = 20, h = 28;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var skin = new Color32(180, 145, 110, 255);
            var helmet = new Color32(90, 85, 75, 255);
            var helmetLight = new Color32(120, 115, 100, 255);
            var uniform = new Color32(110, 100, 80, 255);
            var uniformShadow = new Color32(80, 72, 58, 255);
            var uniformLight = new Color32(140, 128, 100, 255);
            var pants = new Color32(70, 65, 55, 255);
            var boots = new Color32(30, 28, 25, 255);
            var gun = new Color32(45, 45, 50, 255);
            var gunDetail = new Color32(80, 80, 85, 255);
            var outline = new Color32(15, 15, 15, 255);

            // 钢盔（圆形）
            FillRect(px, w, 6, 21, 13, 26, helmet);
            FillRect(px, w, 7, 22, 12, 26, helmetLight);
            // 钢盔带
            FillRect(px, w, 6, 20, 13, 21, helmet);
            // 脸
            FillRect(px, w, 7, 19, 12, 22, skin);
            // 眼睛
            px[20 * w + 8] = outline;
            px[20 * w + 11] = outline;
            // 军服身体
            FillRect(px, w, 6, 12, 13, 18, uniform);
            FillRect(px, w, 6, 12, 6, 18, uniformShadow);
            FillRect(px, w, 12, 12, 13, 18, uniformLight);
            // 弹带
            FillRect(px, w, 6, 14, 13, 15, uniformShadow);
            px[14 * w + 8] = uniformLight;
            px[14 * w + 11] = uniformLight;
            // 手臂
            FillRect(px, w, 4, 13, 6, 17, uniform);
            FillRect(px, w, 13, 13, 15, 17, uniform);
            // 腿
            FillRect(px, w, 7, 6, 9, 11, pants);
            FillRect(px, w, 10, 6, 12, 11, pants);
            // 靴子
            FillRect(px, w, 7, 3, 10, 6, boots);
            FillRect(px, w, 10, 3, 13, 6, boots);
            // 步枪（水平向前）
            FillRect(px, w, 14, 15, 19, 16, gun);
            FillRect(px, w, 13, 16, 14, 16, gun);
            // 枪口
            FillRect(px, w, 19, 15, 19, 16, gunDetail);
            // 枪托
            FillRect(px, w, 12, 15, 13, 17, gun);

            DrawOutline(px, w, h, outline);
            SaveSprite("enemy_rifle", tex, px);
        }

        private static void MakeEnemyShield()
        {
            // 盾兵：深绿色重甲 + 巨型塔盾 + 短矛（举盾推进姿势）
            int w = 22, h = 30;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var skin = new Color32(175, 140, 105, 255);
            var helmet = new Color32(70, 80, 65, 255);
            var helmetLight = new Color32(100, 110, 90, 255);
            var armor = new Color32(85, 95, 75, 255);
            var armorLight = new Color32(115, 125, 100, 255);
            var armorShadow = new Color32(60, 68, 52, 255);
            var shield = new Color32(130, 125, 110, 255);
            var shieldLight = new Color32(170, 165, 150, 255);
            var shieldEdge = new Color32(80, 75, 65, 255);
            var shieldSpike = new Color32(180, 180, 190, 255);
            var pants = new Color32(55, 60, 50, 255);
            var boots = new Color32(25, 25, 20, 255);
            var spear = new Color32(160, 130, 80, 255);
            var spearTip = new Color32(200, 200, 210, 255);
            var outline = new Color32(10, 15, 10, 255);

            // 全盔（只露眼睛）
            FillRect(px, w, 7, 22, 14, 28, helmet);
            FillRect(px, w, 8, 23, 13, 28, helmetLight);
            FillRect(px, w, 8, 20, 13, 23, skin);
            // 眼缝
            FillRect(px, w, 9, 21, 12, 21, outline);
            // 重甲身体
            FillRect(px, w, 7, 12, 14, 19, armor);
            FillRect(px, w, 7, 12, 7, 19, armorShadow);
            FillRect(px, w, 13, 12, 14, 19, armorLight);
            // 肩甲（突出）
            FillRect(px, w, 5, 17, 7, 19, armor);
            FillRect(px, w, 14, 17, 16, 19, armor);
            // 巨型塔盾（左侧，几乎挡住全身）
            FillRect(px, w, 0, 10, 7, 22, shield);
            FillRect(px, w, 1, 11, 6, 21, shieldLight);
            // 盾牌边框（金属）
            FillRect(px, w, 0, 10, 0, 22, shieldEdge);
            FillRect(px, w, 7, 10, 7, 22, shieldEdge);
            FillRect(px, w, 0, 10, 7, 10, shieldEdge);
            FillRect(px, w, 0, 22, 7, 22, shieldEdge);
            // 盾牌中心尖刺
            FillRect(px, w, 3, 15, 4, 17, shieldSpike);
            // 盾牌铆钉
            px[13 * w + 2] = shieldSpike;
            px[13 * w + 5] = shieldSpike;
            px[19 * w + 2] = shieldSpike;
            px[19 * w + 5] = shieldSpike;
            // 腿
            FillRect(px, w, 8, 6, 10, 11, pants);
            FillRect(px, w, 11, 6, 13, 11, pants);
            // 重靴
            FillRect(px, w, 7, 3, 11, 6, boots);
            FillRect(px, w, 10, 3, 14, 6, boots);
            // 短矛（右手持，向前指）
            FillRect(px, w, 15, 14, 20, 15, spear);
            FillRect(px, w, 20, 14, 21, 15, spearTip);

            DrawOutline(px, w, h, outline);
            SaveSprite("enemy_shield", tex, px);
        }

        private static void MakeEnemyDrone()
        {
            // 无人机：飞行单位 + 四旋翼 + 红色单眼 + 机枪（空中悬浮）
            int w = 24, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var body = new Color32(70, 80, 95, 255);
            var bodyLight = new Color32(100, 115, 135, 255);
            var bodyShadow = new Color32(45, 55, 70, 255);
            var rotor = new Color32(200, 200, 210, 255);
            var rotorBlur = new Color32(150, 150, 160, 255);
            var eye = new Color32(255, 60, 40, 255);
            var eyeGlow = new Color32(255, 120, 80, 255);
            var eyeCore = new Color32(255, 200, 150, 255);
            var gun = new Color32(40, 40, 45, 255);
            var gunDetail = new Color32(80, 80, 85, 255);
            var antenna = new Color32(180, 180, 80, 255);
            var outline = new Color32(15, 20, 25, 255);

            // 四旋翼（四个角）
            FillRect(px, w, 0, 10, 6, 12, rotor);
            FillRect(px, w, 17, 10, 23, 12, rotor);
            // 旋翼模糊效果
            FillRect(px, w, 1, 11, 5, 11, rotorBlur);
            FillRect(px, w, 18, 11, 22, 11, rotorBlur);
            // 旋翼中心
            px[11 * w + 3] = bodyShadow;
            px[11 * w + 20] = bodyShadow;
            // 机身（中央椭圆）
            FillRect(px, w, 6, 4, 17, 13, body);
            FillRect(px, w, 6, 4, 6, 13, bodyShadow);
            FillRect(px, w, 15, 4, 17, 13, bodyLight);
            // 机身细节
            FillRect(px, w, 7, 5, 16, 12, body);
            // 红色单眼（大眼）
            FillRect(px, w, 9, 7, 14, 10, eye);
            FillRect(px, w, 10, 8, 13, 9, eyeGlow);
            px[11 * w + 11] = eyeCore;
            px[11 * w + 12] = eyeCore;
            // 天线（顶部）
            FillRect(px, w, 11, 13, 12, 15, antenna);
            px[12 * w + 11] = bodyLight;
            // 机枪（底部向下）
            FillRect(px, w, 10, 1, 13, 4, gun);
            FillRect(px, w, 11, 0, 12, 1, gunDetail);
            // 机枪细节
            px[3 * w + 11] = gunDetail;
            px[3 * w + 12] = gunDetail;
            // 机身支架
            px[4 * w + 14] = bodyLight;
            px[13 * w + 14] = bodyLight;

            DrawOutline(px, w, h, outline);
            SaveSprite("enemy_drone", tex, px);
        }

        private static void MakeEnemyGrenadier()
        {
            // 掷弹兵：棕色军服 + 棕色头盔 + 榴弹发射器 + 手雷腰带（重型武器姿势）
            int w = 22, h = 30;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var skin = new Color32(170, 135, 100, 255);
            var helmet = new Color32(100, 70, 50, 255);
            var helmetLight = new Color32(130, 95, 70, 255);
            var uniform = new Color32(120, 80, 55, 255);
            var uniformLight = new Color32(150, 105, 75, 255);
            var uniformShadow = new Color32(90, 60, 40, 255);
            var pants = new Color32(75, 55, 40, 255);
            var boots = new Color32(30, 25, 20, 255);
            var launcher = new Color32(55, 55, 60, 255);
            var launcherDetail = new Color32(90, 90, 95, 255);
            var grenade = new Color32(180, 160, 60, 255);
            var grenadeDark = new Color32(120, 100, 40, 255);
            var outline = new Color32(15, 10, 10, 255);

            // 头盔（宽边）
            FillRect(px, w, 6, 22, 15, 27, helmet);
            FillRect(px, w, 7, 23, 14, 27, helmetLight);
            FillRect(px, w, 5, 22, 6, 24, helmet);
            // 脸
            FillRect(px, w, 8, 20, 13, 23, skin);
            // 眼睛
            px[21 * w + 9] = outline;
            px[21 * w + 12] = outline;
            // 络腮胡
            FillRect(px, w, 8, 19, 13, 20, uniformShadow);
            // 军服身体（魁梧）
            FillRect(px, w, 6, 12, 15, 19, uniform);
            FillRect(px, w, 6, 12, 6, 19, uniformShadow);
            FillRect(px, w, 14, 12, 15, 19, uniformLight);
            // 手雷腰带（多个手雷）
            FillRect(px, w, 7, 14, 14, 15, uniformShadow);
            px[14 * w + 8] = grenade;
            px[14 * w + 10] = grenade;
            px[14 * w + 12] = grenade;
            px[14 * w + 14] = grenade;
            // 手雷细节
            px[15 * w + 8] = grenadeDark;
            px[15 * w + 10] = grenadeDark;
            px[15 * w + 12] = grenadeDark;
            px[15 * w + 14] = grenadeDark;
            // 手臂（粗壮）
            FillRect(px, w, 4, 13, 6, 17, uniform);
            FillRect(px, w, 15, 13, 17, 17, uniform);
            // 腿
            FillRect(px, w, 7, 6, 10, 11, pants);
            FillRect(px, w, 11, 6, 14, 11, pants);
            // 靴子
            FillRect(px, w, 7, 3, 11, 6, boots);
            FillRect(px, w, 11, 3, 15, 6, boots);
            // 榴弹发射器（粗大，向前）
            FillRect(px, w, 15, 15, 21, 17, launcher);
            FillRect(px, w, 14, 16, 15, 16, launcher);
            // 榴弹（枪口处）
            FillRect(px, w, 20, 15, 21, 17, grenade);
            // 枪管细节
            FillRect(px, w, 17, 18, 19, 19, launcherDetail);

            DrawOutline(px, w, h, outline);
            SaveSprite("enemy_grenadier", tex, px);
        }

        private static void MakeEnemyFlamer()
        {
            // 火焰兵：红色防火服 + 面罩 + 喷火器 + 燃料罐（喷火姿势）
            int w = 22, h = 30;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var suit = new Color32(140, 50, 40, 255);
            var suitLight = new Color32(175, 70, 55, 255);
            var suitShadow = new Color32(100, 35, 30, 255);
            var mask = new Color32(60, 60, 65, 255);
            var maskLens = new Color32(200, 180, 80, 255);
            var tank = new Color32(80, 80, 90, 255);
            var tankLight = new Color32(120, 120, 130, 255);
            var tankShadow = new Color32(50, 50, 60, 255);
            var flamethrower = new Color32(50, 50, 55, 255);
            var flamethrowerDetail = new Color32(90, 90, 95, 255);
            var flame = new Color32(255, 140, 40, 255);
            var flameCore = new Color32(255, 220, 100, 255);
            var pants = new Color32(80, 35, 30, 255);
            var boots = new Color32(30, 25, 20, 255);
            var outline = new Color32(15, 10, 10, 255);

            // 防火头罩（全覆盖）
            FillRect(px, w, 7, 22, 14, 28, suit);
            FillRect(px, w, 8, 23, 13, 28, suitLight);
            // 面罩（黑色）
            FillRect(px, w, 8, 20, 13, 23, mask);
            // 面罩镜片（黄色发光）
            FillRect(px, w, 9, 21, 10, 22, maskLens);
            FillRect(px, w, 11, 21, 12, 22, maskLens);
            // 防火服身体
            FillRect(px, w, 6, 12, 15, 19, suit);
            FillRect(px, w, 6, 12, 6, 19, suitShadow);
            FillRect(px, w, 14, 12, 15, 19, suitLight);
            // 燃料罐（背后）
            FillRect(px, w, 3, 13, 6, 19, tank);
            FillRect(px, w, 3, 13, 3, 19, tankShadow);
            FillRect(px, w, 5, 13, 6, 19, tankLight);
            // 燃料管
            FillRect(px, w, 6, 15, 7, 16, tank);
            // 手臂
            FillRect(px, w, 4, 13, 6, 17, suit);
            FillRect(px, w, 15, 13, 17, 17, suit);
            // 腿
            FillRect(px, w, 7, 6, 10, 11, pants);
            FillRect(px, w, 11, 6, 14, 11, pants);
            // 靴子
            FillRect(px, w, 7, 3, 11, 6, boots);
            FillRect(px, w, 11, 3, 15, 6, boots);
            // 喷火器（粗大）
            FillRect(px, w, 15, 15, 20, 17, flamethrower);
            FillRect(px, w, 14, 16, 15, 16, flamethrower);
            // 喷火器细节
            FillRect(px, w, 16, 18, 18, 19, flamethrowerDetail);
            // 火焰（枪口喷出）
            FillRect(px, w, 20, 14, 21, 18, flame);
            px[15 * w + 21] = flameCore;
            px[16 * w + 21] = flameCore;
            px[17 * w + 21] = flameCore;

            DrawOutline(px, w, h, outline);
            SaveSprite("enemy_flamer", tex, px);
        }

        private static void MakeMiniBossWalker()
        {
            // 步行机甲 Boss：双足机甲 + 双臂加农炮 + 红色独眼 + 导弹发射器（威慑姿态）
            int w = 48, h = 56;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var body = new Color32(65, 60, 70, 255);
            var bodyLight = new Color32(95, 90, 105, 255);
            var bodyShadow = new Color32(40, 38, 48, 255);
            var metal = new Color32(110, 105, 115, 255);
            var metalLight = new Color32(150, 145, 155, 255);
            var metalDark = new Color32(70, 65, 75, 255);
            var eye = new Color32(255, 50, 30, 255);
            var eyeGlow = new Color32(255, 130, 90, 255);
            var eyeCore = new Color32(255, 220, 150, 255);
            var gun = new Color32(35, 35, 40, 255);
            var gunDetail = new Color32(70, 70, 75, 255);
            var missile = new Color32(180, 160, 60, 255);
            var joint = new Color32(50, 45, 55, 255);
            var warning = new Color32(255, 200, 50, 255);
            var outline = new Color32(10, 10, 15, 255);

            // 头部（驾驶舱）
            FillRect(px, w, 14, 42, 33, 54, body);
            FillRect(px, w, 15, 43, 32, 53, bodyLight);
            FillRect(px, w, 14, 42, 14, 54, bodyShadow);
            // 红色独眼（大）
            FillRect(px, w, 17, 46, 30, 50, eye);
            FillRect(px, w, 18, 47, 29, 49, eyeGlow);
            FillRect(px, w, 21, 47, 26, 49, eyeCore);
            // 警示灯（头顶）
            FillRect(px, w, 23, 54, 25, 56, warning);
            // 天线
            FillRect(px, w, 23, 56, 24, 58, metal);
            // 躯干（厚重装甲）
            FillRect(px, w, 10, 24, 37, 42, body);
            FillRect(px, w, 10, 24, 10, 42, bodyShadow);
            FillRect(px, w, 33, 24, 37, 42, bodyLight);
            // 胸口装甲板
            FillRect(px, w, 14, 28, 33, 38, metal);
            FillRect(px, w, 15, 29, 32, 37, metalLight);
            // 装甲铆钉
            for (int x = 12; x <= 35; x += 4)
            {
                px[26 * w + x] = joint;
                px[40 * w + x] = joint;
            }
            // 警示条纹
            FillRect(px, w, 14, 30, 33, 31, warning);
            FillRect(px, w, 14, 34, 33, 35, warning);
            // 左臂（加农炮）
            FillRect(px, w, 2, 26, 10, 36, body);
            FillRect(px, w, 2, 26, 2, 36, bodyShadow);
            // 加农炮管（粗）
            FillRect(px, w, 0, 28, 2, 34, gun);
            FillRect(px, w, 0, 29, 1, 33, gunDetail);
            // 右臂（导弹发射器）
            FillRect(px, w, 37, 26, 45, 36, body);
            FillRect(px, w, 44, 26, 45, 36, bodyShadow);
            FillRect(px, w, 45, 27, 48, 35, gun);
            // 导弹（4发）
            for (int y = 28; y <= 34; y += 2)
            {
                px[y * w + 46] = missile;
                px[y * w + 47] = missile;
            }
            // 双腿（机甲足）
            FillRect(px, w, 14, 8, 22, 24, body);
            FillRect(px, w, 14, 8, 14, 24, bodyShadow);
            FillRect(px, w, 21, 8, 22, 24, bodyLight);
            FillRect(px, w, 25, 8, 33, 24, body);
            FillRect(px, w, 25, 8, 25, 24, bodyShadow);
            FillRect(px, w, 32, 8, 33, 24, bodyLight);
            // 膝关节
            FillRect(px, w, 16, 14, 20, 16, joint);
            FillRect(px, w, 27, 14, 31, 16, joint);
            // 脚（宽大）
            FillRect(px, w, 12, 4, 23, 8, metal);
            FillRect(px, w, 23, 4, 34, 8, metal);
            FillRect(px, w, 12, 4, 12, 8, bodyShadow);
            FillRect(px, w, 23, 4, 23, 8, bodyShadow);
            // 脚趾细节
            px[6 * w + 14] = metalLight;
            px[6 * w + 18] = metalLight;
            px[6 * w + 25] = metalLight;
            px[6 * w + 29] = metalLight;

            DrawOutline(px, w, h, outline);
            SaveSprite("miniboss_walker", tex, px);
        }

        private static void MakeTurretBoss()
        {
            // 炮台 Boss：固定防御工事 + 三炮管 + 红色核心 + 沙袋防护（防御姿态）
            int w = 32, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var body = new Color32(85, 80, 75, 255);
            var bodyLight = new Color32(115, 108, 98, 255);
            var bodyShadow = new Color32(55, 52, 48, 255);
            var metal = new Color32(130, 125, 115, 255);
            var metalLight = new Color32(165, 158, 145, 255);
            var metalDark = new Color32(80, 75, 70, 255);
            var core = new Color32(220, 60, 40, 255);
            var coreGlow = new Color32(255, 120, 60, 255);
            var coreBright = new Color32(255, 200, 120, 255);
            var gun = new Color32(50, 48, 45, 255);
            var gunDetail = new Color32(90, 85, 80, 255);
            var sandbag = new Color32(160, 140, 100, 255);
            var sandbagShadow = new Color32(110, 95, 70, 255);
            var warning = new Color32(255, 200, 50, 255);
            var outline = new Color32(20, 18, 15, 255);

            // 沙袋防护（底座两侧）
            FillRect(px, w, 0, 0, 6, 4, sandbag);
            FillRect(px, w, 0, 0, 3, 3, sandbagShadow);
            FillRect(px, w, 25, 0, 31, 4, sandbag);
            FillRect(px, w, 28, 0, 31, 3, sandbagShadow);
            // 沙袋细节
            px[2 * w + 2] = sandbagShadow;
            px[2 * w + 4] = sandbagShadow;
            px[2 * w + 27] = sandbagShadow;
            px[2 * w + 29] = sandbagShadow;

            // 混凝土底座
            FillRect(px, w, 6, 2, 25, 8, metal);
            FillRect(px, w, 6, 2, 6, 8, bodyShadow);
            FillRect(px, w, 24, 2, 25, 8, metalLight);
            FillRect(px, w, 8, 3, 23, 7, body);
            FillRect(px, w, 10, 4, 21, 6, bodyShadow);
            // 底座铆钉
            px[5 * w + 9] = metalDark;
            px[5 * w + 15] = metalDark;
            px[5 * w + 21] = metalDark;

            // 中段炮塔（旋转部分）
            FillRect(px, w, 8, 8, 23, 22, body);
            FillRect(px, w, 8, 8, 8, 22, bodyShadow);
            FillRect(px, w, 22, 8, 23, 22, bodyLight);
            FillRect(px, w, 10, 10, 21, 20, bodyShadow);
            // 装甲板
            FillRect(px, w, 10, 10, 21, 12, metal);
            FillRect(px, w, 10, 18, 21, 20, metal);

            // 核心（红色发光眼，大）
            FillRect(px, w, 12, 13, 19, 17, core);
            FillRect(px, w, 13, 14, 18, 16, coreGlow);
            FillRect(px, w, 14, 15, 17, 15, coreBright);
            // 警示条纹（核心周围）
            FillRect(px, w, 11, 13, 12, 17, warning);
            FillRect(px, w, 19, 13, 20, 17, warning);

            // 左炮管（粗大）
            FillRect(px, w, 0, 13, 8, 18, gun);
            FillRect(px, w, 0, 14, 7, 17, gunDetail);
            // 炮口
            FillRect(px, w, 0, 13, 0, 18, metalDark);
            px[15 * w + 0] = metalLight;

            // 右炮管（粗大）
            FillRect(px, w, 23, 13, 31, 18, gun);
            FillRect(px, w, 24, 14, 31, 17, gunDetail);
            // 炮口
            FillRect(px, w, 31, 13, 31, 18, metalDark);
            px[15 * w + 31] = metalLight;

            // 顶部装甲（斜面）
            FillRect(px, w, 10, 22, 21, 26, metal);
            FillRect(px, w, 10, 22, 10, 26, bodyShadow);
            FillRect(px, w, 20, 22, 21, 26, metalLight);
            FillRect(px, w, 12, 23, 19, 25, body);

            // 顶炮（向上）
            FillRect(px, w, 14, 26, 17, 31, gun);
            FillRect(px, w, 15, 27, 16, 30, gunDetail);
            // 顶炮炮口
            FillRect(px, w, 14, 31, 17, 31, metalDark);

            // 装甲板铆钉
            for (int x = 10; x <= 21; x += 3)
            {
                px[21 * w + x] = metalLight;
                px[9 * w + x] = metalLight;
            }

            DrawOutline(px, w, h, outline);
            SaveSprite("turret_boss", tex, px);
        }

        // ==================== 地面 ====================

        private static void GenerateGroundSprites()
        {
            MakeGroundBeach();
            MakeGroundVillage();
            MakeGroundTrench();
            MakeGroundFactory();
            MakeGroundCity();
            MakeGroundIndustrial();
        }

        private static void MakeGroundBeach()
        {
            int w = 32, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            var sand = new Color32(200, 180, 130, 255);
            var sandLight = new Color32(220, 200, 150, 255);
            var sandShadow = new Color32(170, 150, 105, 255);
            var grass = new Color32(110, 140, 70, 255);
            var grassDark = new Color32(85, 115, 55, 255);
            var stone = new Color32(140, 135, 125, 255);

            for (int x = 0; x < w; x++)
            {
                FillRect(px, w, x, 28, x, 31, grass);
                if (x % 3 == 0) px[31 * w + x] = grassDark;
            }
            for (int y = 0; y < 28; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = sand;
            var rng = new System.Random(42);
            for (int i = 0; i < 60; i++)
            {
                int x = rng.Next(w), y = rng.Next(28);
                px[y * w + x] = rng.Next(2) == 0 ? sandLight : sandShadow;
            }
            FillRect(px, w, 5, 15, 8, 18, stone);
            FillRect(px, w, 6, 16, 7, 17, sandShadow);
            FillRect(px, w, 22, 8, 25, 11, stone);
            FillRect(px, w, 23, 9, 24, 10, sandShadow);
            px[20 * w + 14] = sandLight;
            px[21 * w + 14] = sandLight;
            px[20 * w + 15] = sandShadow;

            SaveSprite("ground_beach", tex, px);
        }

        private static void MakeGroundVillage()
        {
            int w = 32, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            var wood = new Color32(130, 90, 55, 255);
            var woodLight = new Color32(160, 115, 75, 255);
            var woodDark = new Color32(100, 68, 40, 255);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = wood;
            for (int y = 0; y < h; y += 8)
                FillRect(px, w, 0, y, w - 1, y, woodDark);
            for (int x = 0; x < w; x += 16)
                FillRect(px, w, x, 0, x, h - 1, woodDark);
            var rng = new System.Random(7);
            for (int i = 0; i < 40; i++)
            {
                int x = rng.Next(w), y = rng.Next(h);
                px[y * w + x] = rng.Next(2) == 0 ? woodLight : woodDark;
            }
            for (int y = 1; y < h; y += 8)
                FillRect(px, w, 0, y, w - 1, y, woodLight);
            for (int bx = 4; bx < w; bx += 16)
                for (int by = 4; by < h; by += 8)
                {
                    px[by * w + bx] = woodDark;
                    px[(by + 1) * w + bx] = woodDark;
                }

            SaveSprite("ground_village", tex, px);
        }

        private static void MakeGroundTrench()
        {
            int w = 32, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            var mud = new Color32(75, 55, 35, 255);
            var mudLight = new Color32(95, 72, 48, 255);
            var mudDark = new Color32(55, 40, 25, 255);
            var water = new Color32(50, 60, 75, 255);
            var waterLight = new Color32(70, 85, 100, 255);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = mud;
            var rng = new System.Random(99);
            for (int i = 0; i < 80; i++)
            {
                int x = rng.Next(w), y = rng.Next(h);
                px[y * w + x] = rng.Next(2) == 0 ? mudLight : mudDark;
            }
            FillRect(px, w, 8, 10, 16, 14, water);
            FillRect(px, w, 9, 11, 15, 13, waterLight);
            FillRect(px, w, 20, 20, 27, 24, water);
            FillRect(px, w, 21, 21, 26, 23, waterLight);
            px[18 * w + 5] = mudDark;
            px[18 * w + 6] = mudDark;
            px[17 * w + 5] = mudDark;
            px[22 * w + 24] = mudDark;
            px[22 * w + 25] = mudDark;

            SaveSprite("ground_trench", tex, px);
        }

        private static void MakeGroundFactory()
        {
            int w = 32, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            var metal = new Color32(90, 95, 105, 255);
            var metalLight = new Color32(120, 125, 135, 255);
            var metalDark = new Color32(65, 70, 80, 255);
            var rust = new Color32(140, 80, 45, 255);
            var line = new Color32(40, 45, 55, 255);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = metal;
            for (int y = 0; y < h; y += 16)
                FillRect(px, w, 0, y, w - 1, y, line);
            for (int x = 0; x < w; x += 16)
                FillRect(px, w, x, 0, x, h - 1, line);
            for (int y = 1; y < h; y += 16)
                FillRect(px, w, 0, y, w - 1, y, metalLight);
            for (int x = 1; x < w; x += 16)
                FillRect(px, w, x, 0, x, h - 1, metalLight);
            for (int bx = 3; bx < w; bx += 16)
                for (int by = 3; by < h; by += 16)
                {
                    px[by * w + bx] = metalDark;
                    px[(by + 1) * w + bx] = metalDark;
                    px[by * w + bx + 1] = metalDark;
                }
            var rng = new System.Random(33);
            for (int i = 0; i < 25; i++)
            {
                int x = rng.Next(w), y = rng.Next(h);
                px[y * w + x] = rust;
            }

            SaveSprite("ground_factory", tex, px);
        }

        private static void MakeGroundCity()
        {
            int w = 32, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            var concrete = new Color32(130, 128, 122, 255);
            var concreteLight = new Color32(155, 152, 145, 255);
            var concreteDark = new Color32(100, 98, 92, 255);
            var crack = new Color32(60, 58, 55, 255);
            var rubble = new Color32(90, 80, 70, 255);
            var moss = new Color32(80, 100, 55, 255);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = concrete;
            // 破碎的路面板块
            FillRect(px, w, 0, 0, 15, 15, concreteLight);
            FillRect(px, w, 16, 16, 31, 31, concreteLight);
            FillRect(px, w, 0, 16, 15, 31, concreteDark);
            // 裂缝
            for (int i = 5; i < 20; i++)
                px[(i * 2 % h) * w + i] = crack;
            FillRect(px, w, 10, 5, 14, 6, crack);
            FillRect(px, w, 22, 18, 26, 22, crack);
            // 碎石
            var rng = new System.Random(77);
            for (int i = 0; i < 40; i++)
            {
                int x = rng.Next(w), y = rng.Next(h);
                px[y * w + x] = rubble;
            }
            // 苔藓
            for (int i = 0; i < 15; i++)
            {
                int x = rng.Next(w), y = rng.Next(h);
                px[y * w + x] = moss;
                if (x + 1 < w) px[y * w + x + 1] = moss;
            }

            SaveSprite("ground_city", tex, px);
        }

        private static void MakeGroundIndustrial()
        {
            int w = 32, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            var asphalt = new Color32(55, 55, 60, 255);
            var asphaltLight = new Color32(75, 75, 80, 255);
            var asphaltDark = new Color32(40, 40, 45, 255);
            var yellowLine = new Color32(220, 180, 40, 255);
            var oil = new Color32(25, 25, 30, 255);
            var rust = new Color32(120, 70, 40, 255);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = asphalt;
            // 黄色道路标线
            FillRect(px, w, 15, 0, 16, 31, yellowLine);
            FillRect(px, w, 15, 4, 16, 8, asphalt);
            FillRect(px, w, 15, 12, 16, 16, asphalt);
            FillRect(px, w, 15, 20, 16, 24, asphalt);
            FillRect(px, w, 15, 28, 16, 31, asphalt);
            // 油渍
            var rng = new System.Random(55);
            for (int i = 0; i < 8; i++)
            {
                int cx = rng.Next(w), cy = rng.Next(h);
                int r = rng.Next(2, 5);
                for (int dy = -r; dy <= r; dy++)
                    for (int dx = -r; dx <= r; dx++)
                        if (dx * dx + dy * dy <= r * r)
                        {
                            int x = cx + dx, y = cy + dy;
                            if (x >= 0 && x < w && y >= 0 && y < h)
                                px[y * w + x] = oil;
                        }
            }
            // 锈迹
            for (int i = 0; i < 20; i++)
            {
                int x = rng.Next(w), y = rng.Next(h);
                px[y * w + x] = rust;
            }
            // 明暗变化
            FillRect(px, w, 0, 0, 14, 31, asphaltLight);
            FillRect(px, w, 17, 0, 31, 31, asphaltDark);

            SaveSprite("ground_industrial", tex, px);
        }

        // ==================== 道具 ====================

        private static void GeneratePropSprites()
        {
            MakeCrate();
            MakeWeaponPickup();
            MakeCheckpointFlag();
            MakeHealthPickup();
            MakeExplosiveBarrel();
            MakeSandbag();
            MakeBarricade();
            MakeSpikes();
            MakeSignpost();
        }

        private static void MakeCrate()
        {
            int w = 20, h = 20;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var wood = new Color32(150, 105, 60, 255);
            var woodLight = new Color32(180, 130, 80, 255);
            var woodDark = new Color32(115, 78, 42, 255);
            var metal = new Color32(100, 95, 90, 255);
            var outline = new Color32(50, 30, 15, 255);

            FillRect(px, w, 2, 2, 17, 17, wood);
            FillRect(px, w, 2, 2, 2, 17, woodDark);
            FillRect(px, w, 17, 2, 17, 17, woodLight);
            FillRect(px, w, 2, 7, 17, 8, woodDark);
            FillRect(px, w, 2, 12, 17, 13, woodDark);
            FillRect(px, w, 1, 1, 4, 4, metal);
            FillRect(px, w, 15, 1, 18, 4, metal);
            FillRect(px, w, 1, 15, 4, 18, metal);
            FillRect(px, w, 15, 15, 18, 18, metal);
            for (int i = 2; i <= 17; i++)
            {
                px[i * w + i] = metal;
                px[(19 - i) * w + i] = metal;
            }
            px[5 * w + 8] = woodDark;
            px[10 * w + 12] = woodDark;
            px[15 * w + 6] = woodDark;

            DrawOutline(px, w, h, outline);
            SaveSprite("crate", tex, px);
        }

        private static void MakeCheckpointFlag()
        {
            int w = 16, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var pole = new Color32(120, 100, 70, 255);
            var poleLight = new Color32(150, 125, 90, 255);
            var flag = new Color32(230, 200, 60, 255);
            var flagLight = new Color32(255, 225, 90, 255);
            var flagShadow = new Color32(195, 165, 45, 255);
            var emblem = new Color32(180, 50, 50, 255);
            var outline = new Color32(40, 30, 15, 255);

            FillRect(px, w, 7, 0, 8, 31, pole);
            FillRect(px, w, 7, 0, 7, 31, poleLight);
            FillRect(px, w, 6, 30, 9, 31, poleLight);
            FillRect(px, w, 8, 22, 15, 28, flag);
            FillRect(px, w, 8, 22, 8, 28, flagShadow);
            FillRect(px, w, 14, 22, 15, 28, flagLight);
            px[27 * w + 9] = flag;
            px[27 * w + 13] = flag;
            px[26 * w + 10] = flagShadow;
            px[26 * w + 12] = flagShadow;
            FillRect(px, w, 10, 24, 12, 26, emblem);
            px[25 * w + 11] = flagLight;

            DrawOutline(px, w, h, outline);
            SaveSprite("checkpoint_flag", tex, px);
        }

        private static void MakeHealthPickup()
        {
            int w = 16, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var vial = new Color32(220, 230, 240, 200);
            var vialLight = new Color32(255, 255, 255, 220);
            var liquid = new Color32(80, 220, 100, 255);
            var liquidLight = new Color32(130, 255, 150, 255);
            var cap = new Color32(180, 180, 190, 255);
            var cross = new Color32(255, 255, 255, 255);
            var outline = new Color32(20, 40, 25, 255);

            FillRect(px, w, 5, 12, 10, 14, cap);
            FillRect(px, w, 4, 3, 11, 12, vial);
            FillRect(px, w, 4, 3, 4, 12, vialLight);
            FillRect(px, w, 5, 4, 10, 11, liquid);
            FillRect(px, w, 5, 4, 5, 11, liquidLight);
            FillRect(px, w, 7, 6, 8, 10, cross);
            FillRect(px, w, 6, 7, 9, 8, cross);
            px[9 * w + 5] = vialLight;
            px[7 * w + 5] = vialLight;

            DrawOutline(px, w, h, outline);
            SaveSprite("health_pickup", tex, px);
        }

        private static void MakeWeaponPickup()
        {
            int w = 24, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var gun = new Color32(70, 70, 80, 255);
            var gunLight = new Color32(110, 110, 120, 255);
            var gunDark = new Color32(40, 40, 50, 255);
            var grip = new Color32(90, 65, 45, 255);
            var glow = new Color32(255, 220, 80, 200);
            var outline = new Color32(15, 15, 20, 255);

            FillRect(px, w, 4, 7, 20, 10, gun);
            FillRect(px, w, 4, 7, 4, 10, gunDark);
            FillRect(px, w, 18, 7, 20, 10, gunLight);
            FillRect(px, w, 16, 8, 22, 9, gunDark);
            FillRect(px, w, 6, 3, 9, 7, grip);
            FillRect(px, w, 6, 3, 6, 7, gunDark);
            FillRect(px, w, 10, 10, 14, 12, gunDark);
            FillRect(px, w, 11, 11, 13, 11, glow);
            FillRect(px, w, 9, 4, 12, 7, gunDark);
            px[6 * w + 12] = glow;
            px[5 * w + 13] = glow;

            DrawOutline(px, w, h, outline);
            SaveSprite("weapon_pickup", tex, px);
        }

        private static void MakeExplosiveBarrel()
        {
            int w = 16, h = 24;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var barrel = new Color32(180, 120, 40, 255);
            var barrelLight = new Color32(215, 150, 60, 255);
            var barrelDark = new Color32(140, 90, 25, 255);
            var ring = new Color32(80, 55, 20, 255);
            var warning = new Color32(255, 220, 40, 255);
            var symbol = new Color32(30, 30, 30, 255);
            var outline = new Color32(20, 15, 5, 255);

            FillRect(px, w, 3, 3, 12, 20, barrel);
            FillRect(px, w, 3, 3, 3, 20, barrelDark);
            FillRect(px, w, 11, 3, 12, 20, barrelLight);
            FillRect(px, w, 3, 20, 12, 22, ring);
            FillRect(px, w, 4, 21, 11, 22, barrelLight);
            FillRect(px, w, 3, 1, 12, 3, ring);
            FillRect(px, w, 3, 8, 12, 9, ring);
            FillRect(px, w, 3, 15, 12, 16, ring);
            FillRect(px, w, 3, 10, 12, 13, warning);
            FillRect(px, w, 6, 11, 9, 12, symbol);
            FillRect(px, w, 7, 11, 8, 11, warning);
            px[11 * w + 7] = symbol;
            px[11 * w + 8] = symbol;

            DrawOutline(px, w, h, outline);
            SaveSprite("explosive_barrel", tex, px);
        }

        private static void MakeSandbag()
        {
            int w = 28, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var bag = new Color32(120, 100, 65, 255);
            var bagLight = new Color32(150, 128, 85, 255);
            var bagDark = new Color32(90, 75, 48, 255);
            var stitch = new Color32(70, 55, 35, 255);
            var outline = new Color32(35, 28, 18, 255);

            // 底层沙袋
            FillRect(px, w, 2, 2, 12, 7, bag);
            FillRect(px, w, 15, 2, 25, 7, bag);
            FillRect(px, w, 2, 2, 2, 7, bagDark);
            FillRect(px, w, 12, 2, 12, 7, bagDark);
            FillRect(px, w, 15, 2, 15, 7, bagDark);
            FillRect(px, w, 25, 2, 25, 7, bagDark);
            FillRect(px, w, 3, 3, 5, 5, bagLight);
            FillRect(px, w, 16, 3, 18, 5, bagLight);
            // 顶层沙袋
            FillRect(px, w, 8, 7, 18, 12, bag);
            FillRect(px, w, 8, 7, 8, 12, bagDark);
            FillRect(px, w, 18, 7, 18, 12, bagDark);
            FillRect(px, w, 9, 8, 11, 10, bagLight);
            // 缝线
            px[4 * w + 7] = stitch;
            px[5 * w + 7] = stitch;
            px[9 * w + 7] = stitch;
            px[10 * w + 7] = stitch;
            px[4 * w + 20] = stitch;
            px[5 * w + 20] = stitch;
            px[9 * w + 20] = stitch;
            px[10 * w + 20] = stitch;

            DrawOutline(px, w, h, outline);
            SaveSprite("sandbag", tex, px);
        }

        private static void MakeBarricade()
        {
            int w = 24, h = 24;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var metal = new Color32(85, 88, 95, 255);
            var metalLight = new Color32(115, 118, 125, 255);
            var metalDark = new Color32(55, 58, 65, 255);
            var rust = new Color32(130, 75, 40, 255);
            var warning = new Color32(220, 180, 40, 255);
            var outline = new Color32(20, 22, 28, 255);

            // 主体框架
            FillRect(px, w, 4, 4, 20, 6, metal);
            FillRect(px, w, 4, 4, 4, 6, metalDark);
            FillRect(px, w, 20, 4, 20, 6, metalLight);
            FillRect(px, w, 4, 10, 20, 12, metal);
            FillRect(px, w, 4, 10, 4, 12, metalDark);
            FillRect(px, w, 20, 10, 20, 12, metalLight);
            FillRect(px, w, 4, 16, 20, 18, metal);
            FillRect(px, w, 4, 16, 4, 18, metalDark);
            FillRect(px, w, 20, 16, 20, 18, metalLight);
            // 支撑柱
            FillRect(px, w, 2, 2, 4, 20, metalDark);
            FillRect(px, w, 20, 2, 22, 20, metalDark);
            // 警告条纹
            for (int x = 6; x < 20; x += 2)
            {
                px[5 * w + x] = warning;
                px[11 * w + x] = warning;
                px[17 * w + x] = warning;
            }
            // 锈迹
            var rng = new System.Random(11);
            for (int i = 0; i < 12; i++)
            {
                int x = rng.Next(4, 20), y = rng.Next(4, 18);
                px[y * w + x] = rust;
            }

            DrawOutline(px, w, h, outline);
            SaveSprite("barricade", tex, px);
        }

        private static void MakeSpikes()
        {
            int w = 32, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var spike = new Color32(180, 180, 185, 255);
            var spikeLight = new Color32(220, 220, 225, 255);
            var spikeDark = new Color32(130, 130, 135, 255);
            var baseCol = new Color32(60, 45, 30, 255);
            var outline = new Color32(30, 25, 18, 255);

            // 底座
            FillRect(px, w, 0, 0, 31, 3, baseCol);
            FillRect(px, w, 0, 3, 31, 4, spikeDark);
            // 尖刺
            for (int i = 0; i < 8; i++)
            {
                int x = i * 4 + 1;
                for (int h2 = 0; h2 < 12; h2++)
                {
                    int width = Mathf.Max(1, 3 - h2 / 4);
                    int cx = x + 1;
                    for (int dx = -width; dx <= width; dx++)
                    {
                        int px2 = cx + dx;
                        if (px2 >= 0 && px2 < w)
                            px[(h2 + 4) * w + px2] = h2 < 3 ? spikeLight : h2 > 9 ? spikeDark : spike;
                    }
                }
            }

            DrawOutline(px, w, h, outline);
            SaveSprite("spikes", tex, px);
        }

        private static void MakeSignpost()
        {
            int w = 16, h = 24;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var wood = new Color32(130, 90, 50, 255);
            var woodLight = new Color32(160, 115, 70, 255);
            var woodDark = new Color32(95, 65, 35, 255);
            var sign = new Color32(180, 150, 90, 255);
            var signDark = new Color32(140, 110, 65, 255);
            var text = new Color32(50, 35, 15, 255);
            var outline = new Color32(40, 25, 10, 255);

            // 柱子
            FillRect(px, w, 7, 0, 8, 24, wood);
            FillRect(px, w, 7, 0, 7, 24, woodDark);
            FillRect(px, w, 8, 0, 8, 24, woodLight);
            // 牌子
            FillRect(px, w, 2, 14, 13, 20, sign);
            FillRect(px, w, 2, 14, 2, 20, signDark);
            FillRect(px, w, 13, 14, 13, 20, signDark);
            FillRect(px, w, 2, 19, 13, 20, signDark);
            // 箭头文字
            FillRect(px, w, 5, 16, 9, 17, text);
            FillRect(px, w, 9, 16, 10, 17, text);
            FillRect(px, w, 10, 15, 11, 18, text);

            DrawOutline(px, w, h, outline);
            SaveSprite("signpost", tex, px);
        }

        // ==================== 弹药 ====================

        private static void GenerateProjectileSprites()
        {
            int w = 8, h = 4;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var core = new Color32(255, 240, 100, 255);
            var glow = new Color32(255, 200, 50, 200);
            var trail = new Color32(255, 160, 30, 150);
            FillRect(px, w, 5, 1, 7, 2, core);
            FillRect(px, w, 3, 1, 5, 2, glow);
            FillRect(px, w, 0, 1, 3, 2, trail);
            FillRect(px, w, 4, 0, 6, 3, glow);
            SaveSprite("bullet_player", tex, px);

            tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            px = new Color32[w * h];
            Clear(px, w, h);
            var eCore = new Color32(255, 80, 60, 255);
            var eGlow = new Color32(255, 50, 30, 200);
            var eTrail = new Color32(200, 30, 20, 150);
            FillRect(px, w, 5, 1, 7, 2, eCore);
            FillRect(px, w, 3, 1, 5, 2, eGlow);
            FillRect(px, w, 0, 1, 3, 2, eTrail);
            FillRect(px, w, 4, 0, 6, 3, eGlow);
            SaveSprite("bullet_enemy", tex, px);

            w = 10; h = 6;
            tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            px = new Color32[w * h];
            Clear(px, w, h);
            var shell = new Color32(255, 140, 40, 255);
            var shellLight = new Color32(255, 180, 80, 255);
            var shellTrail = new Color32(255, 80, 20, 200);
            FillRect(px, w, 6, 2, 9, 3, shell);
            FillRect(px, w, 7, 2, 8, 3, shellLight);
            FillRect(px, w, 2, 2, 6, 3, shell);
            FillRect(px, w, 0, 2, 2, 3, shellTrail);
            FillRect(px, w, 5, 1, 7, 4, shellLight);
            SaveSprite("bullet_shell", tex, px);
        }

        // ==================== 背景 ====================

        private static void GenerateBackgroundSprites()
        {
            MakeBackgroundSky();
            MakeBackgroundSea();
            MakeBackgroundCloud();
            MakeBackgroundMountain();
            MakeBackgroundFactory();
            MakeBackgroundCity();
            MakeBackgroundIndustrial();
        }

        private static void MakeBackgroundSky()
        {
            int w = 128, h = 128;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                float t = (float)y / h;
                var c = Color.Lerp(
                    new Color(0.35f, 0.15f, 0.25f),
                    new Color(0.75f, 0.4f, 0.25f),
                    t);
                for (int x = 0; x < w; x++)
                    px[y * w + x] = c;
            }
            var rng = new System.Random(1);
            for (int i = 0; i < 30; i++)
            {
                int x = rng.Next(w), y = rng.Next(h / 2);
                px[y * w + x] = new Color32(255, 240, 200, 200);
            }
            SaveSprite("background_sky", tex, px);
        }

        private static void MakeBackgroundSea()
        {
            int w = 128, h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                float t = (float)y / h;
                var c = Color.Lerp(
                    new Color(0.15f, 0.25f, 0.4f),
                    new Color(0.08f, 0.15f, 0.28f),
                    t);
                for (int x = 0; x < w; x++)
                    px[y * w + x] = c;
            }
            var rng = new System.Random(5);
            for (int i = 0; i < 50; i++)
            {
                int x = rng.Next(w), y = rng.Next(h);
                px[y * w + x] = new Color32(120, 150, 180, 180);
            }
            SaveSprite("background_sea", tex, px);
        }

        private static void MakeBackgroundCloud()
        {
            int w = 48, h = 24;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var cloud = new Color32(200, 195, 210, 200);
            var cloudLight = new Color32(230, 225, 240, 220);
            var cloudDark = new Color32(160, 155, 175, 180);

            FillRect(px, w, 8, 8, 40, 16, cloud);
            FillRect(px, w, 12, 5, 36, 18, cloud);
            FillRect(px, w, 16, 3, 32, 20, cloud);
            FillRect(px, w, 10, 10, 38, 14, cloudLight);
            FillRect(px, w, 8, 14, 40, 16, cloudDark);

            SaveSprite("background_cloud", tex, px);
        }

        private static void MakeBackgroundMountain()
        {
            int w = 128, h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var mountain = new Color32(60, 50, 65, 255);
            var mountainLight = new Color32(85, 72, 88, 255);
            var mountainDark = new Color32(40, 33, 45, 255);
            var snow = new Color32(200, 200, 210, 255);

            for (int x = 0; x < w; x++)
            {
                int peakH = (int)(Mathf.Sin(x * 0.1f) * 15 + Mathf.Sin(x * 0.05f) * 20 + 40);
                for (int y = 0; y < peakH && y < h; y++)
                {
                    var c = y > peakH - 5 ? mountainLight : y < peakH - 20 ? mountainDark : mountain;
                    px[(h - 1 - y) * w + x] = c;
                }
                if (peakH > 45 && peakH <= h)
                    px[(h - peakH) * w + x] = snow;
            }

            SaveSprite("background_mountain", tex, px);
        }

        private static void MakeBackgroundFactory()
        {
            int w = 128, h = 96;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var building = new Color32(50, 48, 55, 255);
            var buildingLight = new Color32(70, 68, 75, 255);
            var buildingDark = new Color32(35, 33, 40, 255);
            var window = new Color32(255, 200, 80, 200);
            var smoke = new Color32(80, 75, 80, 180);

            for (int x = 0; x < w; x++)
            {
                int bH = (int)(Mathf.Sin(x * 0.08f) * 10 + Mathf.Sin(x * 0.03f) * 15 + 50);
                for (int y = 0; y < bH && y < h; y++)
                {
                    var c = y > bH - 5 ? buildingLight : y < bH - 30 ? buildingDark : building;
                    px[(h - 1 - y) * w + x] = c;
                }
            }
            FillRect(px, w, 30, 40, 35, 96, buildingDark);
            FillRect(px, w, 80, 35, 85, 96, buildingDark);
            var rng = new System.Random(11);
            for (int i = 0; i < 25; i++)
            {
                int x = rng.Next(w), y = rng.Next(20, 60);
                px[y * w + x] = window;
            }
            for (int i = 0; i < 15; i++)
            {
                int x = 30 + rng.Next(10), y = 20 + rng.Next(20);
                px[y * w + x] = smoke;
            }

            SaveSprite("background_factory", tex, px);
        }

        private static void MakeBackgroundCity()
        {
            int w = 256, h = 128;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var building = new Color32(45, 42, 50, 230);
            var buildingLight = new Color32(65, 60, 70, 230);
            var buildingDark = new Color32(30, 28, 35, 230);
            var window = new Color32(255, 220, 100, 200);
            var windowDark = new Color32(80, 75, 60, 230);
            var roof = new Color32(35, 32, 38, 230);

            var rng = new System.Random(88);
            int x = 0;
            while (x < w)
            {
                int bw = rng.Next(20, 50);
                int bh = rng.Next(40, 100);
                int bx = x;
                int by = h - bh;
                if (bx + bw > w) bw = w - bx;
                // 建筑主体
                for (int dy = 0; dy < bh; dy++)
                    for (int dx = 0; dx < bw; dx++)
                    {
                        var c = dx == 0 || dx == bw - 1 ? buildingDark : dx < 3 ? buildingLight : building;
                        px[(by + dy) * w + bx + dx] = c;
                    }
                // 屋顶
                for (int dx = 0; dx < bw; dx++)
                    px[by * w + bx + dx] = roof;
                // 窗户
                for (int wy = by + 5; wy < h - 5; wy += 6)
                    for (int wx = bx + 4; wx < bx + bw - 4; wx += 5)
                    {
                        if (rng.Next(3) == 0)
                            px[wy * w + wx] = window;
                        else
                            px[wy * w + wx] = windowDark;
                    }
                x += bw + rng.Next(1, 4);
            }

            SaveSprite("background_city", tex, px);
        }

        private static void MakeBackgroundIndustrial()
        {
            int w = 256, h = 128;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var structure = new Color32(50, 48, 55, 230);
            var structureLight = new Color32(70, 68, 75, 230);
            var structureDark = new Color32(35, 33, 40, 230);
            var pipe = new Color32(80, 75, 70, 230);
            var smoke = new Color32(90, 85, 80, 180);
            var light = new Color32(255, 180, 60, 200);

            var rng = new System.Random(44);
            // 工厂建筑
            int x = 0;
            while (x < w)
            {
                int bw = rng.Next(30, 70);
                int bh = rng.Next(30, 70);
                int bx = x;
                int by = h - bh;
                if (bx + bw > w) bw = w - bx;
                for (int dy = 0; dy < bh; dy++)
                    for (int dx = 0; dx < bw; dx++)
                    {
                        var c = dx == 0 || dx == bw - 1 ? structureDark : dx < 3 ? structureLight : structure;
                        px[(by + dy) * w + bx + dx] = c;
                    }
                // 烟囱
                int chimneyX = bx + bw / 2;
                int chimneyH = rng.Next(20, 50);
                for (int dy = 0; dy < chimneyH; dy++)
                {
                    int cy = h - bh - dy - 1;
                    if (cy >= 0)
                    {
                        px[cy * w + chimneyX] = structureDark;
                        px[cy * w + chimneyX + 1] = structure;
                        px[cy * w + chimneyX + 2] = structureDark;
                    }
                }
                // 烟雾
                for (int i = 0; i < 15; i++)
                {
                    int sx = chimneyX + rng.Next(-3, 4);
                    int sy = h - bh - chimneyH - rng.Next(0, 20);
                    if (sx >= 0 && sx < w && sy >= 0 && sy < h)
                        px[sy * w + sx] = smoke;
                }
                // 灯光
                if (rng.Next(2) == 0)
                {
                    int lx = bx + rng.Next(2, bw - 2);
                    int ly = by + rng.Next(5, bh - 5);
                    px[ly * w + lx] = light;
                }
                x += bw + rng.Next(2, 6);
            }
            // 管道
            for (int py = h - 20; py < h - 10; py++)
                for (int px2 = 0; px2 < w; px2 += 2)
                    px[py * w + px2] = pipe;

            SaveSprite("background_industrial", tex, px);
        }

        // ==================== 特效 ====================

        private static void GenerateEffectSprites()
        {
            MakeMuzzleFlash();
            MakeExplosion();
            MakeHitSpark();
            MakeSmoke();
        }

        private static void MakeMuzzleFlash()
        {
            int w = 16, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var core = new Color32(255, 255, 200, 255);
            var mid = new Color32(255, 220, 80, 230);
            var outer = new Color32(255, 140, 30, 180);

            FillRect(px, w, 7, 6, 8, 9, core);
            FillRect(px, w, 6, 7, 9, 8, core);
            FillRect(px, w, 5, 7, 6, 8, mid);
            FillRect(px, w, 9, 7, 10, 8, mid);
            FillRect(px, w, 7, 4, 8, 5, mid);
            FillRect(px, w, 7, 10, 8, 11, mid);
            FillRect(px, w, 4, 7, 5, 8, outer);
            FillRect(px, w, 10, 7, 11, 8, outer);
            FillRect(px, w, 7, 2, 8, 3, outer);
            FillRect(px, w, 7, 12, 8, 13, outer);
            px[5 * w + 5] = mid;
            px[10 * w + 10] = mid;
            px[5 * w + 10] = mid;
            px[10 * w + 5] = mid;

            SaveSprite("muzzle_flash", tex, px);
        }

        private static void MakeExplosion()
        {
            int w = 32, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var core = new Color32(255, 255, 200, 255);
            var mid = new Color32(255, 180, 40, 240);
            var outer = new Color32(255, 80, 20, 200);
            var smoke = new Color32(80, 70, 65, 180);

            // 爆炸火球
            FillRect(px, w, 12, 12, 19, 19, core);
            FillRect(px, w, 10, 14, 21, 17, mid);
            FillRect(px, w, 14, 10, 17, 21, mid);
            FillRect(px, w, 8, 15, 11, 16, outer);
            FillRect(px, w, 20, 15, 23, 16, outer);
            FillRect(px, w, 15, 8, 16, 11, outer);
            FillRect(px, w, 15, 20, 16, 23, outer);
            // 烟雾
            px[6 * w + 10] = smoke;
            px[5 * w + 20] = smoke;
            px[25 * w + 12] = smoke;
            px[26 * w + 22] = smoke;
            px[4 * w + 16] = smoke;
            px[27 * w + 16] = smoke;

            SaveSprite("explosion", tex, px);
        }

        private static void MakeHitSpark()
        {
            int w = 16, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var core = new Color32(255, 255, 255, 255);
            var spark = new Color32(255, 220, 80, 230);
            var outer = new Color32(255, 120, 30, 180);

            FillRect(px, w, 7, 7, 8, 8, core);
            // 火花线条
            px[5 * w + 7] = spark;
            px[6 * w + 7] = spark;
            px[9 * w + 7] = spark;
            px[10 * w + 7] = spark;
            px[7 * w + 5] = spark;
            px[7 * w + 6] = spark;
            px[7 * w + 9] = spark;
            px[7 * w + 10] = spark;
            // 对角火花
            px[4 * w + 4] = outer;
            px[11 * w + 11] = outer;
            px[4 * w + 11] = outer;
            px[11 * w + 4] = outer;
            px[5 * w + 5] = spark;
            px[10 * w + 10] = spark;

            SaveSprite("hit_spark", tex, px);
        }

        private static void MakeSmoke()
        {
            int w = 24, h = 24;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var smoke = new Color32(100, 95, 90, 180);
            var smokeLight = new Color32(140, 135, 130, 160);
            var smokeDark = new Color32(70, 65, 60, 200);

            FillRect(px, w, 6, 8, 17, 16, smoke);
            FillRect(px, w, 8, 5, 15, 18, smoke);
            FillRect(px, w, 10, 3, 13, 20, smokeLight);
            FillRect(px, w, 6, 14, 17, 16, smokeDark);
            FillRect(px, w, 4, 10, 7, 14, smokeDark);
            FillRect(px, w, 16, 10, 19, 14, smokeDark);

            SaveSprite("smoke", tex, px);
        }

        // ==================== UI 图标 ====================

        private static void GenerateUISprites()
        {
            MakeUIHeart();
            MakeUIEnergy();
            MakeUIWeaponIcon();
            MakeUIArrow();
            MakeUIFrame();
        }

        private static void MakeUIHeart()
        {
            int w = 16, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var heart = new Color32(220, 50, 60, 255);
            var heartLight = new Color32(255, 100, 110, 255);
            var heartDark = new Color32(160, 30, 40, 255);
            var outline = new Color32(60, 15, 20, 255);

            // 心形像素图案
            FillRect(px, w, 3, 10, 5, 13, heart);
            FillRect(px, w, 10, 10, 12, 13, heart);
            FillRect(px, w, 2, 7, 6, 10, heart);
            FillRect(px, w, 9, 7, 13, 10, heart);
            FillRect(px, w, 3, 4, 12, 7, heart);
            FillRect(px, w, 4, 2, 11, 4, heart);
            FillRect(px, w, 6, 1, 9, 2, heart);
            FillRect(px, w, 5, 3, 5, 4, heartLight);
            FillRect(px, w, 4, 5, 5, 6, heartLight);
            FillRect(px, w, 10, 11, 11, 12, heartDark);
            FillRect(px, w, 9, 9, 10, 10, heartDark);
            px[6 * w + 7] = heartLight;
            px[5 * w + 8] = heartLight;

            DrawOutline(px, w, h, outline);
            SaveSprite("ui_heart", tex, px);
        }

        private static void MakeUIEnergy()
        {
            int w = 16, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var energy = new Color32(80, 180, 255, 255);
            var energyLight = new Color32(150, 220, 255, 255);
            var energyDark = new Color32(40, 120, 200, 255);
            var outline = new Color32(15, 40, 70, 255);

            // 闪电图案
            FillRect(px, w, 8, 13, 11, 15, energy);
            FillRect(px, w, 6, 10, 9, 13, energy);
            FillRect(px, w, 5, 7, 8, 10, energy);
            FillRect(px, w, 4, 4, 7, 7, energy);
            FillRect(px, w, 6, 1, 9, 4, energy);
            FillRect(px, w, 7, 13, 8, 14, energyLight);
            FillRect(px, w, 6, 10, 7, 11, energyLight);
            FillRect(px, w, 5, 7, 6, 8, energyLight);
            FillRect(px, w, 9, 4, 10, 6, energyDark);
            FillRect(px, w, 8, 1, 9, 3, energyDark);

            DrawOutline(px, w, h, outline);
            SaveSprite("ui_energy", tex, px);
        }

        private static void MakeUIWeaponIcon()
        {
            int w = 24, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var gun = new Color32(80, 80, 90, 255);
            var gunLight = new Color32(120, 120, 130, 255);
            var gunDark = new Color32(50, 50, 60, 255);
            var grip = new Color32(100, 70, 50, 255);
            var outline = new Color32(15, 15, 20, 255);

            FillRect(px, w, 2, 7, 18, 9, gun);
            FillRect(px, w, 2, 7, 2, 9, gunDark);
            FillRect(px, w, 16, 7, 18, 9, gunLight);
            FillRect(px, w, 18, 6, 21, 10, gun);
            FillRect(px, w, 19, 7, 20, 9, gunLight);
            FillRect(px, w, 5, 3, 8, 7, grip);
            FillRect(px, w, 5, 3, 5, 7, gunDark);
            FillRect(px, w, 10, 10, 14, 11, gunDark);
            FillRect(px, w, 11, 10, 13, 10, gunLight);

            DrawOutline(px, w, h, outline);
            SaveSprite("ui_weapon", tex, px);
        }

        private static void MakeUIArrow()
        {
            int w = 16, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var arrow = new Color32(240, 240, 240, 255);
            var arrowLight = new Color32(255, 255, 255, 255);
            var arrowDark = new Color32(180, 180, 180, 255);
            var outline = new Color32(40, 40, 40, 255);

            // 向右箭头
            FillRect(px, w, 11, 7, 13, 8, arrow);
            FillRect(px, w, 9, 6, 11, 9, arrow);
            FillRect(px, w, 7, 5, 9, 10, arrow);
            FillRect(px, w, 5, 4, 7, 11, arrow);
            FillRect(px, w, 3, 5, 5, 10, arrow);
            FillRect(px, w, 2, 6, 3, 9, arrow);
            FillRect(px, w, 11, 7, 12, 8, arrowLight);
            FillRect(px, w, 9, 6, 10, 7, arrowLight);
            FillRect(px, w, 13, 8, 13, 8, arrowDark);

            DrawOutline(px, w, h, outline);
            SaveSprite("ui_arrow", tex, px);
        }

        private static void MakeUIFrame()
        {
            int w = 32, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var frame = new Color32(180, 150, 90, 255);
            var frameLight = new Color32(220, 190, 130, 255);
            var frameDark = new Color32(130, 100, 60, 255);
            var corner = new Color32(240, 210, 140, 255);
            var outline = new Color32(50, 35, 15, 255);

            // 外框
            FillRect(px, w, 0, 0, 31, 2, frame);
            FillRect(px, w, 0, 29, 31, 31, frame);
            FillRect(px, w, 0, 0, 2, 31, frame);
            FillRect(px, w, 29, 0, 31, 31, frame);
            // 高光
            FillRect(px, w, 0, 0, 31, 0, frameLight);
            FillRect(px, w, 0, 0, 0, 31, frameLight);
            // 阴影
            FillRect(px, w, 0, 2, 31, 2, frameDark);
            FillRect(px, w, 2, 29, 31, 31, frameDark);
            // 角装饰
            FillRect(px, w, 0, 0, 4, 4, corner);
            FillRect(px, w, 27, 0, 31, 4, corner);
            FillRect(px, w, 0, 27, 4, 31, corner);
            FillRect(px, w, 27, 27, 31, 31, corner);
            px[1 * w + 1] = frameLight;
            px[30 * w + 1] = frameLight;
            px[1 * w + 30] = frameLight;
            px[30 * w + 30] = frameLight;

            DrawOutline(px, w, h, outline);
            SaveSprite("ui_frame", tex, px);
        }

        // ==================== 视差背景层 ====================

        private static void GenerateParallaxSprites()
        {
            MakeParallaxFarMountain();
            MakeParallaxMidHill();
            MakeParallaxNearTree();
            MakeParallaxForegroundGrass();
        }

        private static void MakeParallaxFarMountain()
        {
            int w = 256, h = 96;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var mountain = new Color32(75, 65, 85, 220);
            var mountainLight = new Color32(100, 90, 110, 220);
            var mountainDark = new Color32(55, 48, 65, 220);
            var snow = new Color32(220, 220, 230, 220);

            for (int x = 0; x < w; x++)
            {
                int peakH = (int)(Mathf.Sin(x * 0.04f) * 25 + Mathf.Sin(x * 0.08f) * 15 + Mathf.Sin(x * 0.02f) * 20 + 55);
                for (int y = 0; y < peakH && y < h; y++)
                {
                    var c = y > peakH - 8 ? mountainLight : y < peakH - 30 ? mountainDark : mountain;
                    px[(h - 1 - y) * w + x] = c;
                }
                if (peakH > 70 && peakH <= h)
                {
                    int snowIdx = (h - peakH) * w + x;
                    if (snowIdx >= 0 && snowIdx < px.Length)
                        px[snowIdx] = snow;
                    if (peakH > 75 && h - peakH + 1 < h)
                        px[(h - peakH + 1) * w + x] = snow;
                }
            }

            SaveSprite("parallax_far_mountain", tex, px);
        }

        private static void MakeParallaxMidHill()
        {
            int w = 256, h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var hill = new Color32(95, 110, 75, 230);
            var hillLight = new Color32(125, 140, 95, 230);
            var hillDark = new Color32(70, 85, 55, 230);
            var tree = new Color32(50, 75, 45, 230);

            for (int x = 0; x < w; x++)
            {
                int hillH = (int)(Mathf.Sin(x * 0.06f) * 12 + Mathf.Sin(x * 0.12f) * 8 + 30);
                for (int y = 0; y < hillH && y < h; y++)
                {
                    var c = y > hillH - 5 ? hillLight : y < hillH - 15 ? hillDark : hill;
                    px[(h - 1 - y) * w + x] = c;
                }
            }
            // 远处的树
            var rng = new System.Random(7);
            for (int i = 0; i < 30; i++)
            {
                int x = rng.Next(w);
                int baseY = h - (int)(Mathf.Sin(x * 0.06f) * 12 + Mathf.Sin(x * 0.12f) * 8 + 30);
                if (baseY > 0 && baseY < h)
                {
                    px[baseY * w + x] = tree;
                    if (baseY - 1 >= 0) px[(baseY - 1) * w + x] = tree;
                    if (baseY - 2 >= 0) px[(baseY - 2) * w + x] = tree;
                }
            }

            SaveSprite("parallax_mid_hill", tex, px);
        }

        private static void MakeParallaxNearTree()
        {
            int w = 32, h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var trunk = new Color32(80, 55, 35, 255);
            var trunkLight = new Color32(110, 80, 50, 255);
            var trunkDark = new Color32(55, 38, 22, 255);
            var leaf = new Color32(60, 100, 50, 255);
            var leafLight = new Color32(90, 135, 70, 255);
            var leafDark = new Color32(40, 75, 35, 255);
            var outline = new Color32(20, 30, 15, 255);

            // 树干
            FillRect(px, w, 14, 0, 17, 25, trunk);
            FillRect(px, w, 14, 0, 14, 25, trunkLight);
            FillRect(px, w, 17, 0, 17, 25, trunkDark);
            // 树枝
            FillRect(px, w, 11, 18, 14, 20, trunk);
            FillRect(px, w, 17, 20, 20, 22, trunk);
            // 树冠
            FillRect(px, w, 8, 25, 23, 45, leaf);
            FillRect(px, w, 5, 30, 26, 40, leaf);
            FillRect(px, w, 10, 22, 21, 48, leaf);
            FillRect(px, w, 8, 25, 10, 30, leafLight);
            FillRect(px, w, 11, 22, 14, 26, leafLight);
            FillRect(px, w, 20, 30, 23, 35, leafDark);
            FillRect(px, w, 22, 38, 25, 42, leafDark);
            FillRect(px, w, 6, 36, 9, 40, leafDark);
            px[28 * w + 16] = leafLight;
            px[32 * w + 13] = leafLight;
            px[38 * w + 18] = leafLight;

            DrawOutline(px, w, h, outline);
            SaveSprite("parallax_near_tree", tex, px);
        }

        private static void MakeParallaxForegroundGrass()
        {
            int w = 128, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var grass = new Color32(50, 90, 45, 255);
            var grassLight = new Color32(80, 120, 60, 255);
            var grassDark = new Color32(35, 65, 30, 255);

            var rng = new System.Random(3);
            for (int x = 0; x < w; x++)
            {
                int bladeH = rng.Next(8, 20);
                int type = rng.Next(3);
                var c = type == 0 ? grassLight : type == 1 ? grass : grassDark;
                for (int y = 0; y < bladeH && y < h; y++)
                {
                    px[(h - 1 - y) * w + x] = c;
                }
                if (rng.Next(3) == 0 && bladeH > 5)
                {
                    int bendX = x + rng.Next(-2, 3);
                    if (bendX >= 0 && bendX < w)
                        px[(h - bladeH) * w + bendX] = c;
                }
            }

            SaveSprite("parallax_foreground_grass", tex, px);
        }

        // ==================== 额外特效 ====================

        private static void GenerateExtraEffectSprites()
        {
            MakeShellCasing();
            MakeFootprint();
            MakeRingEffect();
            MakeDashTrail();
            MakePickupGlow();
        }

        private static void MakeShellCasing()
        {
            int w = 8, h = 12;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var shell = new Color32(220, 180, 60, 255);
            var shellLight = new Color32(255, 220, 100, 255);
            var shellDark = new Color32(170, 130, 35, 255);
            var outline = new Color32(60, 40, 10, 255);

            FillRect(px, w, 2, 2, 5, 9, shell);
            FillRect(px, w, 2, 2, 2, 9, shellDark);
            FillRect(px, w, 5, 2, 5, 9, shellLight);
            FillRect(px, w, 2, 9, 5, 10, shellDark);
            FillRect(px, w, 2, 1, 5, 2, shellLight);
            px[3 * w + 3] = shellLight;
            px[5 * w + 4] = shellLight;

            DrawOutline(px, w, h, outline);
            SaveSprite("shell_casing", tex, px);
        }

        private static void MakeFootprint()
        {
            int w = 12, h = 12;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var print = new Color32(60, 45, 30, 180);

            // 鞋印图案
            FillRect(px, w, 3, 2, 8, 6, print);
            FillRect(px, w, 4, 1, 7, 2, print);
            FillRect(px, w, 4, 6, 7, 8, print);
            px[2 * w + 5] = print;
            px[2 * w + 6] = print;
            px[8 * w + 4] = print;
            px[8 * w + 7] = print;
            px[9 * w + 5] = print;
            px[9 * w + 6] = print;

            SaveSprite("footprint", tex, px);
        }

        private static void MakeRingEffect()
        {
            int w = 32, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var ring = new Color32(150, 220, 255, 220);
            var ringLight = new Color32(220, 240, 255, 240);
            var ringOuter = new Color32(80, 160, 220, 180);

            // 圆环
            int cx = 16, cy = 16;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int dx = x - cx, dy = y - cy;
                    int dist = (int)(Mathf.Sqrt(dx * dx + dy * dy));
                    if (dist == 11) px[y * w + x] = ring;
                    else if (dist == 12) px[y * w + x] = ringLight;
                    else if (dist == 13 || dist == 10) px[y * w + x] = ringOuter;
                }
            }

            SaveSprite("ring_effect", tex, px);
        }

        private static void MakeDashTrail()
        {
            int w = 24, h = 24;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var trail = new Color32(150, 200, 255, 180);
            var trailLight = new Color32(200, 230, 255, 220);
            var trailDark = new Color32(100, 160, 220, 140);

            // 残影图案
            for (int i = 0; i < 5; i++)
            {
                int xOff = i * 4;
                int alpha = 220 - i * 35;
                var c = new Color32(150, 200, 255, (byte)alpha);
                FillRect(px, w, 2 + xOff, 8, 6 + xOff, 15, c);
                if (i < 3)
                    FillRect(px, w, 3 + xOff, 9, 5 + xOff, 14, trailLight);
            }
            FillRect(px, w, 2, 8, 6, 15, trailLight);
            FillRect(px, w, 18, 8, 22, 15, trailDark);

            SaveSprite("dash_trail", tex, px);
        }

        private static void MakePickupGlow()
        {
            int w = 24, h = 24;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);
            var glow = new Color32(255, 240, 150, 100);
            var glowMid = new Color32(255, 220, 80, 160);
            var glowCore = new Color32(255, 255, 200, 220);

            int cx = 12, cy = 12;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int dx = x - cx, dy = y - cy;
                    int dist = (int)(Mathf.Sqrt(dx * dx + dy * dy));
                    if (dist <= 3) px[y * w + x] = glowCore;
                    else if (dist <= 7) px[y * w + x] = glowMid;
                    else if (dist <= 11) px[y * w + x] = glow;
                }
            }

            SaveSprite("pickup_glow", tex, px);
        }
    }
}
