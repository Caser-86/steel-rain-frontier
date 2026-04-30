# First Level Roadmap And Art Pipeline Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Finish a strong first-level playable slice first, while putting the full art-resource pipeline into the official project plan.

**Architecture:** Keep gameplay and art production separate but connected by stable prefab, scale, collider, and naming rules. Gameplay keeps using graybox prefabs until art is ready; art replaces visuals through child renderers, SpriteRenderer assets, animation controllers, and VFX prefabs without rewriting combat code.

**Tech Stack:** Unity 6000.3, C#, Unity UI, Unity 2D Sprite/2D Animation tools, Krita for free hand-painted source art, optional Aseprite for pixel-style VFX, optional Spine for later professional skeletal animation, Git LFS for large image/audio/source files.

---

## Current State

Project state on 2026-04-30:

- Repository: `https://github.com/Caser-86/steel-rain-frontier.git`
- Active branch: `feature/vertical-slice`
- Unity project opens and Windows build exists.
- Main menu exists: Continue, New Game, Settings, Quit.
- Save system exists: checkpoint, selected character, health, weapon id, weapon level, completion flag.
- AI advisor exists: press F1 for local tactical advice.
- First level exists as graybox and is basically playable.
- Player can move, jump, crouch, climb, shoot, dodge, switch weapons/forms, switch characters, use skills.
- Upgrade system exists: weapon upgrades up to level 3; death removes upgrades.
- Pickups exist: health, weapon upgrade, weapon pickup, destructible crates/barrels.
- First-level enemies exist: rifle, grenadier, shield, sniper, drone, flamer, mortar, crawler, mini-boss.
- Mission clear/fail feedback exists.

Main gaps still important for first level:

- Boss feedback weak: needs health bar, phase UI, weak-point feedback.
- Pause menu still missing.
- Character select/switch UI still basic.
- Enemy AI needs smarter behavior.
- Boss needs clearer phases, telegraphs, weak point, and more memorable attacks.
- First level needs pacing and balance pass after every new system.
- Art is still graybox: no final character, monster, terrain, VFX, UI icon set, or animation style.
- Audio is missing.
- Main branch has not absorbed the vertical-slice branch.

## Product Direction

Priority rule:

1. First level playable and understandable.
2. First level systems deep enough to prove the game.
3. Art production pipeline defined before final art begins.
4. One beautiful art target created first.
5. Replace graybox in controlled passes.

Not doing now:

- Full second level.
- Online co-op.
- Full campaign.
- Large story/cutscene pipeline.
- Five fully polished characters before Aila proves the style.

## Stage 1: First-Level Feedback And UI Completion

Goal: Make current first level readable and comfortable before adding more content.

Files likely touched:

