using System.Collections;
using UnityEngine;

namespace TXGame
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

            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            IsVisible = false;
        }

        public virtual void Init(object data)
        {
        }

        public virtual void Show()
        {
            if (IsVisible) return;
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(FadeInCoroutine());
        }

        public virtual void Hide()
        {
            if (!IsVisible) return;
            StopAllCoroutines();
            StartCoroutine(FadeOutCoroutine());
        }

        public virtual void Refresh(object data)
        {
        }

        protected virtual void OnClose()
        {
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

        protected virtual void OnShowComplete()
        {
        }
    }
}
