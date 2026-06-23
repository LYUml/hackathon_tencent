using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HuaPi.UI.Data;
using UnityEngine.EventSystems;

namespace HuaPi.UI.Panels.Exploration
{
    /// <summary>
    /// 场景人物入口：可点击的角色立绘。
    /// 人物置于画面一侧，Hover 时轮廓微亮并显示角色名。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SceneCharacterEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Config")]
        [SerializeField] private SceneCharacterEntryData data;

        [Header("Visual Components")]
        [SerializeField] private Image portraitImage;      // 角色立绘
        [SerializeField] private Image outlineGlow;        // 轮廓发光（Hover 时）
        [SerializeField] private TextMeshProUGUI nameLabel; // 角色名（Hover 时显示）
        [SerializeField] private GameObject shadowOverlay;  // 阴影加深效果

        [Header("Hover Settings")]
        [SerializeField] private Color portraitIdleColor = Color.white;
        [SerializeField] private Color portraitHoverColor = new Color(1.1f, 1.05f, 1f, 1f);
        [SerializeField] private Color outlineHoverColor = new Color(0.85f, 0.75f, 0.55f, 0.5f);
        [SerializeField] private float hoverFadeSpeed = 8f;

        [Header("State")]
        public bool isInteractable = true;

        public string CharacterId => data?.characterId;
        public string DialogueId => data?.dialogueId;

        private bool _isHovering = false;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            ApplyData(data);
            ResetVisualState();
        }

        private void Update()
        {
            // 平滑过渡颜色
            if (portraitImage != null)
            {
                Color targetColor = _isHovering ? portraitHoverColor : portraitIdleColor;
                portraitImage.color = Color.Lerp(portraitImage.color, targetColor, Time.deltaTime * hoverFadeSpeed);
            }

            if (outlineGlow != null)
            {
                Color targetColor = _isHovering ? outlineHoverColor : Color.clear;
                outlineGlow.color = Color.Lerp(outlineGlow.color, targetColor, Time.deltaTime * hoverFadeSpeed);
            }

            if (nameLabel != null)
            {
                float targetAlpha = _isHovering ? 1f : 0f;
                Color c = nameLabel.color;
                c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * hoverFadeSpeed);
                nameLabel.color = c;
            }

            if (shadowOverlay != null)
            {
                float targetAlpha = _isHovering ? 0.3f : 0.15f;
                CanvasGroup cg = shadowOverlay.GetComponent<CanvasGroup>();
                if (cg == null) cg = shadowOverlay.AddComponent<CanvasGroup>();
                cg.alpha = Mathf.Lerp(cg.alpha, targetAlpha, Time.deltaTime * hoverFadeSpeed);
            }
        }

        /// <summary>
        /// 应用配置数据，使用左下角锚点（绝对像素坐标）
        /// </summary>
        public void ApplyData(SceneCharacterEntryData characterData)
        {
            data = characterData;
            if (data == null) return;

            // 设置 RectTransform：使用左下角锚点，anchoredPosition 就是绝对像素坐标
            _rectTransform.anchorMin = new Vector2(0, 0);
            _rectTransform.anchorMax = new Vector2(0, 0);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = data.anchoredPosition;
            _rectTransform.sizeDelta = data.size;

            // 设置立绘
            if (portraitImage != null && data.portraitSprite != null)
            {
                portraitImage.sprite = data.portraitSprite;
                portraitImage.preserveAspect = true;
            }

            // 设置名称标签
            if (nameLabel != null)
            {
                nameLabel.text = string.IsNullOrEmpty(data.hoverText) ? data.displayName : data.hoverText;
                nameLabel.color = new Color(nameLabel.color.r, nameLabel.color.g, nameLabel.color.b, 0f);
            }
        }

        /// <summary>
        /// 更新角色立绘（用于状态切换）
        /// </summary>
        public void UpdatePortrait(Sprite newSprite)
        {
            if (portraitImage != null && newSprite != null)
            {
                portraitImage.sprite = newSprite;
            }
        }

        private void ResetVisualState()
        {
            if (portraitImage != null)
            {
                portraitImage.color = portraitIdleColor;
            }
            if (outlineGlow != null)
            {
                outlineGlow.color = Color.clear;
            }
            if (nameLabel != null)
            {
                Color c = nameLabel.color;
                c.a = 0f;
                nameLabel.color = c;
            }
            if (shadowOverlay != null)
            {
                CanvasGroup cg = shadowOverlay.GetComponent<CanvasGroup>();
                if (cg == null) cg = shadowOverlay.AddComponent<CanvasGroup>();
                cg.alpha = 0.15f;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExit();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnPointerClick();
        }

        public void OnPointerEnter()
        {
            if (!isInteractable) return;
            _isHovering = true;
        }

        public void OnPointerExit()
        {
            _isHovering = false;
        }

        public void OnPointerClick()
        {
            if (!isInteractable) return;
            // 通知 ExplorationPanel 处理点击
            ExplorationPanel.Instance?.OnCharacterClicked(this);
        }
    }
}
