using System.Collections;
using SteelRain.Core;
using SteelRain.Levels;
using SteelRain.Player;
using SteelRain.Weapons;
using UnityEngine;

namespace SteelRain.Save
{
    public sealed class SaveGameController : MonoBehaviour
    {
        [SerializeField] private string levelId = "Level01";
        [SerializeField] private CheckpointManager checkpoints;
        [SerializeField] private PlayerSquad squad;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private WeaponDefinition[] weapons;

        private void Start()
        {
            if (SaveService.TryConsumePendingLoad(out var data))
                StartCoroutine(RestoreAfterSceneStart(data));
        }

        private void OnEnable()
        {
            GameEvents.CheckpointReached += SaveNow;
            GameEvents.LevelCompleted += SaveCompleted;
            GameEvents.SquadDefeated += SaveNow;
        }

        private void OnDisable()
        {
            GameEvents.CheckpointReached -= SaveNow;
            GameEvents.LevelCompleted -= SaveCompleted;
            GameEvents.SquadDefeated -= SaveNow;
        }

        public void SaveNow()
        {
            SaveService.Save(Capture(false));
        }

        private void SaveCompleted()
        {
            SaveService.Save(Capture(true));
        }

        private SaveData Capture(bool completed)
        {
            return new SaveData
            {
                levelId = levelId,
                checkpoint = checkpoints != null ? checkpoints.CurrentSpawn : Vector3.zero,
                selectedCharacterIndex = squad != null ? squad.ActiveIndex : 0,
                characterHealth = squad != null ? squad.CaptureHealth() : null,
                weaponId = combat != null ? combat.CurrentWeaponId : "assault_rifle",
                weaponLevel = combat != null ? combat.CurrentWeaponLevel : 0,
                level01Cleared = completed
            };
        }

        private void Restore(SaveData data)
        {
            if (data == null)
                return;

            if (checkpoints != null)
                checkpoints.RestoreCheckpoint(data.checkpoint);

            if (squad != null)
                squad.RestoreState(data.selectedCharacterIndex, data.characterHealth);

            var weapon = FindWeapon(data.weaponId);
            if (weapon != null && combat != null)
            {
                combat.EquipWeapon(weapon);
                combat.SetCurrentWeaponLevel(data.weaponLevel);
            }
        }

        private IEnumerator RestoreAfterSceneStart(SaveData data)
        {
            yield return null;
            Restore(data);
        }

        private WeaponDefinition FindWeapon(string id)
        {
            if (weapons == null)
                return null;

            foreach (var weapon in weapons)
            {
                if (weapon != null && weapon.id == id)
                    return weapon;
            }

            return null;
        }
    }
}
