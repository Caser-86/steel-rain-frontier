using System.Collections;
using UnityEngine;

namespace SteelRain.VFX
{
    public sealed class CameraShake : MonoBehaviour
    {
        public void Shake(float duration, float strength)
        {
            StartCoroutine(ShakeRoutine(duration, strength));
        }

        private IEnumerator ShakeRoutine(float duration, float strength)
        {
            var origin = transform.localPosition;
            var endTime = Time.time + duration;

            while (Time.time < endTime)
            {
                var offset = Random.insideUnitCircle * strength;
                transform.localPosition = origin + new Vector3(offset.x, offset.y, 0f);
                yield return null;
            }

            transform.localPosition = origin;
        }
    }
}
