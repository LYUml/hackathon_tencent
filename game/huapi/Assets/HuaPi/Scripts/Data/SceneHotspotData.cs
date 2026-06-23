using UnityEngine;

namespace HuaPi.UI.Data
{
    /// <summary>
    /// 场景热点数据（纯数据结构，用于配置）
    /// </summary>
    [System.Serializable]
    public class SceneHotspotData
    {
        [Tooltip("热点唯一ID")]
        public string hotspotId;

        [Tooltip("热点显示名称（如：镜台）")]
        public string displayName;

        [Tooltip("在场景中的锚点位置（相对于1920x1080）")]
        public Vector2 anchoredPosition = new Vector2(960, 540);

        [Tooltip("热点点击区域大小（宽x高）")]
        public Vector2 size = new Vector2(120, 120);

        [Tooltip("关联的线索ID（点击后获得）")]
        public string linkedClueId;

        [Tooltip("Hover时显示的文本")]
        public string hoverText;

        [Tooltip("点击后的反馈文本（用于提示）")]
        public string clickFeedbackText;

        [Tooltip("是否需要先满足某条件才能点击")]
        public bool requiresCondition = false;

        [Tooltip("需要的条件线索ID列表")]
        public string[] requiredClueIds;
    }
}
