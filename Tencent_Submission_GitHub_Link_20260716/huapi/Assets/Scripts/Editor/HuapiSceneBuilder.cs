using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

namespace TXGame
{
    /// <summary>
    /// 编辑器工具：一键搭建《画皮》游戏场景
    /// - 创建 Player Prefab（挂 PlayerController + PlayerInput）
    /// - 创建 6 个 NPC Prefab（挂 SpriteRenderer + NPCDialogueTrigger）
    /// - 创建调查点 Prefab（挂 InvestigationPoint）
    /// - 在场景中挂 SceneSetup
    /// - 自动绑定精灵和 SO 数据引用
    /// 菜单路径：Tools / HuaPi / Build Scene
    /// </summary>
    public class HuapiSceneBuilder : EditorWindow
    {
        private const string PREFAB_PLAYER_PATH = "Assets/Prefabs/Player.prefab";
        private const string PREFAB_NPC_PATH = "Assets/Prefabs/NPCs";
        private const string PREFAB_INVEST_PATH = "Assets/Prefabs/InvestigationPoints";
        private const string DATA_CHAR_PATH = "Assets/Resources/Characters";
        private const string DATA_DIALOGUE_PATH = "Assets/Resources/Dialogues";
        private const string DATA_CLUE_PATH = "Assets/Resources/Clues";
        private const string SPRITE_CHAR_BASE = "Assets/Art/Sprites/Characters/";
        private const string SPRITE_BG_BASE = "Assets/Art/Sprites/Backgrounds/";

        // 精灵原图约 943x2425 像素，PPU=100，原始世界单位约 9.43 x 24.25
        // 需要缩小到约 1.5-2 单位高度才适合场景（场景高度约 5-6 单位）
        // 24.25 * 0.06 ≈ 1.46 单位高度
        private const float CHAR_SPRITE_SCALE = 0.06f;

        private static readonly (string id, string name, string spriteKey)[] npcDefs = new[]
        {
            ("sheng", "柳梦生", "生"),
            ("dan",   "梅若云", "旦"),
            ("jing",  "钟铁面", "净"),
            ("chou",  "贾三笑", "丑"),
            ("muou",  "墨伶",   "木偶"),
            ("boss",  "班主老爷", "老板"),
        };

        [MenuItem("Tools/HuaPi/Build Scene")]
        private static void ShowWindow()
        {
            GetWindow<HuapiSceneBuilder>("场景搭建工具");
        }

        private void OnGUI()
        {
            GUILayout.Label("《画皮》场景搭建工具", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "此工具将自动完成：\n" +
                "1. 创建 Player Prefab（SpriteRenderer + Rigidbody2D + PlayerController + PlayerInput）\n" +
                "2. 创建 6 个 NPC Prefab（SpriteRenderer + BoxCollider2D + NPCDialogueTrigger）\n" +
                "3. 创建调查点 Prefab 模板\n" +
                "4. 自动绑定精灵到所有 SO 数据\n" +
                "5. 在场景中创建 SceneSetup 挂载点\n\n" +
                "精灵缩放: 0.06x（适配 1024+ 像素立绘）\n" +
                "已存在的 Prefab 不会被覆盖。",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("一键搭建场景", GUILayout.Height(50)))
            {
                BuildAll();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("修复 Input System 配置", GUILayout.Height(35)))
            {
                FixInputSystem();
            }

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("创建 Player Prefab", GUILayout.Height(35)))
                BuildPlayerPrefab();
            if (GUILayout.Button("创建 NPC Prefabs", GUILayout.Height(35)))
                BuildNPCPrefabs();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("创建调查点模板", GUILayout.Height(35)))
                BuildInvestigationPointPrefab();
            if (GUILayout.Button("自动绑定精灵", GUILayout.Height(35)))
                AutoBindSprites();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (GUILayout.Button("放置场景物件到当前场景", GUILayout.Height(35)))
            {
                PlaceSceneObjects();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("🔧 修复场景物件缩放（立绘过大）", GUILayout.Height(40)))
            {
                FixSceneObjectScales();
            }

