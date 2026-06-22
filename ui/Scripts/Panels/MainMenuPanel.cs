using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HuaPi.UI.Core;

namespace HuaPi.UI.Panels
{
    /// <summary>
    /// 主菜单面板：纯净沉浸版，无边界、无框、无底板
    /// 使用 TMP_Text 直接显示文字，不依赖外部美术资源
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

        [Header("Hover Colors")]
        [SerializeField] private Color defaultColor = new Color(0.788f, 0.541f, 0.431f, 0.85f); // #c9a96e
        [SerializeField] private Color hoverColor = new Color(0.545f, 0.125f, 0.125f, 0.9f);   // #8b2020
        [SerializeField] private Color disabledColor = new Color(0.541f, 0.478f, 0.431f, 0.55f); // #8a7a6e

        private TMP_Text[] _menuItems;
        private int _selectedIndex = 0;

        protected override void Awake()
        {
            base.Awake();
            _menuItems = new[] { startGameText, continueGameText, settingsText, quitText };

            // 如果没有赋值，尝试在子对象中查找
            if (titleText == null) titleText = transform.Find("Title")?.GetComponent<TMP_Text>();
            if (subtitleText == null) subtitleText = transform.Find("Subtitle")?.GetComponent<TMP_Text>();
            if (startGameText == null) startGameText = transform.Find("Menu/StartGame")?.GetComponent<TMP_Text>();
            if (continueGameText == null) continueGameText = transform.Find("Menu/ContinueGame")?.GetComponent<TMP_Text>();
            if (settingsText == null) settingsText = transform.Find("Menu/Settings")?.GetComponent<TMP_Text>();
            if (quitText == null) quitText = transform.Find("Menu/Quit")?.GetComponent<TMP_Text>();
            if (bottomInfoText == null) bottomInfoText = transform.Find("BottomInfo")?.GetComponent<TMP_Text>();
        }

        public override void Init(object data)
        {
            base.Init(data);
            SetupMenuItems();
        }

        private void SetupMenuItems()
        {
            // 设置标题
            if (titleText != null)
            {
                titleText.text = "画皮";
                titleText.color = hoverColor; // 暗红
                titleText.alpha = 0.9f;
            }

            if (subtitleText != null)
            {
                subtitleText.text = "MASKS BEHIND MASKS";
                subtitleText.color = new Color(0.353f, 0.227f, 0.165f, 0.6f); // #5a3a2a
            }

            // 设置菜单项
            SetMenuText(startGameText, "开始游戏", 0.85f);
            SetMenuText(continueGameText, "继续游戏", 0.7f);
            SetMenuText(settingsText, "设置", 0.7f);
            SetMenuText(quitText, "退出", 0.55f);

            // 设置底部信息
            if (bottomInfoText != null)
            {
                bottomInfoText.text = "薛家戏班 · 民国二十年";
                bottomInfoText.color = new Color(0.239f, 0.082f, 0.094f, 0.4f); // #3d1518
            }

            // 添加点击事件
            AddClickListener(startGameText, OnStartGame);
            AddClickListener(continueGameText, OnContinueGame);
            AddClickListener(settingsText, OnSettings);
            AddClickListener(quitText, OnQuit);
        }

        private void SetMenuText(TMP_Text text, string content, float opacity)
        {
            if (text == null) return;
            text.text = content;
            text.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, opacity);
        }

        private void AddClickListener(TMP_Text text, UnityEngine.Events.UnityAction action)
        {
            if (text == null) return;
            var button = text.GetComponent<Button>();
            if (button == null)
            {
                button = text.gameObject.AddComponent<Button>();
            }
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);

            // Hover 效果：暗红渗墨
            var hover = text.gameObject.AddComponent<MenuItemHover>();
            hover.Init(text, defaultColor, hoverColor);
        }

        private void OnStartGame()
        {
            Debug.Log("[MainMenu] Start Game");
            // TODO: 切换到游戏场景
            UIManager.Instance.ClosePanel(UIPanelType.MainMenu);
        }

        private void OnContinueGame()
        {
            Debug.Log("[MainMenu] Continue Game");
            // TODO: 加载存档
        }

        private void OnSettings()
        {
            Debug.Log("[MainMenu] Settings");
            UIManager.Instance.OpenPanel(UIPanelType.Pause);
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

        /// <summary>
        /// 菜单项 Hover 效果辅助类
        /// </summary>
        private class MenuItemHover : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
        {
            private TMP_Text _text;
            private Color _defaultColor;
            private Color _hoverColor;

            public void Init(TMP_Text text, Color defaultColor, Color hoverColor)
            {
                _text = text;
                _defaultColor = defaultColor;
                _hoverColor = hoverColor;
            }

            public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
            {
                if (_text == null) return;
                StopAllCoroutines();
                StartCoroutine(FadeToColor(_text, _hoverColor, 0.2f));

                // 文字轻微向左偏移（扭曲感）
                var rect = _text.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x - 1f, rect.anchoredPosition.y);
                }
            }

            public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
            {
                if (_text == null) return;
                StopAllCoroutines();
                StartCoroutine(FadeToColor(_text, _defaultColor, 0.2f));

                var rect = _text.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x + 1f, rect.anchoredPosition.y);
                }
            }

            private System.Collections.IEnumerator FadeToColor(TMP_Text text, Color target, float duration)
            {
                Color start = text.color;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    text.color = Color.Lerp(start, target, elapsed / duration);
                    yield return null;
                }
                text.color = target;
            }
        }
    }
}
