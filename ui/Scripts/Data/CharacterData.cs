using UnityEngine;

namespace HuaPi.UI.Data
{
    /// <summary>
    /// 角色档案数据：ScriptableObject，驱动角色档案显示
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterData", menuName = "HuaPi/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("角色唯一ID")]
        public string characterId;

        [Tooltip("角色名称")]
        public string characterName;

        [Tooltip("角色行当（青衣/小生/文丑/净行）")]
        public string roleType;

        [Tooltip("表面身份")]
        public string surfaceIdentity;

        [Header("视觉")]
        [Tooltip("脸谱图标")]
        public Sprite faceMaskIcon;

        [Tooltip("2D立绘")]
        public Sprite portraitSprite;

        [Tooltip("隐藏画像（被黑雾覆盖）")]
        public Sprite hiddenPortrait;

        [Header("信息")]
        [Tooltip("已知信息")]
        [TextArea(2, 4)]
        public string knownInfo;

        [Tooltip("可疑点")]
        [TextArea(2, 4)]
        public string suspiciousPoints;

        [Header("揭露进度")]
        [Tooltip("揭露进度（0-1）")]
        [Range(0f, 1f)]
        public float revealProgress = 0f;

        [Tooltip("角色识别色")]
        public Color roleColor = Color.white;
    }
}
