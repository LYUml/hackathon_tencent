using UnityEngine;
using UnityEngine.InputSystem;

namespace TXGame
{
    /// <summary>
    /// 玩家控制器 - 俯视角移动 + 调查交互
    /// 适用于《画皮》推理解谜游戏，不包含战斗
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("移动")]
        [SerializeField] private float moveSpeed = 4f;

        [Header("交互")]
        [SerializeField] private float interactRange = 1.5f;
        [SerializeField] private LayerMask interactableLayer;

        [Header("动画")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        // 内部状态
        private Rigidbody2D rb;
        private Vector2 moveInput;
        private Vector2 lookDirection = Vector2.down;

        public Vector2 LookDirection => lookDirection;
        public bool IsMoving => moveInput.sqrMagnitude > 0.01f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }

        private void Update()
        {
            UpdateLookDirection();
            UpdateAnimation();
        }

        #region 输入回调 (通过 PlayerInput 组件调用)

        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                PerformInteract();
            }
        }

        public void OnOpenJournal(InputValue value)
        {
            if (value.isPressed)
            {
                // 打开/关闭调查笔记
                InvestigationUI invUI = FindObjectOfType<InvestigationUI>();
                if (invUI != null)
                {
                    invUI.TogglePanel();
                }
            }
        }

        public void OnOpenHuapi(InputValue value)
        {
            if (value.isPressed)
            {
                // 打开/关闭画皮界面
                CharacterProfileUI profileUI = FindObjectOfType<CharacterProfileUI>();
                if (profileUI != null)
                {
                    profileUI.ShowCharacterList();
                }
            }
        }

        #endregion

        #region 移动与朝向

        private void UpdateLookDirection()
        {
            if (moveInput.sqrMagnitude > 0.01f)
            {
                lookDirection = moveInput.normalized;
            }
        }

        private void UpdateAnimation()
        {
            if (animator != null)
            {
                animator.SetFloat("MoveX", lookDirection.x);
                animator.SetFloat("MoveY", lookDirection.y);
                animator.SetFloat("Speed", moveInput.magnitude);
            }

            // 水平翻转
            if (spriteRenderer != null && Mathf.Abs(lookDirection.x) > 0.1f)
            {
                spriteRenderer.flipX = lookDirection.x < 0;
            }
        }

        #endregion

        #region 交互

        private void PerformInteract()
        {
            Collider2D[] interactables = Physics2D.OverlapCircleAll(
                transform.position,
                interactRange,
                interactableLayer
            );

            IInteractable closest = null;
            float closestDist = float.MaxValue;

            foreach (Collider2D col in interactables)
            {
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract(gameObject))
                {
                    float dist = Vector2.Distance(transform.position, col.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = interactable;
                    }
                }
            }

            if (closest != null)
            {
                closest.OnInteract(gameObject);
            }
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}
