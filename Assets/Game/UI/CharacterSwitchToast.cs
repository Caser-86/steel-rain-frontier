using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// 角色切换时屏幕中央弹出角色名提示，2 秒内淡出（合金弹头简化版）。
    /// 仅显示角色名，无技能信息。
    /// </summary>
    public sealed class CharacterSwitchToast : MonoBehaviour
    {
        [SerializeField] private float showSeconds = 1.6f;
        [SerializeField] private float fadeSeconds = 0.5f;
        [SerializeField] private float slideDistance = 30f;

        private Text nameText;
        private CanvasGroup group;
        private RectTransform nameRect;
        private float showTimer;
        private float startNameY;

        private void Awake()
        {
            try
            {
                BuildUI();
            }
            catch (System.Exception e)
            {
                Debug.LogError("[CharacterSwitchToast] BuildUI failed: " + e);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            GameEvents.PlayerCharacterChanged += OnCharacterChanged;
        }

        private void OnDisable()
        {
            GameEvents.PlayerCharacterChanged -= OnCharacterChanged;
        }

        private void Start()
        {
            if (group != null)
                group.alpha = 0f;
        }

        private void Update()
        {
            if (group == null) return;
            if (showTimer <= 0f) return;

            showTimer -= Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(1f - showTimer / showSeconds);

            float alpha;
            if (showTimer > fadeSeconds)
                alpha = 1f;
            else
                alpha = Mathf.Clamp01(showTimer / fadeSeconds);

            group.alpha = alpha;

            if (nameRect != null)
                nameRect.anchoredPosition = new Vector2(0, startNameY + slideDistance * t);

            if (showTimer <= 0f)
                group.alpha = 0f;
        }

        private void OnCharacterChanged(string displayName)
        {
            if (nameText == null) return;
            nameText.text = displayName.ToUpper();
            showTimer = showSeconds;
            group.alpha = 1f;
            if (nameRect != null) nameRect.anchoredPosition = new Vector2(0, startNameY);
        }

        private void BuildUI()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var rootGo = new GameObject("CharacterSwitchToast");
            rootGo.transform.SetParent(transform, false);
            var rootRt = rootGo.AddComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0.5f, 0.5f);
            rootRt.anchorMax = new Vector2(0.5f, 0.5f);
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.anchoredPosition = new Vector2(0, 60);
            rootRt.sizeDelta = new Vector2(800, 200);
            group = rootGo.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            var nameGo = new GameObject("NameText");
            nameGo.transform.SetParent(rootGo.transform, false);
            nameText = nameGo.AddComponent<Text>();
            nameRect = nameGo.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 0.5f);
            nameRect.anchorMax = new Vector2(0.5f, 0.5f);
            nameRect.pivot = new Vector2(0.5f, 0.5f);
            startNameY = 10f;
            nameRect.anchoredPosition = new Vector2(0, startNameY);
            nameRect.sizeDelta = new Vector2(800, 80);
            if (font != null) nameText.font = font;
            nameText.fontSize = 64;
            nameText.fontStyle = FontStyle.Bold;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.color = UIPalette.TextPrimary;
            nameText.horizontalOverflow = HorizontalWrapMode.Overflow;
            nameText.verticalOverflow = VerticalWrapMode.Overflow;

            var accentGo = new GameObject("Accent");
            accentGo.transform.SetParent(rootGo.transform, false);
            var accentImg = accentGo.AddComponent<Image>();
            var accentRt = accentGo.GetComponent<RectTransform>();
            accentRt.anchorMin = new Vector2(0.5f, 0.5f);
            accentRt.anchorMax = new Vector2(0.5f, 0.5f);
            accentRt.pivot = new Vector2(0.5f, 0.5f);
            accentRt.anchoredPosition = new Vector2(0, -30f);
            accentRt.sizeDelta = new Vector2(160, 3);
            accentImg.color = UIPalette.Primary;
            accentImg.raycastTarget = false;
        }
    }
}
