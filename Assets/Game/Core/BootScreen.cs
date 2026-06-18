using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SteelRain.Core
{
    /// <summary>
    /// 启动画面：显示游戏 LOGO + 进度条，资源就绪后跳转到主菜单。
    /// 挂在 Boot 场景的 UI 根对象上。
    /// </summary>
    public sealed class BootScreen : MonoBehaviour
    {
        [SerializeField] private float minDisplaySeconds = 1.2f;
        [SerializeField] private string nextScene = "MainMenu";

        private Image logoImage;
        private Text logoText;
        private Text taglineText;
        private Image barFill;
        private Text percentText;
        private Text tipText;

        private float startTime;
        private float fakeProgress;
        private bool loadingComplete;

        private static readonly string[] Tips = new[]
        {
            "Switch characters to combine skills.",
            "Weapon upgrades stack - hunt those capsules!",
            "Aim for headshots to maximize combo.",
            "Shield soldiers block frontal damage.",
            "Drones ignore terrain - keep moving!",
            "Use the slow field to dodge boss swarms."
        };

        private void Awake()
        {
            BuildUI();
            startTime = Time.unscaledTime;
            if (tipText != null) tipText.text = "TIP: " + Tips[Random.Range(0, Tips.Length)];
        }

        private void Update()
        {
            float t = (Time.unscaledTime - startTime) / minDisplaySeconds;
            fakeProgress = Mathf.Clamp01(Mathf.MoveTowards(fakeProgress, Mathf.Max(t, 0.25f), Time.unscaledDeltaTime * 0.7f));
            if (barFill != null) barFill.fillAmount = fakeProgress;
            if (percentText != null) percentText.text = Mathf.RoundToInt(fakeProgress * 100f) + "%";

            if (!loadingComplete && t >= 1f && fakeProgress >= 0.99f)
            {
                loadingComplete = true;
                SceneManager.LoadScene(nextScene);
            }
        }

        private void BuildUI()
        {
            // Canvas
            var canvasGo = new GameObject("BootCanvas");
            canvasGo.transform.SetParent(transform);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 背景
            var bg = new GameObject("Background");
            bg.transform.SetParent(canvasGo.transform);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.05f, 0.08f, 1f);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

            // 顶部高光
            var topGlow = new GameObject("TopGlow");
            topGlow.transform.SetParent(canvasGo.transform);
            var topGlowImg = topGlow.AddComponent<Image>();
            topGlowImg.color = new Color(0.12f, 0.18f, 0.32f, 0.5f);
            topGlowImg.raycastTarget = false;
            var topGlowRt = topGlow.GetComponent<RectTransform>();
            topGlowRt.anchorMin = new Vector2(0, 0.6f);
            topGlowRt.anchorMax = new Vector2(1, 1);
            topGlowRt.offsetMin = Vector2.zero; topGlowRt.offsetMax = Vector2.zero;

            // 水平地平线
            var horizon = new GameObject("Horizon");
            horizon.transform.SetParent(canvasGo.transform);
            var horizonImg = horizon.AddComponent<Image>();
            horizonImg.color = new Color(0.85f, 0.50f, 0.15f, 0.4f);
            horizonImg.raycastTarget = false;
            var horizonRt = horizon.GetComponent<RectTransform>();
            horizonRt.anchorMin = new Vector2(0, 0.48f);
            horizonRt.anchorMax = new Vector2(1, 0.50f);
            horizonRt.offsetMin = Vector2.zero; horizonRt.offsetMax = Vector2.zero;

            // LOGO
            var logoGo = new GameObject("Logo");
            logoGo.transform.SetParent(canvasGo.transform);
            var logoRt = logoGo.AddComponent<RectTransform>();
            logoRt.anchorMin = new Vector2(0.5f, 0.5f);
            logoRt.anchorMax = new Vector2(0.5f, 0.5f);
            logoRt.pivot = new Vector2(0.5f, 0.5f);
            logoRt.anchoredPosition = new Vector2(0, 80);
            logoRt.sizeDelta = new Vector2(900, 160);
            logoImage = logoGo.AddComponent<Image>();
            logoImage.color = new Color(1f, 1f, 1f, 0f); // 透明占位
            logoImage.raycastTarget = false;

            // LOGO 文字
            var logoTextGo = new GameObject("LogoText");
            logoTextGo.transform.SetParent(canvasGo.transform);
            logoText = logoTextGo.AddComponent<Text>();
            logoText.text = "STEEL RAIN: FRONTIER";
            logoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            logoText.fontSize = 76;
            logoText.fontStyle = FontStyle.Bold;
            logoText.alignment = TextAnchor.MiddleCenter;
            logoText.color = new Color(0.97f, 0.97f, 0.99f, 1f);
            logoText.horizontalOverflow = HorizontalWrapMode.Overflow;
            logoText.verticalOverflow = VerticalWrapMode.Overflow;
            var logoTextRt = logoTextGo.GetComponent<RectTransform>();
            logoTextRt.anchorMin = new Vector2(0.5f, 0.5f);
            logoTextRt.anchorMax = new Vector2(0.5f, 0.5f);
            logoTextRt.pivot = new Vector2(0.5f, 0.5f);
            logoTextRt.anchoredPosition = new Vector2(0, 80);
            logoTextRt.sizeDelta = new Vector2(1100, 120);

            // LOGO 装饰下划线
            var underline = new GameObject("LogoUnderline");
            underline.transform.SetParent(canvasGo.transform);
            var underlineImg = underline.AddComponent<Image>();
            underlineImg.color = new Color(0.95f, 0.55f, 0.15f, 1f);
            underlineImg.raycastTarget = false;
            var underlineRt = underline.GetComponent<RectTransform>();
            underlineRt.anchorMin = new Vector2(0.5f, 0.5f);
            underlineRt.anchorMax = new Vector2(0.5f, 0.5f);
            underlineRt.pivot = new Vector2(0.5f, 0.5f);
            underlineRt.anchoredPosition = new Vector2(0, 20);
            underlineRt.sizeDelta = new Vector2(280, 4);

            // 副标题
            var taglineGo = new GameObject("Tagline");
            taglineGo.transform.SetParent(canvasGo.transform);
            taglineText = taglineGo.AddComponent<Text>();
            taglineText.text = "A 2D Squad Shooter";
            taglineText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            taglineText.fontSize = 24;
            taglineText.fontStyle = FontStyle.Italic;
            taglineText.alignment = TextAnchor.MiddleCenter;
            taglineText.color = new Color(0.70f, 0.74f, 0.82f, 1f);
            taglineText.horizontalOverflow = HorizontalWrapMode.Overflow;
            var taglineRt = taglineGo.GetComponent<RectTransform>();
            taglineRt.anchorMin = new Vector2(0.5f, 0.5f);
            taglineRt.anchorMax = new Vector2(0.5f, 0.5f);
            taglineRt.pivot = new Vector2(0.5f, 0.5f);
            taglineRt.anchoredPosition = new Vector2(0, -10);
            taglineRt.sizeDelta = new Vector2(600, 40);

            // 进度条背景
            var barBg = new GameObject("BarBackground");
            barBg.transform.SetParent(canvasGo.transform);
            var barBgImg = barBg.AddComponent<Image>();
            barBgImg.color = new Color(0.10f, 0.12f, 0.18f, 0.9f);
            barBgImg.raycastTarget = false;
            var barBgRt = barBg.GetComponent<RectTransform>();
            barBgRt.anchorMin = new Vector2(0.5f, 0.5f);
            barBgRt.anchorMax = new Vector2(0.5f, 0.5f);
            barBgRt.pivot = new Vector2(0.5f, 0.5f);
            barBgRt.anchoredPosition = new Vector2(0, -90);
            barBgRt.sizeDelta = new Vector2(520, 14);

            // 进度条填充
            var barFillGo = new GameObject("BarFill");
            barFillGo.transform.SetParent(barBg.transform);
            barFill = barFillGo.AddComponent<Image>();
            barFill.color = new Color(0.95f, 0.55f, 0.15f, 1f);
            barFill.type = Image.Type.Filled;
            barFill.fillMethod = Image.FillMethod.Horizontal;
            barFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            barFill.fillAmount = 0f;
            barFill.raycastTarget = false;
            var barFillRt = barFillGo.GetComponent<RectTransform>();
            barFillRt.anchorMin = Vector2.zero; barFillRt.anchorMax = Vector2.one;
            barFillRt.offsetMin = new Vector2(2, 2); barFillRt.offsetMax = new Vector2(-2, -2);

            // 百分比文字
            var percentGo = new GameObject("Percent");
            percentGo.transform.SetParent(canvasGo.transform);
            percentText = percentGo.AddComponent<Text>();
            percentText.text = "0%";
            percentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            percentText.fontSize = 18;
            percentText.fontStyle = FontStyle.Bold;
            percentText.alignment = TextAnchor.MiddleCenter;
            percentText.color = new Color(0.97f, 0.97f, 0.99f, 1f);
            var percentRt = percentGo.GetComponent<RectTransform>();
            percentRt.anchorMin = new Vector2(0.5f, 0.5f);
            percentRt.anchorMax = new Vector2(0.5f, 0.5f);
            percentRt.pivot = new Vector2(0.5f, 0.5f);
            percentRt.anchoredPosition = new Vector2(0, -125);
            percentRt.sizeDelta = new Vector2(200, 30);

            // 提示
            var tipGo = new GameObject("TipText");
            tipGo.transform.SetParent(canvasGo.transform);
            tipText = tipGo.AddComponent<Text>();
            tipText.text = "TIP: ...";
            tipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tipText.fontSize = 16;
            tipText.fontStyle = FontStyle.Italic;
            tipText.alignment = TextAnchor.MiddleCenter;
            tipText.color = new Color(0.70f, 0.74f, 0.82f, 1f);
            tipText.horizontalOverflow = HorizontalWrapMode.Wrap;
            var tipRt = tipGo.GetComponent<RectTransform>();
            tipRt.anchorMin = new Vector2(0.5f, 0);
            tipRt.anchorMax = new Vector2(0.5f, 0);
            tipRt.pivot = new Vector2(0.5f, 0);
            tipRt.anchoredPosition = new Vector2(0, 80);
            tipRt.sizeDelta = new Vector2(900, 40);

            // 版本号
            var versionGo = new GameObject("Version");
            versionGo.transform.SetParent(canvasGo.transform);
            var versionText = versionGo.AddComponent<Text>();
            versionText.text = "v1.0.0  -  Unity 6 LTS";
            versionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            versionText.fontSize = 14;
            versionText.alignment = TextAnchor.MiddleCenter;
            versionText.color = new Color(0.50f, 0.54f, 0.62f, 1f);
            var versionRt = versionGo.GetComponent<RectTransform>();
            versionRt.anchorMin = new Vector2(0.5f, 0);
            versionRt.anchorMax = new Vector2(0.5f, 0);
            versionRt.pivot = new Vector2(0.5f, 0);
            versionRt.anchoredPosition = new Vector2(0, 36);
            versionRt.sizeDelta = new Vector2(400, 30);
        }
    }
}
