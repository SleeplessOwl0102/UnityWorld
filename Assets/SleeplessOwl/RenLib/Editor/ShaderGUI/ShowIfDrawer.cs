#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public class ShowIfDrawer : MaterialPropertyDrawer
{
    string propertyName;
    bool condition;

    public ShowIfDrawer(string toggleName)
    {
        propertyName = toggleName;
    }

    override public void OnGUI(Rect pos, MaterialProperty prop, string label, MaterialEditor editor)
    {
        var mat = prop.targets[0] as Material;
        condition = mat.GetFloat(propertyName) == 1;
            
        if (condition)
        {
            editor.DefaultShaderProperty(prop, label);
        }
    }

    override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        //return condition ? base.GetPropertyHeight(prop, label, editor) : 0;
        return 0;
    }
}
#endif