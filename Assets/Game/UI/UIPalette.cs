using UnityEngine;

namespace SteelRain.UI
{
    /// <summary>
    /// 全局 UI 调色板，集中颜色管理避免散落。
    /// </summary>
    public static class UIPalette
    {
        // 主色
        public static readonly Color Primary = new Color(0.95f, 0.55f, 0.15f);    // 橙：行动
        public static readonly Color Accent = new Color(0.20f, 0.65f, 0.95f);     // 蓝：信息
        public static readonly Color Danger = new Color(0.92f, 0.25f, 0.30f);     // 红：危险
        public static readonly Color Success = new Color(0.30f, 0.85f, 0.50f);    // 绿：成功
        public static readonly Color Warning = new Color(0.95f, 0.75f, 0.20f);    // 黄：警告

        // 表面色
        public static readonly Color Background = new Color(0.06f, 0.07f, 0.10f, 1f);
        public static readonly Color Panel = new Color(0.10f, 0.12f, 0.18f, 0.92f);
        public static readonly Color PanelLight = new Color(0.16f, 0.19f, 0.27f, 0.95f);
        public static readonly Color Divider = new Color(0.30f, 0.34f, 0.42f, 0.6f);

        // 文字
        public static readonly Color TextPrimary = new Color(0.97f, 0.97f, 0.99f, 1f);
        public static readonly Color TextSecondary = new Color(0.70f, 0.74f, 0.82f, 1f);
        public static readonly Color TextMuted = new Color(0.50f, 0.54f, 0.62f, 1f);

        // 按钮
        public static readonly Color ButtonNormal = new Color(0.16f, 0.20f, 0.30f, 0.95f);
        public static readonly Color ButtonHover = new Color(0.28f, 0.36f, 0.52f, 1f);
        public static readonly Color ButtonPressed = new Color(0.10f, 0.13f, 0.20f, 1f);
        public static readonly Color ButtonPrimary = new Color(0.85f, 0.45f, 0.10f, 1f);
        public static readonly Color ButtonPrimaryHover = new Color(0.95f, 0.55f, 0.15f, 1f);

        // 状态
        public static readonly Color HealthHigh = new Color(0.30f, 0.85f, 0.50f);
        public static readonly Color HealthMid = new Color(0.95f, 0.75f, 0.20f);
        public static readonly Color HealthLow = new Color(0.92f, 0.25f, 0.30f);
        public static readonly Color Shield = new Color(0.30f, 0.65f, 0.95f);
    }
}
