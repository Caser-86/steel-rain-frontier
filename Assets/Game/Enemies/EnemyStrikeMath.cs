using UnityEngine;

namespace SteelRain.Enemies
{
    public static class EnemyStrikeMath
    {
        public static Vector2 ClampLandingPoint(Vector2 origin, Vector2 target, float maxDistance)
        {
            var delta = target - origin;
            if (delta.magnitude <= maxDistance)
                return target;

            return origin + delta.normalized * maxDistance;
        }
    }
}
