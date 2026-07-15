using UnityEngine;
using UnityEditor;
using System.IO;

namespace TXGame
{
    /// <summary>
    /// 编辑器工具：批量生成《画皮》所有 ScriptableObject 数据
    /// 菜单路径：Tools / HuaPi / Generate All Data
    /// </summary>
    public class HuapiDataGenerator : EditorWindow
    {
        private const string CHARACTER_DATA_PATH = "Assets/Resources/Characters";
        private const string CLUE_DATA_PATH = "Assets/Resources/Clues";
        private const string DIALOGUE_DATA_PATH = "Assets/Resources/Dialogues";
        private const string REVEAL_DATA_PATH = "Assets/Data/Reveals";
        private const string HUAPI_CHARACTER_PATH = "Assets/Data/HuapiCharacters";
        private const string HUAPI_CLUE_PATH = "Assets/Data/HuapiClues";
        private const string HUAPI_DIALOGUE_PATH = "Assets/Data/HuapiDialogues";

        // 角色精灵路径前缀（不带数字后缀）
        private const string SPRITE_CHAR_BASE = "Assets/Art/Sprites/Characters/";
        private const string SPRITE_BG_BASE = "Assets/Art/Sprites/Backgrounds/";

        [MenuItem("Tools/HuaPi/Generate All Data")]
        private static void ShowWindow()
        {
            GetWindow<HuapiDataGenerator>("画皮数据生成器");
        }

        private void OnGUI()
        {
            GUILayout.Label("《画皮》数据批量生成器", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "此工具将自动生成所有 ScriptableObject 数据：\n" +
                "- 6 个角色核心数据 (CharacterData)\n" +
                "- 6 个角色UI数据 (HuapiCharacterData)\n" +
                "- 线索数据 (ClueData + HuapiClueData)\n" +
                "- 对话数据 (DialogueData + HuapiDialogueData)\n" +
                "- 观皮揭露数据 (ObserveSkinRevealData)\n\n" +
                "已存在的数据不会被覆盖。",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("生成全部角色数据", GUILayout.Height(40)))
            {
                GenerateAllCharacters();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("生成全部线索数据", GUILayout.Height(40)))
            {
                GenerateAllClues();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("生成全部对话数据", GUILayout.Height(40)))
            {
                GenerateAllDialogues();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("生成观皮揭露数据", GUILayout.Height(40)))
            {
                GenerateAllReveals();
            }

            GUILayout.Space(5);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("一键生成所有数据", GUILayout.Height(50)))
            {
                GenerateAllCharacters();
                GenerateAllClues();
                GenerateAllDialogues();
                GenerateAllReveals();
                EditorUtility.DisplayDialog("生成完成", "所有 ScriptableObject 数据已生成！\n请在 Data 目录下查看。", "确定");
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "数据生成后请手动在 Inspector 中拖入对应的 Sprite 资源。\n" +
                "精灵路径提示已写入每个 SO 的名称字段。",
                MessageType.Warning);
        }

