using UnityEngine.UIElements;

namespace SleeplessOwl.EditorUtil.UIElement
{
    /// <summary>
    /// MenuTreeView中每个选项按钮的接口，可继承后实现客制化内容
    /// </summary>
    public interface IMenuItem
    {
        string GetMenuPath { get; }

        string GetItemLabelText { get; }

        VisualElement GetVisualElement { get; }

        void Init(int id, MenuTreeView treeView);
    }
}