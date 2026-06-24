using System;
using UnityEngine;

namespace SteelRain.Core
{
    /// <summary>
    /// 存档系统（合金弹头简化版）。
    /// 仅保留：检查点、小队存活、关卡进度、设置、最高分。
    /// 无 NGP、无武器等级、无商店进度。
    /// </summary>
    public static class SaveSystem
    {
        private const string KeyCheckpointX = "Save_CheckpointX";
        private const string KeyCheckpointY = "Save_CheckpointY";
        private const string KeyMasterVolume = "Settings_MasterVolume";
        private const string KeyMusicVolume = "Settings_MusicVolume";
        private const string KeySfxVolume = "Settings_SfxVolume";
        private const string KeyFullscreen = "Settings_Fullscreen";
        private const string KeyResolutionW = "Settings_ResolutionW";
        private const string KeyResolutionH = "Settings_ResolutionH";
        private const string KeyScreenShake = "Settings_ScreenShake";
        private const string KeyHighScore = "Save_HighScore";
        private const string KeySquadAlive = "Save_SquadAlive";
        private const string KeySquadActiveIndex = "Save_SquadActiveIndex";
        private const string KeySquadHealth = "Save_SquadHealth_";
        private const string KeyLevelScore = "Save_LevelScore";
        private const string KeyLevelIndex = "Save_LevelIndex";
        private const string KeyEndlessUnlocked = "Save_EndlessUnlocked";
        private const string KeyEndlessBestWave = "Save_EndlessBestWave";

        private static bool isQuitting;

        public static bool IsQuitting => isQuitting;

        public static void SetQuitting() => isQuitting = true;

        public static void SaveCheckpoint(Vector3 position)
        {
            if (isQuitting) return;
            try
            {
                PlayerPrefs.SetFloat(KeyCheckpointX, position.x);
                PlayerPrefs.SetFloat(KeyCheckpointY, position.y);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] SaveCheckpoint failed: {e.Message}");
            }
        }

        public static Vector3 LoadCheckpoint(Vector3 fallback)
        {
            try
            {
                if (!PlayerPrefs.HasKey(KeyCheckpointX))
                    return fallback;
                var x = PlayerPrefs.GetFloat(KeyCheckpointX, fallback.x);
                var y = PlayerPrefs.GetFloat(KeyCheckpointY, fallback.y);
                return new Vector3(x, y, 0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] LoadCheckpoint failed: {e.Message}");
                return fallback;
            }
        }

