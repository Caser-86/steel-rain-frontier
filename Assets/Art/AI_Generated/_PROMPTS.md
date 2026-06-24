# 铁雨前线 - AI 美术资源生成清单（完整扩展版 v2）

## 使用方法

1. 打开 AI 图像生成工具：
   - Bing Image Creator: https://www.bing.com/images/create （免费，推荐）
   - 或 ChatGPT DALL-E、Midjourney、Stable Diffusion 等

2. 复制下方每个提示词，生成图片

3. 下载图片，重命名为指定文件名，放到：
   ```
   d:\大模型学习\铁雨前线\Assets\Art\AI_Generated\
   ```

4. 全部下载完成后，在 Unity 中执行菜单：
   ```
   Steel Rain > Check AI Art Status   # 先检查完整性
   Steel Rain > Import AI Art         # 导入资源
   Steel Rain > Build All             # 重新构建场景
   ```

5. 脚本会自动：导入图片 → 切割精灵 → 替换占位图 → 重新构建场景

---

## 通用风格说明（每个提示词都包含）

所有图片使用统一风格：
```
pixel art, side view, 2D game sprite, transparent background, clean black outline, 
metal slug style, high detail, vibrant colors, anime quality
```

## 分辨率标准（v2 全面提升）

| 资源类型 | 分辨率 | 说明 |
|---------|--------|------|
| 玩家角色 | **1024×1024** | 动漫级细节，每帧独立文件 |
| 普通敌人 | **1024×1024** | 与玩家同等质量 |
| Boss | **2048×2048** | 超高细节 |
| 道具 | **512×512** | |
| 地形 Tile | **512×512** | |
| 背景 | **4096×2048** | 4K 视差背景 |
| 特效 | **512×512** | |
| UI 图标 | **256×256** | |

## 多帧动画命名规范

每个动作的多帧图片按以下规范命名：
```
player_aila_walk_0.png   # 第 0 帧
player_aila_walk_1.png   # 第 1 帧
player_aila_walk_2.png   # 第 2 帧
player_aila_walk_3.png   # 第 3 帧
```

动画系统会自动按数字顺序播放，循环动画首尾相连。

---

## 一、玩家角色多帧动画（4角色 × 7动作 × 多帧 = 140张）

### 1. Aila - 突击手（蓝风衣 + 红围巾 + 步枪）

#### 1.1 Idle 站立（2帧呼吸动画）
**文件名**: `player_aila_idle_0.png`
```
pixel art game character sprite, side view, female assault soldier named Aila, 
wearing blue trench coat with red scarf, brown hair, holding assault rifle, 
military boots, standing idle pose, breathing in, chest slightly expanded, 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

**文件名**: `player_aila_idle_1.png`
```
pixel art game character sprite, side view, female assault soldier named Aila, 
wearing blue trench coat with red scarf, brown hair, holding assault rifle, 
military boots, standing idle pose, breathing out, chest relaxed, 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

#### 1.2 Walk 走路（6帧循环）
**文件名**: `player_aila_walk_0.png` ~ `player_aila_walk_5.png`
```
pixel art game character sprite, side view, female assault soldier named Aila, 
wearing blue trench coat with red scarf, brown hair, holding assault rifle, 
walking pose frame N of 6 cycle, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

帧描述：
- `walk_0`: 左腿前迈，右腿后蹬，身体微前倾
- `walk_1`: 双腿并拢过渡，重心中位
- `walk_2`: 右腿前迈，左腿后蹬
- `walk_3`: 右腿落地，左腿抬起过渡
- `walk_4`: 左腿前迈准备，手臂摆动
- `walk_5`: 双腿并拢过渡，重心中位（接回 walk_0）

#### 1.3 Run 奔跑（6帧循环）
**文件名**: `player_aila_run_0.png` ~ `player_aila_run_5.png`
```
pixel art game character sprite, side view, female assault soldier named Aila, 
wearing blue trench coat with red scarf, brown hair, holding assault rifle 
tucked at hip, running pose frame N of 6 cycle, [POSE DESCRIPTION], 
dynamic motion blur effect, transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

帧描述：
- `run_0`: 大步前冲，左腿前伸，右腿后蹬，身体大幅前倾
- `run_1`: 腾空过渡，双腿离地，手臂收紧
- `run_2`: 右腿前迈，左腿后摆，风衣飘扬
- `run_3`: 右腿落地支撑，左腿抬起准备
- `run_4`: 左腿大步前迈，右腿后蹬
- `run_5`: 腾空过渡，双腿离地

#### 1.4 Jump 跳跃（4帧）
**文件名**: `player_aila_jump_0.png` ~ `player_aila_jump_3.png`
```
pixel art game character sprite, side view, female assault soldier named Aila, 
wearing blue trench coat with red scarf, brown hair, holding assault rifle, 
[POSE DESCRIPTION], transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

帧描述：
- `jump_0`: 起跳准备，双腿弯曲下蹲，手臂后摆
- `jump_1`: 上升中，双腿伸展，身体向上，风衣飘起
- `jump_2`: 顶点/下降，双腿微收，身体水平
- `jump_3`: 落地，双腿弯曲缓冲，单膝微跪

#### 1.5 Crouch 趴下（3帧）
**文件名**: `player_aila_crouch_0.png` ~ `player_aila_crouch_2.png`
```
pixel art game character sprite, side view, female assault soldier named Aila, 
wearing blue trench coat with red scarf, brown hair, holding assault rifle, 
[POSE DESCRIPTION], transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

帧描述：
- `crouch_0`: 开始趴下，单膝跪地，身体下沉
- `crouch_1`: 完全趴下，双膝跪地，身体压低，步枪端平
- `crouch_2`: 趴下持枪瞄准，身体最低位，枪口前指

#### 1.6 Shoot 射击（4帧）
**文件名**: `player_aila_shoot_0.png` ~ `player_aila_shoot_3.png`
```
pixel art game character sprite, side view, female assault soldier named Aila, 
wearing blue trench coat with red scarf, brown hair, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

帧描述：
- `shoot_0`: 举枪准备，步枪抬至肩部，瞄准前方
- `shoot_1`: 开火瞬间，枪口火焰大爆，身体后坐力
- `shoot_2`: 持续射击，枪口火焰小，弹壳抛出
- `shoot_3`: 收枪，步枪下垂，准备下一动作

#### 1.7 Death 死亡（4帧）
**文件名**: `player_aila_death_0.png` ~ `player_aila_death_3.png`
```
pixel art game character sprite, side view, female assault soldier named Aila, 
wearing blue trench coat with red scarf, brown hair, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

帧描述：
- `death_0`: 中弹瞬间，身体后仰，武器脱手
- `death_1`: 倒下中，膝盖弯曲倒地
- `death_2`: 倒地，身体侧卧地面
- `death_3`: 静止，平躺地面，最终姿态

---

### 2. Bruno - 重装兵（红铠甲 + 大圆盾）

