using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using HuaPi.UI.Data;

namespace HuaPi.UI.Editor
{
    /// <summary>
    /// 场景数据自动生成器：一键为所有背景图创建 ExplorationSceneData ScriptableObject。
    /// 自动匹配角色、创建默认热点和人物配置。
    /// </summary>
    public class SceneDataAutoGenerator : EditorWindow
    {
        [Header("Source Folders")]
        [SerializeField] private string backgroundsPath = "Assets/HuaPi/Art/Backgrounds";
        [SerializeField] private string charactersPath = "Assets/HuaPi/Art/Characters";
        [SerializeField] private string outputPath = "Assets/HuaPi/Data/ExplorationScenes/Generated";

        [Header("Character Matching Rules")]
        [SerializeField] private List<CharacterMatchRule> matchRules = new List<CharacterMatchRule>
        {
            new CharacterMatchRule { sceneKeyword = "後台", characterKeywords = new[] { "旦", "旦角" } },
            new CharacterMatchRule { sceneKeyword = "化妝", characterKeywords = new[] { "旦", "旦角" } },
            new CharacterMatchRule { sceneKeyword = "舞台", characterKeywords = new[] { "生", "生角" } },
            new CharacterMatchRule { sceneKeyword = "戲台", characterKeywords = new[] { "生", "生角" } },
            new CharacterMatchRule { sceneKeyword = "庭院", characterKeywords = new[] { "净", "净角" } },
            new CharacterMatchRule { sceneKeyword = "排練", characterKeywords = new[] { "丑", "丑角" } },
            new CharacterMatchRule { sceneKeyword = "儲藏", characterKeywords = new[] { "木偶", "木偶2" } },
            new CharacterMatchRule { sceneKeyword = "道具", characterKeywords = new[] { "木偶", "木偶2" } },
            new CharacterMatchRule { sceneKeyword = "雅座", characterKeywords = new[] { "老板", "老板0" } },
            new CharacterMatchRule { sceneKeyword = "觀演", characterKeywords = new[] { "老板", "老板0" } },
            new CharacterMatchRule { sceneKeyword = "側台", characterKeywords = new[] { "净", "净角" } },
            new CharacterMatchRule { sceneKeyword = "幕後", characterKeywords = new[] { "生", "生角" } },
            new CharacterMatchRule { sceneKeyword = "戲箱", characterKeywords = new[] { "木偶", "木偶2" } },
            new CharacterMatchRule { sceneKeyword = "盔頭", characterKeywords = new[] { "净", "净角" } },
        };

        [Header("Default Settings")]
        [SerializeField] private string defaultObjective = "探索這個場景，尋找線索並與人物交談。";
        [SerializeField] private bool createDefaultHotspots = true;
        [SerializeField] private bool createDefaultCharacter = true;

        [System.Serializable]
        public class CharacterMatchRule
        {
            public string sceneKeyword;
            public string[] characterKeywords;
        }

        private Vector2 scrollPos;
        private string logOutput = "";
        private bool showAdvanced = false;

        [MenuItem("Tools/HuaPi/Auto Generate Scene Data")]
        public static void ShowWindow()
        {
            GetWindow<SceneDataAutoGenerator>("场景数据生成器");
        }

        private void OnGUI()
        {
            GUILayout.Label("🎬 《画皮》场景数据自动生成器", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // 路径设置
            EditorGUILayout.LabelField("📁 路径配置", EditorStyles.boldLabel);
            backgroundsPath = EditorGUILayout.TextField("背景图文件夹", backgroundsPath);
            charactersPath = EditorGUILayout.TextField("人物立绘文件夹", charactersPath);
            outputPath = EditorGUILayout.TextField("输出文件夹", outputPath);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("⚙️ 默认设置", EditorStyles.boldLabel);
            defaultObjective = EditorGUILayout.TextField("默认目标文本", defaultObjective);
            createDefaultHotspots = EditorGUILayout.Toggle("创建默认热点", createDefaultHotspots);
            createDefaultCharacter = EditorGUILayout.Toggle("创建默认人物", createDefaultCharacter);

            // 高级设置
            showAdvanced = EditorGUILayout.Foldout(showAdvanced, "高级：角色匹配规则");
            if (showAdvanced)
            {
                EditorGUILayout.HelpBox(
                    "根据场景名称关键词自动匹配角色。\n" +
                    "例如：场景名包含 '后台' → 匹配旦角\n" +
                    "场景名包含 '舞台' → 匹配生角",
                    MessageType.Info
                );

                // 显示匹配规则（简化版，不显示完整列表避免界面太长）
                EditorGUILayout.LabelField($"当前共 {matchRules.Count} 条匹配规则");
            }

            EditorGUILayout.Space(20);

            // 生成按钮
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 1f);
            if (GUILayout.Button("🚀 一键生成所有场景数据", GUILayout.Height(40)))
            {
                GenerateAllSceneData();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            // 日志输出
            if (!string.IsNullOrEmpty(logOutput))
            {
                EditorGUILayout.LabelField("📋 生成日志:", EditorStyles.boldLabel);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(150));
                EditorGUILayout.HelpBox(logOutput, MessageType.Info);
                EditorGUILayout.EndScrollView();
            }
        }

