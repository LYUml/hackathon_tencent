using UnityEngine;

namespace TXGame
{
    /// <summary>
    /// 线索数据（UI用）：ScriptableObject，驱动线索列表和详情显示
    /// 对应 hackathon 仓库的 HuaPi.UI.Data.ClueData
    /// </summary>
    [CreateAssetMenu(fileName = "HuapiClueData", menuName = "画皮/UI线索数据")]
    public class HuapiClueData : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("线索唯一ID")]
        public string clueId;

        [Tooltip("线索名称")]
        public string clueName;

        [Tooltip("线索描述")]
        [TextArea(3, 6)]
        public string description;

        [Header("来源")]
        [Tooltip("来源地点")]
        public string sourceLocation;

        [Tooltip("来源人物")]
        public string sourceCharacter;

        [Tooltip("关联角色ID")]
        public string relatedCharacterId;

        [Header("状态")]
        [Tooltip("是否已获得")]
        public bool isAcquired = false;

        [Tooltip("是否已用于观皮")]
        public bool isUsedInGuanPi = false;

        [Header("图标")]
        [Tooltip("线索图标（2D手绘风格）")]
        public Sprite clueIcon;
    }
}
