using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HuaPi.UI.Core;

namespace HuaPi.UI.Panels
{
    /// <summary>
    /// 主菜单面板：纯净沉浸版 V6
    /// 修复：
    /// 1. 菜单项重合 — 增大间距 + 彻底移除 LayoutGroup 影响
    /// 2. 字体方框 — 运行时检测并回退到支持中文的字体
    /// </summary>
    public class MainMenuPanel : PanelBase
    {
        [Header("Title")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;

        [Header("Menu Items")]
        [SerializeField] private TMP_Text startGameText;
        [SerializeField] private TMP_Text continueGameText;
        [SerializeField] private TMP_Text settingsText;
        [SerializeField] private TMP_Text quitText;

        [Header("Bottom Info")]
        [SerializeField] private TMP_Text bottomInfoText;

        [Header("Background")]
        [SerializeField] private Image backgroundImage;

        [Header("Font Fallback")]
        [SerializeField] private TMP_FontAsset fallbackFontAsset; // 可在 Inspector 中指定中文备用字体

        [Header("Hover Colors")]
        [SerializeField] private Color titleColor = new Color(0.545f, 0.125f, 0.125f, 0.9f);
        [SerializeField] private Color subtitleColor = new Color(0.353f, 0.227f, 0.165f, 0.6f);
        [SerializeField] private Color defaultColor = new Color(0.788f, 0.541f, 0.431f, 0.85f);
        [SerializeField] private Color hoverColor = new Color(0.545f, 0.125f, 0.125f, 0.95f);
        [SerializeField] private Color disabledColor = new Color(0.541f, 0.478f, 0.431f, 0.55f);
        [SerializeField] private Color bottomInfoColor = new Color(0.239f, 0.082f, 0.094f, 0.4f);
        [SerializeField] private Color bgColor = new Color(0.04f, 0.03f, 0.03f, 1f);

        private readonly List<MenuItemController> _menuControllers = new List<MenuItemController>();
        private TMP_FontAsset _cachedFallbackFont;

        // V6 布局参数（基于 1920x1080，修复重合问题）
        // 核心修复：间距必须大于字体渲染高度，否则 TMP_Text 的 mesh 会重叠
        private const float TITLE_Y = 260f;
        private const float SUBTITLE_Y = 90f;
        private const float MENU_START_Y = -80f;      // 菜单起始位置（降低，避免和副标题重叠）
        private const float MENU_SPACING = 85f;       // 间距增大到 85px（52px 字体渲染高度约 60-70px）
        private const float BOTTOM_Y = 40f;

        private const float TITLE_FONT_SIZE = 225f;
        private const float SUBTITLE_FONT_SIZE = 18f;
        private const float MENU_PRIMARY_FONT_SIZE = 52f;
        private const float MENU_SECONDARY_FONT_SIZE = 44f;
        private const float BOTTOM_FONT_SIZE = 14f;

        private const float HOVER_SCALE = 1.05f;
        private const float NORMAL_SCALE = 1.0f;

        protected override void Awake()
        {
            base.Awake();

            // 预加载备用字体（只查一次，避免每帧查询）
            CacheFallbackFont();

            // 彻底禁用 LayoutGroup（不是 destroy，而是移除组件引用关系，避免 Rebuild 时重新激活）
            DestroyLayoutGroupsAndButtons();

            FindOrCreateAllElements();
            SetupBackground();
            SetupTitle();
            SetupSubtitle();
            SetupMenuItems();
            SetupBottomInfo();

            // 强制应用位置（覆盖任何 LayoutGroup 的残留影响）
            ApplyMenuPositions();
        }

        public override void Init(object data)
        {
            base.Init(data);
            ApplyMenuPositions();
        }

        /// <summary>
        /// 彻底移除 LayoutGroup 和 Button 组件，避免任何布局重建干扰。
        /// 如果 prefab 中仍然保留了这些组件，运行时直接销毁它们。
        /// </summary>
        private void DestroyLayoutGroupsAndButtons()
        {
            // 遍历所有子对象，销毁 VerticalLayoutGroup 和 HorizontalLayoutGroup
            foreach (var lg in GetComponentsInChildren<UnityEngine.UI.LayoutGroup>(true))
            {
                if (lg != null)
                {
                    lg.enabled = false;
                    Destroy(lg);
                }
            }

            // 遍历所有子对象，销毁 Button 和 LayoutElement
            foreach (var btn in GetComponentsInChildren<Button>(true))
            {
                if (btn != null)
                {
                    btn.enabled = false;
                    Destroy(btn);
                }
            }

            foreach (var le in GetComponentsInChildren<LayoutElement>(true))
            {
                if (le != null)
                {
                    le.enabled = false;
                    Destroy(le);
                }
            }

            // 强制立即刷新 RectTransform，避免残留布局影响
            Canvas.ForceUpdateCanvases();
        }

        private void CacheFallbackFont()
        {
            if (fallbackFontAsset != null)
            {
                _cachedFallbackFont = fallbackFontAsset;
                return;
            }

            // 自动搜索：尝试查找项目中已包含中文字符的 TMP_FontAsset
            // 注意：只在 Editor 和 Standalone 平台有效，WebGL 构建前请确保指定 fallbackFontAsset
            var allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var font in allFonts)
            {
                if (font == null) continue;
                string name = font.name.ToLowerInvariant();
                // 常见中文字体命名特征
                if (name.Contains("chinese") || name.Contains("cjk") || name.Contains("noto") ||
                    name.Contains("source") || name.Contains("simhei") || name.Contains("simsun") ||
                    name.Contains("heiti") || name.Contains("songti") || name.Contains("yahei") ||
                    name.Contains("hakidame") || name.Contains("kozuka") || name.Contains("fontcn") ||
                    name.Contains("cn") || name.Contains("zh"))
                {
                    _cachedFallbackFont = font;
                    Debug.Log($"[MainMenuPanel] 自动找到备用字体: {font.name}");
                    break;
                }
            }

            if (_cachedFallbackFont == null && allFonts.Length > 0)
            {
                // 如果没找到明确的中文标识字体，尝试第一个包含较多字符的字体
                foreach (var font in allFonts)
                {
                    if (font != null && font.characterTable != null && font.characterTable.Count > 1000)
                    {
                        _cachedFallbackFont = font;
                        Debug.Log($"[MainMenuPanel] 使用字符数较多的备用字体: {font.name} (chars={font.characterTable.Count})");
                        break;
                    }
                }
            }

            if (_cachedFallbackFont == null)
            {
                Debug.LogWarning("[MainMenuPanel] 未找到备用中文字体。请在 Inspector 中指定 fallbackFontAsset，否则中文将显示为方框。");
            }
        }

