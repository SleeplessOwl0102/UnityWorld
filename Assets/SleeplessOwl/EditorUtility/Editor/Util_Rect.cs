using UnityEditor;
using UnityEngine;

namespace SleeplessOwl.EditorUtil
{
    public static class Util_Rect
    {
        public static ref Rect NextSingleLine(this ref Rect rect)
        {
            rect.y += EditorGUIUtility.singleLineHeight;
            rect.height = EditorGUIUtility.singleLineHeight;
            return ref rect;
        }

        public static ref Rect NextMultyLine(this ref Rect rect,int lineCount)
        {
            rect.y += EditorGUIUtility.singleLineHeight;
            rect.height = EditorGUIUtility.singleLineHeight * lineCount;
            return ref rect;
        }

        public static ref Rect ShiftRight(this ref Rect rect,float width)
        {
            rect.x += width;
            rect.width -= width;
            return ref rect;
        }

        public static ref Rect ShiftLeft(this ref Rect rect, float width)
        {
            rect.x -= width;
            rect.width += width;
            return ref rect;
        }


        public static ref Rect SingleLine(this ref Rect rect)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            return ref rect;
        }


        public static Rect GetRectLeft(this ref Rect rect, float width)
        {
            var rectCur = rect;

            rect.x += width;
            rect.width -= width;

            rectCur.width = width;
            return rectCur;
        }
        public static Rect GetRectRight(this ref Rect rect, float width)
        {
            var rectCur = rect;
            rect.width -= width;

            rectCur.x = rectCur.x+ rectCur.width - width;
            rectCur.width = width;
            return rectCur;
        }

        public static Rect GetRectRightLabel(this ref Rect rect)
        {
            return rect.GetRectRight(EditorGUIUtility.labelWidth);
        }
    }
}
