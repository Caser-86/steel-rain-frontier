# Sprite Sheet 生成提示词清单（v3.0）

> 本清单用于 AI 生成 Sprite Sheet（大图含多帧），导入时由 `SpriteSheetImporter.cs` 自动切割。
> v3.0 对齐代码中实际角色（Aila/Bruno/Mara/Niko），补全逐帧提示词。
> **25 张大图，326 个子精灵**。

---

## 使用方法

1. 复制提示词到 **ZSky AI**（https://zsky.ai/ ，无限免费）或 **Bing Image Creator**
2. 下载图片，**重命名为指定文件名**
3. 放入 `Assets/Art/AI_Generated/`
4. Unity 菜单 `Steel Rain > Import Sprite Sheets`
5. 执行 `Steel Rain > Build All`

---

## Sprite Sheet 布局规范

### 玩家角色双 4×4 Sheet（30有效帧 + 2弃用占位）
- **画布**：1024×1024 | **单帧**：256×256

**Sheet 1 - 移动动作**：
```
┌──────┬──────┬──────┬──────┐
│idle_0│idle_1│walk_0│walk_1│  Row 0
├──────┼──────┼──────┼──────┤
│walk_2│walk_3│run_0 │run_1 │  Row 1
├──────┼──────┼──────┼──────┤
│run_2 │run_3 │jump_0│jump_1│  Row 2
├──────┼──────┼──────┼──────┤
│jump_2│jump_3│dash_0│_unused│  Row 3（右下角放水印）
└──────┴──────┴──────┴──────┘
```

**Sheet 2 - 战斗/武器**：
```
┌──────┬──────┬──────┬──────┐
│crouch0│crouch1│crouch2│prone_0│  Row 0
├──────┼──────┼──────┼──────┤
│shoot_0│shoot_1│shoot_2│shoot_3│  Row 1
├──────┼──────┼──────┼──────┤
│death_0│death_1│death_2│death_3│  Row 2
├──────┼──────┼──────┼──────┤
│_unused│pistol_0│knife_0│_unused│  Row 3（右下角放水印）
└──────┴──────┴──────┴──────┘
```

> 右下角 `_unused` 专门放置 AI 水印或对齐容错，SpriteSheetImporter 自动跳过。

### Boss 4×4 网格（16帧，2048×2048，单帧 512×512）

### 地形 1×8 横排（8变种，4096×512，单帧 512×512）
- 布局：`[plain][left][right][mid][single][corner_l][corner_r][slope]`

---

## 一、玩家角色（4角色 × 2张 = 8张）

### 公共提示词前缀

以下前缀在所有玩家角色的每张 Sheet 中**必须包含**：

```
pixel art sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different, pixel-perfect, consistent art style
```

---

### 1. Aila — 全能型突击手

**角色设定**：前联邦特种部队队长，冷静果断。棕色头发，蓝色风衣，红色围巾，黑色军靴。持有突击步枪。

#### Sheet 1 `player_aila_sheet1.png` — 移动

```
Aila movement sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, female soldier, brown hair, blue trench coat, red scarf,
black boots, holding assault rifle, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 idle_0: stand tall, breathing in, chest expanded, rifle barrel up 10deg, scarf hangs still
2 idle_1: stand tall, breathing out, chest relaxed, rifle horizontal, eyes half-closed
3 walk_0: left foot forward flat, right foot behind on toes, left arm swings forward
4 walk_1: right foot forward heel landing, left foot back flat, right arm swings forward
5 walk_2: left foot planted, right knee lifting up, both hands grip rifle, barrel level
6 walk_3: right heel down, left toes push off behind, scarf trails left, hair bounces
7 run_0: sprint lean forward, left leg far back, right knee high, arms pump
8 run_1: sprint push off right leg back, left leg high forward, rifle aimed
9 run_2: airborne, left knee tucked, right leg trailing, body tilt 20deg
10 run_3: landing left toe first, knees bent shock, rifle stable
11 jump_0: crouch 90deg preparing jump, rifle angled up 45deg
12 jump_1: ascending, legs extended pushing off, rifle across chest
13 jump_2: jump peak, legs tucked to chest, rifle above head
14 jump_3: descending, legs extending down, one arm balances
15 dash_0: dash dodge, body low sideways lean, rifle held close, one leg kicked back
16 unused: empty corner, ignore
```

#### Sheet 2 `player_aila_sheet2.png` — 战斗

