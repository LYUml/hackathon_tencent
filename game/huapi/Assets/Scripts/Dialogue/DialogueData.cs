using UnityEngine;
using System;
using System.Collections.Generic;

namespace TXGame
{
    /// <summary>
    /// 对话数据定义
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueData", menuName = "画皮/对话数据")]
    public class DialogueData : ScriptableObject
    {
        [Header("基本信息")]
        public string dialogueID;
        public string speakerID;            // 说话角色ID
        public string speakerName;          // 显示名称

        [Header("触发条件")]
        public List<DialogueCondition> conditions;  // 触发条件

        [Header("对话内容")]
        public List<DialogueLine> lines;

        [Header("选项分支")]
        public bool hasChoices;
        public List<DialogueChoice> choices;

        [Header("对话结果")]
        public List<DialogueResult> results;
    }

    [Serializable]
    public class DialogueCondition
    {
        public ConditionType type;
        public string parameter;        // 如线索ID、角色ID
        public float threshold;         // 如揭露进度阈值

        public enum ConditionType
        {
            HasClue,           // 拥有某线索
            RevealProgress,    // 角色揭露进度 >= threshold
            TimeOfDay,         // 特定时段 (0=白天, 1=夜晚)
            DayGreaterThan,    // 天数 >= threshold
            DialogueCompleted, // 完成过某对话
            HasItem,           // 拥有某物品
            SetFlag,           // 拥有某标记
            Always             // 始终可触发
        }
    }

    [Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public Sprite speakerPortrait;      // 说话人头像（可选）
        [TextArea(2, 5)]
        public string text;
        public float autoAdvanceDelay;      // 0 = 手动推进
    }

    [Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public string nextDialogueID;       // 选择后跳转的对话ID
        public List<DialogueResult> choiceResults; // 选择后的结果
    }

    [Serializable]
    public class DialogueResult
    {
        public ResultType type;
        public string parameter;

        public enum ResultType
        {
            UnlockClue,        // 解锁线索
            RevealCharacter,   // 揭露角色区域
            TriggerEvent,      // 触发事件
            SetFlag,           // 设置标记
            AdvanceTime,       // 推进时间
            StartDialogue      // 开始新对话
        }
    }
}
