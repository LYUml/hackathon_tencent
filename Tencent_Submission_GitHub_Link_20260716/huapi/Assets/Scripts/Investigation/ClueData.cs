using UnityEngine;
using System;
using System.Collections.Generic;

namespace TXGame
{
    /// <summary>
    /// 线索数据定义
    /// </summary>
    [CreateAssetMenu(fileName = "ClueData", menuName = "画皮/线索数据")]
    public class ClueData : ScriptableObject
    {
        [Header("基本信息")]
        public string clueID;
        public string clueName;

        [TextArea(2, 4)]
        public string description;          // 线索描述

        [Header("分类")]
        public ClueCategory category;       // 线索类别
        public ClueSource source;           // 获取来源

        [Header("关联")]
        public string relatedCharacterID;   // 关联角色
        public string[] prerequisiteClueIDs; // 前置线索

        [Header("画皮系统")]
        [Range(0, 1)]
        public float revealProgress;        // 揭露进度贡献值
        public Vector2 revealAreaCenter;    // CG上揭露区域中心 (0~1 归一化)
        public float revealAreaRadius;      // CG上揭露区域半径 (0~1 归一化)

        public enum ClueCategory
        {
            物品,       // 实物证据
            对话,       // 对话中获取的信息
            场景,       // 场景中发现的环境线索
            文书,       // 信件、日记等
            记忆        // 回忆片段
        }

        public enum ClueSource
        {
            白天探索,
            角色对话,
            场景调查,
            夜晚推理,
            剧情触发
        }
    }
}
