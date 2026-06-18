using System;
using System.IO;
using UnityEngine;

namespace SteelRain.Audio
{
    /// <summary>
    /// 程序化生成背景音乐 WAV。
    /// 生成三段：海滩探索、村落战斗、Boss 战斗。
    /// 每段约 60-90 秒，循环播放。
    /// </summary>
    public static class MusicGenerator
    {
        private const int SampleRate = 44100;
        private const string OutputDir = "Assets/Audio/Generated";

        public static void GenerateAll()
        {
            if (!Directory.Exists(OutputDir))
                Directory.CreateDirectory(OutputDir);

            GenerateBeachTheme();
            GenerateVillageTheme();
            GenerateBossTheme();

            Debug.Log("[MusicGenerator] All background music generated.");
        }

        // ===== 海滩段：军鼓节奏 + 低频 pad =====
        public static AudioClip GenerateBeachTheme()
        {
            float duration = 80f;
            int totalSamples = (int)(SampleRate * duration);
            float[] data = new float[totalSamples];

            float bpm = 100f;
            float beatInterval = 60f / bpm;
            int beatSamples = (int)(SampleRate * beatInterval);

            // 军鼓鼓点模式：每拍一个底鼓，第3拍加军鼓
            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float beatPos = (i % beatSamples) / (float)beatSamples;
                int beatIndex = i / beatSamples;
                int beatInBar = beatIndex % 4;

                float sample = 0f;

                // 低频 pad（持续）
                sample += Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.08f;
                sample += Mathf.Sin(2f * Mathf.PI * 165f * t) * 0.05f;

                // 底鼓
                if (beatPos < 0.15f)
                {
                    float kickEnv = Mathf.Exp(-beatPos * 20f);
                    float kick = Mathf.Sin(2f * Mathf.PI * (60f - beatPos * 200f) * t) * kickEnv * 0.5f;
                    sample += kick;
                }

                // 军鼓（第2、4拍）
                if ((beatInBar == 1 || beatInBar == 3) && beatPos > 0.4f && beatPos < 0.6f)
                {
                    float snareEnv = Mathf.Exp(-(beatPos - 0.4f) * 30f);
                    float snare = (UnityEngine.Random.value * 2f - 1f) * snareEnv * 0.2f;
                    sample += snare;
                }

                // 旋律线（A 小调五声音阶）
                int melodyNote = GetMelodyNote(beatIndex, new[] { 0, 3, 5, 7, 10 });
                if (melodyNote >= 0)
                {
                    float melodyFreq = 220f * Mathf.Pow(2f, melodyNote / 12f);
                    float melodyEnv = Mathf.Sin(Mathf.PI * beatPos) * 0.12f;
                    sample += Mathf.Sin(2f * Mathf.PI * melodyFreq * t) * melodyEnv;
                }

                data[i] = Mathf.Clamp(sample * 0.6f, -1f, 1f);
            }

            return SaveWav("music_beach", data);
        }

        // ===== 村落段：稍快，加入打击乐 =====
        public static AudioClip GenerateVillageTheme()
        {
            float duration = 80f;
            int totalSamples = (int)(SampleRate * duration);
            float[] data = new float[totalSamples];

            float bpm = 120f;
            float beatInterval = 60f / bpm;
            int beatSamples = (int)(SampleRate * beatInterval);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float beatPos = (i % beatSamples) / (float)beatSamples;
                int beatIndex = i / beatSamples;
                int beatInBar = beatIndex % 4;

                float sample = 0f;

                // pad
                sample += Mathf.Sin(2f * Mathf.PI * 146.8f * t) * 0.06f;
                sample += Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.04f;

                // 底鼓
                if (beatPos < 0.12f)
                {
                    float env = Mathf.Exp(-beatPos * 25f);
                    sample += Mathf.Sin(2f * Mathf.PI * 50f * t) * env * 0.4f;
                }

                // 军鼓
                if (beatInBar % 2 == 1 && beatPos > 0.45f && beatPos < 0.55f)
                {
                    float env = Mathf.Exp(-(beatPos - 0.45f) * 35f);
                    sample += (UnityEngine.Random.value * 2f - 1f) * env * 0.25f;
                }

                // 高音打击
                if (beatPos > 0.48f && beatPos < 0.52f)
                {
                    float env = Mathf.Exp(-(beatPos - 0.48f) * 50f);
                    sample += Mathf.Sin(2f * Mathf.PI * 1200f * t) * env * 0.08f;
                }

                // 旋律
                int melodyNote = GetMelodyNote(beatIndex, new[] { 0, 2, 3, 7, 10 });
                if (melodyNote >= 0)
                {
                    float freq = 293.6f * Mathf.Pow(2f, melodyNote / 12f);
                    float env = Mathf.Sin(Mathf.PI * beatPos) * 0.1f;
                    sample += Mathf.Sin(2f * Mathf.PI * freq * t) * env;
                }

                data[i] = Mathf.Clamp(sample * 0.6f, -1f, 1f);
            }

