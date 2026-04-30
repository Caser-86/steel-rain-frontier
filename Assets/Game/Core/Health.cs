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
        public event Func<DamageInfo, DamageInfo> DamageFilter;

        private bool dead;
        private float invulnerableUntil;

        private void Awake()
        {
            Current = max;
        }

        public void Initialize(int maxHealth, Team assignedTeam)
        {
            Initialize(maxHealth, assignedTeam, maxHealth);
        }

        public void Initialize(int maxHealth, Team assignedTeam, int currentHealth)
        {
            max = Mathf.Max(1, maxHealth);
            team = assignedTeam;
            Current = Mathf.Clamp(currentHealth, 0, max);
            dead = Current == 0;
            Changed?.Invoke(Current, max);
        }

        public void ApplyDamage(DamageInfo info)
        {
            if (dead || info.Amount <= 0 || info.SourceTeam == team)
                return;

            if (Time.time < invulnerableUntil)
                return;

            var finalInfo = DamageFilter != null ? DamageFilter.Invoke(info) : info;
            if (finalInfo.Amount <= 0)
                return;

            Current = Mathf.Max(0, Current - finalInfo.Amount);
            Damaged?.Invoke(finalInfo);
            Changed?.Invoke(Current, max);

            if (Current == 0)
            {
                dead = true;
                Died?.Invoke();
            }
        }

        public void SetInvulnerable(float duration)
        {
            invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + duration);
        }

        public void Heal(int amount)
        {
            if (dead || amount <= 0)
                return;

            Current = Mathf.Min(max, Current + amount);
            Changed?.Invoke(Current, max);
        }
    }
}
