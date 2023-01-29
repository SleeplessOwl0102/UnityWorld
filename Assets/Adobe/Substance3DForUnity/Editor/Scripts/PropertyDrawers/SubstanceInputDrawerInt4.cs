using UnityEditor;
using Adobe.Substance;

namespace Adobe.SubstanceEditor
{
    internal static class SubstanceInputDrawerInt4
    {
        public static bool DrawInput(SerializedProperty valueProperty, SubstanceInputGUIContent content, SubstanceNativeHandler handler, int graphID, int inputID)
        {
            int value0;
            int value1;
            int value2;
            int value3;
            bool changed;

            switch (content.Description.WidgetType)
            {
                //TODO: Add edge cases here.
                default:
                    changed = DrawDefault(valueProperty, content, out value0, out value1, out value2, out value3);
                    break;
            }

            if (changed)
                handler.SetInputInt4(value0, value1, value2, value3, inputID, graphID);

            return changed;
        }

        private static bool DrawDefault(SerializedProperty valueProperty, SubstanceInputGUIContent content, out int newValue0, out int newValue1, out int newValue2, out int newValue3)
        {
            bool result = false;

            var value0 = valueProperty?.FindPropertyRelative("_Data0");
            var value1 = valueProperty?.FindPropertyRelative("_Data1");
            var value2 = valueProperty?.FindPropertyRelative("_Data2");
            var value3 = valueProperty?.FindPropertyRelative("_Data3");

            var previewValue0 = value0.intValue;
            var previewValue1 = value1.intValue;
            var previewValue2 = value2.intValue;
            var previewValue3 = value3.intValue;

            newValue0 = EditorGUILayout.IntField(content, previewValue0);
            newValue1 = EditorGUILayout.IntField(content, previewValue0);
            newValue2 = EditorGUILayout.IntField(content, previewValue0);
            newValue3 = EditorGUILayout.IntField(content, previewValue0);

            if (newValue0 != previewValue0)
            {
                value0.intValue = newValue0;
                result = true;
            }

            if (newValue1 != previewValue1)
            {
                value1.intValue = newValue1;
                result = true;
            }

            if (newValue2 != previewValue2)
            {
                value2.intValue = newValue2;
                result = true;
            }

            if (newValue3 != previewValue3)
            {
                value3.intValue = newValue3;
                result = true;
            }

            return result;
        }
    }
}