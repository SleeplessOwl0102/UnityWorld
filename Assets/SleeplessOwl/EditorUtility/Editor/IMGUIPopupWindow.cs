using System;
using UnityEditor;
using UnityEngine;

namespace SleeplessOwl.EditorUtil
{
    /// <summary>
    /// 用来绘制IMGUI版本的的PopupWindowContent
    /// </summary>
    public class IMGUIPopupWindow : PopupWindowContent
    {
        private Action m_draw;

        public IMGUIPopupWindow(Action drawIMGUIContent)
        {
            m_draw = drawIMGUIContent;
        }
        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 500);
        }

        public override void OnGUI(Rect rect)
        {
            m_draw?.Invoke();
        }
    }
}