# Steel Rain: Frontier v2.1.0 Release Notes

Release Date: 2026-06-21

## Overview
Follow-up to the v2.0.0 simplification. Adds Endless Mode, character selection, and fixes weapon persistence bugs that broke the Metal Slug-style death penalty.

## Major Changes

### New Features
- **Endless Mode**: Wave-based survival mode. Unlocked after completing the campaign. Every 5 waves spawns a Mini-Boss Walker. Enemy count and difficulty scale with wave number
- **Character Select Screen**: Accessible from the main menu. Choose your preferred starting character (Aila, Bruno, Mara, or Niko). Selection persists between sessions via PlayerPrefs

### Bug Fixes
- **Weapon no longer persists through death**: Picking up a weapon and dying used to keep the upgraded weapon on revival. Now correctly resets to the pistol — matching the Metal Slug design
- **Character selection now applies in-game**: Previously the selection screen had no effect on gameplay. Now `PlayerSquad.Awake()` reads the preferred character ID and starts the squad with that character
- **VictoryScreen → GameCompleteScreen flow**: Campaign completion now correctly transitions to the GameCompleteScreen instead of returning directly to the menu
- **Scenes auto-added to BuildSettings**: `LevelBuilder.SaveScene()` now calls `AddSceneToBuildSettings()` to ensure generated scenes are included in builds

### File Changes

**Added:**
- `Assets/Game/Tests/EditMode/TempBuffStateTests.cs` — unit tests for the buff state static class

**Modified:**
- `Assets/Game/Player/PlayerCombat.cs` — added `ResetToStartingWeapon()` method
- `Assets/Game/Player/PlayerSquad.cs` — reads preferred character from `CharacterSelectScreen.GetPreferredCharacterId()` on new game; calls `ResetToStartingWeapon()` on revive
- `Assets/Game/UI/MainMenu.cs` — added Character Select button and screen toggle
- `Assets/Game/UI/CharacterSelectScreen.cs` — saves preferred character ID via PlayerPrefs
- `Assets/Game/Editor/LevelBuilder.cs` — added `BuildEndlessMode()`, `CreateCharacterSelectScreen()`, `AddSceneToBuildSettings()`; fixed asset paths for enemy prefabs/definitions and character definitions
- `Assets/Game/Core/LevelManager.cs` — `LoadEndlessMode()` now loads the `EndlessMode` scene
- `README.md` — documented Endless Mode, updated project structure and version history

## Gameplay Impact
- **More replayability**: Endless Mode provides score-chasing content beyond the campaign
- **Player agency**: Character select lets players start with their preferred playstyle
- **Consistent death penalty**: Weapon reset on death now works reliably across all game modes

---

# Previous Release: v2.0.0 (2026-06-21)

## Overview
Major simplification release. Removed complex progression systems (shop, currency, skills, weapon upgrades, character unlocks, NGP) to deliver classic Metal Slug-style run-and-gun gameplay. The game now focuses on tight combat, weapon pickups, and squad switching.

## Major Changes

### Removed Systems
- **Shop & Currency**: Removed `CurrencyManager`, shop UI, and all currency-related pickups and economy logic
- **Skill System**: Removed `CharacterSkill` component, skill cooldowns, skill animations, and skill-related tutorial prompts
- **Weapon Upgrades**: Removed weapon level system (1-3 upgrades). Weapons no longer have `Level`, `Upgrade()`, or `ResetUpgrades()` methods
- **Weapon Swap Pickup**: Consolidated into `WeaponPickup`. Deleted `WeaponSwapPickup.cs`
- **Character Unlocks**: All 4 characters (Aila, Bruno, Mara, Niko) are available from the start
- **NGP (New Game Plus)**: Removed permanent upgrades and NGP multipliers from enemy health/damage calculations
- **Time Rift**: Removed `TimeRiftActive`/`TimeRiftTimer` from `TempBuffState` and all slow-field references in enemy AI

### New/Changed Systems
- **Weapon Pickups**: Picking up a weapon crate now switches the player's current weapon. Death resets to the pistol
- **Simplified HUD**: Removed skill cooldown and weapon level indicators. HUD now shows health, ammo, score, and squad roster
- **TempBuffState**: Static class now only manages `Shield` and `SpeedBoost` — no more Time Rift
- **CharacterRuntime**: Stripped to essential fields — `Definition` and `CurrentHealth` only
- **WeaponRuntime**: Stripped to essential fields — no level multipliers on damage or fire rate

