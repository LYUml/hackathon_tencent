using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TXGame
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

        private HuapiCharacterData _character;
        private HuapiClueData[] _availableClues;
        private HuapiClueData _selectedClue;

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
                observeButton.onClick.AddListener(OnObserveClicked);
            if (closeButton != null)
                closeButton.onClick.AddListener(() => UIManager.Instance.ClosePanel(UIPanelType.ObserveSkin));
        }

        public override void Init(object data)
        {
            base.Init(data);
            if (data is HuapiCharacterData character)
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
                characterNameText.color = new Color(0.788f, 0.541f, 0.431f);
            }
            if (roleText != null)
                roleText.text = $"{_character.roleType} · {_character.surfaceIdentity}";
            if (suspiciousText != null)
                suspiciousText.text = _character.suspiciousPoints;

            if (fogMaskImage != null)
                fogMaskImage.color = new Color(0.04f, 0.03f, 0.03f, 1f - _character.revealProgress * 0.8f);
        }

        public void SetAvailableClues(HuapiClueData[] clues)
        {
            _availableClues = clues;
            RefreshClueSlots();
        }

        private void RefreshClueSlots()
        {
            if (_availableClues == null || clueSlotsContainer == null) return;

            foreach (Transform child in clueSlotsContainer)
                Destroy(child.gameObject);

            foreach (HuapiClueData clue in _availableClues)
            {
                if (clue.isUsedInGuanPi) continue;
                CreateClueSlot(clue);
            }
        }

        private void CreateClueSlot(HuapiClueData clue)
        {
            if (clueSlotPrefab == null) return;

            GameObject slot = Instantiate(clueSlotPrefab, clueSlotsContainer);
            TMP_Text text = slot.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = clue.clueName;
                text.color = new Color(0.847f, 0.812f, 0.753f);
            }

            Button btn = slot.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => SelectClue(clue));
        }

        private void SelectClue(HuapiClueData clue)
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

            bool isMatch = CheckClueMatch(_selectedClue, _character);

            if (isMatch)
            {
                StartCoroutine(FogFadeOut());
                _character.revealProgress = Mathf.Clamp01(_character.revealProgress + 0.2f);
            }
            else
            {
                StartCoroutine(InkSwallowBack());
            }
        }

        private bool CheckClueMatch(HuapiClueData clue, HuapiCharacterData character)
        {
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
            Debug.Log("[ObserveSkin] Ink swallowed back...");
            yield return new WaitForSeconds(0.3f);
        }
    }
}
