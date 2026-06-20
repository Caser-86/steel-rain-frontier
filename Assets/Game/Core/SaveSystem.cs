using System;
using UnityEngine;

namespace SteelRain.Core
{
    public static class SaveSystem
    {
        private const string KeyCheckpointX = "Save_CheckpointX";
        private const string KeyCheckpointY = "Save_CheckpointY";
        private const string KeyWeaponLevel = "Save_WeaponLevel_";
        private const string KeyMasterVolume = "Settings_MasterVolume";
        private const string KeyMusicVolume = "Settings_MusicVolume";
        private const string KeySfxVolume = "Settings_SfxVolume";
        private const string KeyFullscreen = "Settings_Fullscreen";
        private const string KeyResolutionW = "Settings_ResolutionW";
        private const string KeyResolutionH = "Settings_ResolutionH";
        private const string KeyScreenShake = "Settings_ScreenShake";
        private const string KeyHighScore = "Save_HighScore";
        // 小队存档：存活状态、当前角色、各角色血量、关卡内分数
        private const string KeySquadAlive = "Save_SquadAlive";
        private const string KeySquadActiveIndex = "Save_SquadActiveIndex";
        private const string KeySquadHealth = "Save_SquadHealth_";
        private const string KeyLevelScore = "Save_LevelScore";
        private const string KeyLevelIndex = "Save_LevelIndex";
        private const string KeyMaxHealthBonus = "Save_MaxHealthBonus";

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

        public static void SaveWeaponLevel(string weaponId, int level)
        {
            if (isQuitting) return;
            try
            {
                PlayerPrefs.SetInt(KeyWeaponLevel + weaponId, level);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] SaveWeaponLevel failed: {e.Message}");
            }
        }

        public static int LoadWeaponLevel(string weaponId)
        {
            try
            {
                return PlayerPrefs.GetInt(KeyWeaponLevel + weaponId, 0);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] LoadWeaponLevel failed: {e.Message}");
                return 0;
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

        /// <summary>
        /// 保存小队存活状态（位掩码，bit0=角色0存活...）、当前角色索引、各角色血量。
        /// </summary>
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

        /// <summary>
        /// 清除小队存档（关卡完成或重新开始时调用）。
        /// </summary>
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

        public static void SaveMaxHealthBonus(int bonus)
        {
            if (isQuitting) return;
            try
            {
                PlayerPrefs.SetInt(KeyMaxHealthBonus, bonus);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] SaveMaxHealthBonus failed: {e.Message}");
            }
        }

        public static int LoadMaxHealthBonus()
        {
            try { return PlayerPrefs.GetInt(KeyMaxHealthBonus, 0); }
            catch { return 0; }
        }
    }
}
