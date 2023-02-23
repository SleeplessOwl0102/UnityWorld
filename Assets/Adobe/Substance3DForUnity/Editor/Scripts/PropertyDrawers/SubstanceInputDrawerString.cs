using UnityEditor;
using Adobe.Substance;

namespace Adobe.SubstanceEditor
{
    internal static class SubstanceInputDrawerString
    {
        public static bool DrawInput(SerializedProperty valueProperty, SubstanceInputGUIContent content, SubstanceNativeHandler handler, int graphID, int inputID)
        {
            bool changed;

            switch (content.Description.WidgetType)
            {
                default:
                    changed = DrawDefault(valueProperty, content);
                    break;
            }

            if (changed)
            {
                var stringValue = valueProperty.stringValue;
                handler.SetInputString(stringValue, inputID, graphID);
            }

            return changed;
        }

        private static bool DrawDefault(SerializedProperty valueProperty, SubstanceInputGUIContent content)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(valueProperty, content);
            return EditorGUI.EndChangeCheck();
        }
    }
}