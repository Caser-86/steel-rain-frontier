using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// 成就面板：在主菜单中查看已解锁成就和统计。
    /// </summary>
    public sealed class AchievementPanel : MonoBehaviour
    {
        private GameObject panel;
        private Text titleText;
        private Text statsText;
        private Text achievementText;
        private bool built;

        public void Show()
        {
            if (!built) BuildUI();
            if (panel != null)
            {
                panel.SetActive(true);
                RefreshContent();
            }
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void RefreshContent()
        {
            if (achievementText != null)
            {
                var lines = new System.Text.StringBuilder();
                int unlocked = 0;
                int total = AchievementManager.GetTotalCount();
                foreach (var id in AchievementManager.GetAllAchievements())
                {
                    var check = AchievementManager.IsUnlocked(id) ? "[X]" : "[ ]";
                    var name = AchievementManager.GetAchievementName(id);
                    var desc = AchievementManager.GetAchievementDescription(id);
                    lines.AppendLine($"{check} {name} - {desc}");
                    if (AchievementManager.IsUnlocked(id)) unlocked++;
                }
                lines.Insert(0, $"ACHIEVEMENTS ({unlocked}/{total})\n\n");
                achievementText.text = lines.ToString();
            }

            if (statsText != null)
            {
                var kills = AchievementManager.GetStat(AchievementManager.StatId.TotalKills);
                var deaths = AchievementManager.GetStat(AchievementManager.StatId.TotalDeaths);
                var bossKills = AchievementManager.GetStat(AchievementManager.StatId.TotalBossKills);
                var levels = AchievementManager.GetStat(AchievementManager.StatId.LevelsCompleted);
                var playTime = AchievementManager.GetFloatStat(AchievementManager.StatId.TotalPlayTime);
                var hours = Mathf.FloorToInt(playTime / 3600f);
                var mins = Mathf.FloorToInt((playTime % 3600f) / 60f);
                var combo = AchievementManager.GetStat(AchievementManager.StatId.MaxCombo);

                statsText.text =
                    $"STATISTICS\n\n" +
                    $"Total Kills: {kills}\n" +
                    $"Total Deaths: {deaths}\n" +
                    $"Boss Kills: {bossKills}\n" +
                    $"Levels Completed: {levels}\n" +
                    $"Max Combo: {combo}x\n" +
                    $"Play Time: {hours}h {mins}m\n" +
                    $"High Score: {ScoreManager.HighScore}";
            }
        }

        private void BuildUI()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            panel = new GameObject("AchievementPanel");
            panel.transform.SetParent(transform);
            var panelRt = panel.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.06f, 0.07f, 0.1f, 0.95f);

            // 标题
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(panel.transform);
            titleText = titleGo.AddComponent<Text>();
            if (font != null) titleText.font = font;
            titleText.text = "ACHIEVEMENTS & STATS";
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.UpperCenter;
            titleText.color = UIPalette.Primary;
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.1f, 0.88f);
            titleRt.anchorMax = new Vector2(0.9f, 0.98f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            // 统计文本（左半）
            var statsGo = new GameObject("Stats");
            statsGo.transform.SetParent(panel.transform);
            statsText = statsGo.AddComponent<Text>();
            if (font != null) statsText.font = font;
            statsText.fontSize = 18;
            statsText.alignment = TextAnchor.UpperLeft;
            statsText.color = UIPalette.TextPrimary;
            statsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            var statsRt = statsGo.GetComponent<RectTransform>();
            statsRt.anchorMin = new Vector2(0.05f, 0.05f);
            statsRt.anchorMax = new Vector2(0.45f, 0.85f);
            statsRt.offsetMin = Vector2.zero;
            statsRt.offsetMax = Vector2.zero;

            // 成就文本（右半）
            var achGo = new GameObject("Achievements");
            achGo.transform.SetParent(panel.transform);
            achievementText = achGo.AddComponent<Text>();
            if (font != null) achievementText.font = font;
            achievementText.fontSize = 16;
            achievementText.alignment = TextAnchor.UpperLeft;
            achievementText.color = UIPalette.TextSecondary;
            achievementText.horizontalOverflow = HorizontalWrapMode.Wrap;
            var achRt = achGo.GetComponent<RectTransform>();
            achRt.anchorMin = new Vector2(0.5f, 0.05f);
            achRt.anchorMax = new Vector2(0.95f, 0.85f);
            achRt.offsetMin = Vector2.zero;
            achRt.offsetMax = Vector2.zero;

            // 返回提示
            var backGo = new GameObject("BackHint");
            backGo.transform.SetParent(panel.transform);
            var backText = backGo.AddComponent<Text>();
            if (font != null) backText.font = font;
            backText.text = "Press ESC or click BACK to close";
            backText.fontSize = 14;
            backText.alignment = TextAnchor.MiddleCenter;
            backText.color = UIPalette.TextMuted;
            var backRt = backGo.GetComponent<RectTransform>();
            backRt.anchorMin = new Vector2(0.3f, 0f);
            backRt.anchorMax = new Vector2(0.7f, 0.05f);
            backRt.offsetMin = Vector2.zero;
            backRt.offsetMax = Vector2.zero;

            built = true;
        }
    }
}
