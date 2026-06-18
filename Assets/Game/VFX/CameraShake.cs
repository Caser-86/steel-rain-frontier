using System.Collections;
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.VFX
{
    public sealed class CameraShake : MonoBehaviour
    {
        [SerializeField] private float defaultDuration = 0.2f;
        [SerializeField] private float defaultStrength = 0.15f;

        private Vector3 origin;

        private void Awake()
        {
            origin = transform.localPosition;
        }

        public void Shake()
        {
            var intensity = SaveSystem.LoadScreenShakeIntensity();
            Shake(defaultDuration, defaultStrength * intensity);
        }

        public void Shake(float duration, float strength)
        {
            var intensity = SaveSystem.LoadScreenShakeIntensity();
            StartCoroutine(ShakeRoutine(duration, strength * intensity));
        }

        private IEnumerator ShakeRoutine(float duration, float strength)
        {
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