#### 2.1 Idle 站立（2帧）
**文件名**: `player_bruno_idle_0.png`, `player_bruno_idle_1.png`
```
pixel art game character sprite, side view, male heavy soldier named Bruno, 
wearing red plate armor with shoulder pads, holding large round shield, 
short black hair, beard, muscular, standing idle pose, [BREATHING PHASE], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

#### 2.2 Walk 走路（6帧）
**文件名**: `player_bruno_walk_0.png` ~ `player_bruno_walk_5.png`
```
pixel art game character sprite, side view, male heavy soldier named Bruno, 
wearing red plate armor with shoulder pads, holding large round shield, 
short black hair, beard, walking pose frame N of 6 cycle, [POSE DESCRIPTION], 
heavy armored movement, transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

#### 2.3 Run 奔跑（6帧）
**文件名**: `player_bruno_run_0.png` ~ `player_bruno_run_5.png`
```
pixel art game character sprite, side view, male heavy soldier named Bruno, 
wearing red plate armor with shoulder pads, holding large round shield, 
short black hair, beard, running pose frame N of 6 cycle, [POSE DESCRIPTION], 
shield raised, dynamic motion, transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

#### 2.4 Jump 跳跃（4帧）
**文件名**: `player_bruno_jump_0.png` ~ `player_bruno_jump_3.png`
```
pixel art game character sprite, side view, male heavy soldier named Bruno, 
wearing red plate armor with shoulder pads, holding large round shield, 
short black hair, beard, [POSE DESCRIPTION], transparent background, 
clean black outline, metal slug style, high detail, anime quality, 1024x1024
```

#### 2.5 Crouch 蹲防（3帧）
**文件名**: `player_bruno_crouch_0.png` ~ `player_bruno_crouch_2.png`
```
pixel art game character sprite, side view, male heavy soldier named Bruno, 
wearing red plate armor with shoulder pads, holding large round shield, 
short black hair, beard, [POSE DESCRIPTION], defensive stance, 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

#### 2.6 Shield Bash 盾击（4帧）
**文件名**: `player_bruno_shoot_0.png` ~ `player_bruno_shoot_3.png`
```
pixel art game character sprite, side view, male heavy soldier named Bruno, 
wearing red plate armor with shoulder pads, holding large round shield, 
short black hair, beard, [POSE DESCRIPTION], transparent background, 
clean black outline, metal slug style, high detail, anime quality, 1024x1024
```

帧描述：
- `shoot_0`: 盾牌蓄力，身体后仰
- `shoot_1`: 盾牌前推，全力撞击
- `shoot_2`: 撞击完成，盾牌前伸
- `shoot_3`: 收盾回防

#### 2.7 Death 死亡（4帧）
**文件名**: `player_bruno_death_0.png` ~ `player_bruno_death_3.png`
```
pixel art game character sprite, side view, male heavy soldier named Bruno, 
wearing red plate armor with shoulder pads, holding large round shield, 
short black hair, beard, [POSE DESCRIPTION], transparent background, 
clean black outline, metal slug style, high detail, anime quality, 1024x1024
```

---

### 3. Mara - 狙击手（绿迷彩 + 狙击枪 + 马尾）

#### 3.1 Idle 站立（2帧）
**文件名**: `player_mara_idle_0.png`, `player_mara_idle_1.png`
```
pixel art game character sprite, side view, female sniper named Mara, 
wearing green camo suit with ponytail brown hair, holding long sniper rifle 
with scope, focused expression, standing idle pose, [BREATHING PHASE], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

#### 3.2 Walk 走路（6帧）
**文件名**: `player_mara_walk_0.png` ~ `player_mara_walk_5.png`
```
pixel art game character sprite, side view, female sniper named Mara, 
wearing green camo suit with ponytail brown hair, holding long sniper rifle, 
walking pose frame N of 6 cycle, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

#### 3.3 Run 奔跑（6帧）
**文件名**: `player_mara_run_0.png` ~ `player_mara_run_5.png`
```
pixel art game character sprite, side view, female sniper named Mara, 
wearing green camo suit with ponytail brown hair, holding long sniper rifle, 
running pose frame N of 6 cycle, [POSE DESCRIPTION], rifle tucked, 
ponytail swinging, transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

#### 3.4 Jump 跳跃（4帧）
**文件名**: `player_mara_jump_0.png` ~ `player_mara_jump_3.png`
```
pixel art game character sprite, side view, female sniper named Mara, 
wearing green camo suit with ponytail brown hair, holding long sniper rifle, 
[POSE DESCRIPTION], transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

#### 3.5 Crouch 瞄准（3帧）
**文件名**: `player_mara_crouch_0.png` ~ `player_mara_crouch_2.png`
```
pixel art game character sprite, side view, female sniper named Mara, 
wearing green camo suit with ponytail brown hair, holding long sniper rifle, 
[POSE DESCRIPTION], scoped rifle ready, transparent background, 
clean black outline, metal slug style, high detail, anime quality, 1024x1024
```

#### 3.6 Shoot 射击（4帧）
**文件名**: `player_mara_shoot_0.png` ~ `player_mara_shoot_3.png`
```
pixel art game character sprite, side view, female sniper named Mara, 
wearing green camo suit with ponytail brown hair, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

帧描述：
- `shoot_0`: 举枪瞄准，狙击枪架起
- `shoot_1`: 开火瞬间，巨大后坐力，枪口火焰长
- `shoot_2`: 后坐力消退，枪口烟雾
- `shoot_3`: 收枪，准备下一发

#### 3.7 Death 死亡（4帧）
**文件名**: `player_mara_death_0.png` ~ `player_mara_death_3.png`
```
pixel art game character sprite, side view, female sniper named Mara, 
wearing green camo suit with ponytail brown hair, holding long sniper rifle, 
[POSE DESCRIPTION], transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

---

### 4. Niko - 工程师（紫制服 + 护目镜 + 手雷）

#### 4.1 Idle 站立（2帧）
**文件名**: `player_niko_idle_0.png`, `player_niko_idle_1.png`
```
pixel art game character sprite, side view, male engineer named Niko, 
wearing purple uniform with tactical vest and goggles on forehead, 
holding wrench tool, grenades on belt, purple hair, standing idle pose, 
[BREATHING PHASE], transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

#### 4.2 Walk 走路（6帧）
**文件名**: `player_niko_walk_0.png` ~ `player_niko_walk_5.png`
```
pixel art game character sprite, side view, male engineer named Niko, 
wearing purple uniform with tactical vest and goggles on forehead, 
holding wrench tool, grenades on belt, purple hair, walking pose frame N of 6 cycle, 
[POSE DESCRIPTION], transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

#### 4.3 Run 奔跑（6帧）
**文件名**: `player_niko_run_0.png` ~ `player_niko_run_5.png`
```
pixel art game character sprite, side view, male engineer named Niko, 
wearing purple uniform with tactical vest and goggles on forehead, 
holding wrench tool, grenades on belt, purple hair, running pose frame N of 6 cycle, 
[POSE DESCRIPTION], transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

#### 4.4 Jump 跳跃（4帧）
**文件名**: `player_niko_jump_0.png` ~ `player_niko_jump_3.png`
```
pixel art game character sprite, side view, male engineer named Niko, 
wearing purple uniform with tactical vest and goggles on forehead, 
holding wrench tool, grenades on belt, purple hair, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

