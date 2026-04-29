using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class HealthWidget : MonoBehaviour
    {
        [SerializeField] private Text label;

        public void SetHealth(int current, int max)
        {
            if (label == null)
                return;

            label.text = $"{current}/{max}";
        }
    }
}
