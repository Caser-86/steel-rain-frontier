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

        public bool CanFire()
        {
            return Ammo >= CurrentForm.ammoCost;
        }

        public bool ConsumeAmmo()
        {
            if (!CanFire())
                return false;

            Ammo -= CurrentForm.ammoCost;
            return true;
        }
    }
}