#### 4.5 Crouch 蹲下（3帧）
**文件名**: `player_niko_crouch_0.png` ~ `player_niko_crouch_2.png`
```
pixel art game character sprite, side view, male engineer named Niko, 
wearing purple uniform with tactical vest and goggles on forehead, 
holding wrench tool, grenades on belt, purple hair, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

#### 4.6 Throw 投掷手雷（4帧）
**文件名**: `player_niko_shoot_0.png` ~ `player_niko_shoot_3.png`
```
pixel art game character sprite, side view, male engineer named Niko, 
wearing purple uniform with tactical vest and goggles on forehead, 
[POSE DESCRIPTION], transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

帧描述：
- `shoot_0`: 取手雷，从腰带摘下手雷
- `shoot_1`: 拉环，手臂后摆蓄力
- `shoot_2`: 投掷，手臂前甩，手雷脱手
- `shoot_3`: 收势，手臂下垂

#### 4.7 Death 死亡（4帧）
**文件名**: `player_niko_death_0.png` ~ `player_niko_death_3.png`
```
pixel art game character sprite, side view, male engineer named Niko, 
wearing purple uniform with tactical vest and goggles on forehead, 
holding wrench tool, grenades on belt, purple hair, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

---

## 二、敌人多帧动画（6敌人 × 4动作 × 3-4帧 = 78张）

### 6. 步枪兵（4动作 × 4帧 = 16张）

#### 6.1 Idle（2帧）
**文件名**: `enemy_rifle_idle_0.png`, `enemy_rifle_idle_1.png`
```
pixel art game enemy sprite, side view, enemy soldier with green helmet, 
wearing olive uniform, holding rifle gun, hostile expression, 
standing idle pose, [BREATHING PHASE], transparent background, 
clean black outline, metal slug style, high detail, anime quality, 1024x1024
```

#### 6.2 Walk（4帧）
**文件名**: `enemy_rifle_walk_0.png` ~ `enemy_rifle_walk_3.png`
```
pixel art game enemy sprite, side view, enemy soldier with green helmet, 
wearing olive uniform, holding rifle gun, walking pose frame N of 4 cycle, 
[POSE DESCRIPTION], transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

#### 6.3 Shoot（4帧）
**文件名**: `enemy_rifle_shoot_0.png` ~ `enemy_rifle_shoot_3.png`
```
pixel art game enemy sprite, side view, enemy soldier with green helmet, 
wearing olive uniform, [POSE DESCRIPTION], transparent background, 
clean black outline, metal slug style, high detail, anime quality, 1024x1024
```

#### 6.4 Death（4帧）
**文件名**: `enemy_rifle_death_0.png` ~ `enemy_rifle_death_3.png`
```
pixel art game enemy sprite, side view, enemy soldier with green helmet, 
wearing olive uniform, holding rifle gun, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

---

### 7. 盾牌兵（4动作 × 3帧 = 12张）

#### 7.1 Idle（2帧）
**文件名**: `enemy_shield_idle_0.png`, `enemy_shield_idle_1.png`
```
pixel art game enemy sprite, side view, heavy enemy soldier with green helmet, 
wearing olive armor, carrying large rectangular shield, menacing pose, 
standing idle, [BREATHING PHASE], transparent background, 
clean black outline, metal slug style, high detail, anime quality, 1024x1024
```

#### 7.2 Walk（3帧）
**文件名**: `enemy_shield_walk_0.png` ~ `enemy_shield_walk_2.png`
```
pixel art game enemy sprite, side view, heavy enemy soldier with green helmet, 
wearing olive armor, carrying large rectangular shield, walking pose frame N of 3, 
shield raised, [POSE DESCRIPTION], transparent background, 
clean black outline, metal slug style, high detail, anime quality, 1024x1024
```

#### 7.3 Attack（3帧）
**文件名**: `enemy_shield_shoot_0.png` ~ `enemy_shield_shoot_2.png`
```
pixel art game enemy sprite, side view, heavy enemy soldier with green helmet, 
wearing olive armor, [POSE DESCRIPTION], transparent background, 
clean black outline, metal slug style, high detail, anime quality, 1024x1024
```

#### 7.4 Death（4帧）
**文件名**: `enemy_shield_death_0.png` ~ `enemy_shield_death_3.png`
```
pixel art game enemy sprite, side view, heavy enemy soldier with green helmet, 
wearing olive armor, carrying large rectangular shield, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

---

### 8. 无人机（4动作 × 4帧 = 16张）

#### 8.1 Idle（2帧）
**文件名**: `enemy_drone_idle_0.png`, `enemy_drone_idle_1.png`
```
pixel art game enemy sprite, side view, flying military drone robot, 
metallic gray body with rotor blades, glowing red eye, mounted gun, 
futuristic design, hovering idle, [SLIGHT ALTITUDE CHANGE], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

#### 8.2 Move（4帧）
**文件名**: `enemy_drone_walk_0.png` ~ `enemy_drone_walk_3.png`
```
pixel art game enemy sprite, side view, flying military drone robot, 
metallic gray body with rotor blades spinning, glowing red eye, mounted gun, 
flying forward, motion blur, [ALTITUDE VARIATION], transparent background, 
clean black outline, metal slug style, high detail, anime quality, 1024x1024
```

#### 8.3 Shoot（4帧）
**文件名**: `enemy_drone_shoot_0.png` ~ `enemy_drone_shoot_3.png`
```
pixel art game enemy sprite, side view, flying military drone robot, 
metallic gray body with rotor blades, glowing red eye, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

#### 8.4 Death（4帧）
**文件名**: `enemy_drone_death_0.png` ~ `enemy_drone_death_3.png`
```
pixel art game enemy sprite, side view, flying military drone robot, 
[POSE DESCRIPTION], transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

帧描述：
- `death_0`: 中弹冒烟，机身倾斜
- `death_1`: 火焰喷出，部件脱落
- `death_2`: 爆炸瞬间，火球扩散
- `death_3`: 残骸坠落，烟雾弥漫

---

### 9. 掷弹兵（4动作 × 3帧 = 12张）

#### 9.1 Idle（2帧）
**文件名**: `enemy_grenadier_idle_0.png`, `enemy_grenadier_idle_1.png`
```
pixel art game enemy sprite, side view, enemy grenadier soldier with brown helmet, 
wearing brown uniform, holding grenade launcher, ammo belt, 
standing idle pose, [BREATHING PHASE], transparent background, 
clean black outline, metal slug style, high detail, anime quality, 1024x1024
```

#### 9.2 Walk（3帧）
**文件名**: `enemy_grenadier_walk_0.png` ~ `enemy_grenadier_walk_2.png`
```
pixel art game enemy sprite, side view, enemy grenadier soldier with brown helmet, 
wearing brown uniform, holding grenade launcher, ammo belt, walking pose frame N of 3, 
[POSE DESCRIPTION], transparent background, clean black outline, 
metal slug style, high detail, anime quality, 1024x1024
```

#### 9.3 Throw（3帧）
**文件名**: `enemy_grenadier_shoot_0.png` ~ `enemy_grenadier_shoot_2.png`
```
pixel art game enemy sprite, side view, enemy grenadier soldier with brown helmet, 
wearing brown uniform, [POSE DESCRIPTION], transparent background, 
clean black outline, metal slug style, high detail, anime quality, 1024x1024
```

#### 9.4 Death（4帧）
**文件名**: `enemy_grenadier_death_0.png` ~ `enemy_grenadier_death_3.png`
```
pixel art game enemy sprite, side view, enemy grenadier soldier with brown helmet, 
wearing brown uniform, holding grenade launcher, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

