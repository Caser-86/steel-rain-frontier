using System.Collections;
using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// 成就解锁通知：屏幕右上角显示成就解锁提示。
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public sealed class AchievementNotification : MonoBehaviour
    {
        [SerializeField] private float displayDuration = 4f;
        [SerializeField] private float slideInDuration = 0.5f;

        private Canvas canvas;
        private GameObject notificationPanel;
        private Text titleText;
        private Text nameText;
        private Text descText;
        private Image iconImage;
        private RectTransform panelRect;
        private Vector2 hiddenPosition;
        private Vector2 visiblePosition;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            BuildNotificationUI();
        }

        private void OnEnable()
        {
            AchievementManager.AchievementUnlocked += OnAchievementUnlocked;
        }

        private void OnDisable()
        {
            AchievementManager.AchievementUnlocked -= OnAchievementUnlocked;
        }

        private void BuildNotificationUI()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 通知面板
            notificationPanel = new GameObject("AchievementNotification");
            notificationPanel.transform.SetParent(transform);
            panelRect = notificationPanel.AddComponent<RectTransform>();

            // 锚定在右上角
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            visiblePosition = new Vector2(-20f, -20f);
            hiddenPosition = new Vector2(400f, -20f);
            panelRect.anchoredPosition = hiddenPosition;
            panelRect.sizeDelta = new Vector2(360f, 90f);

            var panelImage = notificationPanel.AddComponent<Image>();
            panelImage.color = new Color(0.08f, 0.09f, 0.12f, 0.95f);

            // 边框
            var borderGo = new GameObject("Border");
            borderGo.transform.SetParent(notificationPanel.transform);
            var borderRt = borderGo.AddComponent<RectTransform>();
            borderRt.anchorMin = Vector2.zero;
            borderRt.anchorMax = Vector2.one;
            borderRt.offsetMin = Vector2.zero;
            borderRt.offsetMax = Vector2.zero;
            var borderImg = borderGo.AddComponent<Image>();
            borderImg.color = UIPalette.Accent;

            // 标题文字
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(notificationPanel.transform);
            titleText = titleGo.AddComponent<Text>();
            titleText.text = "ACHIEVEMENT UNLOCKED";
            if (font != null) titleText.font = font;
            titleText.fontSize = 12;
            titleText.color = UIPalette.Accent;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.UpperLeft;
            var titleRt = titleText.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.05f, 0.7f);
            titleRt.anchorMax = new Vector2(0.95f, 0.95f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            // 成就名称
            var nameGo = new GameObject("AchievementName");
            nameGo.transform.SetParent(notificationPanel.transform);
            nameText = nameGo.AddComponent<Text>();
            nameText.text = "";
            if (font != null) nameText.font = font;
            nameText.fontSize = 18;
            nameText.color = UIPalette.TextPrimary;
            nameText.fontStyle = FontStyle.Bold;
            nameText.alignment = TextAnchor.UpperLeft;
            var nameRt = nameText.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0.05f, 0.35f);
            nameRt.anchorMax = new Vector2(0.95f, 0.65f);
            nameRt.offsetMin = Vector2.zero;
            nameRt.offsetMax = Vector2.zero;

            // 成就描述
            var descGo = new GameObject("AchievementDesc");
            descGo.transform.SetParent(notificationPanel.transform);
            descText = descGo.AddComponent<Text>();
            descText.text = "";
            if (font != null) descText.font = font;
            descText.fontSize = 12;
            descText.color = UIPalette.TextSecondary;
            descText.alignment = TextAnchor.UpperLeft;
            var descRt = descText.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0.05f, 0.1f);
            descRt.anchorMax = new Vector2(0.95f, 0.3f);
            descRt.offsetMin = Vector2.zero;
            descRt.offsetMax = Vector2.zero;

            notificationPanel.SetActive(false);
        }

        private void OnAchievementUnlocked(AchievementManager.AchievementId id)
        {
            if (notificationPanel == null) return;

            nameText.text = AchievementManager.GetAchievementName(id);
            descText.text = AchievementManager.GetAchievementDescription(id);
            StartCoroutine(ShowNotification());
        }

        private IEnumerator ShowNotification()
        {
            notificationPanel.SetActive(true);
            panelRect.anchoredPosition = hiddenPosition;

            // 滑入动画
            float elapsed = 0f;
            while (elapsed < slideInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / slideInDuration);
                // 缓动函数
                t = 1f - Mathf.Pow(1f - t, 3f);
                panelRect.anchoredPosition = Vector2.Lerp(hiddenPosition, visiblePosition, t);
                yield return null;
            }
            panelRect.anchoredPosition = visiblePosition;

            // 显示一段时间
            yield return new WaitForSecondsRealtime(displayDuration);

            // 滑出动画
            elapsed = 0f;
            while (elapsed < slideInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / slideInDuration);
                t = Mathf.Pow(t, 3f);
                panelRect.anchoredPosition = Vector2.Lerp(visiblePosition, hiddenPosition, t);
                yield return null;
            }
            panelRect.anchoredPosition = hiddenPosition;
            notificationPanel.SetActive(false);
        }
    }
}
