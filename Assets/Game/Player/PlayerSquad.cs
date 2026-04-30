using System.Collections.Generic;
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Player
{
    public sealed class PlayerSquad : MonoBehaviour
    {
        [SerializeField] private PlayerController2D controller;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private CharacterDefinition[] members;

        private readonly List<CharacterRuntime> runtimes = new();
        private int activeIndex;
        private float nextSwitchTime;
        private bool initialized;
        private bool missionEnded;
        public int ActiveIndex => activeIndex;

        private void OnEnable()
        {
            GameEvents.PlayerDied += ActiveCharacterDied;
            GameEvents.LevelCompleted += EndMissionSquad;
            GameEvents.SquadDefeated += EndMissionSquad;
        }

        private void OnDisable()
        {
            GameEvents.PlayerDied -= ActiveCharacterDied;
            GameEvents.LevelCompleted -= EndMissionSquad;
            GameEvents.SquadDefeated -= EndMissionSquad;
        }

        private void Start()
        {
            runtimes.Clear();
            foreach (var member in members)
            {
                if (member != null)
                    runtimes.Add(new CharacterRuntime(member));
            }

            if (runtimes.Count == 0)
                return;

            initialized = true;
            ApplyRuntime(0, false);
        }

        private void Update()
        {
            if (!initialized || missionEnded || Time.time < nextSwitchTime)
                return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
                ApplyRuntime(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                ApplyRuntime(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                ApplyRuntime(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                ApplyRuntime(3);
            else if (Input.GetKeyDown(KeyCode.Tab))
                ApplyRuntime(FindNextAliveIndex(activeIndex));
        }

        public void ActiveCharacterDied()
        {
            if (!initialized || missionEnded)
                return;

            StoreActiveRuntime();
            runtimes[activeIndex].ClearCurrentWeaponUpgradeOnDeath();
            var nextIndex = FindNextAliveIndex(activeIndex);
            if (nextIndex == activeIndex)
            {
                GameEvents.RaiseSquadDefeated();
                return;
            }

            ApplyRuntime(nextIndex, false);
        }

        private int FindNextAliveIndex(int fromIndex)
        {
            if (runtimes.Count == 0)
                return fromIndex;

            for (var offset = 1; offset <= runtimes.Count; offset++)
            {
                var candidate = (fromIndex + offset) % runtimes.Count;
                if (runtimes[candidate].CurrentHealth > 0)
                    return candidate;
            }

            return fromIndex;
        }

        private void ApplyRuntime(int index, bool storeCurrent = true)
        {
            if (index < 0 || index >= runtimes.Count || index == activeIndex && initialized && storeCurrent)
                return;

            if (storeCurrent)
                StoreActiveRuntime();

            activeIndex = index;
            nextSwitchTime = Time.time + 0.5f;
            var runtime = runtimes[activeIndex];
            controller.AssignCharacter(runtime.Definition, runtime.CurrentHealth);
            combat.ApplyCharacterRuntime(runtime);
            GameEvents.RaisePlayerCharacterChanged(runtime.Definition.displayName);
        }

        public int[] CaptureHealth()
        {
            StoreActiveRuntime();
            var health = new int[runtimes.Count];
            for (var i = 0; i < runtimes.Count; i++)
                health[i] = runtimes[i].CurrentHealth;

            return health;
        }

        public void RestoreState(int selectedIndex, int[] health)
        {
            if (!initialized || runtimes.Count == 0)
                return;

            for (var i = 0; i < runtimes.Count; i++)
            {
                if (health != null && i < health.Length)
                    runtimes[i].CurrentHealth = Mathf.Clamp(health[i], 0, runtimes[i].Definition.maxHealth);
            }

            ApplyRuntime(Mathf.Clamp(selectedIndex, 0, runtimes.Count - 1), false);
        }

        private void StoreActiveRuntime()
        {
            if (activeIndex < 0 || activeIndex >= runtimes.Count)
                return;

            runtimes[activeIndex].CurrentHealth = controller.CurrentHealth;
            combat.StoreCurrentWeaponLevel();
        }

        private void EndMissionSquad()
        {
            missionEnded = true;
        }
    }
}