            return SaveWav("music_village", data);
        }

        // ===== Boss 段：完整战斗主题，强节奏 =====
        public static AudioClip GenerateBossTheme()
        {
            float duration = 90f;
            int totalSamples = (int)(SampleRate * duration);
            float[] data = new float[totalSamples];

            float bpm = 140f;
            float beatInterval = 60f / bpm;
            int beatSamples = (int)(SampleRate * beatInterval);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float beatPos = (i % beatSamples) / (float)beatSamples;
                int beatIndex = i / beatSamples;
                int beatInBar = beatIndex % 4;
                int halfBeat = (int)(beatPos * 2);

                float sample = 0f;

                // 低频驱动
                sample += Mathf.Sin(2f * Mathf.PI * 82.4f * t) * 0.1f;
                sample += Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.06f;

                // 底鼓（每半拍）
                float halfBeatPos = (i % (beatSamples / 2)) / (float)(beatSamples / 2);
                if (halfBeatPos < 0.1f)
                {
                    float env = Mathf.Exp(-halfBeatPos * 30f);
                    sample += Mathf.Sin(2f * Mathf.PI * 45f * t) * env * 0.5f;
                }

                // 军鼓
                if (beatInBar % 2 == 1 && beatPos > 0.45f && beatPos < 0.6f)
                {
                    float env = Mathf.Exp(-(beatPos - 0.45f) * 30f);
                    sample += (UnityEngine.Random.value * 2f - 1f) * env * 0.3f;
                }

                // 铜管和弦
                int chordIndex = (beatIndex / 4) % 4;
                float[] chordFreqs = GetChord(chordIndex);
                foreach (var freq in chordFreqs)
                {
                    sample += Mathf.Sin(2f * Mathf.PI * freq * t) * 0.04f;
                }

                // 旋律线
                int melodyNote = GetMelodyNote(beatIndex, new[] { 0, 3, 5, 7, 10, 12 });
                if (melodyNote >= 0)
                {
                    float freq = 440f * Mathf.Pow(2f, melodyNote / 12f);
                    float env = Mathf.Sin(Mathf.PI * beatPos) * 0.15f;
                    sample += Mathf.Sin(2f * Mathf.PI * freq * t) * env;
                    // 加入方波叠加增加金属感
                    sample += Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) * env * 0.05f;
                }

                data[i] = Mathf.Clamp(sample * 0.5f, -1f, 1f);
            }

            return SaveWav("music_boss", data);
        }

        private static int GetMelodyNote(int beatIndex, int[] scale)
        {
            // 简单旋律模式：8 拍一个乐句
            int[] pattern = { 0, 2, 4, 2, 0, 4, 2, -1, 0, 2, 4, 5, 4, 2, 0, -1 };
            int pos = beatIndex % pattern.Length;
            if (pattern[pos] < 0) return -1;
            int scaleIndex = pattern[pos] % scale.Length;
            int octave = pattern[pos] / scale.Length;
            return scale[scaleIndex] + octave * 12;
        }

        private static float[] GetChord(int index)
        {
            return index switch
            {
                0 => new[] { 130.8f, 164.8f, 196f },   // C
                1 => new[] { 110f, 164.8f, 196f },      // Am
                2 => new[] { 146.8f, 174.6f, 220f },    // Dm
                3 => new[] { 98f, 123.5f, 146.8f },     // G
                _ => new[] { 130.8f, 164.8f, 196f }
            };
        }

        // ===== WAV 写入 =====

        private static AudioClip SaveWav(string name, float[] data)
        {
            var path = $"{OutputDir}/{name}.wav";
            WriteWavFile(path, data, SampleRate, 1);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.ForceUpdate);
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