        // ==================== 角色定义 ====================
        private static readonly (string id, string name, string roleType, string identity, string trueIdent,
            CharacterData.FaceType faceType, string personality, string secret, string trueStory)[] Characters =
        {
            (
                "sheng", "柳梦生", "小生", "戏班当家小生", "失踪案的幕后推手",
                CharacterData.FaceType.Red,
                "温文尔雅，待人谦和，戏台上风流倜傥。在戏班中人缘极好，是班主最器重的弟子。但偶尔会在无人处露出阴郁的神情，仿佛背负着什么。",
                "柳梦生与失踪的旦角有过一段不为人知的感情纠葛，失火当晚他曾出现在后台。更可怕的是——他的房间里藏着一件不属于他的东西。",
                "柳梦生其实是当年那场火灾的唯一目击者，他看到了真正的纵火者，但因为对方的威胁一直不敢说出真相。他脸上的红脸妆容下，隐藏着深深的恐惧与愧疚。"
            ),
            (
                "dan", "梅若云", "青衣", "戏班花旦", "失踪的旦角之妹",
                CharacterData.FaceType.White,
                "表面柔弱，实则心思缜密。总是安静地观察着戏班里的一切。她对姐姐的失踪始终耿耿于怀，不相信官府的「意外」结论。",
                "梅若云并不是真正的梅若云——真正的梅若云已经在火灾中丧生，现在的她是为了查明真相而假扮姐姐的孪生妹妹。",
                "妹妹扮成姐姐混入戏班，日复一日地在暗中收集证据。她脸上的白脸不是戏妆，而是真正的悲伤之色。她已经掌握了关键证据，但还差最后一块拼图。"
            ),
            (
                "jing", "钟铁面", "净行·武净", "戏班武净花脸", "前江湖人士",
                CharacterData.FaceType.Black,
                "沉默寡言，身材魁梧，脸上总是画着浓重的黑色脸谱。在戏班里负责武戏，平时独来独往，很少与人交流。",
                "钟铁面年轻时曾是一名江湖刀客，手上沾过血。他来到戏班是为了躲避仇家，却意外卷入了失踪案。他房间里的刀，也许不只是道具。",
                "钟铁面在火灾当晚救了一个人——但他不愿说出是谁。黑脸之下是一颗想要赎罪的心。他多年前的仇家，正是这场阴谋的幕后黑手之一。"
            ),
            (
                "chou", "贾三笑", "文丑", "戏班文丑", "知情不报者",
                CharacterData.FaceType.Yellow,
                "戏班里的开心果，插科打诨样样精通。但他笑得太多了，多到让人觉得他在掩盖什么。经常半夜在戏班各处闲逛，说是「找灵感」。",
                "贾三笑在火灾当晚看到了关键的一幕，但他选择了沉默——因为那个纵火者是他的至亲。他每天的笑声背后，是无法言说的痛苦。",
                "贾三笑一直在暗中保护着真正的纵火者，他以为只要掩盖真相就能保住那个人。但当更多无辜的人开始受到威胁时，他必须做出选择——亲情还是正义。"
            ),
            (
                "muou", "墨伶", "木偶师", "流浪木偶艺人", "复仇者",
                CharacterData.FaceType.Silver,
                "来历不明的木偶艺人，带着一个精致的提线木偶在戏班附近游荡。从不说话，只用木偶与人交流。那双空洞的木偶眼睛仿佛能看穿一切。",
                "墨伶是十年前另一场戏班火灾的幸存者，那场火灾夺走了他全家的性命。他的木偶中藏着当年的真相。他来到这个戏班，是为了阻止同样的悲剧再次发生——或者，是为了完成自己的复仇。",
                "墨伶的木偶是用当年火灾中烧毁的戏服用料制作的。每一个木偶关节里都藏着一张纸条，记录着十年前那场火灾的真相。他等待了十年，只为等到幕后黑手再次现身。"
            ),
            (
                "boss", "班主老爷", "班主", "戏班老板", "知情者",
                CharacterData.FaceType.Gold,
                "德高望重的戏班老板，对戏班有着深厚感情。表面上对失踪案痛心疾首，积极配合官府调查。但每当有人提到火灾当晚的细节时，他的眼神会闪烁。",
                "班主老爷知道火灾的真相，但他选择了隐瞒——因为真相一旦曝光，整个戏班都将面临灭顶之灾。他用金脸掩饰着内心的恐惧，用威严压制着所有人的追问。",
                "班主老爷在几十年前欠了一个人情，如今这个人情需要用整个戏班来偿还。失踪案只是开始，更大的阴谋正在逼近。金脸之下是一张被命运折磨得疲惫不堪的面孔。"
            )
        };

