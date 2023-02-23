using UnityEngine.UIElements;

namespace SleeplessOwl.EditorUtil.UIElement
{
    public class HorizontalElement : VisualElement
    {
        public HorizontalElement()
        {
            style.flexDirection = FlexDirection.Row;
        }
    }
}