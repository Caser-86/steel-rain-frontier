using UnityEngine;

namespace SteelRain.Core
{
    public static class ScoreManager
    {
        private static int score;
        private static int combo;
        private static float comboTimer;
        private const float ComboWindow = 3f;
        private const string KeyScore = "Save_Score";
        private const string KeyHighScore = "Save_HighScore";
        private const string KeyLeaderboard = "Save_Leaderboard_";

        public static int Score => score;
        public static int Combo => combo;
        public static int HighScore => PlayerPrefs.GetInt(KeyHighScore, 0);

        public static event System.Action<int> ScoreChanged;
        public static event System.Action<int> ComboChanged;

        public static void AddKill(int basePoints)
        {
            comboTimer = ComboWindow;
            combo++;
            var points = basePoints * Mathf.Max(1, combo);
            score += points;
            ScoreChanged?.Invoke(score);
            ComboChanged?.Invoke(combo);
            // 军票掉落已由 LootDrop 系统生成物理拾取物，此处不再直接添加
        }

        /// <summary>
        /// 直接添加分数（不触发连击），用于波次奖励等。
        /// </summary>
        public static void AddScore(int points)
        {
            if (points <= 0) return;
            score += points;
            ScoreChanged?.Invoke(score);
        }

        public static void Update()
        {
            if (comboTimer > 0f)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0f)
                {
                    combo = 0;
                    ComboChanged?.Invoke(combo);
                }
            }
        }

        public static void Save()
        {
            PlayerPrefs.SetInt(KeyScore, score);
            SaveHighScore(score);
            PlayerPrefs.Save();
        }

        public static void Load()
        {
            score = PlayerPrefs.GetInt(KeyScore, 0);
        }

        public static void Reset()
        {
            score = 0;
            combo = 0;
            comboTimer = 0f;
            ScoreChanged?.Invoke(score);
            ComboChanged?.Invoke(combo);
        }

        public static void CheckHighScore()
        {
            SaveHighScore(score);
            AddToLeaderboard(score);
        }

        public static void SaveHighScore(int newScore)
        {
            if (newScore > HighScore)
            {
                PlayerPrefs.SetInt(KeyHighScore, newScore);
                PlayerPrefs.Save();
            }
        }

        public static void AddToLeaderboard(int newScore)
        {
            // 不添加0分到排行榜
            if (newScore <= 0) return;

            var scores = GetLeaderboard();
            for (int i = 0; i < scores.Length; i++)
            {
                if (newScore > scores[i])
                {
                    for (int j = scores.Length - 1; j > i; j--)
                        scores[j] = scores[j - 1];
                    scores[i] = newScore;
                    break;
                }
            }
            for (int i = 0; i < scores.Length; i++)
                PlayerPrefs.SetInt(KeyLeaderboard + i, scores[i]);
            PlayerPrefs.Save();
        }

        public static int[] GetLeaderboard()
        {
            var scores = new int[10];
            for (int i = 0; i < 10; i++)
                scores[i] = PlayerPrefs.GetInt(KeyLeaderboard + i, 0);
            return scores;
        }
    }
}
