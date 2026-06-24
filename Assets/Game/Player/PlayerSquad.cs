using System.Collections.Generic;
using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.UI;
using UnityEngine;

namespace SteelRain.Player
{
    /// <summary>
    /// 4 人小队切换系统（合金弹头简化版）。
    /// 1/2/3/4 切换角色，Tab 轮换。
    /// 后台角色保留生命，当前角色死亡时自动切换到下一个存活角色。
    /// 无技能、无武器升级、无角色解锁，全部角色开局可用。
    /// </summary>
    public sealed class PlayerSquad : MonoBehaviour
    {
        [SerializeField] private PlayerController2D controller;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private CharacterDefinition[] members;
        [SerializeField] private float switchCooldown = 0.5f;

        private readonly List<CharacterRuntime> runtimes = new();
        private readonly List<bool> alive = new();
        private int activeIndex;
        private float nextSwitchTime;
        private Health cachedHealth;

        public int ActiveIndex => activeIndex;
        public int AliveCount
        {
            get
            {
                int c = 0;
                for (int i = 0; i < alive.Count; i++)
                    if (alive[i]) c++;
                return c;
            }
        }
        public int MemberCount => members != null ? members.Length : 1;

        private void Awake()
        {
            foreach (var member in members)
            {
                runtimes.Add(new CharacterRuntime(member));
                alive.Add(true);
            }

            // 尝试从存档恢复小队状态（断线/崩溃恢复）
            if (SaveSystem.HasSquadSave())
                RestoreFromSave();

            // 确定起始角色：有存档用存档索引，否则用玩家选择的首选角色
            int startIndex = 0;
            if (SaveSystem.HasSquadSave())
            {
                startIndex = SaveSystem.LoadSquadActiveIndex();
            }
            else
            {
                // 读取玩家在角色选择界面选择的首选角色
                var preferredId = UI.CharacterSelectScreen.GetPreferredCharacterId();
                for (int i = 0; i < members.Length; i++)
                {
                    if (members[i] != null && members[i].id == preferredId)
                    {
                        startIndex = i;
                        break;
                    }
                }
            }

            if (runtimes.Count > 0)
                ApplyRuntime(startIndex);

            cachedHealth = controller.GetComponent<Health>();
            if (cachedHealth != null)
                cachedHealth.Changed += OnHealthChanged;

            GameEvents.PlayerDied += OnActiveCharacterDied;
        }

        private void RestoreFromSave()
        {
            var mask = SaveSystem.LoadSquadAliveMask();
            for (int i = 0; i < alive.Count && i < 4; i++)
            {
                alive[i] = (mask & (1 << i)) != 0;
                var savedHp = SaveSystem.LoadSquadHealth(i);
                if (savedHp > 0 && i < runtimes.Count)
                    runtimes[i].CurrentHealth = Mathf.Min(savedHp, runtimes[i].Definition.maxHealth);
            }
        }

        public void SaveSquadState()
        {
            int mask = 0;
            for (int i = 0; i < alive.Count; i++)
                if (alive[i]) mask |= (1 << i);

            var healths = new int[runtimes.Count];
            for (int i = 0; i < runtimes.Count; i++)
                healths[i] = runtimes[i].CurrentHealth;

            SaveSystem.SaveSquadState(mask, activeIndex, healths);
        }

        /// <summary>
        /// 全员死亡后复活所有角色（检查点复活）。
        /// 重置武器为手枪（合金弹头风格）。
        /// </summary>
        public void ReviveAll()
        {
            for (int i = 0; i < alive.Count; i++)
            {
                alive[i] = true;
                runtimes[i].CurrentHealth = runtimes[i].Definition.maxHealth;
            }
            ApplyRuntime(0);
            // 死亡复活后重置武器为手枪
            if (combat != null)
                combat.ResetToStartingWeapon();
        }

        private void OnDestroy()
        {
            if (cachedHealth != null)
                cachedHealth.Changed -= OnHealthChanged;
            GameEvents.PlayerDied -= OnActiveCharacterDied;
        }

        private void OnHealthChanged(int current, int max)
        {
            if (activeIndex >= 0 && activeIndex < runtimes.Count)
                runtimes[activeIndex].CurrentHealth = current;
        }

        private void Update()
        {
            if (Time.time < nextSwitchTime)
                return;

            if (Input.GetKeyDown(KeyCode.Alpha1)) TrySwitch(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) TrySwitch(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) TrySwitch(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) TrySwitch(3);
            else if (Input.GetKeyDown(KeyCode.Tab)) TrySwitchNext();
        }

        private void TrySwitch(int index)
        {
            if (index < 0 || index >= runtimes.Count) return;
            if (index == activeIndex) return;
            if (!alive[index]) return;

            ApplyRuntime(index);
        }

        private void TrySwitchNext()
        {
            for (int i = 1; i <= runtimes.Count; i++)
            {
                int idx = (activeIndex + i) % runtimes.Count;
                if (alive[idx])
                {
                    ApplyRuntime(idx);
                    return;
                }
            }
        }

        private void ApplyRuntime(int index)
        {
            if (index < 0 || index >= runtimes.Count) return;

            activeIndex = index;
            nextSwitchTime = Time.time + switchCooldown;
            controller.SetDodgeLock(false);
            var runtime = runtimes[index];
            controller.AssignCharacter(runtime.Definition, runtime.CurrentHealth);
            combat.ApplyCharacterRuntime(runtime);
            GameEvents.RaisePlayerCharacterChanged(runtime.Definition.displayName);
            AudioManager.Play("sfx_pickup", 0.6f);
        }

        private void OnActiveCharacterDied()
        {
            if (activeIndex >= 0 && activeIndex < alive.Count)
                alive[activeIndex] = false;

            if (AliveCount == 0)
            {
                // 无尽模式下直接显示 GameOverScreen（不复活）
                if (LevelManager.InEndlessMode)
                {
                    var gameOver = FindFirstObjectByType<GameOverScreen>();
                    if (gameOver != null)
                    {
                        gameOver.Show();
                    }
                    else
                    {
                        // 兜底：无尽模式下没有 GameOverScreen 时返回主菜单，避免卡死
                        Debug.LogWarning("[PlayerSquad] EndlessMode ended but no GameOverScreen found, returning to menu.");
                        LevelManager.ReturnToMenu();
                    }
                    return;
                }

                var gameOverNormal = FindFirstObjectByType<GameOverScreen>();
                if (gameOverNormal != null)
                {
                    gameOverNormal.Show();
                }
                else
                {
                    ReviveAll();
                    var health = controller.GetComponent<Health>();
                    if (health != null)
                        health.ReviveFull();
                }
            }
            else
            {
                int prevIndex = activeIndex;
                TrySwitchNext();
                if (activeIndex != prevIndex && activeIndex >= 0 && activeIndex < runtimes.Count)
                {
                    var health = controller.GetComponent<Health>();
                    if (health != null)
                        health.Revive(runtimes[activeIndex].CurrentHealth);
                }
            }
        }

        public CharacterRuntime GetRuntime(int index)
        {
            if (index < 0 || index >= runtimes.Count) return null;
            return runtimes[index];
        }
    }
}