```
Aila combat sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, female soldier, brown hair, blue trench coat, red scarf,
black boots, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 crouch_0: crouch down, knees bent 90deg, rifle held ready at shoulder, body low
2 crouch_1: crouch idle, breathing in low stance, rifle barrel up slightly, eyes alert
3 crouch_2: crouch shuffle forward, left knee up, right knee down, rifle steady aim
4 prone_0: lying prone on belly, rifle forward both hands, legs flat behind, head up aiming
5 shoot_0: aim rifle, eye at sight, barrel level, steady stance
6 shoot_1: fire rifle, muzzle flash, recoil pushes shoulder back, shell ejects
7 shoot_2: recoil peak, torso pushed back 10deg, rifle barrel jumps up, eyes squint
8 shoot_3: recover from recoil, barrel returning down, smoke from barrel
9 death_0: hit reaction, torso jerks back, mouth open, rifle starts to drop
10 death_1: falling backward 30deg, arms flailing, scarf flying up
11 death_2: falling backward 60deg, rifle falling from hands, eyes closing
12 death_3: lying on back, limbs spread, eyes closed, rifle on ground beside
13 unused: empty corner, ignore
14 pistol_0: switch to pistol, right hand holding handgun forward, left hand steadying
15 knife_0: switch to combat knife, lunging forward with knife stab, body low
16 unused: empty corner, ignore
```

---

### 2. Bruno — 重装防御者

**角色设定**：前重装步兵，战场"移动堡垒"。厚重装甲，大盾牌，霰弹枪。沉默寡言。

#### Sheet 1 `player_bruno_sheet1.png` — 移动

```
Bruno movement sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, bulky male soldier, shaved head, dark green heavy armor,
large metal shield on back, combat boots, holding shotgun, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 idle_0: stand wide stance, shotgun resting on shoulder, shield on back, heavy breathing
2 idle_1: shift weight to left foot, shotgun lowered, slight head nod
3 walk_0: heavy left foot step forward, ground trembles, shotgun held across chest
4 walk_1: right foot lands flat, dust kicks up, shield bounces on back
5 walk_2: left foot planted wide, shotgun barrel up, arms tense
6 walk_3: right foot forward, body rocks side to side from heavy armor weight
7 run_0: lumbering run, left leg far back, shotgun pointed forward, armor rattles
8 run_1: push off right leg, heavy footfall, shield shifts on back
9 run_2: airborne moment, both feet off ground, shotgun aimed forward, armor weight visible
10 run_3: heavy landing, knees bent deep, dust burst from impact
11 jump_0: crouch low, shotgun grip tight, shield strap visible, preparing leap
12 jump_1: pushing off with both legs, shotgun across body, heavy armor airborne
13 jump_2: jump peak, body compact, shotgun held close, shield on back
14 jump_3: descending, feet reaching for ground, arms spread for balance
15 dash_0: shield bash forward, body behind shield, shotgun tucked under arm
16 unused: empty corner, ignore
```

#### Sheet 2 `player_bruno_sheet2.png` — 战斗

```
Bruno combat sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, bulky male soldier, dark green heavy armor, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 crouch_0: crouch behind shield, shotgun peeking over shield top, defensive
2 crouch_1: crouch idle, shield up, breathing heavy, eyes scanning
3 crouch_2: crouch shuffle, shield forward, shotgun at hip, low profile
4 prone_0: lying prone, shield beside body, shotgun aimed forward, heavy boots behind
5 shoot_0: aim shotgun, wide stance, both hands on weapon, body leaning back
6 shoot_1: fire shotgun, massive muzzle flash, recoil pushes body back 15deg, smoke
7 shoot_2: recoil peak, shotgun barrel high, smoke cloud, body recovering
8 shoot_3: shotgun returning to ready, smoke clearing, shell ejecting
9 death_0: hit in chest, body jerks, shotgun starts to fall, shield clatters
10 death_1: falling backward, arms spread, shotgun airborne, armor scraping
11 death_2: hitting ground, body flat, shield beside, eyes closing
12 death_3: lying on ground, limbs spread, shield and shotgun on ground
13 unused: empty corner, ignore
14 pistol_0: drop shield, pull sidearm, one hand aim
15 knife_0: knife charge, shield arm forward, knife in other hand, aggressive
16 unused: empty corner, ignore
```

---

### 3. Mara — 远程狙击手

