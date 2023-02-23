
namespace SleeplessOwl.EditorUtil.Mono
{
    using System;


#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
#endif

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class LabelTextAttribute : PropertyAttribute
    {
        public string m_label;
        private int m_indent;

        public LabelTextAttribute() 
        {
            m_label = string.Empty;
        }
        public LabelTextAttribute(string label)
        {
            this.m_label = label;
        }

        public LabelTextAttribute(string label, int indent)
        {
            m_label = label;
            m_indent = indent;
        }


#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(LabelTextAttribute))]
        public class LabelTextDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var attr = attribute as LabelTextAttribute;
                label.text = attr.m_label;

                for (int i = 0; i < attr.m_indent; i++)
                    EditorGUI.indentLevel++;

                EditorGUI.PropertyField(position, property, label,true);

                for (int i = 0; i < attr.m_indent; i++)
                    EditorGUI.indentLevel--;
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
        }
#endif
    }
}
