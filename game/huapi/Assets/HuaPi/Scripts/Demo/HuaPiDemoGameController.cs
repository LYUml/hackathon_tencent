using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HuaPi.Demo
{
    /// <summary>
    /// Two-day vertical-slice controller for the first chapter.
    /// It builds a playable UI flow directly from current placeholder UI direction,
    /// so final art can later replace sprites/prefabs without blocking gameplay.
    /// </summary>
    public class HuaPiDemoGameController : MonoBehaviour
    {
        private enum ScreenMode
        {
            MainMenu,
            ChapterTitle,
            ExploreProps,
            ExploreRehearsal,
            DayTwoTitle,
            ExploreLockedRoom,
            ExploreAudience,
            Dialogue,
            ClueInventory,
            CharacterArchive,
            Judgement,
            ObserveSkin,
            FinalTrial,
            Failure,
            Ending
        }

        private sealed class Clue
        {
            public string Id;
            public string Name;
            public string Source;
            public string Keyword;
            public string Description;
            public bool Acquired;
            public bool Used;
        }

        private sealed class CharacterInfo
        {
            public string Id;
            public string Name;
            public string Role;
            public string Known;
            public string Suspicion;
            public string PortraitPath;
            public bool Revealed;
        }

        private sealed class Hotspot
        {
            public string Id;
            public string Label;
            public Vector2 Position;
            public Vector2 Size;
            public string ClueId;
            public string Feedback;
            public string RequiredClueId;
        }

        private sealed class Topic
        {
            public string Label;
            public string Subtitle;
            public string RequiredClueId;
            public string[] Lines;
            public string GrantsClueId;
            public string UnlocksObjective;
        }

        private Canvas _canvas;
        private RectTransform _root;
        private readonly List<GameObject> _spawned = new List<GameObject>();
        private readonly Dictionary<string, Clue> _clues = new Dictionary<string, Clue>();
        private readonly Dictionary<string, CharacterInfo> _characters = new Dictionary<string, CharacterInfo>();
        private readonly HashSet<string> _askedTopics = new HashSet<string>();
        private ScreenMode _mode;
        private ScreenMode _lastExploreMode = ScreenMode.ExploreProps;
        private string _objective = "调查陈列柜附近的异常";
        private int _hearts = 5;
        private int _dialogueLineIndex;
        private string[] _activeDialogueLines = Array.Empty<string>();
        private string _activeSpeaker = "";
        private string _activeCharacterId = "";

        private static readonly Color DeepBlack = Hex(0x070504, 0.92f);
        private static readonly Color Paper = Hex(0xD8C7A7);
        private static readonly Color PaperText = Hex(0x2D1710);
        private static readonly Color Gold = Hex(0xC9A96E);
        private static readonly Color Red = Hex(0x8B2020);
        private static readonly Color WhiteText = Hex(0xF1E8D8);
        private static readonly Color MutedText = Hex(0xBDAF9A);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindAnyObjectByType<HuaPiDemoGameController>() != null) return;

            var go = new GameObject("HuaPi Demo Game Controller");
            DontDestroyOnLoad(go);
            go.AddComponent<HuaPiDemoGameController>();
        }

        private void Awake()
        {
            EnsureEventSystem();
            SeedData();
            BuildCanvas();
        }

        private void Start()
        {
            ShowMainMenu();
        }

        private void Update()
        {
            if (WasEscapePressed())
            {
                if (_mode == ScreenMode.MainMenu) return;
                ShowPauseOverlay();
            }

            if ((_mode == ScreenMode.Dialogue || _mode == ScreenMode.Ending) &&
                WasAdvancePressed())
            {
                AdvanceDialogue();
            }
        }

        private void SeedData()
        {
            AddClue("old_ticket", "旧戏票", "道具陈列室", "青衣勿忘",
                "票根被烧过一角，背面写着“青衣勿忘”。它指向沈青衣，也指向民国旧案里的《断面》。");
            AddClue("mask_box", "脸谱盒", "道具陈列室", "烧焦脸谱",
                "脸谱盒夹层里藏着半片烧焦脸谱。有人一直保存着火案遗物。");
            AddClue("door_scratch", "门闩划痕", "道具陈列室", "内侧新痕",
                "门闩被人从内侧撬过，划痕很新，位置和旧火案记录里的封门痕迹一致。");
            AddClue("axi_testimony", "阿喜的口供", "阿喜", "昨夜旧词",
                "阿喜说昨夜听见沈青衣在陈列柜前唱《断面》的旧词，还看见班主从后门方向出来。");
            AddClue("show_flow", "演出流程表", "道具陈列室", "流程一致",
                "今晚《断面》的登台顺序、锣鼓点和民国火案当夜完全一致。");
            AddClue("changed_schedule", "被改动的排练表", "排练室", "临时替换",
                "《断面》原本不是今晚复排曲目，是昨晚临时替换进流程的。");
            AddClue("prop_record", "道具库出借记录", "排练室", "班主批准",
                "旧木门闩、青衣旧戏服、脸谱残片昨晚被取出，批准人是薛万山。");
            AddClue("missing_video", "监控缺失片段", "排练室", "三分钟",
                "昨晚 23:57 到 00:00 的监控被管理员权限手动删除。");
            AddClue("burn_mark", "新鲜焦痕", "封闭后台小屋", "新烧旧痕",
                "门框下层是旧火烧痕，上层却有昨晚留下的新焦痕。有人试图把现代痕迹伪装成民国旧案。");
            AddClue("sealed_room_note", "封门纸条", "封闭后台小屋", "别让他出来",
                "纸条写着“别让他出来”。字迹新旧混杂，像是故意模仿民国案卷里的封门记录。");
            AddClue("manager_key", "后台总钥匙", "封闭后台小屋", "薛氏印章",
                "钥匙坠上刻着薛氏剧团的旧印。它能打开道具库、监控室和封闭后台小屋。");
            AddClue("puppet_thread", "戏偶牵线", "雅座", "牵线人",
                "细线从雅座暗格通向侧台门帘。有人不用上台，也能控制台上的“重演”。");
            AddClue("guest_register", "雅座登记册", "雅座", "空白来客",
                "今晚雅座只登记了一个空白姓名，签名栏被墨块涂掉。");
            AddClue("old_case_photo", "民国旧照", "雅座", "没有脸的人",
                "旧照里火案幸存者站在沈家青衣身后，面部被烟熏和刀痕毁去。现在剧团里有人一直在复刻他的归来。");

            _characters["qingyi"] = new CharacterInfo
            {
                Id = "qingyi",
                Name = "沈青衣",
                Role = "旦角 / 剧团台柱",
                Known = "说话轻，回避旧案，对脸谱盒和旧戏票反应异常。",
                Suspicion = "说没去过陈列室，但她知道门闩和旧火案细节。",
                PortraitPath = "Assets/HuaPi/Art/Characters/旦2.png"
            };
            _characters["axi"] = new CharacterInfo
            {
                Id = "axi",
                Name = "阿喜",
                Role = "丑角 / 误导嫌疑人",
                Known = "嘴碎、怕事，偷拿过后台钥匙。",
                Suspicion = "他在撒小谎，但没有改流程和删监控的权限。",
                PortraitPath = "Assets/HuaPi/Art/Characters/丑2.png"
            };
            _characters["zhoudai"] = new CharacterInfo
            {
                Id = "zhoudai",
                Name = "周岱",
                Role = "净角 / 道具执行者",
                Known = "负责戏服、脸谱和后台器材。",
                Suspicion = "签了出借记录，但一直暗示自己只是按上面命令办事。",
                PortraitPath = "Assets/HuaPi/Art/Characters/净2.png"
            };
            _characters["boss"] = new CharacterInfo
            {
                Id = "boss",
                Name = "薛万山",
                Role = "班主 / 管理者",
                Known = "控制复排流程，不许任何人提民国旧案。",
                Suspicion = "拥有改流程、调道具、删监控的完整权限。",
                PortraitPath = "Assets/HuaPi/Art/Characters/老板2.png"
            };
            _characters["faceless"] = new CharacterInfo
            {
                Id = "faceless",
                Name = "无脸人",
                Role = "旧案幸存者 / 被隐藏的人",
                Known = "只在证据和侧台阴影里出现，和民国火案幸存者高度重合。",
                Suspicion = "他可能不是凶手，而是被薛万山用来重演历史的人。",
                PortraitPath = "Assets/HuaPi/Art/Characters/木偶2.png"
            };
        }

        private void AddClue(string id, string name, string source, string keyword, string description)
        {
            _clues[id] = new Clue
            {
                Id = id,
                Name = name,
                Source = source,
                Keyword = keyword,
                Description = description
            };
        }

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("HuaPi Runtime Demo Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(canvasGo);
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9000;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            _root = canvasGo.GetComponent<RectTransform>();
        }

        private void Clear()
        {
            foreach (GameObject go in _spawned)
            {
                if (go != null) Destroy(go);
            }
            _spawned.Clear();
        }

        private GameObject Track(GameObject go)
        {
            _spawned.Add(go);
            return go;
        }

        private void ShowMainMenu()
        {
            _mode = ScreenMode.MainMenu;
            Clear();
            AddBackground("Assets/HuaPi/Art/Backgrounds/戏曲舞台.png");
            AddShade(0.78f);

            Text("画皮", 128, Red, TextAlignmentOptions.Center, new Vector2(0, 205), new Vector2(900, 170), _root);
            Text("现实，是历史的重演", 30, WhiteText, TextAlignmentOptions.Center, new Vector2(0, 105), new Vector2(900, 54), _root);

            ButtonText("开始游戏", new Vector2(0, -40), new Vector2(360, 62), () => ShowChapterTitle());
            ButtonText("继续游戏", new Vector2(0, -118), new Vector2(360, 62), () => ShowChapterTitle(), 0.72f);
            ButtonText("设置", new Vector2(0, -196), new Vector2(360, 62), ShowPauseOverlay, 0.72f);
            ButtonText("退出", new Vector2(0, -274), new Vector2(360, 62), () =>
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }, 0.55f);
        }

        private void ShowChapterTitle()
        {
            _mode = ScreenMode.ChapterTitle;
            Clear();
            AddBackground("Assets/HuaPi/Art/Backgrounds/舞台与幕后.png");
            AddShade(0.82f);
            Text("第一章", 32, Gold, TextAlignmentOptions.Center, new Vector2(0, 90), new Vector2(800, 60), _root);
            Text("第一张皮", 88, WhiteText, TextAlignmentOptions.Center, new Vector2(0, 0), new Vector2(900, 110), _root);
            Text("案件发生在民国。我要去的，却是今天仍在营业的薛氏剧团。", 26, MutedText, TextAlignmentOptions.Center, new Vector2(0, -95), new Vector2(1200, 56), _root);
            ButtonText("进入后台", new Vector2(0, -230), new Vector2(300, 62), () => ShowExploreProps());
        }

        private void ShowExploreProps()
        {
            _mode = ScreenMode.ExploreProps;
            _lastExploreMode = _mode;
            _objective = "调查陈列柜附近的异常";
            Clear();
            AddExplorationFrame("Assets/HuaPi/Art/Backgrounds/戏曲道具与盔头陈列室.png", "戏曲道具与盔头陈列室", _objective);
            AddCharacter(_characters["qingyi"], new Vector2(285, 445), new Vector2(390, 720), () => OpenDialogue("qingyi"));
            AddHotspot(new Hotspot { Id = "ticket_hotspot", Label = "旧戏箱", Position = new Vector2(730, 360), Size = new Vector2(150, 54), ClueId = "old_ticket", Feedback = "获得线索：旧戏箱里的旧戏票" });
            AddHotspot(new Hotspot { Id = "mask_hotspot", Label = "脸谱盒", Position = new Vector2(1060, 505), Size = new Vector2(160, 54), ClueId = "mask_box", Feedback = "获得线索：沾灰的脸谱盒" });
            AddHotspot(new Hotspot { Id = "door_hotspot", Label = "后门门闩", Position = new Vector2(1370, 635), Size = new Vector2(160, 54), ClueId = "door_scratch", Feedback = "获得线索：后台门闩划痕" });
            AddHotspot(new Hotspot { Id = "flow_hotspot", Label = "演出流程表", Position = new Vector2(1180, 310), Size = new Vector2(180, 54), ClueId = "show_flow", Feedback = "获得线索：演出流程表" });

            ButtonText("前往排练室", new Vector2(780, -455), new Vector2(250, 58), () =>
            {
                if (Has("old_ticket") && Has("mask_box") && Has("door_scratch") && Has("show_flow"))
                {
                    ShowExploreRehearsal();
                }
                else
                {
                    Toast("先调查完道具陈列室的关键线索。");
                }
            });
        }

        private void ShowExploreRehearsal()
        {
            _mode = ScreenMode.ExploreRehearsal;
            _lastExploreMode = _mode;
            _objective = "调查排练室，确认复排是否被人为改动";
            Clear();
            AddExplorationFrame("Assets/HuaPi/Art/Backgrounds/排练室.png", "排练室", _objective);
            AddCharacter(_characters["axi"], new Vector2(290, 470), new Vector2(380, 680), () => OpenDialogue("axi"));
            AddCharacter(_characters["zhoudai"], new Vector2(565, 470), new Vector2(390, 690), () => OpenDialogue("zhoudai"));
            AddCharacter(_characters["boss"], new Vector2(1580, 470), new Vector2(360, 660), () => OpenDialogue("boss"));

            AddHotspot(new Hotspot { Id = "schedule_hotspot", Label = "排练表", Position = new Vector2(830, 410), Size = new Vector2(160, 54), ClueId = "changed_schedule", Feedback = "获得线索：被改动的排练表" });
            AddHotspot(new Hotspot { Id = "record_hotspot", Label = "出借记录", Position = new Vector2(1090, 555), Size = new Vector2(170, 54), ClueId = "prop_record", Feedback = "获得线索：道具库出借记录" });
            AddHotspot(new Hotspot { Id = "video_hotspot", Label = "监控屏", Position = new Vector2(1330, 365), Size = new Vector2(160, 54), ClueId = "missing_video", Feedback = "获得线索：监控缺失片段" });

            ButtonText("开始推理", new Vector2(780, -455), new Vector2(250, 58), () =>
            {
                if (Has("changed_schedule") && Has("prop_record") && Has("missing_video"))
                {
                    ShowJudgement();
                }
                else
                {
                    Toast("还缺排练室的关键证据。");
                }
            });
        }

        private void ShowDayTwoTitle()
        {
            _mode = ScreenMode.DayTwoTitle;
            Clear();
            AddBackground("Assets/HuaPi/Art/Backgrounds/剧社侧台门帘处.png");
            AddShade(0.82f);
            Text("第二章", 32, Gold, TextAlignmentOptions.Center, new Vector2(0, 90), new Vector2(800, 60), _root);
            Text("封闭后台小屋", 86, WhiteText, TextAlignmentOptions.Center, new Vector2(0, 0), new Vector2(1100, 112), _root);
            Text("第一张皮揭开后，旧案没有结束。它开始在现实里继续往前走。", 26, MutedText, TextAlignmentOptions.Center, new Vector2(0, -95), new Vector2(1250, 56), _root);
            ButtonText("进入小屋", new Vector2(0, -230), new Vector2(300, 62), ShowExploreLockedRoom);
        }

        private void ShowExploreLockedRoom()
        {
            _mode = ScreenMode.ExploreLockedRoom;
            _lastExploreMode = _mode;
            _objective = "调查封闭小屋，确认旧案如何被现代复刻";
            Clear();
            AddExplorationFrame("Assets/HuaPi/Art/Backgrounds/储藏室.png", "封闭后台小屋", _objective);
            AddCharacter(_characters["boss"], new Vector2(1510, 470), new Vector2(360, 660), () => OpenDialogue("boss"));
            AddCharacter(_characters["zhoudai"], new Vector2(330, 470), new Vector2(390, 690), () => OpenDialogue("zhoudai"));
            AddHotspot(new Hotspot { Id = "burn_hotspot", Label = "门框焦痕", Position = new Vector2(760, 350), Size = new Vector2(170, 54), ClueId = "burn_mark", Feedback = "获得线索：新鲜焦痕" });
            AddHotspot(new Hotspot { Id = "note_hotspot", Label = "封门纸条", Position = new Vector2(1040, 455), Size = new Vector2(170, 54), ClueId = "sealed_room_note", Feedback = "获得线索：封门纸条" });
            AddHotspot(new Hotspot { Id = "key_hotspot", Label = "总钥匙", Position = new Vector2(1240, 630), Size = new Vector2(150, 54), ClueId = "manager_key", Feedback = "获得线索：后台总钥匙" });

            ButtonText("前往雅座", new Vector2(780, -455), new Vector2(250, 58), () =>
            {
                if (Has("burn_mark") && Has("sealed_room_note") && Has("manager_key"))
                {
                    ShowExploreAudience();
                }
                else
                {
                    Toast("封闭小屋里还有没有调查完的关键物。");
                }
            });
        }

        private void ShowExploreAudience()
        {
            _mode = ScreenMode.ExploreAudience;
            _lastExploreMode = _mode;
            _objective = "调查雅座，找出真正操纵重演的人";
            Clear();
            AddExplorationFrame("Assets/HuaPi/Art/Backgrounds/雅座.png", "上座观演雅座", _objective);
            AddCharacter(_characters["qingyi"], new Vector2(300, 470), new Vector2(390, 700), () => OpenDialogue("qingyi"));
            AddCharacter(_characters["faceless"], new Vector2(1550, 460), new Vector2(380, 650), () => OpenDialogue("faceless"));
            AddHotspot(new Hotspot { Id = "thread_hotspot", Label = "暗格细线", Position = new Vector2(760, 410), Size = new Vector2(170, 54), ClueId = "puppet_thread", Feedback = "获得线索：戏偶牵线" });
            AddHotspot(new Hotspot { Id = "register_hotspot", Label = "登记册", Position = new Vector2(1030, 570), Size = new Vector2(160, 54), ClueId = "guest_register", Feedback = "获得线索：雅座登记册" });
            AddHotspot(new Hotspot { Id = "photo_hotspot", Label = "民国旧照", Position = new Vector2(1280, 350), Size = new Vector2(170, 54), ClueId = "old_case_photo", Feedback = "获得线索：民国旧照" });

            ButtonText("最终指认", new Vector2(780, -455), new Vector2(250, 58), () =>
            {
                if (Has("puppet_thread") && Has("guest_register") && Has("old_case_photo"))
                {
                    ShowFinalTrial();
                }
                else
                {
                    Toast("雅座里还有关键线索没有拿到。");
                }
            });
        }

        private void AddExplorationFrame(string backgroundPath, string location, string objective)
        {
            AddBackground(backgroundPath);
            AddShade(0.58f);
            Text(location, 24, Gold, TextAlignmentOptions.Left, new Vector2(-855, 475), new Vector2(620, 44), _root);
            Text($"当前目标：{objective}", 32, WhiteText, TextAlignmentOptions.Left, new Vector2(-855, 424), new Vector2(920, 54), _root);
            ButtonText("线索", new Vector2(1440, 490), new Vector2(110, 44), ShowClueInventory, 0.8f);
            ButtonText("档案", new Vector2(1570, 490), new Vector2(110, 44), ShowArchive, 0.8f);
            ButtonText("暂停", new Vector2(1700, 490), new Vector2(110, 44), ShowPauseOverlay, 0.8f);
        }

        private void AddHotspot(Hotspot data)
        {
            var go = Track(new GameObject(data.Id, typeof(RectTransform), typeof(Image), typeof(Button)));
            go.transform.SetParent(_root, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = ToAnchored(data.Position);
            rect.sizeDelta = data.Size;
            var img = go.GetComponent<Image>();
            img.color = data.RequiredClueId != null && !Has(data.RequiredClueId)
                ? new Color(0.3f, 0.3f, 0.3f, 0.2f)
                : new Color(0.05f, 0.03f, 0.02f, 0.34f);
            img.raycastTarget = true;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = data.ClueId != null && Has(data.ClueId) ? new Color(0.5f, 0.5f, 0.5f, 0.35f) : new Color(0.78f, 0.66f, 0.43f, 0.85f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            Text(data.ClueId != null && Has(data.ClueId) ? "已调查" : data.Label, 20, data.ClueId != null && Has(data.ClueId) ? MutedText : Gold, TextAlignmentOptions.Center, Vector2.zero, data.Size, rect);
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (!string.IsNullOrEmpty(data.RequiredClueId) && !Has(data.RequiredClueId))
                {
                    Toast("现在还不能调查这里。");
                    return;
                }

                if (!string.IsNullOrEmpty(data.ClueId))
                {
                    AcquireClue(data.ClueId, data.Feedback);
                    if (_mode == ScreenMode.ExploreProps) ShowExploreProps();
                    if (_mode == ScreenMode.ExploreRehearsal) ShowExploreRehearsal();
                    if (_mode == ScreenMode.ExploreLockedRoom) ShowExploreLockedRoom();
                    if (_mode == ScreenMode.ExploreAudience) ShowExploreAudience();
                }
            });
        }

        private void AddCharacter(CharacterInfo info, Vector2 position, Vector2 size, Action onClick)
        {
            var go = Track(new GameObject($"{info.Id}_Character", typeof(RectTransform), typeof(Image), typeof(Button)));
            go.transform.SetParent(_root, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = ToAnchored(position);
            rect.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = LoadSprite(info.PortraitPath);
            img.preserveAspect = true;
            img.color = Color.white;
            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());

            var label = Text(info.Name, 22, Gold, TextAlignmentOptions.Center, new Vector2(0, size.y * 0.43f), new Vector2(240, 40), rect);
            label.gameObject.AddComponent<Outline>().effectColor = Color.black;
        }

        private void OpenDialogue(string characterId)
        {
            _activeCharacterId = characterId;
            CharacterInfo character = _characters[characterId];
            _mode = ScreenMode.Dialogue;
            Clear();
            string bg = BackgroundForExploreMode(_lastExploreMode);
            AddBackground(bg);
            AddShade(0.65f);
            AddCharacter(character, new Vector2(310, 450), new Vector2(420, 720), null);
            AddTopicPanel(characterId);
            AddDialogueBox(character.Name, new[] { GetGreeting(characterId) });
        }

        private string GetGreeting(string characterId)
        {
            switch (characterId)
            {
                case "qingyi": return "后台没有什么好拍的。人上台前，都不太像人。";
                case "axi": return "哟，新来的实习生。镜头这么新，是刚被班主骗进来的？";
                case "zhoudai": return "拍花絮的？别拍道具柜，乱。";
                case "boss": return "新来的，手脚勤快些。眼睛别太勤快。";
                case "faceless": return "你看见的不是脸，是别人替我留下来的名字。";
                default: return "……";
            }
        }

        private void AddTopicPanel(string characterId)
        {
            Text("话题", 24, Gold, TextAlignmentOptions.Left, new Vector2(690, 325), new Vector2(480, 42), _root);
            List<Topic> topics = GetTopics(characterId);
            for (int i = 0; i < topics.Count; i++)
            {
                Topic topic = topics[i];
                bool locked = !string.IsNullOrEmpty(topic.RequiredClueId) && !Has(topic.RequiredClueId);
                bool done = _askedTopics.Contains($"{characterId}:{topic.Label}");
                AddTopicButton(topic, new Vector2(690, 260 - i * 86), locked, done, () =>
                {
                    if (locked)
                    {
                        Toast("还缺能打开这个话题的证据。");
                        return;
                    }
                    _askedTopics.Add($"{characterId}:{topic.Label}");
                    if (!string.IsNullOrEmpty(topic.GrantsClueId))
                    {
                        AcquireClue(topic.GrantsClueId, $"获得线索：{_clues[topic.GrantsClueId].Name}", false);
                    }
                    if (!string.IsNullOrEmpty(topic.UnlocksObjective))
                    {
                        _objective = topic.UnlocksObjective;
                    }
                    AddDialogueBox(_characters[characterId].Name, topic.Lines);
                });
            }

            ButtonText("返回探索", new Vector2(785, -385), new Vector2(230, 54), () =>
            {
                ReturnToPreviousExplore();
            });
        }

        private List<Topic> GetTopics(string characterId)
        {
            switch (characterId)
            {
                case "qingyi":
                    return new List<Topic>
                    {
                        new Topic { Label = "今晚的《断面》", Subtitle = "普通话题", Lines = new [] { "戏没有想不想。锣鼓点响了，人就该站到该站的位置。", "剧团里所有人都是被安排好的。有人站台前，有人站幕后。" } },
                        new Topic { Label = "陈列柜里的脸谱", Subtitle = "警戒", RequiredClueId = "mask_box", Lines = new [] { "别拿出来。", "我怕有人看见它还在。能决定它该不该还在的人，就在这个剧团里。" } },
                        new Topic { Label = "昨晚你在哪里", Subtitle = "新解锁", RequiredClueId = "axi_testimony", Lines = new [] { "阿喜听错了。", "人害怕的时候，什么都听得见。", "我害怕今晚锣鼓点响得太准。" } },
                        new Topic { Label = "出示旧戏票", Subtitle = "证据出示", RequiredClueId = "old_ticket", Lines = new [] { "你不该捡它。", "这不是写给我的。可我知道它为什么会留下。" } }
                    };
                case "axi":
                    return new List<Topic>
                    {
                        new Topic { Label = "昨晚的后台", Subtitle = "破绽", RequiredClueId = "door_scratch", GrantsClueId = "axi_testimony", Lines = new [] { "行行行，我说一点，就一点。", "昨晚我确实在后台。我听见青衣姐在陈列柜前唱旧词，还看见班主从后门那边出来。" } },
                        new Topic { Label = "沈青衣", Subtitle = "普通话题", Lines = new [] { "她不怕鬼。她怕班主。", "有些人不是忘了才活着，是靠装作忘了才活着。" } },
                        new Topic { Label = "班主薛万山", Subtitle = "警戒", Lines = new [] { "他不怕旧案。", "他怕有人把旧案讲得太清楚。鬼故事没人信，证据有人信。" } },
                    };
                case "zhoudai":
                    return new List<Topic>
                    {
                        new Topic { Label = "旧道具是谁取的", Subtitle = "证据出示", RequiredClueId = "prop_record", Lines = new [] { "出借记录上不是写着吗？我签的。", "批准人是薛万山。那就说明不是我想拿。" } },
                        new Topic { Label = "临时改戏", Subtitle = "流程权", RequiredClueId = "changed_schedule", Lines = new [] { "原本今晚排《游园》。昨天下午才改成《断面》。", "能让所有人改排练的人，你说是谁？" } },
                        new Topic { Label = "监控缺失", Subtitle = "权限", RequiredClueId = "missing_video", Lines = new [] { "能看到，不代表能删。", "改流程、取道具、删监控、让所有人闭嘴，这些事不是靠手快，是靠位置高。" } },
                    };
                case "boss":
                    return new List<Topic>
                    {
                        new Topic { Label = "今晚为什么唱《断面》", Subtitle = "压迫", Lines = new [] { "剧团排什么，不用实习生过问。", "旧案资料埋死人，别拿来挡活人的路。" } },
                        new Topic { Label = "后台监控", Subtitle = "警戒", RequiredClueId = "missing_video", Lines = new [] { "系统故障。", "你还查了日志？后台不是你练胆子的地方。" } },
                        new Topic { Label = "失踪的人", Subtitle = "破绽", RequiredClueId = "old_ticket", Lines = new [] { "如果失踪的人还活着？", "那他也该学会像死人一样安静。" } },
                        new Topic { Label = "后台总钥匙", Subtitle = "证据压迫", RequiredClueId = "manager_key", Lines = new [] { "钥匙谁都能拿到。", "可钥匙上的旧印只有你在用。你把权限做成了家传的东西。", "少拿聪明当证据。聪明的人死得也不慢。" } },
                    };
                case "faceless":
                    return new List<Topic>
                    {
                        new Topic { Label = "你是谁", Subtitle = "普通话题", RequiredClueId = "old_case_photo", Lines = new [] { "我原本有名字。后来他们只记得我没有脸。", "薛家人把我藏在后台，说这样旧案就不会再有人认出来。" } },
                        new Topic { Label = "谁在重演历史", Subtitle = "破绽", RequiredClueId = "puppet_thread", Lines = new [] { "不是我。", "我没有让锣鼓点重响，也没有让门重新封上。牵线的人坐在高处，看每个人照旧案走。" } },
                        new Topic { Label = "封门纸条", Subtitle = "旧案回声", RequiredClueId = "sealed_room_note", Lines = new [] { "民国那夜，他们也写过这句话。", "当时是为了不让我出去。现在，是为了让你们相信我还会害人。" } },
                    };
                default:
                    return new List<Topic>();
            }
        }

        private void AddTopicButton(Topic topic, Vector2 position, bool locked, bool done, Action onClick)
        {
            var go = Track(new GameObject($"Topic_{topic.Label}", typeof(RectTransform), typeof(Image), typeof(Button)));
            go.transform.SetParent(_root, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(500, 68);
            go.GetComponent<Image>().color = locked ? new Color(0.45f, 0.40f, 0.34f, 0.75f) : (done ? new Color(0.55f, 0.49f, 0.40f, 0.72f) : Paper);
            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
            Text(topic.Label, 22, PaperText, TextAlignmentOptions.Left, new Vector2(22, 12), new Vector2(440, 30), rect);
            Text(locked ? "需要更多线索" : (done ? "已询问" : topic.Subtitle), 14, new Color(0.20f, 0.10f, 0.06f, 0.7f), TextAlignmentOptions.Left, new Vector2(22, -15), new Vector2(440, 24), rect);
        }

        private void AddDialogueBox(string speaker, string[] lines)
        {
            _activeSpeaker = speaker;
            _activeDialogueLines = lines;
            _dialogueLineIndex = 0;
            DrawDialogueLine();
        }

        private void DrawDialogueLine()
        {
            // Remove old dialogue layer only.
            foreach (GameObject go in _spawned.Where(g => g != null && g.name.StartsWith("DialogueLayer")).ToArray())
            {
                _spawned.Remove(go);
                Destroy(go);
            }

            var layer = Track(new GameObject("DialogueLayer", typeof(RectTransform)));
            layer.transform.SetParent(_root, false);
            Stretch(layer.GetComponent<RectTransform>());

            var bg = Image("DialogueBg", new Color(0.02f, 0.015f, 0.012f, 0.92f), new Vector2(0, -420), new Vector2(1920, 300), layer.transform);
            bg.transform.SetAsFirstSibling();
            Text(_activeSpeaker, 26, Gold, TextAlignmentOptions.Center, new Vector2(-220, -300), new Vector2(190, 54), layer.transform);
            Text("平静   警戒   破绽", 17, MutedText, TextAlignmentOptions.Left, new Vector2(-210, -350), new Vector2(360, 32), layer.transform);
            string current = _activeDialogueLines.Length > 0 ? _activeDialogueLines[Mathf.Clamp(_dialogueLineIndex, 0, _activeDialogueLines.Length - 1)] : "";
            Text(current, 34, WhiteText, TextAlignmentOptions.Left, new Vector2(220, -405), new Vector2(1160, 110), layer.transform);
            Text("〉", 30, Gold, TextAlignmentOptions.Center, new Vector2(855, -475), new Vector2(50, 50), layer.transform);
        }

        private void AdvanceDialogue()
        {
            if (_activeDialogueLines == null || _activeDialogueLines.Length == 0) return;
            _dialogueLineIndex++;
            if (_dialogueLineIndex >= _activeDialogueLines.Length)
            {
                _dialogueLineIndex = _activeDialogueLines.Length - 1;
                return;
            }
            DrawDialogueLine();
        }

        private void ShowClueInventory()
        {
            _mode = ScreenMode.ClueInventory;
            Clear();
            AddBackground("Assets/HuaPi/Art/Backgrounds/老旧大戏箱堆放角.png");
            AddShade(0.72f);
            Text("线索背包", 34, Gold, TextAlignmentOptions.Left, new Vector2(-830, 470), new Vector2(500, 58), _root);
            List<Clue> acquired = _clues.Values.Where(c => c.Acquired).ToList();
            if (acquired.Count == 0)
            {
                Text("还没有获得线索。", 32, WhiteText, TextAlignmentOptions.Center, Vector2.zero, new Vector2(600, 80), _root);
            }
            for (int i = 0; i < acquired.Count; i++)
            {
                Clue clue = acquired[i];
                AddPaperCard(clue.Name, clue.Keyword, new Vector2(-650 + (i % 4) * 430, 250 - (i / 4) * 170), () => ShowClueDetail(clue));
            }
            ButtonText("返回", new Vector2(780, -455), new Vector2(200, 56), ReturnToPreviousExplore);
        }

        private void ShowClueDetail(Clue clue)
        {
            Clear();
            AddBackground("Assets/HuaPi/Art/Backgrounds/老旧大戏箱堆放角.png");
            AddShade(0.72f);
            var panel = Image("PaperDetail", Paper, new Vector2(0, 0), new Vector2(1300, 650), _root);
            Text(clue.Name, 44, PaperText, TextAlignmentOptions.Left, new Vector2(-560, 245), new Vector2(1100, 70), panel.transform);
            Text($"来源：{clue.Source}\n关键词：{clue.Keyword}\n\n{clue.Description}", 28, PaperText, TextAlignmentOptions.TopLeft, new Vector2(0, -35), new Vector2(1120, 420), panel.transform);
            ButtonText("返回线索", new Vector2(780, -455), new Vector2(220, 56), ShowClueInventory);
        }

        private void ShowArchive()
        {
            _mode = ScreenMode.CharacterArchive;
            Clear();
            AddBackground("Assets/HuaPi/Art/Backgrounds/老旧大戏箱堆放角.png");
            AddShade(0.72f);
            Text("人物档案", 34, Gold, TextAlignmentOptions.Left, new Vector2(-830, 470), new Vector2(500, 58), _root);
            int index = 0;
            foreach (CharacterInfo c in _characters.Values)
            {
                AddArchiveCard(c, new Vector2(-610 + (index % 2) * 650, 190 - (index / 2) * 300));
                index++;
            }
            ButtonText("返回", new Vector2(780, -455), new Vector2(200, 56), ReturnToPreviousExplore);
        }

        private void AddArchiveCard(CharacterInfo c, Vector2 pos)
        {
            var card = Image($"Archive_{c.Id}", Paper, pos, new Vector2(580, 240), _root);
            var sprite = LoadSprite(c.PortraitPath);
            if (sprite != null)
            {
                var portrait = Image("Portrait", Color.white, new Vector2(-205, 0), new Vector2(150, 210), card.transform);
                portrait.sprite = sprite;
                portrait.preserveAspect = true;
            }
            Text($"{c.Name} · {c.Role}", 25, PaperText, TextAlignmentOptions.Left, new Vector2(85, 70), new Vector2(350, 44), card.transform);
            Text($"已知：{c.Known}\n疑点：{c.Suspicion}", 18, PaperText, TextAlignmentOptions.TopLeft, new Vector2(85, -28), new Vector2(350, 120), card.transform);
        }

        private void ShowJudgement()
        {
            _mode = ScreenMode.Judgement;
            Clear();
            AddBackground("Assets/HuaPi/Art/Backgrounds/排练室.png");
            AddShade(0.76f);
            AddHeartBar();
            Text("推理判定", 34, Gold, TextAlignmentOptions.Center, new Vector2(0, 120), new Vector2(800, 58), _root);
            Text("选择能证明“今晚不是正常复排”的证据", 42, WhiteText, TextAlignmentOptions.Center, new Vector2(0, 45), new Vector2(1200, 80), _root);

            string[] selectable = { "show_flow", "changed_schedule", "prop_record", "missing_video", "old_ticket", "mask_box" };
            var selected = new HashSet<string>();
            for (int i = 0; i < selectable.Length; i++)
            {
                string id = selectable[i];
                Clue clue = _clues[id];
                AddSelectableStrip(clue, new Vector2(-660 + (i % 3) * 440, -280 - (i / 3) * 86), selected);
            }
            ButtonText("提交", new Vector2(780, -455), new Vector2(220, 60), () =>
            {
                bool correct = selected.Contains("show_flow") && selected.Contains("changed_schedule") && selected.Contains("prop_record");
                if (correct)
                {
                    Toast("推理成立：历史重演是人为复刻。");
                    ShowObserveSkin();
                }
                else
                {
                    LoseHeart("这个证据组合还不足以证明复刻链条。");
                    if (_hearts <= 0) ShowFailure();
                    else ShowJudgement();
                }
            });
        }

        private void AddSelectableStrip(Clue clue, Vector2 pos, HashSet<string> selected)
        {
            var go = Track(new GameObject($"Selectable_{clue.Id}", typeof(RectTransform), typeof(Image), typeof(Button)));
            go.transform.SetParent(_root, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(400, 64);
            var img = go.GetComponent<Image>();
            img.color = Paper;
            Text(clue.Name, 21, PaperText, TextAlignmentOptions.Left, new Vector2(20, 10), new Vector2(350, 30), rect);
            Text(clue.Keyword, 14, new Color(0.20f, 0.10f, 0.06f, 0.65f), TextAlignmentOptions.Left, new Vector2(20, -17), new Vector2(350, 24), rect);
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (selected.Contains(clue.Id))
                {
                    selected.Remove(clue.Id);
                    img.color = Paper;
                }
                else
                {
                    selected.Add(clue.Id);
                    img.color = new Color(0.86f, 0.68f, 0.55f, 1f);
                }
            });
        }

        private void ShowObserveSkin()
        {
            _mode = ScreenMode.ObserveSkin;
            Clear();
            AddBackground("Assets/HuaPi/Art/Backgrounds/剧社侧台门帘处.png");
            AddShade(0.74f);
            AddHeartBar();
            Text("观皮", 30, Gold, TextAlignmentOptions.Left, new Vector2(-855, 475), new Vector2(400, 44), _root);
            Text("用 3 条证据揭开沈青衣的第一张皮", 36, WhiteText, TextAlignmentOptions.Left, new Vector2(-855, 424), new Vector2(1100, 54), _root);
            AddCharacter(_characters["qingyi"], new Vector2(960, 475), new Vector2(560, 720), null);

            string[] correct = { "old_ticket", "mask_box", "show_flow" };
            var selected = new HashSet<string>();
            string[] candidates = { "old_ticket", "mask_box", "show_flow", "axi_testimony", "door_scratch" };
            for (int i = 0; i < candidates.Length; i++)
            {
                AddSelectableStrip(_clues[candidates[i]], new Vector2(-650, 245 - i * 86), selected);
            }
            ButtonText("观皮", new Vector2(700, -345), new Vector2(260, 78), () =>
            {
                bool ok = correct.All(selected.Contains) && selected.Count == 3;
                if (ok)
                {
                    _characters["qingyi"].Revealed = true;
                    _clues["old_ticket"].Used = _clues["mask_box"].Used = _clues["show_flow"].Used = true;
                    ShowDayTwoTitle();
                }
                else
                {
                    LoseHeart("黑雾吞回了证据。这个组合还不能揭皮。");
                    if (_hearts <= 0) ShowFailure();
                    else ShowObserveSkin();
                }
            });
        }

        private void ShowFinalTrial()
        {
            _mode = ScreenMode.FinalTrial;
            Clear();
            AddBackground("Assets/HuaPi/Art/Backgrounds/戏曲舞台.png");
            AddShade(0.80f);
            AddHeartBar();
            Text("最终指认", 34, Gold, TextAlignmentOptions.Center, new Vector2(0, 130), new Vector2(800, 58), _root);
            Text("选择证明“民国旧案正在被现代人操控重演”的核心证据", 40, WhiteText, TextAlignmentOptions.Center, new Vector2(0, 54), new Vector2(1400, 80), _root);

            string[] selectable =
            {
                "manager_key", "sealed_room_note", "old_case_photo",
                "puppet_thread", "burn_mark", "guest_register",
                "missing_video", "axi_testimony", "door_scratch"
            };
            var selected = new HashSet<string>();
            for (int i = 0; i < selectable.Length; i++)
            {
                AddSelectableStrip(_clues[selectable[i]], new Vector2(-660 + (i % 3) * 440, -210 - (i / 3) * 86), selected);
            }

            ButtonText("提交指认", new Vector2(780, -455), new Vector2(240, 60), () =>
            {
                bool correct = selected.Contains("manager_key")
                    && selected.Contains("sealed_room_note")
                    && selected.Contains("old_case_photo")
                    && selected.Contains("puppet_thread");

                if (correct)
                {
                    _characters["faceless"].Revealed = true;
                    ShowEnding();
                }
                else
                {
                    LoseHeart("这个组合还没有同时锁定权限、旧案、封门和牵线。");
                    if (_hearts <= 0) ShowFailure();
                    else ShowFinalTrial();
                }
            });
        }

        private void ShowEnding()
        {
            _mode = ScreenMode.Ending;
            Clear();
            AddBackground("Assets/HuaPi/Art/Backgrounds/舞台与幕后.png");
            AddShade(0.82f);
            AddDialogueBox("侦探", new[]
            {
                "薛万山拥有改戏、取道具、删监控和打开封闭小屋的完整权限。",
                "民国旧照证明无脸人不是怪谈，而是当年火案的幸存者。",
                "封门纸条和新鲜焦痕证明旧案不是自然重现，是昨夜被重新布置。",
                "雅座暗格的牵线说明真正操纵重演的人一直坐在高处。",
                "现实不是历史的影子。现实，是有人逼它重演。"
            });
            ButtonText("回到主菜单", new Vector2(760, 430), new Vector2(260, 56), ShowMainMenu);
        }

        private void ShowFailure()
        {
            _mode = ScreenMode.Failure;
            Clear();
            AddBackground("Assets/HuaPi/Art/Backgrounds/舞台与幕后.png");
            AddShade(0.88f);
            AddHeartBar();
            Text("调查失败", 74, Red, TextAlignmentOptions.Center, new Vector2(0, 60), new Vector2(900, 100), _root);
            Text("班主发现你的身份异常，你被请离后台。", 30, WhiteText, TextAlignmentOptions.Center, new Vector2(0, -35), new Vector2(1000, 60), _root);
            ButtonText("从当天重试", new Vector2(0, -170), new Vector2(280, 62), () =>
            {
                _hearts = 5;
                ShowExploreProps();
            });
        }

        private void ShowPauseOverlay()
        {
            var overlay = Track(new GameObject("PauseOverlay", typeof(RectTransform), typeof(Image)));
            overlay.transform.SetParent(_root, false);
            Stretch(overlay.GetComponent<RectTransform>());
            overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
            Text("暂停", 54, Gold, TextAlignmentOptions.Center, new Vector2(0, 160), new Vector2(500, 80), overlay.transform);
            AddOverlayButton(overlay.transform, "继续", new Vector2(0, 50), () => DestroyTracked(overlay));
            AddOverlayButton(overlay.transform, "返回主菜单", new Vector2(0, -40), () => ShowMainMenu());
        }

        private void AddOverlayButton(Transform parent, string label, Vector2 pos, Action action)
        {
            var go = Track(new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button)));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(320, 60);
            go.GetComponent<Image>().color = new Color(0.05f, 0.035f, 0.03f, 0.78f);
            go.GetComponent<Button>().onClick.AddListener(() => action?.Invoke());
            Text(label, 26, WhiteText, TextAlignmentOptions.Center, Vector2.zero, rect.sizeDelta, rect);
        }

        private void ReturnToPreviousExplore()
        {
            switch (_lastExploreMode)
            {
                case ScreenMode.ExploreRehearsal:
                    ShowExploreRehearsal();
                    break;
                case ScreenMode.ExploreLockedRoom:
                    ShowExploreLockedRoom();
                    break;
                case ScreenMode.ExploreAudience:
                    ShowExploreAudience();
                    break;
                default:
                    ShowExploreProps();
                    break;
            }
        }

        private static string BackgroundForExploreMode(ScreenMode mode)
        {
            switch (mode)
            {
                case ScreenMode.ExploreRehearsal:
                    return "Assets/HuaPi/Art/Backgrounds/排练室.png";
                case ScreenMode.ExploreLockedRoom:
                    return "Assets/HuaPi/Art/Backgrounds/储藏室.png";
                case ScreenMode.ExploreAudience:
                    return "Assets/HuaPi/Art/Backgrounds/雅座.png";
                default:
                    return "Assets/HuaPi/Art/Backgrounds/戏曲道具与盔头陈列室.png";
            }
        }

        private void AcquireClue(string id, string message, bool redrawToast = true)
        {
            if (!_clues.TryGetValue(id, out Clue clue)) return;
            clue.Acquired = true;
            if (redrawToast) Toast(message);
        }

        private bool Has(string id)
        {
            return _clues.TryGetValue(id, out Clue clue) && clue.Acquired;
        }

        private void LoseHeart(string message)
        {
            _hearts = Mathf.Max(0, _hearts - 1);
            Toast($"心 -1：{message}");
        }

        private void AddHeartBar()
        {
            for (int i = 0; i < 5; i++)
            {
                var heart = Image($"Heart{i}", i < _hearts ? Red : new Color(0.28f, 0.25f, 0.23f, 0.45f), new Vector2(700 + i * 42, 475), new Vector2(28, 28), _root);
                heart.type = UnityEngine.UI.Image.Type.Simple;
            }
        }

        private void Toast(string text)
        {
            var toast = Image("Toast", Paper, new Vector2(0, -250), new Vector2(780, 58), _root);
            toast.color = new Color(Paper.r, Paper.g, Paper.b, 0.92f);
            Text(text, 24, PaperText, TextAlignmentOptions.Center, Vector2.zero, new Vector2(730, 50), toast.transform);
            Destroy(toast.gameObject, 2.2f);
        }

        private void AddPaperCard(string title, string subtitle, Vector2 pos, Action onClick)
        {
            var go = Track(new GameObject(title, typeof(RectTransform), typeof(Image), typeof(Button)));
            go.transform.SetParent(_root, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(360, 120);
            go.GetComponent<Image>().color = Paper;
            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
            Text(title, 26, PaperText, TextAlignmentOptions.Left, new Vector2(20, 26), new Vector2(310, 38), rect);
            Text(subtitle, 18, new Color(0.20f, 0.10f, 0.06f, 0.72f), TextAlignmentOptions.Left, new Vector2(20, -20), new Vector2(310, 32), rect);
        }

        private Image AddBackground(string assetPath)
        {
            Image bg = Image("Background", Color.white, Vector2.zero, new Vector2(1920, 1080), _root);
            bg.sprite = LoadSprite(assetPath);
            bg.preserveAspect = false;
            bg.raycastTarget = false;
            bg.transform.SetAsFirstSibling();
            return bg;
        }

        private void AddShade(float alpha)
        {
            Image shade = Image("Shade", new Color(0, 0, 0, alpha), Vector2.zero, new Vector2(1920, 1080), _root);
            shade.raycastTarget = false;
        }

        private Image Image(string name, Color color, Vector2 anchoredPosition, Vector2 size, Transform parent)
        {
            var go = Track(new GameObject(name, typeof(RectTransform), typeof(Image)));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private TMP_Text Text(string content, float size, Color color, TextAlignmentOptions alignment, Vector2 anchoredPosition, Vector2 rectSize, Transform parent)
        {
            var go = Track(new GameObject($"Text_{content}", typeof(RectTransform), typeof(TextMeshProUGUI)));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = rectSize;
            TMP_Text text = go.GetComponent<TMP_Text>();
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            return text;
        }

        private void ButtonText(string label, Vector2 anchoredPosition, Vector2 size, Action onClick, float opacity = 0.92f)
        {
            var go = Track(new GameObject($"Button_{label}", typeof(RectTransform), typeof(Image), typeof(Button)));
            go.transform.SetParent(_root, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.05f, 0.035f, 0.03f, opacity);
            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(Gold.r, Gold.g, Gold.b, 0.32f);
            outline.effectDistance = new Vector2(1f, -1f);
            Text(label, 24, Gold, TextAlignmentOptions.Center, Vector2.zero, size, rect);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private Vector2 ToAnchored(Vector2 designPosition)
        {
            return new Vector2(designPosition.x - 960f, designPosition.y - 540f);
        }

        private void DestroyTracked(GameObject go)
        {
            _spawned.Remove(go);
            Destroy(go);
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            DontDestroyOnLoad(go);
        }

        private static bool WasEscapePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }

        private static bool WasAdvancePressed()
        {
#if ENABLE_INPUT_SYSTEM
            bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
            return mousePressed || spacePressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
#else
            return false;
#endif
        }

        private static Color Hex(int rgb, float alpha = 1f)
        {
            return new Color(((rgb >> 16) & 0xff) / 255f, ((rgb >> 8) & 0xff) / 255f, (rgb & 0xff) / 255f, alpha);
        }

        private static Sprite LoadSprite(string assetPath)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
#else
            return null;
#endif
        }
    }
}