**角色设定**：天才狙击手，前奥运射击冠军。长发马尾，深色战术装，狙击步枪。

#### Sheet 1 `player_mara_sheet1.png` — 移动

```
Mara movement sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, lean female soldier, long brown hair in ponytail,
dark grey tactical outfit, sniper rifle with scope, slim build, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 idle_0: stand relaxed, sniper rifle slung on back, ponytail swaying slightly
2 idle_1: shift weight, hand on rifle scope, eyes scanning horizon
3 walk_0: light left foot step, rifle held in one hand, agile movement
4 walk_1: right foot forward heel, body stays low, ponytail swings
5 walk_2: left foot planted, right knee lifts, rifle ready at waist
6 walk_3: right foot lands, left toe pushes off, ponytail trails behind
7 run_0: sprint, body low and forward, rifle pointed ahead, ponytail streams
8 run_1: push off, lean angle 30deg, rifle aimed down range
9 run_2: airborne, knees tucked, rifle horizontal, ponytail vertical
10 run_3: landing light on toes, crouch absorption, rifle steady
11 jump_0: crouch preparing, rifle grip tight, legs coiled
12 jump_1: ascending, legs pushing off, rifle across body
13 jump_2: peak, body compact, rifle above head, ponytail whipping
14 jump_3: descending, legs extending, one arm for balance
15 dash_0: quick dodge roll, body low, rifle close to body, agile
16 unused: empty corner, ignore
```

#### Sheet 2 `player_mara_sheet2.png` — 战斗

```
Mara combat sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, lean female soldier, dark grey tactical outfit,
sniper rifle with scope, ponytail, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 crouch_0: crouch low, sniper rifle on shoulder, scope up, spotting
2 crouch_1: crouch steady, rifle barrel level, breathing controlled
3 crouch_2: crouch shift, adjusting aim, left hand on bipod
4 prone_0: lying prone, rifle on bipod, eye through scope, legs flat behind
5 shoot_0: aim through scope, finger on trigger, breath held
6 shoot_1: fire sniper, massive recoil, rifle jumps, muzzle flash, scope flash
7 shoot_2: recoil peak, body pushed back, rifle barrel high, bolt cycling
8 shoot_3: bolt pulled, spent casing ejecting, smoke from barrel
9 death_0: hit reaction, body jerks sideways, rifle dropping, hair flying
11 death_1: falling, arms reaching, rifle separate, ponytail whipping
12 death_2: mid-fall, body twisted, eyes closing, rifle on ground
13 death_3: lying on side, rifle beside, ponytail spread, eyes closed
13 unused: empty corner, ignore
14 pistol_0: quick draw pistol, one hand aim, sharp stance
15 knife_0: quick knife slash, forward lean, agile strike
16 unused: empty corner, ignore
```

---

### 4. Niko — 时空操控者

**角色设定**：神秘少年黑客，操控时间流速。银白短发，黑色紧身衣，紫色能量武器。

#### Sheet 1 `player_niko_sheet1.png` — 移动

```
Niko movement sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, young male hacker, short silver-white hair,
black tactical bodysuit with purple glow accents, energy pistol, slim agile build,
metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 idle_0: stand casual, energy pistol at side, purple glow pulsing, head slightly tilted
2 idle_1: shift weight, check holographic wrist display, eyes scanning data
3 walk_0: light step, pistol at waist, purple energy trail, effortless gait
4 walk_1: right foot forward, body stays vertical, energy particles trailing
5 walk_2: left foot planted, right knee lifts, pistol ready, glowing accents
6 walk_3: right foot down, casual stride, hair bouncing, energy trail
7 run_0: fast sprint, body lean forward, pistol aimed, purple blur
8 run_1: push off, agile and light, energy trail streams behind
9 run_2: airborne, legs tucked, pistol horizontal, energy crackling
10 run_3: light landing, barely a impact, energy stabilizes
11 jump_0: prepare, legs coiled, energy gathering around feet
12 jump_1: ascending, energy boost from feet, anti-gravity style
13 jump_2: peak, body floating, purple energy aura, hair floating
14 jump_3: descending, controlled fall, energy dissipating
15 dash_0: time-dodge, body blurs with purple afterimage, teleport-like
16 unused: empty corner, ignore
```

#### Sheet 2 `player_niko_sheet2.png` — 战斗