- `Assets/Game/Core/GameEvents.cs`
- `Assets/Game/Enemies/MiniBossWalker.cs`
- `Assets/Game/UI/BossHealthWidget.cs`
- `Assets/Game/UI/PauseMenuController.cs`
- `Assets/Game/UI/CharacterSwitchWidget.cs`
- `Assets/Game/Editor/VerticalSliceBuilder.cs`
- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/Level01_VerticalSlice.unity`

Tasks:

- [ ] Add boss health bar at top center during mini-boss fight.
- [ ] Add boss phase label: normal, enraged, weak point open, defeated.
- [ ] Add visible weak point marker on mini-boss body.
- [ ] Add hit feedback for weak point: brighter hit spark, stronger shake, short phase message.
- [ ] Add pause menu on Esc instead of quitting immediately.
- [ ] Pause menu buttons: Resume, Restart Checkpoint, Main Menu, Quit Game.
- [ ] Add character switch HUD: current character name, health, skill cooldown, preserved upgrade status.
- [ ] Add clearer upgrade HUD: weapon level 0/1/2/3 and level effect text.
- [ ] Add checkpoint/autosave toast that does not block gameplay.
- [ ] Build Windows exe and launch smoke test after UI pass.

Acceptance:

- Player always knows boss health and current phase.
- Esc no longer traps player or exits by surprise.
- Player can see which character is active and what upgrade level is active.
- No new compile errors, no recurring runtime console errors during first-level smoke test.

## Stage 2: First-Level Combat Depth

Goal: Make fights smarter without making first level too hard.

Files likely touched:

- `Assets/Game/Enemies/EnemyController.cs`
- `Assets/Game/Enemies/EnemyAttackPattern.cs`
- `Assets/Game/Enemies/MiniBossWalker.cs`
- `Assets/Game/Levels/LevelSegmentTrigger.cs`
- `Assets/Game/Levels/DestructibleTarget.cs`
- `Assets/Game/Levels/ExplosiveBarrel.cs`
- `Assets/Game/Editor/VerticalSliceBuilder.cs`

Tasks:

- [ ] Rifle soldiers: add short cover stop and retreat behavior when player gets too close.
- [ ] Grenadiers: make grenade arc avoid impossible blind hits; add obvious warning.
- [ ] Shield soldiers: make frontal damage reduction clearer; reward crouch, jump, pierce, or explosive response.
- [ ] Snipers: add red aim line before shot, longer delay for fairness.
- [ ] Drones: add hover, dive, retreat loop instead of simple direct pressure.
- [ ] Mortar soldiers: add ground warning marker before impact.
- [ ] Crawlers: improve low-profile threat so crouch/jump decisions matter.
- [ ] Destructible crates: add more first-level placements, some empty, some health, some upgrades, some temporary invincibility.
- [ ] Explosive barrels: add clear blast range and enemy-friendly damage.
- [ ] Boss phase 1: gun burst plus stomp.
- [ ] Boss phase 2: enraged jump and five-shot burst.
- [ ] Boss phase 3: exposed core, more damage window, fewer random hits.

Acceptance:

- Enemy deaths feel readable and fair.
- Every enemy type asks for a different player answer.
- First level remains beatable with default rifle and no perfect play.
- Damage spikes come from visible warnings, not surprise overlap.

## Stage 3: First-Level Progression And Save Polish

Goal: Make the first level feel like a complete playable mission.

Files likely touched:

- `Assets/Game/Save/SaveData.cs`
- `Assets/Game/Save/SaveService.cs`
- `Assets/Game/Save/SaveGameController.cs`
- `Assets/Game/UI/MainMenuController.cs`
- `Assets/Game/UI/MissionStatusWidget.cs`
- `Assets/Game/UI/SettingsMenuController.cs`
- `Assets/Game/Editor/VerticalSliceBuilder.cs`

Tasks:

- [ ] Add pause restart from latest checkpoint.
- [ ] Add settings menu values: master volume, fullscreen/windowed, resolution preset, screen shake strength.
- [ ] Save settings separately from mission save.
- [ ] Add mission result screen after boss defeat: time, deaths, rescued NPCs, crates destroyed, upgrades found.
- [ ] Add New Game confirmation if save exists.
- [ ] Add Continue disabled state if no save exists.
- [ ] Add first-level completion save flag.
- [ ] Add level select stub that shows Level 01 unlocked and later levels locked.

Acceptance:

- Player can start, continue, pause, restart, quit to menu, and return.
- Save survives application close and reopen.
- Death/restart never leaves weapons or characters in broken state.

## Stage 4: First-Level Length And Tuning

Goal: Compress first level into a strong vertical slice, not a long empty walk.

Files likely touched:

- `Assets/Game/Editor/VerticalSliceBuilder.cs`
- `Assets/Game/Data/Levels/Level01/Waves/*.asset`
- `Assets/Game/Data/Enemies/*.asset`
- `Assets/Game/Data/Weapons/*.asset`
- `Assets/Game/Data/Characters/*.asset`

Target length:

- Current first level should be a compact 6-10 minute mission for testing.
- Later full Level 01 can expand to 12-18 minutes after art and AI prove fun.
- Original 22-28 minute dream stays long-term, not first playable milestone.

Tasks:

- [ ] No empty walking stretch longer than 8 seconds.
- [ ] Upgrade pickups appear often enough that a normal player can reach level 3 before boss.
- [ ] First level includes at least 5 weapon upgrade pickups.
- [ ] Death removes upgrades but checkpoint restart gives enough recovery options.
- [ ] Small health potions despawn after a readable delay.
- [ ] Weapon upgrade items stay forever until picked up.
- [ ] Jump heights stay character-specific but all characters can finish Level 01.
- [ ] Climb ladder remains present as a first tutorial for later climb-focused level.
- [ ] Boss arena gives enough space for crouch, jump, dodge, and weapon switching.
- [ ] Record one full manual playthrough after tuning.

Acceptance:

- New player can finish with several deaths.
- Skilled player can finish cleanly and reach level 3 before boss.
- Switching characters is useful, not required.
- First level teaches movement, crouch, climb, weapons, upgrades, destructibles, and boss reading.

## Stage 5: Art Pre-Production

Goal: Before drawing final assets, lock style, scale, naming, folders, and first asset list.

Recommended visual direction:

- High-quality 2D hand-painted characters.
- Skeleton animation for smooth, beautiful movement.
- Painted terrain with modular tiles and props.
- Pixel-like or hand-painted frame animation for explosions, muzzle flash, sparks, smoke.
- Strong silhouette and color readability over pure realism.

Reason:

- User wants beautiful characters and world.
- Hand-painted + skeletal animation is more achievable for a small team than fully hand-drawn frame-by-frame animation.
- Graybox colliders can remain stable while visuals improve.

Art tool route for beginner:

- Free first: Krita for concept art, character parts, terrain painting, UI icons.
- Unity built-in: Sprite Editor, 2D Animation, Skinning Editor, Animation window.
- Optional paid later: Aseprite for sprite sheets and frame VFX; Spine for advanced skeletal animation; TexturePacker if atlas packing becomes painful.
- Asset source license rule: only use assets with clear commercial permission; save source links and license text.

Official reference links:

- Krita: `https://krita.org/en/`
- Aseprite: `https://www.aseprite.org/`
- Unity Sprite Editor: `https://learn.unity.com/tutorial/introduction-to-sprite-editor-and-sheets`
- Unity 2D Animation rigging: `https://learn.unity.com/course/introduction-to-the-2d-animation-package-toolkit/tutorial/rigging-a-sprite-with-the-2d-animation-package`
- Spine Unity runtime: `https://en.esotericsoftware.com/spine-unity`
- TexturePacker Unity workflow: `https://www.codeandweb.com/texturepacker/for-unity`
- Kenney CC0 assets: `https://kenney.itch.io/kenney-game-assets`
- OpenGameArt license FAQ: `https://opengameart.org/content/faq`

Folders to create:

- `Assets/Art/Characters/Source`
- `Assets/Art/Characters/Exported`
- `Assets/Art/Enemies/Source`
- `Assets/Art/Enemies/Exported`
- `Assets/Art/Bosses/Source`
- `Assets/Art/Bosses/Exported`
- `Assets/Art/Environment/Level01_Beach/Source`
- `Assets/Art/Environment/Level01_Beach/Exported`
- `Assets/Art/VFX/Source`
- `Assets/Art/VFX/Exported`
- `Assets/Art/UI/Source`
- `Assets/Art/UI/Exported`
- `Assets/Art/Licenses`

Naming rules:

- Character source: `chr_aila_source_v001.kra`
- Character exported part: `chr_aila_torso_v001.png`
- Character animation clip: `chr_aila_run.anim`
- Enemy source: `enm_rifle_soldier_source_v001.kra`
- Boss source: `boss_walker_source_v001.kra`
- Terrain tileset: `env_l01_beach_tileset_v001.png`
- Prop: `prop_crate_wood_v001.png`
- VFX frame: `vfx_muzzle_flash_01_0001.png`
- UI icon: `ui_weapon_rifle_v001.png`
- License note: `license_kenney_all_in_one.md`

Scale rules:

- Keep gameplay colliders separate from art.
- Current player height is about 2.8 Unity world units.
- Use 100 pixels per Unity unit for hand-painted exports until a later art test proves a better number.
- Aila final visible height target: about 280 px at 100 PPU.
- Ordinary soldier visible height: 140-160 px.
- Mini-boss visible width: 320-420 px.
- Tile chunk unit: 1 Unity world unit equals 100 px.
- Ground tile strip height: 100 px or 200 px depending on segment.

Beginner art workflow:

- [ ] Pick one style reference board: 5 character refs, 5 terrain refs, 5 VFX refs. Store links in `Assets/Art/Licenses/art_reference_notes.md`; do not copy copyrighted images into the repo unless license allows.
- [ ] Draw 3 black silhouettes for Aila at small size. Pick the one readable at zoomed-out gameplay size.
- [ ] Draw Aila front/side concept in Krita: line art, flat colors, simple shadow.
- [ ] Split Aila into parts: head, hair front, hair back, torso, pelvis, upper arms, lower arms, hands, thighs, calves, feet, weapon.
- [ ] Export transparent PNG parts.
- [ ] Import PNG parts into Unity under `Assets/Art/Characters/Exported/Aila`.
- [ ] Set Sprite Mode to Multiple if using atlases; Single if using separate parts.
- [ ] Set Pixels Per Unit to 100.
- [ ] Use Unity Sprite Editor and Skinning Editor to create bones.
- [ ] Make first animation clips: idle, run, jump, fall, crouch, shoot, hurt, death.
- [ ] Replace only the visual child of current player prefab; keep Rigidbody2D, colliders, combat scripts unchanged.
- [ ] Test Aila in Level 01 at gameplay camera distance.
- [ ] If silhouette is muddy, fix colors and shapes before adding details.

Art acceptance:

- Character readable against beach, village, trench, and boss arena backgrounds.
- Enemy bullets remain visible.
- Player hurt state visible within 0.1 seconds.
- Crouch pose clearly lower than standing pose.
- Weapon muzzle location stable enough for bullets to originate from weapon barrel.
- Animation does not change collider size.
- No asset without license notes if downloaded from outside.

## Stage 6: First Art Target

Goal: Replace the ugliest graybox pieces with one beautiful proof slice.

First art target scope:

- Aila playable character final visual v1.
- Rifle soldier final visual v1.
- Beach ground and 5 props final visual v1.
- Muzzle flash, bullet hit spark, small explosion final visual v1.
- Health potion and weapon upgrade icon final visual v1.

Tasks:

- [ ] Make Aila concept and parts.
- [ ] Rig Aila in Unity 2D Animation.
- [ ] Create Aila animation controller.
- [ ] Make rifle soldier simple rig or sprite animation.
- [ ] Make beach terrain tiles: sand top, sand body, rock edge, metal debris, grass clump.
- [ ] Make props: crate, barrel, sandbag, warning sign, broken boat piece.
- [ ] Make VFX: muzzle flash 4-6 frames, hit spark 4 frames, explosion 8-12 frames.
- [ ] Replace graybox visual on Aila, rifle soldier, beach segment, crate, barrel.
- [ ] Keep all gameplay colliders unchanged unless visual scale forces a small correction.
- [ ] Capture screenshot before/after.

Acceptance:

- First screen no longer looks like graybox.
- Aila has visible personality.
- Rifle soldier reads as enemy immediately.
- Terrain still does not hide bullets or pickups.
- Performance remains stable.

## Stage 7: First-Level Full Art Pass

Goal: Turn Level 01 from graybox into a real level while preserving gameplay.

Content:

- Characters: Aila full, Bruno/Mara/Niko readable placeholders with distinct colors and silhouettes.
- Enemies: all Level 01 enemies at production placeholder quality.
- Boss: mini-boss walker final visual v1 with weak point and damaged states.
- Terrain: beach, village, trench, boss arena.
- Props: crates, barrels, ladders, pickups, rescue NPC, weapon caches.
- UI: weapon icons, health icons, skill icons, upgrade icons, character portraits.
- VFX: muzzle flashes, bullets, explosions, smoke, sparks, weak-point hit, upgrade pickup.

Tasks:

- [ ] Replace all enemy graybox bodies with distinct visuals.
- [ ] Replace boss body with mechanical walker art.
- [ ] Add boss damage-state overlays: normal, cracked armor, exposed core.
- [ ] Replace Level 01 block colors with modular terrain sprites.
- [ ] Add non-colliding background layers: sky, sea, village silhouettes, trench smoke.
- [ ] Add foreground props with clear parallax but low gameplay obstruction.
- [ ] Replace HUD text-only weapon display with icons plus text.
- [ ] Add art import presets and document exact settings.
- [ ] Run full Level 01 readability pass.

Acceptance:

- A screenshot communicates genre immediately: side-scrolling action shooter.
- Player, enemies, bullets, pickups, and platforms are readable at 1920x1080.
- No art asset changes core gameplay unless intentionally approved.

## Stage 8: Audio And Feel Polish

Goal: Give the first level impact after UI, combat, save, and art are stable.

Files likely touched:

- `Assets/Game/Audio`
- `Assets/Game/VFX`
- `Assets/Game/Weapons`
- `Assets/Game/Enemies`
- `Assets/Game/Editor/VerticalSliceBuilder.cs`

Tasks:

- [ ] Add temporary Unity AudioSource wrapper.
- [ ] Weapon sounds: rifle, shotgun, rocket, pickup, upgrade.
- [ ] Character sounds: hurt, death, skill trigger, switch.
- [ ] Enemy sounds: alert, shoot, death.
- [ ] Boss sounds: stomp, phase change, weak point, death.
- [ ] UI sounds: menu select, pause, save toast.
- [ ] Music placeholder: menu loop, Level 01 loop, boss loop.
- [ ] Add screen shake settings slider.
- [ ] Add hit stop only for big hits, not every bullet.

Acceptance:

- Weapon types sound distinct.
- Boss phase change feels big.
- Audio never hides warning cues.

## Stage 9: Verification, Branch Merge, And Public Build

Goal: Make the branch ready to merge and share.

Tasks:

- [ ] Run EditMode tests.
- [ ] Run PlayMode smoke tests if stable.
- [ ] Build MainMenu and Level01 scenes from editor builder.
- [ ] Build Windows exe.
- [ ] Launch exe smoke test.
- [ ] Full manual first-level playthrough.
- [ ] Update README with controls, build path, save path, and known gaps.
- [ ] Merge `feature/vertical-slice` into main after user approval.
- [ ] Tag first playable build: `v0.1.0-first-level-slice`.

Acceptance:

- Fresh clone can open project and build.
- Windows exe launches into menu, starts Level 01, saves, continues, and reaches mission clear.
- GitHub branch contains docs, source, Unity scenes, and reproducible builder code.

## Immediate Next Task Order

Recommended order from today:

1. Boss health bar, phase UI, weak point feedback.
2. Pause menu and restart checkpoint.
3. Character switch HUD and upgrade HUD polish.
4. Enemy warning/AI pass.
5. Boss phase pass.
6. First-level tuning pass.
7. Art folder structure and import settings.
8. Aila first-art target.
9. Rifle soldier and beach art target.
10. Audio placeholder pass.
11. Full verification and merge plan.

## Beginner Art Learning Path

Week 1:

- Install Krita from official site.
- Learn layers, brush, eraser, selection, transform, export PNG.
- Draw only silhouettes and simple flat colors.
- Goal: one readable Aila concept, not beautiful final art.

Week 2:

- Split Aila into body parts.
- Import parts into Unity.
- Learn Sprite Editor and bones.
- Goal: idle and run animation in Unity.

Week 3:

- Add crouch, jump, shoot, hurt.
- Replace graybox player visual.
- Goal: playable Aila in first level.

Week 4:

- Draw one enemy and one terrain set.
- Replace rifle soldier and beach ground.
- Goal: first screen looks like a real game.

Rules for learning:

- Do not start with five characters.
- Do not polish tiny details before silhouette works.
- Do not change colliders to match art unless gameplay still works.
- Save source files, exports, and license notes every time.
- Use version numbers instead of overwriting important art files.

## Self-Review

Coverage check:

- Art resources are now part of official planning.
- Beginner art workflow starts from tool choice, folders, naming, scale, drawing, export, Unity import, rigging, and replacement.
- First-level priority stays ahead of second-level content.
- Current completed systems are reflected.
- Next phases are ordered from playable feedback to combat, save polish, tuning, art, audio, verification.

Ambiguity choices:

- First art method is hand-painted 2D plus skeletal animation, not pure pixel art.
- Aila is first final-art target.
- Full Level 01 is compressed for current playable milestone before expanding.
- Downloaded assets require license notes before shipping.