            GUILayout.Space(5);

            GUI.color = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("⚠ 删除旧 Prefab 并重建（解决缩放问题）", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("确认重建",
                    "将删除以下 Prefab 并重建：\n" +
                    "- Player.prefab\n" +
                    "- 6 个 NPC Prefab\n" +
                    "- InvestigationPointTemplate.prefab\n\n" +
                    "场景中已有的实例不受影响。",
                    "确认重建", "取消"))
                {
                    RebuildAllPrefabs();
                }
            }
            GUI.color = Color.white;
        }

        private void BuildAll()
        {
            BuildPlayerPrefab();
            BuildNPCPrefabs();
            BuildInvestigationPointPrefab();
            AutoBindSprites();
            PlaceSceneObjects();
            EditorUtility.DisplayDialog("搭建完成",
                "场景搭建完成！\n\n" +
                "已完成：\n" +
                "- Player Prefab\n" +
                "- 6 个 NPC Prefab\n" +
                "- 调查点模板\n" +
                "- 精灵自动绑定\n" +
                "- 场景物件放置\n\n" +
                "现在可以点击 Play 测试游戏！",
                "确定");
        }

        #region Player Prefab

        private static void BuildPlayerPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PLAYER_PATH) != null)
            {
                Debug.Log("Player Prefab 已存在，跳过创建");
                return;
            }

            EnsureDir(PREFAB_PLAYER_PATH);

            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Default");

            // SpriteRenderer
            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            // 尝试找玩家精灵（用生作为默认角色精灵）
            Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SPRITE_CHAR_BASE}生.png");
            if (playerSprite != null) sr.sprite = playerSprite;

            // 设置初始缩放（精灵很大，需要缩小）
            player.transform.localScale = new Vector3(CHAR_SPRITE_SCALE, CHAR_SPRITE_SCALE, 1f);

            // Rigidbody2D
            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Collider
            CircleCollider2D col = player.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;

            // PlayerController
            PlayerController controller = player.AddComponent<PlayerController>();
            SerializedObject controllerSO = new SerializedObject(controller);
            controllerSO.FindProperty("interactableLayer").intValue = 1 << LayerMask.NameToLayer("Default");
            controllerSO.ApplyModifiedProperties();

            // PlayerInput (绑定 InputActionAsset)
            UnityEngine.InputSystem.PlayerInput pi = player.AddComponent<UnityEngine.InputSystem.PlayerInput>();
            pi.actions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            pi.defaultActionMap = "Player";
            pi.notificationBehavior = UnityEngine.InputSystem.PlayerNotifications.InvokeUnityEvents;

            // 保存 Prefab
            PrefabUtility.SaveAsPrefabAsset(player, PREFAB_PLAYER_PATH);
            DestroyImmediate(player);

            Debug.Log("✅ Player Prefab 创建完成");
        }

        #endregion

        #region NPC Prefabs

        private static void BuildNPCPrefabs()
        {
            EnsureDir(PREFAB_NPC_PATH + "/");

            foreach (var (id, name, spriteKey) in npcDefs)
            {
                string path = $"{PREFAB_NPC_PATH}/NPC_{name}.prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                {
                    Debug.Log($"NPC_{name} Prefab 已存在，跳过");
                    continue;
                }

                GameObject npc = new GameObject($"NPC_{name}");
                npc.tag = "NPC";
                npc.layer = LayerMask.NameToLayer("Default");

                // SpriteRenderer
                SpriteRenderer sr = npc.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 5;
                // 尝试加载主精灵
                Sprite npcSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SPRITE_CHAR_BASE}{spriteKey}.png");
                if (npcSprite == null)
                    npcSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SPRITE_CHAR_BASE}{spriteKey} 0.png");
                if (npcSprite != null) sr.sprite = npcSprite;

                // 设置初始缩放
                npc.transform.localScale = new Vector3(CHAR_SPRITE_SCALE, CHAR_SPRITE_SCALE, 1f);

                // Collider
                BoxCollider2D col = npc.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.8f, 1.2f);

                // NPCDialogueTrigger
                NPCDialogueTrigger trigger = npc.AddComponent<NPCDialogueTrigger>();
                SerializedObject so = new SerializedObject(trigger);
                so.FindProperty("npcName").stringValue = name;
                so.FindProperty("interactPrompt").stringValue = "交谈";
                so.ApplyModifiedProperties();

                // 自动绑定默认对话
                string dialoguePath = $"{DATA_DIALOGUE_PATH}/Dialogue_{id}_01.asset";
                DialogueData dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(dialoguePath);
                if (dialogue != null)
                {
                    so.FindProperty("defaultDialogue").objectReferenceValue = dialogue;
                    so.ApplyModifiedProperties();
                }

                PrefabUtility.SaveAsPrefabAsset(npc, path);
                DestroyImmediate(npc);

                Debug.Log($"✅ NPC_{name} Prefab 创建完成");
            }
        }

        #endregion

        #region Investigation Point Prefab

        private static void BuildInvestigationPointPrefab()
        {
            string path = $"{PREFAB_INVEST_PATH}/InvestigationPointTemplate.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("InvestigationPointTemplate Prefab 已存在，跳过");
                return;
            }

            EnsureDir(path);

            GameObject point = new GameObject("InvestigationPoint_Template");
            point.layer = LayerMask.NameToLayer("Default");

            // SpriteRenderer（默认显示一个小圆点/问号，实际使用时替换）
            SpriteRenderer sr = point.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;

            // Collider
            CircleCollider2D col = point.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;

            // InvestigationPoint
            point.AddComponent<InvestigationPoint>();

            PrefabUtility.SaveAsPrefabAsset(point, path);
            DestroyImmediate(point);

            Debug.Log("✅ InvestigationPointTemplate Prefab 创建完成");
        }

        #endregion

        #region 自动绑定精灵

        private static void AutoBindSprites()
        {
            int bindCount = 0;

            // 绑定 HuapiCharacterData
            foreach (var (id, name, spriteKey) in npcDefs)
            {
                string path = $"Assets/Data/HuapiCharacters/huapi_{id}.asset";
                HuapiCharacterData data = AssetDatabase.LoadAssetAtPath<HuapiCharacterData>(path);
                if (data == null) continue;

                SerializedObject so = new SerializedObject(data);
                Sprite portrait = AssetDatabase.LoadAssetAtPath<Sprite>($"{SPRITE_CHAR_BASE}{spriteKey}.png");
                Sprite hidden = AssetDatabase.LoadAssetAtPath<Sprite>($"{SPRITE_CHAR_BASE}{spriteKey} 2.png");

                if (portrait != null)
                {
                    so.FindProperty("portraitSprite").objectReferenceValue = portrait;
                    so.FindProperty("faceMaskIcon").objectReferenceValue = portrait;
                    bindCount++;
                }
                if (hidden != null)
                {
                    so.FindProperty("hiddenPortrait").objectReferenceValue = hidden;
                    bindCount++;
                }
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(data);
            }

            // 绑定 ObserveSkinRevealData 的背景图
            string revealDir = "Assets/Data/Reveals";
            if (AssetDatabase.IsValidFolder(revealDir))
            {
                string[] revealFiles = Directory.GetFiles(revealDir, "*.asset");
                foreach (string file in revealFiles)
                {
                    string assetPath = file.Replace("\\", "/");
                    ObserveSkinRevealData reveal = AssetDatabase.LoadAssetAtPath<ObserveSkinRevealData>(assetPath);
                    if (reveal == null) continue;
                    EditorUtility.SetDirty(reveal);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"✅ 精灵自动绑定完成，绑定了 {bindCount} 个精灵引用");
        }

        #endregion

        #region 放置场景物件

        private static void PlaceSceneObjects()
        {
            // 1. 创建或获取 SceneSetup
            SceneSetup setup = FindObjectOfType<SceneSetup>();
            if (setup == null)
            {
                GameObject setupGO = new GameObject("SceneSetup");
                setup = setupGO.AddComponent<SceneSetup>();
                Undo.RegisterCreatedObjectUndo(setupGO, "Create SceneSetup");
            }

            // 绑定引用
            SerializedObject setupSO = new SerializedObject(setup);
            setupSO.FindProperty("autoSetup").boolValue = true;

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PLAYER_PATH);
            if (playerPrefab != null)
                setupSO.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;

            // GameManager Prefab
            string gmPath = "Assets/Prefabs/GameManager.prefab";
            EnsureDir(gmPath);
            GameObject gmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gmPath);
            if (gmPrefab == null)
            {
                GameObject gm = new GameObject("GameManager");
                gm.AddComponent<GameManager>();
                gmPrefab = PrefabUtility.SaveAsPrefabAsset(gm, gmPath);
                DestroyImmediate(gm);
            }
            setupSO.FindProperty("gameManagerPrefab").objectReferenceValue = gmPrefab;

            // UI Canvas Prefab
            GameObject uiRoot = GameObject.Find("UI_Root");
            string uiPrefabPath = "Assets/Prefabs/UI_Root.prefab";
            EnsureDir(uiPrefabPath);
            if (uiRoot != null && AssetDatabase.LoadAssetAtPath<GameObject>(uiPrefabPath) == null)
            {
                GameObject uiPrefab = PrefabUtility.SaveAsPrefabAsset(uiRoot, uiPrefabPath);
                setupSO.FindProperty("uiCanvasPrefab").objectReferenceValue = uiPrefab;
            }
            else
            {
                GameObject existingUIPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(uiPrefabPath);
                if (existingUIPrefab != null)
                    setupSO.FindProperty("uiCanvasPrefab").objectReferenceValue = existingUIPrefab;
            }

            setupSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(setup);

            // 2. 放置 Player
            GameObject playerPrefabGO = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PLAYER_PATH);
            if (playerPrefabGO != null)
            {
                GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
                if (existingPlayer != null)
                {
                    existingPlayer.transform.position = new Vector3(-5f, -1f, 0f);
                    existingPlayer.transform.localScale = new Vector3(CHAR_SPRITE_SCALE, CHAR_SPRITE_SCALE, 1f);
                }
                else
                {
                    GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefabGO);
                    playerInstance.name = "Player";
                    playerInstance.transform.position = new Vector3(-5f, -1f, 0f);
                    playerInstance.transform.localScale = new Vector3(CHAR_SPRITE_SCALE, CHAR_SPRITE_SCALE, 1f);
                    Undo.RegisterCreatedObjectUndo(playerInstance, "Place Player");
                }
            }

            // 3. 放置 NPC（排列在场景中）
            Vector3[] npcPositions = new Vector3[]
            {
                new Vector3(-4f, 2f, 0f),   // 柳梦生
                new Vector3(-2f, 2f, 0f),   // 梅若云
                new Vector3(0f, 2f, 0f),    // 钟铁面
                new Vector3(2f, 2f, 0f),    // 贾三笑
                new Vector3(4f, 2f, 0f),    // 墨伶
                new Vector3(0f, -2f, 0f),   // 班主老爷
            };

            for (int i = 0; i < npcDefs.Length; i++)
            {
                string npcPath = $"{PREFAB_NPC_PATH}/NPC_{npcDefs[i].name}.prefab";
                GameObject npcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(npcPath);
                if (npcPrefab == null) continue;

                // 检查场景中是否已存在
                string npcName = $"NPC_{npcDefs[i].name}";
                GameObject existing = GameObject.Find(npcName);
                if (existing != null)
                {
                    // 修复已存在的 NPC 缩放
                    existing.transform.localScale = new Vector3(CHAR_SPRITE_SCALE, CHAR_SPRITE_SCALE, 1f);
                    continue;
                }

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(npcPrefab);
                instance.name = npcName;
                instance.transform.position = npcPositions[i];
                // 立绘精灵原图通常很大(1024+像素)，需要缩小到场景比例
                instance.transform.localScale = new Vector3(CHAR_SPRITE_SCALE, CHAR_SPRITE_SCALE, 1f);
                Undo.RegisterCreatedObjectUndo(instance, $"Place {npcName}");
            }

            // 3. 放置背景
            CreateBackgroundObject();

            // 4. 放置调查点示例
            CreateSampleInvestigationPoints();

            Debug.Log("✅ 场景物件放置完成");
        }

        private static void CreateBackgroundObject()
        {
            if (GameObject.Find("Background") != null) return;

            GameObject bg = new GameObject("Background");
            bg.transform.position = Vector3.zero;

            SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -10;

            // 尝试加载默认背景
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SPRITE_BG_BASE}后台.png");
            if (bgSprite == null)
                bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SPRITE_BG_BASE}后台 0.png");
            if (bgSprite != null) sr.sprite = bgSprite;

            // 调整大小适配屏幕
            if (bgSprite != null)
            {
                bg.transform.localScale = new Vector3(0.06f, 0.06f, 1f);
            }

            Undo.RegisterCreatedObjectUndo(bg, "Create Background");
        }

        private static void CreateSampleInvestigationPoints()
        {
            string[] clueNames = new[]
            {
                "化妆台", "戏服箱", "木偶摊", "账本柜", "后台入口"
            };

            Vector3[] positions = new Vector3[]
            {
                new Vector3(-3f, 0f, 0f),
                new Vector3(3f, 0f, 0f),
                new Vector3(4.5f, 2f, 0f),
                new Vector3(0f, -3.5f, 0f),
                new Vector3(-5f, -1f, 0f),
            };

            string[] linkedClues = new[]
            {
                "clue_04",  // 化妆台 → 未寄出的信
                "clue_01",  // 戏服箱 → 烧焦戏服
                "clue_07",  // 木偶摊 → 木偶纸片
                "clue_13",  // 账本柜 → 神秘信件
                "clue_06",  // 后台入口 → 破损道具
            };

            for (int i = 0; i < clueNames.Length; i++)
            {
                string goName = $"InvestigationPoint_{clueNames[i]}";
                if (GameObject.Find(goName) != null) continue;

                GameObject point = new GameObject(goName);
                point.transform.position = positions[i];

                // SpriteRenderer（小标记）
                SpriteRenderer sr = point.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 3;
                sr.color = new Color(1f, 0.8f, 0.2f, 0.7f);
                // 创建一个小的方形精灵
                sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

                CircleCollider2D col = point.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.6f;

                InvestigationPoint ip = point.AddComponent<InvestigationPoint>();
                SerializedObject ipSO = new SerializedObject(ip);
                ipSO.FindProperty("pointName").stringValue = clueNames[i];
                ipSO.FindProperty("hintText").stringValue = "调查";
                ipSO.FindProperty("unlockClueIDs").arraySize = 1;
                ipSO.FindProperty("unlockClueIDs").GetArrayElementAtIndex(0).stringValue = linkedClues[i];
                ipSO.ApplyModifiedProperties();

                Undo.RegisterCreatedObjectUndo(point, $"Create {goName}");
            }

            Debug.Log("✅ 5 个调查点已创建");
        }

        #endregion

        #region 修复 Input System

        private static void FixInputSystem()
        {
            // 1. 修复 EventSystem：替换 StandaloneInputModule → InputSystemUIInputModule
            var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null)
            {
                var oldModule = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                if (oldModule != null)
                {
                    DestroyImmediate(oldModule);
                    eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                    Debug.Log("✅ EventSystem StandaloneInputModule → InputSystemUIInputModule");
                }
            }

            // 2. 修复场景中的 Player 实例
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                var playerInput = playerGO.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (playerInput != null)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>("Assets/InputSystem_Actions.inputactions");
                    if (asset != null)
                    {
                        playerInput.actions = asset;
                        Debug.Log("✅ PlayerInput actions 已绑定");
                    }
                }
            }

            // 3. 更新 Player Prefab
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PLAYER_PATH);
            if (playerPrefab != null)
            {
                var ppi = playerPrefab.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (ppi != null)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>("Assets/InputSystem_Actions.inputactions");
                    if (asset != null)
                    {
                        SerializedObject pso = new SerializedObject(ppi);
                        pso.FindProperty("m_Actions").objectReferenceValue = asset;
                        pso.ApplyModifiedProperties();
                        EditorUtility.SetDirty(playerPrefab);
                        Debug.Log("✅ Player Prefab InputAction 已绑定");
                    }
                }
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("修复完成",
                "Input System 配置已修复！\n\n" +
                "- EventSystem → InputSystemUIInputModule\n" +
                "- PlayerInput → 绑定 InputActionAsset\n" +
                "- Player Prefab → 更新绑定\n\n" +
                "请重新运行游戏测试。",
                "确定");
        }

        #endregion

        #region 修复场景物件缩放

        private static void FixSceneObjectScales()
        {
            int fixedCount = 0;

            // 修复所有 NPC
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject go in allObjects)
            {
                if (go.name.StartsWith("NPC_"))
                {
                    go.transform.localScale = new Vector3(CHAR_SPRITE_SCALE, CHAR_SPRITE_SCALE, 1f);
                    fixedCount++;
                    Debug.Log($"✅ 修复 NPC: {go.name} → scale {CHAR_SPRITE_SCALE}");
                }
            }

            // 修复 Player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.localScale = new Vector3(CHAR_SPRITE_SCALE, CHAR_SPRITE_SCALE, 1f);
                fixedCount++;
                Debug.Log($"✅ 修复 Player → scale {CHAR_SPRITE_SCALE}");
            }

            // 修复 InvestigationPoint 缩放
            foreach (GameObject go in allObjects)
            {
                if (go.name.StartsWith("InvestigationPoint_"))
                {
                    go.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                    fixedCount++;
                    Debug.Log($"✅ 修复调查点: {go.name} → scale 0.5");
                }
            }

            // 修复 Background 缩放
            GameObject bg = GameObject.Find("Background");
            if (bg != null)
            {
                bg.transform.localScale = new Vector3(0.06f, 0.06f, 1f);
                fixedCount++;
                Debug.Log("✅ 修复 Background → scale 0.06");
            }

            EditorUtility.DisplayDialog("缩放修复完成",
                $"已修复 {fixedCount} 个场景物件的缩放。\n\n" +
                $"NPC / Player / Background: {CHAR_SPRITE_SCALE}x\n" +
                "调查点: 0.5x\n\n" +
                "如果仍然太大或太小，可以调整后重新运行。",
                "确定");
        }

        #endregion

        #region 删除并重建 Prefab

        private static void RebuildAllPrefabs()
        {
            // 删除旧 Prefab
            DeletePrefab(PREFAB_PLAYER_PATH);
            foreach (var (id, name, _) in npcDefs)
            {
                DeletePrefab($"{PREFAB_NPC_PATH}/NPC_{name}.prefab");
            }
            DeletePrefab($"{PREFAB_INVEST_PATH}/InvestigationPointTemplate.prefab");
            DeletePrefab("Assets/Prefabs/GameManager.prefab");

            AssetDatabase.Refresh();

            // 重建
            BuildPlayerPrefab();
            BuildNPCPrefabs();
            BuildInvestigationPointPrefab();
            AutoBindSprites();

            EditorUtility.DisplayDialog("重建完成",
                "所有 Prefab 已删除并重建！\n\n" +
                $"新 Prefab 默认缩放: {CHAR_SPRITE_SCALE}x\n\n" +
                "请点击「放置场景物件到当前场景」重新放置。",
                "确定");
        }

        private static void DeletePrefab(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"🗑 已删除: {path}");
            }
        }

        #endregion

        #region 工具方法

        private static void EnsureDir(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath).Replace("\\", "/");
            if (string.IsNullOrEmpty(dir)) return;

            string[] parts = dir.Split('/');
            string current = "";
            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;
                string parent = current;
                current = string.IsNullOrEmpty(current) ? part : $"{current}/{part}";
                if (!AssetDatabase.IsValidFolder(current))
                {
                    AssetDatabase.CreateFolder(parent, part);
                }
            }
        }

        #endregion
    }
}
