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
            this.definition = definition ?? throw new ArgumentNullException(nameof(definition));
            if (definition.forms == null || definition.forms.Length == 0)
                throw new ArgumentException("Weapon must have at least one form.", nameof(definition));
            // 统一使用int.MaxValue表示无限弹药，避免-1导致CanFire返回false
            Ammo = ammo < 0 ? int.MaxValue : ammo;
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

        public void ResetUpgrades()
        {
            Level = 0;
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
                // 保持Ammo为int.MaxValue表示无限弹药，避免UI显示-1
                Ammo = int.MaxValue;
                return true;
            }

            Ammo -= CurrentForm.ammoCost;
            return true;
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
    }
}
