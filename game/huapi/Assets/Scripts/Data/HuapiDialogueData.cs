using UnityEngine;

namespace TXGame
{
    /// <summary>
    /// 对话行数据（UI用）
    /// </summary>
    [System.Serializable]
    public class HuapiDialogueLine
    {
        [Tooltip("说话人ID")]
        public string speakerId;

        [Tooltip("对话文本")]
        [TextArea(2, 4)]
        public string text;

        [Tooltip("此对话获得的线索ID列表")]
        public string[] clueIdsToGrant;

        [Tooltip("需要高亮的关键词")]
        public string[] highlightedKeywords;
    }

    /// <summary>
    /// 对话选项数据（UI用）
    /// </summary>
    [System.Serializable]
    public class HuapiDialogueOption
    {
        [Tooltip("选项文本")]
        public string optionText;

        [Tooltip("下一个节点ID")]
        public int nextNodeId;

        [Tooltip("需要的线索ID（为空则不限制）")]
        public string[] requiredClueIds;

        [Tooltip("消耗的线索ID")]
        public string[] clueIdsToConsume;
    }

    /// <summary>
    /// 对话节点数据（UI用）：ScriptableObject，驱动对话面板
    /// 对应 hackathon 仓库的 HuaPi.UI.Data.DialogueData
    /// </summary>
    [CreateAssetMenu(fileName = "HuapiDialogueData", menuName = "画皮/UI对话数据")]
    public class HuapiDialogueData : ScriptableObject
    {
        [Tooltip("对话节点ID")]
        public int nodeId;

        [Tooltip("对话行列表")]
        public HuapiDialogueLine[] lines;

        [Tooltip("对话选项列表")]
        public HuapiDialogueOption[] options;
    }
}
