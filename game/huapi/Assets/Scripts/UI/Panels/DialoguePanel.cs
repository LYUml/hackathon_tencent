using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TXGame
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

        private HuapiDialogueData _currentDialogue;
        private int _currentLineIndex = 0;
        private bool _isTyping = false;
        private Coroutine _typewriterCoroutine;

        protected override void Awake()
        {
            base.Awake();

            if (characterPortrait == null) characterPortrait = transform.Find("Character/Portrait")?.GetComponent<Image>();
            if (characterNameText == null) characterNameText = transform.Find("Character/Name")?.GetComponent<TMP_Text>();
            if (dialogueText == null) dialogueText = transform.Find("Dialogue/Text")?.GetComponent<TMP_Text>();
            if (dialogueBg == null) dialogueBg = transform.Find("Dialogue/Bg")?.GetComponent<Image>();
            if (optionsContainer == null) optionsContainer = transform.Find("Options");

            if (dialogueBg != null)
                dialogueBg.color = new Color(0.04f, 0.03f, 0.03f, 0.75f);
            if (dialogueText != null)
                dialogueText.color = new Color(0.847f, 0.812f, 0.753f);
            if (characterNameText != null)
                characterNameText.color = new Color(0.788f, 0.541f, 0.431f);
        }

        public override void Init(object data)
        {
            base.Init(data);
            if (data is HuapiDialogueData dialogue)
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

            HuapiDialogueLine line = _currentDialogue.lines[_currentLineIndex];

            if (characterNameText != null)
                characterNameText.text = line.speakerId;

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

            foreach (Transform child in optionsContainer)
                Destroy(child.gameObject);

            foreach (HuapiDialogueOption option in _currentDialogue.options)
                CreateOptionButton(option);
        }

        private void CreateOptionButton(HuapiDialogueOption option)
        {
            if (optionPrefab == null || optionsContainer == null) return;

            GameObject btn = Instantiate(optionPrefab, optionsContainer);
            TMP_Text text = btn.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = option.optionText;
                text.color = new Color(0.788f, 0.541f, 0.431f);
            }

            Button button = btn.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => OnOptionSelected(option));
        }

        private void OnOptionSelected(HuapiDialogueOption option)
        {
            Debug.Log($"[Dialogue] Selected: {option.optionText}");
            CloseDialogue();
        }

        private void CloseDialogue()
        {
            UIManager.Instance.ClosePanel(UIPanelType.Dialogue);
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Mouse.current?.leftButton.wasPressedThisFrame == true || UnityEngine.InputSystem.Keyboard.current?.spaceKey.wasPressedThisFrame == true)
            {
                if (_isTyping)
                {
                    if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
                    if (_currentDialogue != null && _currentLineIndex < _currentDialogue.lines.Length)
                        dialogueText.text = _currentDialogue.lines[_currentLineIndex].text;
                    _isTyping = false;
                }
                else
                {
                    _currentLineIndex++;
                    ShowCurrentLine();
                }
            }
        }
    }
}
