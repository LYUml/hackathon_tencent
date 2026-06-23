using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.TextCore.LowLevel;
using HuaPi.UI.Core;
using HuaPi.UI.Panels;
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

            UnityEditor.EditorUtility.DisplayDialog("生成完成", "UI 场景结构已生成！请检查 Hierarchy 面板。", "确定");
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
            string prefabPath = "Assets/HuaPi/Prefabs/MainMenuPanel.prefab";
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
            tmp.enableWordWrapping = false;
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
