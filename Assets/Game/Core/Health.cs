using System;
using SteelRain.Audio;
using SteelRain.Player;
using SteelRain.VFX;
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
        public bool IsDead => dead;

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

        public void InitializeWithCurrent(int maxHealth, Team assignedTeam, int currentHealth)
        {
            max = Mathf.Max(1, maxHealth);
            team = assignedTeam;
            Current = Mathf.Clamp(Mathf.Max(1, currentHealth), 1, max);
            dead = false;
            Changed?.Invoke(Current, max);
        }

        public void ApplyDamage(DamageInfo info)
        {
            if (dead || info.Amount <= 0 || info.SourceTeam == team)
                return;

            if (team == Team.Player && CharacterSkill.ShieldActive)
            {
                var player = GetComponent<PlayerController2D>();
                if (player != null)
                {
                    var attackFromRight = info.Direction.x > 0;
                    var playerFacesRight = player.AimDirection.x >= 0f;
                    if (attackFromRight == playerFacesRight)
                    {
                        AudioManager.Play("sfx_boss_hit", 0.3f);
                        return;
                    }
                }
            }

            Current = Mathf.Max(0, Current - info.Amount);
            Damaged?.Invoke(info);
            Changed?.Invoke(Current, max);

            DamageNumberSpawner.Spawn(transform.position + Vector3.up * 0.5f, info.Amount);

            if (team == Team.Player)
                AudioManager.Play("sfx_hurt", 0.6f);

            if (Current == 0)
            {
                dead = true;
                Died?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (dead || amount <= 0)
                return;

            Current = Mathf.Min(max, Current + amount);
            Changed?.Invoke(Current, max);
        }

        public void Revive(int reviveHealth)
        {
            dead = false;
            Current = Mathf.Clamp(reviveHealth, 1, max);
            Changed?.Invoke(Current, max);
        }

        public void ReviveFull()
        {
            dead = false;
            Current = max;
            Changed?.Invoke(Current, max);
        }
    }
}