---

### 10. 火焰兵（新增敌人，4动作 × 3帧 = 11张）

#### 10.1 Idle（2帧）
**文件名**: `enemy_flamer_idle_0.png`, `enemy_flamer_idle_1.png`
```
pixel art game enemy sprite, side view, enemy flamethrower soldier with red helmet, 
wearing red uniform with fuel tanks on back, holding flamethrower weapon, 
menacing expression, standing idle pose, [BREATHING PHASE], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

#### 10.2 Walk（3帧）
**文件名**: `enemy_flamer_walk_0.png` ~ `enemy_flamer_walk_2.png`
```
pixel art game enemy sprite, side view, enemy flamethrower soldier with red helmet, 
wearing red uniform with fuel tanks on back, holding flamethrower weapon, 
walking pose frame N of 3, [POSE DESCRIPTION], transparent background, 
clean black outline, metal slug style, high detail, anime quality, 1024x1024
```

#### 10.3 Attack（3帧）
**文件名**: `enemy_flamer_shoot_0.png` ~ `enemy_flamer_shoot_2.png`
```
pixel art game enemy sprite, side view, enemy flamethrower soldier with red helmet, 
wearing red uniform with fuel tanks on back, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

#### 10.4 Death（3帧）
**文件名**: `enemy_flamer_death_0.png` ~ `enemy_flamer_death_2.png`
```
pixel art game enemy sprite, side view, enemy flamethrower soldier with red helmet, 
wearing red uniform with fuel tanks on back, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 1024x1024
```

---

### 11. 行走Boss（4动作 × 4帧 = 16张）

#### 11.1 Idle（2帧）
**文件名**: `miniboss_walker_idle_0.png`, `miniboss_walker_idle_1.png`
```
pixel art game boss sprite, side view, large mechanical walker boss robot, 
dark metallic body with red glowing eyes, dual arm cannons, 
bipedal legs, standing idle, intimidating design, [BREATHING PHASE], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 2048x2048
```

#### 11.2 Walk（4帧）
**文件名**: `miniboss_walker_walk_0.png` ~ `miniboss_walker_walk_3.png`
```
pixel art game boss sprite, side view, large mechanical walker boss robot, 
dark metallic body with red glowing eyes, dual arm cannons, 
bipedal legs, walking pose frame N of 4 cycle, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 2048x2048
```

#### 11.3 Attack（4帧）
**文件名**: `miniboss_walker_shoot_0.png` ~ `miniboss_walker_shoot_3.png`
```
pixel art game boss sprite, side view, large mechanical walker boss robot, 
dark metallic body with red glowing eyes, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 2048x2048
```

#### 11.4 Death（4帧）
**文件名**: `miniboss_walker_death_0.png` ~ `miniboss_walker_death_3.png`
```
pixel art game boss sprite, side view, large mechanical walker boss robot, 
[POSE DESCRIPTION], transparent background, clean black outline, 
metal slug style, high detail, anime quality, 2048x2048
```

---

### 12. 炮台Boss（4动作 × 3帧 = 11张）

#### 12.1 Idle（2帧）
**文件名**: `turret_boss_idle_0.png`, `turret_boss_idle_1.png`
```
pixel art game boss sprite, side view, stationary turret boss with triple cannons, 
dark metallic armored body, red glowing core eye, mounted on base, 
idle pose, menacing industrial design, [CORE EYE PULSE], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 2048x2048
```

#### 12.2 Aim（3帧）
**文件名**: `turret_boss_walk_0.png` ~ `turret_boss_walk_2.png`
```
pixel art game boss sprite, side view, stationary turret boss with triple cannons, 
dark metallic armored body, red glowing core eye, [CANNONS AIMING DIRECTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 2048x2048
```

#### 12.3 Attack（3帧）
**文件名**: `turret_boss_shoot_0.png` ~ `turret_boss_shoot_2.png`
```
pixel art game boss sprite, side view, stationary turret boss with triple cannons, 
dark metallic armored body, red glowing core eye, [POSE DESCRIPTION], 
transparent background, clean black outline, metal slug style, 
high detail, anime quality, 2048x2048
```

#### 12.4 Death（3帧）
**文件名**: `turret_boss_death_0.png` ~ `turret_boss_death_2.png`
```
pixel art game boss sprite, side view, stationary turret boss with triple cannons, 
[POSE DESCRIPTION], transparent background, clean black outline, 
metal slug style, high detail, anime quality, 2048x2048
```

---

## 三、地形 Tile 系统（6地形 × 8变种 = 48张，支持任意组合拼接）

### 地形变种说明

每种地形包含 8 个变种，支持任意组合拼接：
- `ground_xxx.png` - 平地（无限横向拼接）
- `ground_xxx_left.png` - 左边缘（平台左端）
- `ground_xxx_right.png` - 右边缘（平台右端）
- `ground_xxx_mid.png` - 平台中段（左右可拼接）
- `ground_xxx_single.png` - 单块（独立浮空块）
- `ground_xxx_corner_left.png` - 左转角（L型左下角）
- `ground_xxx_corner_right.png` - 右转角（L型右下角）
- `ground_xxx_slope.png` - 斜坡（45度过渡）

### 13. 海滩地形（8变种）

#### 13.1 海滩平地
**文件名**: `ground_beach.png`
```
pixel art game tile, beach sand ground texture, yellow sand with small rocks, 
seamless tileable horizontally, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

#### 13.2 海滩左边缘
**文件名**: `ground_beach_left.png`
```
pixel art game tile, beach sand platform left edge, yellow sand with rocks, 
curved left side, seamless tileable right, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

#### 13.3 海滩右边缘
**文件名**: `ground_beach_right.png`
```
pixel art game tile, beach sand platform right edge, yellow sand with rocks, 
curved right side, seamless tileable left, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

#### 13.4 海滩中段
**文件名**: `ground_beach_mid.png`
```
pixel art game tile, beach sand platform middle, yellow sand with rocks, 
seamless tileable both sides, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

#### 13.5 海滩单块
**文件名**: `ground_beach_single.png`
```
pixel art game tile, beach sand floating single block, yellow sand with rocks, 
all four sides visible, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

#### 13.6 海滩左转角
**文件名**: `ground_beach_corner_left.png`
```
pixel art game tile, beach sand L-shape corner left, yellow sand with rocks, 
top and right sides seamless, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

#### 13.7 海滩右转角
**文件名**: `ground_beach_corner_right.png`
```
pixel art game tile, beach sand L-shape corner right, yellow sand with rocks, 
top and left sides seamless, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

#### 13.8 海滩斜坡
**文件名**: `ground_beach_slope.png`
```
pixel art game tile, beach sand slope 45 degrees, yellow sand with rocks, 
diagonal surface, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

---

### 14. 村庄地形（8变种）

#### 14.1 ~ 14.8
**文件名**: `ground_village.png`, `ground_village_left.png`, 
`ground_village_right.png`, `ground_village_mid.png`, 
`ground_village_single.png`, `ground_village_corner_left.png`, 
`ground_village_corner_right.png`, `ground_village_slope.png`

