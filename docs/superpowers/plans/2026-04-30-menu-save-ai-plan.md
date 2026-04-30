# Menu Save AI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a usable main menu, local JSON save system, and in-game local AI advisor overlay.

**Architecture:** Keep systems local and deterministic: `SaveService` owns JSON IO, menu UI calls scene loading, checkpoint and mission events trigger autosave, AI advisor reads current gameplay state and prints tactical rules. Editor builder creates both menu and gameplay scenes so builds stay reproducible.

**Tech Stack:** Unity 6000.3, C#, Unity UI, JSON via `JsonUtility`, local `Application.persistentDataPath`.

---

### Task 1: Save Core

**Files:**
- Create: `Assets/Game/Save/SaveData.cs`
- Create: `Assets/Game/Save/SaveService.cs`
- Modify: `Assets/Game/Levels/CheckpointManager.cs`
- Modify: `Assets/Game/Player/PlayerSquad.cs`

- [ ] Add serializable save data containing level id, checkpoint, selected character, health, weapon id, weapon level, and completion flags.
- [ ] Add `SaveService` with `HasSave`, `Load`, `Save`, `Delete`, and `SavePath`.
- [ ] Autosave on checkpoint and mission completion.

### Task 2: Main Menu

**Files:**
- Create: `Assets/Game/UI/MainMenuController.cs`
- Modify: `Assets/Game/Editor/VerticalSliceBuilder.cs`
- Modify: `Assets/Game/Editor/BuildTools.cs`

- [ ] Build `MainMenu.unity` with buttons: Continue, New Game, Settings, Quit.
- [ ] Continue loads saved checkpoint through a pending load flag.
- [ ] New Game deletes save then loads Level01.
- [ ] Include menu scene before Level01 in Windows build.

### Task 3: AI Advisor

**Files:**
- Create: `Assets/Game/UI/AiAdvisorWidget.cs`
- Modify: `Assets/Game/Editor/VerticalSliceBuilder.cs`

- [ ] Add F1 overlay.
- [ ] Show current tactical advice from local rules: low health, weapon level, crouch, weapon pickups, barrels, Boss.
- [ ] Keep it offline and deterministic.

### Task 4: Verification

**Commands:**
- Unity EditMode compile through `-runTests`.
- Build menu and Level01 with editor builder.
- Windows build with `BuildWindowsVerticalSlice`.
- Launch `Builds/Windows/SteelRainFrontier.exe` as smoke test.
