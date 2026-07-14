using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace TXGame
{
    /// <summary>
    /// 昼夜循环 UI 指示器
    /// </summary>
    public class DayNightUI : MonoBehaviour
    {
        [Header("UI 元素")]
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI phaseText;
        [SerializeField] private Image phaseIcon;
        [SerializeField] private GameObject transitionOverlay;

        [Header("图标")]
        [SerializeField] private Sprite dayIcon;
        [SerializeField] private Sprite nightIcon;

        [Header("动画")]
        [SerializeField] private float transitionDuration = 1.5f;

        private void Start()
        {
            if (DayNightCycle.Instance != null)
            {
                DayNightCycle.Instance.OnDayStart.AddListener(OnDayStart);
                DayNightCycle.Instance.OnNightStart.AddListener(OnNightStart);
                DayNightCycle.Instance.OnPhaseChange.AddListener(OnPhaseChanged);
            }

            transitionOverlay?.SetActive(false);
            UpdateUI(DayNightCycle.TimeOfDay.Day, 1);
        }

        private void OnDayStart(int day)
        {
            StartCoroutine(PhaseTransition(DayNightCycle.TimeOfDay.Day, day));
        }

        private void OnNightStart(int day)
        {
            StartCoroutine(PhaseTransition(DayNightCycle.TimeOfDay.Night, day));
        }

        private void OnPhaseChanged(DayNightCycle.TimeOfDay phase)
        {
            // 不做过渡，直接更新
        }

        private IEnumerator PhaseTransition(DayNightCycle.TimeOfDay phase, int day)
        {
            // 过渡动画
            if (transitionOverlay != null)
            {
                transitionOverlay.SetActive(true);
                CanvasGroup overlayCG = transitionOverlay.GetComponent<CanvasGroup>();
                if (overlayCG != null)
                {
                    overlayCG.alpha = 0;

                    // 淡入
                    float elapsed = 0;
                    while (elapsed < transitionDuration / 2)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        overlayCG.alpha = Mathf.Lerp(0, 1, elapsed / (transitionDuration / 2));
                        yield return null;
                    }

                    UpdateUI(phase, day);

                    // 淡出
                    elapsed = 0;
                    while (elapsed < transitionDuration / 2)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        overlayCG.alpha = Mathf.Lerp(1, 0, elapsed / (transitionDuration / 2));
                        yield return null;
                    }
                }
                transitionOverlay.SetActive(false);
            }
            else
            {
                UpdateUI(phase, day);
            }
        }

        private void UpdateUI(DayNightCycle.TimeOfDay phase, int day)
        {
            if (dayText != null)
                dayText.text = $"第 {day} 天";

            if (phaseText != null)
            {
                phaseText.text = phase == DayNightCycle.TimeOfDay.Day ? "白昼" : "黑夜";
                phaseText.color = phase == DayNightCycle.TimeOfDay.Day
                    ? new Color(1f, 0.85f, 0.3f)
                    : new Color(0.3f, 0.4f, 0.8f);
            }

            if (phaseIcon != null)
            {
                phaseIcon.sprite = phase == DayNightCycle.TimeOfDay.Day ? dayIcon : nightIcon;
            }
        }
    }
}
