using System.Collections.Generic;

namespace HuaPi.UI.Core
{
    /// <summary>
    /// UI 面板栈：管理面板的打开顺序和返回路径
    /// </summary>
    public class UIStack
    {
        private readonly List<UIPanelType> _stack = new List<UIPanelType>();

        public int Count => _stack.Count;

        public void Push(UIPanelType type)
        {
            _stack.Add(type);
        }

        public UIPanelType Pop()
        {
            if (_stack.Count == 0) return UIPanelType.MainMenu;
            UIPanelType top = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            return top;
        }

        public UIPanelType Peek()
        {
            if (_stack.Count == 0) return UIPanelType.MainMenu;
            return _stack[_stack.Count - 1];
        }

        public void Remove(UIPanelType type)
        {
            _stack.Remove(type);
        }

        public void Clear()
        {
            _stack.Clear();
        }

        public bool Contains(UIPanelType type)
        {
            return _stack.Contains(type);
        }
    }
}
