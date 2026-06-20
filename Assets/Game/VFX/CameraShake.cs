using System.Collections;
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.VFX
{
    public sealed class CameraShake : MonoBehaviour
    {
        [SerializeField] private float defaultDuration = 0.2f;
        [SerializeField] private float defaultStrength = 0.15f;

        private Vector3 shakeOffset;
        private Coroutine currentShake;

        public Vector3 ShakeOffset => shakeOffset;

        public void Shake()
        {
            var intensity = SaveSystem.LoadScreenShakeIntensity();
            Shake(defaultDuration, defaultStrength * intensity);
        }

        public void Shake(float duration, float strength)
        {
            var intensity = SaveSystem.LoadScreenShakeIntensity();
            if (currentShake != null) StopCoroutine(currentShake);
            currentShake = StartCoroutine(ShakeRoutine(duration, strength * intensity));
        }

        private IEnumerator ShakeRoutine(float duration, float strength)
        {
            var endTime = Time.time + duration;
            while (Time.time < endTime)
            {
                var offset = Random.insideUnitCircle * strength;
                shakeOffset = new Vector3(offset.x, offset.y, 0f);
                yield return null;
            }
            shakeOffset = Vector3.zero;
            currentShake = null;
        }
    }
}
