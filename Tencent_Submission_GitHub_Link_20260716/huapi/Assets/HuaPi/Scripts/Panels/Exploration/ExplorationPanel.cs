using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using HuaPi.UI.Core;
using HuaPi.UI.Data;

namespace HuaPi.UI.Panels.Exploration
{
    /// <summary>
    /// 场景探索页主面板：管理场景背景、热点、人物、目标提示、线索获取。
    /// 继承 PanelBase，注册到 UIManager，支持数据驱动配置。
    /// </summary>
    public class ExplorationPanel : PanelBase
    {
        public static ExplorationPanel Instance { get; private set; }

        [Header("Core Modules")]
        [SerializeField] private SceneBackgroundView backgroundView;
        [SerializeField] private Transform hotspotContainer;
        [SerializeField] private Transform characterContainer;
        [SerializeField] private ObjectiveNoteView objectiveView;
        [SerializeField] private ClueToastView clueToastView;

        [Header("Prefabs")]
        [SerializeField] private GameObject hotspotItemPrefab;
        [SerializeField] private GameObject characterEntryPrefab;

        [Header("Font")]
        [Tooltip("指定字体Asset（支持中文/繁体）。如果留空，则自动搜索项目中可用的中文字体。")]
        [SerializeField] private TMP_FontAsset fontAsset;

        [Header("Demo Data")]
        [SerializeField] private bool useDemoData = true;
        [SerializeField] private ExplorationSceneData demoSceneData;
        [SerializeField] private Sprite demoBackgroundSprite;
        [SerializeField] private Sprite demoCharacterPortrait;

        [Header("Settings")]
        [SerializeField] private bool pauseOnEscape = true;

        // 当前场景数据
        private ExplorationSceneData _currentSceneData;
        private readonly List<SceneHotspotItem> _activeHotspots = new List<SceneHotspotItem>();
        private readonly List<SceneCharacterEntry> _activeCharacters = new List<SceneCharacterEntry>();
        private readonly HashSet<string> _acquiredClueIds = new HashSet<string>();
        private readonly Dictionary<string, ClueData> _acquiredCluesById = new Dictionary<string, ClueData>();

        #region Lifecycle

        protected override void Awake()
        {
            base.Awake();
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[ExplorationPanel] Multiple instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 确保容器铺满全屏
            EnsureContainerLayout();
        }

        protected void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        protected void Start()
        {
            // 自动搜索并应用字体
            ResolveFont();
            ApplyFontToAllTexts();

            if (useDemoData)
            {
                LoadDemoData();
            }
        }

        /// <summary>
        /// 确保热点和人物容器铺满全屏，子对象使用正确的坐标系
        /// </summary>
        private void EnsureContainerLayout()
        {
            if (hotspotContainer != null)
            {
                var rect = hotspotContainer.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.anchoredPosition = Vector2.zero;
                    rect.pivot = new Vector2(0.5f, 0.5f);
                }
            }

