using UnityEngine;
using TMPro;
using System.Collections;
using HuaPi.UI.Data;

namespace HuaPi.UI.Panels.Exploration
{
    /// <summary>
    /// 获得线索提示：像旧纸条、暗红印章或戏票从画面边缘滑入。
    /// 不遮挡场景，几秒后自然淡出，但可点击打开线索页。
    /// </summary>
    public class ClueToastView : MonoBehaviour
    {
        [Header("Text Components")]
        [SerializeField] private TextMeshProUGUI titleLabel;       // "获得线索"
        [SerializeField] private TextMeshProUGUI clueNameLabel;    // 线索名称
        [SerializeField] private TextMeshProUGUI clueDescLabel;    // 简短描述

        [Header("Visual Style")]
        [SerializeField] private Color titleColor = new Color(0.75f, 0.2f, 0.15f, 1f);    // 暗红色
        [SerializeField] private Color nameColor = new Color(0.85f, 0.8f, 0.7f, 1f);       // 旧纸色
        [SerializeField] private Color descColor = new Color(0.7f, 0.7f, 0.65f, 0.9f);       // 淡灰

        [Header("Animation")]
        [SerializeField] private float slideInDuration = 0.6f;
        [SerializeField] private float displayDuration = 6f;
        [SerializeField] private float fadeOutDuration = 1.2f;
        [SerializeField] private Vector2 hiddenOffset = new Vector2(400, 0);  // 从右侧滑入

        [Header("Click")]
        [SerializeField] private bool clickableToCluePanel = true;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Coroutine _animationCoroutine;
        private Vector2 _shownPosition;

        private ClueData _currentClueData;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            _shownPosition = _rectTransform.anchoredPosition;
        }

        private void Start()
        {
            // 初始隐藏
            _rectTransform.anchoredPosition = _shownPosition + hiddenOffset;
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// 显示线索获得提示
        /// </summary>
        public void ShowClueToast(ClueData clueData)
        {
            if (clueData == null) return;
            _currentClueData = clueData;

            // 设置文本
            if (titleLabel != null)
            {
                titleLabel.text = "获得线索";
                titleLabel.color = titleColor;
            }

            if (clueNameLabel != null)
            {
                clueNameLabel.text = clueData.clueName;
                clueNameLabel.color = nameColor;
            }

            if (clueDescLabel != null)
            {
                // 显示简短描述（最多50字）
                string shortDesc = clueData.description;
                if (shortDesc.Length > 50)
                {
                    shortDesc = shortDesc.Substring(0, 50) + "……";
                }
                clueDescLabel.text = shortDesc;
                clueDescLabel.color = descColor;
            }

            // 停止之前的动画
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            // 开始新动画
            _animationCoroutine = StartCoroutine(ShowAnimation());
        }

        /// <summary>
        /// 外部点击回调：打开线索页
        /// </summary>
        public void OnClick()
        {
            if (clickableToCluePanel && _currentClueData != null)
            {
                // 通知 ExplorationPanel 打开线索页
                ExplorationPanel.Instance?.OpenCluePanel();
            }
        }

        private IEnumerator ShowAnimation()
        {
            _canvasGroup.blocksRaycasts = true;

            // 1. 滑入
            float elapsed = 0f;
            while (elapsed < slideInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slideInDuration;
                t = Mathf.SmoothStep(0, 1, t);
                _rectTransform.anchoredPosition = Vector2.Lerp(_shownPosition + hiddenOffset, _shownPosition, t);
                _canvasGroup.alpha = t;
                yield return null;
            }
            _rectTransform.anchoredPosition = _shownPosition;
            _canvasGroup.alpha = 1f;

            // 2. 显示停留
            yield return new WaitForSeconds(displayDuration);

            // 3. 淡出
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutDuration;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _rectTransform.anchoredPosition = _shownPosition + hiddenOffset;
        }
    }
}
