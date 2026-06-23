using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HuaPi.UI.Core
{
    /// <summary>
    /// UI 面板类型枚举
    /// </summary>
    public enum UIPanelType
    {
        MainMenu,
        Exploration,
        Dialogue,
        ClueInventory,
        CharacterArchive,
        ObserveSkin,
        Pause,
        ConfirmPopup,
        ClueNotification
    }

    /// <summary>
    /// UIManager 单例：统一打开、关闭、切换和返回 UI。
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Canvas Layers")]
        [SerializeField] private Canvas backgroundCanvas;
        [SerializeField] private Canvas worldHUDCanvas;
        [SerializeField] private Canvas dialogueCanvas;
        [SerializeField] private Canvas normalPanelCanvas;
        [SerializeField] private Canvas popupCanvas;
        [SerializeField] private Canvas revealCanvas;
        [SerializeField] private Canvas systemCanvas;

        [Header("Panel Prefabs")]
        [SerializeField] private PanelBase mainMenuPanelPrefab;
        [SerializeField] private PanelBase explorationPanelPrefab;
        [SerializeField] private PanelBase dialoguePanelPrefab;
        [SerializeField] private PanelBase cluePanelPrefab;
        [SerializeField] private PanelBase characterArchivePanelPrefab;
        [SerializeField] private PanelBase observeSkinPanelPrefab;
        [SerializeField] private PanelBase pausePanelPrefab;

        [Header("Startup")]
        [SerializeField] private bool openMainMenuOnStart = true;

        private readonly UIStack _uiStack = new UIStack();
        private readonly Dictionary<UIPanelType, PanelBase> _panelPrefabs = new Dictionary<UIPanelType, PanelBase>();
        private readonly Dictionary<UIPanelType, PanelBase> _activePanels = new Dictionary<UIPanelType, PanelBase>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            RegisterPrefabs();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            if (openMainMenuOnStart && _panelPrefabs.ContainsKey(UIPanelType.MainMenu))
            {
                OpenPanel(UIPanelType.MainMenu);
            }
        }

        private void Update()
        {
            if (IsEscapePressed())
            {
                OnEscapePressed();
            }
        }

        private static bool IsEscapePressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return Keyboard.current.escapeKey.wasPressedThisFrame;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }

        private void RegisterPrefabs()
        {
            if (mainMenuPanelPrefab != null) _panelPrefabs[UIPanelType.MainMenu] = mainMenuPanelPrefab;
            if (explorationPanelPrefab != null) _panelPrefabs[UIPanelType.Exploration] = explorationPanelPrefab;
            if (dialoguePanelPrefab != null) _panelPrefabs[UIPanelType.Dialogue] = dialoguePanelPrefab;
            if (cluePanelPrefab != null) _panelPrefabs[UIPanelType.ClueInventory] = cluePanelPrefab;
            if (characterArchivePanelPrefab != null) _panelPrefabs[UIPanelType.CharacterArchive] = characterArchivePanelPrefab;
            if (observeSkinPanelPrefab != null) _panelPrefabs[UIPanelType.ObserveSkin] = observeSkinPanelPrefab;
            if (pausePanelPrefab != null) _panelPrefabs[UIPanelType.Pause] = pausePanelPrefab;
        }

        /// <summary>
        /// 打开指定面板
        /// </summary>
        public PanelBase OpenPanel(UIPanelType type, object data = null)
        {
            if (_activePanels.ContainsKey(type))
            {
                _activePanels[type].Refresh(data);
                return _activePanels[type];
            }

            if (!_panelPrefabs.ContainsKey(type))
            {
                Debug.LogWarning($"[UIManager] No prefab registered for {type}");
                return null;
            }

            Canvas targetCanvas = GetTargetCanvas(type);
            if (targetCanvas == null)
            {
                Debug.LogError($"[UIManager] Target canvas is null for panel type {type}. Please run Tools/HuaPi/Generate UI first.");
                return null;
            }
            PanelBase panel = Instantiate(_panelPrefabs[type], targetCanvas.transform);
            panel.Init(data);
            panel.Show();

            _activePanels[type] = panel;
            _uiStack.Push(type);

            OnPanelOpened(type);
            return panel;
        }

        /// <summary>
        /// 关闭栈顶面板
        /// </summary>
        public void ClosePanel()
        {
            if (_uiStack.Count == 0) return;

            UIPanelType topType = _uiStack.Pop();
            if (_activePanels.ContainsKey(topType))
            {
                PanelBase panel = _activePanels[topType];
                panel.Hide();
                _activePanels.Remove(topType);

                OnPanelClosed(topType);
            }
        }

        /// <summary>
        /// 关闭指定面板
        /// </summary>
        public void ClosePanel(UIPanelType type)
        {
            if (!_activePanels.ContainsKey(type)) return;

            PanelBase panel = _activePanels[type];
            panel.Hide();
            _activePanels.Remove(type);
            _uiStack.Remove(type);

            OnPanelClosed(type);
        }

        /// <summary>
        /// 关闭所有面板
        /// </summary>
        public void CloseAllPanels()
        {
            foreach (var kvp in _activePanels)
            {
                kvp.Value.Hide();
            }
            _activePanels.Clear();
            _uiStack.Clear();
        }

        /// <summary>
        /// 获取指定面板（如果已打开）
        /// </summary>
        public PanelBase GetPanel(UIPanelType type)
        {
            return _activePanels.ContainsKey(type) ? _activePanels[type] : null;
        }

        /// <summary>
        /// 检查面板是否已打开
        /// </summary>
        public bool IsPanelOpen(UIPanelType type)
        {
            return _activePanels.ContainsKey(type);
        }

        private Canvas GetTargetCanvas(UIPanelType type)
        {
            switch (type)
            {
                case UIPanelType.MainMenu:
                    return systemCanvas;
                case UIPanelType.Exploration:
                    return worldHUDCanvas;
                case UIPanelType.Dialogue:
                    return dialogueCanvas;
                case UIPanelType.ClueInventory:
                case UIPanelType.CharacterArchive:
                    return normalPanelCanvas;
                case UIPanelType.ObserveSkin:
                    return normalPanelCanvas;
                case UIPanelType.Pause:
                    return systemCanvas;
                case UIPanelType.ConfirmPopup:
                case UIPanelType.ClueNotification:
                    return popupCanvas;
                default:
                    return normalPanelCanvas;
            }
        }

        private void OnPanelOpened(UIPanelType type)
        {
            // 暂停场景输入（当非 HUD/Exploration 面板打开时）
            if (type != UIPanelType.Exploration &&
                type != UIPanelType.MainMenu && 
                type != UIPanelType.Pause)
            {
                DisableGameplayInput();
            }
        }

        private void OnPanelClosed(UIPanelType type)
        {
            // 如果栈空，恢复场景输入
            if (_uiStack.Count == 0)
            {
                EnableGameplayInput();
            }
        }

        private void OnEscapePressed()
        {
            if (_uiStack.Count > 0)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel(UIPanelType.Pause);
            }
        }

        private void DisableGameplayInput()
        {
            // TODO: 调用 InputManager 禁用场景输入
            Debug.Log("[UIManager] Gameplay input disabled");
        }

        private void EnableGameplayInput()
        {
            // TODO: 调用 InputManager 启用场景输入
            Debug.Log("[UIManager] Gameplay input enabled");
        }
    }
}
