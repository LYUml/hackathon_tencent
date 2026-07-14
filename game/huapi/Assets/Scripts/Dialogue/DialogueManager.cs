using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace TXGame
{
    /// <summary>
    /// 对话管理器 - 处理对话显示、分支选择、条件判断
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("对话UI")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image speakerPortrait;
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private float textRevealSpeed = 0.03f;  // 逐字显示速度

        [Header("输入")]
        [SerializeField] private Key advanceKey = Key.Space;
        [SerializeField] private Key skipKey = Key.Escape;

        // 内部状态
        private DialogueData currentDialogue;
        private int currentLineIndex;
        private bool isDialogueActive;
        private bool isTyping;
        private Coroutine typingCoroutine;
        private HashSet<string> completedDialogues = new HashSet<string>();
        private HashSet<string> gameFlags = new HashSet<string>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            dialoguePanel?.SetActive(false);
        }

        private void Update()
        {
            if (!isDialogueActive) return;

            if (Keyboard.current != null && Keyboard.current[advanceKey].wasPressedThisFrame)
            {
                if (isTyping)
                {
                    // 跳过打字效果，直接显示完整文本
                    CompleteTyping();
                }
                else
                {
                    AdvanceDialogue();
                }
            }

            if (Keyboard.current != null && Keyboard.current[skipKey].wasPressedThisFrame)
            {
                EndDialogue();
            }
        }

        /// <summary>
        /// 开始一段对话
        /// </summary>
        public void StartDialogue(DialogueData dialogue)
        {
            if (dialogue == null) return;
            if (!CheckConditions(dialogue.conditions)) return;

            currentDialogue = dialogue;
            currentLineIndex = 0;
            isDialogueActive = true;
            dialoguePanel?.SetActive(true);

            // 隐藏选择
            if (choicesContainer != null)
                choicesContainer.gameObject.SetActive(false);

            DisplayCurrentLine();
        }

        /// <summary>
        /// 检查对话触发条件
        /// </summary>
        private bool CheckConditions(List<DialogueCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0) return true;

            InvestigationManager investigation = InvestigationManager.Instance;
            HuapiSystem huapi = FindObjectOfType<HuapiSystem>();
            DayNightCycle dayNight = DayNightCycle.Instance;

            foreach (var condition in conditions)
            {
                switch (condition.type)
                {
                    case DialogueCondition.ConditionType.HasClue:
                        if (investigation == null || !investigation.HasClue(condition.parameter))
                            return false;
                        break;

                    case DialogueCondition.ConditionType.RevealProgress:
                        if (huapi != null && huapi.GetRevealProgress(condition.parameter) < condition.threshold)
                            return false;
                        break;

                    case DialogueCondition.ConditionType.TimeOfDay:
                        if (dayNight == null) return false;
                        int expectedPhase = Mathf.RoundToInt(condition.threshold);
                        if ((int)dayNight.CurrentPhase != expectedPhase) return false;
                        break;

                    case DialogueCondition.ConditionType.DayGreaterThan:
                        if (dayNight == null || dayNight.CurrentDay < condition.threshold)
                            return false;
                        break;

                    case DialogueCondition.ConditionType.DialogueCompleted:
                        if (!completedDialogues.Contains(condition.parameter))
                            return false;
                        break;

                    case DialogueCondition.ConditionType.HasItem:
                        // 检查线索系统中的物品
                        if (investigation == null || !investigation.HasClue(condition.parameter))
                            return false;
                        break;

                    case DialogueCondition.ConditionType.SetFlag:
                        if (!gameFlags.Contains(condition.parameter))
                            return false;
                        break;

                    case DialogueCondition.ConditionType.Always:
                        break;
                }
            }

            return true;
        }

        private void DisplayCurrentLine()
        {
            if (currentDialogue == null || currentLineIndex >= currentDialogue.lines.Count)
            {
                // 对话结束，处理选择和结果
                if (currentDialogue.hasChoices)
                {
                    ShowChoices();
                }
                else
                {
                    ProcessResults(currentDialogue.results);
                    EndDialogue();
                }
                return;
            }

            DialogueLine line = currentDialogue.lines[currentLineIndex];

            if (speakerNameText != null)
                speakerNameText.text = line.speakerName;

            if (speakerPortrait != null)
            {
                if (line.speakerPortrait != null)
                {
                    speakerPortrait.sprite = line.speakerPortrait;
                    speakerPortrait.enabled = true;
                }
                else
                {
                    speakerPortrait.enabled = false;
                }
            }

            // 逐字显示
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(line.text));

            // 自动推进
            if (line.autoAdvanceDelay > 0)
            {
                StartCoroutine(AutoAdvance(line.autoAdvanceDelay));
            }
        }

        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            dialogueText.text = "";

            foreach (char c in text)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(textRevealSpeed);
            }

            isTyping = false;
        }

        private void CompleteTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            if (currentDialogue != null && currentLineIndex < currentDialogue.lines.Count)
            {
                dialogueText.text = currentDialogue.lines[currentLineIndex].text;
            }

            isTyping = false;
        }

        private void AdvanceDialogue()
        {
            currentLineIndex++;
            DisplayCurrentLine();
        }

        private IEnumerator AutoAdvance(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (isDialogueActive)
            {
                AdvanceDialogue();
            }
        }

        private void ShowChoices()
        {
            if (choicesContainer == null || choiceButtonPrefab == null) return;

            choicesContainer.gameObject.SetActive(true);

            // 清除旧选项
            foreach (Transform child in choicesContainer)
                Destroy(child.gameObject);

            foreach (var choice in currentDialogue.choices)
            {
                GameObject btnObj = Instantiate(choiceButtonPrefab, choicesContainer);
                Button btn = btnObj.GetComponent<Button>();
                TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();

                if (label != null) label.text = choice.choiceText;

                // 捕获变量
                var capturedChoice = choice;
                if (btn != null)
                {
                    btn.onClick.AddListener(() =>
                    {
                        choicesContainer.gameObject.SetActive(false);
                        ProcessResults(capturedChoice.choiceResults);

                        // 跳转到下一段对话
                        if (!string.IsNullOrEmpty(capturedChoice.nextDialogueID))
                        {
                            DialogueData nextDialogue = Resources.Load<DialogueData>($"Dialogues/{capturedChoice.nextDialogueID}");
                            if (nextDialogue != null)
                                StartDialogue(nextDialogue);
                            else
                                EndDialogue();
                        }
                        else
                        {
                            EndDialogue();
                        }
                    });
                }
            }
        }

        private void ProcessResults(List<DialogueResult> results)
        {
            if (results == null) return;

            foreach (var result in results)
            {
                switch (result.type)
                {
                    case DialogueResult.ResultType.UnlockClue:
                        InvestigationManager.Instance?.AddClue(result.parameter);
                        break;

                    case DialogueResult.ResultType.RevealCharacter:
                        // 格式: "characterID,centerX,centerY,radius"
                        string[] parts = result.parameter.Split(',');
                        if (parts.Length >= 4)
                        {
                            HuapiSystem huapi = FindObjectOfType<HuapiSystem>();
                            huapi?.RevealArea(
                                parts[0],
                                float.Parse(parts[1]),
                                float.Parse(parts[2]),
                                float.Parse(parts[3])
                            );
                        }
                        break;

                    case DialogueResult.ResultType.TriggerEvent:
                        // 触发 UnityEvent 或特定事件
                        Debug.Log($"触发事件: {result.parameter}");
                        break;

                    case DialogueResult.ResultType.SetFlag:
                        gameFlags.Add(result.parameter);
                        break;

                    case DialogueResult.ResultType.AdvanceTime:
                        DayNightCycle.Instance?.SkipToNextPhase();
                        break;

                    case DialogueResult.ResultType.StartDialogue:
                        DialogueData nextDialogue = Resources.Load<DialogueData>($"Dialogues/{result.parameter}");
                        if (nextDialogue != null)
                            StartDialogue(nextDialogue);
                        break;
                }
            }
        }

        public void EndDialogue()
        {
            if (currentDialogue != null)
                completedDialogues.Add(currentDialogue.dialogueID);

            isDialogueActive = false;
            dialoguePanel?.SetActive(false);
            currentDialogue = null;

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
        }

        public bool IsDialogueCompleted(string dialogueID)
        {
            return completedDialogues.Contains(dialogueID);
        }

        public bool HasFlag(string flag)
        {
            return gameFlags.Contains(flag);
        }
    }
}
