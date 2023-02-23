using UnityEditor;
using UnityEngine;
using Adobe.Substance;

namespace Adobe.SubstanceEditor
{
    internal static class SubstanceInputDrawerInt3
    {
        public static bool DrawInput(SerializedProperty valueProperty, SubstanceInputGUIContent content, SubstanceNativeHandler handler, int graphID, int inputID)
        {
            Vector3Int newValue;
            bool changed;

            switch (content.Description.WidgetType)
            {
                default:
                    changed = DrawDefault(valueProperty, content, out newValue);
                    break;
            }

            if (changed)
                handler.SetInputInt3(newValue, inputID, graphID);

            return changed;
        }

        private static bool DrawDefault(SerializedProperty valueProperty, SubstanceInputGUIContent content, out Vector3Int newValue)
        {
            var previewValue = valueProperty.vector3IntValue;
            newValue = EditorGUILayout.Vector3IntField(content, previewValue);

            if (newValue != previewValue)
            {
                valueProperty.vector3IntValue = newValue;
                return true;
            }

            return false;
        }
    }
}