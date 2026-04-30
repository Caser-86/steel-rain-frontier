using System.Collections;
using UnityEngine;

namespace SteelRain.VFX
{
    public sealed class CameraShake : MonoBehaviour
    {
        public static CameraShake Active { get; private set; }
        public Vector3 Offset { get; private set; }

        private Coroutine routine;

        private void Awake()
        {
            Active = this;
        }

        private void OnDestroy()
        {
            if (Active == this)
                Active = null;
        }

        public static void ShakeGlobal(float duration, float strength)
        {
            if (Active != null)
                Active.Shake(duration, strength);
        }

        public void Shake(float duration, float strength)
        {
            if (routine != null)
                StopCoroutine(routine);

            routine = StartCoroutine(ShakeRoutine(duration, strength));
        }

        private IEnumerator ShakeRoutine(float duration, float strength)
        {
            var endTime = Time.time + duration;

            while (Time.time < endTime)
            {
                var offset = Random.insideUnitCircle * strength;
                Offset = new Vector3(offset.x, offset.y, 0f);
                yield return null;
            }

            Offset = Vector3.zero;
            routine = null;
        }
    }
}
