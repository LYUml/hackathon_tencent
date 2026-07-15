using UnityEngine;

namespace TXGame
{
    /// <summary>
    /// 观皮揭露节点数据：ScriptableObject，驱动观皮匹配判定和黑雾揭露进度
    /// </summary>
    [CreateAssetMenu(fileName = "ObserveSkinRevealData", menuName = "画皮/观皮揭露数据")]
    public class ObserveSkinRevealData : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("节点唯一ID")]
        public string nodeId;

        [Tooltip("关联角色ID")]
        public string relatedCharacterId;

        [Tooltip("所需线索ID")]
        public string requiredClueId;

        [Header("揭露区域")]
        [Tooltip("在隐藏画像上的归一化区域（x, y, width, height 均为 0-1）")]
        public Rect revealArea = new Rect(0.3f, 0.2f, 0.4f, 0.3f);

        [Header("揭露内容")]
        [Tooltip("揭露后显示的文本")]
        [TextArea(2, 4)]
        public string revealedText;

        [Tooltip("是否触发新目标")]
        public bool triggersNewObjective = false;

        [Tooltip("新目标ID")]
        public string newObjectiveId;

        [Header("动效")]
        [Tooltip("黑雾消散持续时间（秒）")]
        public float fogFadeDuration = 0.8f;
    }
}
