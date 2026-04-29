# Metal Slug-Like Vertical Slice Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build an 8-10 minute Unity vertical slice for 《钢雨前线》 with one playable character, three weapons, nine weapon forms, core enemy AI, checkpoints, HUD, and a grayboxed first-level segment.

**Architecture:** Use a small data-driven Unity 6.3 LTS project. Keep gameplay logic in focused C# components under `Assets/Game`, with ScriptableObject configs for characters, weapons, weapon forms, enemies, waves, and checkpoints. Build a playable graybox before final art so movement, shooting, difficulty, and first-level pacing can be tested early.

**Tech Stack:** Unity 6.3 LTS, C#, URP 2D Renderer, Unity Input System, Unity Test Framework, Addressables, Git LFS, FMOD for Unity 2.03 after core gameplay is stable.

---

## Scope

This plan covers the vertical slice only:

- One complete playable character: Aila.
- Three weapons: assault rifle, shotgun, rocket launcher.
- Three forms per weapon.
- Eight enemy types plus one mini-boss.
- First-level graybox covering beach, village, trench, and mini-boss arena.
- Basic HUD, checkpoints, pickups, damage, death, and restart.
- Art-ready scene structure, but final production art is out of scope for this plan.

## File Structure

Create this Unity structure:

```text
Assets/
  Game/
    Core/
      GameBootstrap.cs
      GameEvents.cs
      Health.cs
      DamageInfo.cs
      ObjectPool.cs
      Team.cs
    Player/
      PlayerController2D.cs
      PlayerCombat.cs
      PlayerDodge.cs
      CharacterDefinition.cs
      CharacterRuntime.cs
    Weapons/
      WeaponDefinition.cs
      WeaponFormDefinition.cs
      WeaponRuntime.cs
      Projectile.cs
      ProjectilePattern.cs
      WeaponPickup.cs
    Enemies/
      EnemyDefinition.cs
      EnemyController.cs
      EnemyAttackPattern.cs
      EnemySpawner.cs
      MiniBossWalker.cs
    Levels/
      Checkpoint.cs
      CheckpointManager.cs
      LevelSegmentTrigger.cs
      WaveDefinition.cs
      RescueNpc.cs
    UI/
      HudPresenter.cs
      AmmoWidget.cs
      HealthWidget.cs
    VFX/
      HitFlash.cs
      CameraShake.cs
    Tests/
      EditMode/
        WeaponRuntimeTests.cs
        HealthTests.cs
      PlayMode/
        PlayerControllerSmokeTests.cs
```

Scenes:

```text
Assets/Scenes/Boot.unity
Assets/Scenes/Level01_VerticalSlice.unity
```

Data assets:

```text
Assets/Game/Data/Characters/Aila.asset
Assets/Game/Data/Weapons/AssaultRifle.asset
Assets/Game/Data/Weapons/Shotgun.asset
Assets/Game/Data/Weapons/RocketLauncher.asset
Assets/Game/Data/Enemies/RifleSoldier.asset
Assets/Game/Data/Enemies/ShieldSoldier.asset
Assets/Game/Data/Levels/Level01/Waves/
```

## Task 1: Unity Project Setup

**Files:**

- Create: `Assets/Scenes/Boot.unity`
- Create: `Assets/Scenes/Level01_VerticalSlice.unity`
- Create: `Assets/Game/Core/GameBootstrap.cs`
- Create: `Assets/Game/Core/GameEvents.cs`
- Create: `Assets/Game/Core/Team.cs`
- Modify: `ProjectSettings/ProjectSettings.asset`
- Modify: `Packages/manifest.json`

- [ ] **Step 1: Create Unity 6.3 LTS 2D URP project**

Use Unity Hub:

```text
Template: 2D URP
Project name: SteelRainFrontier
Location: D:\Files\Game
Unity version: 6.3 LTS
```

Expected: Unity creates `Assets`, `Packages`, and `ProjectSettings`.

- [ ] **Step 2: Add required packages**

In Package Manager, install:

```text
Input System
2D Animation
2D Sprite
2D Tilemap Editor
Addressables
Unity Test Framework
Cinemachine
```

Expected: `Packages/manifest.json` contains package entries for input, 2D animation, addressables, tests, and Cinemachine.

- [ ] **Step 3: Create core enum**

Create `Assets/Game/Core/Team.cs`:

```csharp
namespace SteelRain.Core
{
    public enum Team
    {
        Player,
        Enemy,
        Neutral
    }
}
```

- [ ] **Step 4: Create event hub**

Create `Assets/Game/Core/GameEvents.cs`:

```csharp
using System;

namespace SteelRain.Core
{
    public static class GameEvents
    {
        public static event Action<int, int> PlayerHealthChanged;
        public static event Action<string, int> AmmoChanged;
        public static event Action<string> WeaponFormChanged;
        public static event Action CheckpointReached;
        public static event Action PlayerDied;

        public static void RaisePlayerHealthChanged(int current, int max) =>
            PlayerHealthChanged?.Invoke(current, max);

        public static void RaiseAmmoChanged(string weaponName, int ammo) =>
            AmmoChanged?.Invoke(weaponName, ammo);

        public static void RaiseWeaponFormChanged(string formName) =>
            WeaponFormChanged?.Invoke(formName);

        public static void RaiseCheckpointReached() =>
            CheckpointReached?.Invoke();

        public static void RaisePlayerDied() =>
            PlayerDied?.Invoke();
    }
}
```

- [ ] **Step 5: Create bootstrap**

Create `Assets/Game/Core/GameBootstrap.cs`:

```csharp
using UnityEngine;

namespace SteelRain.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;
        }
    }
}
```

- [ ] **Step 6: Configure scenes**

Create:

