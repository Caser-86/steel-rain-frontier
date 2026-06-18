# Steel Rain: Frontier v1.0.1 Release Notes

Release Date: 2026-06-19

## Overview
This release focuses on critical bug fixes that prevented the game from being playable, plus significant UI/UX improvements and expanded test coverage.

## Critical Bug Fixes

### 1. Main Menu Scene Transition Fix
- **Issue**: SceneFader component in MainMenu scene had `fadeImage` set to `{fileID: 0}`, causing the fade-to-black transition to fail silently
- **Impact**: Game could not properly transition from main menu to gameplay
- **Fix**: Updated MainMenu.unity scene file to properly reference the fadeImage component (fileID: 411655963)
- **Files Modified**: `Assets/Scenes/MainMenu.unity`

### 2. Shield Pickup Permanent Invincibility Fix
- **Issue**: When picking up a shield capsule, the `ShieldActive` flag was set to true but no code decremented `ShieldTimer` when not in skill mode
- **Impact**: Player remained permanently invincible after shield pickup
- **Fix**: Added shield timer decrement logic in `CharacterSkill.Update()`. When shield is active and not in skill mode, the timer counts down and automatically deactivates the shield
- **Files Modified**: `Assets/Game/Player/CharacterSkill.cs`

### 3. Invincible Pickup Permanent Invincibility Fix
- **Issue**: `InvinciblePickup` started a coroutine and immediately disabled the pickup game object, causing the coroutine to terminate before restoring the player's team
- **Impact**: Player remained permanently invincible after invincible pickup
- **Fix**: Created `PickupCoroutineRunner` component. The coroutine now runs on the player object instead of the pickup, ensuring it completes properly
- **Files Modified**: `Assets/Game/Pickups/InvinciblePickup.cs`

### 4. Character Switch Toast Display Fix
- **Issue**: Character switch toast displayed the character's `displayName` instead of the character's skill name
- **Impact**: Players couldn't see which skill the current character had
- **Fix**: Added `GetSkillName()` method that maps `CharacterSkillId` enum values to human-readable skill names. Toast now displays "Character Name + Skill Name"
- **Files Modified**: `Assets/Game/UI/CharacterSwitchToast.cs`, `Assets/Game/Player/CharacterSkill.cs`

## New Features

### Boot Screen
- **What**: Animated loading screen displayed on game startup
- **Features**:
  - Progress bar showing game startup progress
  - Random game tips ("Pro tip: Switch characters to manage health across your squad", etc.)
  - Logo display with steel rain theme
- **Files Added**: `Assets/Game/Core/BootScreen.cs`

### Menu Background Animations
- **What**: Floating colored star particles on the main menu
- **Features**:
  - 20+ floating stars with random velocities
  - Colorful palette (orange, red, blue, yellow) matching the game's theme
  - Bounce off screen edges
- **Files Added**: `Assets/Game/UI/MenuBackgroundAnimator.cs`

### Interactive UI Feedback
- **What**: Button hover and press visual feedback
- **Features**:
  - Color changes on hover (lighter color)
  - Color changes on press (darker color)
  - Click sound effects for all interactive buttons
  - Scale animation (1.0x → 1.05x) on hover
- **Files Added**: `Assets/Game/UI/ButtonHighlight.cs`

### UI Color Palette System
- **What**: Centralized color management for consistent UI styling
- **Features**:
  - Primary color (orange: #e94f37)
  - Accent color (yellow: #f6ae2d)
  - Text colors (primary, secondary, muted)
  - Panel and divider colors
  - Used by all menu and UI components
- **Files Added**: `Assets/Game/UI/UIPalette.cs`

### Low Health Vignette
- **What**: Red screen edge effect when player health drops below 30%
- **Features**:
  - Intensity increases as health decreases
  - Provides visual feedback for critical health state
  - Clears when health is restored
- **Files Modified**: `Assets/Game/UI/LowHealthVignette.cs`

### Damage Direction Indicator
- **What**: Red arrows appearing at screen edges showing where damage came from
- **Features**:
  - Arrow points toward damage source (opposite of damage direction)
  - Fade out over 1.5 seconds
  - Cleared when player dies
- **Files Modified**: `Assets/Game/UI/DamageDirectionIndicator.cs`

### Camera Shake
- **What**: Screen shake effect on player damage and explosions
- **Features**:
  - Configurable shake duration and intensity
  - Subtle shake on regular player damage
  - Heavy shake on explosions
  - Follows player position
- **Files Modified**: `Assets/Game/VFX/CameraShake.cs`

### Character Switch Toast
- **What**: Brief notification showing current character and skill when switching
- **Features**:
  - Displays character name in large text
  - Displays skill name below
  - Fades in/out over 2 seconds
  - Vertical rise animation (text moves up during display)
- **Files Added**: `Assets/Game/UI/CharacterSwitchToast.cs`

## Test Coverage Expansion

- **Total Tests**: 47 (up from 17)
- **All Tests Passing**: Yes
- **New Test Files**:
  - `CharacterRuntimeTests.cs`: 12 tests covering character runtime state management
  - `DifficultyManagerTests.cs`: 15 tests covering difficulty scaling
  - `ScoreManagerTests.cs`: 13 tests covering scoring and combo system
- **Existing Tests**: 7 tests for PlayerSquad and other core systems

## Configuration Updates

### Unity Version
- **Before**: 6000.3.0f1
- **After**: 6000.3.18f1
- **Files Modified**: `ProjectSettings/ProjectVersion.txt`

### URP Package
- **Version**: 17.3.0
- **Files Modified**: `Packages/packages-lock.json`

### Scene Registration
- All 4 scenes properly registered in build settings:
  - `Boot.unity`
  - `MainMenu.unity`
  - `Level01_VerticalSlice.unity`
  - `Level02_Factory.unity`

## Technical Summary

### Files Modified (21 files)
- Core: `GameBootstrap.cs`
- Editor: `VerticalSliceBuilder.cs`
- Player: `CharacterSkill.cs`
- UI: `MainMenu.cs`, `CharacterSwitchToast.cs`, `ButtonHighlight.cs`, `UIPalette.cs`, `MenuBackgroundAnimator.cs`
- Pickups: `InvinciblePickup.cs`
- Scenes: `Boot.unity`, `MainMenu.unity`, `Level01_VerticalSlice.unity`, `Level02_Factory.unity`
- Audio: 8 generated WAV files
- Project Settings: `ProjectVersion.txt`, `ShaderGraphSettings.asset`, `UniversalRenderPipelineGlobalSettings.asset`
- Packages: `packages-lock.json`

### Files Added (14 files)
- Core: `BootScreen.cs` (+ meta)
- UI: `ButtonHighlight.cs` (+ meta), `CharacterSwitchToast.cs` (+ meta), `MenuBackgroundAnimator.cs` (+ meta), `UIPalette.cs` (+ meta)
- Tests: `CharacterRuntimeTests.cs` (+ meta), `DifficultyManagerTests.cs` (+ meta), `ScoreManagerTests.cs` (+ meta)

## Known Issues
- None reported. All known critical bugs from v1.0.0 have been fixed in this release.

## How to Update
1. Pull the latest version from GitHub
2. Open the project in Unity 6000.3.18f1 or later
3. Run `Steel Rain > Build All` to regenerate all assets and scenes
4. Press Play in the `Level01_VerticalSlice` scene
