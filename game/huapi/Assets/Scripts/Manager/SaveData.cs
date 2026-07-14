using UnityEngine;
using System;

namespace TXGame
{
    /// <summary>
    /// 游戏存档数据 - 可序列化的游戏状态
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // 游戏进度
        public int saveVersion = 1;
        public string slotId;
        public string savedAt;
        public string displayTitle;

        public int currentDay;
        public string currentSceneName;
        public float gameTime;
        public int hearts = 5;
        public string uiViewName;
        public bool introCompleted;
        public string phaseName;
        public string gameplayBackgroundPath;

        // 已收集的线索
        public string[] collectedClueIDs;

        // 角色揭露进度 (characterID -> 0~1)
        public CharacterProgress[] characterProgresses;

        // 已完成的对话
        public string[] completedDialogueIDs;

        // 当前阶段
        public GamePhase currentPhase;

        // 设置
        public float bgmVolume = 1f;
        public float sfxVolume = 1f;
        public float textSpeed = 1f;
    }

    [Serializable]
    public class CharacterProgress
    {
        public string characterID;
        public float revealProgress;    // 0~1
    }

    public enum GamePhase
    {
        序章,
        白天探索,
        夜晚观皮,
        关键揭露,
        终章
    }
}
