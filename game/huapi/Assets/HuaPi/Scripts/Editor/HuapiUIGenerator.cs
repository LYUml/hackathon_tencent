using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.TextCore.LowLevel;
using HuaPi.UI.Core;
using HuaPi.UI.Panels;
using HuaPi.UI.Panels.Exploration;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace HuaPi.UI.Editor
{
    /// <summary>
    /// 编辑器工具：一键生成《画皮》UI 场景结构
    /// 菜单路径：Tools / HuaPi / Generate UI
    /// </summary>
    public class HuapiUIGenerator : UnityEditor.EditorWindow
    {
        [MenuItem("Tools/HuaPi/Generate UI")]
        private static void ShowWindow()
        {
            GetWindow<HuapiUIGenerator>("HuaPi UI Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("《画皮》UI 生成器", UnityEditor.EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("此工具将自动生成以下结构：", UnityEditor.EditorStyles.wordWrappedLabel);
            GUILayout.Label("- Canvas 分层（Background/HUD/Dialogue/Panel/Popup/Reveal/System）");
            GUILayout.Label("- UIManager 单例");
            GUILayout.Label("- 各面板基础结构（TMP_Text + Image + Button）");
            GUILayout.Label("- 基础数据 ScriptableObject 文件夹");

            GUILayout.Space(10);

            if (GUILayout.Button("生成 UI 场景", GUILayout.Height(40)))
            {
                GenerateUIScene();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("仅生成 Canvas 层级", GUILayout.Height(30)))
            {
                GenerateCanvasLayers();
            }

            GUILayout.Space(10);

            UnityEditor.EditorGUILayout.HelpBox(
                "注意：生成前请确保已导入 TextMeshPro Essentials（Window > TextMeshPro > Import TMP Essential Resources）",
                UnityEditor.MessageType.Info);
        }

        private static void GenerateUIScene()
        {
            // 创建根物体
            GameObject existing = GameObject.Find("UI_Root");
            GameObject uiRoot = existing != null ? existing : new GameObject("UI_Root");
            if (existing == null)
            {
                Undo.RegisterCreatedObjectUndo(uiRoot, "Create UI Root");
            }

            // 生成 Canvas 层级
            GenerateCanvasLayers(uiRoot);

            // 生成 UIManager
            UIManager uiManager = GenerateUIManager(uiRoot);

            // 创建数据文件夹
            CreateDataFolders();

            // 创建并绑定可见的主菜单 Prefab
            MainMenuPanel mainMenuPanel = CreateMainMenuPrefab();
            AssignMainMenuPrefab(uiManager, mainMenuPanel);

            // 创建子模块 Prefab
            CreateExplorationSubPrefabs();

            // 创建探索系统 Prefab
            ExplorationPanel explorationPanel = CreateExplorationPanelPrefab();
            AssignExplorationPrefab(uiManager, explorationPanel);

            UnityEditor.EditorUtility.DisplayDialog("生成完成", "UI 场景结构已生成！请检查 Hierarchy 面板。\n\n新增内容：\n- ExplorationPanel（场景探索）\n- SceneBackgroundView\n- SceneHotspotItem\n- SceneCharacterEntry\n- ObjectiveNoteView\n- ClueToastView", "确定");
        }

        private static void GenerateCanvasLayers()
        {
            GameObject existing = GameObject.Find("UI_Root");
            GameObject uiRoot = existing != null ? existing : new GameObject("UI_Root");
            if (existing == null) Undo.RegisterCreatedObjectUndo(uiRoot, "Create UI Root");

            GenerateCanvasLayers(uiRoot);
        }

        private static void GenerateCanvasLayers(GameObject uiRoot)
        {
            // BackgroundCanvas (Sort Order 0)
            CreateCanvas(uiRoot, "BackgroundCanvas", 0);

            // WorldHUDCanvas (Sort Order 10)
            CreateCanvas(uiRoot, "WorldHUDCanvas", 10);

            // DialogueCanvas (Sort Order 20)
            CreateCanvas(uiRoot, "DialogueCanvas", 20);

            // NormalPanelCanvas (Sort Order 30)
            CreateCanvas(uiRoot, "NormalPanelCanvas", 30);

            // PopupCanvas (Sort Order 40)
            CreateCanvas(uiRoot, "PopupCanvas", 40);

            // RevealCanvas (Sort Order 50)
            CreateCanvas(uiRoot, "RevealCanvas", 50);

            // SystemCanvas (Sort Order 60)
            CreateCanvas(uiRoot, "SystemCanvas", 60);

            // EventSystem
            if (UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
                eventSystem.AddComponent<InputSystemUIInputModule>();
#else
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
                eventSystem.transform.SetParent(uiRoot.transform);
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            }
        }

        private static GameObject CreateCanvas(GameObject parent, string name, int sortOrder)
        {
            Transform existing = parent.transform.Find(name);
            GameObject canvasGO = existing != null ? existing.gameObject : new GameObject(name);
            canvasGO.transform.SetParent(parent.transform);
            if (existing == null)
            {
                Undo.RegisterCreatedObjectUndo(canvasGO, $"Create {name}");
            }

            Canvas canvas = canvasGO.GetComponent<Canvas>();
            if (canvas == null) canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (canvasGO.GetComponent<GraphicRaycaster>() == null)
            {
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // 创建 CanvasGroup（用于控制整层透明度）
            CanvasGroup canvasGroup = canvasGO.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = canvasGO.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            return canvasGO;
        }

        private static UIManager GenerateUIManager(GameObject uiRoot)
        {
            Transform existing = uiRoot.transform.Find("UIManager");
            GameObject uiManagerGO = existing != null ? existing.gameObject : new GameObject("UIManager");
            uiManagerGO.transform.SetParent(uiRoot.transform);
            if (existing == null)
            {
                Undo.RegisterCreatedObjectUndo(uiManagerGO, "Create UIManager");
            }

            var uiManager = uiManagerGO.GetComponent<UIManager>();
            if (uiManager == null) uiManager = uiManagerGO.AddComponent<UIManager>();

            // 通过反射设置字段（因为字段是 private）
            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(uiManager);
            so.FindProperty("backgroundCanvas").objectReferenceValue = FindCanvas("BackgroundCanvas");
            so.FindProperty("worldHUDCanvas").objectReferenceValue = FindCanvas("WorldHUDCanvas");
            so.FindProperty("dialogueCanvas").objectReferenceValue = FindCanvas("DialogueCanvas");
            so.FindProperty("normalPanelCanvas").objectReferenceValue = FindCanvas("NormalPanelCanvas");
            so.FindProperty("popupCanvas").objectReferenceValue = FindCanvas("PopupCanvas");
            so.FindProperty("revealCanvas").objectReferenceValue = FindCanvas("RevealCanvas");
            so.FindProperty("systemCanvas").objectReferenceValue = FindCanvas("SystemCanvas");
            so.ApplyModifiedProperties();

            UnityEditor.EditorUtility.SetDirty(uiManager);
            return uiManager;
        }

        private static Canvas FindCanvas(string name)
        {
            GameObject go = GameObject.Find(name);
            return go != null ? go.GetComponent<Canvas>() : null;
        }

        private static void CreateDataFolders()
        {
            EnsureFolder("Assets", "HuaPi");
            EnsureFolder("Assets/HuaPi", "Data");
            EnsureFolder("Assets/HuaPi", "Prefabs");
            EnsureFolder("Assets/HuaPi", "Scenes");
        }

        private static void EnsureFolder(string parentPath, string folderName)
        {
            string fullPath = $"{parentPath}/{folderName}";
            if (!UnityEditor.AssetDatabase.IsValidFolder(fullPath))
            {
                UnityEditor.AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        private static MainMenuPanel CreateMainMenuPrefab()
        {
            string prefabPath = "Assets/HuaPi/Prefabs/Panels/MainMenuPanel.prefab";
            TMP_FontAsset fontAsset = CreateOrLoadHuapiFontAsset();
            GameObject existingPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                MainMenuPanel existingPanel = existingPrefab.GetComponent<MainMenuPanel>();
                ApplyFontToPrefab(existingPrefab, fontAsset);
                ApplyTraditionalMainMenuText(existingPrefab);
                UnityEditor.AssetDatabase.SaveAssets();
                return existingPanel;
            }

            GameObject panel = new GameObject("MainMenuPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(MainMenuPanel));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            Stretch(panelRect);

            Image background = CreateImage(panel.transform, "Background", new Color(0.035f, 0.028f, 0.025f, 1f));
            Stretch(background.rectTransform);

            TMP_Text title = CreateText(panel.transform, "Title", "画皮", 156, TextAlignmentOptions.Center);
            title.font = fontAsset;
            title.color = new Color(0.545f, 0.125f, 0.125f, 0.94f);
            title.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            title.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            title.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            title.rectTransform.anchoredPosition = new Vector2(0f, 190f);
            title.rectTransform.sizeDelta = new Vector2(620f, 190f);

            TMP_Text subtitle = CreateText(panel.transform, "Subtitle", "MASKS BEHIND MASKS", 28, TextAlignmentOptions.Center);
            subtitle.font = fontAsset;
            subtitle.color = new Color(0.788f, 0.541f, 0.431f, 0.68f);
            subtitle.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            subtitle.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            subtitle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            subtitle.rectTransform.anchoredPosition = new Vector2(0f, 92f);
            subtitle.rectTransform.sizeDelta = new Vector2(560f, 56f);

            GameObject menu = new GameObject("Menu", typeof(RectTransform), typeof(VerticalLayoutGroup));
            menu.transform.SetParent(panel.transform, false);
            RectTransform menuRect = menu.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(0.5f, 0.5f);
            menuRect.anchorMax = new Vector2(0.5f, 0.5f);
            menuRect.pivot = new Vector2(0.5f, 0.5f);
            menuRect.anchoredPosition = new Vector2(0f, -80f);
            menuRect.sizeDelta = new Vector2(320f, 260f);

            VerticalLayoutGroup layout = menu.GetComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = 18f;

            CreateMenuText(menu.transform, "StartGame", "開始遊戲");
            CreateMenuText(menu.transform, "ContinueGame", "繼續遊戲");
            CreateMenuText(menu.transform, "Settings", "設定");
            CreateMenuText(menu.transform, "Quit", "退出");

            TMP_Text bottom = CreateText(panel.transform, "BottomInfo", "薛家戲班 · 民國二十年", 24, TextAlignmentOptions.Center);
            bottom.font = fontAsset;
            bottom.color = new Color(0.788f, 0.541f, 0.431f, 0.45f);
            bottom.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            bottom.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            bottom.rectTransform.pivot = new Vector2(0.5f, 0f);
            bottom.rectTransform.anchoredPosition = new Vector2(0f, 56f);
            bottom.rectTransform.sizeDelta = new Vector2(620f, 48f);

            ApplyFontToPrefab(panel, fontAsset);
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(panel, prefabPath);
            UnityEngine.Object.DestroyImmediate(panel);
            UnityEditor.AssetDatabase.SaveAssets();
            return savedPrefab.GetComponent<MainMenuPanel>();
        }

        private static void AssignMainMenuPrefab(UIManager uiManager, MainMenuPanel mainMenuPanel)
        {
            if (uiManager == null || mainMenuPanel == null) return;

            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(uiManager);
            so.FindProperty("mainMenuPanelPrefab").objectReferenceValue = mainMenuPanel;
            so.FindProperty("openMainMenuOnStart").boolValue = true;
            so.ApplyModifiedProperties();
            UnityEditor.EditorUtility.SetDirty(uiManager);
        }

        #region Exploration System Generation

        /// <summary>
        /// 创建探索面板主 Prefab（场景探索系统）
        /// </summary>
        private static ExplorationPanel CreateExplorationPanelPrefab()
        {
            string prefabPath = "Assets/HuaPi/Prefabs/Panels/ExplorationPanel.prefab";
            GameObject existingPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                return existingPrefab.GetComponent<ExplorationPanel>();
            }

            TMP_FontAsset fontAsset = CreateOrLoadHuapiFontAsset();

            // --- Root ---
            GameObject panel = new GameObject("ExplorationPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(ExplorationPanel));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            Stretch(panelRect);

            var canvasGroup = panel.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;

            var panelComp = panel.GetComponent<ExplorationPanel>();

            // --- BackgroundView ---
            GameObject bgViewObj = new GameObject("BackgroundView", typeof(RectTransform), typeof(SceneBackgroundView));
            bgViewObj.transform.SetParent(panel.transform, false);
            Stretch(bgViewObj.GetComponent<RectTransform>());

            var bgView = bgViewObj.GetComponent<SceneBackgroundView>();

            // Background Image
            Image bgImage = CreateImage(bgViewObj.transform, "BackgroundImage", Color.white);
            bgImage.raycastTarget = false;
            Stretch(bgImage.rectTransform);
            bgImage.sprite = null;
            bgImage.type = Image.Type.Simple;

            // Dark Overlay
            Image darkOverlay = CreateImage(bgViewObj.transform, "DarkOverlay", new Color(0, 0, 0, 0.35f));
            darkOverlay.raycastTarget = false;
            Stretch(darkOverlay.rectTransform);

            // Vignette Overlay
            Image vignetteOverlay = CreateImage(bgViewObj.transform, "VignetteOverlay", new Color(0, 0, 0, 0.5f));
            vignetteOverlay.raycastTarget = false;
            Stretch(vignetteOverlay.rectTransform);
            // 创建暗角效果（中心透明，边缘黑）
            // 使用 radial gradient 材质或简单 sprite
            // 这里简化：使用全屏半透明黑色

            // 绑定 BackgroundView 字段
            UnityEditor.SerializedObject bgSo = new UnityEditor.SerializedObject(bgView);
            bgSo.FindProperty("backgroundImage").objectReferenceValue = bgImage;
            bgSo.FindProperty("darkOverlay").objectReferenceValue = darkOverlay;
            bgSo.FindProperty("vignetteOverlay").objectReferenceValue = vignetteOverlay;
            bgSo.ApplyModifiedProperties();

            // --- Hotspot Container ---
            GameObject hotspotContainer = new GameObject("HotspotContainer", typeof(RectTransform));
            hotspotContainer.transform.SetParent(panel.transform, false);
            Stretch(hotspotContainer.GetComponent<RectTransform>());

            // --- Character Container ---
            GameObject characterContainer = new GameObject("CharacterContainer", typeof(RectTransform));
            characterContainer.transform.SetParent(panel.transform, false);
            Stretch(characterContainer.GetComponent<RectTransform>());

            // --- ObjectiveNoteView ---
            GameObject objectiveObj = new GameObject("ObjectiveNoteView", typeof(RectTransform), typeof(ObjectiveNoteView));
            objectiveObj.transform.SetParent(panel.transform, false);
            RectTransform objRect = objectiveObj.GetComponent<RectTransform>();
            objRect.anchorMin = new Vector2(0, 1);
            objRect.anchorMax = new Vector2(0, 1);
            objRect.pivot = new Vector2(0, 1);
            objRect.anchoredPosition = new Vector2(40, -40);
            objRect.sizeDelta = new Vector2(500, 160);

            var objectiveView = objectiveObj.GetComponent<ObjectiveNoteView>();

            // Location Text — 顶部固定，不拉伸
            TMP_Text locationText = CreateText(objectiveObj.transform, "LocationText", "<size=90%>當前地點</size>\n<size=120%>後台化妝間</size>", 24, TextAlignmentOptions.TopLeft);
            locationText.font = fontAsset;
            locationText.color = new Color(0.8f, 0.75f, 0.65f, 0.85f);
            locationText.rectTransform.anchorMin = new Vector2(0, 1);
            locationText.rectTransform.anchorMax = new Vector2(1, 1);
            locationText.rectTransform.pivot = new Vector2(0, 1);
            locationText.rectTransform.anchoredPosition = new Vector2(0, 0);
            locationText.rectTransform.sizeDelta = new Vector2(0, 60);
            locationText.textWrappingMode = TextWrappingModes.Normal;

            // Objective Text — 在 LocationText 下方，不重叠
            TMP_Text objectiveText = CreateText(objectiveObj.transform, "ObjectiveText", "<size=90%>當前目標</size>\n調查鏡台附近的異常，並試著與旦角交談。", 22, TextAlignmentOptions.TopLeft);
            objectiveText.font = fontAsset;
            objectiveText.color = new Color(0.8f, 0.75f, 0.65f, 0.85f);
            objectiveText.rectTransform.anchorMin = new Vector2(0, 1);
            objectiveText.rectTransform.anchorMax = new Vector2(1, 1);
            objectiveText.rectTransform.pivot = new Vector2(0, 1);
            objectiveText.rectTransform.anchoredPosition = new Vector2(0, -60);
            objectiveText.rectTransform.sizeDelta = new Vector2(0, 80);
            objectiveText.textWrappingMode = TextWrappingModes.Normal;

            // 绑定 ObjectiveNoteView
            UnityEditor.SerializedObject objSo = new UnityEditor.SerializedObject(objectiveView);
            objSo.FindProperty("locationText").objectReferenceValue = locationText;
            objSo.FindProperty("objectiveText").objectReferenceValue = objectiveText;
            objSo.ApplyModifiedProperties();

            // --- ClueToastView ---
            GameObject toastObj = new GameObject("ClueToastView", typeof(RectTransform), typeof(ClueToastView));
            toastObj.transform.SetParent(panel.transform, false);
            RectTransform toastRect = toastObj.GetComponent<RectTransform>();
            toastRect.anchorMin = new Vector2(1, 1);
            toastRect.anchorMax = new Vector2(1, 1);
            toastRect.pivot = new Vector2(1, 1);
            toastRect.anchoredPosition = new Vector2(-40, -40);
            toastRect.sizeDelta = new Vector2(480, 200);

            var toastView = toastObj.GetComponent<ClueToastView>();
            var toastCanvasGroup = toastObj.AddComponent<CanvasGroup>();
            toastCanvasGroup.alpha = 0f;
            toastCanvasGroup.blocksRaycasts = false;

            // Title Label — 顶部
            TMP_Text toastTitle = CreateText(toastObj.transform, "TitleLabel", "獲得線索", 28, TextAlignmentOptions.TopLeft);
            toastTitle.font = fontAsset;
            toastTitle.color = new Color(0.75f, 0.2f, 0.15f, 1f);
            toastTitle.rectTransform.anchorMin = new Vector2(0, 1);
            toastTitle.rectTransform.anchorMax = new Vector2(1, 1);
            toastTitle.rectTransform.pivot = new Vector2(0, 1);
            toastTitle.rectTransform.anchoredPosition = new Vector2(0, 0);
            toastTitle.rectTransform.sizeDelta = new Vector2(0, 40);

            // Clue Name — 在 Title 下方
            TMP_Text clueName = CreateText(toastObj.transform, "ClueNameLabel", "鏡台下的舊戲票", 24, TextAlignmentOptions.TopLeft);
            clueName.font = fontAsset;
            clueName.color = new Color(0.85f, 0.8f, 0.7f, 1f);
            clueName.rectTransform.anchorMin = new Vector2(0, 1);
            clueName.rectTransform.anchorMax = new Vector2(1, 1);
            clueName.rectTransform.pivot = new Vector2(0, 1);
            clueName.rectTransform.anchoredPosition = new Vector2(0, -40);
            clueName.rectTransform.sizeDelta = new Vector2(0, 40);

            // Clue Description — 在 ClueName 下方
            TMP_Text clueDesc = CreateText(toastObj.transform, "ClueDescLabel", "一張被灰塵和胭脂沾污的舊戲票……", 20, TextAlignmentOptions.TopLeft);
            clueDesc.font = fontAsset;
            clueDesc.color = new Color(0.7f, 0.7f, 0.65f, 0.9f);
            clueDesc.rectTransform.anchorMin = new Vector2(0, 1);
            clueDesc.rectTransform.anchorMax = new Vector2(1, 1);
            clueDesc.rectTransform.pivot = new Vector2(0, 1);
            clueDesc.rectTransform.anchoredPosition = new Vector2(0, -80);
            clueDesc.rectTransform.sizeDelta = new Vector2(0, 80);
            clueDesc.textWrappingMode = TextWrappingModes.Normal;

            // 绑定 ClueToastView
            UnityEditor.SerializedObject toastSo = new UnityEditor.SerializedObject(toastView);
            toastSo.FindProperty("titleLabel").objectReferenceValue = toastTitle;
            toastSo.FindProperty("clueNameLabel").objectReferenceValue = clueName;
            toastSo.FindProperty("clueDescLabel").objectReferenceValue = clueDesc;
            toastSo.ApplyModifiedProperties();

            // 加载子模块 Prefab 引用
            GameObject hotspotPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/HuaPi/Prefabs/Exploration/SceneHotspotItem.prefab");
            GameObject charPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/HuaPi/Prefabs/Exploration/SceneCharacterEntry.prefab");

            // 绑定 ExplorationPanel 私有序列化字段
            UnityEditor.SerializedObject panelSo = new UnityEditor.SerializedObject(panelComp);
            panelSo.FindProperty("backgroundView").objectReferenceValue = bgView;
            panelSo.FindProperty("hotspotContainer").objectReferenceValue = hotspotContainer.transform;
            panelSo.FindProperty("characterContainer").objectReferenceValue = characterContainer.transform;
            panelSo.FindProperty("objectiveView").objectReferenceValue = objectiveView;
            panelSo.FindProperty("clueToastView").objectReferenceValue = toastView;
            panelSo.FindProperty("hotspotItemPrefab").objectReferenceValue = hotspotPrefab;
            panelSo.FindProperty("characterEntryPrefab").objectReferenceValue = charPrefab;
            panelSo.ApplyModifiedProperties();

            // --- Prefab 保存 ---
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(panel, prefabPath);
            UnityEngine.Object.DestroyImmediate(panel);
            UnityEditor.AssetDatabase.SaveAssets();
            return savedPrefab.GetComponent<ExplorationPanel>();
        }

        /// <summary>
        /// 创建探索子模块 Prefab（热点 + 人物）
        /// </summary>
        private static void CreateExplorationSubPrefabs()
        {
            TMP_FontAsset fontAsset = CreateOrLoadHuapiFontAsset();

            // --- HotspotItem Prefab ---
            string hotspotPath = "Assets/HuaPi/Prefabs/Exploration/SceneHotspotItem.prefab";
            GameObject existingHotspot = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(hotspotPath);
            if (existingHotspot == null)
            {
                GameObject hotspotPrefab = new GameObject("SceneHotspotItem", typeof(RectTransform), typeof(SceneHotspotItem));
                RectTransform hotspotRect = hotspotPrefab.GetComponent<RectTransform>();
                hotspotRect.sizeDelta = new Vector2(120, 120);

                var hotspotComp = hotspotPrefab.GetComponent<SceneHotspotItem>();

                // Hotspot Area (透明点击区域)
                Image areaImage = CreateImage(hotspotPrefab.transform, "HotspotArea", new Color(0, 0, 0, 0.05f));
                areaImage.raycastTarget = true;
                Stretch(areaImage.rectTransform);
                areaImage.type = Image.Type.Simple;

                // Glow Border (Hover 发光)
                Image glowBorder = CreateImage(hotspotPrefab.transform, "GlowBorder", Color.clear);
                glowBorder.raycastTarget = false;
                Stretch(glowBorder.rectTransform);
                glowBorder.type = Image.Type.Simple;

                // Name Label (Hover 时显示)
                TMP_Text nameLabel = CreateText(hotspotPrefab.transform, "NameLabel", "镜台", 22, TextAlignmentOptions.Center);
                nameLabel.font = fontAsset;
                nameLabel.color = new Color(0.85f, 0.75f, 0.55f, 0f);
                nameLabel.rectTransform.anchorMin = new Vector2(0.5f, 1);
                nameLabel.rectTransform.anchorMax = new Vector2(0.5f, 1);
                nameLabel.rectTransform.pivot = new Vector2(0.5f, 0);
                nameLabel.rectTransform.anchoredPosition = new Vector2(0, 10);
                nameLabel.rectTransform.sizeDelta = new Vector2(200, 36);

                // 添加 EventTrigger 用于 Hover/Click
                var eventTrigger = hotspotPrefab.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                AddEventTriggerEntry(eventTrigger, UnityEngine.EventSystems.EventTriggerType.PointerEnter, (e) => hotspotComp.OnPointerEnter());
                AddEventTriggerEntry(eventTrigger, UnityEngine.EventSystems.EventTriggerType.PointerExit, (e) => hotspotComp.OnPointerExit());
                AddEventTriggerEntry(eventTrigger, UnityEngine.EventSystems.EventTriggerType.PointerClick, (e) => hotspotComp.OnPointerClick());

                // 绑定字段
                UnityEditor.SerializedObject hsSo = new UnityEditor.SerializedObject(hotspotComp);
                hsSo.FindProperty("hotspotArea").objectReferenceValue = areaImage;
                hsSo.FindProperty("glowBorder").objectReferenceValue = glowBorder;
                hsSo.FindProperty("nameLabel").objectReferenceValue = nameLabel;
                hsSo.ApplyModifiedProperties();

                PrefabUtility.SaveAsPrefabAsset(hotspotPrefab, hotspotPath);
                UnityEngine.Object.DestroyImmediate(hotspotPrefab);
            }

            // --- CharacterEntry Prefab ---
            string charPath = "Assets/HuaPi/Prefabs/Exploration/SceneCharacterEntry.prefab";
            GameObject existingChar = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(charPath);
            if (existingChar == null)
            {
                GameObject charPrefab = new GameObject("SceneCharacterEntry", typeof(RectTransform), typeof(SceneCharacterEntry));
                RectTransform charRect = charPrefab.GetComponent<RectTransform>();
                charRect.sizeDelta = new Vector2(400, 700);

                var charComp = charPrefab.GetComponent<SceneCharacterEntry>();

                // Portrait Image
                Image portraitImage = CreateImage(charPrefab.transform, "PortraitImage", Color.white);
                portraitImage.raycastTarget = true;
                Stretch(portraitImage.rectTransform);
                portraitImage.preserveAspect = true;

                // Outline Glow (Hover)
                Image outlineGlow = CreateImage(charPrefab.transform, "OutlineGlow", Color.clear);
                outlineGlow.raycastTarget = false;
                Stretch(outlineGlow.rectTransform);
                outlineGlow.type = Image.Type.Simple;

                // Shadow Overlay
                Image shadowOverlay = CreateImage(charPrefab.transform, "ShadowOverlay", new Color(0, 0, 0, 0.15f));
                shadowOverlay.raycastTarget = false;
                Stretch(shadowOverlay.rectTransform);
                var shadowCg = shadowOverlay.gameObject.AddComponent<CanvasGroup>();
                shadowCg.alpha = 0.15f;

                // Name Label
                TMP_Text charNameLabel = CreateText(charPrefab.transform, "NameLabel", "旦角", 24, TextAlignmentOptions.Center);
                charNameLabel.font = fontAsset;
                charNameLabel.color = new Color(0.85f, 0.75f, 0.55f, 0f);
                charNameLabel.rectTransform.anchorMin = new Vector2(0.5f, 1);
                charNameLabel.rectTransform.anchorMax = new Vector2(0.5f, 1);
                charNameLabel.rectTransform.pivot = new Vector2(0.5f, 0);
                charNameLabel.rectTransform.anchoredPosition = new Vector2(0, 10);
                charNameLabel.rectTransform.sizeDelta = new Vector2(300, 40);

                // 添加 EventTrigger
                var charEventTrigger = charPrefab.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                AddEventTriggerEntry(charEventTrigger, UnityEngine.EventSystems.EventTriggerType.PointerEnter, (e) => charComp.OnPointerEnter());
                AddEventTriggerEntry(charEventTrigger, UnityEngine.EventSystems.EventTriggerType.PointerExit, (e) => charComp.OnPointerExit());
                AddEventTriggerEntry(charEventTrigger, UnityEngine.EventSystems.EventTriggerType.PointerClick, (e) => charComp.OnPointerClick());

                // 绑定字段
                UnityEditor.SerializedObject charSo = new UnityEditor.SerializedObject(charComp);
                charSo.FindProperty("portraitImage").objectReferenceValue = portraitImage;
                charSo.FindProperty("outlineGlow").objectReferenceValue = outlineGlow;
                charSo.FindProperty("nameLabel").objectReferenceValue = charNameLabel;
                charSo.FindProperty("shadowOverlay").objectReferenceValue = shadowOverlay.gameObject;
                charSo.ApplyModifiedProperties();

                PrefabUtility.SaveAsPrefabAsset(charPrefab, charPath);
                UnityEngine.Object.DestroyImmediate(charPrefab);
            }

            UnityEditor.AssetDatabase.SaveAssets();
        }

        private static void AddEventTriggerEntry(UnityEngine.EventSystems.EventTrigger trigger, UnityEngine.EventSystems.EventTriggerType eventType, System.Action<UnityEngine.EventSystems.BaseEventData> action)
        {
            var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
            entry.eventID = eventType;
            entry.callback.AddListener((e) => action(e));
            trigger.triggers.Add(entry);
        }

        private static void AssignExplorationPrefab(UIManager uiManager, ExplorationPanel explorationPanel)
        {
            if (uiManager == null || explorationPanel == null) return;

            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(uiManager);
            so.FindProperty("explorationPanelPrefab").objectReferenceValue = explorationPanel;
            so.ApplyModifiedProperties();
            UnityEditor.EditorUtility.SetDirty(uiManager);
        }

        #endregion

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text CreateText(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TMP_Text tmp = go.GetComponent<TMP_Text>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = true;
            return tmp;
        }

        private static void CreateMenuText(Transform parent, string name, string text)
        {
            TMP_Text tmp = CreateText(parent, name, text, 34, TextAlignmentOptions.Center);
            tmp.font = CreateOrLoadHuapiFontAsset();
            tmp.color = new Color(0.788f, 0.541f, 0.431f, 0.85f);

            RectTransform rect = tmp.rectTransform;
            rect.sizeDelta = new Vector2(320f, 48f);

            Button button = tmp.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;

            LayoutElement layout = tmp.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 48f;
            layout.minHeight = 48f;
        }

        private static TMP_FontAsset CreateOrLoadHuapiFontAsset()
        {
            const string fontAssetPath = "Assets/Fonts/hakidame SDF.asset";
            TMP_FontAsset existing = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
            if (existing != null)
            {
                if (!IsFontAssetUsable(existing))
                {
                    UnityEditor.AssetDatabase.DeleteAsset(fontAssetPath);
                    existing = null;
                }
            }

            if (existing != null)
            {
                ConfigureFontAsset(existing);
                return existing;
            }

            Font sourceFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/hakidame.TTF");
            if (sourceFont == null)
            {
                Debug.LogWarning("[HuaPi UI] Assets/Fonts/hakidame.TTF not found. Falling back to TMP default font.");
                return TMP_Settings.defaultFontAsset;
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                90,
                9,
                GlyphRenderMode.SDFAA,
                2048,
                2048,
                AtlasPopulationMode.Dynamic,
                true);

            fontAsset.name = "hakidame SDF";
            UnityEditor.AssetDatabase.CreateAsset(fontAsset, fontAssetPath);
            SaveFontAssetSubAssets(fontAsset);
            ConfigureFontAsset(fontAsset);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            return fontAsset;
        }

        private static void ConfigureFontAsset(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null) return;

            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fontAsset.isMultiAtlasTexturesEnabled = true;
            fontAsset.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
            fontAsset.fallbackFontAssetTable.Clear();
            fontAsset.TryAddCharacters("画皮開始遊戲繼續設定退出薛家戲班民國二十年MASKSBEHIND");

            UnityEditor.EditorUtility.SetDirty(fontAsset);
            if (fontAsset.atlasTexture != null) UnityEditor.EditorUtility.SetDirty(fontAsset.atlasTexture);
            if (fontAsset.material != null) UnityEditor.EditorUtility.SetDirty(fontAsset.material);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        private static bool IsFontAssetUsable(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null || fontAsset.material == null)
            {
                return false;
            }

            Texture2D atlasTexture = null;
            try
            {
                atlasTexture = fontAsset.atlasTexture;
            }
            catch (MissingReferenceException)
            {
                return false;
            }

            return atlasTexture != null;
        }

        private static void SaveFontAssetSubAssets(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null) return;

            Texture2D atlasTexture = fontAsset.atlasTexture;
            if (atlasTexture != null && !UnityEditor.AssetDatabase.Contains(atlasTexture))
            {
                atlasTexture.name = "hakidame SDF Atlas";
                atlasTexture.hideFlags = HideFlags.None;
                UnityEditor.AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
            }

            Material material = fontAsset.material;
            if (material != null && !UnityEditor.AssetDatabase.Contains(material))
            {
                material.name = "hakidame SDF Material";
                material.hideFlags = HideFlags.None;
                UnityEditor.AssetDatabase.AddObjectToAsset(material, fontAsset);
            }
        }

        private static void ApplyFontToPrefab(GameObject root, TMP_FontAsset fontAsset)
        {
            if (root == null || fontAsset == null) return;

            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                text.font = fontAsset;
                UnityEditor.EditorUtility.SetDirty(text);
            }
        }

        private static void ApplyTraditionalMainMenuText(GameObject root)
        {
            SetChildText(root.transform, "Title", "画皮");
            SetChildText(root.transform, "Menu/StartGame", "開始遊戲");
            SetChildText(root.transform, "Menu/ContinueGame", "繼續遊戲");
            SetChildText(root.transform, "Menu/Settings", "設定");
            SetChildText(root.transform, "Menu/Quit", "退出");
            SetChildText(root.transform, "BottomInfo", "薛家戲班 · 民國二十年");
        }

        private static void SetChildText(Transform root, string path, string value)
        {
            TMP_Text text = root.Find(path)?.GetComponent<TMP_Text>();
            if (text == null) return;

            text.text = value;
            UnityEditor.EditorUtility.SetDirty(text);
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
