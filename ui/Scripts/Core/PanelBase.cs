using System.Collections;
using UnityEngine;

namespace HuaPi.UI.Core
{
    /// <summary>
    /// 所有 UI 面板的抽象基类：提供 Init、Show、Hide、Refresh 标准生命周期。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PanelBase : MonoBehaviour
    {
        [Header("Panel Settings")]
        [SerializeField] protected float fadeInDuration = 0.3f;
        [SerializeField] protected float fadeOutDuration = 0.2f;

        protected CanvasGroup CanvasGroup { get; private set; }
        protected RectTransform RectTransform { get; private set; }
        protected bool IsVisible { get; private set; }

        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            RectTransform = GetComponent<RectTransform>();

            // 初始状态：隐藏
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            IsVisible = false;
        }

        /// <summary>
        /// 初始化面板，接收数据
        /// </summary>
        public virtual void Init(object data)
        {
            // 子类重写
        }

        /// <summary>
        /// 显示面板（播放淡入动画）
        /// </summary>
        public virtual void Show()
        {
            if (IsVisible) return;
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(FadeInCoroutine());
        }

        /// <summary>
        /// 隐藏面板（播放淡出动画）
        /// </summary>
        public virtual void Hide()
        {
            if (!IsVisible) return;
            StopAllCoroutines();
            StartCoroutine(FadeOutCoroutine());
        }

        /// <summary>
        /// 根据数据刷新面板
        /// </summary>
        public virtual void Refresh(object data)
        {
            // 子类重写
        }

        /// <summary>
        /// 关闭回调（可通知调用者）
        /// </summary>
        protected virtual void OnClose()
        {
            // 子类重写
        }

        private IEnumerator FadeInCoroutine()
        {
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                CanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                yield return null;
            }

            CanvasGroup.alpha = 1f;
            IsVisible = true;
            OnShowComplete();
        }

        private IEnumerator FadeOutCoroutine()
        {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;

            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                CanvasGroup.alpha = Mathf.Clamp01(1f - elapsed / fadeOutDuration);
                yield return null;
            }

            CanvasGroup.alpha = 0f;
            IsVisible = false;
            gameObject.SetActive(false);
            OnClose();
        }

        /// <summary>
        /// 显示完成回调
        /// </summary>
        protected virtual void OnShowComplete()
        {
            // 子类重写
        }
    }
}
