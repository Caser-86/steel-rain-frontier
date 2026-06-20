#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SteelRain.Editor
{
    /// <summary>
    /// 高细节像素角色预览生成器（48×64）。
    /// 用于评估代码生成像素艺术的最高质量。
    /// </summary>
    public static class HighDetailCharacterPreview
    {
        private const string PreviewDir = "Assets/Art/Preview";

        [MenuItem("Steel Rain/Preview High Detail Character")]
        public static void GeneratePreview()
        {
            if (!Directory.Exists(PreviewDir))
                Directory.CreateDirectory(PreviewDir);

            // 生成 4 个角色的 idle 帧，展示最高细节
            GenerateAilaIdle();
            GenerateKaelIdle();
            GenerateMiraIdle();
            GenerateZenIdle();

            // 生成一个走路动画帧序列（Aila 4 帧）
            GenerateAilaWalkFrames();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HighDetailPreview] Preview sprites generated at Assets/Art/Preview/");
        }

        // ===== Aila - 突击手（蓝色风衣 + 红围巾 + 步枪）=====
        private static void GenerateAilaIdle()
        {
            int w = 48, h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            // 调色板（精致配色 + 高光/阴影）
            var skin = new Color32(245, 210, 170, 255);
            var skinHi = new Color32(255, 230, 195, 255);
            var skinSh = new Color32(200, 165, 130, 255);
            var hair = new Color32(80, 50, 35, 255);
            var hairHi = new Color32(120, 80, 55, 255);
            var coat = new Color32(55, 95, 160, 255);
            var coatHi = new Color32(90, 135, 200, 255);
            var coatSh = new Color32(35, 65, 120, 255);
            var scarf = new Color32(200, 55, 55, 255);
            var scarfHi = new Color32(235, 85, 85, 255);
            var scarfSh = new Color32(155, 35, 35, 255);
            var pants = new Color32(45, 50, 65, 255);
            var pantsSh = new Color32(30, 35, 50, 255);
            var boots = new Color32(30, 25, 20, 255);
            var bootsHi = new Color32(55, 45, 35, 255);
            var gun = new Color32(45, 45, 50, 255);
            var gunHi = new Color32(80, 80, 85, 255);
            var gunSh = new Color32(25, 25, 30, 255);
            var belt = new Color32(70, 50, 30, 255);
            var buckle = new Color32(200, 170, 80, 255);
            var eye = new Color32(30, 30, 40, 255);
            var outline = new Color32(15, 15, 25, 255);

            // 头发（顶部 + 侧边，带高光）
            FillRect(px, w, 16, 50, 31, 60, hair);
            FillRect(px, w, 17, 52, 30, 60, hairHi);  // 高光
            FillRect(px, w, 14, 48, 18, 52, hair);     // 左鬓
            FillRect(px, w, 29, 48, 33, 52, hair);     // 右鬓
            FillRect(px, w, 18, 58, 29, 60, hairHi);   // 顶部高光

            // 脸部
            FillRect(px, w, 17, 44, 30, 50, skin);
            FillRect(px, w, 17, 44, 18, 46, skinSh);   // 左脸阴影
            FillRect(px, w, 29, 44, 30, 50, skinHi);   // 右脸高光

            // 眼睛
            FillRect(px, w, 20, 47, 22, 48, eye);
            FillRect(px, w, 25, 47, 27, 48, eye);
            // 眼睛高光
            px[48 * w + 21] = Color.white;
            px[48 * w + 26] = Color.white;

            // 嘴
            FillRect(px, w, 22, 44, 25, 44, skinSh);

            // 围巾（颈部）
            FillRect(px, w, 17, 40, 30, 43, scarf);
            FillRect(px, w, 19, 42, 28, 43, scarfHi);
            FillRect(px, w, 17, 40, 18, 41, scarfSh);
            // 围巾飘带
            FillRect(px, w, 14, 36, 17, 40, scarf);
            FillRect(px, w, 14, 36, 15, 39, scarfHi);

            // 风衣（躯干，带阴影/高光/褶皱）
            FillRect(px, w, 16, 26, 31, 40, coat);
            FillRect(px, w, 16, 26, 17, 40, coatSh);   // 左侧阴影
            FillRect(px, w, 30, 26, 31, 40, coatHi);   // 右侧高光
            // 衣领
            FillRect(px, w, 18, 40, 22, 41, coatSh);
            FillRect(px, w, 25, 40, 29, 41, coatSh);
            // 纽扣
            px[38 * w + 23] = buckle;
            px[34 * w + 23] = buckle;
            px[30 * w + 23] = buckle;
            // 腰带
            FillRect(px, w, 16, 25, 31, 26, belt);
            px[25 * w + 23] = buckle;
            px[25 * w + 24] = buckle;

            // 手臂
            FillRect(px, w, 12, 28, 16, 38, coat);
            FillRect(px, w, 12, 28, 13, 38, coatSh);
            FillRect(px, w, 31, 28, 35, 38, coat);
            FillRect(px, w, 34, 28, 35, 38, coatHi);
            // 手
            FillRect(px, w, 12, 26, 15, 28, skin);
            FillRect(px, w, 32, 26, 35, 28, skin);

            // 腿
            FillRect(px, w, 18, 14, 23, 25, pants);
            FillRect(px, w, 24, 14, 29, 25, pants);
            FillRect(px, w, 18, 14, 19, 25, pantsSh);  // 左腿阴影
            // 膝盖高光
            FillRect(px, w, 20, 19, 22, 20, pantsSh);

            // 靴子
            FillRect(px, w, 17, 8, 23, 14, boots);
            FillRect(px, w, 23, 8, 29, 14, boots);
            FillRect(px, w, 17, 12, 23, 14, bootsHi);  // 靴子高光
            FillRect(px, w, 23, 12, 29, 14, bootsHi);

            // 步枪（右侧，斜持）
            FillRect(px, w, 33, 32, 45, 33, gun);
            FillRect(px, w, 33, 33, 45, 33, gunHi);    // 枪管高光
            FillRect(px, w, 33, 32, 35, 33, gunSh);    // 枪身阴影
            // 枪托
            FillRect(px, w, 33, 34, 36, 36, gunSh);
            // 扳机护圈
            FillRect(px, w, 38, 30, 39, 32, gun);
            // 瞄准镜
            FillRect(px, w, 40, 34, 43, 35, gunHi);
            px[34 * w + 41] = Color.white;             // 镜片反光

            DrawOutline(px, w, h, outline);
            SavePreviewSprite("preview_aila_idle", tex, px);
        }

        // ===== Kael - 重装兵（红色铠甲 + 盾牌）=====
        private static void GenerateKaelIdle()
        {
            int w = 48, h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var skin = new Color32(230, 190, 150, 255);
            var skinHi = new Color32(245, 210, 170, 255);
            var skinSh = new Color32(195, 155, 120, 255);
            var hair = new Color32(35, 30, 25, 255);
            var hairHi = new Color32(60, 50, 40, 255);
            var armor = new Color32(140, 45, 45, 255);
            var armorHi = new Color32(180, 70, 70, 255);
            var armorSh = new Color32(95, 30, 30, 255);
            var plate = new Color32(180, 180, 195, 255);
            var plateHi = new Color32(220, 220, 235, 255);
            var plateSh = new Color32(130, 130, 145, 255);
            var shield = new Color32(160, 160, 175, 255);
            var shieldHi = new Color32(200, 200, 215, 255);
            var shieldSh = new Color32(110, 110, 125, 255);
            var shieldEdge = new Color32(200, 170, 80, 255);
            var pants = new Color32(55, 40, 40, 255);
            var pantsSh = new Color32(35, 25, 25, 255);
            var boots = new Color32(30, 25, 20, 255);
            var bootsHi = new Color32(55, 45, 35, 255);
            var eye = new Color32(30, 20, 20, 255);
            var outline = new Color32(15, 10, 10, 255);

            // 头发（短发）
            FillRect(px, w, 17, 50, 30, 60, hair);
            FillRect(px, w, 18, 52, 29, 60, hairHi);
            FillRect(px, w, 16, 48, 18, 50, hair);
            FillRect(px, w, 29, 48, 31, 50, hair);

            // 脸
            FillRect(px, w, 17, 44, 30, 50, skin);
            FillRect(px, w, 17, 44, 18, 46, skinSh);
            FillRect(px, w, 29, 44, 30, 50, skinHi);

            // 眼睛（坚定眼神）
            FillRect(px, w, 20, 47, 22, 48, eye);
            FillRect(px, w, 25, 47, 27, 48, eye);
            // 眉毛
            FillRect(px, w, 20, 49, 22, 49, hair);
            FillRect(px, w, 25, 49, 27, 49, hair);

            // 下巴胡茬
            FillRect(px, w, 21, 43, 26, 44, hairHi);

            // 颈甲
            FillRect(px, w, 17, 40, 30, 43, armor);
            FillRect(px, w, 17, 40, 18, 43, armorSh);

            // 胸甲（带板甲纹理）
            FillRect(px, w, 16, 26, 31, 40, armor);
            FillRect(px, w, 16, 26, 17, 40, armorSh);
            FillRect(px, w, 30, 26, 31, 40, armorHi);
            // 胸甲板纹
            FillRect(px, w, 20, 36, 27, 37, armorSh);
            FillRect(px, w, 20, 32, 27, 33, armorSh);
            // 中央装饰
            FillRect(px, w, 22, 30, 25, 38, plate);
            FillRect(px, w, 23, 31, 24, 37, plateHi);
            // 肩甲
            FillRect(px, w, 12, 36, 17, 42, plate);
            FillRect(px, w, 13, 37, 16, 41, plateHi);
            FillRect(px, w, 30, 36, 35, 42, plate);
            FillRect(px, w, 31, 37, 34, 41, plateHi);

            // 手臂
            FillRect(px, w, 13, 28, 16, 36, armor);
            FillRect(px, w, 31, 28, 34, 36, armor);
            // 手
            FillRect(px, w, 13, 26, 16, 28, skin);
            FillRect(px, w, 31, 26, 34, 28, skin);

            // 腰带
            FillRect(px, w, 16, 24, 31, 26, armorSh);

            // 腿甲
            FillRect(px, w, 18, 14, 23, 24, armor);
            FillRect(px, w, 24, 14, 29, 24, armor);
            FillRect(px, w, 18, 14, 19, 24, armorSh);
            // 膝甲
            FillRect(px, w, 19, 18, 22, 20, plate);
            FillRect(px, w, 25, 18, 28, 20, plate);

            // 靴子
            FillRect(px, w, 17, 8, 23, 14, boots);
            FillRect(px, w, 23, 8, 29, 14, boots);
            FillRect(px, w, 17, 12, 23, 14, bootsHi);
            FillRect(px, w, 23, 12, 29, 14, bootsHi);

            // 盾牌（左侧，大圆盾）
            FillRect(px, w, 4, 22, 14, 38, shield);
            FillRect(px, w, 4, 22, 5, 38, shieldSh);
            FillRect(px, w, 13, 22, 14, 38, shieldHi);
            FillRect(px, w, 4, 22, 14, 23, shieldEdge);   // 上边
            FillRect(px, w, 4, 37, 14, 38, shieldEdge);   // 下边
            FillRect(px, w, 4, 22, 5, 38, shieldEdge);    // 左边
            FillRect(px, w, 13, 22, 14, 38, shieldEdge);  // 右边
            // 盾牌中央纹章
            FillRect(px, w, 8, 28, 10, 32, shieldEdge);
            FillRect(px, w, 7, 29, 11, 31, shieldEdge);

            DrawOutline(px, w, h, outline);
            SavePreviewSprite("preview_kael_idle", tex, px);
        }

        // ===== Mira - 狙击手（绿色迷彩服 + 狙击枪）=====
        private static void GenerateMiraIdle()
        {
            int w = 48, h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var skin = new Color32(220, 180, 140, 255);
            var skinHi = new Color32(235, 195, 155, 255);
            var skinSh = new Color32(185, 145, 110, 255);
            var hair = new Color32(130, 95, 55, 255);
            var hairHi = new Color32(165, 125, 75, 255);
            var suit = new Color32(65, 105, 65, 255);
            var suitHi = new Color32(90, 135, 90, 255);
            var suitSh = new Color32(45, 80, 45, 255);
            var camo1 = new Color32(85, 75, 45, 255);    // 迷彩斑
            var camo2 = new Color32(50, 80, 50, 255);
            var pants = new Color32(55, 65, 55, 255);
            var pantsSh = new Color32(35, 45, 35, 255);
            var boots = new Color32(35, 30, 25, 255);
            var bootsHi = new Color32(60, 50, 40, 255);
            var rifle = new Color32(40, 40, 45, 255);
            var rifleHi = new Color32(75, 75, 80, 255);
            var scope = new Color32(30, 25, 20, 255);
            var scopeLens = new Color32(100, 150, 200, 255);
            var glove = new Color32(50, 45, 40, 255);
            var eye = new Color32(35, 30, 25, 255);
            var outline = new Color32(15, 20, 15, 255);

            // 头发（马尾）
            FillRect(px, w, 16, 50, 31, 60, hair);
            FillRect(px, w, 17, 52, 30, 60, hairHi);
            FillRect(px, w, 14, 48, 18, 52, hair);
            FillRect(px, w, 29, 48, 33, 52, hair);
            // 马尾辫
            FillRect(px, w, 30, 44, 33, 52, hair);
            FillRect(px, w, 31, 45, 32, 51, hairHi);

            // 脸
            FillRect(px, w, 17, 44, 30, 50, skin);
            FillRect(px, w, 17, 44, 18, 46, skinSh);
            FillRect(px, w, 29, 44, 30, 50, skinHi);

            // 眼睛（专注）
            FillRect(px, w, 20, 47, 22, 48, eye);
            FillRect(px, w, 25, 47, 27, 48, eye);
            px[48 * w + 21] = Color.white;
            px[48 * w + 26] = Color.white;

            // 嘴
            FillRect(px, w, 22, 44, 25, 44, skinSh);

            // 迷彩服（躯干）
            FillRect(px, w, 16, 26, 31, 40, suit);
            FillRect(px, w, 16, 26, 17, 40, suitSh);
            FillRect(px, w, 30, 26, 31, 40, suitHi);
            // 迷彩斑
            FillRect(px, w, 19, 36, 22, 38, camo1);
            FillRect(px, w, 25, 32, 28, 35, camo2);
            FillRect(px, w, 20, 30, 23, 32, camo1);
            FillRect(px, w, 26, 38, 29, 40, camo2);

            // 衣领
            FillRect(px, w, 18, 40, 22, 41, suitSh);
            FillRect(px, w, 25, 40, 29, 41, suitSh);

            // 手臂
            FillRect(px, w, 12, 28, 16, 38, suit);
            FillRect(px, w, 12, 28, 13, 38, suitSh);
            FillRect(px, w, 31, 28, 35, 38, suit);
            FillRect(px, w, 34, 28, 35, 38, suitHi);
            // 手套
            FillRect(px, w, 12, 26, 15, 28, glove);
            FillRect(px, w, 32, 26, 35, 28, glove);

            // 腰带
            FillRect(px, w, 16, 25, 31, 26, suitSh);

            // 腿
            FillRect(px, w, 18, 14, 23, 25, pants);
            FillRect(px, w, 24, 14, 29, 25, pants);
            FillRect(px, w, 18, 14, 19, 25, pantsSh);
            // 膝盖护垫
            FillRect(px, w, 19, 18, 22, 20, suitSh);
            FillRect(px, w, 25, 18, 28, 20, suitSh);

            // 靴子
            FillRect(px, w, 17, 8, 23, 14, boots);
            FillRect(px, w, 23, 8, 29, 14, boots);
            FillRect(px, w, 17, 12, 23, 14, bootsHi);
            FillRect(px, w, 23, 12, 29, 14, bootsHi);

            // 狙击枪（长枪，斜持）
            FillRect(px, w, 33, 32, 46, 33, rifle);
            FillRect(px, w, 33, 33, 46, 33, rifleHi);
            FillRect(px, w, 33, 32, 36, 33, rifle);  // 枪身
            // 枪托
            FillRect(px, w, 33, 34, 37, 37, rifle);
            // 狙击镜
            FillRect(px, w, 39, 35, 43, 36, scope);
            FillRect(px, w, 42, 35, 43, 36, scopeLens);  // 镜片
            px[35 * w + 42] = Color.white;               // 反光
            // 扳机
            FillRect(px, w, 38, 30, 39, 32, rifle);
            // 枪口
            FillRect(px, w, 45, 32, 46, 33, rifleHi);

            DrawOutline(px, w, h, outline);
            SavePreviewSprite("preview_mira_idle", tex, px);
        }

        // ===== Zen - 工程师（紫色制服 + 手雷）=====
        private static void GenerateZenIdle()
        {
            int w = 48, h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var skin = new Color32(235, 200, 160, 255);
            var skinHi = new Color32(250, 215, 175, 255);
            var skinSh = new Color32(200, 165, 130, 255);
            var hair = new Color32(60, 50, 70, 255);
            var hairHi = new Color32(90, 75, 105, 255);
            var suit = new Color32(85, 60, 110, 255);
            var suitHi = new Color32(115, 85, 145, 255);
            var suitSh = new Color32(60, 40, 85, 255);
            var vest = new Color32(50, 45, 55, 255);    // 战术背心
            var vestHi = new Color32(75, 65, 80, 255);
            var pants = new Color32(50, 45, 60, 255);
            var pantsSh = new Color32(35, 30, 45, 255);
            var boots = new Color32(30, 25, 20, 255);
            var bootsHi = new Color32(55, 45, 35, 255);
            var grenade = new Color32(80, 90, 70, 255);
            var grenadeHi = new Color32(110, 120, 95, 255);
            var tool = new Color32(150, 130, 80, 255);  // 工具
            var glove = new Color32(60, 55, 65, 255);
            var eye = new Color32(40, 35, 50, 255);
            var goggle = new Color32(80, 140, 180, 255); // 护目镜
            var goggleFrame = new Color32(40, 35, 45, 255);
            var outline = new Color32(15, 12, 20, 255);

            // 头发
            FillRect(px, w, 16, 50, 31, 60, hair);
            FillRect(px, w, 17, 52, 30, 60, hairHi);
            FillRect(px, w, 14, 48, 18, 52, hair);
            FillRect(px, w, 29, 48, 33, 52, hair);

            // 脸
            FillRect(px, w, 17, 44, 30, 50, skin);
            FillRect(px, w, 17, 44, 18, 46, skinSh);
            FillRect(px, w, 29, 44, 30, 50, skinHi);

            // 护目镜（额头上）
            FillRect(px, w, 19, 49, 28, 50, goggleFrame);
            FillRect(px, w, 20, 49, 22, 50, goggle);
            FillRect(px, w, 25, 49, 27, 50, goggle);

            // 眼睛
            FillRect(px, w, 20, 47, 22, 48, eye);
            FillRect(px, w, 25, 47, 27, 48, eye);
            px[48 * w + 21] = Color.white;
            px[48 * w + 26] = Color.white;

            // 嘴（微笑）
            FillRect(px, w, 22, 44, 25, 44, skinSh);
            px[44 * w + 23] = skinHi;
            px[44 * w + 24] = skinHi;

            // 颈部
            FillRect(px, w, 18, 40, 29, 43, skin);
            FillRect(px, w, 18, 40, 19, 43, skinSh);

            // 紫色制服
            FillRect(px, w, 16, 26, 31, 40, suit);
            FillRect(px, w, 16, 26, 17, 40, suitSh);
            FillRect(px, w, 30, 26, 31, 40, suitHi);

            // 战术背心
            FillRect(px, w, 19, 28, 28, 40, vest);
            FillRect(px, w, 19, 28, 20, 40, vestHi);
            // 背心口袋
            FillRect(px, w, 21, 32, 24, 35, vestHi);
            FillRect(px, w, 23, 37, 26, 40, vestHi);
            // 拉链
            FillRect(px, w, 23, 28, 24, 40, suitSh);

            // 手臂
            FillRect(px, w, 12, 28, 16, 38, suit);
            FillRect(px, w, 12, 28, 13, 38, suitSh);
            FillRect(px, w, 31, 28, 35, 38, suit);
            FillRect(px, w, 34, 28, 35, 38, suitHi);
            // 手套
            FillRect(px, w, 12, 26, 15, 28, glove);
            FillRect(px, w, 32, 26, 35, 28, glove);

            // 腰带
            FillRect(px, w, 16, 25, 31, 26, vest);

            // 腿
            FillRect(px, w, 18, 14, 23, 25, pants);
            FillRect(px, w, 24, 14, 29, 25, pants);
            FillRect(px, w, 18, 14, 19, 25, pantsSh);

            // 靴子
            FillRect(px, w, 17, 8, 23, 14, boots);
            FillRect(px, w, 23, 8, 29, 14, boots);
            FillRect(px, w, 17, 12, 23, 14, bootsHi);
            FillRect(px, w, 23, 12, 29, 14, bootsHi);

            // 手雷（腰间）
            FillRect(px, w, 14, 22, 16, 25, grenade);
            FillRect(px, w, 14, 24, 16, 25, grenadeHi);
            px[25 * w + 15] = grenadeHi;

            // 工具（右手持）
            FillRect(px, w, 33, 24, 36, 26, tool);
            FillRect(px, w, 33, 24, 36, 24, glove);

            DrawOutline(px, w, h, outline);
            SavePreviewSprite("preview_zen_idle", tex, px);
        }

        // ===== Aila 走路动画 4 帧 =====
        private static void GenerateAilaWalkFrames()
        {
            GenerateAilaWalk(0, "preview_aila_walk_0");
            GenerateAilaWalk(1, "preview_aila_walk_1");
            GenerateAilaWalk(2, "preview_aila_walk_2");
            GenerateAilaWalk(3, "preview_aila_walk_3");
        }

        private static void GenerateAilaWalk(int frame, string name)
        {
            int w = 48, h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Clear(px, w, h);

            var skin = new Color32(245, 210, 170, 255);
            var skinSh = new Color32(200, 165, 130, 255);
            var hair = new Color32(80, 50, 35, 255);
            var hairHi = new Color32(120, 80, 55, 255);
            var coat = new Color32(55, 95, 160, 255);
            var coatHi = new Color32(90, 135, 200, 255);
            var coatSh = new Color32(35, 65, 120, 255);
            var scarf = new Color32(200, 55, 55, 255);
            var scarfHi = new Color32(235, 85, 85, 255);
            var pants = new Color32(45, 50, 65, 255);
            var pantsSh = new Color32(30, 35, 50, 255);
            var boots = new Color32(30, 25, 20, 255);
            var bootsHi = new Color32(55, 45, 35, 255);
            var gun = new Color32(45, 45, 50, 255);
            var gunHi = new Color32(80, 80, 85, 255);
            var eye = new Color32(30, 30, 40, 255);
            var outline = new Color32(15, 15, 25, 255);

            // 头部（保持不变）
            FillRect(px, w, 16, 50, 31, 60, hair);
            FillRect(px, w, 17, 52, 30, 60, hairHi);
            FillRect(px, w, 14, 48, 18, 52, hair);
            FillRect(px, w, 29, 48, 33, 52, hair);
            FillRect(px, w, 17, 44, 30, 50, skin);
            FillRect(px, w, 17, 44, 18, 46, skinSh);
            FillRect(px, w, 20, 47, 22, 48, eye);
            FillRect(px, w, 25, 47, 27, 48, eye);
            FillRect(px, w, 22, 44, 25, 44, skinSh);

            // 围巾
            FillRect(px, w, 17, 40, 30, 43, scarf);
            FillRect(px, w, 19, 42, 28, 43, scarfHi);

            // 躯干（轻微摆动）
            int bodyOffset = frame == 1 ? 1 : (frame == 3 ? -1 : 0);
            FillRect(px, w, 16 + bodyOffset, 26, 31 + bodyOffset, 40, coat);
            FillRect(px, w, 16 + bodyOffset, 26, 17 + bodyOffset, 40, coatSh);
            FillRect(px, w, 30 + bodyOffset, 26, 31 + bodyOffset, 40, coatHi);

            // 手臂（摆动）
            int armOffset = frame == 0 ? 0 : (frame == 1 ? -2 : (frame == 2 ? 0 : 2));
            FillRect(px, w, 12, 28 + armOffset, 16, 38 + armOffset, coat);
            FillRect(px, w, 31, 28 - armOffset, 35, 38 - armOffset, coat);
            FillRect(px, w, 12, 26 + armOffset, 15, 28 + armOffset, skin);
            FillRect(px, w, 32, 26 - armOffset, 35, 28 - armOffset, skin);

            // 腿部动画（4 帧循环：站立、迈左、站立、迈右）
            switch (frame)
            {
                case 0: // 站立
                    FillRect(px, w, 18, 14, 23, 25, pants);
                    FillRect(px, w, 24, 14, 29, 25, pants);
                    FillRect(px, w, 17, 8, 23, 14, boots);
                    FillRect(px, w, 23, 8, 29, 14, boots);
                    break;
                case 1: // 迈左腿
                    FillRect(px, w, 16, 14, 21, 25, pants);
                    FillRect(px, w, 24, 14, 29, 25, pants);
                    FillRect(px, w, 14, 10, 20, 16, boots);  // 左腿前迈
                    FillRect(px, w, 24, 8, 30, 14, boots);
                    FillRect(px, w, 14, 14, 20, 16, bootsHi);
                    FillRect(px, w, 24, 12, 30, 14, bootsHi);
                    break;
                case 2: // 站立
                    FillRect(px, w, 18, 14, 23, 25, pants);
                    FillRect(px, w, 24, 14, 29, 25, pants);
                    FillRect(px, w, 17, 8, 23, 14, boots);
                    FillRect(px, w, 23, 8, 29, 14, boots);
                    break;
                case 3: // 迈右腿
                    FillRect(px, w, 18, 14, 23, 25, pants);
                    FillRect(px, w, 26, 14, 31, 25, pants);
                    FillRect(px, w, 17, 8, 23, 14, boots);
                    FillRect(px, w, 27, 10, 33, 16, boots);  // 右腿前迈
                    FillRect(px, w, 17, 12, 23, 14, bootsHi);
                    FillRect(px, w, 27, 14, 33, 16, bootsHi);
                    break;
            }

            // 枪（保持位置）
            FillRect(px, w, 33, 32, 45, 33, gun);
            FillRect(px, w, 33, 33, 45, 33, gunHi);

            DrawOutline(px, w, h, outline);
            SavePreviewSprite(name, tex, px);
        }

        // ===== 工具方法 =====
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

        private static void SavePreviewSprite(string name, Texture2D tex, Color32[] px)
        {
            tex.SetPixels32(px);
            tex.Apply();
            var path = $"{PreviewDir}/{name}.png";
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
            Debug.Log($"[HighDetailPreview] Saved {name}.png ({tex.width}x{tex.height})");
        }
    }
}
#endif
