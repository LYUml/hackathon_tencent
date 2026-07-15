using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TXGame
{
    /// <summary>
    /// Builds the complete temporary game UI inside the existing UI_Root canvas stack.
    /// This is the integration layer for the current playable scene; final art can replace
    /// the generated Image/Text nodes without changing the scene flow.
    /// </summary>
    public class HuapiFullUIInstaller : MonoBehaviour
    {
        public static bool IsInstalled { get; private set; }

        private enum View
        {
            MainMenu,
            Intro,
            Monologue,
            Hud,
            CrimeScene,
            SceneMap,
            Guide,
            Credits,
            Dialogue,
            Clues,
            Archive,
            Observe,
            Trial,
            SaveLoad,
            Pause,
            DayTransition,
            GameOver,
            Ending
        }

        private sealed class Clue
        {
            public string Id;
            public string Name;
            public string Tag;
            public string Source;
            public string Detail;
            public bool Acquired;
            public bool Used;
        }

        private sealed class Character
        {
            public string Id;
            public string Name;
            public string Role;
            public string Known;
            public string Suspicion;
            public float Reveal;
        }

        private sealed class IntroSlide
        {
            public string Title;
            public string Body;
            public string BackgroundPath;
            public string PortraitPath;
            public string Caption;
        }

        private sealed class InvestigationHotspot
        {
            public string Id;
            public string Name;
            public string Description;
            public string ClueId;
            public string SceneId;
            public string OverlayPath;
            public Vector2 Position;
            public Vector2 Size;
            public Vector2 OverlayPosition;
            public Vector2 OverlaySize;
            public bool SecondDayOnly;
            public bool Puzzle;
            public string PuzzleKind;
            public string PuzzleHint;
            public bool Examined;
        }

        private sealed class SceneActor
        {
            public string Id;
            public string Name;
            public string PortraitPath;
            public string SceneId;
            public Vector2 Position;
            public Vector2 Size;
        }

        private sealed class SceneNode
        {
            public string Id;
            public string Name;
            public string BackgroundPath;
            public string Description;
            public string Front;
            public string Back;
            public string Left;
            public string Right;
            public Vector2 MapPosition;
        }

        private sealed class DragSlot
        {
            public string Label;
            public Vector2 Position;
            public bool Occupied;
        }

        private sealed class DialogueTopic
        {
            public string Id;
            public string Label;
            public string Question;
            public string Response;
            public bool SecondDayOnly;
        }

        private readonly List<GameObject> spawned = new List<GameObject>();
        private readonly Dictionary<string, Clue> clues = new Dictionary<string, Clue>();
        private readonly Dictionary<string, Character> characters = new Dictionary<string, Character>();
        private readonly Dictionary<string, SceneNode> sceneNodes = new Dictionary<string, SceneNode>();
        private readonly HashSet<string> askedDialogueTopics = new HashSet<string>();
        private readonly List<SceneActor> sceneActors = new List<SceneActor>();
        private Canvas hudCanvas;
        private Canvas dialogueCanvas;
        private Canvas normalCanvas;
        private Canvas popupCanvas;
        private Canvas revealCanvas;
        private Canvas systemCanvas;
        private RectTransform currentLayer;
        private Image fadeOverlay;
        private bool isTransitioning;
        private int hearts = 5;
        private int introIndex;
        private int monologueIndex;
        private bool evidenceBoardVisible;
        private IntroSlide[] introSlides;
        private IntroSlide[] monologueSlides;
        private InvestigationHotspot[] crimeSceneHotspots;
        private string currentSceneId = "props";
        private int facePuzzleStep;
        private int latchPuzzleStep;
        private int gongPuzzleStep;
        private int wirePuzzleStep;
        private int casePuzzleStep;
        private int dialogueBeat;
        private int currentDay = 1;
        private bool isNightPhase;
        private bool dayOneTrialComplete;
        private bool dayTwoNightStarted;
        private bool demoCompleted;
        private const string PropsRoomBackground = "Assets/Art/Sprites/Backgrounds/bg_props_room_panorama.png";
        private string gameplayBackgroundPath = "Assets/Art/Sprites/Generated/generated_theater_exterior.png";
        private View currentView = View.MainMenu;
        private AudioSource uiAudioSource;
        private AudioSource bgmAudioSource;
        private AudioClip clickClip;
        private AudioClip hintClip;
        private bool pauseOpenedFromMainMenu;
        private bool guideOpenedFromMainMenu;
        private float musicVolume = 0.75f;
        private float sfxVolume = 0.48f;
        private float textSpeed = 0.5f;
        private static Font runtimeFont;
        private static TMP_FontAsset runtimeTmpFont;
        private float nextTmpFontRefreshTime;
        private const int DefaultChineseFontSize = 32;
        private const int DefaultEnglishFontSize = 26;
        private const int StoryBodyFontSize = 32;
        private const int StoryBodyMinimumFontSize = 30;

        private static readonly Color Clear = new Color(1f, 1f, 1f, 0f);
        private static readonly Color StoryBackdrop = new Color(0.015f, 0.012f, 0.010f, 0.78f);
        private static readonly Color StoryShade = new Color(0f, 0f, 0f, 0.30f);
        private static readonly Color Ink = new Color(0.015f, 0.012f, 0.010f, 0.86f);
        private static readonly Color Dark = new Color(0.09f, 0.055f, 0.040f, 0.92f);
        private static readonly Color Paper = Hex(0xD8C7A7);
        private static readonly Color PaperText = Hex(0x2D1710);
        private static readonly Color Gold = Hex(0xC9A96E);
        private static readonly Color Red = Hex(0x8B2020);
        private static readonly Color White = Hex(0xF1E8D8);
        private static readonly Color Muted = Hex(0xBDAF9A);
        private static readonly Vector2 StoryPrimaryButtonPos = new Vector2(720f, -430f);
        private static readonly Vector2 StorySecondaryButtonPos = new Vector2(500f, -430f);
        private const string UI2026 = "Assets/Art/Sprites/UI2026/";
        private const string UIHome = UI2026 + "01home/";
        private const string UIHomeIcons = UIHome + "01icons/";
        private const string UIWindow = UIHome + "01widnow_icons/";
        private const string UISettings = UI2026 + "02settings/";
        private const string UITutorial = UI2026 + "03tutorial/";
        private const string UICredits = UI2026 + "04credits/";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindObjectOfType<HuapiFullUIInstaller>() != null) return;
            GameObject root = GameObject.Find("UI_Root") ?? new GameObject("UI_Root");
            root.AddComponent<HuapiFullUIInstaller>();
        }

        private void Awake()
        {
            IsInstalled = true;
            SuppressLegacyRuntimeUi();
            ConfigureFullscreenTestMode();
            EnsureEventSystem();
            HideLegacySceneBackground();
            SeedData();
            ResolveCanvases();
            ApplyRuntimeFontToAllTmpTexts();
            SetupAudio();
            ShowMainMenu();
        }

        private void SuppressLegacyRuntimeUi()
        {
            MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null || behaviour == this) continue;
                Type type = behaviour.GetType();
                string fullName = type.FullName;
                if (fullName == "HuaPi.Demo.HuaPiDemoGameController" ||
                    fullName == "HuaPi.UI.Core.UIManager")
                {
                    behaviour.enabled = false;
                }
            }
        }

        private void OnDestroy()
        {
            if (IsInstalled) IsInstalled = false;
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (Keyboard.current == null) return;
            if (isTransitioning) return;

            if (currentView == View.MainMenu || currentView == View.Intro) return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (currentView == View.Guide)
                {
                    CloseGuide();
                    return;
                }

                if (currentView == View.Credits)
                {
                    TransitionTo(ShowMainMenu);
                    return;
                }
            }

            if (currentView == View.CrimeScene)
            {
                SceneNode scene = CurrentScene();
                if (Keyboard.current.upArrowKey.wasPressedThisFrame) MoveScene(scene.Front, "前");
                if (Keyboard.current.downArrowKey.wasPressedThisFrame) MoveScene(scene.Back, "后");
                if (Keyboard.current.leftArrowKey.wasPressedThisFrame) MoveScene(scene.Left, "左");
                if (Keyboard.current.rightArrowKey.wasPressedThisFrame) MoveScene(scene.Right, "右");
            }

            if (Keyboard.current.f1Key.wasPressedThisFrame) TransitionTo(ShowGuide);
            if (Keyboard.current.cKey.wasPressedThisFrame) ToggleClues();
            if (Keyboard.current.mKey.wasPressedThisFrame) ToggleSceneMap();
            if (Keyboard.current.xKey.wasPressedThisFrame) TransitionTo(ShowArchive);
            if (Keyboard.current.vKey.wasPressedThisFrame && HasClue("ticket")) TransitionTo(() => ShowPhaseTransition("擦雾", "FOG WIPE", "用证据擦开人物表层的雾。", "开始擦雾", ShowObserve));
            if (Keyboard.current.tKey.wasPressedThisFrame && CanOpenTrial()) TransitionTo(OpenCurrentTrial);
            if (Keyboard.current.escapeKey.wasPressedThisFrame) TogglePause();
        }

        private void LateUpdate()
        {
            if (Time.unscaledTime < nextTmpFontRefreshTime) return;
            nextTmpFontRefreshTime = Time.unscaledTime + 0.25f;
            ApplyRuntimeFontToAllTmpTexts();
        }

        private void SeedData()
        {
            SeedIntro();
            SeedMonologue();

            AddClue("ticket", "旧戏票", "青衣勿忘", "道具陈列室", "票根背面写着“青衣勿忘”，失踪旦角在开演前曾把这张票塞进戏箱夹层。");
            AddClue("mask", "烧焦脸谱", "残片", "脸谱盒", "脸谱盒夹层里藏着火案残片和半截花旦簪脚，说明有人长期保存旧案遗物。");
            AddClue("flow", "演出流程表", "流程一致", "后台公告板", "今晚《断面》的锣鼓点、登台顺序与陈伶宜失踪当夜一致。");
            AddClue("record", "道具出借记录", "班主批准", "排练室", "旧门闩、旧戏服、脸谱残片和木偶箱均由薛万山批准取出。");
            AddClue("video", "更夫空白签", "三刻钟", "更房巡夜簿", "戌时三刻到四刻的巡夜签名被空开，说明有人让更夫离开了后台廊口。");
            AddClue("key", "后台总钥匙", "薛氏印章", "封闭小屋", "钥匙能打开道具库、账房和封闭后台小屋，钥匙链完整。");
            AddClue("photo", "旧照", "没有脸的人", "雅座暗格", "旧照上有花旦陈伶宜和木偶师何颂生，旁边的班主被墨点抹去半张脸。");
            AddClue("thread", "戏偶牵线", "高处操控", "雅座", "细线从雅座通往侧台，也能牵动木偶箱机关。真正操控重演的人一直坐在高处。");
            AddClue("bodymark", "倒地痕迹", "非自然摔倒", "侧台门帘前地板", "侧台门帘前有一道向里拖拽的暗痕，钟铁面并不是自己绊倒，而是被人借锣声遮住了倒地声。");
            AddClue("latch", "反扣门闩", "外侧锁定", "后台侧门", "门闩从内侧看似合上，但木屑集中在外侧，说明有人用细线或薄片从门外复原了锁门状态。");
            AddClue("ash", "新鲜香灰", "伪造旧火", "烧焦脸谱盒", "香灰还带温热，和梨园旧火案遗物混在一起，是有人故意制造“鬼火重临”的气味。");
            AddClue("gong", "错位锣点", "提前三拍", "锣鼓公告板", "排练锣点被提前三拍，刚好能盖住侧台门闩落下和人体倒地的两声。");
            AddClue("medicine", "嗓子药瓶", "镇静成分", "化妆台边缘", "瓶身写着润喉，瓶底却残留镇静药粉，足够让人短时间眩晕失力。");
            AddClue("wire", "吊景细线", "高处牵引", "幕布滑轨", "细线绕过滑轨通向雅座方向，可以远距离牵动门闩和幕布。");
            AddClue("curtain_thread", "新线头", "第二天出现", "侧台门帘", "第一天只看到磨痕，第二天夜里门帘内侧多出新线头，还缠着一小片发簪珠花。");
            AddClue("night_gong", "夜场锣槌", "夜锣再错", "戏曲舞台", "第二天夜晚锣槌位置被调换，锣点再次提前，和第一天案发时的遮声手法一致。");
            AddClue("case_seal", "木偶箱封条", "浆糊未干", "老旧戏箱", "旧戏箱封条被新糊过，箱内有一对定情木偶和残缺招魂符，证明旧案并不止火灾。");

            SeedCrimeScene();
            SeedSceneMap();
            SeedSceneActors();

            characters["qingyi"] = new Character { Id = "qingyi", Name = "沈青衣", Role = "旦角 / 台柱", Known = "回避旧案，对脸谱盒和旧戏票反应异常。", Suspicion = "知道门闩和旧火案细节，但不是机关操控者。", Reveal = 0.45f };
            characters["axi"] = new Character { Id = "axi", Name = "阿喜", Role = "丑角 / 误导嫌疑人", Known = "嘴碎怕事，偷拿过后台钥匙。", Suspicion = "撒了小谎，但没有改锣点和调走更夫的本事。", Reveal = 0.15f };
            characters["boss"] = new Character { Id = "boss", Name = "薛万山", Role = "班主 / 管理者", Known = "控制夜场、道具、账房和封闭小屋。", Suspicion = "与陈伶宜失踪当夜的后台钥匙记录有关。", Reveal = 0.75f };
            characters["faceless"] = new Character { Id = "faceless", Name = "何颂生", Role = "木偶师 / 旧案遗留者", Known = "传闻他为失踪花旦做过一对定情木偶。", Suspicion = "像被旧案逼疯的人，也像被人利用的证人。", Reveal = 0.6f };
        }

        private void AddClue(string id, string name, string tag, string source, string detail)
        {
            clues[id] = new Clue { Id = id, Name = name, Tag = tag, Source = source, Detail = detail };
        }

        private void SeedCrimeScene()
        {
            crimeSceneHotspots = new[]
            {
                new InvestigationHotspot
                {
                    Id = "body",
                    Name = "倒地位置",
                    ClueId = "bodymark",
                    SceneId = "side",
                    OverlayPath = "Assets/Art/Sprites/Overlays/fallen_performer_floor_overlay.png",
                    Position = new Vector2(-255, -410),
                    Size = new Vector2(560, 150),
                    OverlayPosition = new Vector2(-255, -410),
                    OverlaySize = new Vector2(580, 160),
                    Puzzle = true,
                    PuzzleKind = "body",
                    PuzzleHint = "先看面具朝向，再看拖痕终点，最后对照台口方向。",
                    Description = "钟铁面的面具歪在侧台门帘前，身体被抬走后只留下暗色拖痕。痕迹从帘后向台口延伸，不像自己摔倒。"
                },
                new InvestigationHotspot
                {
                    Id = "door",
                    Name = "侧门门闩",
                    ClueId = "latch",
                    SceneId = "props",
                    Position = new Vector2(470, -40),
                    Size = new Vector2(86, 86),
                    Puzzle = true,
                    PuzzleKind = "latch",
                    PuzzleHint = "门外能做到的顺序是先穿线，再压住门闩，最后回抽伪装反扣。",
                    Description = "门闩表面干净，缝隙里却有新木屑。像是有人在门外完成了一个“屋内反锁”的假象。"
                },
                new InvestigationHotspot
                {
                    Id = "maskbox",
                    Name = "脸谱盒",
                    ClueId = "ash",
                    SceneId = "props",
                    Position = new Vector2(-110, 135),
                    Size = new Vector2(94, 94),
                    Puzzle = true,
                    PuzzleKind = "face",
                    PuzzleHint = "旧戏单上的行当顺序能决定脸谱颜色，白脸不在台前。",
                    Description = "烧焦脸谱被摆在盒子最上层，旁边的香灰还带一点温。旧案遗物不该有今天的温度。"
                },
                new InvestigationHotspot
                {
                    Id = "gong",
                    Name = "锣鼓板",
                    ClueId = "gong",
                    SceneId = "props",
                    Position = new Vector2(380, 220),
                    Size = new Vector2(96, 86),
                    Puzzle = true,
                    PuzzleKind = "gong",
                    PuzzleHint = "短声遮开门，长声遮倒地，停拍遮脚步；公告板写着重复的短声。",
                    Description = "公告板上的锣点被铅笔改过。提前三拍，刚好能让一切杂音淹在练声和锣鼓里。"
                },
                new InvestigationHotspot
                {
                    Id = "medicine",
                    Name = "化妆台药瓶",
                    ClueId = "medicine",
                    SceneId = "props",
                    Position = new Vector2(-640, 220),
                    Size = new Vector2(90, 90),
                    Puzzle = true,
                    PuzzleKind = "medicine",
                    PuzzleHint = "药瓶、喝水处、白色粉末三者能对应上，先确认瓶底残留。",
                    Description = "药瓶标签写着润喉，瓶底却粘着白色粉末。钟铁面刚才说嗓子干，喝过这里的水。"
                },
                new InvestigationHotspot
                {
                    Id = "wire",
                    Name = "幕布滑轨",
                    ClueId = "wire",
                    SceneId = "props",
                    Position = new Vector2(90, 300),
                    Size = new Vector2(100, 72),
                    Puzzle = true,
                    PuzzleKind = "wire",
                    PuzzleHint = "线从上轨经过侧轮，绕到雅座铜环，最后才牵动门闩。",
                    Description = "细线从滑轨后面垂下，末端被剪断。线的方向通向二楼雅座，不像临时失误。"
                },
                new InvestigationHotspot
                {
                    Id = "side_curtain",
                    Name = "侧台门帘",
                    SceneId = "side",
                    ClueId = "curtain_thread",
                    Position = new Vector2(-170, -20),
                    Size = new Vector2(360, 480),
                    SecondDayOnly = true,
                    Puzzle = true,
                    PuzzleKind = "curtain",
                    PuzzleHint = "第二天新出现的线头，要和第一天看到的磨痕、珠花位置对上。",
                    Description = "门帘内侧有细小磨痕，像是反复被线拉过。第一天只能确认方向，第二天夜里这里会出现新的线头。"
                },
                new InvestigationHotspot
                {
                    Id = "rehearsal_board",
                    Name = "排练公告板",
                    SceneId = "rehearsal",
                    ClueId = "flow",
                    Position = new Vector2(180, 115),
                    Size = new Vector2(420, 240),
                    Puzzle = true,
                    PuzzleKind = "schedule",
                    PuzzleHint = "旧案记录、今日流程、夜场改锣点是同一条时间线。",
                    Description = "公告板上写着今日夜场流程。登台顺序和旧案记录高度一致，像是有人故意照着旧案排。"
                },
                new InvestigationHotspot
                {
                    Id = "stage_gong",
                    Name = "舞台锣位",
                    SceneId = "stage",
                    ClueId = "night_gong",
                    Position = new Vector2(320, -40),
                    Size = new Vector2(300, 260),
                    SecondDayOnly = true,
                    Puzzle = true,
                    PuzzleKind = "gong",
                    PuzzleHint = "夜场锣位对应后台声音，顺着短、长、停、短复原。",
                    Description = "锣槌放得很端正。第一天只能确认这里能盖住后台声音；第二天夜场开锣后，锣点会再次变化。"
                },
                new InvestigationHotspot
                {
                    Id = "seat_ring",
                    Name = "雅座铜环",
                    SceneId = "seat",
                    ClueId = "thread",
                    Position = new Vector2(210, -120),
                    Size = new Vector2(240, 220),
                    Puzzle = true,
                    PuzzleKind = "ring",
                    PuzzleHint = "铜环不是装饰，它把滑轨上的细线转向门闩。",
                    Description = "雅座扶手下有一个不起眼的铜环，方向正对舞台滑轨。它不像装饰，更像牵线转向点。"
                },
                new InvestigationHotspot
                {
                    Id = "storage_case",
                    Name = "旧戏箱封条",
                    SceneId = "storage",
                    ClueId = "case_seal",
                    Position = new Vector2(-240, -80),
                    Size = new Vector2(520, 320),
                    SecondDayOnly = true,
                    Puzzle = true,
                    PuzzleKind = "case",
                    PuzzleHint = "封条方向不是随机的，旧戏箱上的方位要按东、南、西、北复原。",
                    Description = "封条新旧混在一起，有一张浆糊还没干透。第一天缺少钥匙，第二天才能开箱检查。"
                }
            };
        }

        private void SeedSceneMap()
        {
            sceneNodes.Clear();
            sceneNodes["props"] = new SceneNode
            {
                Id = "props",
                Name = "道具陈列室",
                BackgroundPath = PropsRoomBackground,
                Description = "第一案现场。盔头、脸谱盒、药瓶和门闩都在这里。",
                Front = "stage",
                Back = "storage",
                Left = "side",
                Right = "rehearsal",
                MapPosition = new Vector2(0, 30)
            };
            sceneNodes["side"] = new SceneNode
            {
                Id = "side",
                Name = "侧台门帘",
                BackgroundPath = "Assets/Art/Sprites/Backgrounds/剧社侧台门帘处.png",
                Description = "通往舞台和后台的交界处，适合观察人流。",
                Front = "stage",
                Back = "props",
                Right = "props",
                MapPosition = new Vector2(-260, 30)
            };
            sceneNodes["rehearsal"] = new SceneNode
            {
                Id = "rehearsal",
                Name = "排练室",
                BackgroundPath = "Assets/Art/Sprites/Backgrounds/排练室.png",
                Description = "演员对词和改锣点的地方，公告板线索来自这里。",
                Back = "props",
                Left = "props",
                MapPosition = new Vector2(260, 30)
            };
            sceneNodes["stage"] = new SceneNode
            {
                Id = "stage",
                Name = "戏曲舞台",
                BackgroundPath = "Assets/Art/Sprites/Backgrounds/戏曲舞台.png",
                Description = "今晚夜场《断面》的主舞台，锣鼓声会遮住后台杂音。",
                Back = "props",
                Left = "side",
                Right = "seat",
                MapPosition = new Vector2(0, 230)
            };
            sceneNodes["seat"] = new SceneNode
            {
                Id = "seat",
                Name = "上座雅座",
                BackgroundPath = "Assets/Art/Sprites/Backgrounds/上座观演雅座.png",
                Description = "高处视角，细线方向最终通往这里。",
                Back = "stage",
                Left = "stage",
                MapPosition = new Vector2(260, 230)
            };
            sceneNodes["storage"] = new SceneNode
            {
                Id = "storage",
                Name = "老旧戏箱堆放角",
                BackgroundPath = "Assets/Art/Sprites/Backgrounds/老旧大戏箱堆放角.png",
                Description = "旧戏箱和民国遗物集中堆放的角落。",
                Front = "props",
                MapPosition = new Vector2(0, -170)
            };
        }

        private void SeedSceneActors()
        {
            sceneActors.Clear();
            sceneActors.Add(new SceneActor
            {
                Id = "qingyi",
                Name = "沈青衣",
                PortraitPath = "Assets/Art/Sprites/Characters/旦2.png",
                SceneId = "stage",
                Position = new Vector2(-640, -210),
                Size = new Vector2(300, 560)
            });
            sceneActors.Add(new SceneActor
            {
                Id = "axi",
                Name = "阿喜",
                PortraitPath = "Assets/Art/Sprites/Characters/丑2.png",
                SceneId = "side",
                Position = new Vector2(-660, -210),
                Size = new Vector2(300, 560)
            });
            sceneActors.Add(new SceneActor
            {
                Id = "boss",
                Name = "薛万山",
                PortraitPath = "Assets/Art/Sprites/Characters/老板2.png",
                SceneId = "rehearsal",
                Position = new Vector2(-655, -205),
                Size = new Vector2(315, 575)
            });
        }

        private void SeedIntro()
        {
            introSlides = new[]
            {
                new IntroSlide
                {
                    Title = "你是被请来的侦探",
                    Caption = "民国二十三年 / 薛氏戏园",
                    BackgroundPath = "Assets/Art/Sprites/Generated/generated_theater_exterior.png",
                    PortraitPath = "",
                    Body = "你是巡捕房外聘的私家侦探，专查那些不能只靠口供结案的奇案。\n\n今晚，薛氏班在戏园重演失传剧目《断面》。班主派人送来一封没有署名的请帖，只写了地点、时辰，以及一句话：\n\n“别让那场火，再烧一次。”"
                },
                new IntroSlide
                {
                    Title = "旧案未冷，新戏将开",
                    Caption = "第一天 · 白天 / 戏曲舞台",
                    BackgroundPath = "Assets/Art/Sprites/Generated/generated_opera_stage.png",
                    PortraitPath = "Assets/Art/Sprites/Characters/老板2.png",
                    Body = "十七年前，薛氏班在同一座戏台演《断面》。锣鼓点错三声，后台起火，花旦陈伶宜失踪，木偶师何颂生也从此疯癫。\n\n案卷最终以“灯油意外”结案。可今天白天的排练流程、开场锣点、道具出借记录，正一项一项复刻当年的案发顺序。夜场还没开始，你还有时间把人和物先查清楚。"
                },
                new IntroSlide
                {
                    Title = "戏，是旧案的重演",
                    Caption = "后台 / 老旧大戏箱",
                    BackgroundPath = "Assets/Art/Sprites/Generated/generated_props_crime_scene.png",
                    PortraitPath = "Assets/Art/Sprites/Characters/木偶2.png",
                    Body = "你在后台戏箱里看见三样不该同时出现的东西：烧焦的脸谱残片、旧戏票、写着“青衣勿忘”的封门纸。\n\n箱底还有一对木偶的压痕，像有人曾把它们当作定情信物，也像有人后来拿它们做过别的仪式。"
                },
                new IntroSlide
                {
                    Title = "第一天：白天探索",
                    Caption = "进入剧团 / 调查开始",
                    BackgroundPath = "Assets/Art/Sprites/Generated/generated_backstage_corridor.png",
                    PortraitPath = "Assets/Art/Sprites/Characters/旦2.png",
                    Body = "你要做的不是证明鬼怪存在，而是拆穿“鬼怪”需要谁的钥匙、谁的机关、谁的沉默。\n\n现在是第一天白天：先从道具陈列室、侧台门帘、排练室和雅座开始自由探索。夜晚会在关键线索收集后再推进，不会开场直接跳夜晚。"
                }
            };
        }

        private void SeedMonologue()
        {
            monologueSlides = new[]
            {
                new IntroSlide
                {
                    Title = "侦探独白",
                    Caption = "戏园外 / 第一天下午",
                    BackgroundPath = "Assets/Art/Sprites/Generated/generated_theater_exterior.png",
                    Body = "我到的时候，戏园外的雨刚停。\n\n门口没有围观的人，也没有巡捕常见的封锁绳。只有夜场戏报贴在玻璃里，像一张刚刚被擦干净的脸。\n\n送帖的人没有留名。可越是不留名的委托，越说明有人不想让这件事进巡捕房的正式案册。"
                },
                new IntroSlide
                {
                    Title = "侦探独白",
                    Caption = "戏园外 / 案件判断",
                    BackgroundPath = "Assets/Art/Sprites/Generated/generated_theater_exterior.png",
                    Body = "“旧戏，是旧案的重演。”\n\n这句话听起来像怪谈，实际上更像一种操作手法。只要有人掌握流程、道具、钥匙和沉默的人，旧案就能被重新排出来。\n\n我现在还不能判断谁是凶手，但我可以先判断一件事：今晚的戏，不是单纯的戏。"
                },
                new IntroSlide
                {
                    Title = "进入戏班后台",
                    Caption = "薛氏剧团 / 后台入口",
                    BackgroundPath = "Assets/Art/Sprites/Generated/generated_backstage_corridor.png",
                    Body = "我推开侧门，后台的木地板发出很轻的一声响。\n\n戏服挂在暗处，盔头箱半开着，舞台那边有人试了一下锣点，又很快停住。\n\n第一天白天的调查，从这里开始。先找人问话，再看物证。夜场开锣之前，我必须知道是谁在安排这场重演。"
                }
            };
        }

        private void ResolveCanvases()
        {
            hudCanvas = GetOrCreateCanvas("WorldHUDCanvas", 10);
            dialogueCanvas = GetOrCreateCanvas("DialogueCanvas", 20);
            normalCanvas = GetOrCreateCanvas("NormalPanelCanvas", 30);
            popupCanvas = GetOrCreateCanvas("PopupCanvas", 40);
            revealCanvas = GetOrCreateCanvas("RevealCanvas", 50);
            systemCanvas = GetOrCreateCanvas("SystemCanvas", 60);
        }

        private Canvas GetOrCreateCanvas(string name, int sortOrder)
        {
            Transform existing = transform.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);

            Canvas canvas = go.GetComponent<Canvas>();
            if (canvas == null)
                canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GraphicRaycaster raycaster = go.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
                go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private void ShowMainMenu()
        {
            currentView = View.MainMenu;
            Time.timeScale = 0f;
            EnsureBgmPlaying();
            RectTransform root = BeginLayer(systemCanvas, "MainMenuLayer");
            FullscreenDesignImage("MainMenuDesign2026", UIHome + "01home1.jpg", new Color(0.06f, 0.035f, 0.028f, 1f), root);

            if (!RuntimeImageAssetsAvailable)
            {
                Text("画皮", 72, Gold, TextAlignmentOptions.Center, new Vector2(0, 180), new Vector2(720, 96), root);
                Text("腾讯游戏创作赛 WebGL 版", 26, Muted, TextAlignmentOptions.Center, new Vector2(0, 110), new Vector2(780, 42), root);
                Button("开始游戏", new Vector2(-360, -260), new Vector2(260, 70), () => TransitionTo(() => ShowSaveLoad(false)), Red, White);
                Button("设置", new Vector2(-90, -260), new Vector2(220, 70), () =>
                {
                    pauseOpenedFromMainMenu = true;
                    TransitionTo(ShowPauseMenu);
                }, Dark, White);
                Button("玩法说明", new Vector2(150, -260), new Vector2(220, 70), () =>
                {
                    guideOpenedFromMainMenu = true;
                    Time.timeScale = 1f;
                    TransitionTo(ShowGuide);
                }, Dark, White);
                Button("制作名单", new Vector2(390, -260), new Vector2(220, 70), () => TransitionTo(ShowCredits), Dark, White);
                return;
            }

            TransparentButton("MenuPlay", new Vector2(-715, -382), new Vector2(300, 140), () => TransitionTo(() => ShowSaveLoad(false)));
            TransparentButton("MenuSettings", new Vector2(-360, -382), new Vector2(300, 140), () =>
            {
                pauseOpenedFromMainMenu = true;
                TransitionTo(ShowPauseMenu);
            });
            TransparentButton("MenuTutorial", new Vector2(0, -382), new Vector2(300, 140), () =>
            {
                guideOpenedFromMainMenu = true;
                Time.timeScale = 1f;
                TransitionTo(ShowGuide);
            });
            TransparentButton("MenuCredits", new Vector2(355, -382), new Vector2(300, 140), () => TransitionTo(ShowCredits));
            TransparentButton("MenuQuit", new Vector2(715, -382), new Vector2(300, 140), () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }

        private void ShowIntro(int index)
        {
            currentView = View.Intro;
            Time.timeScale = 0f;

            if (introSlides == null || introSlides.Length == 0)
            {
                FinishIntro();
                return;
            }

            introIndex = Mathf.Clamp(index, 0, introSlides.Length - 1);
            IntroSlide slide = introSlides[introIndex];
            RectTransform root = BeginLayer(systemCanvas, "IntroLayer");

            FullscreenImage("IntroBackground", slide.BackgroundPath, new Color(0.05f, 0.035f, 0.03f, 1f), root);
            FullscreenPanel("IntroVignette", StoryShade, root);
            Panel("IntroTextPanel", StoryBackdrop, new Vector2(0, -54), new Vector2(1260, 620), root);
            Panel("IntroGoldLine", Gold, new Vector2(0, 270), new Vector2(1260, 4), root);

            Text(slide.Caption, 22, Gold, TextAlignmentOptions.Center, new Vector2(0, 224), new Vector2(1080, 34), root);
            Text(slide.Title, 34, White, TextAlignmentOptions.Center, new Vector2(0, 150), new Vector2(1080, 60), root);
            UnityEngine.UI.Text introBodyText = Text(slide.Body, StoryBodyFontSize, White, TextAlignmentOptions.Top, new Vector2(0, -128), new Vector2(1180, 330), root);
            ConfigureStoryBodyText(introBodyText);

            if (!string.IsNullOrEmpty(slide.PortraitPath))
            {
                Image portrait = ImagePanel("IntroPortrait", slide.PortraitPath, Clear, new Vector2(675, -120), new Vector2(230, 430), root);
                portrait.preserveAspect = true;
                Panel("PortraitBase", Clear, new Vector2(675, -345), new Vector2(260, 18), root);
            }

            Button(introIndex >= introSlides.Length - 1 ? "进入调查" : "继续", StoryPrimaryButtonPos, new Vector2(220, 58), () =>
            {
                if (introIndex >= introSlides.Length - 1) TransitionTo(FinishIntro);
                else TransitionTo(() => ShowIntro(introIndex + 1));
            });
            Button("跳过", StorySecondaryButtonPos, new Vector2(220, 58), () => TransitionTo(FinishIntro));
        }

        private void FinishIntro()
        {
            ShowMonologue(0);
        }

        private void ShowMonologue(int index)
        {
            currentView = View.Monologue;
            Time.timeScale = 0f;

            if (monologueSlides == null || monologueSlides.Length == 0)
            {
                FinishMonologue();
                return;
            }

            monologueIndex = Mathf.Clamp(index, 0, monologueSlides.Length - 1);
            IntroSlide slide = monologueSlides[monologueIndex];
            RectTransform root = BeginLayer(systemCanvas, "MonologueLayer");

            FullscreenImage("MonologueBackground", slide.BackgroundPath, new Color(0.05f, 0.04f, 0.035f, 1f), root);
            FullscreenPanel("MonologueShade", StoryShade, root);
            Panel("MonologueBox", StoryBackdrop, new Vector2(0, -240), new Vector2(1420, 520), root);
            Panel("MonologueLine", Gold, new Vector2(0, 0), new Vector2(1420, 4), root);

            Text(slide.Caption, 24, Gold, TextAlignmentOptions.Center, new Vector2(0, -48), new Vector2(980, 38), root);
            Text(slide.Title, 38, White, TextAlignmentOptions.Center, new Vector2(0, -112), new Vector2(980, 64), root);
            UnityEngine.UI.Text monologueBodyText = Text(slide.Body, StoryBodyFontSize, White, TextAlignmentOptions.Top, new Vector2(0, -310), new Vector2(1280, 260), root);
            ConfigureStoryBodyText(monologueBodyText);
            Button(monologueIndex >= monologueSlides.Length - 1 ? "进入后台" : "继续", StoryPrimaryButtonPos, new Vector2(220, 58), () =>
            {
                if (monologueIndex >= monologueSlides.Length - 1) TransitionTo(FinishMonologue);
                else TransitionTo(() => ShowMonologue(monologueIndex + 1));
            });
            Button("跳过", StorySecondaryButtonPos, new Vector2(220, 58), () => TransitionTo(FinishMonologue));
        }

        private void FinishMonologue()
        {
            currentSceneId = "props";
            gameplayBackgroundPath = CurrentScene().BackgroundPath;
            Time.timeScale = 1f;
            ShowCrimeScene();
            Toast("第一天白天：后台案发现场开放");
        }

        private void TransitionTo(Action drawAction)
        {
            if (drawAction == null) return;
            if (isTransitioning) return;
            StartCoroutine(FadeTransition(drawAction));
        }

        private IEnumerator FadeTransition(Action drawAction)
        {
            isTransitioning = true;
            Image overlay = EnsureFadeOverlay();
            overlay.raycastTarget = true;
            overlay.transform.SetAsLastSibling();

            yield return FadeOverlay(0f, 1f, 0.18f);
            drawAction.Invoke();
            overlay.transform.SetAsLastSibling();
            yield return null;
            yield return FadeOverlay(1f, 0f, 0.24f);

            overlay.raycastTarget = false;
            isTransitioning = false;
        }

        private IEnumerator FadeOverlay(float from, float to, float duration)
        {
            Image overlay = EnsureFadeOverlay();
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                float eased = t * t * (3f - 2f * t);
                overlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, eased));
                yield return null;
            }
            overlay.color = new Color(0f, 0f, 0f, to);
        }

        private Image EnsureFadeOverlay()
        {
            if (fadeOverlay != null) return fadeOverlay;
            Canvas parentCanvas = systemCanvas != null ? systemCanvas : hudCanvas;
            GameObject go = new GameObject("GlobalFadeOverlay", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parentCanvas.transform, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            Stretch(rect);
            fadeOverlay = go.GetComponent<Image>();
            fadeOverlay.color = new Color(0f, 0f, 0f, 0f);
            fadeOverlay.raycastTarget = false;
            return fadeOverlay;
        }

        private void ClearSpawnedUi()
        {
            foreach (GameObject go in spawned)
            {
                if (go != null) Destroy(go);
            }
            spawned.Clear();
        }

        private RectTransform BeginLayer(Canvas canvas, string name, bool dim = true)
        {
            ClearSpawnedUi();
            GameObject layer = Track(new GameObject(name, typeof(RectTransform)));
            layer.transform.SetParent(canvas.transform, false);
            currentLayer = layer.GetComponent<RectTransform>();
            Stretch(currentLayer);
            if (dim) FullscreenPanel("Dim", Ink, currentLayer);
            return currentLayer;
        }

        private void ShowHud()
        {
            currentView = View.Hud;
            RectTransform root = BeginLayer(hudCanvas, "HuapiHUD", false);
            SceneBackgroundImage("GameplayBackground", gameplayBackgroundPath, new Color(0.05f, 0.05f, 0.07f, 1f), root);
            FullscreenPanel("GameplaySceneShade", Clear, root);
            Panel("TopShade", Clear, new Vector2(0, 495), new Vector2(1920, 90), root);
            Text("第一天 · 白天探索 · 薛氏剧团", 26, Gold, TextAlignmentOptions.Left, new Vector2(-860, 500), new Vector2(650, 44), root);
            Text("目标：先调查剧团外景、后台和排练区，夜晚演出会在关键线索后推进", 24, White, TextAlignmentOptions.Left, new Vector2(-860, 462), new Vector2(1100, 38), root);
            HeartBar(root, new Vector2(720, 500));
            Button("指南 F1", new Vector2(770, 452), new Vector2(110, 42), () => TransitionTo(ShowGuide));
            Button("线索 C", new Vector2(895, 452), new Vector2(110, 42), ToggleClues);
            Button("地图 M", new Vector2(1020, 452), new Vector2(110, 42), () => TransitionTo(ShowSceneMap));
            Button("暂停 Esc", new Vector2(1160, 452), new Vector2(130, 42), TogglePause);
        }

        private void ShowCrimeScene()
        {
            currentView = View.CrimeScene;
            SceneNode scene = CurrentScene();
            gameplayBackgroundPath = scene.BackgroundPath;
            RectTransform root = BeginLayer(hudCanvas, "CrimeSceneLayer", false);
            SceneBackgroundImage("CrimeSceneBackground", gameplayBackgroundPath, new Color(0.05f, 0.045f, 0.04f, 1f), root);
            FullscreenPanel("CrimeSceneShade", Clear, root);

            Panel("TopShade", Clear, new Vector2(0, 486), new Vector2(1920, 122), root);
            Text(PhaseTitle() + " / " + scene.Name, 34, Gold, TextAlignmentOptions.Center, new Vector2(-515, 510), new Vector2(820, 48), root);
            Text(scene.Description, 26, White, TextAlignmentOptions.Center, new Vector2(-515, 455), new Vector2(980, 44), root);
            HeartBar(root, new Vector2(520, 505));
            Button("指南 F1", new Vector2(720, 458), new Vector2(110, 38), () => TransitionTo(ShowGuide));
            Button("线索 C", new Vector2(845, 458), new Vector2(110, 38), ToggleClues);
            Button("地图 M", new Vector2(970, 458), new Vector2(110, 38), () => TransitionTo(ShowSceneMap));
            Button("暂停 Esc", new Vector2(1110, 458), new Vector2(130, 38), TogglePause);

            int sceneHotspotCount = 0;
            foreach (InvestigationHotspot hotspot in crimeSceneHotspots)
            {
                if (!string.Equals(hotspot.SceneId ?? "props", currentSceneId, StringComparison.OrdinalIgnoreCase)) continue;
                HotspotMarker(hotspot);
                sceneHotspotCount++;
            }

            DrawSceneActors();

            bool showEvidenceBoard = evidenceBoardVisible;
            if (showEvidenceBoard)
            {
                Panel("EvidenceBoard", Clear, new Vector2(-720, -155), new Vector2(390, 440), root);
                Text("已获线索", 30, Gold, TextAlignmentOptions.Center, new Vector2(-720, 25), new Vector2(300, 44), root);
                int shown = 0;
                foreach (Clue clue in clues.Values)
                {
                    if (!clue.Acquired) continue;
                    EvidenceStrip(root, clue, new Vector2(-720, -35 - shown * 68), () => ShowClueDetail(root, clue), new Vector2(320, 56));
                    shown++;
                    if (shown >= 5) break;
                }
                if (shown == 0)
                    Text("尚未记录证据\n点击现场异常点开始调查", 22, Muted, TextAlignmentOptions.Center, new Vector2(-720, -95), new Vector2(300, 110), root);
                else if (AcquiredEvidenceCount() > shown)
                    Text("按 C 查看全部线索", 18, Muted, TextAlignmentOptions.Center, new Vector2(-720, -310), new Vector2(300, 30), root);
            }

            Panel("SearchHint", Clear, new Vector2(-515, -474), new Vector2(520, 48), root);
            Text(sceneHotspotCount > 0 ? SearchHintText() : scene.Name + "：此处可自由查看，关键线索会在第二天变化。", 26, White, TextAlignmentOptions.Center, new Vector2(-515, -474), new Vector2(760, 42), root);
            DrawInvestigationHint(root);

            DrawSceneNavigation(root, scene);

            if (CanOpenTrial())
            {
                Button("询问目击者", new Vector2(150, -410), new Vector2(180, 50), () => TransitionTo(ShowDialogue), Red, White);
                Button(currentDay == 2 && !isNightPhase ? "进入夜晚" : "整理推理", new Vector2(350, -410), new Vector2(180, 50), () =>
                {
                    if (currentDay == 2 && !isNightPhase) TransitionTo(AdvanceToSecondNight);
                    else TransitionTo(ShowTrial);
                }, Dark, White);
            }
        }

        private void ToggleEvidenceBoard()
        {
            evidenceBoardVisible = !evidenceBoardVisible;
            ShowCrimeScene();
        }

        private void ToggleSceneMap()
        {
            if (currentView == View.SceneMap)
            {
                TransitionTo(ShowCrimeScene);
                return;
            }

            TransitionTo(ShowSceneMap);
        }

        private void ToggleClues()
        {
            if (currentView == View.Clues)
            {
                TransitionTo(ShowCrimeScene);
                return;
            }

            TransitionTo(ShowClues);
        }

        private void HotspotMarker(InvestigationHotspot hotspot)
        {
            Vector2 size = hotspot.Size == Vector2.zero ? new Vector2(86, 86) : hotspot.Size;
            if (!string.IsNullOrEmpty(hotspot.OverlayPath))
            {
                Vector2 overlaySize = hotspot.OverlaySize == Vector2.zero ? size : hotspot.OverlaySize;
                Vector2 overlayPos = hotspot.OverlayPosition == Vector2.zero ? hotspot.Position : hotspot.OverlayPosition;
                Image overlay = ImagePanel("HotspotOverlay_" + hotspot.Id, hotspot.OverlayPath, new Color(0, 0, 0, 0), overlayPos, overlaySize, currentLayer);
                overlay.raycastTarget = false;
                overlay.color = hotspot.Examined ? new Color(1f, 1f, 1f, 0.68f) : Color.white;
            }

            if (!hotspot.Examined && !IsLockedForCurrentPhase(hotspot))
            {
                Panel("HotspotHintDot", new Color(Gold.r, Gold.g, Gold.b, 0.92f), hotspot.Position, new Vector2(34, 34), currentLayer).raycastTarget = false;
                Text("?", 32, PaperText, TextAlignmentOptions.Center, hotspot.Position + new Vector2(0, 1), new Vector2(44, 42), currentLayer);
            }

            Image hoverPanel = Panel("HotspotHover", Clear, hotspot.Position + new Vector2(0, size.y * 0.5f + 30), new Vector2(220, 46), currentLayer);
            string hoverText = IsLockedForCurrentPhase(hotspot) ? "第二天再查：" + hotspot.Name : (hotspot.Examined ? "已调查：" + hotspot.Name : "可调查：" + hotspot.Name);
            Text(hoverText, 18, hotspot.Examined ? Muted : Gold, TextAlignmentOptions.Center, Vector2.zero, new Vector2(200, 32), hoverPanel.rectTransform);
            hoverPanel.gameObject.SetActive(false);

            GameObject go = Track(new GameObject("Hotspot_" + hotspot.Id, typeof(RectTransform), typeof(Image), typeof(Button), typeof(EventTrigger)));
            go.transform.SetParent(currentLayer, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = ClampToSafeArea(hotspot.Position, size);
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;
            Button button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => ShowHotspotDetail(hotspot));
            EventTrigger trigger = go.GetComponent<EventTrigger>();
            AddHoverEvent(trigger, EventTriggerType.PointerEnter, () => hoverPanel.gameObject.SetActive(true));
            AddHoverEvent(trigger, EventTriggerType.PointerExit, () => hoverPanel.gameObject.SetActive(false));
        }

        private void DrawSceneActors()
        {
            foreach (SceneActor actor in sceneActors)
            {
                if (actor.SceneId != currentSceneId) continue;

                Image shadow = Panel("ActorShadow", Clear, actor.Position + new Vector2(0, -actor.Size.y * 0.48f + 10), new Vector2(actor.Size.x * 0.70f, 22), currentLayer);
                shadow.raycastTarget = false;
                Image actorImage = ImagePanel("Actor_" + actor.Id, actor.PortraitPath, Clear, actor.Position, actor.Size, currentLayer);
                actorImage.raycastTarget = false;
                Image hoverPanel = Panel("ActorHover", Clear, actor.Position + new Vector2(0, -actor.Size.y * 0.48f - 26), new Vector2(190, 42), currentLayer);
                Text("交谈：" + actor.Name, 18, Gold, TextAlignmentOptions.Center, Vector2.zero, new Vector2(170, 30), hoverPanel.rectTransform);
                hoverPanel.gameObject.SetActive(false);

                GameObject go = Track(new GameObject("ActorButton_" + actor.Id, typeof(RectTransform), typeof(Image), typeof(Button), typeof(EventTrigger)));
                go.transform.SetParent(currentLayer, false);
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = actor.Position;
                rect.sizeDelta = actor.Size;
                Image image = go.GetComponent<Image>();
                image.color = new Color(1f, 1f, 1f, 0f);
                Button button = go.GetComponent<Button>();
                button.onClick.AddListener(() => ShowDialogue(actor.Id));
                EventTrigger trigger = go.GetComponent<EventTrigger>();
                AddHoverEvent(trigger, EventTriggerType.PointerEnter, () => hoverPanel.gameObject.SetActive(true));
                AddHoverEvent(trigger, EventTriggerType.PointerExit, () => hoverPanel.gameObject.SetActive(false));
            }
        }

        private string SearchHintText()
        {
            int count = AcquiredCurrentPhaseEvidenceCount();
            int target = CurrentPhaseEvidenceTargetCount();
            if (count == 0)
                return currentDay == 1 ? "搜索：移动鼠标查看可调查处，按 M 查看地图。" : "第二天调查：昨日无法确认的位置有新变化，按 M 查看地图。";
            if (count < target)
                return $"搜索进度：{count}/{target}。继续换房间调查，按 M 查看地图。";
            return currentDay == 1 ? "第一天关键证据已记录完毕，可询问目击者或整理推理。M 查看地图。" : "第二天关键证据已记录完毕，可以整理夜晚推理。M 查看地图。";
        }

        private void DrawInvestigationHint(RectTransform root)
        {
            string hint = NextInvestigationHint();
            Panel("NextHintPanel", Clear, new Vector2(0, -420), new Vector2(760, 46), root);
            Text(hint, 18, Gold, TextAlignmentOptions.Center, new Vector2(0, -420), new Vector2(720, 30), root);
        }

        private string NextInvestigationHint()
        {
            if (CanOpenTrial())
                return currentDay == 2 && !isNightPhase ? "提示：第二天白天线索已齐，点击「进入夜晚」推进。" : "提示：当前阶段线索已齐，点击「整理推理」推进。";

            InvestigationHotspot current = FindNextMissingHotspot(currentSceneId);
            if (current != null)
                return $"提示：本房间还可调查「{current.Name}」。";

            InvestigationHotspot next = FindNextMissingHotspot(null);
            if (next == null)
                return "提示：暂无可调查目标，按 C 查看线索，或按 M 查看地图。";

            string sceneName = sceneNodes.TryGetValue(next.SceneId ?? "props", out SceneNode scene) ? scene.Name : "其他房间";
            return $"提示：下一条线索在「{sceneName}」的「{next.Name}」。按 M 查看地图。";
        }

        private InvestigationHotspot FindNextMissingHotspot(string sceneId)
        {
            if (crimeSceneHotspots == null) return null;
            foreach (InvestigationHotspot hotspot in crimeSceneHotspots)
            {
                if (!IsCurrentPhaseHotspot(hotspot)) continue;
                if (IsLockedForCurrentPhase(hotspot)) continue;
                if (sceneId != null && !string.Equals(hotspot.SceneId ?? "props", sceneId, StringComparison.OrdinalIgnoreCase)) continue;
                if (string.IsNullOrEmpty(hotspot.ClueId)) continue;
                if (clues.TryGetValue(hotspot.ClueId, out Clue clue) && clue.Acquired) continue;
                return hotspot;
            }
            return null;
        }

        private string PhaseTitle()
        {
            return $"第{currentDay}天 {(isNightPhase ? "夜晚" : "白天")}";
        }

        private bool IsLockedForCurrentPhase(InvestigationHotspot hotspot)
        {
            return hotspot.SecondDayOnly && currentDay < 2;
        }

        private bool IsCurrentPhaseHotspot(InvestigationHotspot hotspot)
        {
            if (currentDay <= 1) return !hotspot.SecondDayOnly;
            return hotspot.SecondDayOnly;
        }

        private bool CanOpenTrial()
        {
            int target = CurrentPhaseEvidenceTargetCount();
            return target > 0 && AcquiredCurrentPhaseEvidenceCount() >= target;
        }

        private SceneNode CurrentScene()
        {
            if (!sceneNodes.TryGetValue(currentSceneId, out SceneNode scene))
            {
                currentSceneId = "props";
                scene = sceneNodes[currentSceneId];
            }
            return scene;
        }

        private void DrawSceneNavigation(RectTransform root, SceneNode scene)
        {
            Panel("MovePad", Clear, new Vector2(640, -385), new Vector2(310, 210), root);
            Text("方向", 26, Muted, TextAlignmentOptions.Center, new Vector2(640, -292), new Vector2(180, 40), root);
            if (CanMove(scene.Front)) Button("↑\n前", new Vector2(640, -335), new Vector2(104, 72), () => MoveScene(scene.Front, "前"));
            if (CanMove(scene.Left)) Button("←\n左", new Vector2(575, -408), new Vector2(104, 72), () => MoveScene(scene.Left, "左"));
            if (CanMove(scene.Back)) Button("↓\n后", new Vector2(640, -480), new Vector2(104, 72), () => MoveScene(scene.Back, "后"));
            if (CanMove(scene.Right)) Button("→\n右", new Vector2(705, -408), new Vector2(104, 72), () => MoveScene(scene.Right, "右"));
        }

        private bool CanMove(string targetId)
        {
            return !string.IsNullOrEmpty(targetId) && sceneNodes.ContainsKey(targetId);
        }

        private void MoveScene(string targetId, string direction)
        {
            if (string.IsNullOrEmpty(targetId) || !sceneNodes.ContainsKey(targetId))
            {
                Toast(direction + "侧暂时没有可进入的场景");
                return;
            }

            currentSceneId = targetId;
            evidenceBoardVisible = false;
            TransitionTo(() =>
            {
                ShowCrimeScene();
                Toast("移动到：" + CurrentScene().Name);
            });
        }

        private void ShowSceneMap()
        {
            currentView = View.SceneMap;
            RectTransform root = BeginLayer(systemCanvas, "SceneMapLayer");
            FullscreenImage("GeneratedSceneMap", "Assets/Art/Sprites/Generated/generated_overall_scene_map.png", new Color(0.05f, 0.04f, 0.035f, 1f), root);
            AddInvisibleMapInteractions();
#if false
            FullscreenPanel("MapShade", Clear, root);
            ImagePanel("MapFrame2026", UIWindow + "widnow.frame02.png", new Color(0, 0, 0, 0), new Vector2(0, 5), new Vector2(1321, 614), root);
            Text("场景地图 / 当前：" + CurrentScene().Name, 30, Gold, TextAlignmentOptions.Center, new Vector2(0, 320), new Vector2(820, 44), root);
            Text("再次按 M 返回游戏界面", 22, White, TextAlignmentOptions.Center, new Vector2(0, -318), new Vector2(620, 36), root);
            DrawMapMechanismHints(root);
            ImageButton("MapBack", UIWindow + "back.btn.png", new Vector2(0, -390), new Vector2(201, 65), () => TransitionTo(ShowCrimeScene));
#endif
        }

        private void AddInvisibleMapInteractions()
        {
            foreach (SceneNode scene in sceneNodes.Values)
            {
                SceneNode targetScene = scene;
                TransparentButton("MapScene_" + targetScene.Id, targetScene.MapPosition, new Vector2(250, 150), () =>
                {
                    currentSceneId = targetScene.Id;
                    evidenceBoardVisible = false;
                    TransitionTo(ShowCrimeScene);
                });
            }

            TransparentButton("MapBackInvisible", new Vector2(0, -420), new Vector2(360, 110), () => TransitionTo(ShowCrimeScene));
        }

        private void DrawMapConnection(RectTransform root, SceneNode from, string targetId)
        {
            if (string.IsNullOrEmpty(targetId) || !sceneNodes.TryGetValue(targetId, out SceneNode to)) return;
            Vector2 delta = to.MapPosition - from.MapPosition;
            if (delta.sqrMagnitude < 1f) return;
            Vector2 midpoint = (from.MapPosition + to.MapPosition) * 0.5f;
            Vector2 size = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                ? new Vector2(Mathf.Abs(delta.x), 5f)
                : new Vector2(5f, Mathf.Abs(delta.y));
            Panel("MapLine", new Color(0.20f, 0.10f, 0.06f, 0.38f), midpoint, size, root);
        }

        private void DrawMapMechanismHints(RectTransform root)
        {
            foreach (SceneNode scene in sceneNodes.Values)
            {
                int pending = PendingPuzzleCount(scene.Id);
                bool current = string.Equals(scene.Id, currentSceneId, StringComparison.OrdinalIgnoreCase);
                Color nodeColor = pending > 0 ? new Color(0.36f, 0.12f, 0.06f, 0.86f) : Clear;
                Vector2 nodeSize = current ? new Vector2(132, 54) : new Vector2(112, 46);
                Panel("MapHintNode_" + scene.Id, nodeColor, scene.MapPosition, nodeSize, root);
                Text(scene.Name, current ? 18 : 16, current ? Gold : White, TextAlignmentOptions.Center, scene.MapPosition + new Vector2(0, 6), new Vector2(nodeSize.x - 12, 24), root);
                if (pending > 0)
                {
                    Panel("MapHintBadge_" + scene.Id, Red, scene.MapPosition + new Vector2(nodeSize.x * 0.43f, nodeSize.y * 0.32f), new Vector2(32, 28), root);
                    Text(pending.ToString(), 17, White, TextAlignmentOptions.Center, scene.MapPosition + new Vector2(nodeSize.x * 0.43f, nodeSize.y * 0.32f), new Vector2(28, 22), root);
                }
            }

            Panel("MapMechanismHintPanel", Clear, new Vector2(555, 20), new Vector2(470, 450), root);
            Text("机关提示", 30, Gold, TextAlignmentOptions.Center, new Vector2(555, 200), new Vector2(390, 44), root);
            Text("按地图节点数字去找，先解当前阶段未完成的机关。", 18, Muted, TextAlignmentOptions.Center, new Vector2(555, 162), new Vector2(410, 34), root);

            int row = 0;
            foreach (SceneNode scene in sceneNodes.Values)
            {
                InvestigationHotspot hotspot = FirstPendingPuzzle(scene.Id);
                if (hotspot == null) continue;

                string line = scene.Name + "：" + hotspot.Name;
                string hint = string.IsNullOrEmpty(hotspot.PuzzleHint) ? "观察房间里的异常位置。" : hotspot.PuzzleHint;
                float y = 112 - row * 76;
                Panel("MapPuzzleRow_" + scene.Id, Clear, new Vector2(555, y), new Vector2(400, 62), root);
                Text(line, 19, Gold, TextAlignmentOptions.Left, new Vector2(555, y + 12), new Vector2(360, 24), root);
                Text(hint, 15, White, TextAlignmentOptions.Left, new Vector2(555, y - 13), new Vector2(360, 24), root);
                row++;
                if (row >= 5) break;
            }

            if (row == 0)
                Text("当前阶段机关已清空，可以回到现场整理推理。", 20, White, TextAlignmentOptions.Center, new Vector2(555, 45), new Vector2(380, 80), root);
        }

        private int PendingPuzzleCount(string sceneId)
        {
            int count = 0;
            if (crimeSceneHotspots == null) return count;
            foreach (InvestigationHotspot hotspot in crimeSceneHotspots)
            {
                if (!IsPendingPuzzle(hotspot)) continue;
                if (string.Equals(hotspot.SceneId ?? "props", sceneId, StringComparison.OrdinalIgnoreCase))
                    count++;
            }
            return count;
        }

        private InvestigationHotspot FirstPendingPuzzle(string sceneId)
        {
            if (crimeSceneHotspots == null) return null;
            foreach (InvestigationHotspot hotspot in crimeSceneHotspots)
            {
                if (!IsPendingPuzzle(hotspot)) continue;
                if (string.Equals(hotspot.SceneId ?? "props", sceneId, StringComparison.OrdinalIgnoreCase))
                    return hotspot;
            }
            return null;
        }

        private bool IsPendingPuzzle(InvestigationHotspot hotspot)
        {
            if (hotspot == null || !hotspot.Puzzle) return false;
            if (!IsCurrentPhaseHotspot(hotspot) || IsLockedForCurrentPhase(hotspot)) return false;
            if (string.IsNullOrEmpty(hotspot.ClueId)) return !hotspot.Examined;
            return !clues.TryGetValue(hotspot.ClueId, out Clue clue) || !clue.Acquired;
        }

        private string CrimeScenePrompt()
        {
            int count = AcquiredCurrentPhaseEvidenceCount();
            int target = CurrentPhaseEvidenceTargetCount();
            if (count == 0)
                return "钟铁面在排练中途倒下。众人说他是被旧案诅咒，但现场比传闻更诚实。先从门、地面、脸谱盒和锣鼓点查起。";
            if (count < target)
                return $"已记录 {count}/{target} 条当前阶段关键证据。其他区域可以先看，部分线索会留到第二天出现。";
            return "现场证据已经收齐。接下来要问的是：谁能改锣点，谁能碰药瓶，谁能从门外制造反锁，谁又知道民国旧案的全部细节。";
        }

        private int AcquiredEvidenceCount()
        {
            int count = 0;
            if (crimeSceneHotspots == null) return count;
            foreach (InvestigationHotspot hotspot in crimeSceneHotspots)
            {
                if (!string.IsNullOrEmpty(hotspot.ClueId) && clues.TryGetValue(hotspot.ClueId, out Clue clue) && clue.Acquired)
                    count++;
            }
            return count;
        }

        private int AcquiredCurrentPhaseEvidenceCount()
        {
            int count = 0;
            if (crimeSceneHotspots == null) return count;
            foreach (InvestigationHotspot hotspot in crimeSceneHotspots)
            {
                if (!IsCurrentPhaseHotspot(hotspot)) continue;
                if (!string.IsNullOrEmpty(hotspot.ClueId) && clues.TryGetValue(hotspot.ClueId, out Clue clue) && clue.Acquired)
                    count++;
            }
            return count;
        }

        private int CurrentPhaseEvidenceTargetCount()
        {
            int count = 0;
            if (crimeSceneHotspots == null) return count;
            foreach (InvestigationHotspot hotspot in crimeSceneHotspots)
            {
                if (IsCurrentPhaseHotspot(hotspot) && !string.IsNullOrEmpty(hotspot.ClueId))
                    count++;
            }
            return count;
        }

        private void ShowHotspotDetail(InvestigationHotspot hotspot)
        {
            if (IsLockedForCurrentPhase(hotspot))
            {
                Toast("这里会在第二天出现新线索，先继续第一天调查。");
                return;
            }

            if (hotspot.Puzzle && !hotspot.Examined)
            {
                ShowMechanismPuzzle(hotspot);
                return;
            }

            Panel("HotspotBlocker", Clear, Vector2.zero, new Vector2(1920, 1080), currentLayer);
            Panel("HotspotPaper", Paper, new Vector2(0, -20), new Vector2(960, 430), currentLayer);
            Text(hotspot.Name, 42, PaperText, TextAlignmentOptions.Center, new Vector2(0, 115), new Vector2(780, 60), currentLayer);
            Text(hotspot.Description, 28, PaperText, TextAlignmentOptions.TopLeft, new Vector2(0, -20), new Vector2(800, 150), currentLayer);

            string clueName = clues.TryGetValue(hotspot.ClueId, out Clue clue) ? clue.Name : "现场记录";
            Button(hotspot.Examined ? "返回现场" : "记录证据：" + clueName, new Vector2(0, -190), new Vector2(360, 62), () =>
            {
                string toast = null;
                if (!hotspot.Examined && clues.TryGetValue(hotspot.ClueId, out Clue found))
                {
                    hotspot.Examined = true;
                    found.Acquired = true;
                    found.Used = false;
                    toast = "获得证据：" + found.Name;
                }
                ShowCrimeScene();
                if (!string.IsNullOrEmpty(toast)) Toast(toast);
            }, Red, White);
        }

        private void ShowFacePuzzle()
        {
            currentView = View.CrimeScene;
            RectTransform root = BeginLayer(popupCanvas, "FacePuzzleLayer", false);
            SceneBackgroundImage("PuzzleBackground", CurrentScene().BackgroundPath, new Color(0.05f, 0.045f, 0.04f, 1f), root);
            FullscreenPanel("PuzzleShade", Clear, root);
            Panel("PuzzleBoard", Clear, new Vector2(0, -25), new Vector2(1180, 680), root);
            Text("脸谱五色", 42, Gold, TextAlignmentOptions.Center, new Vector2(0, 230), new Vector2(720, 58), root);
            Text("按旧戏单的行当顺序摆回脸谱。提示：白脸不在台前。", 24, White, TextAlignmentOptions.Center, new Vector2(0, 176), new Vector2(900, 42), root);
            Text("当前顺序：" + FacePuzzleProgress(), 24, Muted, TextAlignmentOptions.Center, new Vector2(0, 116), new Vector2(860, 44), root);

            Button("红", new Vector2(-380, 10), new Vector2(140, 100), () => ChooseFaceColor("红"), Red, White);
            Button("金", new Vector2(-190, 10), new Vector2(140, 100), () => ChooseFaceColor("金"), Gold, PaperText);
            Button("黑", new Vector2(0, 10), new Vector2(140, 100), () => ChooseFaceColor("黑"), Dark, White);
            Button("蓝", new Vector2(190, 10), new Vector2(140, 100), () => ChooseFaceColor("蓝"), Hex(0x263E65), White);
            Button("白", new Vector2(380, 10), new Vector2(140, 100), () => ChooseFaceColor("白"), White, PaperText);

            Text("正确解开后，脸谱盒夹层会打开。", 22, Muted, TextAlignmentOptions.Center, new Vector2(0, -128), new Vector2(740, 40), root);
            Button("重置", new Vector2(-120, -230), new Vector2(180, 54), () => { facePuzzleStep = 0; ShowFacePuzzle(); });
            Button("返回现场", new Vector2(120, -230), new Vector2(180, 54), () => TransitionTo(ShowCrimeScene));
        }

        private void ShowMechanismPuzzle(InvestigationHotspot hotspot)
        {
            switch (hotspot.PuzzleKind)
            {
                case "body":
                    ShowDragMechanismPuzzle(hotspot, "倒地痕迹复原", "把现场痕迹放回正确关系，确认钟铁面不是自然摔倒。", new[] { "面具", "拖痕", "台口" }, new[] { "台口", "面具", "拖痕" });
                    break;
                case "latch":
                    ShowDragMechanismPuzzle(hotspot, "门闩反扣", "把机关部件拖到门板槽位，复原门外反扣。", new[] { "穿线", "压闩", "回抽" }, new[] { "回抽", "穿线", "压闩" });
                    break;
                case "medicine":
                    ShowDragMechanismPuzzle(hotspot, "药瓶残留比对", "把药瓶、入口和残留痕迹对应起来，确认嗓药被动过手脚。", new[] { "药瓶", "水杯", "粉末" }, new[] { "粉末", "药瓶", "水杯" });
                    break;
                case "schedule":
                    ShowDragMechanismPuzzle(hotspot, "流程表复盘", "把公告板上的三段时间线排到对应位置，找出被照抄的旧案流程。", new[] { "旧案", "今日", "夜场" }, new[] { "夜场", "旧案", "今日" });
                    break;
                case "gong":
                    ShowDragMechanismPuzzle(hotspot, "锣点复原", "把锣点牌拖进谱格：短声遮门，长声遮倒地，停拍遮脚步。", new[] { "短锣", "长锣", "停拍", "短锣" }, new[] { "停拍", "短锣", "短锣", "长锣" });
                    break;
                case "wire":
                    ShowDragMechanismPuzzle(hotspot, "滑轨牵线", "把线轴拖回滑轨路径，复原从侧台到雅座的牵引。", new[] { "上轨", "侧轮", "铜环", "门闩" }, new[] { "门闩", "上轨", "铜环", "侧轮" });
                    break;
                case "ring":
                    ShowDragMechanismPuzzle(hotspot, "雅座铜环转向", "把牵线经过的节点放回路径，确认雅座铜环的转向用途。", new[] { "铜环", "细线", "滑轨", "门闩" }, new[] { "细线", "门闩", "铜环", "滑轨" });
                    break;
                case "curtain":
                    ShowDragMechanismPuzzle(hotspot, "门帘线头复核", "把第二天出现的新痕迹和第一天观察到的位置对上。", new[] { "磨痕", "线头", "珠花" }, new[] { "珠花", "磨痕", "线头" });
                    break;
                case "case":
                    ShowDragMechanismPuzzle(hotspot, "木偶箱封印", "把定情木偶的四枚封印拖回方位，打开旧戏箱。", new[] { "东", "南", "西", "北" }, new[] { "西", "北", "东", "南" });
                    break;
                default:
                    ShowFacePuzzle();
                    break;
            }
        }

        private void ShowSequencePuzzle(InvestigationHotspot hotspot, string title, string prompt, string[] order, ref int step)
        {
            currentView = View.CrimeScene;
            RectTransform root = BeginLayer(popupCanvas, "MechanismPuzzleLayer", false);
            SceneBackgroundImage("MechanismBackground", CurrentScene().BackgroundPath, new Color(0.05f, 0.045f, 0.04f, 1f), root);
            FullscreenPanel("MechanismShade", Clear, root);
            Panel("MechanismBoard", Clear, new Vector2(0, -15), new Vector2(1240, 690), root);
            Panel("MechanismLine", Gold, new Vector2(0, 292), new Vector2(1040, 4), root);

            Text(title, 44, Gold, TextAlignmentOptions.Center, new Vector2(0, 230), new Vector2(900, 62), root);
            Text(prompt, 23, White, TextAlignmentOptions.Center, new Vector2(0, 170), new Vector2(980, 58), root);
            Text("当前进度：" + SequenceProgress(order, step), 23, Muted, TextAlignmentOptions.Center, new Vector2(0, 104), new Vector2(980, 42), root);

            string[] choices = BuildMechanismChoices(order);
            for (int i = 0; i < choices.Length; i++)
            {
                Vector2 pos = new Vector2(-390 + i * 260, -18);
                string choice = choices[i];
                Button(choice, pos, new Vector2(190, 92), () => ChooseSequenceStep(hotspot, order, choice), Dark, White);
            }

            Text("机关类线索必须解开后才会写入证据条。错误一次扣一颗心。", 21, Muted, TextAlignmentOptions.Center, new Vector2(0, -152), new Vector2(860, 42), root);
            Button("重置机关", new Vector2(-140, -250), new Vector2(190, 56), () => { ResetMechanismStep(hotspot.PuzzleKind); ShowMechanismPuzzle(hotspot); });
            Button("返回现场", new Vector2(140, -250), new Vector2(190, 56), () => TransitionTo(ShowCrimeScene));
        }

        private void ShowDragMechanismPuzzle(InvestigationHotspot hotspot, string title, string prompt, string[] targets, string[] pieces)
        {
            currentView = View.CrimeScene;
            RectTransform root = BeginLayer(popupCanvas, "DragMechanismPuzzleLayer", false);
            SceneBackgroundImage("DragMechanismBackground", CurrentScene().BackgroundPath, new Color(0.05f, 0.045f, 0.04f, 1f), root);
            FullscreenPanel("DragMechanismShade", Clear, root);
            ImagePanel("DragMechanismFrame", UIWindow + "widnow.frame02.png", Clear, Vector2.zero, new Vector2(1321, 614), root);

            Text(title, 46, Gold, TextAlignmentOptions.Center, new Vector2(0, 250), new Vector2(900, 64), root);
            Text(prompt, 22, White, TextAlignmentOptions.Center, new Vector2(0, 196), new Vector2(980, 48), root);
            Text("拖动下方部件到上方对应槽位。放错会弹回，全部放对后记录证据。", 20, Muted, TextAlignmentOptions.Center, new Vector2(0, -242), new Vector2(980, 38), root);

            if (!string.IsNullOrEmpty(hotspot.PuzzleHint))
                Text("提示：" + hotspot.PuzzleHint, 18, Gold, TextAlignmentOptions.Center, new Vector2(0, -278), new Vector2(1040, 34), root);

            DrawMechanismDiagram(root, hotspot.PuzzleKind);

            List<DragSlot> slotPositions = new List<DragSlot>();
            int count = targets.Length;
            float startX = -((count - 1) * 185f) * 0.5f;
            for (int i = 0; i < targets.Length; i++)
            {
                string target = targets[i];
                Vector2 slotPos = new Vector2(startX + i * 185f, 72);
                slotPositions.Add(new DragSlot { Label = target, Position = slotPos });
                Panel("DropSlot_" + target, Clear, slotPos, new Vector2(142, 88), root);
                Panel("DropSlotLine_" + target, Gold, slotPos + new Vector2(0, -48), new Vector2(142, 4), root);
                Text((i + 1).ToString(), 26, Gold, TextAlignmentOptions.Center, slotPos + new Vector2(0, 4), new Vector2(80, 40), root);
            }

            for (int i = 0; i < pieces.Length; i++)
            {
                string piece = pieces[i];
                Vector2 startPos = new Vector2(startX + i * 185f, -110);
                CreateDraggablePiece(root, piece, startPos, slotPositions, targets.Length, () => CompleteMechanismPuzzle(hotspot));
            }

            ImageButton("DragPuzzleBack", UIWindow + "back.btn.png", new Vector2(0, -330), new Vector2(201, 65), () => TransitionTo(ShowCrimeScene));
        }

        private void DrawMechanismDiagram(RectTransform root, string kind)
        {
            if (kind == "latch")
            {
                Panel("DoorDiagram", Clear, new Vector2(0, 78), new Vector2(760, 230), root);
                Panel("DoorGap", Clear, new Vector2(0, 78), new Vector2(6, 230), root);
                Panel("DoorLatch", Gold, new Vector2(-205, 118), new Vector2(170, 12), root);
                Panel("DoorLatch2", Gold, new Vector2(205, 36), new Vector2(170, 12), root);
                Text("门板 / 门闩 / 细线孔", 18, Muted, TextAlignmentOptions.Center, new Vector2(0, -25), new Vector2(520, 34), root);
            }
            else if (kind == "gong")
            {
                Panel("GongScore", Clear, new Vector2(0, 78), new Vector2(760, 230), root);
                for (int i = 0; i < 5; i++)
                    Panel("ScoreLine", Gold, new Vector2(0, 15 + i * 34), new Vector2(660, 3), root);
                Text("锣鼓谱格", 18, Muted, TextAlignmentOptions.Center, new Vector2(0, -25), new Vector2(520, 34), root);
            }
            else if (kind == "wire")
            {
                Panel("Rail", Gold, new Vector2(0, 142), new Vector2(680, 8), root);
                Panel("Rail2", Gold, new Vector2(0, 72), new Vector2(520, 8), root);
                Panel("PulleyA", new Color(0.45f, 0.35f, 0.18f, 0.9f), new Vector2(-260, 106), new Vector2(70, 70), root);
                Panel("PulleyB", new Color(0.45f, 0.35f, 0.18f, 0.9f), new Vector2(260, 106), new Vector2(70, 70), root);
                Text("滑轨 / 侧轮 / 铜环 / 门闩", 18, Muted, TextAlignmentOptions.Center, new Vector2(0, -25), new Vector2(620, 34), root);
            }
            else if (kind == "body")
            {
                Panel("BodyTrace", Clear, new Vector2(0, 78), new Vector2(760, 230), root);
                Panel("BodyTraceLine", Red, new Vector2(-50, 92), new Vector2(520, 10), root);
                Panel("BodyMaskMark", Gold, new Vector2(-270, 126), new Vector2(90, 48), root);
                Panel("BodyStageMark", new Color(0.35f, 0.25f, 0.14f, 0.9f), new Vector2(270, 48), new Vector2(120, 72), root);
                Text("面具 / 拖痕 / 台口", 18, Muted, TextAlignmentOptions.Center, new Vector2(0, -25), new Vector2(620, 34), root);
            }
            else if (kind == "medicine")
            {
                Panel("MedicineDesk", Clear, new Vector2(0, 70), new Vector2(720, 200), root);
                Panel("MedicineBottle", Gold, new Vector2(-210, 112), new Vector2(64, 120), root);
                Panel("MedicineCup", new Color(0.7f, 0.65f, 0.52f, 0.9f), new Vector2(0, 92), new Vector2(92, 78), root);
                Panel("MedicinePowder", White, new Vector2(210, 62), new Vector2(120, 22), root);
                Text("药瓶 / 水杯 / 粉末", 18, Muted, TextAlignmentOptions.Center, new Vector2(0, -25), new Vector2(620, 34), root);
            }
            else if (kind == "schedule")
            {
                Panel("ScheduleBoard", Clear, new Vector2(0, 78), new Vector2(760, 230), root);
                for (int i = 0; i < 3; i++)
                    Panel("ScheduleLine", Gold, new Vector2(0, 138 - i * 58), new Vector2(560, 4), root);
                Text("旧案 / 今日 / 夜场", 18, Muted, TextAlignmentOptions.Center, new Vector2(0, -25), new Vector2(620, 34), root);
            }
            else if (kind == "ring")
            {
                Panel("RingPath", Clear, new Vector2(0, 78), new Vector2(760, 230), root);
                Panel("RingRail", Gold, new Vector2(-150, 135), new Vector2(300, 7), root);
                Panel("RingMark", new Color(0.6f, 0.46f, 0.2f, 0.95f), new Vector2(80, 82), new Vector2(92, 92), root);
                Panel("RingDoor", Red, new Vector2(260, 52), new Vector2(120, 18), root);
                Text("铜环 / 细线 / 滑轨 / 门闩", 18, Muted, TextAlignmentOptions.Center, new Vector2(0, -25), new Vector2(620, 34), root);
            }
            else if (kind == "curtain")
            {
                Panel("CurtainPanel", Clear, new Vector2(0, 78), new Vector2(760, 230), root);
                Panel("CurtainLeft", Red, new Vector2(-120, 86), new Vector2(8, 180), root);
                Panel("CurtainRight", Red, new Vector2(120, 86), new Vector2(8, 180), root);
                Panel("CurtainThread", Gold, new Vector2(0, 122), new Vector2(380, 6), root);
                Text("磨痕 / 线头 / 珠花", 18, Muted, TextAlignmentOptions.Center, new Vector2(0, -25), new Vector2(620, 34), root);
            }
            else
            {
                Panel("PuppetCase", Clear, new Vector2(0, 92), new Vector2(620, 210), root);
                Panel("PuppetCaseSeal", Red, new Vector2(0, 92), new Vector2(520, 18), root);
                Panel("PuppetLeft", Gold, new Vector2(-110, 128), new Vector2(70, 120), root);
                Panel("PuppetRight", Gold, new Vector2(110, 128), new Vector2(70, 120), root);
                Text("木偶箱 / 定情木偶 / 封印方位", 18, Muted, TextAlignmentOptions.Center, new Vector2(0, -25), new Vector2(620, 34), root);
            }
        }

        private void CreateDraggablePiece(RectTransform root, string label, Vector2 startPos, List<DragSlot> slotPositions, int targetCount, Action onComplete)
        {
            GameObject go = Track(new GameObject("DragPiece_" + label, typeof(RectTransform), typeof(Image), typeof(EventTrigger)));
            go.transform.SetParent(root, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = startPos;
            rect.sizeDelta = new Vector2(134, 70);
            Image image = go.GetComponent<Image>();
            image.color = Paper;
            Text(label, 24, PaperText, TextAlignmentOptions.Center, Vector2.zero, new Vector2(112, 42), rect);

            EventTrigger trigger = go.GetComponent<EventTrigger>();
            Vector2 origin = startPos;
            bool locked = false;
            AddHoverEvent(trigger, EventTriggerType.BeginDrag, () =>
            {
                if (locked) return;
                PlayClickSound();
                go.transform.SetAsLastSibling();
            });
            AddDragEvent(trigger, EventTriggerType.Drag, data =>
            {
                if (locked) return;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(root, data.position, data.pressEventCamera, out Vector2 local);
                rect.anchoredPosition = ClampToSafeArea(local, rect.sizeDelta);
            });
            AddDragEvent(trigger, EventTriggerType.EndDrag, data =>
            {
                if (locked) return;

                DragSlot targetSlot = null;
                float bestDistance = float.MaxValue;
                foreach (DragSlot slot in slotPositions)
                {
                    if (slot.Occupied || slot.Label != label) continue;
                    float distance = Vector2.Distance(rect.anchoredPosition, slot.Position);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        targetSlot = slot;
                    }
                }

                if (targetSlot != null && bestDistance <= 85f)
                {
                    rect.anchoredPosition = targetSlot.Position;
                    image.color = Gold;
                    locked = true;
                    image.raycastTarget = false;
                    targetSlot.Occupied = true;
                    PlayHintSound();
                    int occupied = 0;
                    foreach (DragSlot slot in slotPositions)
                    {
                        if (slot.Occupied) occupied++;
                    }
                    if (occupied >= targetCount)
                        onComplete?.Invoke();
                }
                else
                {
                    rect.anchoredPosition = origin;
                    LoseHeart("机关部件放错");
                }
            });
        }

        private string[] BuildMechanismChoices(string[] order)
        {
            if (order.Length <= 4) return order;
            string[] choices = new string[4];
            Array.Copy(order, choices, 4);
            return choices;
        }

        private string SequenceProgress(string[] order, int step)
        {
            if (step <= 0) return "未开始";
            int count = Mathf.Clamp(step, 0, order.Length);
            return string.Join(" → ", new ArraySegment<string>(order, 0, count));
        }

        private void ChooseSequenceStep(InvestigationHotspot hotspot, string[] order, string choice)
        {
            int step = GetMechanismStep(hotspot.PuzzleKind);
            if (step < order.Length && choice == order[step])
            {
                SetMechanismStep(hotspot.PuzzleKind, step + 1);
                if (step + 1 >= order.Length)
                {
                    CompleteMechanismPuzzle(hotspot);
                    return;
                }
                ShowMechanismPuzzle(hotspot);
                return;
            }

            ResetMechanismStep(hotspot.PuzzleKind);
            LoseHeart("机关顺序错误");
            ShowMechanismPuzzle(hotspot);
        }

        private int GetMechanismStep(string kind)
        {
            switch (kind)
            {
                case "latch": return latchPuzzleStep;
                case "gong": return gongPuzzleStep;
                case "wire": return wirePuzzleStep;
                case "case": return casePuzzleStep;
                default: return 0;
            }
        }

        private void SetMechanismStep(string kind, int value)
        {
            switch (kind)
            {
                case "latch": latchPuzzleStep = value; break;
                case "gong": gongPuzzleStep = value; break;
                case "wire": wirePuzzleStep = value; break;
                case "case": casePuzzleStep = value; break;
            }
        }

        private void ResetMechanismStep(string kind)
        {
            SetMechanismStep(kind, 0);
        }

        private void CompleteMechanismPuzzle(InvestigationHotspot hotspot)
        {
            ResetMechanismStep(hotspot.PuzzleKind);
            hotspot.Examined = true;
            if (clues.TryGetValue(hotspot.ClueId, out Clue clue))
                clue.Acquired = true;
            ShowCrimeScene();
            Toast("机关解开：获得证据「" + (clues.TryGetValue(hotspot.ClueId, out Clue found) ? found.Name : hotspot.Name) + "」");
        }

        private string FacePuzzleProgress()
        {
            string[] order = { "红", "金", "黑", "蓝", "白" };
            if (facePuzzleStep <= 0) return "未开始";
            int count = Mathf.Clamp(facePuzzleStep, 0, order.Length);
            return string.Join(" → ", new ArraySegment<string>(order, 0, count));
        }

        private void ChooseFaceColor(string color)
        {
            string[] order = { "红", "金", "黑", "蓝", "白" };
            if (facePuzzleStep < order.Length && color == order[facePuzzleStep])
            {
                facePuzzleStep++;
                if (facePuzzleStep >= order.Length)
                {
                    CompleteFacePuzzle();
                    return;
                }
                ShowFacePuzzle();
                return;
            }

            facePuzzleStep = 0;
            LoseHeart("脸谱顺序错误");
            ShowFacePuzzle();
        }

        private void CompleteFacePuzzle()
        {
            facePuzzleStep = 0;
            foreach (InvestigationHotspot hotspot in crimeSceneHotspots)
            {
                if (hotspot.Id != "maskbox") continue;
                hotspot.Examined = true;
                if (clues.TryGetValue(hotspot.ClueId, out Clue clue))
                    clue.Acquired = true;
                break;
            }
            ShowCrimeScene();
            Toast("机关解开：获得证据「新鲜香灰」");
        }

        private void ShowGuide()
        {
            currentView = View.Guide;
            RectTransform root = BeginLayer(systemCanvas, "GuideLayer", false);
            FullscreenDesignImage("TutorialBackground2026", UISettings + "02settings_bg.jpg", Clear, root);
            ImagePanel("TutorialBoard2026", UITutorial + "03tutorial_widnow.frame.png", new Color(0, 0, 0, 0), Vector2.zero, new Vector2(1746, 900), root);
            string[] rows =
            {
                "WASD / 方向键：移动侦探",
                "E：调查场景热点 / 与人物对话",
                "C：打开线索背包",
                "X：打开人物档案",
                "V：进入观察画皮",
                "T：进入推理判定",
                "Esc：暂停 / 返回"
            };
            for (int i = 0; i < rows.Length; i++)
                Text(rows[i], 32, White, TextAlignmentOptions.Left, new Vector2(0, 180 - i * 62), new Vector2(900, 48), root);

            TransparentButton("GuideClose", new Vector2(830, 455), new Vector2(120, 120), CloseGuide);
        }

        private void CloseGuide()
        {
            if (guideOpenedFromMainMenu)
            {
                guideOpenedFromMainMenu = false;
                TransitionTo(ShowMainMenu);
            }
            else TransitionTo(ShowCrimeScene);
        }

        private void ShowCredits()
        {
            currentView = View.Credits;
            RectTransform root = BeginLayer(systemCanvas, "CreditsLayer", false);
            FullscreenDesignImage("CreditsDesign2026", UICredits + "04credits.jpg", Clear, root);
            Vector2 creditNameSize = new Vector2(420, 34);
            Vector2 creditEnglishSize = new Vector2(420, 28);
            float creditNameX = -145f;
            string[] chineseNames = { "孔陌羚", "吕明樑", "曾堉琳", "孔陌羚", "吕明樑" };
            string[] englishNames = { "Moling Kong", "Mingliang Lyu", "Yulin Zeng", "Moling Kong", "Mingliang Lyu" };
            float[] creditLineY = { 150f, 38f, -69f, -172f, -280f };
            for (int i = 0; i < creditLineY.Length; i++)
            {
                Text(chineseNames[i], 26, White, TextAlignmentOptions.Center, new Vector2(creditNameX, creditLineY[i] + 24f), creditNameSize, root);
                Text(englishNames[i], 18, Muted, TextAlignmentOptions.Center, new Vector2(creditNameX, creditLineY[i] - 28f), creditEnglishSize, root);
            }
            TransparentButton("CreditsClose", new Vector2(830, 455), new Vector2(120, 120), () => TransitionTo(ShowMainMenu));
        }

        private void ShowDialogue()
        {
            ShowDialogue("qingyi");
        }

        private void ShowDialogue(string actorId)
        {
            currentView = View.Dialogue;
            dialogueBeat++;
            RectTransform root = BeginDialogueLayer(actorId, out SceneActor actor);

            ImagePanel("DialogueTopicFrame2026", UIWindow + "widnow.frame01.png", new Color(0, 0, 0, 0), new Vector2(700, -300), new Vector2(380, 390), root);
            Text(actor.Name, 28, Gold, TextAlignmentOptions.Center, new Vector2(-520, -362), new Vector2(300, 42), root);
            UnityEngine.UI.Text openingText = Text(DialogueOpening(actor.Id), 27, White, TextAlignmentOptions.TopLeft, new Vector2(-5, -447), new Vector2(1080, 112), root);
            ConfigureDialogueText(openingText, 23);

            Text("盘问话题", 24, Gold, TextAlignmentOptions.Center, new Vector2(700, -165), new Vector2(280, 38), root);
            DialogueTopic[] topics = DialogueTopics(actor.Id);
            int shown = 0;
            foreach (DialogueTopic topic in topics)
            {
                if (topic.SecondDayOnly && currentDay < 2) continue;
                bool asked = askedDialogueTopics.Contains(DialogueTopicKey(actor.Id, topic.Id));
                string label = asked ? "已问：" + topic.Label : topic.Label;
                DialogueTopic captured = topic;
                Button(label, new Vector2(700, -225 - shown * 58), new Vector2(285, 46), () => ShowDialogueResponse(actor.Id, captured.Id), asked ? Dark : Red, White);
                shown++;
                if (shown >= 4) break;
            }

            if (shown == 0)
                Text("暂时没有可问的话题。", 20, Muted, TextAlignmentOptions.Center, new Vector2(700, -282), new Vector2(260, 80), root);

            ImageButton("DialogueBack", UIWindow + "back.btn.png", new Vector2(700, -438), new Vector2(150, 49), () => TransitionTo(ShowCrimeScene));
        }

        private void ShowEvidenceDialogueLegacy(string actorId)
        {
            currentView = View.Dialogue;
            dialogueBeat++;
            RectTransform root = BeginLayer(dialogueCanvas, "DialogueLayer", false);
            SceneNode scene = CurrentScene();
            SceneBackgroundImage("DialogueBackground", scene.BackgroundPath, new Color(0.05f, 0.045f, 0.04f, 1f), root);
            FullscreenPanel("DialogueShade", Clear, root);

            SceneActor actor = FindSceneActor(actorId) ?? new SceneActor
            {
                Id = "qingyi",
                Name = "沈青衣",
                PortraitPath = "Assets/Art/Sprites/Characters/旦2.png",
                Position = new Vector2(-420, -80),
                Size = new Vector2(300, 560)
            };

            ImagePanel("DialogueActor_" + actor.Id, actor.PortraitPath, Clear, new Vector2(-610, -100), new Vector2(300, 555), root);
            ImagePanel("DialogueBox2026", UIWindow + "dialoguebox.png", new Color(0, 0, 0, 0), new Vector2(0, -446), new Vector2(1920, 188), root);
            ImagePanel("EvidenceTalkFrame2026", UIWindow + "widnow.frame01.png", new Color(0, 0, 0, 0), new Vector2(760, -345), new Vector2(285, 276), root);
            Text(actor.Name, 28, Gold, TextAlignmentOptions.Center, new Vector2(-520, -362), new Vector2(300, 42), root);
            Text(DialogueLine(actor.Id), 22, White, TextAlignmentOptions.Top, new Vector2(-20, -447), new Vector2(1000, 110), root);

            Text("旧对话已停用", 22, Gold, TextAlignmentOptions.Center, new Vector2(760, -252), new Vector2(220, 38), root);
            Button("返回盘问", new Vector2(760, -315), new Vector2(170, 46), () => ShowDialogue(actor.Id));
            Button("继续调查", new Vector2(760, -375), new Vector2(170, 46), () => TransitionTo(ShowCrimeScene));
            ImageButton("DialogueBack", UIWindow + "back.btn.png", new Vector2(760, -438), new Vector2(150, 49), () => TransitionTo(ShowCrimeScene));
        }

        private RectTransform BeginDialogueLayer(string actorId, out SceneActor actor)
        {
            RectTransform root = BeginLayer(dialogueCanvas, "DialogueLayer", false);
            SceneNode scene = CurrentScene();
            SceneBackgroundImage("DialogueBackground", scene.BackgroundPath, new Color(0.05f, 0.045f, 0.04f, 1f), root);
            FullscreenPanel("DialogueShade", Clear, root);

            actor = FindSceneActor(actorId) ?? new SceneActor
            {
                Id = "qingyi",
                Name = "沈青衣",
                PortraitPath = "Assets/Art/Sprites/Characters/旦2.png",
                Position = new Vector2(-420, -80),
                Size = new Vector2(300, 560)
            };

            ImagePanel("DialogueActor_" + actor.Id, actor.PortraitPath, Clear, new Vector2(-610, -100), new Vector2(300, 555), root);
            ImagePanel("DialogueBox2026", UIWindow + "dialoguebox.png", new Color(0, 0, 0, 0), new Vector2(0, -446), new Vector2(1920, 188), root);
            return root;
        }

        private void ShowDialogueResponse(string actorId, string topicId)
        {
            currentView = View.Dialogue;
            dialogueBeat++;
            RectTransform root = BeginDialogueLayer(actorId, out SceneActor actor);
            DialogueTopic topic = FindDialogueTopic(actor.Id, topicId);
            if (topic == null)
            {
                ShowDialogue(actor.Id);
                return;
            }

            askedDialogueTopics.Add(DialogueTopicKey(actor.Id, topic.Id));
            ImagePanel("DialogueAnswerFrame2026", UIWindow + "widnow.frame01.png", new Color(0, 0, 0, 0), new Vector2(660, 20), new Vector2(500, 300), root);
            Text(actor.Name, 28, Gold, TextAlignmentOptions.Center, new Vector2(-520, -362), new Vector2(300, 42), root);
            UnityEngine.UI.Text questionText = Text("你：" + topic.Question, 24, Muted, TextAlignmentOptions.TopLeft, new Vector2(-5, -379), new Vector2(1160, 38), root);
            ConfigureDialogueText(questionText, 22);
            UnityEngine.UI.Text answerText = Text(actor.Name + "：" + topic.Response, 30, White, TextAlignmentOptions.TopLeft, new Vector2(-5, -460), new Vector2(1160, 90), root);
            ConfigureDialogueText(answerText, 24);

            Text("盘问结论", 26, Gold, TextAlignmentOptions.Center, new Vector2(660, 108), new Vector2(360, 40), root);
            UnityEngine.UI.Text takeawayText = Text(DialogueTakeaway(actor.Id, topic.Id), 22, White, TextAlignmentOptions.TopLeft, new Vector2(660, 25), new Vector2(390, 100), root);
            ConfigureDialogueText(takeawayText, 19);
            Button("继续盘问", new Vector2(585, -92), new Vector2(170, 50), () => ShowDialogue(actor.Id), Red, White);
            ImageButton("DialogueAnswerBack", UIWindow + "back.btn.png", new Vector2(760, -92), new Vector2(140, 46), () => TransitionTo(ShowCrimeScene));
        }

        private string DialogueTopicKey(string actorId, string topicId)
        {
            return actorId + ":" + topicId;
        }

        private DialogueTopic FindDialogueTopic(string actorId, string topicId)
        {
            foreach (DialogueTopic topic in DialogueTopics(actorId))
            {
                if (topic.Id == topicId) return topic;
            }
            return null;
        }

        private string DialogueOpening(string actorId)
        {
            if (actorId == "axi")
                return "阿喜搓着袖口，眼神总往门帘和雅座之间飘。他不是不想说，是怕自己说错一句就被推出去顶罪。";
            if (actorId == "boss")
                return "薛万山站得很稳，说话像在管一场还没散的戏。他要的是戏班不停演，也要旧案永远别翻到台面上。";
            if (actorId == "faceless")
                return "何颂生的声音像从木偶箱里透出来。他不急着辩解，只一遍遍把旧案和今夜摆到同一张桌上。";
            return "沈青衣把水袖拢得很紧。她知道的比她愿意承认的多，但她每句话都绕开了那个已经失踪很久的名字。";
        }

        private DialogueTopic[] DialogueTopics(string actorId)
        {
            if (actorId == "axi")
            {
                return new[]
                {
                    new DialogueTopic { Id = "where", Label = "案发时你在哪", Question = "锣响前后，你到底站在哪里？", Response = "我在侧台门帘边搬箱子。先听见短短一声锣，像是排练里混进去的；再过一会儿才有人喊钟爷倒了。我要是真动手，早该跑，不会还站在门帘边发抖。" },
                    new DialogueTopic { Id = "water", Label = "茶水和药瓶", Question = "钟铁面的水和药瓶，你碰过没有？", Response = "杯子我端过，可端过去时杯子已经摆在桌上，药瓶也开过封。我只是被骂倒水慢，没胆子往嗓药里加东西。真要下药，得知道他什么时候一定会喝水。" },
                    new DialogueTopic { Id = "wire", Label = "细线去向", Question = "你说见过细线，它原本在哪里？", Response = "那根线原来吊景用，后来一直缠在雅座栏杆底下。今天它不在栏杆上，方向却还朝着滑轨。要从外头做门闩机关，得懂这条线怎么绕。" },
                    new DialogueTopic { Id = "night", Label = "夜里谁支开你", Question = "第二天夜里，是谁让你离开侧台？", Response = "班主叫我去前厅盯客人，说别让怪谈传出去。可我走开没多久，门帘那边就又响了。有人知道我怕事，也知道怎么把我从该站的位置挪开。", SecondDayOnly = true }
                };
            }

            if (actorId == "boss")
            {
                return new[]
                {
                    new DialogueTopic { Id = "show", Label = "为何坚持开演", Question = "出了人命，你为什么还坚持夜场照演？", Response = "戏班靠一口气活着。今天停演，明天就是闹鬼，后天所有人都散。我请你来，是要一个能对外说的答案，不是让旧案把新戏吞了。" },
                    new DialogueTopic { Id = "gong", Label = "谁能改锣点", Question = "锣点被改，谁有资格碰公告板？", Response = "公告板人人能看，但不是人人敢改。能改三拍又不让鼓师立刻怀疑的人，必须懂场面，也懂谁会在锣响时回头。" },
                    new DialogueTopic { Id = "keys", Label = "钥匙和封箱", Question = "后台钥匙和旧戏箱是谁管？", Response = "钥匙在账房登记，旧戏箱封条也是我让人贴的。可封条新旧混在一起，这说明有人借着我的规矩藏东西，也想把责任推到管规矩的人身上。" },
                    new DialogueTopic { Id = "oldcase", Label = "旧案名字", Question = "陈伶宜这个名字，你到底知道多少？", Response = "这个名字不该在戏班里再被提起。民国旧案死了人，也毁了一个班子。我知道它危险，所以我压着；但压着旧案，不等于我能让死人按我的意思回来。", SecondDayOnly = true }
                };
            }

            if (actorId == "faceless")
            {
                return new[]
                {
                    new DialogueTopic { Id = "ash", Label = "香灰为何是新的", Question = "脸谱盒里的香灰为什么还新？", Response = "因为有人要把旧物做成刚从火里取出的样子。旧案不会自己发热，旧物也不会自己摆成证词。你看到的不是鬼，是有人替鬼布景。" },
                    new DialogueTopic { Id = "ticket", Label = "旧戏票背面", Question = "旧戏票背面的字是谁写的？", Response = "不是写给观众看的，是写给后来查案的人看的。那晚有人把一个名字从戏单里抹掉；现在又有人想把它写回来，让每个人都以为旧案亲自回了头。" },
                    new DialogueTopic { Id = "puppet", Label = "定情木偶", Question = "你为什么一直提那对木偶？", Response = "木偶不会说谎，它只记得谁把线系在它身上。定情木偶本该是活人的念想，后来却成了旧案的锁。箱子开了，谁在借旧情装神弄鬼就藏不住。" },
                    new DialogueTopic { Id = "truth", Label = "谁在幕后", Question = "你觉得真正的幕后人是谁？", Response = "不是最会哭的人，也不是最会逃的人。幕后人要同时知道旧案、锣点、钥匙和人心。他站在看不见的地方，让每个人都以为自己只是被旧戏推了一把。", SecondDayOnly = true }
                };
            }

            return new[]
            {
                new DialogueTopic { Id = "ticket", Label = "旧戏票", Question = "旧戏票上的名字，为什么让你这么害怕？", Response = "因为那不是传闻，是戏班里被人硬生生擦掉的一页。陈伶宜不是怪谈，她曾经站在台上，也曾经有人不许她再站上去。" },
                new DialogueTopic { Id = "mask", Label = "脸谱盒", Question = "烧焦脸谱为什么会在今天出现？", Response = "它不是自己回来的。有人把旧案的火灰摆到今天的桌上，是想让所有人先信鬼，再忘了去问活人能做什么。" },
                new DialogueTopic { Id = "latch", Label = "反锁机关", Question = "门闩从外面反扣，你以前见过这种手法吗？", Response = "旧戏班有过类似的把戏，用来让后台小屋看起来没人进出。知道这个办法的人不多，知道它和旧案有关的人更少。" },
                new DialogueTopic { Id = "confess", Label = "你隐瞒了什么", Question = "你一直绕开旧案，是在护谁？", Response = "我不是护凶手。我护的是一个已经没有机会替自己说话的人。可如果今夜有人借她的名字害人，我不会再替任何活人沉默。", SecondDayOnly = true }
            };
        }

        private string DialogueTakeaway(string actorId, string topicId)
        {
            if (actorId == "axi")
                return "阿喜的说法把自己放在侧台门帘旁：他能听见锣点和门帘动静，但缺少设计整套机关的胆量和位置。";
            if (actorId == "boss")
                return "薛万山承认自己控制演出、钥匙和规矩，但他的辩解重点是：有人正在利用这些规矩嫁祸给管事的人。";
            if (actorId == "faceless")
                return "何颂生把旧案解释成一套被重新布置的舞台效果：香灰、戏票和木偶都是人为摆出的旧案回声。";
            return "沈青衣承认旧案不是传闻。她隐瞒的不是机关手法本身，而是陈伶宜这个名字与当年戏班的关系。";
        }

        private SceneActor FindSceneActor(string actorId)
        {
            foreach (SceneActor actor in sceneActors)
            {
                if (actor.Id == actorId) return actor;
            }
            return null;
        }

        private string DialogueLine(string actorId)
        {
            if (actorId == "axi")
            {
                if (HasClue("wire"))
                    return PickDialogue("我看见过那根细线，原本是吊景用的。后来不用了，就一直缠在雅座栏杆底下。\n可今天它不在栏杆上，像是有人刚拿它做过什么。",
                        "您别只盯着我。我偷懒归偷懒，真要把门闩从外面扣上，那得知道滑轨的方向。这个后台，知道方向的人不止我。");
                if (HasClue("medicine"))
                    return PickDialogue("钟爷今天嗓子干，骂我倒水太慢。我端过去的时候杯子已经在桌上了，药瓶也是。\n我承认我碰过杯子，可我没往里加东西。",
                        "阿喜我胆小，但不傻。药这种东西一查就露馅，谁会让我这种打杂的背这么大的事？");
                return PickDialogue("我只是在侧台帮忙搬东西。门闩那边响了一下，我以为是风，真的只是风。",
                    "锣响的时候我就在门帘边，先是短短一下，后面才有人喊钟爷倒了。那一声锣，听着不像排练。");
            }

            if (actorId == "boss")
            {
                if (HasClue("gong"))
                    return PickDialogue("锣鼓点错一拍，演员会乱；错三拍，整个后台都会替它闭嘴。\n你问是谁改的？公告板人人能看，但不是人人敢动。",
                        "你把锣点查出来了，很好。可排练表不是凶器，拿着表的人也未必亲自动手。");
                if (HasClue("record"))
                    return PickDialogue("戏班有戏班的规矩。道具谁领、钥匙谁拿，账上都写得清楚。\n你要查，可以查。但别把旧账翻成新罪。",
                        "夜场《断面》是为了让戏活过来，不是为了让死人回来。传闻这种东西，越拦越像真的。");
                return PickDialogue("侦探先生，我请你来，是为了压住怪谈，不是让戏班停演。\n今天白天你可以查，天黑之前给我一个能对外说的答案。",
                    "钟铁面脾气坏，得罪人不奇怪。但后台这么多人，能让所有人都以为是旧案重演的人，才是真的麻烦。");
            }

            if (HasClue("ash"))
                return PickDialogue("这香灰太新了。旧案不会自己发热，旧物也不会刚烧过还装作九十三年前的灰。\n有人想让你闻到历史的味道。",
                    "别碰那张脸谱。不是怕脏，是怕有人已经把它摆成了他想让你看的样子。");
            if (HasClue("ticket"))
                return PickDialogue("旧戏票背面的字，不是写给观众看的。\n民国那晚，有人把一个名字从戏单里抹掉了。现在又有人想把它写回来。",
                    "你问我为什么懂旧案？因为有些故事，台上不唱，台下也会一代一代传下去。");
            return PickDialogue("戏没有想不想。锣鼓点响了，人就该站到该站的位置。\n有人站台前，有人站幕后，最要命的是有人一直站在看不见的地方。",
                "我不信鬼。鬼不会知道谁喝哪只杯子，谁拿哪把钥匙，谁会在锣响的时候回头。");
        }

        private bool HasClue(string clueId)
        {
            return clues.TryGetValue(clueId, out Clue clue) && clue.Acquired;
        }

        private string PickDialogue(params string[] lines)
        {
            if (lines == null || lines.Length == 0) return string.Empty;
            int index = Mathf.Abs(dialogueBeat) % lines.Length;
            return lines[index];
        }

        private void ShowClues()
        {
            currentView = View.Clues;
            RectTransform root = BeginLayer(normalCanvas, "ClueLayer", false);
            FullscreenPanel("ClueBlackBackground", Clear, root);
            ImagePanel("ClueFrame2026", UIWindow + "widnow.frame02.png", new Color(0, 0, 0, 0), new Vector2(0, 10), new Vector2(1321, 614), root);
            Text("线索背包", 52, Gold, TextAlignmentOptions.Center, new Vector2(0, 300), new Vector2(760, 80), root);
            Text("条状证据用于擦雾和推理判定；对话已改为纯盘问选项。再次按 C 返回游戏界面。", 22, Muted, TextAlignmentOptions.Center, new Vector2(0, 245), new Vector2(1000, 44), root);
            int index = 0;
            foreach (Clue clue in clues.Values)
            {
                if (!clue.Acquired) continue;
                Vector2 pos = new Vector2(-560 + (index % 2) * 620, 220 - (index / 2) * 92);
                EvidenceStrip(root, clue, pos, () => ShowClueDetail(root, clue));
                index++;
            }
            if (index == 0)
                Text("尚未取得证据。\n回到案发现场，点击异常位置记录证据。", 30, White, TextAlignmentOptions.Center, Vector2.zero, new Vector2(760, 140), root);
            Button("地图 M", new Vector2(470, -405), new Vector2(220, 56), ToggleSceneMap);
            ImageButton("ClueBack", UIWindow + "back.btn.png", new Vector2(720, -405), new Vector2(201, 65), () => TransitionTo(ShowCrimeScene));
        }

        private void ShowClueDetail(RectTransform root, Clue clue)
        {
            Panel("DetailPaper", Paper, new Vector2(420, -80), new Vector2(560, 410), root);
            Text(clue.Name, 34, PaperText, TextAlignmentOptions.Left, new Vector2(210, 50), new Vector2(480, 50), root);
            Text($"来源：{clue.Source}\n关键词：{clue.Tag}\n\n{clue.Detail}", 24, PaperText, TextAlignmentOptions.TopLeft, new Vector2(420, -100), new Vector2(470, 260), root);
        }

        private void ShowArchive()
        {
            currentView = View.Archive;
            RectTransform root = BeginLayer(normalCanvas, "ArchiveLayer", false);
            FullscreenPanel("ArchiveBlackBackground", Clear, root);
            ImagePanel("ArchiveFrame2026", UIWindow + "widnow.frame02.png", new Color(0, 0, 0, 0), new Vector2(0, 5), new Vector2(1321, 614), root);
            Text("人物档案", 46, Gold, TextAlignmentOptions.Center, new Vector2(0, 368), new Vector2(760, 64), root);
            Text("口供、旧案关系和当前嫌疑程度。", 21, Muted, TextAlignmentOptions.Center, new Vector2(0, 320), new Vector2(900, 36), root);
            HeartBar(root, new Vector2(675, 370));
            int index = 0;
            foreach (Character c in characters.Values)
            {
                Vector2 pos = new Vector2(-350 + (index % 2) * 700, 150 - (index / 2) * 245);
                Panel("ArchiveCard", Clear, pos, new Vector2(620, 205), root);
                Panel("ArchiveCardLine", Gold, pos + new Vector2(0, 82), new Vector2(560, 3), root);
                Text(c.Name, 29, Gold, TextAlignmentOptions.Left, pos + new Vector2(-245, 52), new Vector2(260, 42), root);
                Text(c.Role, 18, Muted, TextAlignmentOptions.Right, pos + new Vector2(175, 52), new Vector2(300, 34), root);
                Text("已知：" + c.Known, 18, White, TextAlignmentOptions.TopLeft, pos + new Vector2(0, -8), new Vector2(540, 62), root);
                Text("疑点：" + c.Suspicion, 18, White, TextAlignmentOptions.TopLeft, pos + new Vector2(0, -70), new Vector2(540, 62), root);
                index++;
            }
            Button("返回", new Vector2(0, -398), new Vector2(220, 56), () => TransitionTo(ShowCrimeScene));
        }

        private void ShowObserve()
        {
            currentView = View.Observe;
            RectTransform root = BeginLayer(revealCanvas, "ObserveLayer");
            Title("擦雾", "选择证据擦开人物表层迷雾，选错扣心。", root);
            Character c = characters["qingyi"];
            ImagePanel("ObservePortraitFrame", UIWindow + "widnow.frame02.png", Clear, new Vector2(-430, -40), new Vector2(520, 650), root);
            Image portrait = ImagePanel("ObservePortrait", "Assets/Art/Sprites/Characters/旦2.png", Clear, new Vector2(-430, -70), new Vector2(360, 560), root);
            portrait.raycastTarget = false;
            Text(c.Name, 42, Gold, TextAlignmentOptions.Center, new Vector2(-430, 260), new Vector2(360, 60), root);
            float fogAmount = Mathf.Clamp01(1f - c.Reveal);
            Text("剩余迷雾：" + Mathf.RoundToInt(fogAmount * 100f) + "%", 22, Muted, TextAlignmentOptions.Center, new Vector2(-430, -354), new Vector2(360, 36), root);
            DrawFogVeil(root, new Vector2(-430, -70), new Vector2(360, 560), fogAmount);
            Panel("EvidenceDock", Clear, new Vector2(330, 40), new Vector2(620, 410), root);
            Text("可用于擦雾的证据", 32, Gold, TextAlignmentOptions.Center, new Vector2(330, 205), new Vector2(500, 48), root);
            int i = 0;
            foreach (string id in new[] { "ticket", "ash", "flow", "bodymark", "medicine", "gong", "latch", "wire" })
            {
                if (!clues.TryGetValue(id, out Clue clue) || !clue.Acquired) continue;
                EvidenceStrip(root, clue, new Vector2(330, 130 - i * 72), () => Toast("证据已放入擦雾槽"), new Vector2(480, 58));
                i++;
                if (i >= 4) break;
            }
            if (i == 0)
                Text("当前还没有可用于擦雾的证据。\n先回到案发现场完成侦察。", 26, Muted, TextAlignmentOptions.Center, new Vector2(330, 60), new Vector2(520, 120), root);
            Button("擦雾判定", new Vector2(250, -245), new Vector2(260, 66), () => { if (clues.TryGetValue("ticket", out Clue ticket)) ticket.Used = true; c.Reveal = Mathf.Clamp01(c.Reveal + 0.2f); Toast("擦雾成功：第一张皮开始脱落"); TransitionTo(ShowObserve); });
            Button("错误示例 -1心", new Vector2(540, -245), new Vector2(260, 66), () => LoseHeart("证据组合错误"));
            Button("返回白天", new Vector2(690, -405), new Vector2(220, 56), () => TransitionTo(ReturnFromFogToDay));
        }

        private void DrawFogVeil(RectTransform root, Vector2 center, Vector2 size, float amount)
        {
            float clamped = Mathf.Clamp01(amount);
            if (clamped <= 0.03f) return;

            Panel("FogBase", new Color(0.58f, 0.60f, 0.62f, 0.34f * clamped), center, size, root).raycastTarget = false;
            Panel("FogEdgeTop", new Color(0.82f, 0.84f, 0.82f, 0.22f * clamped), center + new Vector2(0, size.y * 0.30f), new Vector2(size.x * 0.92f, 86), root).raycastTarget = false;
            Panel("FogEdgeBottom", new Color(0.70f, 0.72f, 0.72f, 0.18f * clamped), center + new Vector2(0, -size.y * 0.28f), new Vector2(size.x * 0.86f, 76), root).raycastTarget = false;

            for (int i = 0; i < 6; i++)
            {
                float y = Mathf.Lerp(-size.y * 0.36f, size.y * 0.34f, i / 5f);
                float x = ((i % 2) == 0 ? -32f : 28f);
                float alpha = (0.18f + i * 0.018f) * clamped;
                Vector2 bandSize = new Vector2(size.x * (0.72f + i * 0.045f), 34f + i * 7f);
                Panel("FogBand_" + i, new Color(0.88f, 0.88f, 0.82f, alpha), center + new Vector2(x, y), bandSize, root).raycastTarget = false;
            }

            Panel("FogInkEdge", Clear, center, new Vector2(size.x, size.y), root).raycastTarget = false;
        }

        private void OpenCurrentTrial()
        {
            if (!CanOpenTrial())
            {
                LoseHeart("证据不足");
                return;
            }

            ShowTrial();
        }

        private void ShowTrial()
        {
            currentView = View.Trial;
            RectTransform root = BeginLayer(normalCanvas, "TrialLayer");
            bool secondDayDay = currentDay == 2 && !isNightPhase;
            string trialTitle = currentDay == 1 ? "第一天白天判定" : secondDayDay ? "第二天白天整理" : "第二天夜晚判定";
            string trialSubtitle = currentDay == 1
                ? "证明后台事故不是旧案诅咒，而是人为复刻。"
                : secondDayDay ? "第二天白天新线索已经确认，夜场即将开锣。" : "确认第二天夜场前，凶手再次布置了重演机关。";
            Title(trialTitle, trialSubtitle, root);
            string[] ids = currentDay == 1
                ? new[] { "bodymark", "medicine", "gong", "latch", "wire", "ash", "flow", "thread" }
                : new[] { "curtain_thread", "night_gong", "case_seal", "thread", "flow" };
            int shown = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                if (!clues.ContainsKey(ids[i]) || !clues[ids[i]].Acquired) continue;
                EvidenceStrip(root, clues[ids[i]], new Vector2(-560 + (i % 2) * 620, 170 - (i / 2) * 92), () => Toast("证据已选择"));
                shown++;
            }
            if (shown == 0)
                Text("推理判定需要现场证据。\n先完成后台案发现场侦察。", 30, White, TextAlignmentOptions.Center, Vector2.zero, new Vector2(760, 140), root);
            string submitLabel = currentDay == 1 ? "进入第一天夜晚" : secondDayDay ? "进入第二天夜晚" : "结束 Demo";
            Button(submitLabel, new Vector2(460, -335), new Vector2(260, 66), () =>
            {
                if (!CanOpenTrial())
                {
                    LoseHeart("证据不足");
                    return;
                }

                if (currentDay == 1) TransitionTo(AdvanceToFirstNight);
                else if (secondDayDay) TransitionTo(AdvanceToSecondNight);
                else TransitionTo(ShowEnding);
            }, Red, White);
            Button("提交错误链条", new Vector2(750, -335), new Vector2(260, 66), () => LoseHeart("缺少关键钥匙或操控证据"));
            Button("返回", new Vector2(720, -430), new Vector2(220, 56), () => TransitionTo(ShowCrimeScene));
        }

        private void AdvanceToFirstNight()
        {
            currentDay = 1;
            isNightPhase = true;
            currentSceneId = "stage";
            ShowPhaseTransition("第一天 · 夜晚", "NIGHT TIME", "夜场开锣，后台灯影压低。白天查到的机关开始在夜色里连成一条线。", "进入擦雾阶段", ShowNightToFog);
        }

        private void ShowNightToFog()
        {
            ShowPhaseTransition("擦雾阶段", "FOG WIPE", "夜晚不是调查的终点。用证据擦开人物表层的雾，看看谁的说法露出了底色。", "开始擦雾", ShowObserve);
        }

        private void ReturnFromFogToDay()
        {
            if (currentDay == 1 && isNightPhase)
            {
                AdvanceToSecondDay();
                return;
            }

            ShowPhaseTransition(PhaseTitle(), "DAY TIME", "擦雾结束，回到现场继续调查。", "返回白天", () =>
            {
                isNightPhase = false;
                Time.timeScale = 1f;
                ShowCrimeScene();
            });
        }

        private void AdvanceToSecondDay()
        {
            dayOneTrialComplete = true;
            currentDay = 2;
            isNightPhase = false;
            dayTwoNightStarted = false;
            currentSceneId = "side";
            ShowPhaseTransition("第二天 · 白天", "DAY TIME", "擦雾结束。钟铁面被送医后，后台反而更安静。第一天无法确认的线头、封条和锣位，都在第二天出现了新的痕迹。", "开始第二天调查", () =>
            {
                Time.timeScale = 1f;
                ShowCrimeScene();
            });
        }

        private void AdvanceToSecondNight()
        {
            currentDay = 2;
            isNightPhase = true;
            dayTwoNightStarted = true;
            currentSceneId = "stage";
            ShowPhaseTransition("第二天 · 夜晚", "NIGHT TIME", "夜场开锣。锣声再次提前，旧戏箱的封条也被人重新贴过。现在可以进行第二天夜晚判定。", "进入夜晚判定", ShowTrial);
        }

        private void ShowDayTransition(string title, string body, string buttonLabel, Action next)
        {
            ShowPhaseTransition(title, "NEXT PHASE", body, buttonLabel, next);
        }

        private void ShowPhaseTransition(string title, string englishTitle, string body, string buttonLabel, Action next)
        {
            currentView = View.DayTransition;
            Time.timeScale = 0f;
            RectTransform root = BeginLayer(systemCanvas, "DayTransitionLayer");
            SceneBackgroundImage("TransitionBackground", CurrentScene().BackgroundPath, new Color(0.04f, 0.032f, 0.028f, 1f), root);
            FullscreenPanel("TransitionShade", StoryShade, root);

            Panel("PhaseSlashTop", Red, new Vector2(-620, 170), new Vector2(980, 34), root);
            Panel("PhaseSlashMid", Gold, new Vector2(0, 46), new Vector2(1540, 6), root);
            Panel("PhaseSlashBottom", Red, new Vector2(620, -176), new Vector2(980, 34), root);
            Panel("PhaseBox", StoryBackdrop, Vector2.zero, new Vector2(1180, 430), root);
            Panel("PhaseBoxLine", Gold, new Vector2(0, 160), new Vector2(1040, 5), root);

            Text(englishTitle, 30, Red, TextAlignmentOptions.Center, new Vector2(0, 112), new Vector2(840, 44), root);
            Text(title, 64, Gold, TextAlignmentOptions.Center, new Vector2(0, 48), new Vector2(940, 86), root);
            UnityEngine.UI.Text phaseBodyText = Text(body, StoryBodyFontSize, White, TextAlignmentOptions.Center, new Vector2(0, -72), new Vector2(1040, 160), root);
            ConfigureStoryBodyText(phaseBodyText);
            Text("PHASE SHIFT", 18, Muted, TextAlignmentOptions.Center, new Vector2(0, -150), new Vector2(520, 30), root);
            Button(buttonLabel, StoryPrimaryButtonPos, new Vector2(290, 62), () => TransitionTo(() =>
            {
                Time.timeScale = 1f;
                next?.Invoke();
            }), Red, White);
        }

        private void ShowEnding()
        {
            demoCompleted = true;
            currentView = View.Ending;
            Time.timeScale = 0f;
            RectTransform root = BeginLayer(systemCanvas, "EndingLayer");
            SceneBackgroundImage("EndingBackground", "Assets/Art/Sprites/Backgrounds/戏曲舞台.png", new Color(0.04f, 0.032f, 0.028f, 1f), root);
            FullscreenPanel("EndingShade", Clear, root);
            Text("未完待续", 76, Gold, TextAlignmentOptions.Center, new Vector2(0, 90), new Vector2(900, 110), root);
            Text("第二天夜晚，旧案重演的机关已经露出形状。\n但真正能调动戏班、旧箱、锣点和更夫的人，还没有站到台前。", 28, White, TextAlignmentOptions.Center, new Vector2(0, -45), new Vector2(980, 130), root);
            Button("回到标题", StorySecondaryButtonPos, new Vector2(240, 62), () => TransitionTo(ShowMainMenu), Dark, White);
            Button("继续查看现场", StoryPrimaryButtonPos, new Vector2(260, 62), () =>
            {
                Time.timeScale = 1f;
                TransitionTo(ShowCrimeScene);
            }, Red, White);
        }

        private void TogglePause()
        {
            if (isTransitioning) return;
            if (currentView == View.Pause)
            {
                if (pauseOpenedFromMainMenu)
                {
                    pauseOpenedFromMainMenu = false;
                    TransitionTo(ShowMainMenu);
                    return;
                }

                TransitionTo(() =>
                {
                    Time.timeScale = 1f;
                    ShowCrimeScene();
                });
                return;
            }

            pauseOpenedFromMainMenu = false;
            TransitionTo(ShowPauseMenu);
        }

        private void ShowPauseMenu()
        {
            currentView = View.Pause;
            Time.timeScale = 0f;
            RectTransform root = BeginLayer(systemCanvas, "PauseLayer", false);
            if (pauseOpenedFromMainMenu)
            {
                FullscreenDesignImage("SettingsBackground2026", UISettings + "02settings_bg.jpg", Clear, root);
                ImagePanel("SettingsFrame2026", UISettings + "02settings_widnow.frame2.png", Clear, Vector2.zero, new Vector2(1920, 1080), root);
                SettingsSlider("MusicVolumeSlider", new Vector2(50, 143), musicVolume, value => SetMusicVolume(value));
                SettingsSlider("SfxVolumeSlider", new Vector2(50, 32), sfxVolume, value => SetSfxVolume(value));
                SettingsSlider("TextSpeedSlider", new Vector2(50, -78), textSpeed, value => textSpeed = value);
                TransparentButton("SettingsClose", new Vector2(830, 455), new Vector2(120, 120), TogglePause);
                TransparentButton("SettingsFullscreenToggle", new Vector2(15, -160), new Vector2(220, 80), () => Screen.fullScreen = !Screen.fullScreen);
                return;
            }

            FullscreenDesignImage("PauseDesign2026", UIHome + "01home4.jpg", Clear, root);
            TransparentButton("PauseClose", new Vector2(520, 190), new Vector2(110, 110), TogglePause);
            TransparentButton("PauseResume", new Vector2(0, 115), new Vector2(360, 58), TogglePause);
            TransparentButton("PauseMainMenu", new Vector2(0, 55), new Vector2(360, 58), () => TransitionTo(ShowMainMenu));
            TransparentButton("PauseSaveLoad", new Vector2(0, -5), new Vector2(360, 58), () => TransitionTo(() => ShowSaveLoad(true)));
            TransparentButton("PauseQuitGame", new Vector2(0, -65), new Vector2(360, 58), () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }

        private void ShowSaveLoad(bool fromPause)
        {
            currentView = View.SaveLoad;
            Time.timeScale = 0f;
            RectTransform root = BeginLayer(systemCanvas, "SaveLoadLayer", false);
            FullscreenDesignImage("SaveLoadDesign2026", UIHome + "01home5.jpg", Clear, root);
            if (!RuntimeImageAssetsAvailable)
            {
                Text(fromPause ? "保存进度" : "选择存档", 48, Gold, TextAlignmentOptions.Center, new Vector2(0, 210), new Vector2(720, 72), root);
                Text(fromPause ? "点击一个槽位覆盖保存" : "点击空槽开始新游戏", 24, Muted, TextAlignmentOptions.Center, new Vector2(0, 155), new Vector2(900, 42), root);
            }

            for (int i = 0; i < SaveManager.SlotIds.Length; i++)
            {
                string slotId = SaveManager.SlotIds[i];
                bool exists = SaveManager.Exists(slotId);
                Vector2 pos = new Vector2(-370 + i * 250, 8);
                if (!RuntimeImageAssetsAvailable)
                {
                    Button(exists ? "存档 " + slotId : "新游戏 " + slotId, pos + new Vector2(0, 35), new Vector2(180, 110), () =>
                    {
                        if (fromPause) SaveToSlot(slotId);
                        else if (exists) TransitionTo(() => LoadFromSlot(slotId));
                        else TransitionTo(() => StartNewGameInSlot(slotId));
                    }, exists ? Gold : Red, exists ? PaperText : White);
                }
                else if (exists)
                    Text(SaveManager.GetSlotShortSummary(slotId), 18, White, TextAlignmentOptions.Center, pos + new Vector2(0, -178), new Vector2(170, 72), root);
                TransparentButton("SaveSlot_" + slotId, pos + new Vector2(0, 35), new Vector2(170, 170), () =>
                {
                    if (fromPause) SaveToSlot(slotId);
                    else if (exists) TransitionTo(() => LoadFromSlot(slotId));
                    else TransitionTo(() => StartNewGameInSlot(slotId));
                });
            }

            if (!RuntimeImageAssetsAvailable)
                Button("返回", new Vector2(0, -260), new Vector2(220, 62), () =>
                {
                    if (fromPause) TransitionTo(ShowPauseMenu);
                    else TransitionTo(ShowMainMenu);
                }, Dark, White);
            TransparentButton("SaveBack", new Vector2(-835, -485), new Vector2(210, 90), () =>
            {
                if (fromPause) TransitionTo(ShowPauseMenu);
                else TransitionTo(ShowMainMenu);
            });
        }

        private void SaveToSlot(string slotId)
        {
            SaveData data = CreateSaveData(slotId);
            SaveManager.Save(slotId, data);
            ShowSaveLoad(true);
            Toast($"已保存到槽位 {slotId}");
        }

        private void StartNewGameInSlot(string slotId)
        {
            ResetGameState();
            currentSceneId = "props";
            gameplayBackgroundPath = CurrentScene().BackgroundPath;
            SaveData data = CreateSaveData(slotId);
            data.displayTitle = "画皮 " + slotId;
            data.uiViewName = View.Intro.ToString();
            data.introCompleted = false;
            data.phaseName = "开场";
            data.gameplayBackgroundPath = gameplayBackgroundPath;
            data.collectedClueIDs = Array.Empty<string>();
            SaveManager.Save(slotId, data);
            ShowIntro(0);
        }

        private void ResetGameState()
        {
            hearts = 5;
            currentDay = 1;
            isNightPhase = false;
            dayOneTrialComplete = false;
            dayTwoNightStarted = false;
            demoCompleted = false;
            introIndex = 0;
            monologueIndex = 0;
            facePuzzleStep = 0;
            latchPuzzleStep = 0;
            gongPuzzleStep = 0;
            wirePuzzleStep = 0;
            casePuzzleStep = 0;
            askedDialogueTopics.Clear();
            evidenceBoardVisible = false;
            currentSceneId = "props";
            gameplayBackgroundPath = PropsRoomBackground;
            foreach (Clue clue in clues.Values)
            {
                clue.Acquired = false;
                clue.Used = false;
            }
            if (crimeSceneHotspots == null) return;
            foreach (InvestigationHotspot hotspot in crimeSceneHotspots)
                hotspot.Examined = false;
        }

        private void LoadFromSlot(string slotId)
        {
            if (!SaveManager.TryLoad(slotId, out SaveData data))
            {
                Toast($"槽位 {slotId} 为空");
                return;
            }

            ApplySaveData(data);
            if (!data.introCompleted)
                ShowIntro(0);
            else if (demoCompleted)
                ShowEnding();
            else
                ShowCrimeScene();
            Toast($"已读取槽位 {slotId}");
        }

        private SaveData CreateSaveData(string slotId)
        {
            List<string> acquiredClues = new List<string>();
            foreach (Clue clue in clues.Values)
            {
                if (clue.Acquired)
                    acquiredClues.Add(clue.Id);
            }

            return new SaveData
            {
                slotId = slotId,
                displayTitle = "画皮 " + slotId,
                currentDay = currentDay,
                currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                gameTime = Time.realtimeSinceStartup,
                hearts = hearts,
                uiViewName = currentView.ToString(),
                introCompleted = currentView != View.MainMenu && currentView != View.Intro,
                phaseName = PhaseTitle(),
                gameplayBackgroundPath = gameplayBackgroundPath,
                collectedClueIDs = acquiredClues.ToArray(),
                completedDialogueIDs = Array.Empty<string>(),
                characterProgresses = Array.Empty<CharacterProgress>(),
                currentPhase = isNightPhase ? GamePhase.夜晚观皮 : GamePhase.白天探索
            };
        }

        private void ApplySaveData(SaveData data)
        {
            hearts = Mathf.Clamp(data.hearts <= 0 ? 5 : data.hearts, 0, 5);
            currentDay = Mathf.Max(1, data.currentDay <= 0 ? 1 : data.currentDay);
            isNightPhase = data.currentPhase == GamePhase.夜晚观皮 || (!string.IsNullOrEmpty(data.phaseName) && data.phaseName.Contains("夜晚"));
            dayOneTrialComplete = currentDay >= 2;
            dayTwoNightStarted = currentDay >= 2 && isNightPhase;
            demoCompleted = data.uiViewName == View.Ending.ToString();
            gameplayBackgroundPath = string.IsNullOrEmpty(data.gameplayBackgroundPath)
                ? "Assets/Art/Sprites/Generated/generated_backstage_corridor.png"
                : data.gameplayBackgroundPath;
            currentSceneId = SceneIdFromBackground(gameplayBackgroundPath);
            if (sceneNodes.TryGetValue(currentSceneId, out SceneNode normalizedScene))
                gameplayBackgroundPath = normalizedScene.BackgroundPath;
            foreach (Clue clue in clues.Values)
                clue.Acquired = false;
            if (data.collectedClueIDs != null)
            {
                foreach (string clueId in data.collectedClueIDs)
                {
                    if (clues.TryGetValue(clueId, out Clue clue))
                        clue.Acquired = true;
                }
            }
            if (crimeSceneHotspots != null)
            {
                foreach (InvestigationHotspot hotspot in crimeSceneHotspots)
                {
                    hotspot.Examined = !string.IsNullOrEmpty(hotspot.ClueId)
                        && clues.TryGetValue(hotspot.ClueId, out Clue clue)
                        && clue.Acquired;
                }
            }
        }

        private string SceneIdFromBackground(string backgroundPath)
        {
            if (!string.IsNullOrEmpty(backgroundPath))
            {
                string normalized = backgroundPath.Replace('\\', '/').ToLowerInvariant();
                if (normalized.Contains("generated_props_crime_scene")
                    || normalized.Contains("bg_props_room_panorama")
                    || normalized.Contains("props")
                    || normalized.Contains("道具")
                    || normalized.Contains("盔头"))
                    return "props";
            }
            foreach (SceneNode scene in sceneNodes.Values)
            {
                if (string.Equals(scene.BackgroundPath, backgroundPath, StringComparison.OrdinalIgnoreCase))
                    return scene.Id;
            }
            return "props";
        }

        private void LoseHeart(string reason)
        {
            if (hearts <= 0 || currentView == View.GameOver) return;

            hearts = Mathf.Max(0, hearts - 1);
            if (hearts <= 0)
            {
                ShowGameOver(reason);
                return;
            }

            Toast($"心 -1：{reason}");
        }

        private void ShowGameOver(string reason)
        {
            currentView = View.GameOver;
            Time.timeScale = 0f;
            RectTransform root = BeginLayer(systemCanvas, "GameOverLayer");
            SceneBackgroundImage("GameOverBackground", CurrentScene().BackgroundPath, new Color(0.04f, 0.032f, 0.028f, 1f), root);
            FullscreenPanel("GameOverShade", Clear, root);
            HeartBar(root, new Vector2(0, 250));
            Text("调查失败", 76, Red, TextAlignmentOptions.Center, new Vector2(0, 130), new Vector2(900, 110), root);
            Text($"最后失误：{reason}\n心力耗尽，线索链条断裂。请重新开始调查。", 28, White, TextAlignmentOptions.Center, new Vector2(0, 0), new Vector2(980, 130), root);
            Button("重新开始", StorySecondaryButtonPos, new Vector2(240, 62), () => TransitionTo(() =>
            {
                Time.timeScale = 1f;
                ResetGameState();
                ShowCrimeScene();
            }), Red, White);
            Button("回到标题", StoryPrimaryButtonPos, new Vector2(240, 62), () => TransitionTo(ShowMainMenu), Dark, White);
        }

        private void Toast(string message)
        {
            PlayHintSound();
            RectTransform root = currentLayer != null ? currentLayer : BeginLayer(popupCanvas, "ToastLayer", false);
            Image toast = Panel("Toast", Paper, new Vector2(0, -510), new Vector2(780, 48), root);
            Text(message, 21, PaperText, TextAlignmentOptions.Center, Vector2.zero, new Vector2(730, 38), toast.rectTransform);
            Destroy(toast.gameObject, 1.7f);
        }

        private void Title(string title, string subtitle, RectTransform root)
        {
            Text(title, 54, Gold, TextAlignmentOptions.Center, new Vector2(0, 405), new Vector2(900, 70), root);
            Text(subtitle, 24, Muted, TextAlignmentOptions.Center, new Vector2(0, 350), new Vector2(1300, 42), root);
            HeartBar(root, new Vector2(690, 405));
        }

        private void EvidenceStrip(RectTransform parent, Clue clue, Vector2 pos, Action onClick)
        {
            EvidenceStrip(parent, clue, pos, onClick, new Vector2(520, 66));
        }

        private void EvidenceStrip(RectTransform parent, Clue clue, Vector2 pos, Action onClick, Vector2 size)
        {
            Button(clue.Name, pos, size, onClick, Paper, PaperText);
            Text(clue.Tag, 17, new Color(0.20f, 0.10f, 0.06f, 0.72f), TextAlignmentOptions.Left, pos + new Vector2(-size.x * 0.40f, -size.y * 0.28f), new Vector2(size.x * 0.78f, 24), parent);
        }

        private void SetupAudio()
        {
            uiAudioSource = gameObject.GetComponent<AudioSource>();
            if (uiAudioSource == null)
                uiAudioSource = gameObject.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
            uiAudioSource.loop = false;
            uiAudioSource.spatialBlend = 0f;
            uiAudioSource.volume = sfxVolume;
            clickClip = CreateToneClip("Huapi_Click", 0.055f, 420f, 230f, 0.16f);
            hintClip = CreateHintClip();

            bgmAudioSource = gameObject.AddComponent<AudioSource>();
            bgmAudioSource.playOnAwake = false;
            bgmAudioSource.loop = true;
            bgmAudioSource.mute = false;
            bgmAudioSource.ignoreListenerPause = true;
            bgmAudioSource.spatialBlend = 0f;
            bgmAudioSource.volume = musicVolume;
            EnsureBgmPlaying();
        }

        private void EnsureBgmPlaying()
        {
            if (bgmAudioSource == null) return;

            EnsureAudioCanBeHeard();
            bgmAudioSource.volume = musicVolume;
            AudioClip bgm = Resources.Load<AudioClip>("Audio/BGM/Midnight in the Empty Theatre");
            if (bgm != null)
            {
                if (bgm.loadState == AudioDataLoadState.Unloaded)
                    bgm.LoadAudioData();
                if (bgmAudioSource.clip != bgm)
                    bgmAudioSource.clip = bgm;
                if (!bgmAudioSource.isPlaying)
                    bgmAudioSource.Play();
            }
            else
            {
                Debug.LogWarning("未找到 BGM：Resources/Audio/BGM/Midnight in the Empty Theatre");
            }
        }

        private void EnsureAudioCanBeHeard()
        {
            AudioListener.pause = false;
            AudioListener.volume = 1f;
            if (FindObjectOfType<AudioListener>() != null) return;

            Camera camera = Camera.main;
            if (camera != null)
                camera.gameObject.AddComponent<AudioListener>();
            else
                gameObject.AddComponent<AudioListener>();
        }

        private void PlayClickSound()
        {
            if (uiAudioSource != null && clickClip != null)
                uiAudioSource.PlayOneShot(clickClip, 0.38f);
        }

        private void PlayHintSound()
        {
            if (uiAudioSource != null && hintClip != null)
                uiAudioSource.PlayOneShot(hintClip, 0.30f);
        }

        private void AdjustMusicVolume(float delta)
        {
            SetMusicVolume(musicVolume + delta);
        }

        private void AdjustSfxVolume(float delta)
        {
            SetSfxVolume(sfxVolume + delta);
        }

        private void SetMusicVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            if (bgmAudioSource != null)
                bgmAudioSource.volume = musicVolume;
        }

        private void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            if (uiAudioSource != null)
                uiAudioSource.volume = sfxVolume;
        }

        private static AudioClip CreateToneClip(string name, float duration, float startHz, float endHz, float volume)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                float hz = Mathf.Lerp(startHz, endHz, t);
                float envelope = Mathf.Sin(Mathf.PI * t) * (1f - t * 0.35f);
                data[i] = Mathf.Sin(2f * Mathf.PI * hz * i / sampleRate) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateHintClip()
        {
            const int sampleRate = 44100;
            float duration = 0.13f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float hz = t < 0.055f ? 760f : 560f;
                float local = t < 0.055f ? t / 0.055f : (t - 0.055f) / 0.075f;
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(local)) * Mathf.Exp(-t * 8f);
                data[i] = Mathf.Sin(2f * Mathf.PI * hz * t) * envelope * 0.12f;
            }

            AudioClip clip = AudioClip.Create("Huapi_Hint_Jike", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void HeartBar(RectTransform parent, Vector2 pos)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 heartPos = pos + new Vector2(i * 34, 0);
                Panel("HeartBack", Clear, heartPos, new Vector2(30, 30), parent);
                Text("♥", 26, i < hearts ? Red : new Color(0.45f, 0.38f, 0.34f, 0.72f), TextAlignmentOptions.Center, heartPos + new Vector2(0, -1), new Vector2(34, 34), parent);
            }
        }

        private Image ImagePanel(string name, string assetPath, Color fallback, Vector2 pos, Vector2 size, Transform parent)
        {
            Image image = Panel(name, fallback, pos, size, parent);
            Sprite sprite = LoadSprite(assetPath);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
                image.color = Color.white;
            }
            return image;
        }

        private Image FullscreenImage(string name, string assetPath, Color fallback, Transform parent)
        {
            Image image = FullscreenPanel(name, fallback, parent);
            Sprite sprite = LoadSprite(assetPath);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
                image.color = Color.white;
                AspectRatioFitter fitter = image.gameObject.GetComponent<AspectRatioFitter>() ?? image.gameObject.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
                image.rectTransform.localScale = Vector3.one * 1.03f;
            }
            return image;
        }

        private Image FullscreenDesignImage(string name, string assetPath, Color fallback, Transform parent)
        {
            Image image = FullscreenPanel(name, fallback, parent);
            Sprite sprite = LoadSprite(assetPath);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
                image.color = Color.white;
            }
            return image;
        }

        private Image SceneBackgroundImage(string name, string assetPath, Color fallback, Transform parent)
        {
            FullscreenPanel(name + "_Matte", fallback, parent);
            Image image = FullscreenPanel(name, new Color(0, 0, 0, 0), parent);
            Sprite sprite = LoadSprite(assetPath);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
                image.color = Color.white;
                AspectRatioFitter fitter = image.gameObject.GetComponent<AspectRatioFitter>() ?? image.gameObject.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
                image.rectTransform.localScale = Vector3.one * 0.94f;
            }
            return image;
        }

        private Image FullscreenPanel(string name, Color color, Transform parent)
        {
            GameObject go = Track(new GameObject(name, typeof(RectTransform), typeof(Image)));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            Stretch(rect);
            Image img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        private Image Panel(string name, Color color, Vector2 pos, Vector2 size, Transform parent)
        {
            GameObject go = Track(new GameObject(name, typeof(RectTransform), typeof(Image)));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            Image img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        private Button Button(string label, Vector2 pos, Vector2 size, Action onClick)
        {
            return Button(label, pos, size, onClick, Dark, White);
        }

        private Button Button(string label, Vector2 pos, Vector2 size, Action onClick, Color bg, Color fg)
        {
            GameObject go = Track(new GameObject("Button_" + label, typeof(RectTransform), typeof(Image), typeof(Button)));
            go.transform.SetParent(currentLayer, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = ClampToSafeArea(pos, size);
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = bg;
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                PlayClickSound();
                onClick?.Invoke();
            });
            Text(label, Mathf.Min(20f, size.y * 0.36f), fg, TextAlignmentOptions.Center, Vector2.zero, size + new Vector2(0, 12), rect);
            return button;
        }

        private Button TransparentButton(string name, Vector2 pos, Vector2 size, Action onClick)
        {
            GameObject go = Track(new GameObject("TransparentButton_" + name, typeof(RectTransform), typeof(Image), typeof(Button)));
            go.transform.SetParent(currentLayer, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = ClampToSafeArea(pos, size);
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                PlayClickSound();
                onClick?.Invoke();
            });
            return button;
        }

        private Slider SettingsSlider(string name, Vector2 pos, float value, Action<float> onChanged)
        {
            GameObject go = Track(new GameObject(name, typeof(RectTransform), typeof(Slider)));
            go.transform.SetParent(currentLayer, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = ClampToSafeArea(pos, new Vector2(500, 56));
            rect.sizeDelta = new Vector2(500, 56);

            Image track = SettingsSliderImage("Track", go.transform, new Color(White.r, White.g, White.b, 0.42f));
            RectTransform trackRect = track.rectTransform;
            trackRect.anchorMin = trackRect.anchorMax = new Vector2(0.5f, 0.5f);
            trackRect.anchoredPosition = Vector2.zero;
            trackRect.sizeDelta = new Vector2(450, 5);

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = fillAreaRect.anchorMax = new Vector2(0.5f, 0.5f);
            fillAreaRect.anchoredPosition = Vector2.zero;
            fillAreaRect.sizeDelta = new Vector2(450, 7);

            Image fill = SettingsSliderImage("Fill", fillArea.transform, new Color(Gold.r, Gold.g, Gold.b, 0.92f));
            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = new Vector2(0f, 0.5f);
            fillRect.anchorMax = new Vector2(1f, 0.5f);
            fillRect.pivot = new Vector2(0.5f, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = new Vector2(0, 7);

            Image handle = SettingsSliderImage("Handle", go.transform, Clear);
            RectTransform handleRect = handle.rectTransform;
            handleRect.anchorMin = handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.anchoredPosition = Vector2.zero;
            handleRect.sizeDelta = new Vector2(44, 44);

            Slider slider = go.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.direction = Slider.Direction.LeftToRight;
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            slider.value = Mathf.Clamp01(value);
            slider.onValueChanged.AddListener(v => onChanged?.Invoke(v));
            return slider;
        }

        private Image SettingsSliderImage(string name, Transform parent, Color color)
        {
            GameObject go = Track(new GameObject(name, typeof(RectTransform), typeof(Image)));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private Button ImageButton(string name, string spritePath, Vector2 pos, Vector2 size, Action onClick)
        {
            GameObject go = Track(new GameObject("ImageButton_" + name, typeof(RectTransform), typeof(Image), typeof(Button)));
            go.transform.SetParent(currentLayer, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = ClampToSafeArea(pos, size);
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            Sprite sprite = LoadSprite(spritePath);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
                image.color = Color.white;
            }
            else
            {
                image.color = Dark;
            }
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                PlayClickSound();
                onClick?.Invoke();
            });
            return button;
        }

        private static void AddHoverEvent(EventTrigger trigger, EventTriggerType type, Action action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(_ => action?.Invoke());
            trigger.triggers.Add(entry);
        }

        private static void AddDragEvent(EventTrigger trigger, EventTriggerType type, Action<PointerEventData> action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(data => action?.Invoke((PointerEventData)data));
            trigger.triggers.Add(entry);
        }

        private static void ConfigureDialogueText(UnityEngine.UI.Text text, int minimumFontSize)
        {
            text.alignment = TextAnchor.UpperLeft;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Min(minimumFontSize, text.fontSize);
            text.resizeTextMaxSize = text.fontSize;
            text.lineSpacing = 1.15f;
        }

        private static void ConfigureStoryBodyText(UnityEngine.UI.Text text)
        {
            text.alignment = TextAnchor.UpperCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Min(StoryBodyMinimumFontSize, text.fontSize);
            text.resizeTextMaxSize = StoryBodyFontSize;
            text.lineSpacing = 1.25f;
        }

        private UnityEngine.UI.Text Text(string content, float size, Color color, TextAlignmentOptions alignment, Vector2 pos, Vector2 rectSize, Transform parent)
        {
            if (!string.IsNullOrEmpty(content) && (content.Contains("涓ゆ棩") || content.Contains("试玩")))
                content = "薛氏剧团 · 本格推理";

            GameObject go = Track(new GameObject("Text", typeof(RectTransform), typeof(UnityEngine.UI.Text)));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            Vector2 safeTextSize = ExpandTextRect(rectSize, size);
            rect.anchoredPosition = ClampToSafeArea(pos, safeTextSize);
            rect.sizeDelta = safeTextSize;
            UnityEngine.UI.Text text = go.GetComponent<UnityEngine.UI.Text>();
            text.text = content;
            text.font = GetRuntimeFont();
            int resolvedFontSize = ResolveRuntimeFontSize(content, size, rectSize);
            text.fontSize = resolvedFontSize;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Clamp(Mathf.RoundToInt(resolvedFontSize * 0.50f), 12, resolvedFontSize);
            text.resizeTextMaxSize = resolvedFontSize;
            text.color = color;
            text.alignment = ToCenteredLegacyAlignment(alignment);
            text.lineSpacing = 1.05f;
            text.alignByGeometry = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private GameObject Track(GameObject go)
        {
            spawned.Add(go);
            return go;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject go = new GameObject("EventSystem", typeof(EventSystem));
                eventSystem = go.GetComponent<EventSystem>();
            }

            foreach (StandaloneInputModule oldModule in eventSystem.GetComponents<StandaloneInputModule>())
                Destroy(oldModule);

            InputSystemUIInputModule module = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (module == null)
                module = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            if (module.actionsAsset == null)
                module.AssignDefaultActions();
        }

        private static void ConfigureFullscreenTestMode()
        {
#if !UNITY_EDITOR
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;
            if (Display.main != null)
                Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
#endif
        }

        private static void HideLegacySceneBackground()
        {
            GameObject legacyBackground = GameObject.Find("Background");
            if (legacyBackground != null)
                legacyBackground.SetActive(false);
        }

        private static Color Hex(int rgb, float alpha = 1f)
        {
            return new Color(((rgb >> 16) & 0xff) / 255f, ((rgb >> 8) & 0xff) / 255f, (rgb & 0xff) / 255f, alpha);
        }

        private static Vector2 ClampToSafeArea(Vector2 pos, Vector2 size)
        {
            const float halfWidth = 960f;
            const float halfHeight = 540f;
            const float margin = 26f;
            float halfRectWidth = Mathf.Max(0f, size.x * 0.5f);
            float halfRectHeight = Mathf.Max(0f, size.y * 0.5f);
            float minX = -halfWidth + margin + halfRectWidth;
            float maxX = halfWidth - margin - halfRectWidth;
            float minY = -halfHeight + margin + halfRectHeight;
            float maxY = halfHeight - margin - halfRectHeight;

            pos.x = minX > maxX ? 0f : Mathf.Clamp(pos.x, minX, maxX);
            pos.y = minY > maxY ? 0f : Mathf.Clamp(pos.y, minY, maxY);
            return pos;
        }

        private static Vector2 ExpandTextRect(Vector2 size, float fontSize)
        {
            float extraHeight = Mathf.Clamp(fontSize * 0.45f, 8f, 30f);
            return new Vector2(size.x, size.y + extraHeight);
        }

        private static Font GetRuntimeFont()
        {
            if (runtimeFont != null) return runtimeFont;
            runtimeFont = Resources.Load<Font>("Fonts/simkai");
            if (runtimeFont == null)
                runtimeFont = Font.CreateDynamicFontFromOSFont(new[] { "KaiTi", "SimKai", "楷体", "华文楷体" }, 32);
            if (runtimeFont == null) runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (runtimeFont == null) runtimeFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return runtimeFont;
        }

        private static TMP_FontAsset GetRuntimeTmpFont()
        {
            if (runtimeTmpFont != null) return runtimeTmpFont;

            Font sourceFont = GetRuntimeFont();
            if (sourceFont == null) return null;

            runtimeTmpFont = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                90,
                9,
                GlyphRenderMode.SDFAA,
                4096,
                4096,
                AtlasPopulationMode.Dynamic,
                true);

            runtimeTmpFont.name = "Runtime KaiTi TMP";
            return runtimeTmpFont;
        }

        private static void ApplyRuntimeFontToAllTmpTexts()
        {
            TMP_FontAsset tmpFont = GetRuntimeTmpFont();
            if (tmpFont == null) return;

            TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                if (text == null) continue;
                if (text.font != tmpFont)
                    text.font = tmpFont;
                text.fontSharedMaterial = tmpFont.material;
                tmpFont.TryAddCharacters(text.text, out _);
                text.havePropertiesChanged = true;
                text.SetAllDirty();
            }
        }

        private static int ResolveRuntimeFontSize(string content, float requestedSize, Vector2 rectSize)
        {
            int requested = Mathf.RoundToInt(requestedSize);
            if (requested >= 36) return requested;
            if (requested < 17) return requested;
            if (string.IsNullOrWhiteSpace(content)) return requested;
            if (requested <= 24 && rectSize.x <= 160f && rectSize.y <= 110f) return requested;

            bool roomyTextBlock = rectSize.y >= 58f || content.Contains("\n");
            bool ordinaryTextSize = requested >= 20 && requested <= 30;
            if (!roomyTextBlock && !ordinaryTextSize) return requested;

            if (ContainsCjk(content)) return DefaultChineseFontSize;
            if (IsMostlyLatin(content)) return DefaultEnglishFontSize;
            return requested;
        }

        private static bool ContainsCjk(string content)
        {
            foreach (char c in content)
            {
                if ((c >= '\u3400' && c <= '\u4dbf') ||
                    (c >= '\u4e00' && c <= '\u9fff') ||
                    (c >= '\uf900' && c <= '\ufaff'))
                    return true;
            }

            return false;
        }

        private static bool IsMostlyLatin(string content)
        {
            int latin = 0;
            int letters = 0;
            foreach (char c in content)
            {
                if (!char.IsLetter(c)) continue;
                letters++;
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) latin++;
            }

            return letters > 0 && latin >= Mathf.CeilToInt(letters * 0.65f);
        }

        private static Sprite LoadSprite(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return null;
#if UNITY_EDITOR
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (texture != null)
            {
                bool spriteUsesWholeTexture = sprite != null
                    && Mathf.Abs(sprite.rect.width - texture.width) < 1f
                    && Mathf.Abs(sprite.rect.height - texture.height) < 1f;
                if (!spriteUsesWholeTexture)
                    return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                return sprite;
            }

            if (sprite != null) return sprite;

            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (asset is Sprite childSprite)
                    return childSprite;
            }

            string absolutePath = Path.GetFullPath(assetPath);
            if (File.Exists(absolutePath))
            {
                byte[] bytes = File.ReadAllBytes(absolutePath);
                Texture2D diskTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (diskTexture.LoadImage(bytes))
                    return Sprite.Create(diskTexture, new Rect(0f, 0f, diskTexture.width, diskTexture.height), new Vector2(0.5f, 0.5f), 100f);
            }
#else
            return null;
#endif
            return null;
        }

        private static bool RuntimeImageAssetsAvailable
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        private static TextAnchor ToCenteredLegacyAlignment(TextAlignmentOptions alignment)
        {
            if ((alignment & TextAlignmentOptions.Top) == TextAlignmentOptions.Top)
            {
                return TextAnchor.UpperCenter;
            }

            if ((alignment & TextAlignmentOptions.Bottom) == TextAlignmentOptions.Bottom)
                return TextAnchor.LowerCenter;

            return TextAnchor.MiddleCenter;
        }
    }
}
