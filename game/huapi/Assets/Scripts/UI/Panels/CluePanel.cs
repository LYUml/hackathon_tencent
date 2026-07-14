using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TXGame
{
    /// <summary>
    /// 线索面板：像散落的旧纸和侦探笔记
    /// </summary>
    public class CluePanel : PanelBase
    {
        [Header("List")]
        [SerializeField] private Transform clueListContainer;
        [SerializeField] private GameObject clueCardPrefab;

        [Header("Detail")]
        [SerializeField] private TMP_Text detailTitleText;
        [SerializeField] private TMP_Text detailSourceText;
        [SerializeField] private TMP_Text detailDescriptionText;
        [SerializeField] private TMP_Text detailRelatedText;
        [SerializeField] private TMP_Text detailStatusText;

        [Header("Close")]
        [SerializeField] private Button closeButton;

        private HuapiClueData[] _allClues;
        private HuapiClueData _selectedClue;

        protected override void Awake()
        {
            base.Awake();

            if (clueListContainer == null) clueListContainer = transform.Find("ClueList/Container");
            if (detailTitleText == null) detailTitleText = transform.Find("Detail/Title")?.GetComponent<TMP_Text>();
            if (detailSourceText == null) detailSourceText = transform.Find("Detail/Source")?.GetComponent<TMP_Text>();
            if (detailDescriptionText == null) detailDescriptionText = transform.Find("Detail/Description")?.GetComponent<TMP_Text>();
            if (detailRelatedText == null) detailRelatedText = transform.Find("Detail/Related")?.GetComponent<TMP_Text>();
            if (detailStatusText == null) detailStatusText = transform.Find("Detail/Status")?.GetComponent<TMP_Text>();
            if (closeButton == null) closeButton = transform.Find("CloseButton")?.GetComponent<Button>();

            if (closeButton != null)
                closeButton.onClick.AddListener(() => UIManager.Instance.ClosePanel(UIPanelType.ClueInventory));
        }

        public override void Init(object data)
        {
            base.Init(data);
            if (data is HuapiClueData[] clues)
            {
                _allClues = clues;
                RefreshClueList();
            }
        }

        public override void Refresh(object data)
        {
            base.Refresh(data);
            if (data is HuapiClueData[] clues)
            {
                _allClues = clues;
                RefreshClueList();
            }
        }

        private void RefreshClueList()
        {
            if (_allClues == null || clueListContainer == null) return;

            foreach (Transform child in clueListContainer)
                Destroy(child.gameObject);

            foreach (HuapiClueData clue in _allClues)
            {
                if (!clue.isAcquired) continue;
                CreateClueCard(clue);
            }
        }

        private void CreateClueCard(HuapiClueData clue)
        {
            if (clueCardPrefab == null) return;

            GameObject card = Instantiate(clueCardPrefab, clueListContainer);
            TMP_Text text = card.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = clue.clueName;
                text.color = clue.isUsedInGuanPi
                    ? new Color(0.545f, 0.125f, 0.125f)
                    : new Color(0.847f, 0.812f, 0.753f);
            }

            Button btn = card.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => SelectClue(clue));

            RectTransform rect = card.GetComponent<RectTransform>();
            if (rect != null)
                rect.rotation = Quaternion.Euler(0, 0, Random.Range(-3f, 3f));
        }

        private void SelectClue(HuapiClueData clue)
        {
            _selectedClue = clue;
            ShowClueDetail(clue);
        }

        private void ShowClueDetail(HuapiClueData clue)
        {
            if (detailTitleText != null)
            {
                detailTitleText.text = clue.clueName;
                detailTitleText.color = new Color(0.231f, 0.165f, 0.102f);
            }
            if (detailSourceText != null)
                detailSourceText.text = $"来源：{clue.sourceLocation} · {clue.sourceCharacter}";
            if (detailDescriptionText != null)
                detailDescriptionText.text = clue.description;
            if (detailRelatedText != null)
                detailRelatedText.text = $"关联角色：{clue.relatedCharacterId}";
            if (detailStatusText != null)
            {
                detailStatusText.text = clue.isUsedInGuanPi ? "已用于观皮" : "未使用";
                detailStatusText.color = clue.isUsedInGuanPi
                    ? new Color(0.545f, 0.125f, 0.125f)
                    : new Color(0.541f, 0.478f, 0.431f);
            }
        }
    }
}
