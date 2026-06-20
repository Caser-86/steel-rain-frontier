using System.Collections.Generic;
using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Player;
using SteelRain.UI;
using UnityEngine;

namespace SteelRain.Player
{
    /// <summary>
    /// 4 人小队切换系统。
    /// 1/2/3/4 切换角色，Tab 轮换。
    /// 后台角色保留生命、武器等级、技能状态。
    /// 当前角色死亡时清空其武器升级，自动切换到下一个存活角色。
    /// </summary>
    public sealed class PlayerSquad : MonoBehaviour
    {
        [SerializeField] private PlayerController2D controller;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private CharacterSkill skill;
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

        private void Awake()
        {
            foreach (var member in members)
            {
                runtimes.Add(new CharacterRuntime(member));
                alive.Add(true);
            }

            // 应用商店购买的最大血量加成
            var hpBonus = SaveSystem.LoadMaxHealthBonus();
            if (hpBonus > 0)
            {
                foreach (var runtime in runtimes)
                    runtime.CurrentHealth = runtime.Definition.maxHealth + hpBonus;
            }

            // 尝试从存档恢复小队状态（断线/崩溃恢复）
            if (SaveSystem.HasSquadSave())
            {
                RestoreFromSave();
            }

            if (runtimes.Count > 0)
                ApplyRuntime(SaveSystem.HasSquadSave() ? SaveSystem.LoadSquadActiveIndex() : 0);

            cachedHealth = controller.GetComponent<Health>();
            if (cachedHealth != null)
                cachedHealth.Changed += OnHealthChanged;

            GameEvents.PlayerDied += OnActiveCharacterDied;
            GameEvents.MaxHealthUpgraded += OnMaxHealthUpgraded;
        }

        /// <summary>
        /// 从存档恢复小队存活状态和各角色血量。
        /// </summary>
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

        /// <summary>
        /// 保存当前小队状态到存档系统（检查点触发或角色死亡时调用）。
        /// </summary>
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
        /// 复活所有角色（复活信标使用）。
        /// </summary>
        public void ReviveAll()
        {
            for (int i = 0; i < alive.Count; i++)
            {
                alive[i] = true;
                runtimes[i].CurrentHealth = runtimes[i].Definition.maxHealth;
                runtimes[i].SetWeaponLevel(runtimes[i].SelectedWeaponId, 1);
            }
            ApplyRuntime(0);
        }

        private void OnDestroy()
        {
            if (cachedHealth != null)
                cachedHealth.Changed -= OnHealthChanged;
            GameEvents.PlayerDied -= OnActiveCharacterDied;
            GameEvents.MaxHealthUpgraded -= OnMaxHealthUpgraded;
        }

        private void OnHealthChanged(int current, int max)
        {
            if (activeIndex >= 0 && activeIndex < runtimes.Count)
                runtimes[activeIndex].CurrentHealth = current;
        }

        private void OnMaxHealthUpgraded()
        {
            var bonus = SaveSystem.LoadMaxHealthBonus();
            foreach (var runtime in runtimes)
                runtime.CurrentHealth = Mathf.Max(runtime.CurrentHealth, runtime.Definition.maxHealth + bonus);
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
            // 未解锁的角色不可切换
            if (!CharacterUnlockManager.IsUnlocked(members[index].id)) return;

            ApplyRuntime(index);
        }

        private void TrySwitchNext()
        {
            for (int i = 1; i <= runtimes.Count; i++)
            {
                int idx = (activeIndex + i) % runtimes.Count;
                if (alive[idx] && CharacterUnlockManager.IsUnlocked(members[idx].id))
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
            controller.SetSkillLock(false);
            var runtime = runtimes[index];
            controller.AssignCharacter(runtime.Definition, runtime.CurrentHealth);
            combat.ApplyCharacterRuntime(runtime);
            if (skill != null) skill.AssignRuntime(runtime);
            GameEvents.RaisePlayerCharacterChanged(runtime.Definition.displayName);
            AudioManager.Play("sfx_pickup", 0.6f);
        }

        private void OnActiveCharacterDied()
        {
            if (activeIndex >= 0 && activeIndex < alive.Count)
            {
                alive[activeIndex] = false;
                runtimes[activeIndex].ClearCurrentWeaponUpgradeOnDeath();
            }

            if (AliveCount == 0)
            {
                var gameOver = FindFirstObjectByType<GameOverScreen>();
                if (gameOver != null)
                {
                    gameOver.Show();
                }
                else
                {
                    for (int i = 0; i < alive.Count; i++)
                    {
                        alive[i] = true;
                        runtimes[i].CurrentHealth = runtimes[i].Definition.maxHealth;
                    }
                    ApplyRuntime(0);
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
                    // 切换到下一个存活角色，使用其当前血量（保留战斗中损失的血量）
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
