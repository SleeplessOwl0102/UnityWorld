using UnityEngine.UIElements;


namespace SleeplessOwl.EditorUtil.EditorUtil
{

    public static class Util_VisualElement
    {
        public static bool TryRemove(this VisualElement visualElement, VisualElement removeTarget)
        {
            if (visualElement.Contains(removeTarget))
            {
                visualElement.Remove(removeTarget);
                return true;
            }
            return false;
        }

        public static VisualElement SetAllUnFocusable(this VisualElement ve)
        {
            foreach (var child in ve.Children())
            {
                child.focusable = false;
            }
            return ve;
        }

        public static VisualElement SetAllOpacity(this VisualElement ve)
        {
            foreach (var child in ve.Children())
            {
                child.style.opacity = 1;
                SetAllOpacity(child);
            }
            return ve;
        }
    }
}