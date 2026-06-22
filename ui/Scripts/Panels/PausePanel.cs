using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HuaPi.UI.Core;

namespace HuaPi.UI.Panels
{
    /// <summary>
    /// 暂停面板：暗场中的微弱烛光/墨迹字
    /// </summary>
    public class PausePanel : PanelBase
    {
        [Header("Background")]
        [SerializeField] private Image overlayImage;

        [Header("Menu Items")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button volumeButton;
        [SerializeField] private Button textSpeedButton;
        [SerializeField] private Button brightnessButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        [Header("Labels")]
        [SerializeField] private TMP_Text resumeLabel;
        [SerializeField] private TMP_Text volumeLabel;
        [SerializeField] private TMP_Text textSpeedLabel;
        [SerializeField] private TMP_Text brightnessLabel;
        [SerializeField] private TMP_Text mainMenuLabel;
        [SerializeField] private TMP_Text quitLabel;

        [Header("Settings")]
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Slider textSpeedSlider;
        [SerializeField] private Slider brightnessSlider;

        protected override void Awake()
        {
            base.Awake();

            // 自动查找
            if (overlayImage == null) overlayImage = transform.Find("Overlay")?.GetComponent<Image>();
            if (resumeButton == null) resumeButton = transform.Find("Menu/Resume")?.GetComponent<Button>();
            if (volumeButton == null) volumeButton = transform.Find("Menu/Volume")?.GetComponent<Button>();
            if (textSpeedButton == null) textSpeedButton = transform.Find("Menu/TextSpeed")?.GetComponent<Button>();
            if (brightnessButton == null) brightnessButton = transform.Find("Menu/Brightness")?.GetComponent<Button>();
            if (mainMenuButton == null) mainMenuButton = transform.Find("Menu/MainMenu")?.GetComponent<Button>();
            if (quitButton == null) quitButton = transform.Find("Menu/Quit")?.GetComponent<Button>();

            if (volumeSlider == null) volumeSlider = transform.Find("Settings/VolumeSlider")?.GetComponent<Slider>();
            if (textSpeedSlider == null) textSpeedSlider = transform.Find("Settings/TextSpeedSlider")?.GetComponent<Slider>();
            if (brightnessSlider == null) brightnessSlider = transform.Find("Settings/BrightnessSlider")?.GetComponent<Slider>();

            // 设置半透明黑底
            if (overlayImage != null)
            {
                overlayImage.color = new Color(0.04f, 0.03f, 0.03f, 0.8f); // #0a0808, opacity 0.8
            }

            // 设置文字颜色
            SetLabelColor(resumeLabel);
            SetLabelColor(volumeLabel);
            SetLabelColor(textSpeedLabel);
            SetLabelColor(brightnessLabel);
            SetLabelColor(mainMenuLabel);
            SetLabelColor(quitLabel);

            // 绑定按钮事件
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResume);
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenu);
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuit);
        }

        private void SetLabelColor(TMP_Text label)
        {
            if (label != null)
            {
                label.color = new Color(0.788f, 0.541f, 0.431f); // #c9a96e
            }
        }

        private void OnResume()
        {
            UIManager.Instance.ClosePanel(UIPanelType.Pause);
        }

        private void OnMainMenu()
        {
            UIManager.Instance.CloseAllPanels();
            UIManager.Instance.OpenPanel(UIPanelType.MainMenu);
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
