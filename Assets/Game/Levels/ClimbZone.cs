using SteelRain.Player;
using UnityEngine;

namespace SteelRain.Levels
{
    public sealed class ClimbZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController2D controller))
                controller.SetClimbZone(this, true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController2D controller))
                controller.SetClimbZone(this, false);
        }
    }
}