        private void FindOrCreateAllElements()
        {
            if (backgroundImage == null)
                backgroundImage = transform.Find("Background")?.GetComponent<Image>();

            if (titleText == null)
                titleText = FindTextInChildren("Title");

            if (subtitleText == null)
                subtitleText = FindTextInChildren("Subtitle");

            Transform menuContainer = transform.Find("Menu");
            if (menuContainer == null)
            {
                menuContainer = new GameObject("Menu").transform;
                menuContainer.SetParent(transform, false);
            }

            startGameText = FindOrCreateMenuItem(menuContainer, "StartGame", "开始游戏");
            continueGameText = FindOrCreateMenuItem(menuContainer, "ContinueGame", "继续游戏");
            settingsText = FindOrCreateMenuItem(menuContainer, "Settings", "设置");
            quitText = FindOrCreateMenuItem(menuContainer, "Quit", "退出");

            if (bottomInfoText == null)
                bottomInfoText = FindTextInChildren("BottomInfo");
        }

        private TMP_Text FindTextInChildren(string name)
        {
            Transform t = transform.Find(name);
            if (t != null) return t.GetComponent<TMP_Text>();
            foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.name == name) return text;
            }
            return null;
        }

        private TMP_Text FindOrCreateMenuItem(Transform parent, string name, string defaultText)
        {
            Transform t = parent.Find(name);
            if (t != null)
            {
                TMP_Text text = t.GetComponent<TMP_Text>();
                if (text != null) return text;
            }
            foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.name == name) return text;
            }
            return null;
        }

        private void SetupBackground()
        {
            if (backgroundImage == null) return;
            backgroundImage.color = bgColor;
            backgroundImage.raycastTarget = false;
            RectTransform rect = backgroundImage.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
            }
        }

        private void SetupTitle()
        {
            if (titleText == null) return;
            titleText.text = "畫皮";
            titleText.color = titleColor;
            titleText.fontSize = TITLE_FONT_SIZE;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Normal;
            FixFont(titleText);

            RectTransform rect = titleText.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, TITLE_Y);
                rect.sizeDelta = new Vector2(1200, 300);
            }
        }

        private void SetupSubtitle()
        {
            if (subtitleText == null) return;
            subtitleText.text = "MASKS BEHIND MASKS";
            subtitleText.color = subtitleColor;
            subtitleText.fontSize = SUBTITLE_FONT_SIZE;
            subtitleText.alignment = TextAlignmentOptions.Center;
            FixFont(subtitleText);

            RectTransform rect = subtitleText.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, SUBTITLE_Y);
                rect.sizeDelta = new Vector2(1200, 60);
            }
        }

        private void SetupMenuItems()
        {
            SetupMenuItem(startGameText, "開始遊戲", MENU_PRIMARY_FONT_SIZE, 0.85f, OnStartGame);
            SetupMenuItem(continueGameText, "繼續遊戲", MENU_SECONDARY_FONT_SIZE, 0.70f, OnContinueGame);
            SetupMenuItem(settingsText, "設定", MENU_SECONDARY_FONT_SIZE, 0.70f, OnSettings);
            SetupMenuItem(quitText, "退出", MENU_SECONDARY_FONT_SIZE, 0.55f, OnQuit);
        }

        private void SetupMenuItem(TMP_Text text, string content, float fontSize, float opacity, System.Action onClick)
        {
            if (text == null) return;

            text.text = content;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Normal;
            text.raycastTarget = true;
            FixFont(text);

            Color targetColor = content == "退出" ? disabledColor : defaultColor;
            text.color = new Color(targetColor.r, targetColor.g, targetColor.b, opacity);

            RectTransform rect = text.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                // sizeDelta 高度必须大于字体渲染高度，避免 mesh 被截断或重叠判定错误
                rect.sizeDelta = new Vector2(1200, fontSize * 2.0f);
                rect.localScale = Vector3.one;
            }

            var controller = text.gameObject.GetComponent<MenuItemController>();
            if (controller == null)
                controller = text.gameObject.AddComponent<MenuItemController>();
            controller.Init(text, targetColor, hoverColor, opacity, onClick, HOVER_SCALE, NORMAL_SCALE);
            _menuControllers.Add(controller);
        }

        private void SetupBottomInfo()
        {
            if (bottomInfoText == null) return;
            bottomInfoText.text = "薛家戲班 · 民國二十年";
            bottomInfoText.color = bottomInfoColor;
            bottomInfoText.fontSize = BOTTOM_FONT_SIZE;
            bottomInfoText.alignment = TextAlignmentOptions.Center;
            bottomInfoText.raycastTarget = false;
            FixFont(bottomInfoText);

            RectTransform rect = bottomInfoText.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0, BOTTOM_Y);
                rect.sizeDelta = new Vector2(1200, 40);
            }
        }

        /// <summary>
        /// 修复字体缺失：如果当前字体不包含文本中的字符，回退到备用字体。
        /// </summary>
        private void FixFont(TMP_Text text)
        {
            if (text == null) return;
            if (string.IsNullOrEmpty(text.text)) return;

            TMP_FontAsset currentFont = text.font;
            uint[] missingChars = null;
            bool hasAll = false;
            if (currentFont != null)
            {
                hasAll = currentFont.HasCharacters(text.text, out missingChars, false);
            }

            if (hasAll)
            {
                return;
            }

            if (_cachedFallbackFont != null)
            {
                text.font = _cachedFallbackFont;
                text.fontSharedMaterial = _cachedFallbackFont.material;
                int missingCount = missingChars != null ? missingChars.Length : 0;
                Debug.Log($"[MainMenuPanel] 字体回退：{text.name} -> {_cachedFallbackFont.name}（原文缺失 {missingCount} 个字符）");
            }
            else
            {
                Debug.LogWarning($"[MainMenuPanel] 字体缺失：{text.name} 的文本 '{text.text}' 缺少字符，但未找到备用字体。请在 Inspector 的 fallbackFontAsset 中指定中文字体。");
            }
        }

        private void ApplyMenuPositions()
        {
            float currentY = MENU_START_Y;

            ApplyMenuItemPosition(startGameText, currentY);
            currentY -= MENU_SPACING;

            ApplyMenuItemPosition(continueGameText, currentY);
            currentY -= MENU_SPACING;

            ApplyMenuItemPosition(settingsText, currentY);
            currentY -= MENU_SPACING;

            ApplyMenuItemPosition(quitText, currentY);

            // 通知所有 controller 更新 originalPosition
            foreach (var controller in _menuControllers)
            {
                controller?.UpdateOriginalPosition();
            }
        }

        private void ApplyMenuItemPosition(TMP_Text text, float y)
        {
            if (text == null) return;
            RectTransform rect = text.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(0, y);
            }
        }

        private void OnStartGame()
        {
            Debug.Log("[MainMenu] Start Game");
            UIManager.Instance?.ClosePanel(UIPanelType.MainMenu);
        }

        private void OnContinueGame()
        {
            Debug.Log("[MainMenu] Continue Game");
        }

        private void OnSettings()
        {
            Debug.Log("[MainMenu] Settings");
            UIManager.Instance?.OpenPanel(UIPanelType.Pause);
        }

        private void OnQuit()
        {
            Debug.Log("[MainMenu] Quit");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private class MenuItemController : MonoBehaviour,
            UnityEngine.EventSystems.IPointerEnterHandler,
            UnityEngine.EventSystems.IPointerExitHandler,
            UnityEngine.EventSystems.IPointerClickHandler
        {
            private TMP_Text _text;
            private Color _defaultColor;
            private Color _hoverColor;
            private float _defaultOpacity;
            private System.Action _onClick;
            private float _hoverScale;
            private float _normalScale;
            private Vector2 _originalPosition;
            private RectTransform _rectTransform;

            public void Init(TMP_Text text, Color defaultColor, Color hoverColor, float defaultOpacity,
                System.Action onClick, float hoverScale, float normalScale)
            {
                _text = text;
                _defaultColor = defaultColor;
                _hoverColor = hoverColor;
                _defaultOpacity = defaultOpacity;
                _onClick = onClick;
                _hoverScale = hoverScale;
                _normalScale = normalScale;
                _rectTransform = text?.GetComponent<RectTransform>();
                UpdateOriginalPosition();
            }

            public void UpdateOriginalPosition()
            {
                if (_rectTransform != null)
                {
                    _originalPosition = _rectTransform.anchoredPosition;
                }
            }

            public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
            {
                if (_text == null) return;
                _text.color = new Color(_hoverColor.r, _hoverColor.g, _hoverColor.b, 0.95f);
                if (_rectTransform != null)
                    _rectTransform.localScale = new Vector3(_hoverScale, _hoverScale, 1f);
            }

            public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
            {
                if (_text == null) return;
                _text.color = new Color(_defaultColor.r, _defaultColor.g, _defaultColor.b, _defaultOpacity);
                if (_rectTransform != null)
                {
                    _rectTransform.localScale = new Vector3(_normalScale, _normalScale, 1f);
                    _rectTransform.anchoredPosition = _originalPosition;
                }
            }

            public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
            {
                _onClick?.Invoke();
            }
        }
    }
}
