using UnityEngine;

namespace TXGame
{
    /// <summary>
    /// 场景调查点 - 白天探索时可调查的场景物体
    /// </summary>
    public class InvestigationPoint : MonoBehaviour, IInteractable
    {
        [Header("调查点设置")]
        [SerializeField] private string pointName = "可疑物品";
        [SerializeField] private string[] unlockClueIDs;        // 调查后解锁的线索
        [SerializeField] private string[] requiredClueIDs;      // 需要的先决线索

        [Header("提示")]
        [SerializeField] private string hintText = "调查";

        [Header("一次性")]
        [SerializeField] private bool oneTimeUse = true;
        [SerializeField] private Sprite investigatedSprite;     // 调查后的图

        private bool isInvestigated;
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void OnInteract(GameObject interactor)
        {
            if (isInvestigated && oneTimeUse) return;
            if (!CanInteract(interactor)) return;

            // 解锁线索
            foreach (string clueID in unlockClueIDs)
            {
                InvestigationManager.Instance?.AddClue(clueID);
            }

            isInvestigated = true;

            // 更新外观
            if (spriteRenderer != null && investigatedSprite != null)
            {
                spriteRenderer.sprite = investigatedSprite;
            }

            Debug.Log($"调查了: {pointName}");
        }

        public string GetInteractPrompt()
        {
            if (isInvestigated && oneTimeUse)
                return $"{pointName} (已调查)";
            return $"{hintText} {pointName}";
        }

        public bool CanInteract(GameObject interactor)
        {
            if (isInvestigated && oneTimeUse) return false;

            // 检查先决线索
            if (requiredClueIDs != null && requiredClueIDs.Length > 0)
            {
                InvestigationManager inv = InvestigationManager.Instance;
                if (inv == null) return false;

                foreach (string id in requiredClueIDs)
                {
                    if (!inv.HasClue(id)) return false;
                }
            }

            return true;
        }
    }
}