            if (characterContainer != null)
            {
                var rect = characterContainer.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.anchoredPosition = Vector2.zero;
                    rect.pivot = new Vector2(0.5f, 0.5f);
                }
            }
        }

        #endregion

        #region Data Loading

        /// <summary>
        /// 加载场景探索数据（主入口）
        /// </summary>
        public void LoadExplorationScene(ExplorationSceneData sceneData)
        {
            _currentSceneData = sceneData;
            ClearScene();

            if (sceneData == null)
            {
                Debug.LogWarning("[ExplorationPanel] Scene data is null.");
                return;
            }

            // 设置背景
            if (backgroundView != null)
            {
                if (sceneData.backgroundSprite != null)
                {
                    backgroundView.SetBackground(sceneData.backgroundSprite);
                }
                else
                {
                    backgroundView.ShowImmediately();
                }
            }

            // 设置目标
            if (objectiveView != null)
            {
                objectiveView.SetObjective(sceneData.displayName, sceneData.objectiveText);
            }

            // 创建热点
            if (sceneData.hotspots != null)
            {
                foreach (var hotspotData in sceneData.hotspots)
                {
                    CreateHotspot(hotspotData);
                }
            }

            // 创建人物入口
            if (sceneData.characters != null)
            {
                foreach (var characterData in sceneData.characters)
                {
                    CreateCharacterEntry(characterData);
                }
            }

            // 场景加载完成后，再次应用字体（确保动态创建的文本也有正确字体）
            ApplyFontToAllTexts();
        }

        /// <summary>
        /// 加载 Demo 数据：后台化妆间
        /// </summary>
        private void LoadDemoData()
        {
            if (demoSceneData != null)
            {
                LoadExplorationScene(demoSceneData);
                return;
            }

            var sceneData = ScriptableObject.CreateInstance<ExplorationSceneData>();
            sceneData.sceneId = "demo_backstage";
            sceneData.displayName = "后台化妆间";
            sceneData.objectiveText = "调查镜台附近的异常，并试着与旦角交谈。";
            sceneData.backgroundSprite = demoBackgroundSprite;

            // 配置热点：镜台（绝对像素坐标，左下角为原点）
            sceneData.hotspots = new SceneHotspotData[]
            {
                new SceneHotspotData
                {
                    hotspotId = "hotspot_mirror_table",
                    displayName = "镜台",
                    anchoredPosition = new Vector2(1200, 500), // 屏幕中下方偏右
                    size = new Vector2(150, 150),
                    linkedClueId = "clue_old_ticket_under_mirror",
                    hoverText = "镜台",
                    clickFeedbackText = "你在镜台下发现了一张旧戏票。"
                }
            };

            // 配置人物：旦角（绝对像素坐标，左下角为原点）
            sceneData.characters = new SceneCharacterEntryData[]
            {
                new SceneCharacterEntryData
                {
                    characterId = "char_dan",
                    displayName = "旦角",
                    portraitSprite = demoCharacterPortrait,
                    anchoredPosition = new Vector2(280, 540), // 左侧，y=540为屏幕垂直中心
                    size = new Vector2(380, 680),
                    dialogueId = "dialogue_dan_backstage",
                    hoverText = "旦角"
                }
            };

            LoadExplorationScene(sceneData);
        }

        private void ClearScene()
        {
            // 清除热点
            foreach (var hotspot in _activeHotspots)
            {
                if (hotspot != null) Destroy(hotspot.gameObject);
            }
            _activeHotspots.Clear();

            // 清除人物
            foreach (var character in _activeCharacters)
            {
                if (character != null) Destroy(character.gameObject);
            }
            _activeCharacters.Clear();
        }

        private void CreateHotspot(SceneHotspotData data)
        {
            if (hotspotItemPrefab == null || hotspotContainer == null) return;

            GameObject go = Instantiate(hotspotItemPrefab, hotspotContainer);
            var hotspot = go.GetComponent<SceneHotspotItem>();
            if (hotspot == null)
            {
                hotspot = go.AddComponent<SceneHotspotItem>();
            }

            hotspot.ApplyData(data);
            _activeHotspots.Add(hotspot);

            // 对新创建的热点应用字体
            ApplyFontToGameObject(go);
        }

        private void CreateCharacterEntry(SceneCharacterEntryData data)
        {
            if (characterEntryPrefab == null || characterContainer == null) return;

            GameObject go = Instantiate(characterEntryPrefab, characterContainer);
            var entry = go.GetComponent<SceneCharacterEntry>();
            if (entry == null)
            {
                entry = go.AddComponent<SceneCharacterEntry>();
            }

            entry.ApplyData(data);
            _activeCharacters.Add(entry);

            // 对新创建的人物入口应用字体
            ApplyFontToGameObject(go);
        }

        #endregion

        #region Interaction Handlers

        /// <summary>
        /// 热点被点击时调用（由 SceneHotspotItem 调用）
        /// </summary>
        public void OnHotspotClicked(SceneHotspotItem hotspot)
        {
            if (hotspot == null || string.IsNullOrEmpty(hotspot.LinkedClueId)) return;

            // 标记为已调查
            hotspot.MarkAsDiscovered();

            // 获得线索
            AcquireClue(hotspot.LinkedClueId);

            Debug.Log($"[ExplorationPanel] Hotspot clicked: {hotspot.HotspotId}, clue: {hotspot.LinkedClueId}");
        }

        /// <summary>
        /// 人物被点击时调用（由 SceneCharacterEntry 调用）
        /// </summary>
        public void OnCharacterClicked(SceneCharacterEntry character)
        {
            if (character == null) return;

            Debug.Log($"[ExplorationPanel] Character clicked: {character.CharacterId}");
            OpenDialogue(character.CharacterId, character.DialogueId);
        }

        #endregion

        #region Clue System

        /// <summary>
        /// 获得线索（可复用接口）
        /// </summary>
        public void AcquireClue(string clueId)
        {
            if (string.IsNullOrEmpty(clueId)) return;
            if (_acquiredClueIds.Contains(clueId)) return;

            _acquiredClueIds.Add(clueId);

            // 查找线索数据（简化版：实际应从 ClueManager 或 DataManager 获取）
            ClueData clueData = FindClueData(clueId);
            if (clueData != null)
            {
                clueData.isAcquired = true;
                _acquiredCluesById[clueId] = clueData;
            }

            if (clueData != null && clueToastView != null)
            {
                clueToastView.ShowClueToast(clueData);
            }

            Debug.Log($"[ExplorationPanel] Clue acquired: {clueId}");
        }

        /// <summary>
        /// 检查是否已获得某线索
        /// </summary>
        public bool HasAcquiredClue(string clueId)
        {
            return _acquiredClueIds.Contains(clueId);
        }

        /// <summary>
        /// 查找线索数据（简化实现，实际应从 ClueManager 获取）
        /// </summary>
        private ClueData FindClueData(string clueId)
        {
            // 尝试从 Resources 加载
            var clue = Resources.Load<ClueData>($"HuaPi/Data/Clues/{clueId}");
            if (clue != null) return clue;

            // 创建 Demo 线索数据（如果找不到）
            if (clueId == "clue_old_ticket_under_mirror")
            {
                var demoClue = ScriptableObject.CreateInstance<ClueData>();
                demoClue.clueId = clueId;
                demoClue.clueName = "镜台下的旧戏票";
                demoClue.sourceLocation = "后台化妆间";
                demoClue.sourceCharacter = "旦";
                demoClue.relatedCharacterId = "char_dan";
                demoClue.description = "一张被灰尘和胭脂沾污的旧戏票，角落残留着“薛家戏班”的字样。票根边缘有轻微烧痕。";
                demoClue.isAcquired = true;
                return demoClue;
            }

            return null;
        }

        #endregion

        #region Navigation Interface (预留接口)

        /// <summary>
        /// 打开对话页（预留接口）
        /// </summary>
        public void OpenDialogue(string characterId, string dialogueId = null)
        {
            Debug.Log($"[ExplorationPanel] Opening dialogue for character: {characterId}, dialogue: {dialogueId}");

            // 第一阶段只预留接口：尝试打开对话页，但保留探索页，避免未完成对话模块打断 Demo 链路。
            if (UIManager.Instance == null)
            {
                return;
            }

            UIManager.Instance.OpenPanel(UIPanelType.Dialogue, new DialogueInitData
            {
                characterId = characterId,
                dialogueId = dialogueId
            });
        }

        /// <summary>
        /// 打开线索页（预留接口）
        /// </summary>
        public void OpenCluePanel()
        {
            Debug.Log("[ExplorationPanel] Opening clue panel.");
            var acquiredClues = new List<ClueData>(_acquiredCluesById.Values).ToArray();
            UIManager.Instance?.OpenPanel(UIPanelType.ClueInventory, acquiredClues);
        }

        /// <summary>
        /// 打开暂停/设置页（预留接口）
        /// </summary>
        public void OpenPausePanel()
        {
            Debug.Log("[ExplorationPanel] Opening pause panel.");
            UIManager.Instance?.OpenPanel(UIPanelType.Pause);
        }

        /// <summary>
        /// 切换场景（预留接口）
        /// </summary>
        public void SwitchScene(string sceneId)
        {
            Debug.Log($"[ExplorationPanel] Switching to scene: {sceneId}");
            // 实际实现中应从 SceneDataManager 加载对应场景数据
            // LoadExplorationScene(SceneDataManager.GetSceneData(sceneId));
        }

        #endregion

        #region PanelBase Overrides

        public override void Init(object data)
        {
            base.Init(data);

            // 自动搜索并应用字体（确保传入数据前字体已就绪）
            ResolveFont();
            ApplyFontToAllTexts();

            // 如果传入数据是 ExplorationSceneData，直接加载
            if (data is ExplorationSceneData sceneData)
            {
                LoadExplorationScene(sceneData);
            }
        }

        public override void Show()
        {
            base.Show();
            // 探索面板打开时恢复 gameplay 输入
            // 由 UIManager 的 OnPanelOpened 处理
        }

        public override void Hide()
        {
            base.Hide();
        }

        #endregion

        #region Font Management

        /// <summary>
        /// 自动搜索并设置支持中文/繁体的字体。
        /// 探索页正文优先使用可读中文字体；hakidame 只适合标题和短恐怖提示，不强套到长文本。
        /// </summary>
        private void ResolveFont()
        {
            if (fontAsset != null && IsReadableBodyFont(fontAsset)) return;

            var allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();

            // 1. 搜索项目内已存在的可读中文字体（排除 hakidame）。
            foreach (var font in allFonts)
            {
                if (font == null) continue;
                if (!IsReadableBodyFont(font)) continue;

                fontAsset = font;
                Debug.Log($"[ExplorationPanel] 自动找到正文字体: {font.name}");
                return;
            }

            // 2. 本地 Demo 兜底：从系统中文字体动态创建 TMP FontAsset。
            fontAsset = CreateRuntimeChineseFontAsset();
            if (fontAsset != null)
            {
                Debug.Log($"[ExplorationPanel] 已创建运行时中文正文字体: {fontAsset.name}");
                return;
            }

            // 3. 最后才退回到名称疑似中文的字体，避免完全没有字体可用。
            foreach (var font in allFonts)
            {
                if (font == null) continue;
                string name = font.name.ToLowerInvariant();
                if (name.Contains("chinese") || name.Contains("cjk") || name.Contains("noto") ||
                    name.Contains("source") || name.Contains("simhei") || name.Contains("simsun") ||
                    name.Contains("heiti") || name.Contains("songti") || name.Contains("yahei") ||
                    name.Contains("kozuka") || name.Contains("fontcn") ||
                    name.Contains("cn") || name.Contains("zh") || name.Contains("fang") ||
                    name.Contains("ming") || name.Contains("kai"))
                {
                    fontAsset = font;
                    Debug.Log($"[ExplorationPanel] 自动找到字体: {font.name}");
                    return;
                }
            }

            // 4. 如果未找到，尝试使用字符数较多的非 hakidame 字体（可能包含中文）。
            foreach (var font in allFonts)
            {
                if (font == null) continue;
                if (IsHakidameFont(font)) continue;
                if (font.characterTable != null && font.characterTable.Count > 1000)
                {
                    fontAsset = font;
                    Debug.Log($"[ExplorationPanel] 使用字符数较多的字体: {font.name} (chars={font.characterTable.Count})");
                    return;
                }
            }

            if (fontAsset == null)
            {
                Debug.LogWarning("[ExplorationPanel] 未找到支持中文的正文字体。请在 Inspector 的 fontAsset 字段中指定思源黑体/Noto Sans SC 等可读中文字体。");
            }
        }

        private static bool IsReadableBodyFont(TMP_FontAsset font)
        {
            if (font == null || IsHakidameFont(font)) return false;
            string name = font.name.ToLowerInvariant();
            bool nameLooksCjk = name.Contains("chinese") || name.Contains("cjk") || name.Contains("noto") ||
                                name.Contains("source") || name.Contains("simhei") || name.Contains("simsun") ||
                                name.Contains("heiti") || name.Contains("songti") || name.Contains("yahei") ||
                                name.Contains("pingfang") || name.Contains("arial unicode") ||
                                name.Contains("cn") || name.Contains("zh");
            return nameLooksCjk || font.HasCharacter('戏') || font.HasCharacter('镜') || font.HasCharacter('线');
        }

        private static bool IsHakidameFont(TMP_FontAsset font)
        {
            return font != null && font.name.ToLowerInvariant().Contains("hakidame");
        }

        private static TMP_FontAsset CreateRuntimeChineseFontAsset()
        {
            string[] osFontNames =
            {
                "PingFang SC",
                "Heiti SC",
                "STHeiti",
                "Songti SC",
                "Arial Unicode MS"
            };

            foreach (string osFontName in osFontNames)
            {
                Font font = Font.CreateDynamicFontFromOSFont(osFontName, 32);
                if (font == null) continue;

                TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(
                    font,
                    64,
                    9,
                    GlyphRenderMode.SDFAA,
                    2048,
                    2048,
                    AtlasPopulationMode.Dynamic,
                    true);

                if (asset == null) continue;

                asset.name = $"Runtime {osFontName} TMP";
                return asset;
            }

            return null;
        }

        /// <summary>
        /// 将字体应用到所有子组件的 TMP_Text（包括动态创建的）
        /// </summary>
        private void ApplyFontToAllTexts()
        {
            if (fontAsset == null) return;

            var allTexts = GetComponentsInChildren<TMP_Text>(true);
            int appliedCount = 0;

            foreach (var text in allTexts)
            {
                if (text == null) continue;
                if (text.font == fontAsset) continue;

                text.font = fontAsset;
                appliedCount++;
            }

            if (appliedCount > 0)
            {
                Debug.Log($"[ExplorationPanel] 已为 {appliedCount} 个文本组件应用字体: {fontAsset.name}");
            }
        }

        /// <summary>
        /// 将字体应用到指定 GameObject 及其子对象的所有 TMP_Text
        /// </summary>
        private void ApplyFontToGameObject(GameObject go)
        {
            if (fontAsset == null || go == null) return;

            var texts = go.GetComponentsInChildren<TMP_Text>(true);
            foreach (var text in texts)
            {
                if (text != null && text.font != fontAsset)
                {
                    text.font = fontAsset;
                }
            }
        }

        #endregion

        #region Utility

        private void Update()
        {
            // 监听 Escape 打开暂停菜单
            if (pauseOnEscape && IsEscapePressed() && IsVisible)
            {
                OpenPausePanel();
            }
        }

        private static bool IsEscapePressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                return UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }

        #endregion
    }

    /// <summary>
    /// 对话初始化数据（用于 OpenDialogue 传参）
    /// </summary>
    public class DialogueInitData
    {
        public string characterId;
        public string dialogueId;
    }
}
