using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace TXGame
{
    /// <summary>
    /// 画皮系统 - 核心玩法
    /// 每位角色有一张完整CG，初始被黑雾覆盖
    /// 玩家获得线索后对应区域逐步显现
    /// </summary>
    public class HuapiSystem : MonoBehaviour
    {
        [Header("画皮UI")]
        [SerializeField] private GameObject huapiPanel;
        [SerializeField] private Image characterCGImage;
        [SerializeField] private Image blackFogOverlay;      // 黑雾遮罩（用黑色半透明Image + Mask）
        [SerializeField] private RectTransform fogContainer;  // 黑雾父容器
        [SerializeField] private GameObject fogCellPrefab;    // 单个黑雾格子预制体

        [Header("黑雾设置")]
        [SerializeField] private int gridSizeX = 10;          // 黑雾网格 X
        [SerializeField] private int gridSizeY = 14;          // 黑雾网格 Y

        [Header("角色档案UI")]
        [SerializeField] private Transform characterListContainer;
        [SerializeField] private GameObject characterButtonPrefab;

        // 内部数据
        private Dictionary<string, bool[,]> characterFogMaps;  // 每个角色的黑雾网格
        private Dictionary<string, List<GameObject>> fogCells; // 每个角色的黑雾格子对象
        private string currentViewingCharacterID;
        private CharacterData[] allCharacters;

        private void Start()
        {
            characterFogMaps = new Dictionary<string, bool[,]>();
            fogCells = new Dictionary<string, List<GameObject>>();
            huapiPanel?.SetActive(false);

            LoadAllCharacters();
        }

        private void LoadAllCharacters()
        {
            allCharacters = Resources.LoadAll<CharacterData>("Characters");
            if (allCharacters == null || allCharacters.Length == 0)
            {
                Debug.LogWarning("未找到角色数据！请在 Resources/Characters/ 下放置 CharacterData 文件");
                return;
            }

            foreach (var character in allCharacters)
            {
                InitializeCharacterFog(character.characterID);
            }
        }

        /// <summary>
        /// 初始化角色的黑雾网格
        /// </summary>
        private void InitializeCharacterFog(string characterID)
        {
            bool[,] fogMap = new bool[gridSizeX, gridSizeY];
            // 全部初始化为 true（被黑雾覆盖）
            for (int x = 0; x < gridSizeX; x++)
                for (int y = 0; y < gridSizeY; y++)
                    fogMap[x, y] = true;

            characterFogMaps[characterID] = fogMap;
            fogCells[characterID] = new List<GameObject>();
        }

        /// <summary>
        /// 揭露角色CG的指定区域
        /// </summary>
        /// <param name="characterID">角色ID</param>
        /// <param name="centerX">区域中心X (0~1归一化)</param>
        /// <param name="centerY">区域中心Y (0~1归一化)</param>
        /// <param name="radius">区域半径 (0~1归一化)</param>
        public void RevealArea(string characterID, float centerX, float centerY, float radius)
        {
            if (!characterFogMaps.ContainsKey(characterID))
            {
                Debug.LogWarning($"角色 {characterID} 不存在于黑雾系统中");
                return;
            }

            bool[,] fogMap = characterFogMaps[characterID];
            int centerGridX = Mathf.RoundToInt(centerX * gridSizeX);
            int centerGridY = Mathf.RoundToInt(centerY * gridSizeY);
            int gridRadius = Mathf.RoundToInt(radius * Mathf.Max(gridSizeX, gridSizeY));

            int revealedCount = 0;
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerGridX, centerGridY));
                    if (dist <= gridRadius && fogMap[x, y])
                    {
                        fogMap[x, y] = false;
                        revealedCount++;
                    }
                }
            }

            // 如果当前正在查看该角色，刷新黑雾显示
            if (currentViewingCharacterID == characterID)
            {
                RefreshFogDisplay(characterID);
            }

            Debug.Log($"揭露角色 {characterID} 的区域: 清除了 {revealedCount} 个黑雾格子");
        }

        /// <summary>
        /// 获取角色揭露进度 (0~1)
        /// </summary>
        public float GetRevealProgress(string characterID)
        {
            if (!characterFogMaps.ContainsKey(characterID)) return 0f;

            bool[,] fogMap = characterFogMaps[characterID];
            int total = gridSizeX * gridSizeY;
            int revealed = 0;

            for (int x = 0; x < gridSizeX; x++)
                for (int y = 0; y < gridSizeY; y++)
                    if (!fogMap[x, y]) revealed++;

            return (float)revealed / total;
        }

        /// <summary>
        /// 打开画皮界面查看角色
        /// </summary>
        public void OpenHuapiView(string characterID)
        {
            currentViewingCharacterID = characterID;
            huapiPanel?.SetActive(true);

            // 设置CG图
            CharacterData data = GetCharacterData(characterID);
            if (data != null && characterCGImage != null)
            {
                characterCGImage.sprite = data.fullBodyCG;
            }

            // 生成/刷新黑雾
            RefreshFogDisplay(characterID);
        }

        /// <summary>
        /// 关闭画皮界面
        /// </summary>
        public void CloseHuapiView()
        {
            huapiPanel?.SetActive(false);
            ClearFogCells();
            currentViewingCharacterID = null;
        }

        /// <summary>
        /// 刷新黑雾显示
        /// </summary>
        private void RefreshFogDisplay(string characterID)
        {
            ClearFogCells();

            if (!characterFogMaps.ContainsKey(characterID)) return;
            if (fogContainer == null || fogCellPrefab == null) return;

            bool[,] fogMap = characterFogMaps[characterID];
            Vector2 cellSize = new Vector2(
                fogContainer.rect.width / gridSizeX,
                fogContainer.rect.height / gridSizeY
            );

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    if (fogMap[x, y]) // 还被黑雾覆盖
                    {
                        GameObject cell = Instantiate(fogCellPrefab, fogContainer);
                        RectTransform rt = cell.GetComponent<RectTransform>();
                        rt.sizeDelta = cellSize;
                        rt.anchoredPosition = new Vector2(
                            x * cellSize.x + cellSize.x / 2,
                            -y * cellSize.y - cellSize.y / 2
                        );

                        fogCells[characterID].Add(cell);
                    }
                }
            }
        }

        private void ClearFogCells()
        {
            if (currentViewingCharacterID == null) return;
            if (!fogCells.ContainsKey(currentViewingCharacterID)) return;

            foreach (var cell in fogCells[currentViewingCharacterID])
            {
                if (cell != null) Destroy(cell);
            }
            fogCells[currentViewingCharacterID].Clear();
        }

        private CharacterData GetCharacterData(string id)
        {
            if (allCharacters == null) return null;
            foreach (var c in allCharacters)
            {
                if (c.characterID == id) return c;
            }
            return null;
        }

        /// <summary>
        /// 获取所有角色ID列表
        /// </summary>
        public string[] GetAllCharacterIDs()
        {
            if (allCharacters == null) return new string[0];
            string[] ids = new string[allCharacters.Length];
            for (int i = 0; i < allCharacters.Length; i++)
                ids[i] = allCharacters[i].characterID;
            return ids;
        }

        /// <summary>
        /// 获取角色数据
        /// </summary>
        public CharacterData GetCharacter(string id)
        {
            return GetCharacterData(id);
        }
    }
}
