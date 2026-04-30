using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Player
{
    public sealed class PlayerSkillController : MonoBehaviour
    {
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private Health health;

        private bool missionEnded;

        private void OnEnable()
        {
            GameEvents.LevelCompleted += EndMissionSkill;
            GameEvents.SquadDefeated += EndMissionSkill;
        }

        private void OnDisable()
        {
            GameEvents.LevelCompleted -= EndMissionSkill;
            GameEvents.SquadDefeated -= EndMissionSkill;
        }

        private void Update()
        {
            if (missionEnded)
                return;

            var runtime = combat != null ? combat.CurrentCharacterRuntime : null;
            if (runtime == null)
                return;

            if (runtime.SkillCooldownRemaining > 0f)
                runtime.SkillCooldownRemaining = Mathf.Max(0f, runtime.SkillCooldownRemaining - Time.deltaTime);

            RefreshStatus(runtime);

            if (Input.GetKeyDown(KeyCode.L))
                TryUseSkill(runtime);
        }

        private void TryUseSkill(CharacterRuntime runtime)
        {
            if (combat.CurrentWeaponLevel < 3 || runtime.SkillCooldownRemaining > 0f)
                return;

            switch (runtime.Definition.skillId)
            {
                case CharacterSkillId.BreakthroughFire:
                    health.SetInvulnerable(0.45f);
                    combat.BeginTemporaryFirepower(4f, 1.4f, 1.2f);
                    break;
                case CharacterSkillId.TrenchShield:
                    health.SetInvulnerable(5f);
                    combat.BeginTemporaryFirepower(5f, 0.8f, 1.1f);
                    break;
                case CharacterSkillId.BombardmentMatrix:
                    combat.BeginTemporaryFirepower(5f, 1f, 2.2f);
                    break;
                case CharacterSkillId.TimeRift:
                    health.SetInvulnerable(2f);
                    combat.BeginTemporaryFirepower(4f, 1.25f, 1.1f);
                    break;
            }

            runtime.SkillCooldownRemaining = runtime.Definition.skillCooldown;
            RefreshStatus(runtime);
        }

        private void EndMissionSkill()
        {
            missionEnded = true;
        }

        private void RefreshStatus(CharacterRuntime runtime)
        {
            if (combat.CurrentWeaponLevel < 3)
            {
                GameEvents.RaiseSkillStatusChanged("Skill Locked");
                return;
            }

            if (runtime.SkillCooldownRemaining > 0f)
            {
                GameEvents.RaiseSkillStatusChanged($"Skill {runtime.SkillCooldownRemaining:0}s");
                return;
            }

            GameEvents.RaiseSkillStatusChanged("Skill Ready");
        }
    }
}