        // ==================== 线索定义 ====================
        private static readonly (string id, string name, string desc, ClueData.ClueCategory cat,
            ClueData.ClueSource src, string charId, float progress)[] Clues =
        {
            ("clue_01", "烧焦的戏服碎片", "在后台角落发现的烧焦戏服碎片，上面隐约能看到绣花的痕迹。这应该是属于某个旦角的戏服，但为何会在那里？", ClueData.ClueCategory.物品, ClueData.ClueSource.场景调查, "dan", 0.15f),
            ("clue_02", "一把旧刀", "钟铁面房间床下发现的一把旧刀，刀刃上有干涸的血迹。刀柄上刻着一个模糊的名字。", ClueData.ClueCategory.物品, ClueData.ClueSource.场景调查, "jing", 0.20f),
            ("clue_03", "半夜脚步声", "多位戏班成员提到，每晚深夜都能听到走廊里有轻微的脚步声，但打开门却空无一人。", ClueData.ClueCategory.对话, ClueData.ClueSource.角色对话, "chou", 0.10f),
            ("clue_04", "一封未寄出的信", "在梅若云的化妆台下发现一封泛黄的信，收信人是失踪的旦角，落款日期是火灾前一天。信中提到了「那件事」。", ClueData.ClueCategory.文书, ClueData.ClueSource.场景调查, "dan", 0.25f),
            ("clue_05", "破损的木偶", "墨伶的木偶不知何时掉落了一只手臂，手臂关节里掉出一张烧焦的纸片。上面写着半个名字。", ClueData.ClueCategory.物品, ClueData.ClueSource.场景调查, "muou", 0.30f),
            ("clue_06", "班主的账本", "账本中有一笔奇怪的支出，每月固定数额汇往同一个地址。但那个地址属于一个不存在的地方。", ClueData.ClueCategory.文书, ClueData.ClueSource.场景调查, "boss", 0.20f),
            ("clue_07", "柳梦生的秘密对话", "深夜无意中听到柳梦生在后台对着空气说话，语气充满恐惧和恳求。他在对谁说话？", ClueData.ClueCategory.对话, ClueData.ClueSource.角色对话, "sheng", 0.15f),
            ("clue_08", "火灾当晚的节目单", "节目单上有六个人的名字被划掉了——其中包括失踪的旦角。但划掉笔迹各不相同，不像是一个人干的。", ClueData.ClueCategory.文书, ClueData.ClueSource.场景调查, "boss", 0.15f),
            ("clue_09", "贾三笑的秘密", "在贾三笑的枕头下发现一张画——画上是他和一个陌生男子的合影，背面写着「对不起」。", ClueData.ClueCategory.物品, ClueData.ClueSource.场景调查, "chou", 0.25f),
            ("clue_10", "火油痕迹", "在戏台后方发现了少量火油的痕迹，经确认不是戏班日常使用的灯油。有人故意准备了纵火工具。", ClueData.ClueCategory.场景, ClueData.ClueSource.场景调查, "jing", 0.30f),
            ("clue_11", "墨伶的身世", "从老戏迷口中打听到，十年前确实有另一个戏班发生过类似的火灾。那场火灾中唯一生还的是一个孩子。", ClueData.ClueCategory.对话, ClueData.ClueSource.角色对话, "muou", 0.20f),
            ("clue_12", "关键证人", "戏班门口卖馄饨的老王说，火灾当晚他看到一个黑影从后台翻墙而出。但第二天官府却说「没有外人进入的迹象」。", ClueData.ClueCategory.对话, ClueData.ClueSource.角色对话, "sheng", 0.20f),
            ("clue_13", "神秘人信件", "班主房间的暗格中发现几封信，信上要求班主「履行当年的承诺」。最后期限是下个月初一。", ClueData.ClueCategory.文书, ClueData.ClueSource.场景调查, "boss", 0.30f),
            ("clue_14", "两个梅若云", "仔细对比戏班的老照片发现——「失踪前的梅若云」和「现在的梅若云」虽然极其相似，但耳垂形状不同。", ClueData.ClueCategory.场景, ClueData.ClueSource.白天探索, "dan", 0.35f),
            ("clue_15", "木偶中的纸片", "修复墨伶的木偶时发现，每个关节都藏着纸片。拼凑起来是一份完整的十年前火灾调查报告——但报告结论被篡改了。", ClueData.ClueCategory.物品, ClueData.ClueSource.夜晚推理, "muou", 0.40f),
        };

