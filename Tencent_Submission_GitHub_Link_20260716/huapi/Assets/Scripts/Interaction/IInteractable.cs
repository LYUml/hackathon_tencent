using UnityEngine;

namespace TXGame
{
    /// <summary>
    /// 可交互接口 - 所有可交互物体应实现此接口
    /// </summary>
    public interface IInteractable
    {
        void OnInteract(GameObject interactor);
        string GetInteractPrompt();
        bool CanInteract(GameObject interactor);
    }
}
