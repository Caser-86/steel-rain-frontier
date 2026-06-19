using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// 角色切换时屏幕中央弹出角色名提示，2 秒内淡出。
    /// 挂在游戏 HUD 的 Canvas 子对象上即可。
    /// </summary>
    public sealed class CharacterSwitchToast : MonoBehaviour
    {
        [SerializeField] private float showSeconds = 1.6f;
        [SerializeField] private float fadeSeconds = 0.5f;
        [SerializeField] private float slideDistance = 30f;

        private Text nameText;
        private Text skillText;
        private CanvasGroup group;
        private RectTransform nameRect;
        private RectTransform skillRect;
        private float showTimer;
        private float startNameY;
        private float startSkillY;

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
            {
                group.alpha = 0f;
            }
        }

        private void Update()
        {
            if (group == null) return;
            if (showTimer <= 0f) return;

            showTimer -= Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(1f - showTimer / showSeconds);

            // 第一秒保持不透明，最后 fadeSeconds 淡出
            float alpha;
            if (showTimer > fadeSeconds)
                alpha = 1f;
            else
                alpha = Mathf.Clamp01(showTimer / fadeSeconds);

            group.alpha = alpha;

            // 向上滑动
            if (nameRect != null)
                nameRect.anchoredPosition = new Vector2(0, startNameY + slideDistance * t);
            if (skillRect != null)
                skillRect.anchoredPosition = new Vector2(0, startSkillY + slideDistance * t);

            if (showTimer <= 0f)
            {
                group.alpha = 0f;
            }
        }

        private void OnCharacterChanged(string displayName)
        {
            if (nameText == null) return;
            nameText.text = displayName.ToUpper();
            var skill = FindObjectOfType<Player.CharacterSkill>();
            if (skillText != null)
            {
                if (skill != null && skill.Runtime != null && skill.Runtime.Definition != null)
                    skillText.text = $"Skill: {GetSkillName(skill.Runtime.Definition.skillId)}";
                else
                    skillText.text = "";
            }
            showTimer = showSeconds;
            group.alpha = 1f;
            if (nameRect != null) nameRect.anchoredPosition = new Vector2(0, startNameY);
            if (skillRect != null) skillRect.anchoredPosition = new Vector2(0, startSkillY);
        }

        private static string GetSkillName(Player.CharacterSkillId skillId)
        {
            return skillId switch
            {
                Player.CharacterSkillId.BreakthroughFire => "Breakthrough Fire",
                Player.CharacterSkillId.TrenchShield => "Trench Shield",
                Player.CharacterSkillId.BombardmentMatrix => "Bombardment Matrix",
                Player.CharacterSkillId.TimeRift => "Time Rift",
                _ => "Unknown"
            };
        }

        private void BuildUI()
        {
            // CanvasGroup 容器
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

            // 角色名
            var nameGo = new GameObject("NameText");
            nameGo.transform.SetParent(rootGo.transform, false);
            nameText = nameGo.AddComponent<Text>();
            nameRect = nameGo.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 0.5f);
            nameRect.anchorMax = new Vector2(0.5f, 0.5f);
            nameRect.pivot = new Vector2(0.5f, 0.5f);
            startNameY = 30f;
            nameRect.anchoredPosition = new Vector2(0, startNameY);
            nameRect.sizeDelta = new Vector2(800, 80);
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 64;
            nameText.fontStyle = FontStyle.Bold;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.color = UIPalette.TextPrimary;
            nameText.horizontalOverflow = HorizontalWrapMode.Overflow;
            nameText.verticalOverflow = VerticalWrapMode.Overflow;

            // 名字下的橙色装饰线
            var accentGo = new GameObject("Accent");
            accentGo.transform.SetParent(rootGo.transform, false);
            var accentImg = accentGo.AddComponent<Image>();
            var accentRt = accentGo.GetComponent<RectTransform>();
            accentRt.anchorMin = new Vector2(0.5f, 0.5f);
            accentRt.anchorMax = new Vector2(0.5f, 0.5f);
            accentRt.pivot = new Vector2(0.5f, 0.5f);
            accentRt.anchoredPosition = new Vector2(0, -10f);
            accentRt.sizeDelta = new Vector2(160, 3);
            accentImg.color = UIPalette.Primary;
            accentImg.raycastTarget = false;

            // 技能名
            var skillGo = new GameObject("SkillText");
            skillGo.transform.SetParent(rootGo.transform, false);
            skillText = skillGo.AddComponent<Text>();
            skillRect = skillGo.GetComponent<RectTransform>();
            skillRect.anchorMin = new Vector2(0.5f, 0.5f);
            skillRect.anchorMax = new Vector2(0.5f, 0.5f);
            skillRect.pivot = new Vector2(0.5f, 0.5f);
            startSkillY = -40f;
            skillRect.anchoredPosition = new Vector2(0, startSkillY);
            skillRect.sizeDelta = new Vector2(600, 40);
            skillText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            skillText.fontSize = 22;
            skillText.fontStyle = FontStyle.Italic;
            skillText.alignment = TextAnchor.MiddleCenter;
            skillText.color = UIPalette.Accent;
            skillText.horizontalOverflow = HorizontalWrapMode.Overflow;
        }
    }
}