### File Changes

**Deleted:**
- `Assets/Game/Pickups/WeaponSwapPickup.cs`
- `Assets/Game/Player/CharacterSkill.cs` (previous release)
- `Assets/Game/Core/CurrencyManager.cs` (previous release)
- `Assets/Art/Animations/player_aila/skill.anim`

**Modified:**
- `Assets/Game/Player/CharacterRuntime.cs` — removed weaponLevels, skill cooldown
- `Assets/Game/Weapons/WeaponRuntime.cs` — removed Level, Upgrade, ResetUpgrades
- `Assets/Game/Weapons/WeaponFormDefinition.cs` — removed level multiplier fields
- `Assets/Game/Weapons/WeaponDefinition.cs` — removed level label fields
- `Assets/Game/Player/PlayerCombat.cs` — removed CurrentWeaponLevel property
- `Assets/Game/Core/TempBuffState.cs` — removed TimeRift fields and logic
- `Assets/Game/Enemies/EnemyController.cs` — removed TimeRift slow multiplier
- `Assets/Game/Enemies/DroneAI.cs` — removed TimeRift slow multiplier
- `Assets/Game/Enemies/GrenadierAI.cs` — removed TimeRift slow multiplier
- `Assets/Game/Enemies/MiniBossWalker.cs` — removed TimeRift slow multiplier
- `Assets/Game/Pickups/PickupKind.cs` — removed SpeedBoost, TimeRift kinds
- `Assets/Game/Pickups/TimedPickup.cs` — removed TimeRift expiry logic
- `Assets/Game/Editor/VerticalSliceBuilder.cs` — removed skill system setup, updated tutorial text
- `Assets/Game/Editor/AssetDatabaseGenerator.cs` — removed level label generation
- `Assets/Prefabs/Pickup_Weapon_rocket_launcher.prefab` — now references WeaponPickup
- `Assets/Prefabs/Pickup_Weapon_shotgun.prefab` — now references WeaponPickup
- `Assets/Game/Tests/EditMode/CharacterRuntimeTests.cs` — updated for simplified API
- `Assets/Game/Tests/EditMode/WeaponRuntimeTests.cs` — removed upgrade tests

## Migration Notes
- Existing save data with weapon levels or currency will be ignored on load
- Character .asset files no longer contain `skillId` or `skillCooldown` fields
- Weapon .asset files no longer contain `levelOneLabel`/`levelTwoLabel`/`levelThreeLabel` fields
- Weapon form .asset files no longer contain level multiplier fields

## Gameplay Impact
- **Faster pace**: No more grinding for currency or upgrades — pick up weapons and fight
- **Clearer feedback**: HUD shows only essential information
- **More replayability via squad**: Switching characters mid-fight is now the primary tactical depth, replacing skill usage
- **Death penalty**: Losing a weapon on death encourages careful play without being punishing

---

# Previous Release: v1.0.2 (2026-06-19)

## Critical Bug Fixes
- Level02 scene missing HUD, GameOver, Victory, BossHealthBar, and DamageIndicator UI components
- Level01 scene missing LevelEndTrigger — level end logic now complete

## Additions
- Achievement system with unlockable achievements and tracked statistics
- Achievement notification UI (slide-in animation on unlock)
- Tutorial prompt system in Level 1 (contextual control tutorials)
- Level02 now has full UI parity with Level01

---

# Previous Release: v1.0.1 (2026-06-19)

## Critical Bug Fixes
- Main menu SceneFader `fadeImage` not set — scene transitions now work correctly
- Shield pickup caused permanent invincibility — shield timer now properly counts down
- Invincible pickup caused permanent invincibility — coroutine now runs on player object
- Character switch toast displayed character name instead of skill name — now shows correct character

## Additions
- Boot screen with loading progress and random game tips
- Menu background animations (floating star particles)
- UI button hover/press visual feedback and click sound effects
- Low health vignette (red screen edge when health below 30%)
- Damage direction indicator (arrows showing where damage came from)
- Camera shake on player damage and explosions
- UI color palette system for consistent visual styling

---

# v1.0.0
- Initial release: Full 2D squad shooter with 5 levels, 4 characters, 9 enemy types