```text
Assets/Scenes/Boot.unity
Assets/Scenes/Level01_VerticalSlice.unity
```

Add `GameBootstrap` to `Boot.unity`. Add both scenes to Build Settings with `Boot.unity` first.

- [ ] **Step 7: Commit**

Run:

```powershell
git add Assets Packages ProjectSettings
git commit -m "chore: set up unity vertical slice project"
```

Expected: Commit succeeds if repository has been initialized. If repository does not exist, run:

```powershell
git init
git add .
git commit -m "chore: initialize project"
```

## Task 2: Health and Damage Foundation

**Files:**

- Create: `Assets/Game/Core/DamageInfo.cs`
- Create: `Assets/Game/Core/Health.cs`
- Create: `Assets/Game/Tests/EditMode/HealthTests.cs`

- [ ] **Step 1: Write health tests**

Create `Assets/Game/Tests/EditMode/HealthTests.cs`:

```csharp
using NUnit.Framework;
using SteelRain.Core;
using UnityEngine;

public sealed class HealthTests
{
    [Test]
    public void Damage_ReducesCurrentHealth()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);

        health.ApplyDamage(new DamageInfo(3, Team.Enemy, Vector2.right));

        Assert.AreEqual(7, health.Current);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Damage_FromSameTeam_IsIgnored()
    {
        var go = new GameObject("health");
        var health = go.AddComponent<Health>();
        health.Initialize(10, Team.Player);

        health.ApplyDamage(new DamageInfo(3, Team.Player, Vector2.right));

        Assert.AreEqual(10, health.Current);
        Object.DestroyImmediate(go);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run in Unity Test Runner:

```text
EditMode > HealthTests
```

Expected: compile fails because `Health` and `DamageInfo` do not exist.

- [ ] **Step 3: Add damage data**

Create `Assets/Game/Core/DamageInfo.cs`:

```csharp
using UnityEngine;

namespace SteelRain.Core
{
    public readonly struct DamageInfo
    {
        public readonly int Amount;
        public readonly Team SourceTeam;
        public readonly Vector2 Direction;

        public DamageInfo(int amount, Team sourceTeam, Vector2 direction)
        {
            Amount = amount;
            SourceTeam = sourceTeam;
            Direction = direction;
        }
    }
}
```

- [ ] **Step 4: Add health component**

Create `Assets/Game/Core/Health.cs`:

```csharp
using System;
using UnityEngine;

namespace SteelRain.Core
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] private int max = 5;
        [SerializeField] private Team team = Team.Enemy;

        public int Current { get; private set; }
        public int Max => max;
        public Team Team => team;

        public event Action<int, int> Changed;
        public event Action<DamageInfo> Damaged;
        public event Action Died;

        private bool dead;

        private void Awake()
        {
            Current = max;
        }

        public void Initialize(int maxHealth, Team assignedTeam)
        {
            max = Mathf.Max(1, maxHealth);
            team = assignedTeam;
            Current = max;
            dead = false;
            Changed?.Invoke(Current, max);
        }

        public void ApplyDamage(DamageInfo info)
        {
            if (dead || info.Amount <= 0 || info.SourceTeam == team)
                return;

            Current = Mathf.Max(0, Current - info.Amount);
            Damaged?.Invoke(info);
            Changed?.Invoke(Current, max);

            if (Current == 0)
            {
                dead = true;
                Died?.Invoke();
            }
        }
    }
}
```

- [ ] **Step 5: Run test to verify it passes**

Run:

```text
EditMode > HealthTests
```

Expected: both tests pass.

- [ ] **Step 6: Commit**

```powershell
git add Assets/Game/Core Assets/Game/Tests/EditMode/HealthTests.cs
git commit -m "feat: add health and damage foundation"
```

## Task 3: Player Controller

**Files:**

- Create: `Assets/Game/Player/CharacterDefinition.cs`
- Create: `Assets/Game/Player/CharacterRuntime.cs`
- Create: `Assets/Game/Player/PlayerController2D.cs`
- Create: `Assets/Game/Player/PlayerDodge.cs`
- Create: `Assets/Game/Data/Characters/Aila.asset`
- Create: `Assets/Game/Tests/PlayMode/PlayerControllerSmokeTests.cs`

- [ ] **Step 1: Create character definition**

Create `Assets/Game/Player/CharacterDefinition.cs`:

```csharp
using UnityEngine;

namespace SteelRain.Player
{
    [CreateAssetMenu(menuName = "Steel Rain/Character Definition")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        public string id = "aila";
        public string displayName = "Aila";
        public int maxHealth = 6;
        public float moveSpeed = 7.5f;
        public float jumpVelocity = 14f;
        public float dodgeSpeed = 13f;
        public float dodgeDuration = 0.18f;
        public float dodgeCooldown = 0.55f;
    }
}
```

- [ ] **Step 2: Create runtime holder**

Create `Assets/Game/Player/CharacterRuntime.cs`:

```csharp
namespace SteelRain.Player
{
    public sealed class CharacterRuntime
    {
        public CharacterDefinition Definition { get; }