        // ==================== 观皮揭露定义 ====================
        private static readonly (string id, string charId, string clueId, Rect area, string text)[] Reveals =
        {
            ("reveal_sheng_01", "sheng", "clue_07", new Rect(0.1f, 0.1f, 0.35f, 0.4f), "柳梦生的脸上出现了恐惧的表情——原来他在火灾当晚看到了不该看到的人。"),
            ("reveal_sheng_02", "sheng", "clue_12", new Rect(0.5f, 0.1f, 0.4f, 0.4f), "红脸下露出了深深的泪痕。他一直在为没能阻止那场火灾而自责。"),
            ("reveal_dan_01", "dan", "clue_04", new Rect(0.05f, 0.05f, 0.4f, 0.5f), "梅若云的真实身份被揭露——她不是失踪的旦角，而是她的孪生妹妹。"),
            ("reveal_dan_02", "dan", "clue_14", new Rect(0.5f, 0.05f, 0.45f, 0.5f), "白脸之下是一双充满泪水的眼睛。她为了查明姐姐的真相，已经伪装了整整一年。"),
            ("reveal_jing_01", "jing", "clue_02", new Rect(0.1f, 0.1f, 0.35f, 0.45f), "钟铁面的黑脸下露出了疲惫——那把刀上刻的名字，是他十年前失散的兄弟。"),
            ("reveal_jing_02", "jing", "clue_10", new Rect(0.5f, 0.1f, 0.4f, 0.45f), "他看到了火油是谁准备的。那天晚上他救的人，就是那个纵火者——他的亲兄弟。"),
            ("reveal_chou_01", "chou", "clue_03", new Rect(0.1f, 0.05f, 0.4f, 0.5f), "贾三笑笑容背后的真相——每天深夜的脚步声，是他去给藏匿的纵火者送饭。"),
            ("reveal_chou_02", "chou", "clue_09", new Rect(0.55f, 0.05f, 0.4f, 0.5f), "黄脸之下是深深的痛苦。他保护的不是别人，是他的父亲——当年的仇家已经找到了他们。"),
            ("reveal_muou_01", "muou", "clue_05", new Rect(0.1f, 0.1f, 0.35f, 0.4f), "木偶的空洞眼睛后面是墨伶真正的眼神——十年前火灾唯一的幸存者。"),
            ("reveal_muou_02", "muou", "clue_15", new Rect(0.55f, 0.1f, 0.35f, 0.4f), "银脸下是一张被烧伤的脸。他等了十年，就是为了揭穿那个篡改调查报告的人——班主老爷。"),
            ("reveal_boss_01", "boss", "clue_06", new Rect(0.1f, 0.1f, 0.35f, 0.4f), "班主老爷的金脸出现了裂痕——那个不存在的地址，其实是他的老家。他每个月都在给十年前那场火灾的遇难者家属汇款。"),
            ("reveal_boss_02", "boss", "clue_13", new Rect(0.55f, 0.1f, 0.35f, 0.4f), "班主老爷彻底崩溃——当年欠下的人情是替人顶罪。真正的纵火者还逍遥法外，现在又回来要挟他了。"),
        };

        // ==================== 生成方法 ====================

