using UnityEngine;
using System.Collections.Generic;

namespace TXGame
{
    /// <summary>
    /// 角色数据定义 - 所有NPC的核心数据
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterData", menuName = "画皮/角色数据")]
    public class CharacterData : ScriptableObject
    {
        [Header("基本信息")]
        public string characterID;
        public string characterName;
        public string faceName;           // 脸谱名称（如"红脸关公"）
        public FaceType faceType;         // 脸谱类型

        [Header("身份")]
        public string publicIdentity;     // 公开身份（戏班中的角色）
        public string trueIdentity;       // 真实身份（揭露后）

        [Header("性格特征")]
        [TextArea(2, 4)]
        public string personality;

        [Header("完整CG")]
        public Sprite fullBodyCG;         // 完整角色CG图
        public Sprite faceMaskSprite;     // 脸谱面具图

        [Header("隐藏信息")]
        [TextArea(3, 6)]
        public string secret;             // 核心秘密

        [TextArea(3, 6)]
        public string trueStory;          // 真实故事（完全揭露后）

        [Header("关联线索")]
        public List<string> relatedClueIDs;  // 关联的线索ID列表

        public enum FaceType
        {
            Red,      // 红脸 - 忠勇
            Black,    // 黑脸 - 刚正
            White,    // 白脸 - 奸诈
            Blue,     // 蓝脸 - 刚强
            Yellow,   // 黄脸 - 勇猛
            Green,    // 绿脸 - 暴躁
            Gold,     // 金脸 - 神仙
            Silver    // 银脸 - 妖怪
        }

        /// <summary>
        /// 获取脸谱类型的中文描述
        /// </summary>
        public string GetFaceTypeDescription()
        {
            return faceType switch
            {
                FaceType.Red => "红脸 - 忠勇义烈",
                FaceType.Black => "黑脸 - 刚正不阿",
                FaceType.White => "白脸 - 奸诈多疑",
                FaceType.Blue => "蓝脸 - 刚强骁勇",
                FaceType.Yellow => "黄脸 - 勇猛凶悍",
                FaceType.Green => "绿脸 - 暴躁勇猛",
                FaceType.Gold => "金脸 - 神仙精怪",
                FaceType.Silver => "银脸 - 妖魔鬼怪",
                _ => "未知"
            };
        }
    }
}
