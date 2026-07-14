using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TXGame
{
    /// <summary>
    /// 调查管理器 - 管理线索收集、推理笔记
    /// </summary>
    public class InvestigationManager : MonoBehaviour
    {
        public static InvestigationManager Instance { get; private set; }

        [Header("线索数据库")]
        [SerializeField] private ClueData[] allClues;

        [Header("事件")]
        public System.Action<ClueData> OnClueCollected;
        public System.Action<ClueData, float> OnClueRevealed;  // 线索 + 角色揭露进度

        // 已收集的线索
        private HashSet<string> collectedClueIDs = new HashSet<string>();

        // 推理笔记
        private List<NoteEntry> investigationNotes = new List<NoteEntry>();

        [System.Serializable]
        public class NoteEntry
        {
            public string title;
            [TextArea(2, 5)]
            public string content;
            public string relatedClueID;
            public string relatedCharacterID;
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            LoadAllClues();
        }

        private void LoadAllClues()
        {
            allClues = Resources.LoadAll<ClueData>("Clues");
#if UNITY_EDITOR
            if (allClues == null || allClues.Length == 0)
            {
                string[] guids = AssetDatabase.FindAssets("t:ClueData", new[] { "Assets/Data/Clues" });
                List<ClueData> editorClues = new List<ClueData>();
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    ClueData clue = AssetDatabase.LoadAssetAtPath<ClueData>(path);
                    if (clue != null) editorClues.Add(clue);
                }
                allClues = editorClues.ToArray();
            }
#endif
            if (allClues == null || allClues.Length == 0)
            {
                Debug.LogWarning("未找到线索数据！请在 Resources/Clues/ 下放置 ClueData 文件");
            }
        }

        /// <summary>
        /// 添加线索
        /// </summary>
        public bool AddClue(string clueID)
        {
            if (collectedClueIDs.Contains(clueID))
                return false;

            collectedClueIDs.Add(clueID);

            // 查找线索数据
            ClueData clue = GetClueData(clueID);
            if (clue != null)
            {
                OnClueCollected?.Invoke(clue);

                // 触发画皮揭露
                if (!string.IsNullOrEmpty(clue.relatedCharacterID))
                {
                    HuapiSystem huapi = FindObjectOfType<HuapiSystem>();
                    if (huapi != null)
                    {
                        huapi.RevealArea(
                            clue.relatedCharacterID,
                            clue.revealAreaCenter.x,
                            clue.revealAreaCenter.y,
                            clue.revealAreaRadius
                        );

                        float progress = huapi.GetRevealProgress(clue.relatedCharacterID);
                        OnClueRevealed?.Invoke(clue, progress);
                    }
                }

                Debug.Log($"获得线索: {clue.clueName} [{clueID}]");
            }

            return true;
        }

        /// <summary>
        /// 检查是否拥有某线索
        /// </summary>
        public bool HasClue(string clueID)
        {
            return collectedClueIDs.Contains(clueID);
        }

        /// <summary>
        /// 获取已收集的线索列表
        /// </summary>
        public List<ClueData> GetCollectedClues()
        {
            List<ClueData> result = new List<ClueData>();
            foreach (string id in collectedClueIDs)
            {
                ClueData clue = GetClueData(id);
                if (clue != null) result.Add(clue);
            }
            return result;
        }

        /// <summary>
        /// 获取特定类别的线索
        /// </summary>
        public List<ClueData> GetCluesByCategory(ClueData.ClueCategory category)
        {
            List<ClueData> result = new List<ClueData>();
            foreach (string id in collectedClueIDs)
            {
                ClueData clue = GetClueData(id);
                if (clue != null && clue.category == category)
                    result.Add(clue);
            }
            return result;
        }

        /// <summary>
        /// 获取关联某角色的线索
        /// </summary>
        public List<ClueData> GetCluesForCharacter(string characterID)
        {
            List<ClueData> result = new List<ClueData>();
            foreach (string id in collectedClueIDs)
            {
                ClueData clue = GetClueData(id);
                if (clue != null && clue.relatedCharacterID == characterID)
                    result.Add(clue);
            }
            return result;
        }

        /// <summary>
        /// 添加推理笔记
        /// </summary>
        public void AddNote(string title, string content, string clueID = "", string characterID = "")
        {
            investigationNotes.Add(new NoteEntry
            {
                title = title,
                content = content,
                relatedClueID = clueID,
                relatedCharacterID = characterID
            });
        }

        /// <summary>
        /// 获取推理笔记
        /// </summary>
        public List<NoteEntry> GetNotes()
        {
            return new List<NoteEntry>(investigationNotes);
        }

        private ClueData GetClueData(string clueID)
        {
            if (allClues == null) return null;
            foreach (var clue in allClues)
            {
                if (clue.clueID == clueID) return clue;
            }
            return null;
        }
    }
}
