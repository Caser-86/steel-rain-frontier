using NUnit.Framework;
using SteelRain.Weapons;
using UnityEngine;

public sealed class WeaponRuntimeTests
{
    [Test]
    public void CycleForm_WrapsBackToFirstForm()
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        weapon.displayName = "Assault Rifle";
        weapon.forms = new[]
        {
            CreateForm("Auto"),
            CreateForm("Pierce"),
            CreateForm("Grenade")
        };

        var runtime = new WeaponRuntime(weapon, 90);
        runtime.CycleForm();
        runtime.CycleForm();
        runtime.CycleForm();

        Assert.AreEqual("Auto", runtime.CurrentForm.displayName);
        Object.DestroyImmediate(weapon);
    }

    private static WeaponFormDefinition CreateForm(string name)
    {
        var form = ScriptableObject.CreateInstance<WeaponFormDefinition>();
        form.displayName = name;
        form.ammoCost = 1;
        return form;
    }
}