        public CharacterRuntime(CharacterDefinition definition)
        {
            Definition = definition;
        }
    }
}
```

- [ ] **Step 3: Create player controller**

Create `Assets/Game/Player/PlayerController2D.cs`:

```csharp
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public sealed class PlayerController2D : MonoBehaviour
    {
        [SerializeField] private CharacterDefinition character;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundRadius = 0.12f;

        private Rigidbody2D body;
        private Health health;
        private Vector2 moveInput;
        private bool jumpQueued;
        private bool crouching;

        public bool IsGrounded { get; private set; }
        public Vector2 AimDirection { get; private set; } = Vector2.right;
        public CharacterDefinition Character => character;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            health.Initialize(character.maxHealth, Team.Player);
            health.Changed += GameEvents.RaisePlayerHealthChanged;
            health.Died += GameEvents.RaisePlayerDied;
        }

        private void Update()
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            crouching = Input.GetAxisRaw("Vertical") < -0.5f;

            if (Input.GetButtonDown("Jump"))
                jumpQueued = true;

            var aim = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (aim.sqrMagnitude > 0.1f)
                AimDirection = aim.normalized;
        }

        private void FixedUpdate()
        {
            IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

            var velocity = body.linearVelocity;
            velocity.x = moveInput.x * character.moveSpeed;

            if (jumpQueued && IsGrounded && !crouching)
                velocity.y = character.jumpVelocity;

            body.linearVelocity = velocity;
            jumpQueued = false;
        }
    }
}
```

- [ ] **Step 4: Create dodge component**

Create `Assets/Game/Player/PlayerDodge.cs`:

```csharp
using System.Collections;
using UnityEngine;

namespace SteelRain.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerDodge : MonoBehaviour
    {
        [SerializeField] private PlayerController2D controller;

        private Rigidbody2D body;
        private float nextAllowedTime;
        private bool dodging;

        public bool IsDodging => dodging;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
                TryDodge();
        }

        public void TryDodge()
        {
            if (dodging || Time.time < nextAllowedTime)
                return;

            StartCoroutine(DodgeRoutine());
        }

        private IEnumerator DodgeRoutine()
        {
            dodging = true;
            nextAllowedTime = Time.time + controller.Character.dodgeCooldown;

            var direction = controller.AimDirection.x < 0f ? -1f : 1f;
            body.linearVelocity = new Vector2(direction * controller.Character.dodgeSpeed, 0f);

            yield return new WaitForSeconds(controller.Character.dodgeDuration);
            dodging = false;
        }
    }
}
```

- [ ] **Step 5: Build Aila prefab**

In Unity:

```text
Create Aila.asset from Character Definition
Set maxHealth: 6
Set moveSpeed: 7.5
Set jumpVelocity: 14
Set dodgeSpeed: 13
Set dodgeDuration: 0.18
Set dodgeCooldown: 0.55
```

Create `Player_Aila` prefab:

```text
Components:
- Rigidbody2D
- BoxCollider2D
- Health
- PlayerController2D
- PlayerDodge
Child:
- GroundCheck
```

- [ ] **Step 6: Create smoke test**

Create `Assets/Game/Tests/PlayMode/PlayerControllerSmokeTests.cs`:

```csharp
using NUnit.Framework;
using SteelRain.Player;
using UnityEngine;

public sealed class PlayerControllerSmokeTests
{
    [Test]
    public void CharacterDefinition_DefaultAilaValues_ArePlayable()
    {
        var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
        Assert.GreaterOrEqual(definition.maxHealth, 3);
        Assert.Greater(definition.moveSpeed, 0f);
        Assert.Greater(definition.jumpVelocity, 0f);
        Object.DestroyImmediate(definition);
    }
}
```

- [ ] **Step 7: Run tests**

Run:

```text
PlayMode > PlayerControllerSmokeTests
EditMode > HealthTests
```

Expected: all tests pass.

- [ ] **Step 8: Commit**

```powershell
git add Assets/Game/Player Assets/Game/Data/Characters Assets/Game/Tests/PlayMode
git commit -m "feat: add aila player controller"
```

## Task 4: Weapon Forms and Projectiles

**Files:**

- Create: `Assets/Game/Weapons/ProjectilePattern.cs`
- Create: `Assets/Game/Weapons/WeaponFormDefinition.cs`
- Create: `Assets/Game/Weapons/WeaponDefinition.cs`
- Create: `Assets/Game/Weapons/WeaponRuntime.cs`
- Create: `Assets/Game/Weapons/Projectile.cs`
- Create: `Assets/Game/Player/PlayerCombat.cs`
- Create: `Assets/Game/Tests/EditMode/WeaponRuntimeTests.cs`

- [ ] **Step 1: Write weapon runtime tests**

Create `Assets/Game/Tests/EditMode/WeaponRuntimeTests.cs`:

```csharp
using NUnit.Framework;
using SteelRain.Weapons;
using UnityEngine;

public sealed class WeaponRuntimeTests
{
    [Test]
    public void CycleForm_WrapsBackToFirstForm()
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.displayName = "Assault Rifle";
        weapon.forms = new[]
        {
            CreateForm("Auto"),
            CreateForm("Pierce"),
            CreateForm("Grenade")
        };

        var runtime = new WeaponRuntime(weapon, 90);
        runtime.CycleForm();
        runtime.CycleForm();
        runtime.CycleForm();