```
Niko combat sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, young male hacker, silver-white hair, black bodysuit,
purple energy weapon, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 crouch_0: crouch low, energy pistol aimed, purple glow intense, calculating
2 crouch_1: crouch idle, holographic display projected, scanning
3 crouch_2: crouch forward, pistol extended, energy charging
4 prone_0: lying prone, pistol forward, energy trail along ground, stealth
5 shoot_0: aim energy pistol, purple charge building, steady
6 shoot_1: fire energy bolt, purple blast, recoil minimal, muzzle energy
7 shoot_2: energy bolt flying, recoil absorbed, glow fading
8 shoot_3: energy recharging, pistol smoking purple, ready again
9 death_0: hit, body glitches with purple static, staggering
10 death_1: falling, energy flickering, body phasing, hair wild
11 death_2: hitting ground, energy dissipating, body crumpling
12 death_3: lying still, purple glow fading, eyes closed
13 unused: empty corner, ignore
14 pistol_0: switch to backup, energy knife appearing, close combat
15 knife_0: energy blade slash, purple arc trail, aggressive lunge
16 unused: empty corner, ignore
```

---

## 二、敌人（6种 × 1张 = 6张）

### 公共提示词前缀

```
pixel art sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different
```

---

### 5. 步枪兵 `enemy_rifle_sheet.png`

```
pixel art sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, enemy soldier, red military uniform with helmet,
holding assault rifle, aggressive stance, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 idle_0: stand alert, rifle at ready, scanning
2 idle_1: rifle slightly lowered, breathing
3 walk_0: patrol left foot forward, rifle aimed
4 walk_1: right foot forward, scanning area
5 walk_2: left foot plant, head turning
6 walk_3: right foot, body rock
7 patrol_0: walking with rifle lowered
8 patrol_1: walking, head on swivel
9 alert_0: stop, rifle up, spotted enemy
10 alert_1: freeze, weapon raised, tense
11 alert_2: crouch ready, aiming
12 alert_3: full aim stance, tracking
13 shoot_0: fire rifle, muzzle flash
14 death_0: hit reaction, staggering
15 death_1: falling, dropping weapon
16 unused: empty corner, ignore
```

### 6. 盾牌兵 `enemy_shield_sheet.png`

```
pixel art sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, enemy shield soldier, heavy red armor, large metal shield,
baton weapon, defensive stance, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 idle_0: shield up, baton ready, wide stance
2 idle_1: shield slightly angled, breathing
3 walk_0: shield forward, baton at side
4 walk_1: shield up while walking, heavy steps
5 shield_0: shield raised high, blocking
6 shield_1: shield absorbing hit, sparks
7 shield_2: shield pushing forward, aggressive block
8 shield_3: shield bash motion
9 charge_0: shield forward, running start
10 charge_1: full charge, shield lowered like battering ram
11 charge_2: impact moment, shield hitting
12 charge_3: follow-through, recovery
13 attack_0: baton swing, shield to side
14 death_0: hit through shield, staggering
15 death_1: falling, shield dropping
16 unused: empty corner, ignore
```

### 7. 无人机 `enemy_drone_sheet.png`

```
pixel art sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view, enemy combat drone, flying machine, propellers, laser cannon,
metallic gray with red accents, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 hover_0: hovering, propellers spinning, level
2 hover_1: hovering, slight bob, propellers blur
3 move_0: moving right, tilted forward, propellers angled
4 move_1: moving fast, blur trail, speed lines
5 chase_0: pursuing, angled down, laser charging
6 chase_1: dive start, body tilts 45deg
7 dive_0: full dive, body vertical, speed lines
8 dive_1: dive impact, sparks, explosion
9 aim_0: hovering, laser targeting player
9 aim_1: laser locked, red beam
10 shoot_0: fire laser, beam visible
11 shoot_1: laser beam retracting
12 explode_0: taking damage, sparks, smoke
13 explode_1: explosion expanding, debris
14 destroy: breaking apart, pieces flying
15 _unused: empty corner, ignore
```

### 8. 掷弹兵 `enemy_grenadier_sheet.png`

```
pixel art sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, enemy grenadier, green military vest, ammo belts,
grenade launcher, stocky build, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 idle_0: stand with launcher, ammo belts visible
2 idle_1: weight shift, launcher lowered
3 walk_0: heavy step forward, launcher at side
4 walk_1: right foot, body sway from weight
5 aim_0: raise launcher, sight down barrel
6 aim_1: aiming at arc angle, calculating
7 throw_0: wind up, arm back, grenade visible
8 throw_1: release, arm forward, grenade flying
9 fire_0: launcher recoil, smoke puff
10 fire_1: smoke clearing, reloading
11 reload_0: open launcher, insert grenade
12 reload_1: close launcher, ready
13 shoot_0: fire grenade, muzzle flash
14 death_0: hit, dropping launcher
15 death_1: falling backward
16 unused: empty corner, ignore
```

