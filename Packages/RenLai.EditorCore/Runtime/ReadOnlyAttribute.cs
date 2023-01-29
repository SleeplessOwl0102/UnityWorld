using UnityEngine;
using System;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Let serialized field be readonly in editor.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
[Conditional("UNITY_EDITOR")]
public class ReadOnlyAttribute : PropertyAttribute
{
    public ReadOnlyAttribute()
    {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
#endif

}
