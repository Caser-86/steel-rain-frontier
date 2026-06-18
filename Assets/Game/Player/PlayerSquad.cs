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

            if (runtimes.Count > 0)
                ApplyRuntime(0);

            cachedHealth = controller.GetComponent<Health>();
            if (cachedHealth != null)
                cachedHealth.Changed += OnHealthChanged;

            GameEvents.PlayerDied += OnActiveCharacterDied;
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
                var gameOver = FindObjectOfType<GameOverScreen>();
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
                    runtimes[activeIndex].CurrentHealth = runtimes[activeIndex].Definition.maxHealth;
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