### 9. 火焰兵 `enemy_flamer_sheet.png`

```
pixel art sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, enemy flamethrower soldier, orange fireproof suit,
gas tank on back, flamethrower weapon, menacing stance, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 idle_0: stand with flamethrower, gas tank visible, heavy breathing
2 idle_1: shift weight, fuel sloshing in tank
3 walk_0: step forward, flamethrower at hip
4 walk_1: right foot, tank bouncing
5 charge_0: running forward, flamethrower raised
6 charge_1: full charge, body low, aggressive
7 flame_0: ignite, small flame burst from nozzle
8 flame_1: full flame stream, fire cone
9 flame_2: flame sustaining, heat distortion
10 flame_3: flame ending, smoke trail
11 fire_0: fire burst, muzzle flash, fire particles
12 fire_1: fire aftermath, embers
13 attack_0: close range flame sweep
14 death_0: hit, tank rupturing
15 death_1: falling, fire spreading
16 unused: empty corner, ignore
```

### 10. 精英兵 `enemy_elite_sheet.png`

```
pixel art sprite sheet, 4x4 grid, 256x256 per cell, 1024x1024 total,
side view facing right, enemy elite soldier, black tactical armor, red visor,
advanced rifle, agile stance, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 idle_0: alert stance, rifle ready, scanning
2 idle_1: slight movement, rifle tracking
3 walk_0: tactical walk, rifle aimed forward
4 walk_1: careful step, checking corners
5 run_0: sprint, rifle forward, fast
6 run_1: agile run, body lean
7 dodge_0: dodge roll start, body low
8 dodge_1: mid roll, tuck and tumble
9 dodge_2: roll end, recovering stance
10 dodge_3: back on feet, rifle ready
11 shoot_0: precise aim, steady
12 shoot_1: fire, muzzle flash, controlled
13 death_0: hit reaction, stagger
14 death_1: armor sparking, falling
15 _unused: empty corner, ignore
```

---

## 三、Boss（2种 × 1张 = 2张）

### 公共提示词前缀

```
pixel art sprite sheet, 4x4 grid, 512x512 per cell, 2048x2048 total,
side view, metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different
```

---

### 11. 行走者机甲（Mini-Boss） `miniboss_walker_sheet.png`

```
pixel art sprite sheet, 4x4 grid, 512x512 per cell, 2048x2048 total,
side view, giant walking mech boss, bipedal war machine, heavy armor,
missile pods on shoulders, machine gun arms, menacing red eye visor,
metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 idle_0: standing, slight sway, missiles visible, red eye scanning
2 idle_1: weight shift, armor plates adjusting
3 walk_0: left leg forward, heavy footstep, ground shake
4 walk_1: right leg forward, missile pods bouncing
5 charge_0: lean forward, arms back, charging
6 charge_1: full charge, feet digging ground, dust
7 charge_2: charge impact, sparks flying
8 charge_3: recoil, recovery
9 missile_0: missile pod opening, targeting
9 missile_1: missile launch, smoke trail
10 missile_2: missiles flying, multiple trails
11 missile_3: missiles impacting, explosions
12 machinegun_0: machine gun firing, muzzle flashes
12 machinegun_1: sustained fire, shells ejecting
13 damage_0: taking hit, armor sparking
14 damage_1: heavy damage, parts flying
15 explode_0: critical damage, explosion starting
16 _unused: empty corner, ignore
```

### 12. 炮塔Boss `turret_boss_sheet.png`

