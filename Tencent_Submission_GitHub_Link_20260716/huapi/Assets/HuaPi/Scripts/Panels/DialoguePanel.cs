using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HuaPi.UI.Core;
using HuaPi.UI.Data;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HuaPi.UI.Panels
{
    /// <summary>
    /// 对话面板：无边界，像戏台字幕从暗场中浮现
    /// </summary>
    public class DialoguePanel : PanelBase
    {
        [Header("Character")]
        [SerializeField] private Image characterPortrait;
        [SerializeField] private TMP_Text characterNameText;

        [Header("Dialogue")]
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Image dialogueBg;

        [Header("Options")]
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private GameObject optionPrefab;

        [Header("Settings")]
        [SerializeField] private float typewriterSpeed = 0.05f;

        private DialogueData _currentDialogue;
        private int _currentLineIndex = 0;
        private bool _isTyping = false;
        private Coroutine _typewriterCoroutine;

        protected override void Awake()
        {
            base.Awake();

            // 自动查找子对象
            if (characterPortrait == null) characterPortrait = transform.Find("Character/Portrait")?.GetComponent<Image>();
            if (characterNameText == null) characterNameText = transform.Find("Character/Name")?.GetComponent<TMP_Text>();
            if (dialogueText == null) dialogueText = transform.Find("Dialogue/Text")?.GetComponent<TMP_Text>();
            if (dialogueBg == null) dialogueBg = transform.Find("Dialogue/Bg")?.GetComponent<Image>();
            if (optionsContainer == null) optionsContainer = transform.Find("Options");

            // 设置暗场渐变背景
            if (dialogueBg != null)
            {
                dialogueBg.color = new Color(0.04f, 0.03f, 0.03f, 0.75f); // #0a0808, opacity 0.75
            }

            // 设置文字颜色
            if (dialogueText != null)
            {
                dialogueText.color = new Color(0.847f, 0.812f, 0.753f); // #d8cfc0
            }
            if (characterNameText != null)
            {
                characterNameText.color = new Color(0.788f, 0.541f, 0.431f); // #c9a96e
            }
        }

        public override void Init(object data)
        {
            base.Init(data);
            if (data is DialogueData dialogue)
            {
                _currentDialogue = dialogue;
                _currentLineIndex = 0;
                ShowCurrentLine();
            }
        }

        private void ShowCurrentLine()
        {
            if (_currentDialogue == null || _currentLineIndex >= _currentDialogue.lines.Length)
            {
                ShowOptions();
                return;
            }

            DialogueLine line = _currentDialogue.lines[_currentLineIndex];

            if (characterNameText != null)
            {
                characterNameText.text = line.speakerId;
            }

            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = StartCoroutine(TypewriterEffect(line.text));
        }

        private System.Collections.IEnumerator TypewriterEffect(string fullText)
        {
            _isTyping = true;
            if (dialogueText != null)
            {
                dialogueText.text = "";
                for (int i = 0; i < fullText.Length; i++)
                {
                    dialogueText.text += fullText[i];
                    yield return new WaitForSeconds(typewriterSpeed);
                }
            }
            _isTyping = false;
        }

        private void ShowOptions()
        {
            if (_currentDialogue == null || _currentDialogue.options == null || _currentDialogue.options.Length == 0)
            {
                CloseDialogue();
                return;
            }

            // 清除旧选项
            foreach (Transform child in optionsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (DialogueOption option in _currentDialogue.options)
            {
                CreateOptionButton(option);
            }
        }

        private void CreateOptionButton(DialogueOption option)
        {
            if (optionPrefab == null || optionsContainer == null) return;

            GameObject btn = Instantiate(optionPrefab, optionsContainer);
            TMP_Text text = btn.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = option.optionText;
                text.color = new Color(0.788f, 0.541f, 0.431f); // #c9a96e
            }

            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnOptionSelected(option));
            }
        }

        private void OnOptionSelected(DialogueOption option)
        {
            Debug.Log($"[Dialogue] Selected: {option.optionText}");
            // TODO: 处理选项逻辑，进入下一个节点
            CloseDialogue();
        }

        private void CloseDialogue()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ClosePanel(UIPanelType.Dialogue);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (IsAdvancePressed())
            {
                if (_isTyping)
                {
                    // 跳过打字机效果，显示完整文本
                    if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
                    if (_currentDialogue != null && _currentLineIndex < _currentDialogue.lines.Length)
                    {
                        dialogueText.text = _currentDialogue.lines[_currentLineIndex].text;
                    }
                    _isTyping = false;
                }
                else
                {
                    // 进入下一行
                    _currentLineIndex++;
                    ShowCurrentLine();
                }
            }
        }

        private static bool IsAdvancePressed()
        {
#if ENABLE_INPUT_SYSTEM
            bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
            if (mousePressed || spacePressed)
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
#else
            return false;
#endif
        }
    }
}
