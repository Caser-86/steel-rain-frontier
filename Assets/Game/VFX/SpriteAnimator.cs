using UnityEngine;

namespace SteelRain.VFX
{
    public sealed class SpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] walkFrames;
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private float walkFrameRate = 8f;
        [SerializeField] private float idleFrameRate = 2f;

        private float frameTimer;
        private int currentFrame;
        private bool isWalking;
        private Sprite[] currentSequence;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            currentSequence = idleFrames;
        }

        private void Update()
        {
            if (spriteRenderer == null) return;
            if (currentSequence == null || currentSequence.Length == 0) return;

            frameTimer += Time.deltaTime;
            var rate = isWalking ? walkFrameRate : idleFrameRate;
            if (frameTimer >= 1f / rate)
            {
                frameTimer = 0f;
                currentFrame = (currentFrame + 1) % currentSequence.Length;
                spriteRenderer.sprite = currentSequence[currentFrame];
            }
        }

        public void SetWalking(bool walking)
        {
            if (isWalking == walking) return;
            isWalking = walking;
            currentFrame = 0;
            frameTimer = 0f;
            currentSequence = walking ? walkFrames : idleFrames;
            if (currentSequence != null && currentSequence.Length > 0 && spriteRenderer != null)
                spriteRenderer.sprite = currentSequence[0];
        }
    }
}