        private void GenerateAllSceneData()
        {
            logOutput = "";
            int createdCount = 0;
            int skippedCount = 0;

            // 确保输出文件夹存在
            if (!AssetDatabase.IsValidFolder(outputPath))
            {
                string parentPath = System.IO.Path.GetDirectoryName(outputPath).Replace('\\', '/');
                string folderName = System.IO.Path.GetFileName(outputPath);
                AssetDatabase.CreateFolder(parentPath, folderName);
                logOutput += $"✅ 创建输出文件夹: {outputPath}\n";
            }

            // 加载所有背景图
            string[] backgroundGuids = AssetDatabase.FindAssets("t:Sprite", new[] { backgroundsPath });
            logOutput += $"📁 找到 {backgroundGuids.Length} 张背景图\n";

            // 加载所有人物立绘
            string[] characterGuids = AssetDatabase.FindAssets("t:Sprite", new[] { charactersPath });
            List<Sprite> allCharacters = new List<Sprite>();
            foreach (var guid in characterGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) allCharacters.Add(sprite);
            }
            logOutput += $"👤 找到 {allCharacters.Count} 张人物立绘\n\n";

            // 为每个背景图创建场景数据
            foreach (var bgGuid in backgroundGuids)
            {
                string bgPath = AssetDatabase.GUIDToAssetPath(bgGuid);
                Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
                if (bgSprite == null) continue;

                string fileName = System.IO.Path.GetFileNameWithoutExtension(bgPath);
                string sceneId = SanitizeSceneId(fileName);
                string outputFilePath = $"{outputPath}/Scene_{sceneId}.asset";

                // 检查是否已存在
                if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(outputFilePath) != null)
                {
                    logOutput += $"⏭️ 跳过（已存在）: {fileName}\n";
                    skippedCount++;
                    continue;
                }

                // 创建 ExplorationSceneData
                var sceneData = ScriptableObject.CreateInstance<ExplorationSceneData>();
                sceneData.sceneId = sceneId;
                sceneData.displayName = ExtractDisplayName(fileName);
                sceneData.backgroundSprite = bgSprite;
                sceneData.objectiveText = GenerateObjectiveText(fileName);

                // 匹配角色
                Sprite matchedCharacter = MatchCharacter(fileName, allCharacters);
                if (matchedCharacter != null && createDefaultCharacter)
                {
                    sceneData.characters = new[]
                    {
                        new SceneCharacterEntryData
                        {
                            characterId = $"char_{SanitizeSceneId(matchedCharacter.name)}",
                            displayName = ExtractCharacterDisplayName(matchedCharacter.name),
                            portraitSprite = matchedCharacter,
                            anchoredPosition = new Vector2(280, 540),
                            size = new Vector2(380, 680),
                            dialogueId = $"dialogue_{SanitizeSceneId(matchedCharacter.name)}_{sceneId}",
                            hoverText = ExtractCharacterDisplayName(matchedCharacter.name)
                        }
                    };
                    logOutput += $"  👤 匹配角色: {matchedCharacter.name}\n";
                }
                else
                {
                    sceneData.characters = new SceneCharacterEntryData[0];
                }

