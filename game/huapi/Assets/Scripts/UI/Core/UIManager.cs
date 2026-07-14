using System;
using System.Collections.Generic;
using UnityEngine;

namespace TXGame
{
    /// <summary>
    /// UI 面板类型枚举
    /// </summary>
    public enum UIPanelType
    {
        MainMenu,
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
        [SerializeField] private PanelBase dialoguePanelPrefab;
        [SerializeField] private PanelBase cluePanelPrefab;
        [SerializeField] private PanelBase characterArchivePanelPrefab;
        [SerializeField] private PanelBase observeSkinPanelPrefab;
        [SerializeField] private PanelBase pausePanelPrefab;

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
            DontDestroyOnLoad(gameObject);

            RegisterPrefabs();
        }

        private void Update()
        {
            if (HuapiFullUIInstaller.IsInstalled)
            {
                return;
            }

            if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                OnEscapePressed();
            }
        }

        private void RegisterPrefabs()
        {
            if (mainMenuPanelPrefab != null) _panelPrefabs[UIPanelType.MainMenu] = mainMenuPanelPrefab;
            if (dialoguePanelPrefab != null) _panelPrefabs[UIPanelType.Dialogue] = dialoguePanelPrefab;
            if (cluePanelPrefab != null) _panelPrefabs[UIPanelType.ClueInventory] = cluePanelPrefab;
            if (characterArchivePanelPrefab != null) _panelPrefabs[UIPanelType.CharacterArchive] = characterArchivePanelPrefab;
            if (observeSkinPanelPrefab != null) _panelPrefabs[UIPanelType.ObserveSkin] = observeSkinPanelPrefab;
            if (pausePanelPrefab != null) _panelPrefabs[UIPanelType.Pause] = pausePanelPrefab;
        }

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
            PanelBase panel = Instantiate(_panelPrefabs[type], targetCanvas.transform);
            panel.Init(data);
            panel.Show();

            _activePanels[type] = panel;
            _uiStack.Push(type);

            OnPanelOpened(type);
            return panel;
        }

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

        public void ClosePanel(UIPanelType type)
        {
            if (!_activePanels.ContainsKey(type)) return;

            PanelBase panel = _activePanels[type];
            panel.Hide();
            _activePanels.Remove(type);
            _uiStack.Remove(type);

            OnPanelClosed(type);
        }

        public void CloseAllPanels()
        {
            foreach (var kvp in _activePanels)
            {
                kvp.Value.Hide();
            }
            _activePanels.Clear();
            _uiStack.Clear();
        }

        public PanelBase GetPanel(UIPanelType type)
        {
            return _activePanels.ContainsKey(type) ? _activePanels[type] : null;
        }

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
            if (type != UIPanelType.MainMenu && type != UIPanelType.Pause)
            {
                DisableGameplayInput();
            }
        }

        private void OnPanelClosed(UIPanelType type)
        {
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
            Debug.Log("[UIManager] Gameplay input disabled");
        }

        private void EnableGameplayInput()
        {
            Debug.Log("[UIManager] Gameplay input enabled");
        }
    }
}
