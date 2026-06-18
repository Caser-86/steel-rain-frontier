using UnityEngine;

namespace SteelRain.Audio
{
    /// <summary>
    /// 背景音乐播放器，根据玩家位置自动切换段落音乐。
    /// </summary>
    public sealed class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private AudioClip beachMusic;
        [SerializeField] private AudioClip villageMusic;
        [SerializeField] private AudioClip bossMusic;
        [SerializeField] private float beachEndX = 45f;
        [SerializeField] private float bossStartX = 120f;
        [SerializeField] private float baseVolume = 0.3f;
        [SerializeField] private float crossfadeDuration = 1.5f;

        private AudioSource sourceA;
        private AudioSource sourceB;
        private AudioSource activeSource;
        private AudioClip currentClip;
        private float crossfadeTimer;
        private bool crossfading;

        private void Awake()
        {
            sourceA = gameObject.AddComponent<AudioSource>();
            sourceA.loop = true;
            sourceA.volume = baseVolume;
            sourceA.spatialBlend = 0f;

            sourceB = gameObject.AddComponent<AudioSource>();
            sourceB.loop = true;
            sourceB.volume = 0f;
            sourceB.spatialBlend = 0f;

            activeSource = sourceA;
        }

        private void Update()
        {
            if (player == null) return;

            var x = player.position.x;
            AudioClip target;

            if (x < beachEndX)
                target = beachMusic;
            else if (x < bossStartX)
                target = villageMusic;
            else
                target = bossMusic;

            var vol = baseVolume * AudioManager.GetMusicVolume() * AudioManager.GetMasterVolume();

            if (crossfading)
            {
                crossfadeTimer += Time.deltaTime;
                var t = Mathf.Clamp01(crossfadeTimer / crossfadeDuration);
                activeSource.volume = vol * t;
                var otherSource = activeSource == sourceA ? sourceB : sourceA;
                otherSource.volume = vol * (1f - t);

                if (t >= 1f)
                {
                    otherSource.Stop();
                    crossfading = false;
                }
            }
            else
            {
                activeSource.volume = vol;
            }

            if (target != null && target != currentClip)
            {
                currentClip = target;
                var newSource = activeSource == sourceA ? sourceB : sourceA;
                newSource.clip = target;
                newSource.volume = 0f;
                newSource.Play();
                crossfadeTimer = 0f;
                crossfading = true;
                activeSource = newSource;
            }
            else if (currentClip != null && !activeSource.isPlaying)
            {
                activeSource.Play();
            }
        }
    }
}
