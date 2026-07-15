using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HuaPi.UI.Core;
using HuaPi.UI.Data;

namespace HuaPi.UI.Panels
{
    /// <summary>
    /// 角色档案面板：像摊开的侦探笔记与旧照片
    /// </summary>
    public class CharacterArchivePanel : PanelBase
    {
        [Header("Portrait")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private Image fogMask;

        [Header("Info")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text roleText;
        [SerializeField] private TMP_Text identityText;
        [SerializeField] private TMP_Text knownInfoText;
        [SerializeField] private TMP_Text suspiciousText;

        [Header("Progress")]
        [SerializeField] private Image progressBarFill;
        [SerializeField] private TMP_Text progressText;

        [Header("Close")]
        [SerializeField] private Button closeButton;

        private CharacterData _character;

        protected override void Awake()
        {
            base.Awake();

            if (portraitImage == null) portraitImage = transform.Find("Portrait/Image")?.GetComponent<Image>();
            if (fogMask == null) fogMask = transform.Find("Portrait/FogMask")?.GetComponent<Image>();
            if (nameText == null) nameText = transform.Find("Info/Name")?.GetComponent<TMP_Text>();
            if (roleText == null) roleText = transform.Find("Info/Role")?.GetComponent<TMP_Text>();
            if (identityText == null) identityText = transform.Find("Info/Identity")?.GetComponent<TMP_Text>();
            if (knownInfoText == null) knownInfoText = transform.Find("Info/KnownInfo")?.GetComponent<TMP_Text>();
            if (suspiciousText == null) suspiciousText = transform.Find("Info/Suspicious")?.GetComponent<TMP_Text>();
            if (progressBarFill == null) progressBarFill = transform.Find("Progress/Bar/Fill")?.GetComponent<Image>();
            if (progressText == null) progressText = transform.Find("Progress/Text")?.GetComponent<TMP_Text>();
            if (closeButton == null) closeButton = transform.Find("CloseButton")?.GetComponent<Button>();

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ClosePanel(UIPanelType.CharacterArchive);
                    }
                });
            }
        }

        public override void Init(object data)
        {
            base.Init(data);
            if (data is CharacterData character)
            {
                _character = character;
                RefreshCharacterInfo();
            }
        }

        public override void Refresh(object data)
        {
            base.Refresh(data);
            if (data is CharacterData character)
            {
                _character = character;
                RefreshCharacterInfo();
            }
        }

        private void RefreshCharacterInfo()
        {
            if (_character == null) return;

            if (nameText != null)
            {
                nameText.text = _character.characterName;
                nameText.color = _character.roleColor;
            }
            if (roleText != null)
            {
                roleText.text = $"{_character.roleType} · {_character.surfaceIdentity}";
            }
            if (identityText != null)
            {
                identityText.text = $"表面身份：{_character.surfaceIdentity}";
            }
            if (knownInfoText != null)
            {
                knownInfoText.text = _character.knownInfo;
            }
            if (suspiciousText != null)
            {
                suspiciousText.text = _character.suspiciousPoints;
            }

            // 进度条
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = _character.revealProgress;
                progressBarFill.color = new Color(0.545f, 0.125f, 0.125f); // #8b2020
            }
            if (progressText != null)
            {
                progressText.text = $"揭露进度：{_character.revealProgress * 100:F0}%";
            }

            // 黑雾遮罩
            if (fogMask != null)
            {
                fogMask.color = new Color(0.04f, 0.03f, 0.03f, 1f - _character.revealProgress * 0.8f);
            }
        }
    }
}