        public static void SaveVolume(float master, float music, float sfx)
        {
            if (isQuitting) return;
            try
            {
                PlayerPrefs.SetFloat(KeyMasterVolume, master);
                PlayerPrefs.SetFloat(KeyMusicVolume, music);
                PlayerPrefs.SetFloat(KeySfxVolume, sfx);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] SaveVolume failed: {e.Message}");
            }
        }

        public static float LoadMasterVolume()
        {
            try { return PlayerPrefs.GetFloat(KeyMasterVolume, 1f); }
            catch { return 1f; }
        }
        public static float LoadMusicVolume()
        {
            try { return PlayerPrefs.GetFloat(KeyMusicVolume, 0.7f); }
            catch { return 0.7f; }
        }
        public static float LoadSfxVolume()
        {
            try { return PlayerPrefs.GetFloat(KeySfxVolume, 1f); }
            catch { return 1f; }
        }

        public static void SaveDisplaySettings(bool fullscreen, int width, int height)
        {
            if (isQuitting) return;
            try
            {
                PlayerPrefs.SetInt(KeyFullscreen, fullscreen ? 1 : 0);
                PlayerPrefs.SetInt(KeyResolutionW, width);
                PlayerPrefs.SetInt(KeyResolutionH, height);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] SaveDisplaySettings failed: {e.Message}");
            }
        }

        public static bool LoadFullscreen()
        {
            try { return PlayerPrefs.GetInt(KeyFullscreen, 1) == 1; }
            catch { return true; }
        }
        public static int LoadResolutionW()
        {
            try { return PlayerPrefs.GetInt(KeyResolutionW, 1920); }
            catch { return 1920; }
        }
        public static int LoadResolutionH()
        {
            try { return PlayerPrefs.GetInt(KeyResolutionH, 1080); }
            catch { return 1080; }
        }

        public static void SaveScreenShakeIntensity(float intensity)
        {
            if (isQuitting) return;
            try
            {
                PlayerPrefs.SetFloat(KeyScreenShake, intensity);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] SaveScreenShakeIntensity failed: {e.Message}");
            }
        }

        public static float LoadScreenShakeIntensity()
        {
            try { return PlayerPrefs.GetFloat(KeyScreenShake, 1f); }
            catch { return 1f; }
        }

        public static int LoadHighScore()
        {
            try { return PlayerPrefs.GetInt(KeyHighScore, 0); }
            catch { return 0; }
        }

        public static void SaveHighScore(int score)
        {
            if (isQuitting) return;
            try
            {
                if (score > LoadHighScore())
                {
                    PlayerPrefs.SetInt(KeyHighScore, score);
                    PlayerPrefs.Save();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] SaveHighScore failed: {e.Message}");
            }
        }

        public static void ClearAll()
        {
            if (isQuitting) return;
            try
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] ClearAll failed: {e.Message}");
            }
        }

        // ===== 小队与关卡进度存档 =====

        public static void SaveSquadState(int aliveMask, int activeIndex, int[] healths)
        {
            if (isQuitting) return;
            try
            {
                PlayerPrefs.SetInt(KeySquadAlive, aliveMask);
                PlayerPrefs.SetInt(KeySquadActiveIndex, activeIndex);
                if (healths != null)
                {
                    for (int i = 0; i < healths.Length; i++)
                        PlayerPrefs.SetInt(KeySquadHealth + i, healths[i]);
                }
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] SaveSquadState failed: {e.Message}");
            }
        }

        public static int LoadSquadAliveMask()
        {
            try { return PlayerPrefs.GetInt(KeySquadAlive, 0b1111); }
            catch { return 0b1111; }
        }

        public static int LoadSquadActiveIndex()
        {
            try { return PlayerPrefs.GetInt(KeySquadActiveIndex, 0); }
            catch { return 0; }
        }

        public static int LoadSquadHealth(int index)
        {
            try { return PlayerPrefs.GetInt(KeySquadHealth + index, -1); }
            catch { return -1; }
        }

        public static bool HasSquadSave()
        {
            try { return PlayerPrefs.HasKey(KeySquadAlive); }
            catch { return false; }
        }

        public static bool HasCheckpointSave()
        {
            try { return PlayerPrefs.HasKey(KeyCheckpointX); }
            catch { return false; }
        }

        public static void ClearSquadSave()
        {
            try
            {
                PlayerPrefs.DeleteKey(KeySquadAlive);
                PlayerPrefs.DeleteKey(KeySquadActiveIndex);
                for (int i = 0; i < 4; i++)
                    PlayerPrefs.DeleteKey(KeySquadHealth + i);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] ClearSquadSave failed: {e.Message}");
            }
        }

        public static void SaveLevelScore(int score)
        {
            if (isQuitting) return;
            try
            {
                PlayerPrefs.SetInt(KeyLevelScore, score);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] SaveLevelScore failed: {e.Message}");
            }
        }

        public static int LoadLevelScore()
        {
            try { return PlayerPrefs.GetInt(KeyLevelScore, 0); }
            catch { return 0; }
        }

        public static void SaveLevelIndex(int index)
        {
            if (isQuitting) return;
            try
            {
                PlayerPrefs.SetInt(KeyLevelIndex, index);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] SaveLevelIndex failed: {e.Message}");
            }
        }

        public static int LoadLevelIndex()
        {
            try { return PlayerPrefs.GetInt(KeyLevelIndex, 0); }
            catch { return 0; }
        }

        // ===== 无尽模式解锁 =====

        public static bool IsEndlessUnlocked()
        {
            try { return PlayerPrefs.GetInt(KeyEndlessUnlocked, 0) == 1; }
            catch { return false; }
        }

        public static void UnlockEndlessMode()
        {
            if (isQuitting) return;
            try
            {
                PlayerPrefs.SetInt(KeyEndlessUnlocked, 1);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] UnlockEndlessMode failed: {e.Message}");
            }
        }

        /// <summary>
        /// 获取无尽模式最高波次。
        /// </summary>
        public static int GetEndlessBestWave()
        {
            try { return PlayerPrefs.GetInt(KeyEndlessBestWave, 0); }
            catch { return 0; }
        }

        /// <summary>
        /// 更新无尽模式最高波次（仅在超过记录时保存）。
        /// </summary>
        public static void UpdateEndlessBestWave(int wave)
        {
            if (isQuitting || wave <= 0) return;
            try
            {
                var current = PlayerPrefs.GetInt(KeyEndlessBestWave, 0);
                if (wave > current)
                {
                    PlayerPrefs.SetInt(KeyEndlessBestWave, wave);
                    PlayerPrefs.Save();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] UpdateEndlessBestWave failed: {e.Message}");
            }
        }
    }
}
