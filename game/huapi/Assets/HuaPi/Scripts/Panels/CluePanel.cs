using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HuaPi.UI.Core;
using HuaPi.UI.Data;

namespace HuaPi.UI.Panels
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

        [Header("Filter")]
        [SerializeField] private Transform filterContainer;

        [Header("Close")]
        [SerializeField] private Button closeButton;

        private ClueData[] _allClues;
        private ClueData _selectedClue;

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
            {
                closeButton.onClick.AddListener(() =>
                {
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ClosePanel(UIPanelType.ClueInventory);
                    }
                });
            }
        }

        public override void Init(object data)
        {
            base.Init(data);
            if (data is ClueData[] clues)
            {
                _allClues = clues;
                RefreshClueList();
            }
        }

        public override void Refresh(object data)
        {
            base.Refresh(data);
            if (data is ClueData[] clues)
            {
                _allClues = clues;
                RefreshClueList();
            }
        }

        private void RefreshClueList()
        {
            if (_allClues == null || clueListContainer == null) return;

            // 清除旧列表
            foreach (Transform child in clueListContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (ClueData clue in _allClues)
            {
                if (!clue.isAcquired) continue;
                CreateClueCard(clue);
            }
        }

        private void CreateClueCard(ClueData clue)
        {
            if (clueCardPrefab == null) return;

            GameObject card = Instantiate(clueCardPrefab, clueListContainer);
            TMP_Text text = card.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = clue.clueName;
                text.color = clue.isUsedInGuanPi
                    ? new Color(0.545f, 0.125f, 0.125f) // #8b2020 已使用
                    : new Color(0.847f, 0.812f, 0.753f); // #d8cfc0
            }

            Button btn = card.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => SelectClue(clue));
            }

            // 旧纸轻微旋转
            RectTransform rect = card.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.rotation = Quaternion.Euler(0, 0, Random.Range(-3f, 3f));
            }
        }

        private void SelectClue(ClueData clue)
        {
            _selectedClue = clue;
            ShowClueDetail(clue);
        }

        private void ShowClueDetail(ClueData clue)
        {
            if (detailTitleText != null)
            {
                detailTitleText.text = clue.clueName;
                detailTitleText.color = new Color(0.231f, 0.165f, 0.102f); // #3a2a1a
            }
            if (detailSourceText != null)
            {
                detailSourceText.text = $"来源：{clue.sourceLocation} · {clue.sourceCharacter}";
            }
            if (detailDescriptionText != null)
            {
                detailDescriptionText.text = clue.description;
            }
            if (detailRelatedText != null)
            {
                detailRelatedText.text = $"关联角色：{clue.relatedCharacterId}";
            }
            if (detailStatusText != null)
            {
                detailStatusText.text = clue.isUsedInGuanPi ? "已用于观皮" : "未使用";
                detailStatusText.color = clue.isUsedInGuanPi
                    ? new Color(0.545f, 0.125f, 0.125f) // #8b2020
                    : new Color(0.541f, 0.478f, 0.431f); // #8a7a6e
            }
        }
    }
}
