using UnityEngine;

namespace HuaPi.UI.Data
{
    /// <summary>
    /// 场景探索数据：ScriptableObject，用于配置一个完整的探索场景。
    /// </summary>
    [CreateAssetMenu(fileName = "ExplorationSceneData", menuName = "HuaPi/ExplorationSceneData")]
    public class ExplorationSceneData : ScriptableObject
    {
        [Tooltip("场景唯一ID")]
        public string sceneId;

        [Tooltip("场景显示名称（如：后台化妆间）")]
        public string displayName;

        [Tooltip("场景背景 Sprite")]
        public Sprite backgroundSprite;

        [Tooltip("当前目标文本")]
        [TextArea(2, 3)]
        public string objectiveText;

        [Tooltip("场景热点列表")]
        public SceneHotspotData[] hotspots;

        [Tooltip("场景中的人物入口列表")]
        public SceneCharacterEntryData[] characters;
    }
}