        private static void GenerateAllCharacters()
        {
            EnsureDirectoriesExist(CHARACTER_DATA_PATH, HUAPI_CHARACTER_PATH);

            foreach (var (id, name, roleType, identity, trueIdent, faceType, personality, secret, trueStory) in Characters)
            {
                // 核心角色数据
                string corePath = $"{CHARACTER_DATA_PATH}/Character_{id}.asset";
                if (!AssetDatabase.LoadAssetAtPath<CharacterData>(corePath))
                {
                    var cd = CreateInstance<CharacterData>();
                    cd.characterID = id;
                    cd.characterName = name;
                    cd.faceName = faceType switch
                    {
                        CharacterData.FaceType.Red => "红脸",
                        CharacterData.FaceType.Black => "黑脸",
                        CharacterData.FaceType.White => "白脸",
                        CharacterData.FaceType.Blue => "蓝脸",
                        CharacterData.FaceType.Yellow => "黄脸",
                        CharacterData.FaceType.Green => "绿脸",
                        CharacterData.FaceType.Gold => "金脸",
                        CharacterData.FaceType.Silver => "银脸",
                        _ => "未知"
                    };
                    cd.faceType = faceType;
                    cd.publicIdentity = identity;
                    cd.trueIdentity = trueIdent;
                    cd.personality = personality;
                    cd.secret = secret;
                    cd.trueStory = trueStory;

                    // 尝试自动分配精灵
                    cd.fullBodyCG = LoadSprite($"{SPRITE_CHAR_BASE}{name}.png");
                    cd.faceMaskSprite = LoadSprite($"{SPRITE_CHAR_BASE}{name}0.png");

                    AssetDatabase.CreateAsset(cd, corePath);
                    Debug.Log($"✓ 已创建核心角色数据: {corePath}");
                }
                else
                {
                    Debug.Log($"⊙ 角色数据已存在，跳过: {corePath}");
                }

                // UI 角色数据
                string uiPath = $"{HUAPI_CHARACTER_PATH}/HuapiChar_{id}.asset";
                if (!AssetDatabase.LoadAssetAtPath<HuapiCharacterData>(uiPath))
                {
                    var hcd = CreateInstance<HuapiCharacterData>();
                    hcd.characterId = id;
                    hcd.characterName = name;
                    hcd.roleType = roleType;
                    hcd.surfaceIdentity = identity;
                    hcd.knownInfo = $"行当：{roleType}。身份：{identity}。";
                    hcd.suspiciousPoints = secret.Length > 100 ? secret.Substring(0, 100) + "..." : secret;
                    hcd.portraitSprite = LoadSprite($"{SPRITE_CHAR_BASE}{name}.png");
                    hcd.hiddenPortrait = LoadSprite($"{SPRITE_CHAR_BASE}{name} 2.png");
                    hcd.faceMaskIcon = LoadSprite($"{SPRITE_CHAR_BASE}{name}0.png");
                    hcd.revealProgress = 0f;
                    hcd.roleColor = faceType switch
                    {
                        CharacterData.FaceType.Red => new Color(0.8f, 0.2f, 0.2f),
                        CharacterData.FaceType.Black => new Color(0.2f, 0.2f, 0.2f),
                        CharacterData.FaceType.White => new Color(0.9f, 0.9f, 0.9f),
                        CharacterData.FaceType.Blue => new Color(0.2f, 0.3f, 0.8f),
                        CharacterData.FaceType.Yellow => new Color(0.9f, 0.8f, 0.2f),
                        CharacterData.FaceType.Green => new Color(0.2f, 0.7f, 0.3f),
                        CharacterData.FaceType.Gold => new Color(0.9f, 0.7f, 0.1f),
                        CharacterData.FaceType.Silver => new Color(0.7f, 0.7f, 0.8f),
                        _ => Color.white
                    };

                    AssetDatabase.CreateAsset(hcd, uiPath);
                    Debug.Log($"✓ 已创建UI角色数据: {uiPath}");
                }
                else
                {
                    Debug.Log($"⊙ UI角色数据已存在，跳过: {uiPath}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("角色数据生成完成", $"已生成 {Characters.Length} 个角色数据。\n请在 Data/Characters 和 Data/HuapiCharacters 下查看。", "确定");
        }

        private static void GenerateAllClues()
        {
            EnsureDirectoriesExist(CLUE_DATA_PATH, HUAPI_CLUE_PATH);

            foreach (var (id, name, desc, cat, src, charId, progress) in Clues)
            {
                // 核心线索数据
                string corePath = $"{CLUE_DATA_PATH}/{id}.asset";
                if (!AssetDatabase.LoadAssetAtPath<ClueData>(corePath))
                {
                    var cd = CreateInstance<ClueData>();
                    cd.clueID = id;
                    cd.clueName = name;
                    cd.description = desc;
                    cd.category = cat;
                    cd.source = src;
                    cd.relatedCharacterID = charId;
                    cd.revealProgress = progress;
                    cd.revealAreaCenter = new Vector2(0.3f + (progress * 0.4f), 0.3f);
                    cd.revealAreaRadius = 0.15f + (progress * 0.05f);

                    AssetDatabase.CreateAsset(cd, corePath);
                    Debug.Log($"✓ 已创建核心线索数据: {corePath}");
                }
                else
                {
                    Debug.Log($"⊙ 核心线索数据已存在，跳过: {corePath}");
                }

                // UI 线索数据
                string uiPath = $"{HUAPI_CLUE_PATH}/{id}.asset";
                if (!AssetDatabase.LoadAssetAtPath<HuapiClueData>(uiPath))
                {
                    var hcd = CreateInstance<HuapiClueData>();
                    hcd.clueId = id;
                    hcd.clueName = name;
                    hcd.description = desc;
                    hcd.sourceLocation = src == ClueData.ClueSource.场景调查 ? "戏班" :
                                        src == ClueData.ClueSource.白天探索 ? "戏班各处" :
                                        src == ClueData.ClueSource.角色对话 ? "对话中" :
                                        src == ClueData.ClueSource.夜晚推理 ? "夜晚推理" : "剧情";
                    hcd.sourceCharacter = charId;
                    hcd.relatedCharacterId = charId;
                    hcd.isAcquired = false;
                    hcd.isUsedInGuanPi = false;

                    AssetDatabase.CreateAsset(hcd, uiPath);
                    Debug.Log($"✓ 已创建UI线索数据: {uiPath}");
                }
                else
                {
                    Debug.Log($"⊙ UI线索数据已存在，跳过: {uiPath}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("线索数据生成完成", $"已生成 {Clues.Length} 个线索数据。\n请在 Data/Clues 和 Data/HuapiClues 下查看。", "确定");
        }

        private static void GenerateAllDialogues()
        {
            EnsureDirectoriesExist(DIALOGUE_DATA_PATH, HUAPI_DIALOGUE_PATH);

            // 为每个角色生成对话数据
            string[][] dialogueChains = {
                new[] { "sheng_01", "柳梦生", "柳梦生",
                    "你来找我有什么事吗？",
                    "关于失火那天晚上……你在哪里？",
                    "那天晚上？我在排练《牡丹亭》，很多人都看到了。" },
                new[] { "sheng_02", "柳梦生", "柳梦生",
                    "……你看起来不太对劲。",
                    "你在害怕什么？",
                    "（沉默良久）你不懂。有些事，不知道反而更安全。" },
                new[] { "dan_01", "梅若云", "梅若云",
                    "（轻轻整理着戏服）你也是来问我关于姐姐的事吗？",
                    "我想知道真相。",
                    "真相……（苦笑）我也想知道真相。但有些人不想让我们知道。" },
                new[] { "dan_02", "梅若云", "梅若云",
                    "你真的想帮我查清楚吗？",
                    "当然。",
                    "那好，但你必须答应我——无论查到什么，都要让真相大白于天下。" },
                new[] { "jing_01", "钟铁面", "钟铁面",
                    "（默默擦拭手中的刀）什么事？",
                    "这把刀……不是道具吧？",
                    "（手微微一顿）和你无关。走开。" },
                new[] { "jing_02", "钟铁面", "钟铁面",
                    "你还在查那个案子？",
                    "我不会放弃的。",
                    "（长叹）既然如此……我告诉你，那天晚上我看到一个人从后台跑出去。但不是外人——是我们戏班的人。" },
                new[] { "chou_01", "贾三笑", "贾三笑",
                    "嘿嘿，又来查案？你比官府还认真啊！",
                    "你不觉得奇怪吗？为什么大家都遮遮掩掩的？",
                    "（笑容微微僵硬）这个嘛……每个人都有自己的苦衷嘛，哈哈哈……" },
                new[] { "chou_02", "贾三笑", "贾三笑",
                    "你……你是不是知道了什么？",
                    "你深夜在走廊里做什么？",
                    "（笑容彻底消失）……求你了，别再问了。再查下去，会有更多人遭殃。" },
                new[] { "muou_01", "墨伶", "墨伶",
                    "（墨伶没有说话，木偶却抬起了头）……",
                    "你的木偶……里面藏着什么？",
                    "（木偶的手突然指向戏台方向，然后猛地落下）" },
                new[] { "muou_02", "墨伶", "墨伶",
                    "（墨伶终于开口了，声音沙哑）你很像当年的我。",
                    "十年前……到底发生了什么？",
                    "（从木偶中取出一张泛黄的纸）看完这个你就明白了。但记住——一旦知道真相，就没有回头路了。" },
                new[] { "boss_01", "班主老爷", "班主老爷",
                    "年轻人，这么晚了还在四处走动？",
                    "班主，我想问您几个问题。",
                    "（捋了捋胡须）问吧，但有些问题我可能无法回答。" },
                new[] { "boss_02", "班主老爷", "班主老爷",
                    "你查到了多少？",
                    "够多了。我知道那场火灾不是意外。",
                    "（长叹一声）你终究还是查到了这一步……坐下吧，我给你讲一个三十年前的故事。" },
            };

            int nodeCounter = 1;
            foreach (var chain in dialogueChains)
            {
                string id = chain[0];
                string speakerId = chain[1];
                string speakerName = chain[2];
                string npcLine = chain[3];
                string playerChoice = chain[4];
                string npcReply = chain[5];

                // 核心对话数据
                string corePath = $"{DIALOGUE_DATA_PATH}/Dialogue_{id}.asset";
                if (!AssetDatabase.LoadAssetAtPath<DialogueData>(corePath))
                {
                    var dd = CreateInstance<DialogueData>();
                    dd.dialogueID = id;
                    dd.speakerID = speakerId;
                    dd.speakerName = speakerName;

                    dd.conditions = new System.Collections.Generic.List<DialogueCondition>
                    {
                        new DialogueCondition { type = DialogueCondition.ConditionType.Always, parameter = "", threshold = 0 }
                    };

                    dd.lines = new System.Collections.Generic.List<DialogueLine>
                    {
                        new DialogueLine { speakerName = speakerName, text = npcLine, autoAdvanceDelay = 0 }
                    };

                    dd.hasChoices = true;
                    dd.choices = new System.Collections.Generic.List<DialogueChoice>
                    {
                        new DialogueChoice
                        {
                            choiceText = playerChoice,
                            nextDialogueID = "",
                            choiceResults = new System.Collections.Generic.List<DialogueResult>
                            {
                                new DialogueResult { type = DialogueResult.ResultType.SetFlag, parameter = $"{id}_asked" }
                            }
                        }
                    };

                    dd.results = new System.Collections.Generic.List<DialogueResult>();

                    AssetDatabase.CreateAsset(dd, corePath);
                    Debug.Log($"✓ 已创建核心对话数据: {corePath}");
                }
                else
                {
                    Debug.Log($"⊙ 核心对话数据已存在，跳过: {corePath}");
                }

                // UI 对话数据
                string uiPath = $"{HUAPI_DIALOGUE_PATH}/HuapiDialogue_{id}.asset";
                if (!AssetDatabase.LoadAssetAtPath<HuapiDialogueData>(uiPath))
                {
                    var hdd = CreateInstance<HuapiDialogueData>();
                    hdd.nodeId = nodeCounter++;

                    hdd.lines = new HuapiDialogueLine[]
                    {
                        new HuapiDialogueLine
                        {
                            speakerId = speakerId,
                            text = npcLine,
                            highlightedKeywords = ExtractKeywords(npcLine)
                        },
                        new HuapiDialogueLine
                        {
                            speakerId = "player",
                            text = playerChoice,
                            highlightedKeywords = new string[0]
                        },
                        new HuapiDialogueLine
                        {
                            speakerId = speakerId,
                            text = npcReply,
                            highlightedKeywords = ExtractKeywords(npcReply)
                        }
                    };

                    hdd.options = new HuapiDialogueOption[]
                    {
                        new HuapiDialogueOption
                        {
                            optionText = playerChoice,
                            nextNodeId = -1,
                            requiredClueIds = new string[0],
                            clueIdsToConsume = new string[0]
                        }
                    };

                    AssetDatabase.CreateAsset(hdd, uiPath);
                    Debug.Log($"✓ 已创建UI对话数据: {uiPath}");
                }
                else
                {
                    Debug.Log($"⊙ UI对话数据已存在，跳过: {uiPath}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("对话数据生成完成", $"已生成 {dialogueChains.Length} 个对话数据。\n请在 Data/Dialogues 和 Data/HuapiDialogues 下查看。", "确定");
        }

        private static void GenerateAllReveals()
        {
            EnsureDirectoriesExist(REVEAL_DATA_PATH);

            foreach (var (id, charId, clueId, area, text) in Reveals)
            {
                string path = $"{REVEAL_DATA_PATH}/{id}.asset";
                if (!AssetDatabase.LoadAssetAtPath<ObserveSkinRevealData>(path))
                {
                    var rd = CreateInstance<ObserveSkinRevealData>();
                    rd.nodeId = id;
                    rd.relatedCharacterId = charId;
                    rd.requiredClueId = clueId;
                    rd.revealArea = area;
                    rd.revealedText = text;
                    rd.triggersNewObjective = id.Contains("_02"); // 第二个揭露触发新目标
                    rd.newObjectiveId = id.Contains("_02") ? $"objective_{charId}_complete" : "";
                    rd.fogFadeDuration = 0.8f + Random.value * 0.4f;

                    AssetDatabase.CreateAsset(rd, path);
                    Debug.Log($"✓ 已创建观皮揭露数据: {path}");
                }
                else
                {
                    Debug.Log($"⊙ 观皮揭露数据已存在，跳过: {path}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("观皮揭露数据生成完成", $"已生成 {Reveals.Length} 个揭露数据。\n请在 Data/Reveals 下查看。", "确定");
        }

        // ==================== 辅助方法 ====================

        private static void EnsureDirectoriesExist(params string[] paths)
        {
            foreach (string path in paths)
            {
                string fullPath = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(fullPath) && !Directory.Exists(fullPath))
                {
                    // 递归创建目录
                    string[] parts = path.Split('/');
                    string currentPath = "";
                    for (int i = 0; i < parts.Length; i++)
                    {
                        currentPath += (i == 0 ? parts[i] : "/" + parts[i]);
                        if (i > 0 && !AssetDatabase.IsValidFolder(currentPath))
                        {
                            string parent = currentPath.Substring(0, currentPath.LastIndexOf('/'));
                            string folderName = currentPath.Substring(currentPath.LastIndexOf('/') + 1);
                            AssetDatabase.CreateFolder(parent, folderName);
                        }
                    }
                }
            }
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static string[] ExtractKeywords(string text)
        {
            // 简单关键词提取：引号内的内容
            var keywords = new System.Collections.Generic.List<string>();
            var matches = System.Text.RegularExpressions.Regex.Matches(text, "「(.+?)」|\"(.+?)\"|《(.+?)》");
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                for (int i = 1; i <= 3; i++)
                {
                    if (match.Groups[i].Success)
                    {
                        keywords.Add(match.Groups[i].Value);
                        break;
                    }
                }
            }
            return keywords.ToArray();
        }
    }
}