```
pixel art sprite sheet, 4x4 grid, 512x512 per cell, 2048x2048 total,
side view, giant turret boss, stationary fortress, multiple cannons,
rotating top section, laser eye core, heavy armor plating,
metal slug style, 1px black outline,
PURE transparent background, no gray background, no ground shadows,
NO frame labels, NO F numbers, NO letters, NO text anywhere, NO watermarks,
all 16 frames clearly different:

1 idle_0: base stationary, turret rotating slowly, cannons tracking
2 idle_1: turret stops, aiming at player
3 aim_0: turret aligning, laser charging
4 aim_1: laser locked on target, beam visible
5 cannon_0: cannon firing, massive muzzle flash, recoil
5 cannon_1: shell ejecting, smoke, second cannon firing
6 cannon_2: rapid fire, multiple flashes
6 cannon_3: cannons cooling, smoke
7 laser_0: laser core charging, purple glow building
8 laser_1: laser beam firing, massive energy beam
9 laser_2: laser sustained, ground burning
10 laser_3: laser ending, aftermath
11 charge_0: energy building, armor plates opening
12 charge_1: full charge, all weapons firing
13 damage_0: armor cracking, sparks
14 damage_1: heavy damage, fire, smoke
15 explode_0: massive explosion, core detonating
16 _unused: empty corner, ignore
```

---

## 四、地形（7种 × 1张 = 7张）

### 公共提示词前缀

```
pixel art tile sheet, 8 tiles in a single row,
side view, metal slug style, high detail,
PURE transparent background, seamless tiling,
NO frame labels, NO text, NO watermarks
```

---

### 13. 海滩 `ground_beach_sheet.png`

```
pixel art tile sheet, 8 tiles in a single row,
beach terrain variants, sand with seashells and driftwood,
tile 1: plain, tile 2: left edge, tile 3: right edge,
tile 4: middle, tile 5: single, tile 6: corner left,
tile 7: corner right, tile 8: slope,
each tile 512x512, total 4096x512,
transparent background, seamless tiling,
metal slug style, high detail, no labels, no text
```

### 14. 村庄 `ground_village_sheet.png`

```
pixel art tile sheet, 8 tiles in a single row,
village terrain variants, cobblestone with grass and flowers,
tile 1: plain, tile 2: left edge, tile 3: right edge,
tile 4: middle, tile 5: single, tile 6: corner left,
tile 7: corner right, tile 8: slope,
each tile 512x512, total 4096x512,
transparent background, seamless tiling,
metal slug style, high detail, no labels, no text
```

### 15. 战壕 `ground_trench_sheet.png`

```
pixel art tile sheet, 8 tiles in a single row,
trench terrain variants, muddy ground with sandbags and barbed wire,
tile 1: plain, tile 2: left edge, tile 3: right edge,
tile 4: middle, tile 5: single, tile 6: corner left,
tile 7: corner right, tile 8: slope,
each tile 512x512, total 4096x512,
transparent background, seamless tiling,
metal slug style, high detail, no labels, no text
```

### 16. 工厂 `ground_factory_sheet.png`

```
pixel art tile sheet, 8 tiles in a single row,
factory terrain variants, metal floor with pipes and grates,
tile 1: plain, tile 2: left edge, tile 3: right edge,
tile 4: middle, tile 5: single, tile 6: corner left,
tile 7: corner right, tile 8: slope,
each tile 512x512, total 4096x512,
transparent background, seamless tiling,
metal slug style, high detail, no labels, no text
```

### 17. 城市 `ground_city_sheet.png`

```
pixel art tile sheet, 8 tiles in a single row,
city terrain variants, cracked asphalt with debris and rebar,
tile 1: plain, tile 2: left edge, tile 3: right edge,
tile 4: middle, tile 5: single, tile 6: corner left,
tile 7: corner right, tile 8: slope,
each tile 512x512, total 4096x512,
transparent background, seamless tiling,
metal slug style, high detail, no labels, no text
```

### 18. 工业 `ground_industrial_sheet.png`

```
pixel art tile sheet, 8 tiles in a single row,
industrial terrain variants, concrete floor with oil stains and metal plates,
tile 1: plain, tile 2: left edge, tile 3: right edge,
tile 4: middle, tile 5: single, tile 6: corner left,
tile 7: corner right, tile 8: slope,
each tile 512x512, total 4096x512,
transparent background, seamless tiling,
metal slug style, high detail, no labels, no text
```

### 19. 雪地 `ground_snow_sheet.png`

```
pixel art tile sheet, 8 tiles in a single row,
snow terrain variants, white snow with ice crystals and footprints,
tile 1: plain, tile 2: left edge, tile 3: right edge,
tile 4: middle, tile 5: single, tile 6: corner left,
tile 7: corner right, tile 8: slope,
each tile 512x512, total 4096x512,
transparent background, seamless tiling,
metal slug style, high detail, no labels, no text
```

---

