using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.VFX;
using SteelRain.Weapons;
using UnityEngine;

namespace SteelRain.Player
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private PlayerController2D controller;
        [SerializeField] private Transform muzzle;
        [SerializeField] private WeaponDefinition startingWeapon;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private MuzzleFlash muzzleFlash;

        private WeaponRuntime currentWeapon;
        private CharacterRuntime characterRuntime;
        private float nextFireTime;

        public WeaponRuntime CurrentWeapon => currentWeapon;
        public int CurrentWeaponLevel => currentWeapon?.Level ?? 0;

        private void Awake()
        {
            if (startingWeapon != null)
            {
                currentWeapon = new WeaponRuntime(startingWeapon, startingWeapon.startingAmmo);
                GameEvents.RaiseWeaponFormChanged(currentWeapon.CurrentForm.displayName);
                GameEvents.RaiseAmmoChanged(currentWeapon.Definition.displayName, currentWeapon.Ammo);
                GameEvents.RaiseWeaponLevelChanged(currentWeapon.Level);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
                CycleForm();

            if (Input.GetButton("Fire1") || Input.GetKey(KeyCode.J))
                TryFire();
        }

        private void CycleForm()
        {
            if (currentWeapon == null) return;
            currentWeapon.CycleForm();
            AudioManager.Play("sfx_form_switch", 0.5f);
            GameEvents.RaiseWeaponFormChanged(currentWeapon.CurrentForm.displayName);
        }

        private void TryFire()
        {
            if (currentWeapon == null || projectilePrefab == null) return;

            var fireRate = currentWeapon.GetFireRate();
            if (Time.time < nextFireTime || !currentWeapon.ConsumeAmmo())
                return;

            nextFireTime = Time.time + 1f / fireRate;
            FirePattern(currentWeapon.CurrentForm, currentWeapon.GetDamage());
            AudioManager.Play("sfx_gunshot", 0.7f);
            if (muzzleFlash != null && controller != null)
                muzzleFlash.Flash(controller.AimDirection);

            // 弹壳粒子
            if (muzzle != null && controller != null)
                ParticleSpawner.SpawnShell(muzzle.position, controller.AimDirection);

            // 突破姿态 buff
            if (CharacterSkill.BreakthroughBuff)
                nextFireTime -= 1f / fireRate * 0.4f;

            GameEvents.RaiseAmmoChanged(currentWeapon.Definition.displayName, currentWeapon.Ammo);
        }

        private void FirePattern(WeaponFormDefinition form, int damage)
        {
            var count = Mathf.Max(1, form.projectileCount);
            var startAngle = -form.spreadAngle * 0.5f;
            var step = count == 1 ? 0f : form.spreadAngle / (count - 1);
            var aim = controller != null ? controller.AimDirection : Vector2.right;

            for (var i = 0; i < count; i++)
            {
                var direction = Quaternion.Euler(0f, 0f, startAngle + step * i) * aim;
                var pos = muzzle != null ? muzzle.position : transform.position;
                var projectile = Instantiate(projectilePrefab, pos, Quaternion.identity);
                projectile.LaunchWithDamage(direction, form.projectileSpeed, damage, form.pierceCount, Team.Player);
            }
        }

        public void SwapWeapon(WeaponDefinition newWeapon, int ammo)
        {
            if (newWeapon == null) return;
            currentWeapon = new WeaponRuntime(newWeapon, ammo < 0 ? newWeapon.startingAmmo : ammo);
            var savedLevel = SaveSystem.LoadWeaponLevel(newWeapon.id);
            for (var i = 0; i < savedLevel; i++)
                currentWeapon.Upgrade();
            GameEvents.RaiseWeaponFormChanged(currentWeapon.CurrentForm.displayName);
            GameEvents.RaiseAmmoChanged(currentWeapon.Definition.displayName, currentWeapon.Ammo);
            GameEvents.RaiseWeaponLevelChanged(currentWeapon.Level);
        }

        public void UpgradeCurrentWeapon()
        {
            if (currentWeapon == null) return;
            currentWeapon.Upgrade();
            characterRuntime?.SetWeaponLevel(currentWeapon.Definition.id, currentWeapon.Level);
            SaveSystem.SaveWeaponLevel(currentWeapon.Definition.id, currentWeapon.Level);
            GameEvents.RaiseWeaponLevelChanged(currentWeapon.Level);
            GameEvents.RaiseAmmoChanged(currentWeapon.Definition.displayName, currentWeapon.Ammo);
        }

        public void ApplyCharacterRuntime(CharacterRuntime runtime)
        {
            characterRuntime = runtime;
            if (currentWeapon != null)
            {
                currentWeapon.ResetUpgrades();
                var savedLevel = SaveSystem.LoadWeaponLevel(currentWeapon.Definition.id);
                var targetLevel = runtime?.GetWeaponLevel(currentWeapon.Definition.id) ?? 0;
                var finalLevel = Mathf.Max(savedLevel, targetLevel);
                for (var i = 0; i < finalLevel; i++)
                    currentWeapon.Upgrade();
                GameEvents.RaiseWeaponLevelChanged(currentWeapon.Level);
            }
        }
    }
}
