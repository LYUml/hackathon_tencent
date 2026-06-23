using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HuaPi.UI.Core;
using HuaPi.UI.Data;

namespace HuaPi.UI.Panels
{
    /// <summary>
    /// 观皮面板：玩家直接面对被黑雾覆盖的画像
    /// </summary>
    public class ObserveSkinPanel : PanelBase
    {
        [Header("Portrait")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private Image fogMaskImage;
        [SerializeField] private RectTransform fogMaskContainer;

        [Header("Character Info")]
        [SerializeField] private TMP_Text characterNameText;
        [SerializeField] private TMP_Text roleText;
        [SerializeField] private TMP_Text suspiciousText;

        [Header("Clue Slots")]
        [SerializeField] private Transform clueSlotsContainer;
        [SerializeField] private GameObject clueSlotPrefab;

        [Header("Action")]
        [SerializeField] private Button observeButton;
        [SerializeField] private Button closeButton;

        private CharacterData _character;
        private ClueData[] _availableClues;
        private ClueData _selectedClue;

        protected override void Awake()
        {
            base.Awake();

            if (portraitImage == null) portraitImage = transform.Find("Portrait/Image")?.GetComponent<Image>();
            if (fogMaskImage == null) fogMaskImage = transform.Find("Portrait/FogMask")?.GetComponent<Image>();
            if (fogMaskContainer == null) fogMaskContainer = transform.Find("Portrait/FogMask")?.GetComponent<RectTransform>();
            if (characterNameText == null) characterNameText = transform.Find("CharacterInfo/Name")?.GetComponent<TMP_Text>();
            if (roleText == null) roleText = transform.Find("CharacterInfo/Role")?.GetComponent<TMP_Text>();
            if (suspiciousText == null) suspiciousText = transform.Find("CharacterInfo/Suspicious")?.GetComponent<TMP_Text>();
            if (clueSlotsContainer == null) clueSlotsContainer = transform.Find("ClueSlots/Container");
            if (observeButton == null) observeButton = transform.Find("Actions/ObserveButton")?.GetComponent<Button>();
            if (closeButton == null) closeButton = transform.Find("Actions/CloseButton")?.GetComponent<Button>();

            if (observeButton != null)
            {
                observeButton.onClick.AddListener(OnObserveClicked);
            }
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ClosePanel(UIPanelType.ObserveSkin);
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
                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            if (_character == null) return;

            if (characterNameText != null)
            {
                characterNameText.text = _character.characterName;
                characterNameText.color = new Color(0.788f, 0.541f, 0.431f); // #c9a96e
            }
            if (roleText != null)
            {
                roleText.text = $"{_character.roleType} · {_character.surfaceIdentity}";
            }
            if (suspiciousText != null)
            {
                suspiciousText.text = _character.suspiciousPoints;
            }

            // 黑雾遮罩
            if (fogMaskImage != null)
            {
                fogMaskImage.color = new Color(0.04f, 0.03f, 0.03f, 1f - _character.revealProgress * 0.8f);
            }
        }

        public void SetAvailableClues(ClueData[] clues)
        {
            _availableClues = clues;
            RefreshClueSlots();
        }

        private void RefreshClueSlots()
        {
            if (_availableClues == null || clueSlotsContainer == null) return;

            foreach (Transform child in clueSlotsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (ClueData clue in _availableClues)
            {
                if (clue.isUsedInGuanPi) continue;
                CreateClueSlot(clue);
            }
        }

        private void CreateClueSlot(ClueData clue)
        {
            if (clueSlotPrefab == null) return;

            GameObject slot = Instantiate(clueSlotPrefab, clueSlotsContainer);
            TMP_Text text = slot.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = clue.clueName;
                text.color = new Color(0.847f, 0.812f, 0.753f); // #d8cfc0
            }

            Button btn = slot.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => SelectClue(clue));
            }
        }

        private void SelectClue(ClueData clue)
        {
            _selectedClue = clue;
            Debug.Log($"[ObserveSkin] Selected clue: {clue.clueName}");
        }

        private void OnObserveClicked()
        {
            if (_selectedClue == null)
            {
                Debug.Log("[ObserveSkin] No clue selected");
                return;
            }

            if (_character == null) return;

            // 检查线索是否匹配
            bool isMatch = CheckClueMatch(_selectedClue, _character);

            if (isMatch)
            {
                // 正确匹配：黑雾退散
                StartCoroutine(FogFadeOut());
                _character.revealProgress = Mathf.Clamp01(_character.revealProgress + 0.2f);
            }
            else
            {
                // 错误匹配：墨吞回
                StartCoroutine(InkSwallowBack());
            }
        }

        private bool CheckClueMatch(ClueData clue, CharacterData character)
        {
            // TODO: 根据 ObserveSkinRevealData 判定匹配
            return clue.relatedCharacterId == character.characterId;
        }

        private System.Collections.IEnumerator FogFadeOut()
        {
            if (fogMaskImage != null)
            {
                float targetAlpha = 1f - _character.revealProgress * 0.8f;
                float startAlpha = fogMaskImage.color.a;
                float elapsed = 0f;
                float duration = 0.8f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    fogMaskImage.color = new Color(0.04f, 0.03f, 0.03f, Mathf.Lerp(startAlpha, targetAlpha, t));
                    yield return null;
                }
            }
            Debug.Log("[ObserveSkin] Fog faded!");
        }

        private System.Collections.IEnumerator InkSwallowBack()
        {
            // 错误反馈：选中的线索变暗并收缩
            if (_selectedClue != null)
            {
                Debug.Log("[ObserveSkin] Ink swallowed back...");
            }
            yield return new WaitForSeconds(0.3f);
        }
    }
}
