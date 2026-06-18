# Steel Rain: Frontier

A 2D side-scrolling action shooter built in Unity 6. Command a 4-person squad, switch between characters in real-time, fight through two zones to defeat bosses, and collect weapon upgrades along the way.

## Quick Start

1. Open the project in **Unity 6000.3.0f1** or later
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
| Skill (Lv3) | Q / Right Mouse |
| Switch Character 1-4 | 1 / 2 / 3 / 4 |
| Rotate Character | Tab |
| Pause | ESC |
| Restart (paused) | R |
| Return to Menu (paused) | Q |

## Characters

| Character | Skill | Description |
|-----------|-------|-------------|
| **Aila** | Breakthrough Fire | High-speed dash + auto-scan burst, then 4s fire rate buff |
| **Bruno** | Trench Shield | 5s frontal bulletproof shield, ends with shield bash |
| **Mara** | Bombardment Matrix | Mark area, 6 bombs fall after 1s delay |
| **Niko** | Time Rift | 4s slow field, enemies and bullets slowed 60% |

## Weapons

| Weapon | Forms |
|--------|-------|
| Assault Rifle | Auto / Pierce / Grenade |
| Shotgun | Scatter / Slug / Burning |
| Rocket Launcher | Direct / Split / Seeker |

All weapons support 3 upgrade levels. Collect upgrade capsules to power up. Weapon swap pickups are placed in the level.

## Difficulty

Choose Easy / Normal / Hard from the main menu. Difficulty affects enemy damage, health, and speed multipliers.

## Level Structure

### Level 1: Beach → Village → Trench
- **Zone A (Beach)**: Rifle soldier tutorial, 2-3 enemy waves
- **Zone B (Village)**: Shield soldier introduction, mixed waves
- **Zone C (Trench)**: Drone + mixed enemy gauntlet
- **Boss Arena**: Mini-Boss Walker (3 phases: machine gun sweep, jump slam, core exposed)

### Level 2: Factory
- Industrial themed level with moving platforms, crumbling platforms, explosive barrels, and spike hazards
- More enemy variety and tougher waves
- **Boss Arena**: Mini-Boss Walker

Checkpoints are placed between zones. Health pickups, weapon upgrades, and weapon swap capsules are scattered throughout.

## Features

- **4-person squad system** with per-character health, weapon levels, and skill cooldowns
- **6 enemy types**: Rifle Soldier, Shield Soldier, Drone, Grenadier, Mini-Boss Walker
- **Environmental hazards**: Spike traps, explosive barrels, moving platforms, crumbling platforms
- **Score system** with combo multiplier and local leaderboard (Top 10)
- **Save system**: Checkpoints, weapon upgrades, settings all persist via PlayerPrefs
- **Procedural content**: All art, audio, and music are generated at build time — zero external assets
- **HUD**: Health bar, ammo display, weapon level, skill cooldown, squad roster, score, combo counter, boss health bar, damage numbers
- **Scene transitions**: Fade-to-black between scenes
- **Settings**: Master/Music/SFX volume, fullscreen toggle, screen shake intensity

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
    Core/        - Health, Damage, Team, GameEvents, SaveSystem, ScoreManager, DifficultyManager, LevelManager
    Player/      - Controller, Combat, Dodge, Squad, Skills
    Enemies/     - AI controllers (Rifle, Shield, Drone, Grenadier, MiniBoss, Turret)
    Weapons/     - Weapon definitions, projectiles, patterns
    Levels/      - Checkpoints, triggers, destructibles, hazards, platforms
    Pickups/     - Health, weapon upgrades, weapon swaps, shield, invincible
    UI/          - HUD, pause, settings, game over, victory, boss bar, health bar, damage numbers, tutorial
    Audio/       - Procedural SFX + music generation, crossfade music player
    VFX/         - Explosions, muzzle flash, camera shake, particles, sprite animator, damage numbers
    Editor/      - One-click build tools
    Data/        - ScriptableObject assets (auto-generated)
    Tests/       - NUnit edit-mode tests (17 tests)
  Art/Generated/ - Procedural sprites (auto-generated)
  Audio/Generated/ - Procedural audio (auto-generated)
  Scenes/        - Boot, MainMenu, Level01, Level02 (auto-generated)
  Prefabs/       - All prefabs (auto-generated)
```

## Technical Notes

- All art, audio, and prefabs are procedurally generated at build time
- Zero external asset dependencies
- Single assembly (`SteelRain.Game`) with clean namespace separation
- Event-driven architecture via `GameEvents` static event bus
- URP rendering pipeline with Linear color space

## Requirements

- Unity 6000.3.0f1 (Unity 6 LTS)
- Universal Render Pipeline (URP) 17.3.0
- Windows 10+ (for built executable)
