using SteelRain.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// 按钮悬停与按下视觉反馈。
    /// 挂在任何 Button 所在 GameObject 上即可生效。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image target;
        [SerializeField] private Color normalColor = new Color(0.18f, 0.22f, 0.32f, 0.95f);
        [SerializeField] private Color hoverColor = new Color(0.28f, 0.38f, 0.55f, 1f);
        [SerializeField] private Color pressedColor = new Color(0.12f, 0.15f, 0.22f, 1f);
        [SerializeField] private Vector3 normalScale = Vector3.one;
        [SerializeField] private Vector3 hoverScale = new Vector3(1.06f, 1.06f, 1f);
        [SerializeField] private Vector3 pressedScale = new Vector3(0.96f, 0.96f, 1f);
        [SerializeField] private string clickSfx = "sfx_ui_click";

        private RectTransform rect;
        private Text label;
        private bool pointerDownThisFrame;

        private void Awake()
        {
            if (target == null) target = GetComponent<Image>();
            rect = GetComponent<RectTransform>();
            label = GetComponentInChildren<Text>();
            ApplyVisual(normalColor, normalScale);
            var btn = GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(PlayClickSfx);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ApplyVisual(hoverColor, hoverScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ApplyVisual(normalColor, normalScale);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDownThisFrame = true;
            ApplyVisual(pressedColor, pressedScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ApplyVisual(hoverColor, hoverScale);
        }

        private void PlayClickSfx()
        {
            if (pointerDownThisFrame && !string.IsNullOrEmpty(clickSfx))
            {
                AudioManager.Play(clickSfx, 0.7f);
            }
            pointerDownThisFrame = false;
        }

        private void ApplyVisual(Color c, Vector3 s)
        {
            if (target != null) target.color = c;
            if (rect != null) rect.localScale = s;
        }
    }
}

