using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace TXGame
{
    /// <summary>
    /// 调查笔记 UI - 夜晚观皮阶段的线索整理界面
    /// </summary>
    public class InvestigationUI : MonoBehaviour
    {
        [Header("面板")]
        [SerializeField] private GameObject investigationPanel;
        [SerializeField] private Transform clueListContainer;
        [SerializeField] private GameObject clueEntryPrefab;

        [Header("线索详情")]
        [SerializeField] private TextMeshProUGUI clueNameText;
        [SerializeField] private TextMeshProUGUI clueDescriptionText;
        [SerializeField] private TextMeshProUGUI clueCategoryText;
        [SerializeField] private Image clueIcon;

        [Header("推理笔记")]
        [SerializeField] private Transform notesContainer;
        [SerializeField] private GameObject noteEntryPrefab;
        [SerializeField] private TMP_InputField noteTitleInput;
        [SerializeField] private TMP_InputField noteContentInput;

        [Header("分类标签")]
        [SerializeField] private Toggle[] categoryToggles;

        private ClueData.ClueCategory currentFilter = ClueData.ClueCategory.物品;
        private bool showAllCategories = true;

        private void Start()
        {
            investigationPanel?.SetActive(false);
        }

        private void Update()
        {
            // Tab 切换调查面板
            if (UnityEngine.InputSystem.Keyboard.current?.tabKey.wasPressedThisFrame == true && DayNightCycle.Instance?.CurrentPhase == DayNightCycle.TimeOfDay.Night)
            {
                TogglePanel();
            }
        }

        public void TogglePanel()
        {
            bool isActive = investigationPanel != null && investigationPanel.activeSelf;
            investigationPanel?.SetActive(!isActive);

            if (!isActive)
            {
                RefreshClueList();
                RefreshNotes();
            }
        }

        public void ShowPanel()
        {
            investigationPanel?.SetActive(true);
            RefreshClueList();
            RefreshNotes();
        }

        public void HidePanel()
        {
            investigationPanel?.SetActive(false);
        }

        /// <summary>
        /// 刷新线索列表
        /// </summary>
        public void RefreshClueList()
        {
            if (clueListContainer == null || clueEntryPrefab == null) return;

            // 清除
            foreach (Transform child in clueListContainer)
                Destroy(child.gameObject);

            InvestigationManager inv = InvestigationManager.Instance;
            if (inv == null) return;

            List<ClueData> clues;
            if (showAllCategories)
            {
                clues = inv.GetCollectedClues();
            }
            else
            {
                clues = inv.GetCluesByCategory(currentFilter);
            }

            foreach (var clue in clues)
            {
                GameObject entry = Instantiate(clueEntryPrefab, clueListContainer);
                Button btn = entry.GetComponent<Button>();
                TextMeshProUGUI label = entry.GetComponentInChildren<TextMeshProUGUI>();

                if (label != null)
                    label.text = clue.clueName;

                if (btn != null)
                {
                    var capturedClue = clue;
                    btn.onClick.AddListener(() => ShowClueDetail(capturedClue));
                }
            }
        }

        /// <summary>
        /// 显示线索详情
        /// </summary>
        private void ShowClueDetail(ClueData clue)
        {
            if (clueNameText != null)
                clueNameText.text = clue.clueName;

            if (clueDescriptionText != null)
                clueDescriptionText.text = clue.description;

            if (clueCategoryText != null)
                clueCategoryText.text = $"类别: {GetCategoryName(clue.category)}";
        }

        /// <summary>
        /// 刷新推理笔记
        /// </summary>
        private void RefreshNotes()
        {
            if (notesContainer == null || noteEntryPrefab == null) return;

            foreach (Transform child in notesContainer)
                Destroy(child.gameObject);

            InvestigationManager inv = InvestigationManager.Instance;
            if (inv == null) return;

            foreach (var note in inv.GetNotes())
            {
                GameObject entry = Instantiate(noteEntryPrefab, notesContainer);
                TextMeshProUGUI[] texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2)
                {
                    texts[0].text = note.title;
                    texts[1].text = note.content;
                }
            }
        }

        /// <summary>
        /// 添加推理笔记
        /// </summary>
        public void AddNote()
        {
            if (noteTitleInput == null || noteContentInput == null) return;
            if (string.IsNullOrWhiteSpace(noteTitleInput.text)) return;

            InvestigationManager.Instance?.AddNote(
                noteTitleInput.text,
                noteContentInput.text
            );

            noteTitleInput.text = "";
            noteContentInput.text = "";
            RefreshNotes();
        }

        /// <summary>
        /// 按类别过滤
        /// </summary>
        public void FilterByCategory(int categoryIndex)
        {
            showAllCategories = false;
            currentFilter = (ClueData.ClueCategory)categoryIndex;
            RefreshClueList();
        }

        /// <summary>
        /// 显示全部线索
        /// </summary>
        public void ShowAllClues()
        {
            showAllCategories = true;
            RefreshClueList();
        }

        private string GetCategoryName(ClueData.ClueCategory category)
        {
            return category switch
            {
                ClueData.ClueCategory.物品 => "实物证据",
                ClueData.ClueCategory.对话 => "对话信息",
                ClueData.ClueCategory.场景 => "环境线索",
                ClueData.ClueCategory.文书 => "文书信件",
                ClueData.ClueCategory.记忆 => "回忆片段",
                _ => "未知"
            };
        }
    }
}