通用提示词模板（替换 [VARIANT]）：
```
pixel art game tile, village stone [VARIANT], gray cobblestone, 
moss spots, [EDGE DESCRIPTION], side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

变种描述：
- `ground_village.png`: 平地，无缝横向拼接
- `ground_village_left.png`: 左边缘，右侧可拼接
- `ground_village_right.png`: 右边缘，左侧可拼接
- `ground_village_mid.png`: 中段，两侧可拼接
- `ground_village_single.png`: 单块，四面可见
- `ground_village_corner_left.png`: 左转角，上右可拼接
- `ground_village_corner_right.png`: 右转角，上左可拼接
- `ground_village_slope.png`: 45度斜坡

---

### 15. 战壕地形（8变种）

#### 15.1 ~ 15.8
**文件名**: `ground_trench.png`, `ground_trench_left.png`, 
`ground_trench_right.png`, `ground_trench_mid.png`, 
`ground_trench_single.png`, `ground_trench_corner_left.png`, 
`ground_trench_corner_right.png`, `ground_trench_slope.png`

通用提示词模板：
```
pixel art game tile, muddy trench [VARIANT], brown dirt with puddles, 
wooden planks, [EDGE DESCRIPTION], side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

---

### 16. 工厂地形（8变种）

#### 16.1 ~ 16.8
**文件名**: `ground_factory.png`, `ground_factory_left.png`, 
`ground_factory_right.png`, `ground_factory_mid.png`, 
`ground_factory_single.png`, `ground_factory_corner_left.png`, 
`ground_factory_corner_right.png`, `ground_factory_slope.png`

通用提示词模板：
```
pixel art game tile, factory metal [VARIANT], steel plates with rivets, 
industrial texture, [EDGE DESCRIPTION], side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

---

### 17. 城市地形（8变种）

#### 17.1 ~ 17.8
**文件名**: `ground_city.png`, `ground_city_left.png`, 
`ground_city_right.png`, `ground_city_mid.png`, 
`ground_city_single.png`, `ground_city_corner_left.png`, 
`ground_city_corner_right.png`, `ground_city_slope.png`

通用提示词模板：
```
pixel art game tile, city concrete [VARIANT], gray concrete with cracks, 
urban debris, [EDGE DESCRIPTION], side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

---

### 18. 工业地形（8变种）

#### 18.1 ~ 18.8
**文件名**: `ground_industrial.png`, `ground_industrial_left.png`, 
`ground_industrial_right.png`, `ground_industrial_mid.png`, 
`ground_industrial_single.png`, `ground_industrial_corner_left.png`, 
`ground_industrial_corner_right.png`, `ground_industrial_slope.png`

通用提示词模板：
```
pixel art game tile, industrial metal grating [VARIANT], steel grid pattern, 
rust spots, [EDGE DESCRIPTION], side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

---

### 19. 雪地地形（新增，8变种）

#### 19.1 ~ 19.8
**文件名**: `ground_snow.png`, `ground_snow_left.png`, 
`ground_snow_right.png`, `ground_snow_mid.png`, 
`ground_snow_single.png`, `ground_snow_corner_left.png`, 
`ground_snow_corner_right.png`, `ground_snow_slope.png`

通用提示词模板：
```
pixel art game tile, snow [VARIANT], white snow with ice crystals, 
small rocks visible, [EDGE DESCRIPTION], side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

---

## 四、道具（20张）

### 20. 木箱
**文件名**: `crate.png`
```
pixel art game item, wooden crate with metal corners, brown wood planks, 
metal reinforcement, side view, transparent background, clean black outline, 
metal slug style, high detail, 512x512
```

### 21. 武器拾取物
**文件名**: `weapon_pickup.png`
```
pixel art game item, weapon pickup crate with weapon icon inside, 
glowing yellow-orange light, metal casing, side view, 
transparent background, clean black outline, metal slug style, 
high detail, 512x512
```

### 22. 检查点旗帜
**文件名**: `checkpoint_flag.png`
```
pixel art game object, checkpoint flag pole with yellow flag, 
red emblem on flag, wooden pole, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

### 23. 医疗包
**文件名**: `health_pickup.png`
```
pixel art game item, health pickup medical kit, green liquid vial with white cross, 
glass tube with green liquid, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

### 24. 武器拾取
**文件名**: `weapon_pickup.png`
```
pixel art game item, weapon pickup gun with golden glow, 
dark metal gun with yellow energy, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

### 25. 爆炸桶
**文件名**: `explosive_barrel.png`
```
pixel art game object, red explosive barrel with yellow warning stripes, 
hazard symbol, metal rings, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

### 26. 沙袋
**文件名**: `sandbag.png`
```
pixel art game object, stack of military sandbags, brown burlap bags, 
stacked barricade, side view, transparent background, clean black outline, 
metal slug style, high detail, 512x512
```

### 27. 路障
**文件名**: `barricade.png`
```
pixel art game object, metal barricade with yellow warning stripes, 
rust spots, steel frame, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

### 28. 尖刺
**文件名**: `spikes.png`
```
pixel art game hazard, row of metal spikes on wooden base, 
sharp silver spikes, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