## 五、道具/拾取物（4张）

> 这些是独立小图，不是 Sheet。AI 生成单张后直接使用。

### 20. 武器拾取物 `weapon_pickup.png`

```
pixel art weapon pickup crate, single item, 256x256,
glowing yellow-orange crate with weapon icon inside, metallic casing,
sci-fi supply box aesthetic, metal slug style, 1px black outline,
PURE transparent background, centered, no text, no labels
```

### 21. 生命药剂 `health_pickup.png`

```
pixel art health pickup, single item, 256x256,
red cross health pack, glowing, medical supplies aesthetic,
metal slug style, 1px black outline,
PURE transparent background, centered, no text, no labels
```

### 22. 军票 `currency_pickup.png`

```
pixel art currency pickup, single item, 256x256,
glowing golden military dog tag or coin, energy particles around it,
metal slug style, 1px black outline,
PURE transparent background, centered, no text, no labels
```

### 23. 存档检查点 `checkpoint_flag.png`

```
pixel art checkpoint flag, single item, 256x256,
military flag on a pole, glowing beacon on top, sandbag base,
metal slug style, 1px black outline,
PURE transparent background, centered, no text, no labels
```

---

## 六、UI 元素（6张）

### 24-29. UI 图标

```
pixel art UI icon set, each 64x64, clean and readable at small size,
metal slug military style, consistent color palette:

1 ui_heart.png: red heart health icon
2 ui_ammo.png: bullet/ammo icon
3 ui_weapon.png: rifle weapon icon
4 ui_arrow.png: directional arrow indicator
```

---

## 总计

| 类别 | 数量 | 子精灵 |
|------|------|--------|
| 玩家角色（4×2张） | 8 张 | 120 个 |
| 敌人（6×1张） | 6 张 | 90 个 |
| Boss（2×1张） | 2 张 | 30 个 |
| 地形（7×1张） | 7 张 | 56 个 |
| 道具/拾取物 | 4 张 | 4 个 |
| UI 元素 | 6 张 | 6 个 |
| **合计** | **33 张** | **306 个** |

---

## 单张默认帧（可选，导入器自动生成）

| 文件名 | 来源 |
|--------|------|
| player_aila.png | player_aila_sheet1 的 idle_0 |
| player_bruno.png | player_bruno_sheet1 的 idle_0 |
| player_mara.png | player_mara_sheet1 的 idle_0 |
| player_niko.png | player_niko_sheet1 的 idle_0 |
| enemy_rifle.png | enemy_rifle_sheet 的 idle_0 |
| enemy_shield.png | enemy_shield_sheet 的 idle_0 |
| enemy_drone.png | enemy_drone_sheet 的 idle_0 |
| enemy_grenadier.png | enemy_grenadier_sheet 的 idle_0 |
| enemy_flamer.png | enemy_flamer_sheet 的 idle_0 |
| enemy_elite.png | enemy_elite_sheet 的 idle_0 |
| miniboss_walker.png | miniboss_walker_sheet 的 idle_0 |
| turret_boss.png | turret_boss_sheet 的 idle_0 |

---

## 生成优先级

1. **第一批（测试验证）**：`player_aila_sheet1.png` + `player_aila_sheet2.png`
2. **第二批（玩家角色）**：Bruno/Mara/Niko（6张）+ 4个道具/拾取物
3. **第三批（敌人）**：6个敌人（6张）
4. **第四批（Boss）**：2个Boss（2张）
5. **第五批（地形）**：7个地形（7张）+ 6个UI元素

## 推荐工具

| 工具 | 特点 | 链接 |
|------|------|------|
| ZSky AI | 无限免费，1080p | https://zsky.ai/ |
| Bing Image Creator | 每天15次加速 | https://www.bing.com/images/create |
| Easy-Peasy.AI | 专做 sprite sheet | https://easy-peasy.ai/ai-image-generator/sprite-sheet |

## 注意事项

1. **文件名必须准确**：角色以 `_sheet1.png`/`_sheet2.png` 结尾，敌人/地形以 `_sheet.png` 结尾
2. **网格必须对齐**：AI 生成的可能不严格对齐，需检查后用 Photoshop/GIMP 微调
3. **背景必须透明**：提示词已强调 `PURE transparent background`
4. **无文字标签**：提示词已强调 `NO labels, NO text, NO watermarks`
5. **如果网格不齐**：用 https://www.remove.bg 去背景后重新拼接
