using System.Collections.Generic;
using UnityEditor;
/// Written by @cortvi
using UnityEngine;

public static class EditorGUILayoutUtil
{
    public static bool DrawDefaultInspectorWithoutScriptField(this Editor Inspector)
    {
        //Example
        //public override void OnInspectorGUI()
        //{
        //    this.DrawDefaultInspectorWithoutScriptField();
        //}
        EditorGUI.BeginChangeCheck();
        Inspector.serializedObject.Update();
        SerializedProperty Iterator = Inspector.serializedObject.GetIterator();
        Iterator.NextVisible(true);

        while (Iterator.NextVisible(false))
        {
            EditorGUILayout.PropertyField(Iterator, true);
        }
        Inspector.serializedObject.ApplyModifiedProperties();
        return (EditorGUI.EndChangeCheck());
    }

    #region HorizontalLine

    static readonly Color DefaultColor = new Color(1f, 1f, 1f, 0.2f);
    static readonly Vector2 DefaultMargin = new Vector2(2f, 2f);
    static readonly float DefaultLineHeight = 1.5f;

    public static void HorizontalLine(Color color, float lineHeight, Vector2 margin)
    {
        GUILayout.Space(margin.x);
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, lineHeight), color);
        GUILayout.Space(margin.y);
    }

    public static void HorizontalLine(Color color, float height) => HorizontalLine(color, height, DefaultMargin);
    public static void HorizontalLine(Color color, Vector2 margin) => HorizontalLine(color, DefaultLineHeight, margin);
    public static void HorizontalLine(float height, Vector2 margin) => HorizontalLine(DefaultColor, height, margin);
    public static void HorizontalLine(Color color) => HorizontalLine(color, DefaultLineHeight, DefaultMargin);
    public static void HorizontalLine(float height) => HorizontalLine(DefaultColor, height, DefaultMargin);
    public static void HorizontalLine(Vector2 margin) => HorizontalLine(DefaultColor, DefaultLineHeight, margin);
    public static void HorizontalLine() => HorizontalLine(DefaultColor, DefaultLineHeight, DefaultMargin);

    #endregion HorizontalLine

    public static LayerMask LayerMaskField(string label, LayerMask layerMask)
    {
        List<string> layers = new List<string>();
        List<int> layerNumbers = new List<int>();

        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (layerName != "")
            {
                layers.Add(layerName);
                layerNumbers.Add(i);
            }
        }
        int maskWithoutEmpty = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                maskWithoutEmpty |= (1 << i);
        }
        maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
        int mask = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if ((maskWithoutEmpty & (1 << i)) > 0)
                mask |= (1 << layerNumbers[i]);
        }
        layerMask.value = mask;
        return layerMask;
    }
}