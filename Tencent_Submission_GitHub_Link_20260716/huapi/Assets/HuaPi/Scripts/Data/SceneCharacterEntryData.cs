using UnityEngine;

namespace HuaPi.UI.Data
{
    /// <summary>
    /// 场景人物入口数据（纯数据结构，用于配置）
    /// </summary>
    [System.Serializable]
    public class SceneCharacterEntryData
    {
        [Tooltip("角色唯一ID")]
        public string characterId;

        [Tooltip("角色显示名称（如：旦角）")]
        public string displayName;

        [Tooltip("角色立绘Sprite")]
        public Sprite portraitSprite;

        [Tooltip("在场景中的锚点位置（相对于1920x1080）")]
        public Vector2 anchoredPosition = new Vector2(300, 540);

        [Tooltip("立绘显示大小（宽x高）")]
        public Vector2 size = new Vector2(400, 700);

        [Tooltip("关联的对话ID")]
        public string dialogueId;

        [Tooltip("角色状态（如：正常、恐惧、死亡）")]
        public string characterState = "正常";

        [Tooltip("Hover时显示的提示文本")]
        public string hoverText;
    }
}
