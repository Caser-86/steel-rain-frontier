using SteelRain.Player;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class AiAdvisorWidget : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private PlayerController2D controller;
        [SerializeField] private PlayerCombat combat;

        private bool open;

        private void Awake()
        {
            if (label != null)
                label.enabled = false;
        }

        private void Start()
        {
            if (controller == null)
                controller = FindFirstObjectByType<PlayerController2D>();

            if (combat == null)
                combat = FindFirstObjectByType<PlayerCombat>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                open = !open;
                if (label != null)
                    label.enabled = open;
            }

            if (open)
                Refresh();
        }

        private void Refresh()
        {
            if (label == null || controller == null || combat == null)
                return;

            var x = controller.transform.position.x;
            var healthAdvice = controller.CurrentHealth <= 2
                ? "Health low: break crates and switch to a healthier squad member."
                : "Health stable: keep pressure and collect upgrades.";
            var upgradeAdvice = combat.CurrentWeaponLevel < 3
                ? "Priority: collect blue upgrades. Lv3 unlocks character skill."
                : "Lv3 ready: use L for character skill before elite fights.";
            var positionAdvice = x switch
            {
                < 32f => "Beach: crouch under high fire, use barrels to clear groups.",
                < 68f => "Village: climb to high platform for backup upgrade.",
                < 116f => "Trench: rocket cache ahead, drones punish standing still.",
                _ => "Boss: bait jump stomp, then punish during landing recovery."
            };

            label.text = $"AI TACTICAL ADVISOR (F1)\n{healthAdvice}\n{upgradeAdvice}\n{positionAdvice}";
        }
    }
}
