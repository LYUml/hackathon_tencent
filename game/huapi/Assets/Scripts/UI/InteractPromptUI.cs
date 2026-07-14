using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TXGame
{
    /// <summary>
    /// 场景可调查物体的交互提示 UI
    /// </summary>
    public class InteractPromptUI : MonoBehaviour
    {
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private float checkRadius = 2f;
        [SerializeField] private LayerMask interactableLayer;

        private Transform player;
        private IInteractable currentTarget;

        private void Start()
        {
            promptPanel?.SetActive(false);

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        private void Update()
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.transform;
                else return;
            }

            Collider2D[] interactables = Physics2D.OverlapCircleAll(
                player.position, checkRadius, interactableLayer
            );

            IInteractable closest = null;
            float closestDist = float.MaxValue;

            foreach (Collider2D col in interactables)
            {
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract(player.gameObject))
                {
                    float dist = Vector2.Distance(player.position, col.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = interactable;
                    }
                }
            }

            if (closest != currentTarget)
            {
                currentTarget = closest;
                if (closest != null)
                {
                    ShowPrompt(closest.GetInteractPrompt());
                }
                else
                {
                    HidePrompt();
                }
            }
        }

        private void ShowPrompt(string text)
        {
            promptPanel?.SetActive(true);
            if (promptText != null) promptText.text = text;
        }

        private void HidePrompt()
        {
            promptPanel?.SetActive(false);
            currentTarget = null;
        }
    }
}
