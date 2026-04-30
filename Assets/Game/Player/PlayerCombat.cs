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
        private CharacterRuntime characterRuntime;
        private float nextFireTime;
        private float firepowerUntil;
        private float fireRateMultiplier = 1f;
        private float damageMultiplier = 1f;

        public int CurrentWeaponLevel => currentWeapon.Level;
        public CharacterRuntime CurrentCharacterRuntime => characterRuntime;

        private void Awake()
        {
            currentWeapon = new WeaponRuntime(startingWeapon, startingWeapon.startingAmmo);
            GameEvents.RaiseWeaponFormChanged(currentWeapon.CurrentForm.displayName);
            GameEvents.RaiseWeaponLevelChanged(currentWeapon.Level);
            GameEvents.RaiseAmmoChanged(currentWeapon.Definition.displayName, currentWeapon.Ammo);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
                CycleForm();

            if (Input.GetButton("Fire1") || Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.LeftControl) || Input.GetMouseButton(0))
                TryFire();
        }

        private void CycleForm()
        {
            currentWeapon.CycleForm();
            GameEvents.RaiseWeaponFormChanged(currentWeapon.CurrentForm.displayName);
        }

        public void EquipWeapon(WeaponDefinition weapon)
        {
            currentWeapon = new WeaponRuntime(weapon, weapon.startingAmmo);
            nextFireTime = 0f;
            if (characterRuntime != null)
            {
                characterRuntime.SelectedWeaponId = weapon.id;
                ApplyStoredWeaponLevel();
            }

            GameEvents.RaiseWeaponFormChanged(currentWeapon.CurrentForm.displayName);
            GameEvents.RaiseWeaponLevelChanged(currentWeapon.Level);
            GameEvents.RaiseAmmoChanged(currentWeapon.Definition.displayName, currentWeapon.Ammo);
        }

        public void UpgradeCurrentWeapon()
        {
            currentWeapon.Upgrade();
            StoreCurrentWeaponLevel();
            GameEvents.RaiseWeaponLevelChanged(currentWeapon.Level);
            GameEvents.RaiseAmmoChanged(currentWeapon.Definition.displayName, currentWeapon.Ammo);
        }

        public void ApplyCharacterRuntime(CharacterRuntime runtime)
        {
            characterRuntime = runtime;
            if (currentWeapon == null)
                return;

            characterRuntime.SelectedWeaponId = currentWeapon.Definition.id;
            ApplyStoredWeaponLevel();
            GameEvents.RaiseWeaponLevelChanged(currentWeapon.Level);
            GameEvents.RaiseAmmoChanged(currentWeapon.Definition.displayName, currentWeapon.Ammo);
        }

        public void StoreCurrentWeaponLevel()
        {
            if (characterRuntime == null || currentWeapon == null)
                return;

            characterRuntime.SelectedWeaponId = currentWeapon.Definition.id;
            characterRuntime.SetWeaponLevel(currentWeapon.Definition.id, currentWeapon.Level);
        }

        private void ApplyStoredWeaponLevel()
        {
            currentWeapon.ResetUpgrades();
            var level = characterRuntime.GetWeaponLevel(currentWeapon.Definition.id);
            for (var i = 0; i < level; i++)
                currentWeapon.Upgrade();
        }

        private void TryFire()
        {
            var form = currentWeapon.CurrentForm;
            if (Time.time < nextFireTime || !currentWeapon.ConsumeAmmo())
                return;

            nextFireTime = Time.time + 1f / (currentWeapon.GetFireRate() * CurrentFireRateMultiplier);
            FirePattern(form);
            GameEvents.RaiseAmmoChanged(currentWeapon.Definition.displayName, currentWeapon.Ammo);
        }

        public void BeginTemporaryFirepower(float duration, float newFireRateMultiplier, float newDamageMultiplier)
        {
            firepowerUntil = Mathf.Max(firepowerUntil, Time.time + duration);
            fireRateMultiplier = Mathf.Max(fireRateMultiplier, newFireRateMultiplier);
            damageMultiplier = Mathf.Max(damageMultiplier, newDamageMultiplier);
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
                projectile.Launch(direction, form, Team.Player, Mathf.Max(1, Mathf.RoundToInt(currentWeapon.GetDamage() * CurrentDamageMultiplier)));
            }
        }

        private float CurrentFireRateMultiplier => Time.time < firepowerUntil ? fireRateMultiplier : 1f;
        private float CurrentDamageMultiplier => Time.time < firepowerUntil ? damageMultiplier : 1f;
    }
}
