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
        public int Level { get; private set; }

        public WeaponRuntime(WeaponDefinition definition, int ammo)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (definition.forms == null || definition.forms.Length == 0)
                throw new ArgumentException("Weapon must have at least one form.", nameof(definition));

            this.definition = definition;
            Ammo = ammo;
        }

        public void CycleForm()
        {
            formIndex = (formIndex + 1) % definition.forms.Length;
        }

        public int Upgrade()
        {
            Level = Math.Min(3, Level + 1);
            return Level;
        }

        public void SetLevel(int level)
        {
            Level = Math.Clamp(level, 0, 3);
        }

        public void ResetUpgrades()
        {
            Level = 0;
        }

        public int GetDamage()
        {
            var multiplier = Level switch
            {
                1 => CurrentForm.levelOneDamageMultiplier,
                2 => CurrentForm.levelTwoDamageMultiplier,
                3 => CurrentForm.levelThreeDamageMultiplier,
                _ => 1f
            };

            return Math.Max(1, (int)Math.Round(CurrentForm.damage * multiplier));
        }

        public float GetFireRate()
        {
            var multiplier = Level switch
            {
                1 => CurrentForm.levelOneFireRateMultiplier,
                2 => CurrentForm.levelTwoFireRateMultiplier,
                3 => CurrentForm.levelThreeFireRateMultiplier,
                _ => 1f
            };

            return CurrentForm.fireRate * multiplier;
        }

        public bool CanFire()
        {
            return definition.baseAmmoInfinite || Ammo >= CurrentForm.ammoCost;
        }

        public bool ConsumeAmmo()
        {
            if (!CanFire())
                return false;

            if (definition.baseAmmoInfinite)
            {
                Ammo = -1;
                return true;
            }

            Ammo -= CurrentForm.ammoCost;
            return true;
        }
    }
}
