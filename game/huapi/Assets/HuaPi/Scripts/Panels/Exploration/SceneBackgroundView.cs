using UnityEngine;
using UnityEngine.UI;

namespace HuaPi.UI.Panels.Exploration
{
    /// <summary>
    /// 全屏场景背景显示：适配 16:9，铺满屏幕但不拉伸变形。
    /// 支持压暗渐变和黑雾叠加。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SceneBackgroundView : MonoBehaviour
    {
        [Header("Background")]
        [SerializeField] private Image backgroundImage;      // 主背景图
        [SerializeField] private Image darkOverlay;         // 压暗遮罩
        [SerializeField] private Image vignetteOverlay;     // 暗角/黑雾效果
        [SerializeField] private Color defaultBackgroundColor = new Color(0.06f, 0.04f, 0.03f, 1f); // 默认暗褐色

        [Header("Dark Settings")]
        [SerializeField] [Range(0f, 1f)] private float darkAmount = 0.35f;
        [SerializeField] private Color darkColor = new Color(0, 0, 0, 0.35f);
        [SerializeField] private Color vignetteColor = new Color(0, 0, 0, 0.5f);

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.8f;
        private float _currentFade = 0f;
        private bool _isFadingIn = false;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            SetupFullScreen();
            UpdateBackgroundFill();

            // 如果没有设置 Sprite，使用默认暗色背景
            if (backgroundImage != null && backgroundImage.sprite == null)
            {
                backgroundImage.color = defaultBackgroundColor;
            }
        }

        private void Update()
        {
            if (_isFadingIn && _currentFade < 1f)
            {
                _currentFade += Time.deltaTime / fadeInDuration;
                _currentFade = Mathf.Clamp01(_currentFade);
                ApplyFade(_currentFade);
            }
        }

        /// <summary>
        /// 设置全屏填充（Stretch-Stretch）
        /// </summary>
        private void SetupFullScreen()
        {
            _rectTransform.anchorMin = Vector2.zero;
            _rectTransform.anchorMax = Vector2.one;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
            _rectTransform.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// 设置背景 Sprite
        /// </summary>
        public void SetBackground(Sprite sprite)
        {
            if (backgroundImage != null)
            {
                if (sprite != null)
                {
                    backgroundImage.sprite = sprite;
                    backgroundImage.color = Color.white;
                }
                else
                {
                    // 没有 Sprite 时使用默认暗色背景
                    backgroundImage.sprite = null;
                    backgroundImage.color = defaultBackgroundColor;
                }
                backgroundImage.preserveAspect = false;
                UpdateBackgroundFill();
            }
            TriggerFadeIn();
        }

        /// <summary>
        /// 设置压暗程度
        /// </summary>
        public void SetDarkness(float amount)
        {
            darkAmount = Mathf.Clamp01(amount);
            if (darkOverlay != null)
            {
                darkColor.a = darkAmount;
                darkOverlay.color = darkColor;
            }
        }

        /// <summary>
        /// 设置暗角强度
        /// </summary>
        public void SetVignette(float amount)
        {
            if (vignetteOverlay != null)
            {
                Color c = vignetteColor;
                c.a = Mathf.Clamp01(amount);
                vignetteOverlay.color = c;
            }
        }

        /// <summary>
        /// 触发淡入动画
        /// </summary>
        public void TriggerFadeIn()
        {
            _currentFade = 0f;
            _isFadingIn = true;
            ApplyFade(0f);
        }

        /// <summary>
        /// 立即显示（无淡入）
        /// </summary>
        public void ShowImmediately()
        {
            _currentFade = 1f;
            _isFadingIn = false;
            ApplyFade(1f);
        }

        private void ApplyFade(float alpha)
        {
            if (backgroundImage != null)
            {
                Color c = backgroundImage.color;
                c.a = alpha;
                backgroundImage.color = c;
            }
            if (darkOverlay != null)
            {
                Color c = darkColor;
                c.a *= alpha;
                darkOverlay.color = c;
            }
            if (vignetteOverlay != null)
            {
                Color c = vignetteColor;
                c.a *= alpha;
                vignetteOverlay.color = c;
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateBackgroundFill();
        }

        /// <summary>
        /// 按 cover 方式铺满背景：保持原图比例，必要时裁切边缘，避免 16:9 下拉伸变形。
        /// </summary>
        private void UpdateBackgroundFill()
        {
            if (backgroundImage == null || backgroundImage.sprite == null || _rectTransform == null)
            {
                return;
            }

            RectTransform imageRect = backgroundImage.rectTransform;
            Rect parentRect = _rectTransform.rect;
            float parentWidth = Mathf.Max(parentRect.width, 1f);
            float parentHeight = Mathf.Max(parentRect.height, 1f);
            Rect spriteRect = backgroundImage.sprite.rect;
            float spriteAspect = spriteRect.width / Mathf.Max(spriteRect.height, 1f);
            float parentAspect = parentWidth / parentHeight;

            imageRect.anchorMin = new Vector2(0.5f, 0.5f);
            imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.anchoredPosition = Vector2.zero;

            if (spriteAspect > parentAspect)
            {
                imageRect.sizeDelta = new Vector2(parentHeight * spriteAspect, parentHeight);
            }
            else
            {
                imageRect.sizeDelta = new Vector2(parentWidth, parentWidth / spriteAspect);
            }
        }
    }
}
