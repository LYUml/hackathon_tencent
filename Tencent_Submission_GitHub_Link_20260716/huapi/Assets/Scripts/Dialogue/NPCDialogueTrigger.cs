using UnityEngine;

namespace TXGame
{
    /// <summary>
    /// NPC 对话触发器 - 挂载到 NPC 上，靠近后按交互键触发对话
    /// </summary>
    public class NPCDialogueTrigger : MonoBehaviour, IInteractable
    {
        [Header("对话设置")]
        [SerializeField] private DialogueData defaultDialogue;   // 默认对话
        [SerializeField] private DialogueData[] conditionalDialogues; // 条件对话（按优先级排列）

        [Header("NPC信息")]
        [SerializeField] private string npcName = "戏班成员";
        [SerializeField] private string interactPrompt = "交谈";

        [Header("视觉")]
        [SerializeField] private GameObject interactionIndicator;
        [SerializeField] private SpriteRenderer npcSprite;

        private bool playerInRange;

        private void Awake()
        {
            if (interactionIndicator == null)
                CreateInteractionIndicator();
        }

        private void Start()
        {
            interactionIndicator?.SetActive(false);
        }

        public void OnInteract(GameObject interactor)
        {
            DialogueData dialogue = GetAvailableDialogue();
            if (dialogue != null)
            {
                DialogueManager.Instance?.StartDialogue(dialogue);
            }
        }

        public string GetInteractPrompt()
        {
            return $"{interactPrompt} {npcName}";
        }

        public bool CanInteract(GameObject interactor)
        {
            return GetAvailableDialogue() != null;
        }

        /// <summary>
        /// 获取当前可用的对话（按优先级检查条件对话）
        /// </summary>
        private DialogueData GetAvailableDialogue()
        {
            if (DialogueManager.Instance == null) return defaultDialogue;

            // 按顺序检查条件对话
            if (conditionalDialogues != null)
            {
                foreach (var dialogue in conditionalDialogues)
                {
                    if (dialogue == null) continue;

                    // 检查条件是否满足
                    bool conditionsMet = true;
                    if (dialogue.conditions != null)
                    {
                        foreach (var condition in dialogue.conditions)
                        {
                            switch (condition.type)
                            {
                                case DialogueCondition.ConditionType.DialogueCompleted:
                                    if (!DialogueManager.Instance.IsDialogueCompleted(condition.parameter))
                                        conditionsMet = false;
                                    break;
                                case DialogueCondition.ConditionType.SetFlag:
                                    if (!DialogueManager.Instance.HasFlag(condition.parameter))
                                        conditionsMet = false;
                                    break;
                            }
                            if (!conditionsMet) break;
                        }
                    }

                    if (conditionsMet)
                    {
                        return dialogue;
                    }
                }
            }

            // 返回默认对话
            return defaultDialogue;
        }

        private void CreateInteractionIndicator()
        {
            GameObject go = new GameObject("InteractionIndicator_Auto");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 1.45f, 0f);

            TextMesh marker = go.AddComponent<TextMesh>();
            marker.text = "!";
            marker.fontSize = 96;
            marker.characterSize = 0.22f;
            marker.anchor = TextAnchor.MiddleCenter;
            marker.alignment = TextAlignment.Center;
            marker.color = new Color(1f, 0.78f, 0.25f, 0.95f);

            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            renderer.sortingOrder = 50;

            interactionIndicator = go;
            interactionIndicator.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                interactionIndicator?.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                interactionIndicator?.SetActive(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1.5f);
        }
    }
}