                // 创建默认热点
                if (createDefaultHotspots)
                {
                    sceneData.hotspots = new[]
                    {
                        new SceneHotspotData
                        {
                            hotspotId = $"hotspot_default_{sceneId}",
                            displayName = "可疑之處",
                            anchoredPosition = new Vector2(1200, 500),
                            size = new Vector2(150, 150),
                            linkedClueId = $"clue_{sceneId}_default",
                            hoverText = "調查此處",
                            clickFeedbackText = "你發現了一些可疑的痕跡..."
                        }
                    };
                }
                else
                {
                    sceneData.hotspots = new SceneHotspotData[0];
                }

                // 保存 Asset
                AssetDatabase.CreateAsset(sceneData, outputFilePath);
                EditorUtility.SetDirty(sceneData);

                logOutput += $"✅ 创建场景: {sceneData.displayName} ({sceneId})\n";
                createdCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            logOutput += $"\n🎉 完成！创建了 {createdCount} 个场景，跳过 {skippedCount} 个已存在场景。";
            logOutput += $"\n📂 输出位置: {outputPath}";

            EditorUtility.DisplayDialog(
                "生成完成",
                $"成功创建 {createdCount} 个场景数据！\n" +
                $"跳过 {skippedCount} 个已存在场景。\n\n" +
                $"输出位置: {outputPath}",
                "确定"
            );
        }

        /// <summary>
        /// 根据场景名称匹配最合适的角色
        /// </summary>
        private Sprite MatchCharacter(string sceneName, List<Sprite> characters)
        {
            sceneName = sceneName.ToLower();

            foreach (var rule in matchRules)
            {
                if (sceneName.Contains(rule.sceneKeyword.ToLower()))
                {
                    foreach (var charKeyword in rule.characterKeywords)
                    {
                        var matched = characters.FirstOrDefault(c =>
                            c.name.ToLower().Contains(charKeyword.ToLower())
                        );
                        if (matched != null) return matched;
                    }
                }
            }

            // 默认返回第一个角色
            return characters.Count > 0 ? characters[0] : null;
        }

        /// <summary>
        /// 从文件名提取显示名称
        /// </summary>
        private string ExtractDisplayName(string fileName)
        {
            // 移除 "2" 后缀（变体标识）
            string clean = fileName.Replace(" 2", "").Replace("2", "").Trim();

            // 清理特殊字符
            clean = clean.Replace(":", "·").Replace("/", "·");

            return string.IsNullOrEmpty(clean) ? fileName : clean;
        }

        /// <summary>
        /// 提取角色显示名称
        /// </summary>
        private string ExtractCharacterDisplayName(string spriteName)
        {
            string name = spriteName.Replace(" 2", "").Trim();

            // 映射到中文角色名
            if (name.Contains("旦")) return "旦角";
            if (name.Contains("生")) return "生角";
            if (name.Contains("净")) return "净角";
            if (name.Contains("丑")) return "丑角";
            if (name.Contains("老板")) return "班主";
            if (name.Contains("木偶")) return "木偶";

            return name;
        }

        /// <summary>
        /// 生成場景專屬的目標文本
        /// </summary>
        private string GenerateObjectiveText(string sceneName)
        {
            string scene = sceneName.ToLower();

            if (scene.Contains("後台") || scene.Contains("化妝"))
                return "調查鏡台附近的異常，並試著與旦角交談。";
            if (scene.Contains("舞台") || scene.Contains("戲台"))
                return "檢查舞台上的道具，尋找生角留下的線索。";
            if (scene.Contains("庭院"))
                return "在庭院中搜尋，注意淨角可能留下的痕跡。";
            if (scene.Contains("排練"))
                return "觀察排練室的痕跡，與丑角對話。";
            if (scene.Contains("儲藏") || scene.Contains("戲箱"))
                return "在舊物中尋找線索，調查木偶的秘密。";
            if (scene.Contains("雅座") || scene.Contains("觀演"))
                return "與班主交談，了解戲班近日的異常。";
            if (scene.Contains("幕後"))
                return "探索幕後的秘密，尋找可疑的痕跡。";

            return defaultObjective;
        }

        /// <summary>
        /// 将文件名转换为合法的 Scene ID
        /// </summary>
        private string SanitizeSceneId(string input)
        {
            return input.ToLower()
                .Replace(" ", "_")
                .Replace("·", "_")
                .Replace("·", "_")
                .Replace(":", "_")
                .Replace("/", "_")
                .Replace("\\", "_")
                .Replace("-", "_")
                .Replace("__", "_")
                .Trim('_');
        }
    }
}