### 29. 路标
**文件名**: `signpost.png`
```
pixel art game object, wooden signpost with arrow sign, 
brown wood, pointing direction, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

### 30. 弹药箱
**文件名**: `ammo_box.png`
```
pixel art game item, military ammunition box, green metal box with bullets, 
side view, transparent background, clean black outline, 
metal slug style, high detail, 512x512
```

### 31. 能量核心
**文件名**: `energy_core.png`
```
pixel art game item, glowing blue energy core crystal, futuristic power cell, 
side view, transparent background, clean black outline, 
metal slug style, high detail, 512x512
```

### 32. 钥匙卡（新增）
**文件名**: `keycard.png`
```
pixel art game item, security keycard with yellow stripe, 
blue plastic card with magnetic strip, side view, transparent background, 
clean black outline, metal slug style, high detail, 512x512
```

### 33. 地雷（新增）
**文件名**: `landmine.png`
```
pixel art game hazard, military landmine disc, green metal disc with pressure plate, 
side view, transparent background, clean black outline, 
metal slug style, high detail, 512x512
```

### 34. 油桶（新增）
**文件名**: `fuel_barrel.png`
```
pixel art game object, red fuel barrel with cap, metal cylinder, 
side view, transparent background, clean black outline, 
metal slug style, high detail, 512x512
```

### 35. 望远镜（新增）
**文件名**: `binoculars.png`
```
pixel art game item, military binoculars, black optical device, 
side view, transparent background, clean black outline, 
metal slug style, high detail, 512x512
```

### 36. 无线电（新增）
**文件名**: `radio.png`
```
pixel art game item, military radio communicator, green box with antenna, 
side view, transparent background, clean black outline, 
metal slug style, high detail, 512x512
```

### 37. 战利品箱（新增）
**文件名**: `loot_chest.png`
```
pixel art game object, military supply chest, green metal box with lock, 
side view, transparent background, clean black outline, 
metal slug style, high detail, 512x512
```

### 38. 雷达（新增）
**文件名**: `radar.png`
```
pixel art game object, military radar dish, gray satellite dish on stand, 
side view, transparent background, clean black outline, 
metal slug style, high detail, 512x512
```

### 39. 旗帜（新增）
**文件名**: `flag.png`
```
pixel art game object, military flag on pole, red flag with star emblem, 
side view, transparent background, clean black outline, 
metal slug style, high detail, 512x512
```

---

## 五、背景（多层视差，12张）

### 40. 海滩背景（远景）
**文件名**: `bg_beach.png`
```
pixel art game background, beach war zone, ocean horizon, distant cliffs, 
sky with clouds, military ships silhouette, no foreground, 
side view, metal slug style, high detail, anime quality, 4096x2048
```

### 41. 海滩背景（中景）
**文件名**: `bg_beach_mid.png`
```
pixel art game background, beach war zone midground, palm trees, 
sand dunes, military bunkers, no foreground, side view, 
metal slug style, high detail, anime quality, 4096x2048
```

### 42. 海滩背景（前景装饰）
**文件名**: `bg_beach_fg.png`
```
pixel art game background, beach war zone foreground decoration, 
rocks, driftwood, beach grass, no characters, side view, 
metal slug style, high detail, anime quality, 4096x2048
```

### 43. 工厂背景（远景）
**文件名**: `bg_factory.png`
```
pixel art game background, factory interior, pipes and machinery, 
industrial pipes, conveyor belts, dark atmosphere, no foreground, 
side view, metal slug style, high detail, anime quality, 4096x2048
```

### 44. 工厂背景（中景）
**文件名**: `bg_factory_mid.png`
```
pixel art game background, factory interior midground, catwalks, 
machinery details, warning signs, no foreground, side view, 
metal slug style, high detail, anime quality, 4096x2048
```

### 45. 城市背景（远景）
**文件名**: `bg_city.png`
```
pixel art game background, war-torn city skyline, destroyed buildings, 
smoke clouds, distant ruins, no foreground, side view, 
metal slug style, high detail, anime quality, 4096x2048
```

### 46. 城市背景（中景）
**文件名**: `bg_city_mid.png`
```
pixel art game background, war-torn city midground, broken walls, 
rubble, abandoned cars, no foreground, side view, 
metal slug style, high detail, anime quality, 4096x2048
```

### 47. 雪地背景（远景，新增）
**文件名**: `bg_snow.png`
```
pixel art game background, snowy mountain battlefield, white peaks, 
pine trees, frozen lake, no foreground, side view, 
metal slug style, high detail, anime quality, 4096x2048
```

### 48. 雪地背景（中景，新增）
**文件名**: `bg_snow_mid.png`
```
pixel art game background, snowy battlefield midground, snow covered rocks, 
frozen bunker, icicles, no foreground, side view, 
metal slug style, high detail, anime quality, 4096x2048
```

### 49. 战壕背景（远景，新增）
**文件名**: `bg_trench.png`
```
pixel art game background, war trench battlefield, muddy landscape, 
barbed wire, distant explosions, no foreground, side view, 
metal slug style, high detail, anime quality, 4096x2048
```

### 50. 战壕背景（中景，新增）
**文件名**: `bg_trench_mid.png`
```
pixel art game background, war trench midground, sandbag walls, 
wooden supports, mud, no foreground, side view, 
metal slug style, high detail, anime quality, 4096x2048
```

### 51. 夜空背景（新增）
**文件名**: `bg_night_sky.png`
```
pixel art game background, night sky with stars and moon, 
distant city lights, clouds, no foreground, side view, 
metal slug style, high detail, anime quality, 4096x2048
```

---

## 六、特效（15张）

### 52. 爆炸
**文件名**: `explosion.png`
```
pixel art game effect, explosion blast, orange and yellow fireball, 
smoke clouds, sparks, side view, transparent background, 
metal slug style, high detail, 512x512
```

### 53. 枪口火焰
**文件名**: `muzzle_flash.png`
```
pixel art game effect, muzzle flash from gun, bright yellow orange flash, 
star burst shape, side view, transparent background, 
metal slug style, high detail, 512x512
```

### 54. 火花
**文件名**: `spark.png`
```
pixel art game effect, spark impact, small yellow sparks burst, 
side view, transparent background, metal slug style, high detail, 256x256
```

### 55. 烟雾
**文件名**: `smoke.png`
```
pixel art game effect, gray smoke cloud puff, soft edges, 
side view, transparent background, metal slug style, high detail, 512x512
```

### 56. 血液飞溅
**文件名**: `blood_splash.png`
```
pixel art game effect, red blood splash splatter, droplets, 
side view, transparent background, metal slug style, high detail, 256x256
```

### 57. 弹壳
**文件名**: `shell_casing.png`
```
pixel art game effect, brass bullet shell casing, small golden cylinder, 
side view, transparent background, metal slug style, high detail, 128x128
```

### 58. 命中效果
**文件名**: `hit_effect.png`
```
pixel art game effect, bullet hit impact, white flash with sparks, 
circular burst, side view, transparent background, 
metal slug style, high detail, 256x256
```

### 59. 能量护盾
**文件名**: `shield_effect.png`
```
pixel art game effect, blue energy shield bubble, glowing force field, 
circular protective barrier, side view, transparent background, 
metal slug style, high detail, 512x512
```

### 60. 火焰（新增）
**文件名**: `fire_effect.png`
```
pixel art game effect, fire flame, orange red flames, 
animated fire, side view, transparent background, 
metal slug style, high detail, 512x512
```

### 61. 冰冻效果（新增）
**文件名**: `ice_effect.png`
```
pixel art game effect, ice freeze effect, blue ice crystals, 
frozen splatter, side view, transparent background, 
metal slug style, high detail, 512x512
```

### 62. 闪电效果（新增）
**文件名**: `lightning_effect.png`
```
pixel art game effect, lightning bolt electric, yellow white electric arc, 
side view, transparent background, metal slug style, high detail, 512x512
```

### 63. 烟雾弹（新增）
**文件名**: `smoke_grenade_effect.png`
```
pixel art game effect, smoke grenade explosion, white gray smoke cloud expanding, 
side view, transparent background, metal slug style, high detail, 512x512
```

### 64. 治疗光环（新增）
**文件名**: `heal_effect.png`
```
pixel art game effect, healing green glow aura, sparkling particles, 
side view, transparent background, metal slug style, high detail, 512x512
```

### 65. 升级光环（新增）
**文件名**: `upgrade_effect.png`
```
pixel art game effect, upgrade golden glow aura, rising particles, 
side view, transparent background, metal slug style, high detail, 512x512
```

### 66. 弹幕轨迹（新增）
**文件名**: `bullet_trail.png`
```
pixel art game effect, bullet trail streak, yellow white motion line, 
horizontal, side view, transparent background, 
metal slug style, high detail, 256x256
```

---

## 七、UI 图标（20张）

### 67. 生命值图标
**文件名**: `ui_health.png`
```
pixel art UI icon, heart health symbol, red heart with white cross, 
transparent background, clean black outline, metal slug style, 256x256
```

### 68. 弹药图标
**文件名**: `ui_ammo.png`
```
pixel art UI icon, ammunition bullet icon, golden bullet, 
transparent background, clean black outline, metal slug style, 256x256
```

### 69. 分数图标
**文件名**: `ui_score.png`
```
pixel art UI icon, score star icon, golden star, 
transparent background, clean black outline, metal slug style, 256x256
```

### 70. 暂停图标
**文件名**: `ui_pause.png`
```
pixel art UI icon, pause button icon, two vertical bars, 
transparent background, clean black outline, metal slug style, 256x256
```

### 71. 播放图标
**文件名**: `ui_play.png`
```
pixel art UI icon, play button icon, green triangle, 
transparent background, clean black outline, metal slug style, 256x256
```

### 72. 设置图标
**文件名**: `ui_settings.png`
```
pixel art UI icon, settings gear icon, gray gear cog, 
transparent background, clean black outline, metal slug style, 256x256
```

### 73. 返回图标
**文件名**: `ui_back.png`
```
pixel art UI icon, back arrow icon, left pointing arrow, 
transparent background, clean black outline, metal slug style, 256x256
```

### 74. 成就图标
**文件名**: `ui_achievement.png`
```
pixel art UI icon, achievement trophy icon, golden trophy, 
transparent background, clean black outline, metal slug style, 256x256
```

### 75. 角色头像 Aila
**文件名**: `ui_avatar_aila.png`
```
pixel art UI icon, character avatar portrait, female soldier Aila, 
blue coat red scarf, brown hair, transparent background, 
clean black outline, metal slug style, 256x256
```

### 76. 角色头像 Bruno
**文件名**: `ui_avatar_bruno.png`
```
pixel art UI icon, character avatar portrait, male soldier Bruno, 
red armor, black hair beard, transparent background, 
clean black outline, metal slug style, 256x256
```

### 77. 角色头像 Mara
**文件名**: `ui_avatar_mara.png`
```
pixel art UI icon, character avatar portrait, female sniper Mara, 
green camo suit, ponytail, transparent background, 
clean black outline, metal slug style, 256x256
```

### 78. 角色头像 Niko
**文件名**: `ui_avatar_niko.png`
```
pixel art UI icon, character avatar portrait, male engineer Niko, 
purple uniform, goggles, transparent background, 
clean black outline, metal slug style, 256x256
```

### 79. 关卡选择图标
**文件名**: `ui_level_select.png`
```
pixel art UI icon, level select map icon, folded map with pin, 
transparent background, clean black outline, metal slug style, 256x256
```

### 80. 任务图标
**文件名**: `ui_mission.png`
```
pixel art UI icon, mission objective icon, clipboard with checkmark, 
transparent background, clean black outline, metal slug style, 256x256
```

### 81. 锁定图标
**文件名**: `ui_lock.png`
```
pixel art UI icon, lock padlock icon, golden lock, 
transparent background, clean black outline, metal slug style, 256x256
```

### 82. 解锁图标
**文件名**: `ui_unlock.png`
```
pixel art UI icon, unlock open padlock icon, golden open lock, 
transparent background, clean black outline, metal slug style, 256x256
```

---

## 总计：86 个资源类别，约 350+ 张图片

| 类别 | 数量 | 分辨率 |
|------|------|--------|
| 玩家角色动画 | 5角色 × (2+6+6+4+3+4+4) = 5×29 = 145 张 | 1024×1024 |
| 敌人动画 | 6敌人 × (2~4)×4 = 约 78 张 | 1024×1024 |
| Boss 动画 | 2Boss × (2+4+4+4) = 约 27 张 | 2048×2048 |
| 地形 Tile | 7地形 × 8变种 = 56 张 | 512×512 |
| 道具 | 20 张 | 512×512 |
| 背景 | 12 张 | 4096×2048 |
| 特效 | 15 张 | 256×256 / 512×512 |
| UI 图标 | 20 张 | 256×256 |
| **总计** | **约 373 张** | |

## 下载完成后

把所有图片放到：
```
d:\大模型学习\铁雨前线\Assets\Art\AI_Generated\
```

然后在 Unity 中执行：
```
Steel Rain > Check AI Art Status   # 检查资源完整性
Steel Rain > Import AI Art         # 导入资源
Steel Rain > Build All             # 重新构建场景
```

脚本会自动处理导入、配置、替换。

## 小贴士

1. **批量生成**：Bing Image Creator 每天有 15 次免费加速额度，普通速度无限
2. **去背景**：用 https://www.remove.bg 批量去除背景
3. **分批导入**：可以先生成几个测试效果，满意后再批量生成
4. **文件名必须准确**：脚本按文件名匹配，文件名错误会导致替换失败
5. **多帧动画**：动作帧按 `_0` `_1` `_2` 命名，脚本会自动识别为动画序列
6. **优先级建议**：先生成玩家角色（5×29=145张），再生成敌人，最后地形和道具
7. **分辨率提示**：Bing Image Creator 默认生成 1024×1024，可在提示词末尾强调分辨率

---

## 八、补充资源（代码引用必需，v2.1 新增 35 张）

以下资源在项目代码中被直接引用，必须生成，否则会导致运行时精灵缺失。

### 87. 玩家子弹
**文件名**: `bullet_player.png`
```
pixel art game projectile sprite, yellow glowing bullet, 
trailing light streak, side view, transparent background, 
clean black outline, metal slug style, high detail, 256x256
```

### 88. 敌人子弹
**文件名**: `bullet_enemy.png`
```
pixel art game projectile sprite, red glowing enemy bullet, 
trailing dark streak, side view, transparent background, 
clean black outline, metal slug style, high detail, 256x256
```

### 89. 炮弹（榴弹）
**文件名**: `bullet_shell.png`
```
pixel art game projectile sprite, orange artillery shell, 
grenade mortar, trailing smoke, side view, transparent background, 
clean black outline, metal slug style, high detail, 256x256
```

### 90. 天空背景（旧命名，VerticalSliceBuilder 引用）
**文件名**: `background_sky.png`
```
pixel art game background, war torn sky, dark clouds, 
orange sunset glow, distant smoke, side view, 
metal slug style, high detail, anime quality, 4096x2048
```

### 91. 海洋背景（旧命名）
**文件名**: `background_sea.png`
```
pixel art game background, ocean sea surface, waves, 
distant horizon, side view, metal slug style, 
high detail, anime quality, 4096x2048
```

### 92. 云朵背景（旧命名）
**文件名**: `background_cloud.png`
```
pixel art game background, fluffy clouds layer, 
white gray clouds, side view, transparent background, 
metal slug style, high detail, anime quality, 4096x2048
```

### 93. 山脉背景（旧命名）
**文件名**: `background_mountain.png`
```
pixel art game background, distant mountain range, 
layered peaks, fog, side view, metal slug style, 
high detail, anime quality, 4096x2048
```

### 94. 工厂背景（旧命名）
**文件名**: `background_factory.png`
```
pixel art game background, factory interior backdrop, 
pipes machinery silhouette, dark, side view, 
metal slug style, high detail, anime quality, 4096x2048
```

### 95. 城市背景（旧命名）
**文件名**: `background_city.png`
```
pixel art game background, war torn city skyline, 
destroyed buildings silhouette, smoke, side view, 
metal slug style, high detail, anime quality, 4096x2048
```

### 96. 工业背景（旧命名）
**文件名**: `background_industrial.png`
```
pixel art game background, industrial zone backdrop, 
factory chimneys, smoke stacks, side view, 
metal slug style, high detail, anime quality, 4096x2048
```

### 97. 视差远山
**文件名**: `parallax_far_mountain.png`
```
pixel art game parallax layer, distant mountains silhouette, 
snowy peaks, fog, side view, transparent background, 
metal slug style, high detail, anime quality, 4096x1024
```

### 98. 视差中景山丘
**文件名**: `parallax_mid_hill.png`
```
pixel art game parallax layer, midground rolling hills, 
green hills with distant trees, side view, transparent background, 
metal slug style, high detail, anime quality, 4096x1024
```

### 99. 视差近景树
**文件名**: `parallax_near_tree.png`
```
pixel art game parallax layer, foreground tree, 
war torn tree silhouette, side view, transparent background, 
metal slug style, high detail, anime quality, 1024x2048
```

### 100. 视差前景草
**文件名**: `parallax_foreground_grass.png`
```
pixel art game parallax layer, foreground grass bushes, 
tall grass, side view, transparent background, 
metal slug style, high detail, anime quality, 4096x512
```

### 101. UI 心形（代码命名 ui_heart）
**文件名**: `ui_heart.png`
```
pixel art UI icon, red heart symbol, health icon, 
transparent background, clean black outline, metal slug style, 256x256
```

### 102. UI 能量（代码命名 ui_energy）
**文件名**: `ui_energy.png`
```
pixel art UI icon, blue energy lightning bolt, 
transparent background, clean black outline, metal slug style, 256x256
```

### 103. UI 武器（代码命名 ui_weapon）
**文件名**: `ui_weapon.png`
```
pixel art UI icon, weapon rifle icon, assault rifle silhouette, 
transparent background, clean black outline, metal slug style, 256x256
```

### 104. UI 箭头（代码命名 ui_arrow）
**文件名**: `ui_arrow.png`
```
pixel art UI icon, white arrow pointer, right pointing arrow, 
transparent background, clean black outline, metal slug style, 256x256
```

### 105. UI 框架（代码命名 ui_frame）
**文件名**: `ui_frame.png`
```
pixel art UI element, frame border, ornate metal frame, 
transparent background, clean black outline, metal slug style, 256x256
```

### 106. 命中火花（代码命名 hit_spark）
**文件名**: `hit_spark.png`
```
pixel art game effect, bullet hit spark impact, 
yellow white sparks burst, side view, transparent background, 
metal slug style, high detail, 256x256
```

### 107. 脚印
**文件名**: `footprint.png`
```
pixel art game effect, boot footprint, muddy footprint, 
side view, transparent background, metal slug style, 128x128
```

### 108. 环形特效
**文件名**: `ring_effect.png`
```
pixel art game effect, expanding ring shockwave, 
blue energy ring, side view, transparent background, 
metal slug style, high detail, 256x256
```

### 109. 冲刺残影
**文件名**: `dash_trail.png`
```
pixel art game effect, dash motion trail, 
character afterimage, side view, transparent background, 
metal slug style, high detail, 256x256
```

### 110. 拾取光晕
**文件名**: `pickup_glow.png`
```
pixel art game effect, pickup glow aura, 
golden sparkle light, side view, transparent background, 
metal slug style, high detail, 256x256
```

### 111. 三角形（UI 箭头指示器，VerticalSliceBuilder 引用）
**文件名**: `triangle.png`
```
pixel art UI element, white filled triangle, 
simple geometric shape, transparent background, 
clean black outline, 64x64
```

### 112. 默认角色帧（VerticalSliceBuilder 引用 player_aila.png 作为默认站立帧）
**文件名**: `player_aila.png`
```
（此文件可复制 player_aila_idle_0.png 作为别名，无需单独生成）
```

### 113. Bruno 默认帧（EnhancedArtGenerator 引用）
**文件名**: `player_bruno.png`
```
（此文件可复制 player_bruno_idle_0.png 作为别名，无需单独生成）
```

### 114. Mara 默认帧（EnhancedArtGenerator 引用）
**文件名**: `player_mara.png`
```
（此文件可复制 player_mara_idle_0.png 作为别名，无需单独生成）
```

### 115. Niko 默认帧（EnhancedArtGenerator 引用）
**文件名**: `player_niko.png`
```
（此文件可复制 player_niko_idle_0.png 作为别名，无需单独生成）
```

### 116. 步枪兵默认帧（VerticalSliceBuilder 引用）
**文件名**: `enemy_rifle.png`
```
（此文件可复制 enemy_rifle_idle_0.png 作为别名，无需单独生成）
```

### 117. 盾牌兵默认帧（VerticalSliceBuilder 引用）
**文件名**: `enemy_shield.png`
```
（此文件可复制 enemy_shield_idle_0.png 作为别名，无需单独生成）
```

### 118. 无人机默认帧（VerticalSliceBuilder 引用）
**文件名**: `enemy_drone.png`
```
（此文件可复制 enemy_drone_idle_0.png 作为别名，无需单独生成）
```

### 119. 掷弹兵默认帧（EnhancedArtGenerator 引用）
**文件名**: `enemy_grenadier.png`
```
（此文件可复制 enemy_grenadier_idle_0.png 作为别名，无需单独生成）
```

### 120. 行走Boss默认帧（VerticalSliceBuilder 引用）
**文件名**: `miniboss_walker.png`
```
（此文件可复制 miniboss_walker_idle_0.png 作为别名，无需单独生成）
```

### 121. 炮台Boss默认帧（VerticalSliceBuilder 引用）
**文件名**: `turret_boss.png`
```
（此文件可复制 turret_boss_idle_0.png 作为别名，无需单独生成）
```

### 122. 火焰兵默认帧（EnhancedArtGenerator 引用）
**文件名**: `enemy_flamer.png`
```
（此文件可复制 enemy_flamer_idle_0.png 作为别名，无需单独生成）
```

---

## 九、音频资源（可选，代码已程序化生成）

项目已通过 `AudioGenerator.cs` 和 `MusicGenerator.cs` 程序化生成以下音频，如需更高质量可考虑替换：

**音效（14个）**:
- sfx_gunshot, sfx_explosion, sfx_jump, sfx_hurt, sfx_pickup, 
- sfx_upgrade, sfx_enemy_shoot, sfx_checkpoint, sfx_boss_hit, 
- sfx_dodge, sfx_form_switch, sfx_victory, sfx_gameover, sfx_ui_click

**音乐（3个）**:
- music_beach, music_village, music_boss

如需替换，将音频文件放入 `Assets/Audio/Generated/` 目录。

---

## 更新后总计

| 类别 | 数量 | 分辨率 |
|------|------|--------|
| 玩家角色动画 | 145 张 | 1024×1024 |
| 敌人动画 | 78 张 | 1024×1024 |
| Boss 动画 | 27 张 | 2048×2048 |
| 地形 Tile | 56 张 | 512×512 |
| 道具 | 20 张 | 512×512 |
| 背景 | 12 张 | 4096×2048 |
| 特效 | 15 张 | 256×256 / 512×512 |
| UI 图标 | 20 张 | 256×256 |
| 补充资源（v2.1） | 35 张 | 多种 |
| 单张默认帧（v2.2） | 12 张 | 1024×1024 / 2048×2048 |
| **总计** | **约 420 张** | |
