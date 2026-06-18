using System;
using System.IO;
using UnityEngine;

namespace SteelRain.Audio
{
    /// <summary>
    /// 程序化生成 WAV 音效，无需任何外部音频软件。
    /// 在 Editor Script 中调用 GenerateAll() 一次性生成全部音效。
    /// </summary>
    public static class AudioGenerator
    {
        private const int SampleRate = 44100;
        private const string OutputDir = "Assets/Audio/Generated";

        public static void GenerateAll()
        {
            if (!Directory.Exists(OutputDir))
                Directory.CreateDirectory(OutputDir);

            GenerateGunShot();
            GenerateExplosion();
            GenerateJump();
            GenerateHurt();
            GeneratePickup();
            GenerateUpgrade();
            GenerateEnemyShoot();
            GenerateCheckPoint();
            GenerateBossHit();
            GenerateDodge();
            GenerateFormSwitch();
            GenerateVictory();
            GenerateGameOver();
            GenerateUIClick();

            Debug.Log("[AudioGenerator] All sound effects generated.");
        }

        // ===== 单个音效生成 =====

        public static AudioClip GenerateGunShot()
        {
            float duration = 0.08f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float noise = UnityEngine.Random.value * 2f - 1f;
                float tone = Mathf.Sin(2f * Mathf.PI * 120f * t);
                float env = Mathf.Exp(-t * 30f);
                data[i] = (noise * 0.7f + tone * 0.3f) * env * 0.8f;
            }
            return SaveWav("sfx_gunshot", data);
        }

        public static AudioClip GenerateExplosion()
        {
            float duration = 0.5f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float noise = UnityEngine.Random.value * 2f - 1f;
                float lowBoom = Mathf.Sin(2f * Mathf.PI * 60f * t);
                float env = Mathf.Exp(-t * 5f);
                data[i] = (noise * 0.5f + lowBoom * 0.5f) * env;
            }
            return SaveWav("sfx_explosion", data);
        }

        public static AudioClip GenerateJump()
        {
            float duration = 0.15f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = 300f + 800f * t;
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float env = Mathf.Sin(Mathf.PI * t);
                data[i] = wave * env * 0.4f;
            }
            return SaveWav("sfx_jump", data);
        }

        public static AudioClip GenerateHurt()
        {
            float duration = 0.12f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float noise = UnityEngine.Random.value * 2f - 1f;
                float env = Mathf.Exp(-t * 15f);
                data[i] = noise * env * 0.5f;
            }
            return SaveWav("sfx_hurt", data);
        }

        public static AudioClip GeneratePickup()
        {
            float duration = 0.2f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = 440f + 660f * t;
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float env = Mathf.Exp(-t * 4f);
                data[i] = wave * env * 0.3f;
            }
            return SaveWav("sfx_pickup", data);
        }

        public static AudioClip GenerateUpgrade()
        {
            float duration = 0.4f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float f1 = Mathf.Sin(2f * Mathf.PI * 523f * t);
                float f2 = Mathf.Sin(2f * Mathf.PI * 659f * t);
                float f3 = Mathf.Sin(2f * Mathf.PI * 784f * t);
                float env = Mathf.Exp(-t * 3f);
                data[i] = (f1 + f2 + f3) / 3f * env * 0.3f;
            }
            return SaveWav("sfx_upgrade", data);
        }

        public static AudioClip GenerateEnemyShoot()
        {
            float duration = 0.06f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float noise = UnityEngine.Random.value * 2f - 1f;
                float tone = Mathf.Sin(2f * Mathf.PI * 200f * t);
                float env = Mathf.Exp(-t * 40f);
                data[i] = (noise * 0.6f + tone * 0.4f) * env * 0.6f;
            }
            return SaveWav("sfx_enemy_shoot", data);
        }

        public static AudioClip GenerateCheckPoint()
        {
            float duration = 0.5f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float f1 = Mathf.Sin(2f * Mathf.PI * 659f * t);
                float f2 = Mathf.Sin(2f * Mathf.PI * 880f * t);
                float env = Mathf.Exp(-t * 2.5f);
                data[i] = (f1 * 0.6f + f2 * 0.4f) * env * 0.25f;
            }
            return SaveWav("sfx_checkpoint", data);
        }

        public static AudioClip GenerateBossHit()
        {
            float duration = 0.3f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float noise = UnityEngine.Random.value * 2f - 1f;
                float metal = Mathf.Sin(2f * Mathf.PI * 150f * t);
                float env = Mathf.Exp(-t * 8f);
                data[i] = (noise * 0.3f + metal * 0.7f) * env * 0.5f;
            }
            return SaveWav("sfx_boss_hit", data);
        }

        public static AudioClip GenerateDodge()
        {
            float duration = 0.1f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = 600f - 400f * t;
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float env = Mathf.Exp(-t * 20f);
                data[i] = wave * env * 0.3f;
            }
            return SaveWav("sfx_dodge", data);
        }

        public static AudioClip GenerateFormSwitch()
        {
            float duration = 0.15f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float f1 = Mathf.Sin(2f * Mathf.PI * 800f * t);
                float f2 = Mathf.Sin(2f * Mathf.PI * 1200f * t);
                float env = Mathf.Exp(-t * 10f);
                data[i] = (f1 + f2) * 0.5f * env * 0.2f;
            }
            return SaveWav("sfx_form_switch", data);
        }

        public static AudioClip GenerateVictory()
        {
            float duration = 1.5f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            float[] notes = { 523f, 659f, 784f, 1047f };
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                int noteIdx = Mathf.Clamp((int)(t * notes.Length), 0, notes.Length - 1);
                float freq = notes[noteIdx];
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float env = Mathf.Exp(-t * 1.5f);
                data[i] = wave * env * 0.3f;
            }
            return SaveWav("sfx_victory", data);
        }

        public static AudioClip GenerateGameOver()
        {
            float duration = 1.0f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = 300f - 200f * t;
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float env = Mathf.Exp(-t * 2f);
                data[i] = wave * env * 0.3f;
            }
            return SaveWav("sfx_gameover", data);
        }

        public static AudioClip GenerateUIClick()
        {
            float duration = 0.05f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = 1000f;
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float env = Mathf.Exp(-t * 40f);
                data[i] = wave * env * 0.2f;
            }
            return SaveWav("sfx_ui_click", data);
        }

        // ===== WAV 写入 =====

        private static AudioClip SaveWav(string name, float[] data)
        {
            var path = $"{OutputDir}/{name}.wav";
            WriteWavFile(path, data, SampleRate, 1);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.ForceUpdate);
            var importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.AudioImporter;
            if (importer != null)
            {
                importer.forceToMono = true;
                importer.SaveAndReimport();
            }
            return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
#else
            return null;
#endif
        }

        private static void WriteWavFile(string path, float[] data, int sampleRate, int channels)
        {
            int samples = data.Length;
            int dataSize = samples * 2;

            using var fs = new FileStream(path, FileMode.Create);
            using var bw = new BinaryWriter(fs);

            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + dataSize);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);
            bw.Write((short)1);
            bw.Write((short)channels);
            bw.Write(sampleRate);
            bw.Write(sampleRate * channels * 2);
            bw.Write((short)(channels * 2));
            bw.Write((short)16);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write(dataSize);

            for (int i = 0; i < samples; i++)
            {
                short val = (short)(Mathf.Clamp(data[i], -1f, 1f) * 32767f);
                bw.Write(val);
            }
        }
    }
}
