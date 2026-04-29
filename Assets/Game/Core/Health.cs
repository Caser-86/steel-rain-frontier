using System;
using UnityEngine;

namespace SteelRain.Core
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] private int max = 5;
        [SerializeField] private Team team = Team.Enemy;

        public int Current { get; private set; }
        public int Max => max;
        public Team Team => team;

        public event Action<int, int> Changed;
        public event Action<DamageInfo> Damaged;
        public event Action Died;

        private bool dead;

        private void Awake()
        {
            Current = max;
        }

        public void Initialize(int maxHealth, Team assignedTeam)
        {
            max = Mathf.Max(1, maxHealth);
            team = assignedTeam;
            Current = max;
            dead = false;
            Changed?.Invoke(Current, max);
        }

        public void ApplyDamage(DamageInfo info)
        {
            if (dead || info.Amount <= 0 || info.SourceTeam == team)
                return;

            Current = Mathf.Max(0, Current - info.Amount);
            Damaged?.Invoke(info);
            Changed?.Invoke(Current, max);

            if (Current == 0)
            {
                dead = true;
                Died?.Invoke();
            }
        }
    }
}
