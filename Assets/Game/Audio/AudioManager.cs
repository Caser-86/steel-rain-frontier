using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Audio
{
    /// <summary>
    /// 全局音效播放器，挂载一个 AudioSource 池。
    /// 通过 AudioManager.Play("sfx_gunshot") 调用。
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField] private int poolSize = 8;
        [SerializeField] private AudioClip[] clipsToLoad;

        private static AudioManager instance;
        private AudioSource[] sources;
        private int nextSource;
        private static float masterVolume = 1f;
        private static float musicVolume = 0.7f;
        private static float sfxVolume = 1f;

        private void Awake()
        {
            instance = this;
            sources = new AudioSource[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject($"AudioSource_{i}");
                go.transform.SetParent(transform);
                sources[i] = go.AddComponent<AudioSource>();
                sources[i].playOnAwake = false;
                sources[i].spatialBlend = 0f;
            }

            masterVolume = SaveSystem.LoadMasterVolume();
            musicVolume = SaveSystem.LoadMusicVolume();
            sfxVolume = SaveSystem.LoadSfxVolume();
        }

        public static void Play(string clipName, float volume = 1f)
        {
            if (instance == null || instance.sources == null) return;
            var clip = instance.FindClip(clipName);
            if (clip == null) return;

            var src = instance.sources[instance.nextSource];
            instance.nextSource = (instance.nextSource + 1) % instance.poolSize;
            src.clip = clip;
            src.volume = volume * sfxVolume * masterVolume;
            src.Play();
        }

        public static void SetMasterVolume(float v) => masterVolume = Mathf.Clamp01(v);
        public static void SetMusicVolume(float v) => musicVolume = Mathf.Clamp01(v);
        public static void SetSfxVolume(float v) => sfxVolume = Mathf.Clamp01(v);
        public static float GetMasterVolume() => masterVolume;
        public static float GetMusicVolume() => musicVolume;
        public static float GetSfxVolume() => sfxVolume;

        private AudioClip FindClip(string name)
        {
            if (clipsToLoad == null) return null;
            foreach (var c in clipsToLoad)
            {
                if (c != null && c.name == name)
                    return c;
            }
            return null;
        }
    }
}
