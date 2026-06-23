using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HuaPi.UI.Data;
using UnityEngine.EventSystems;

namespace HuaPi.UI.Panels.Exploration
{
    /// <summary>
    /// 场景调查热点：场景中可点击的物件入口。
    /// 默认状态低调（仅半透明暗区），Hover 时浮现暗金描边与名称。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SceneHotspotItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Config")]
        [SerializeField] private SceneHotspotData data;

        [Header("Visual Components")]
        [SerializeField] private Image hotspotArea;      // 点击区域指示（默认透明或极淡）
        [SerializeField] private Image glowBorder;       // Hover 时的发光边框
        [SerializeField] private TextMeshProUGUI nameLabel; // 物件名称（Hover 时显示）

        [Header("Hover Settings")]
        [SerializeField] private Color idleColor = new Color(0, 0, 0, 0.05f);
        [SerializeField] private Color hoverColor = new Color(0.85f, 0.75f, 0.55f, 0.25f);
        [SerializeField] private Color hoverBorderColor = new Color(0.85f, 0.75f, 0.55f, 0.6f);
        [SerializeField] private float hoverFadeSpeed = 8f;

        [Header("State")]
        public bool isDiscovered = false;
        public bool isInteractable = true;

        // 运行时数据
        public string HotspotId => data?.hotspotId;
        public string LinkedClueId => data?.linkedClueId;

        private bool _isHovering = false;
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 确保始终可见，不可交互时只是不响应点击
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        private void Start()
        {
            ApplyData(data);
            ResetVisualState();
        }

        private void Update()
        {
            // 平滑过渡颜色
            if (hotspotArea != null)
            {
                Color targetColor = _isHovering ? hoverColor : idleColor;
                hotspotArea.color = Color.Lerp(hotspotArea.color, targetColor, Time.deltaTime * hoverFadeSpeed);
            }

            if (glowBorder != null)
            {
                Color targetBorderColor = _isHovering ? hoverBorderColor : Color.clear;
                glowBorder.color = Color.Lerp(glowBorder.color, targetBorderColor, Time.deltaTime * hoverFadeSpeed);
            }

            if (nameLabel != null)
            {
                float targetAlpha = _isHovering ? 1f : 0f;
                Color c = nameLabel.color;
                c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * hoverFadeSpeed);
                nameLabel.color = c;
            }
        }

        /// <summary>
        /// 应用配置数据，使用左下角锚点（绝对像素坐标）
        /// </summary>
        public void ApplyData(SceneHotspotData hotspotData)
        {
            data = hotspotData;
            if (data == null) return;

            // 设置 RectTransform：使用左下角锚点，anchoredPosition 就是绝对像素坐标
            _rectTransform.anchorMin = new Vector2(0, 0);
            _rectTransform.anchorMax = new Vector2(0, 0);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = data.anchoredPosition;
            _rectTransform.sizeDelta = data.size;

            // 设置名称标签
            if (nameLabel != null)
            {
                nameLabel.text = string.IsNullOrEmpty(data.hoverText) ? data.displayName : data.hoverText;
                nameLabel.color = new Color(nameLabel.color.r, nameLabel.color.g, nameLabel.color.b, 0f);
            }
        }

        /// <summary>
        /// 标记为已调查（视觉上可变化，但热点不消失）
        /// </summary>
        public void MarkAsDiscovered()
        {
            isDiscovered = true;
            // 已调查后，idle 状态更淡，hover 状态更弱
            idleColor = new Color(0, 0, 0, 0.02f);
            hoverColor = new Color(0.85f, 0.75f, 0.55f, 0.15f);
        }

        private void ResetVisualState()
        {
            if (hotspotArea != null)
            {
                hotspotArea.color = idleColor;
            }
            if (glowBorder != null)
            {
                glowBorder.color = Color.clear;
            }
            if (nameLabel != null)
            {
                Color c = nameLabel.color;
                c.a = 0f;
                nameLabel.color = c;
            }
        }

        // 事件处理：支持 EventSystem 直接调用，也兼容 Prefab 上已有的 EventTrigger。
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
            ExplorationPanel.Instance?.OnHotspotClicked(this);
        }
    }
}
