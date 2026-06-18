using UnityEngine;

namespace SteelRain.Core
{
    public readonly struct DamageInfo
    {
        public readonly int Amount;
        public readonly Team SourceTeam;
        public readonly Vector2 Direction;

        public DamageInfo(int amount, Team sourceTeam, Vector2 direction)
        {
            Amount = amount;
            SourceTeam = sourceTeam;
            Direction = direction;
        }
    }
}
