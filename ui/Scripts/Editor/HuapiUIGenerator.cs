using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HuaPi.UI.Editor
{
    /// <summary>
    /// 编辑器工具：一键生成《画皮》UI 场景结构
    /// 菜单路径：Tools / HuaPi / Generate UI
    /// </summary>
    public class HuapiUIGenerator : UnityEditor.EditorWindow
    {
        [UnityEditor.MenuItem("Tools/HuaPi/Generate UI")]
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
            GameObject uiRoot = new GameObject("UI_Root");
            Undo.RegisterCreatedObjectUndo(uiRoot, "Create UI Root");

            // 生成 Canvas 层级
            GenerateCanvasLayers(uiRoot);

            // 生成 UIManager
            GenerateUIManager(uiRoot);

            // 创建数据文件夹
            CreateDataFolders();

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
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                eventSystem.transform.SetParent(uiRoot.transform);
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            }
        }

        private static GameObject CreateCanvas(GameObject parent, string name, int sortOrder)
        {
            GameObject canvasGO = new GameObject(name);
            canvasGO.transform.SetParent(parent.transform);
            Undo.RegisterCreatedObjectUndo(canvasGO, $"Create {name}");

            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // 创建 CanvasGroup（用于控制整层透明度）
            CanvasGroup canvasGroup = canvasGO.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            return canvasGO;
        }

        private static void GenerateUIManager(GameObject uiRoot)
        {
            GameObject uiManagerGO = new GameObject("UIManager");
            uiManagerGO.transform.SetParent(uiRoot.transform);
            Undo.RegisterCreatedObjectUndo(uiManagerGO, "Create UIManager");

            var uiManager = uiManagerGO.AddComponent<Core.UIManager>();

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
        }

        private static Canvas FindCanvas(string name)
        {
            GameObject go = GameObject.Find(name);
            return go != null ? go.GetComponent<Canvas>() : null;
        }

        private static void CreateDataFolders()
        {
            string dataPath = "Assets/HuaPi/Data";
            if (!UnityEditor.AssetDatabase.IsValidFolder(dataPath))
            {
                UnityEditor.AssetDatabase.CreateFolder("Assets", "HuaPi");
                UnityEditor.AssetDatabase.CreateFolder("Assets/HuaPi", "Data");
            }
        }
    }
}
