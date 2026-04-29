# Unity Handoff Notes

Date: 2026-04-29

Project: Steel Rain Frontier / 《钢雨前线》

## Open Project

Open this folder in Unity Hub:

```text
D:\Files\Game\.worktrees\vertical-slice
```

Use Unity 6.3 LTS or newer Unity 6 LTS.

## Generate Vertical Slice Content

After Unity opens and scripts compile, run:

```text
Steel Rain > Build Vertical Slice Assets
Steel Rain > Build Level 01 Graybox
```

The menu tools create:

- `Assets/Game/Data/Characters/Aila.asset`
- weapon form assets for assault rifle, shotgun, and rocket launcher
- enemy definition assets
- player, projectile, enemy, and mini-boss prefabs
- wave assets for beach, village, and trench
- `Assets/Scenes/Level01_VerticalSlice.unity`

## Required Manual Checks

Run Unity Test Runner:

```text
EditMode:
- HealthTests
- WeaponRuntimeTests

PlayMode:
- PlayerControllerSmokeTests
```

Open `Level01_VerticalSlice.unity`, press Play, and check:

- Aila moves, jumps, shoots, and switches weapon form with `E`.
- Rifle soldiers and other wave enemies spawn when crossing segment triggers.
- Rifle soldiers, grenadiers, drones, flamers, mortar soldiers, and the mini-boss fire enemy projectiles.
- Checkpoints update when crossed.
- Mini-boss can take damage and die.

## Known Limits

- Enemy projectiles use placeholder straight-line physics; grenades and mortars are readable pressure tools, not final ballistic arcs.
- Camera follow is generated; Cinemachine can replace it after player movement tuning.
- HUD canvas is generated but text widgets still need TMP child labels wired if automatic wiring fails.
