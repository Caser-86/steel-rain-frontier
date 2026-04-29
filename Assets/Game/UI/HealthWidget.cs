using TMPro;
using UnityEngine;

namespace SteelRain.UI
{
    public sealed class HealthWidget : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        public void SetHealth(int current, int max)
        {
            if (label == null)
                return;

            label.text = $"{current}/{max}";
        }
    }
}
