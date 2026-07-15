using UnityEngine;
using TMPro;
using System.Collections;

namespace HuaPi.UI.Panels.Exploration
{
    /// <summary>
    /// 当前目标提示：像墨迹或旧纸条般低调常驻，显示当前地点和短期目标。
    /// </summary>
    public class ObjectiveNoteView : MonoBehaviour
    {
        [Header("Text Components")]
        [SerializeField] private TextMeshProUGUI locationText;   // 当前地点
        [SerializeField] private TextMeshProUGUI objectiveText;  // 当前目标

        [Header("Visual Style")]
        [SerializeField] private Color inkColor = new Color(0.8f, 0.75f, 0.65f, 0.85f);
        [SerializeField] private Color dimColor = new Color(0.8f, 0.75f, 0.65f, 0.5f);

        [Header("Fade Settings")]
        [SerializeField] private float displayDuration = 8f;     // 显示后自动淡入低可见度
        [SerializeField] private float dimAlpha = 0.4f;          // 低可见度时的 Alpha

        private CanvasGroup _canvasGroup;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            // 初始状态：可见
            _canvasGroup.alpha = 1f;
            // 几秒后自动淡入低调状态
            if (displayDuration > 0)
            {
                _fadeCoroutine = StartCoroutine(AutoDim());
            }
        }

        /// <summary>
        /// 设置目标内容
        /// </summary>
        public void SetObjective(string location, string objective)
        {
            if (locationText != null)
            {
                locationText.text = $"<size=90%>當前地點</size>\n<size=120%>{location}</size>";
                locationText.color = inkColor;
            }

            if (objectiveText != null)
            {
                objectiveText.text = $"<size=90%>當前目標</size>\n{objective}";
                objectiveText.color = inkColor;
            }

            // 重置为可见状态
            _canvasGroup.alpha = 1f;
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            if (displayDuration > 0)
            {
                _fadeCoroutine = StartCoroutine(AutoDim());
            }
        }

        /// <summary>
        /// 短暂高亮提示（如目标更新时）
        /// </summary>
        public void PulseHighlight()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(PulseCoroutine());
        }

        private IEnumerator AutoDim()
        {
            yield return new WaitForSeconds(displayDuration);

            float elapsed = 0f;
            float duration = 2f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1f, dimAlpha, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = dimAlpha;
        }

        private IEnumerator PulseCoroutine()
        {
            // 先亮起来
            float elapsed = 0f;
            float duration = 0.5f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(dimAlpha, 1f, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            // 等待后再次淡入
            yield return new WaitForSeconds(displayDuration);
            elapsed = 0f;
            duration = 2f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1f, dimAlpha, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = dimAlpha;
        }
    }
}
