# Steel Rain: Frontier

A 2D side-scrolling action shooter built in Unity 6, inspired by classic run-and-gun games like Metal Slug. Command a 4-person squad, switch between characters in real-time, fight through five zones to defeat bosses, and pick up weapons along the way.

## Quick Start

1. Open the project in **Unity 6000.3.18f1** or later
2. Run **Steel Rain > Build All** from the menu bar to generate all assets, prefabs, and scenes
3. Press Play in the `Level01_VerticalSlice` scene

## Controls

| Action | Key |
|--------|-----|
| Move | WASD / Arrow Keys |
| Jump | Space |
| Shoot | Left Mouse / J |
| Cycle Weapon Form | E |
| Dodge | Left Shift |
| Switch Character 1-4 | 1 / 2 / 3 / 4 |
| Rotate Character | Tab |
| Pause | ESC |
| Restart (paused) | R |
| Return to Menu (paused) | Q |

## Characters

| Character | Description |
|-----------|-------------|
| **Aila** | Balanced assault specialist |
| **Bruno** | Heavy defense operator |
| **Mara** | Long-range sniper |
| **Niko** | High-mobility skirmisher |

All 4 characters are available from the start. Switch between them in real-time — each has independent health. If one falls, switch to another and keep fighting.

## Weapons

| Weapon | Forms |
|--------|-------|
| Assault Rifle | Auto / Pierce / Grenade |
| Shotgun | Scatter / Slug / Burning |
| Rocket Launcher | Direct / Split / Seeker |

Pick up weapon crates dropped by enemies or found in the level to switch your current weapon. **Death resets to the pistol.** Press E to cycle between weapon forms.

## Difficulty

Choose Easy / Normal / Hard from the main menu. Difficulty affects enemy damage, health, and speed multipliers.

## Level Structure

### Level 1: Beach → Village → Trench
- **Zone A (Beach)**: Rifle soldier tutorial, 2-3 enemy waves
- **Zone B (Village)**: Shield soldier introduction, mixed waves
- **Zone C (Trench)**: Drone + mixed enemy gauntlet
- **Boss Arena**: Turret Boss (stationary turret with bullet patterns)

### Level 2: Factory
- Industrial themed level with moving platforms, crumbling platforms, explosive barrels, and spike hazards
- More enemy variety and tougher waves
- **Boss Arena**: Mini-Boss Walker (3 phases: machine gun sweep, jump slam, core exposed)

### Level 3: Warzone
- Ruined cityscape with rubble, craters, and sandbag emplacements
- Mixed enemy gauntlet with snipers and heavy gunners
- **Boss Arena**: Turret Boss

### Level 4: Bunker
- Tight corridors and confined rooms with limited cover
- Close-quarters combat with shield soldiers and grenadiers
- **Boss Arena**: Mini-Boss Walker

### Level 5: Citadel
- Final assault on the enemy stronghold with layered defenses
- All enemy types in sustained pressure waves
- **Boss Arena**: Turret Boss (final showdown)

Checkpoints are placed between zones. Health packs and weapon pickups are scattered throughout.

## Features

- **4-person squad system** with per-character health and real-time switching
- **9 enemy types**: Rifle Soldier, Shield Soldier, Drone, Grenadier, Sniper, Heavy Gunner, Charger, Mini-Boss Walker, Turret Boss
- **Environmental hazards**: Spike traps, explosive barrels, moving platforms, crumbling platforms
- **Score system** with combo multiplier and local leaderboard (Top 10)
- **Achievement system**: unlockable achievements + tracked statistics
- **Endless Mode**: Wave-based survival mode unlocked after completing the campaign — every 5 waves spawns a boss
- **Save system**: Checkpoints and settings persist via PlayerPrefs
- **Procedural content**: All art, audio, and music are generated at build time — zero external assets
- **HUD**: Health bar, ammo display, squad roster, score, combo counter, boss health bar, damage numbers
- **Scene transitions**: Fade-to-black between scenes
- **Settings**: Master/Music/SFX volume, fullscreen toggle, screen shake intensity
- **Boot screen**: Loading progress + random game tips displayed on startup
- **Menu animations**: Floating star particles on the main menu
- **Interactive UI**: Button hover/press feedback, click sound effects
- **Character switch toast**: Shows current character name when switching
- **Low health vignette**: Red screen edge when health is below 30%
- **Damage direction indicator**: Arrows showing where damage came from
- **Camera shake**: Screen shake on player damage and explosions
- **Tutorial system**: Contextual prompts teaching controls in Level 1

## Building

