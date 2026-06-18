using SteelRain.Audio;
using SteelRain.Player;
using UnityEngine;

namespace SteelRain.Pickups
{
    /// <summary>
    /// 武器升级胶囊拾取物，永久存在直到拾取。
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class WeaponUpgradePickup : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerCombat combat))
                return;

            combat.UpgradeCurrentWeapon();
            AudioManager.Play("sfx_upgrade");
            gameObject.SetActive(false);
        }
    }
}
