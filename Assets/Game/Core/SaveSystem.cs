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

        public static void SaveCheckpoint(Vector3 position)
        {
            PlayerPrefs.SetFloat(KeyCheckpointX, position.x);
            PlayerPrefs.SetFloat(KeyCheckpointY, position.y);
            PlayerPrefs.Save();
        }

        public static Vector3 LoadCheckpoint(Vector3 fallback)
        {
            if (!PlayerPrefs.HasKey(KeyCheckpointX))
                return fallback;
            var x = PlayerPrefs.GetFloat(KeyCheckpointX, fallback.x);
            var y = PlayerPrefs.GetFloat(KeyCheckpointY, fallback.y);
            return new Vector3(x, y, 0f);
        }

        public static void SaveWeaponLevel(string weaponId, int level)
        {
            PlayerPrefs.SetInt(KeyWeaponLevel + weaponId, level);
            PlayerPrefs.Save();
        }

        public static int LoadWeaponLevel(string weaponId)
        {
            return PlayerPrefs.GetInt(KeyWeaponLevel + weaponId, 0);
        }

        public static void SaveVolume(float master, float music, float sfx)
        {
            PlayerPrefs.SetFloat(KeyMasterVolume, master);
            PlayerPrefs.SetFloat(KeyMusicVolume, music);
            PlayerPrefs.SetFloat(KeySfxVolume, sfx);
            PlayerPrefs.Save();
        }

        public static float LoadMasterVolume() => PlayerPrefs.GetFloat(KeyMasterVolume, 1f);
        public static float LoadMusicVolume() => PlayerPrefs.GetFloat(KeyMusicVolume, 0.7f);
        public static float LoadSfxVolume() => PlayerPrefs.GetFloat(KeySfxVolume, 1f);

        public static void SaveDisplaySettings(bool fullscreen, int width, int height)
        {
            PlayerPrefs.SetInt(KeyFullscreen, fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(KeyResolutionW, width);
            PlayerPrefs.SetInt(KeyResolutionH, height);
            PlayerPrefs.Save();
        }

        public static bool LoadFullscreen() => PlayerPrefs.GetInt(KeyFullscreen, 1) == 1;
        public static int LoadResolutionW() => PlayerPrefs.GetInt(KeyResolutionW, 1920);
        public static int LoadResolutionH() => PlayerPrefs.GetInt(KeyResolutionH, 1080);

        public static void SaveScreenShakeIntensity(float intensity)
        {
            PlayerPrefs.SetFloat(KeyScreenShake, intensity);
            PlayerPrefs.Save();
        }

        public static float LoadScreenShakeIntensity() => PlayerPrefs.GetFloat(KeyScreenShake, 1f);

        public static int LoadHighScore() => PlayerPrefs.GetInt(KeyHighScore, 0);

        public static void SaveHighScore(int score)
        {
            if (score > LoadHighScore())
            {
                PlayerPrefs.SetInt(KeyHighScore, score);
                PlayerPrefs.Save();
            }
        }

        public static void ClearAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}