### Editor Build
```
Steel Rain > Build All        # Generate all assets + scenes
Steel Rain > Build Windows    # Generate + build Windows executable
```

### Command Line Build
```
Unity.exe -batchmode -executeMethod SteelRain.EditorTools.BuildTools.BuildWindows -quit
```

Output: `Builds/Windows/SteelRainFrontier.exe`

## Project Structure

```
Assets/
  Game/
    Core/        - Health, Damage, Team, GameEvents, SaveSystem, ScoreManager, DifficultyManager, LevelManager, TempBuffState
    Player/      - Controller, Combat, Dodge, Squad, CharacterRuntime
    Enemies/     - AI controllers (Rifle, Shield, Drone, Grenadier, MiniBoss, Turret)
    Weapons/     - Weapon definitions, projectiles, patterns
    Levels/      - Checkpoints, triggers, destructibles, hazards, platforms
    Pickups/     - Health, weapon pickups, shield, invincible
    UI/          - HUD, pause, settings, game over, victory, boss bar, health bar, damage numbers, tutorial
    Audio/       - Procedural SFX + music generation, crossfade music player
    VFX/         - Explosions, muzzle flash, camera shake, particles, sprite animator, damage numbers
    Editor/      - One-click build tools
    Data/        - ScriptableObject assets (auto-generated)
    Tests/       - NUnit edit-mode tests
  Art/Generated/ - Procedural sprites (auto-generated)
  Audio/Generated/ - Procedural audio (auto-generated)
  Scenes/        - Boot, MainMenu, Level01-05, EndlessMode (auto-generated)
  Prefabs/       - All prefabs (auto-generated)
```

## Technical Notes

- All art, audio, and prefabs are procedurally generated at build time
- Zero external asset dependencies
- Single assembly (`SteelRain.Game`) with clean namespace separation
- Event-driven architecture via `GameEvents` static event bus
- URP rendering pipeline with Linear color space

## Requirements

- Unity 6000.3.18f1 (Unity 6 LTS)
- Universal Render Pipeline (URP) 17.3.0
- Windows 10+ (for built executable)

## Version History

### v2.1.0 (2026-06-21)
- **Added**: Endless Mode scene generation (wave-based survival, boss every 5 waves)
- **Added**: Character Select screen in main menu (choose preferred starting character)
- **Fixed**: Death no longer preserves picked-up weapons — resets to pistol (Metal Slug style)
- **Fixed**: Character selection now actually applies to the starting character in-game
- **Fixed**: VictoryScreen now properly links to GameCompleteScreen for campaign completion
- **Added**: Scenes auto-added to BuildSettings during level generation
- **Added**: TempBuffState unit tests

### v2.0.0 (2026-06-21)
- **Major simplification**: Removed shop, currency, skill system, weapon upgrades, character unlocks, and NGP progression
- **Metal Slug-style gameplay**: Classic run-and-gun with weapon pickups that switch weapons
- **Weapon switching**: Pick up weapon crates to swap weapons; death resets to pistol
- **Squad system retained**: 4 characters available from the start, switch in real-time
- **Streamlined HUD**: Focus on health, ammo, and score
- **Removed**: CharacterSkill, CurrencyManager, WeaponUpgradePickup, WeaponSwapPickup, NGP system, skill animations

### v1.0.2 (2026-06-19)
- Fixed: Level02 scene missing HUD, GameOver, Victory, BossHealthBar, and DamageIndicator UI components
- Fixed: Level01 scene missing LevelEndTrigger — level end logic now complete
- Added: Achievement system with unlockable achievements and tracked statistics
- Added: Achievement notification UI (slide-in animation on unlock)
- Added: Tutorial prompt system in Level 1 (contextual control tutorials)
- Improved: Level02 now has full UI parity with Level01

### v1.0.1 (2026-06-19)
- Fixed: Main menu SceneFader `fadeImage` not set — scene transitions now work correctly
- Fixed: Shield pickup caused permanent invincibility — shield timer now properly counts down
- Fixed: Invincible pickup caused permanent invincibility — coroutine now runs on player object
- Added: Boot screen with loading progress and random game tips
- Added: Menu background animations (floating star particles)
- Added: UI button hover/press visual feedback and click sound effects
- Added: Low health vignette (red screen edge when health below 30%)
- Added: Damage direction indicator (arrows showing where damage came from)
- Added: Camera shake on player damage and explosions
- Improved: UI color palette system for consistent visual styling

### v1.0.0
- Initial release: Full 2D squad shooter with 5 levels, 4 characters, 9 enemy types
