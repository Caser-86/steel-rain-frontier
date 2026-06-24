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
            return Math.Max(1, CurrentForm.damage);
        }

        public float GetFireRate()
        {
            return CurrentForm.fireRate;
        }
    }
}