        Assert.AreEqual("Auto", runtime.CurrentForm.displayName);
        Object.DestroyImmediate(weapon);
    }

    private static WeaponFormDefinition CreateForm(string name)
    {
        var form = ScriptableObject.CreateInstance<WeaponFormDefinition>();
        form.displayName = name;
        form.ammoCost = 1;
        return form;
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```text
EditMode > WeaponRuntimeTests
```

Expected: compile fails because weapon classes do not exist.

- [ ] **Step 3: Add projectile pattern enum**

Create `Assets/Game/Weapons/ProjectilePattern.cs`:

```csharp
namespace SteelRain.Weapons
{
    public enum ProjectilePattern
    {
        Single,
        Spread,
        LobbedExplosive,
        Piercing,
        SplitRocket
    }
}
```

- [ ] **Step 4: Add weapon form definition**

Create `Assets/Game/Weapons/WeaponFormDefinition.cs`:

```csharp
using UnityEngine;

namespace SteelRain.Weapons
{
    [CreateAssetMenu(menuName = "Steel Rain/Weapon Form")]
    public sealed class WeaponFormDefinition : ScriptableObject
    {
        public string id = "auto";
        public string displayName = "Auto";
        public ProjectilePattern pattern = ProjectilePattern.Single;
        public int damage = 1;
        public int ammoCost = 1;
        public float fireRate = 9f;
        public float projectileSpeed = 18f;
        public int projectileCount = 1;
        public float spreadAngle = 0f;
        public int pierceCount = 0;
        public float explosionRadius = 0f;
    }
}
```

- [ ] **Step 5: Add weapon definition**

Create `Assets/Game/Weapons/WeaponDefinition.cs`:

```csharp
using UnityEngine;

namespace SteelRain.Weapons
{
    [CreateAssetMenu(menuName = "Steel Rain/Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public string id = "assault_rifle";
        public string displayName = "Assault Rifle";
        public int startingAmmo = 90;
        public WeaponFormDefinition[] forms;
        public Projectile projectilePrefab;
    }
}
```

- [ ] **Step 6: Add weapon runtime**

Create `Assets/Game/Weapons/WeaponRuntime.cs`:

```csharp
using System;

namespace SteelRain.Weapons
{
    public sealed class WeaponRuntime
    {
        private readonly WeaponDefinition definition;
        private int formIndex;

        public WeaponDefinition Definition => definition;
        public WeaponFormDefinition CurrentForm => definition.forms[formIndex];
        public int Ammo { get; private set; }

        public WeaponRuntime(WeaponDefinition definition, int ammo)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            this.definition = definition;
            if (definition.forms == null || definition.forms.Length == 0)
                throw new ArgumentException("Weapon must have at least one form.", nameof(definition));

            Ammo = ammo;
        }

        public void CycleForm()
        {
            formIndex = (formIndex + 1) % definition.forms.Length;
        }

        public bool CanFire()
        {
            return Ammo >= CurrentForm.ammoCost;
        }

        public bool ConsumeAmmo()
        {
            if (!CanFire())
                return false;

            Ammo -= CurrentForm.ammoCost;
            return true;
        }
    }
}
```

- [ ] **Step 7: Add projectile**

Create `Assets/Game/Weapons/Projectile.cs`:

```csharp
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;

        private Rigidbody2D body;
        private Team sourceTeam;
        private int damage;
        private int pierceRemaining;
        private float despawnAt;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        public void Launch(Vector2 direction, WeaponFormDefinition form, Team team)
        {
            sourceTeam = team;
            damage = form.damage;
            pierceRemaining = form.pierceCount;
            despawnAt = Time.time + lifetime;
            body.linearVelocity = direction.normalized * form.projectileSpeed;
        }

        private void Update()
        {
            if (Time.time >= despawnAt)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Health health))
                return;

            health.ApplyDamage(new DamageInfo(damage, sourceTeam, body.linearVelocity.normalized));

            if (pierceRemaining > 0)
            {
                pierceRemaining--;
                return;
            }

            Destroy(gameObject);
        }
    }
}
```

- [ ] **Step 8: Add player combat**

Create `Assets/Game/Player/PlayerCombat.cs`:

```csharp
using SteelRain.Core;
using SteelRain.Weapons;
using UnityEngine;

namespace SteelRain.Player
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private PlayerController2D controller;
        [SerializeField] private Transform muzzle;
        [SerializeField] private WeaponDefinition startingWeapon;

        private WeaponRuntime currentWeapon;
        private float nextFireTime;

        private void Awake()
        {
            currentWeapon = new WeaponRuntime(startingWeapon, startingWeapon.startingAmmo);
            GameEvents.RaiseWeaponFormChanged(currentWeapon.CurrentForm.displayName);
            GameEvents.RaiseAmmoChanged(currentWeapon.Definition.displayName, currentWeapon.Ammo);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
                CycleForm();

            if (Input.GetButton("Fire1"))
                TryFire();
        }

        private void CycleForm()
        {
            currentWeapon.CycleForm();
            GameEvents.RaiseWeaponFormChanged(currentWeapon.CurrentForm.displayName);
        }

        private void TryFire()
        {
            var form = currentWeapon.CurrentForm;
            if (Time.time < nextFireTime || !currentWeapon.ConsumeAmmo())
                return;

            nextFireTime = Time.time + 1f / form.fireRate;
            FirePattern(form);
            GameEvents.RaiseAmmoChanged(currentWeapon.Definition.displayName, currentWeapon.Ammo);
        }

        private void FirePattern(WeaponFormDefinition form)
        {
            var count = Mathf.Max(1, form.projectileCount);
            var startAngle = -form.spreadAngle * 0.5f;
            var step = count == 1 ? 0f : form.spreadAngle / (count - 1);

            for (var i = 0; i < count; i++)
            {
                var direction = Quaternion.Euler(0f, 0f, startAngle + step * i) * controller.AimDirection;
                var projectile = Instantiate(currentWeapon.Definition.projectilePrefab, muzzle.position, Quaternion.identity);
                projectile.Launch(direction, form, Team.Player);
            }
        }
    }
}
```

- [ ] **Step 9: Run tests**

Run:

```text
EditMode > WeaponRuntimeTests
EditMode > HealthTests
```

Expected: all tests pass.

- [ ] **Step 10: Create weapon data**

Create ScriptableObjects:

```text
AssaultRifle forms:
- Auto: damage 1, ammoCost 1, fireRate 9, projectileCount 1, spreadAngle 0
- Pierce: damage 2, ammoCost 2, fireRate 4, pierceCount 2
- Grenade: damage 4, ammoCost 5, fireRate 1.2, pattern LobbedExplosive, explosionRadius 2

Shotgun forms:
- Scatter: damage 1, ammoCost 2, fireRate 1.8, projectileCount 6, spreadAngle 35
- Slug: damage 5, ammoCost 3, fireRate 1.1, projectileCount 1
- Burning: damage 2, ammoCost 4, fireRate 1.0, projectileCount 3, spreadAngle 20

RocketLauncher forms:
- Direct: damage 8, ammoCost 1, fireRate 0.7
- Split: damage 4, ammoCost 2, fireRate 0.6, pattern SplitRocket
- Seeker: damage 5, ammoCost 2, fireRate 0.8
```

- [ ] **Step 11: Commit**

```powershell
git add Assets/Game/Weapons Assets/Game/Player/PlayerCombat.cs Assets/Game/Data/Weapons Assets/Game/Tests/EditMode/WeaponRuntimeTests.cs
git commit -m "feat: add weapon form system"
```

## Task 5: Enemy Foundation

**Files:**

- Create: `Assets/Game/Enemies/EnemyDefinition.cs`
- Create: `Assets/Game/Enemies/EnemyAttackPattern.cs`
- Create: `Assets/Game/Enemies/EnemyController.cs`
- Create: `Assets/Game/Enemies/EnemySpawner.cs`

- [ ] **Step 1: Create enemy attack enum**

Create `Assets/Game/Enemies/EnemyAttackPattern.cs`:

```csharp
namespace SteelRain.Enemies
{
    public enum EnemyAttackPattern
    {
        RifleBurst,
        GrenadeArc,
        ShieldAdvance,
        DroneDive,
        FlamethrowerCone,
        MortarMarker
    }
}
```

- [ ] **Step 2: Create enemy definition**

Create `Assets/Game/Enemies/EnemyDefinition.cs`:

```csharp
using UnityEngine;

namespace SteelRain.Enemies
{
    [CreateAssetMenu(menuName = "Steel Rain/Enemy Definition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        public string id = "rifle_soldier";
        public string displayName = "Rifle Soldier";
        public int maxHealth = 3;
        public float moveSpeed = 2.5f;
        public float detectRange = 9f;
        public float attackRange = 6f;
        public float attackCooldown = 1.4f;
        public EnemyAttackPattern attackPattern = EnemyAttackPattern.RifleBurst;
    }
}
```

- [ ] **Step 3: Create enemy controller**

Create `Assets/Game/Enemies/EnemyController.cs`:

```csharp
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Enemies
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private Transform target;

        private Health health;
        private Rigidbody2D body;
        private float nextAttackTime;

        private void Awake()
        {
            health = GetComponent<Health>();
            body = GetComponent<Rigidbody2D>();
            health.Initialize(definition.maxHealth, Team.Enemy);
            health.Died += () => Destroy(gameObject);
        }

        private void FixedUpdate()
        {
            if (target == null)
                return;

            var delta = target.position - transform.position;
            var distance = Mathf.Abs(delta.x);

            if (distance > definition.detectRange)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                return;
            }

            if (distance > definition.attackRange)
            {
                var direction = Mathf.Sign(delta.x);
                body.linearVelocity = new Vector2(direction * definition.moveSpeed, body.linearVelocity.y);
                return;
            }

            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            TryAttack();
        }

        private void TryAttack()
        {
            if (Time.time < nextAttackTime)
                return;

            nextAttackTime = Time.time + definition.attackCooldown;
        }

        public void AssignTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
```

- [ ] **Step 4: Create spawner**

Create `Assets/Game/Enemies/EnemySpawner.cs`:

```csharp
using UnityEngine;

namespace SteelRain.Enemies
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private Transform player;
        [SerializeField] private Transform[] spawnPoints;

        public void SpawnAll()
        {
            foreach (var point in spawnPoints)
            {
                var enemy = Instantiate(enemyPrefab, point.position, Quaternion.identity);
                enemy.AssignTarget(player);
            }
        }
    }
}
```

- [ ] **Step 5: Create vertical slice enemy data**

Create EnemyDefinition assets:

```text
RifleSoldier: health 3, speed 2.5, attackRange 6, pattern RifleBurst
Grenadier: health 3, speed 2.0, attackRange 7, pattern GrenadeArc
ShieldSoldier: health 6, speed 1.6, attackRange 2, pattern ShieldAdvance
Sniper: health 2, speed 1.2, attackRange 10, pattern RifleBurst
Drone: health 2, speed 4.2, attackRange 5, pattern DroneDive
Flamer: health 5, speed 1.9, attackRange 3, pattern FlamethrowerCone
MortarSoldier: health 4, speed 1.0, attackRange 9, pattern MortarMarker
CrawlerMutant: health 3, speed 3.6, attackRange 1.4, pattern ShieldAdvance
```

- [ ] **Step 6: Manual test in scene**

In `Level01_VerticalSlice.unity`:

```text
Place Player_Aila at x=0.
Place 3 RifleSoldier prefabs at x=8, x=11, x=14.
Press Play.
Walk right.
Expected: enemies detect player, move into attack range, stop near attack range, die when shot.
```

- [ ] **Step 7: Commit**

```powershell
git add Assets/Game/Enemies Assets/Game/Data/Enemies
git commit -m "feat: add enemy foundation"
```

## Task 6: Checkpoints, Waves, and Level Flow

**Files:**

- Create: `Assets/Game/Levels/WaveDefinition.cs`
- Create: `Assets/Game/Levels/LevelSegmentTrigger.cs`
- Create: `Assets/Game/Levels/Checkpoint.cs`
- Create: `Assets/Game/Levels/CheckpointManager.cs`
- Create: `Assets/Game/Levels/RescueNpc.cs`

- [ ] **Step 1: Add wave definition**

Create `Assets/Game/Levels/WaveDefinition.cs`:

```csharp
using SteelRain.Enemies;
using UnityEngine;

namespace SteelRain.Levels
{
    [CreateAssetMenu(menuName = "Steel Rain/Wave Definition")]
    public sealed class WaveDefinition : ScriptableObject
    {
        public EnemyController[] enemyPrefabs;
        public Vector2[] spawnOffsets;
    }
}
```

- [ ] **Step 2: Add level segment trigger**

Create `Assets/Game/Levels/LevelSegmentTrigger.cs`:

```csharp
using SteelRain.Enemies;
using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class LevelSegmentTrigger : MonoBehaviour
    {
        [SerializeField] private WaveDefinition wave;
        [SerializeField] private Transform player;
        [SerializeField] private bool triggerOnce = true;

        private bool triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered && triggerOnce)
                return;

            if (!other.CompareTag("Player"))
                return;

            triggered = true;
            SpawnWave();
        }

        private void SpawnWave()
        {
            for (var i = 0; i < wave.enemyPrefabs.Length; i++)
            {
                var offset = i < wave.spawnOffsets.Length ? wave.spawnOffsets[i] : Vector2.zero;
                var enemy = Instantiate(wave.enemyPrefabs[i], (Vector2)transform.position + offset, Quaternion.identity);
                enemy.AssignTarget(player);
            }
        }
    }
}
```

- [ ] **Step 3: Add checkpoint**

Create `Assets/Game/Levels/Checkpoint.cs`:

```csharp
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class Checkpoint : MonoBehaviour
    {
        [SerializeField] private CheckpointManager manager;

        private bool used;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (used || !other.CompareTag("Player"))
                return;

            used = true;
            manager.SetCheckpoint(transform.position);
            GameEvents.RaiseCheckpointReached();
        }
    }
}
```

- [ ] **Step 4: Add checkpoint manager**

Create `Assets/Game/Levels/CheckpointManager.cs`:

```csharp
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class CheckpointManager : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Vector3 fallbackSpawn;

        private Vector3 currentSpawn;

        private void Awake()
        {
            currentSpawn = fallbackSpawn;
            GameEvents.PlayerDied += RespawnPlayer;
        }

        private void OnDestroy()
        {
            GameEvents.PlayerDied -= RespawnPlayer;
        }

        public void SetCheckpoint(Vector3 position)
        {
            currentSpawn = position;
        }

        private void RespawnPlayer()
        {
            player.position = currentSpawn;
        }
    }
}
```

- [ ] **Step 5: Add rescue NPC**

Create `Assets/Game/Levels/RescueNpc.cs`:

```csharp
using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class RescueNpc : MonoBehaviour
    {
        [SerializeField] private GameObject rewardPrefab;
        [SerializeField] private Transform rewardPoint;

        private bool rescued;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (rescued || !other.CompareTag("Player"))
                return;

            rescued = true;
            if (rewardPrefab != null)
                Instantiate(rewardPrefab, rewardPoint.position, Quaternion.identity);

            gameObject.SetActive(false);
        }
    }
}
```

- [ ] **Step 6: Build graybox flow**

In `Level01_VerticalSlice.unity`, create:

```text
Segment A: Beach, x=0 to x=45, 3 waves, rifle soldiers and grenadiers.
Checkpoint A: x=45.
Segment B: Village, x=45 to x=95, roof path, rescue NPC, shield soldier.
Checkpoint B: x=95.
Segment C: Trench, x=95 to x=150, mortar markers, drones, crawler mutants.
Checkpoint C: x=150.
Segment D: Mini-boss arena, x=150 to x=185.
```

Use rectangles and Tilemap blocks. Art fidelity is less important than readable jumps, cover, and enemy timing.

- [ ] **Step 7: Commit**

```powershell
git add Assets/Game/Levels Assets/Scenes/Level01_VerticalSlice.unity Assets/Game/Data/Levels
git commit -m "feat: add checkpoints and level waves"
```

## Task 7: Mini-Boss Walker

**Files:**

- Create: `Assets/Game/Enemies/MiniBossWalker.cs`
- Create: `Assets/Game/Data/Enemies/MiniBossWalker.asset`

- [ ] **Step 1: Add mini-boss controller**

Create `Assets/Game/Enemies/MiniBossWalker.cs`:

```csharp
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Enemies
{
    [RequireComponent(typeof(Health))]
    public sealed class MiniBossWalker : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float gunCooldown = 1.2f;
        [SerializeField] private float jumpCooldown = 4f;
        [SerializeField] private int contactDamage = 2;

        private Health health;
        private float nextGunTime;
        private float nextJumpTime;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Initialize(35, Team.Enemy);
            health.Died += () => Destroy(gameObject);
        }

        private void Update()
        {
            if (target == null)
                return;

            if (Time.time >= nextGunTime)
            {
                nextGunTime = Time.time + gunCooldown;
            }

            if (Time.time >= nextJumpTime && Mathf.Abs(target.position.x - transform.position.x) < 7f)
            {
                nextJumpTime = Time.time + jumpCooldown;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.TryGetComponent(out Health other))
                return;

            other.ApplyDamage(new DamageInfo(contactDamage, Team.Enemy, Vector2.right));
        }

        public void AssignTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
```

- [ ] **Step 2: Build mini-boss prefab**

In Unity:

```text
Prefab name: MiniBoss_Walker
Components:
- Rigidbody2D
- BoxCollider2D
- Health
- MiniBossWalker
Health: max initialized by MiniBossWalker to 35
Scale: 3x player height
Arena: x=150 to x=185
```

- [ ] **Step 3: Manual boss test**

Run scene:

```text
Start at checkpoint C.
Enter mini-boss arena.
Shoot boss with assault rifle auto form.
Expected: boss survives normal enemy burst, loses health, damages player on contact, dies before 2 minutes with good play.
```

- [ ] **Step 4: Commit**

```powershell
git add Assets/Game/Enemies/MiniBossWalker.cs Assets/Game/Data/Enemies Assets/Prefabs
git commit -m "feat: add walker mini boss"
```

## Task 8: HUD and Feedback

**Files:**

- Create: `Assets/Game/UI/HudPresenter.cs`
- Create: `Assets/Game/UI/AmmoWidget.cs`
- Create: `Assets/Game/UI/HealthWidget.cs`
- Create: `Assets/Game/VFX/HitFlash.cs`
- Create: `Assets/Game/VFX/CameraShake.cs`

- [ ] **Step 1: Add health widget**

Create `Assets/Game/UI/HealthWidget.cs`:

```csharp
using TMPro;
using UnityEngine;

namespace SteelRain.UI
{
    public sealed class HealthWidget : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        public void SetHealth(int current, int max)
        {
            label.text = $"{current}/{max}";
        }
    }
}
```

- [ ] **Step 2: Add ammo widget**

Create `Assets/Game/UI/AmmoWidget.cs`:

```csharp
using TMPro;
using UnityEngine;

namespace SteelRain.UI
{
    public sealed class AmmoWidget : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        public void SetAmmo(string weaponName, int ammo)
        {
            label.text = $"{weaponName} {ammo}";
        }

        public void SetForm(string formName)
        {
            label.text = $"{label.text} [{formName}]";
        }
    }
}
```

- [ ] **Step 3: Add HUD presenter**

Create `Assets/Game/UI/HudPresenter.cs`:

```csharp
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.UI
{
    public sealed class HudPresenter : MonoBehaviour
    {
        [SerializeField] private HealthWidget healthWidget;
        [SerializeField] private AmmoWidget ammoWidget;

        private void OnEnable()
        {
            GameEvents.PlayerHealthChanged += healthWidget.SetHealth;
            GameEvents.AmmoChanged += ammoWidget.SetAmmo;
            GameEvents.WeaponFormChanged += ammoWidget.SetForm;
        }

        private void OnDisable()
        {
            GameEvents.PlayerHealthChanged -= healthWidget.SetHealth;
            GameEvents.AmmoChanged -= ammoWidget.SetAmmo;
            GameEvents.WeaponFormChanged -= ammoWidget.SetForm;
        }
    }
}
```

- [ ] **Step 4: Add hit flash**

Create `Assets/Game/VFX/HitFlash.cs`:

```csharp
using System.Collections;
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.VFX
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Health))]
    public sealed class HitFlash : MonoBehaviour
    {
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float duration = 0.06f;

        private SpriteRenderer spriteRenderer;
        private Health health;
        private Color originalColor;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            health = GetComponent<Health>();
            originalColor = spriteRenderer.color;
            health.Damaged += _ => StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(duration);
            spriteRenderer.color = originalColor;
        }
    }
}
```

- [ ] **Step 5: Add camera shake**

Create `Assets/Game/VFX/CameraShake.cs`:

```csharp
using System.Collections;
using UnityEngine;

namespace SteelRain.VFX
{
    public sealed class CameraShake : MonoBehaviour
    {
        public void Shake(float duration, float strength)
        {
            StartCoroutine(ShakeRoutine(duration, strength));
        }

        private IEnumerator ShakeRoutine(float duration, float strength)
        {
            var origin = transform.localPosition;
            var endTime = Time.time + duration;

            while (Time.time < endTime)
            {
                var offset = Random.insideUnitCircle * strength;
                transform.localPosition = origin + new Vector3(offset.x, offset.y, 0f);
                yield return null;
            }

            transform.localPosition = origin;
        }
    }
}
```

- [ ] **Step 6: Build HUD canvas**

In `Level01_VerticalSlice.unity`:

```text
Canvas: Screen Space Overlay
Top-left: HealthWidget text
Top-right: AmmoWidget text
Attach HudPresenter to Canvas
Wire HealthWidget and AmmoWidget references
```

Expected: health changes after damage, ammo count changes after firing, weapon form text changes after pressing E.

- [ ] **Step 7: Commit**

```powershell
git add Assets/Game/UI Assets/Game/VFX Assets/Scenes/Level01_VerticalSlice.unity
git commit -m "feat: add vertical slice hud feedback"
```

## Task 9: Vertical Slice Tuning Pass

**Files:**

- Modify: `Assets/Game/Data/Characters/Aila.asset`
- Modify: `Assets/Game/Data/Weapons/*.asset`
- Modify: `Assets/Game/Data/Enemies/*.asset`
- Modify: `Assets/Scenes/Level01_VerticalSlice.unity`

- [ ] **Step 1: Tune movement baseline**

Use these starting values:

```text
Aila:
moveSpeed: 7.5
jumpVelocity: 14
dodgeSpeed: 13
dodgeDuration: 0.18
dodgeCooldown: 0.55
```

Expected manual result:

```text
Player can cross one screen in about 2.5 seconds.
Player can jump over one-tile obstacles reliably.
Dodge feels useful but cannot be spammed.
```

- [ ] **Step 2: Tune weapon baseline**

Use these targets:

```text
Assault Rifle Auto: kills rifle soldier in about 0.35 seconds.
Assault Rifle Pierce: kills shield soldier faster from front than Auto.
Shotgun Scatter: strongest under 3 meters.
Shotgun Slug: useful against mini-boss weak windows.
Rocket Direct: strong but ammo-limited.
```

Expected manual result:

```text
Each weapon form has a reason to exist.
No form dominates every enemy and distance.
```

- [ ] **Step 3: Tune first 10 minutes pacing**

Target sequence:

```text
00:00-00:30 movement and first rifle soldiers
00:30-02:30 beach waves and first weapon pickup
02:30-04:30 village route split and rescue NPC
04:30-06:30 shield soldier and grenade pressure
06:30-08:30 trench, drones, crawler mutants
08:30-10:00 mini-boss fight
```

Expected manual result:

```text
No empty walking stretch exceeds 12 seconds.
No combat encounter exceeds 75 seconds before a new event appears.
Deaths restart within 5 seconds at nearest checkpoint.
```

- [ ] **Step 4: Record tuning notes**

Create `docs/superpowers/specs/2026-04-29-vertical-slice-tuning-notes.md`:

```markdown
# Vertical Slice Tuning Notes

Date: 2026-04-29

## Movement

- Aila movement values accepted when beach traversal feels responsive at 60 FPS.

## Weapons

- Assault Rifle Auto is the baseline for ordinary soldiers.
- Pierce form is the answer to shield pressure.
- Grenade and rocket forms are burst tools with ammo pressure.

## Enemies

- Rifle soldiers teach aiming.
- Shield soldiers teach form switching.
- Drones teach vertical aiming.
- Crawler mutants teach spacing.

## Level Flow

- Beach introduces controls.
- Village introduces alternate routes and rescue.
- Trench introduces pressure.
- Mini-boss validates sustained combat.
```

- [ ] **Step 5: Commit**

```powershell
git add Assets/Game/Data Assets/Scenes/Level01_VerticalSlice.unity docs/superpowers/specs/2026-04-29-vertical-slice-tuning-notes.md
git commit -m "tune: balance vertical slice baseline"
```

## Task 10: Verification and Delivery

**Files:**

- Modify: `docs/superpowers/specs/2026-04-29-metal-slug-like-design.md` if scope notes need clarification
- Create: `docs/superpowers/specs/2026-04-29-vertical-slice-playtest-checklist.md`

- [ ] **Step 1: Create playtest checklist**

Create `docs/superpowers/specs/2026-04-29-vertical-slice-playtest-checklist.md`:

```markdown
# Vertical Slice Playtest Checklist

Date: 2026-04-29

## Controls

- Player can move, jump, crouch, dodge, aim, shoot, and switch weapon form.
- Player understands controls without reading external documentation.

## Combat

- Rifle soldiers, shield soldiers, drones, and crawler mutants require different responses.
- Weapon forms are useful in distinct situations.
- Player can recover from one mistake without instant death.

## Level

- First 8-10 minutes have no long empty walk.
- Each segment introduces one new pressure or mechanic.
- Checkpoints restart quickly.

## Presentation

- HUD health, ammo, and form text update correctly.
- Hit flash and camera shake improve readability.
- Scene remains readable with explosions and multiple enemies.

## Performance

- Game holds 60 FPS in beach, village, trench, and mini-boss arena.
- No recurring console errors during a full playthrough.
```

- [ ] **Step 2: Run automated tests**

Run in Unity Test Runner:

```text
EditMode:
- HealthTests
- WeaponRuntimeTests

PlayMode:
- PlayerControllerSmokeTests
```

Expected:

```text
All tests pass.
No compile errors.
```

- [ ] **Step 3: Run full manual playthrough**

Manual route:

```text
Start Level01_VerticalSlice.
Reach Checkpoint A.
Rescue village NPC.
Reach Checkpoint B.
Clear trench.
Reach Checkpoint C.
Defeat MiniBoss_Walker.
Restart once from death to confirm checkpoint.
```

Expected:

```text
Full run takes 8-10 minutes.
No blocker bugs.
Player always respawns at latest checkpoint.
Mini-boss can be killed with available ammo.
```

- [ ] **Step 4: Build Windows executable**

In Unity:

```text
File > Build Profiles > Windows
Architecture: x86_64
Development Build: enabled for vertical slice
Scene list: Boot, Level01_VerticalSlice
Build folder: Builds/Windows/VerticalSlice
```

Expected:

```text
Build completes.
Builds/Windows/VerticalSlice contains executable and data folder.
Executable launches into playable slice.
```

- [ ] **Step 5: Commit verification docs**

```powershell
git add docs/superpowers/specs/2026-04-29-vertical-slice-playtest-checklist.md Builds
git commit -m "test: add vertical slice playtest checklist"
```

## Milestones

Milestone 1, day 1-2:

- Unity project opens.
- Player moves, jumps, and takes damage.

Milestone 2, day 3-5:

- Weapon runtime and three weapon definitions exist.
- Player shoots and switches forms.

Milestone 3, day 6-8:

- Basic enemies spawn, chase, stop, and die.
- Beach and village graybox playable.

Milestone 4, day 9-12:

- Checkpoints, rescue NPC, trench segment, and mini-boss work.

Milestone 5, day 13-15:

- HUD, hit feedback, tuning, tests, and Windows build complete.

## Self-Review

Spec coverage:

- Beautiful characters and picture quality: covered by art-ready URP 2D setup, player prefab, hit flash, camera shake, and vertical slice art boundary.
- Varied monsters and terrain: covered by eight enemy definitions, beach/village/trench/arena graybox, wave triggers.
- Multi-form weapons: covered by three weapons with three forms each.
- Several switchable characters: full roster stays in the design document; vertical slice implements Aila first to control scope.
- Long first level: full 22-28 minute first level stays in the design document; this plan implements the first 8-10 minute slice as the proof segment.

Known scope decisions:

- Local co-op is excluded from this plan.
- Final art production is excluded from this plan.
- FMOD integration begins after core gameplay is stable, because Unity audio is enough for early tuning.
- Full Boss 巴别-01 is excluded from this plan; MiniBoss_Walker validates boss foundation.
