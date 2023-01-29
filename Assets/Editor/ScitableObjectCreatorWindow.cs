using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ScitableObjectCreatorWindow : EditorWindow
{
    [MenuItem("RenTool/ScitableObject Creator Window")]
    private static void OpenWindow()
    {
        var window = GetWindow<ScitableObjectCreatorWindow>();
        window.titleContent = new GUIContent("ScitableObject Creator");
        window.Show();
    }

    private List<string> names;
    List<Type> types;

    protected void OnEnable()
    {
        names = new List<string>();
        types = new List<Type>();

        var SOTypes = Util_Type.GetDerivedTypes_InAppDomain(typeof(ScriptableObject));
        foreach (var item in SOTypes)
        {
            var attr = Util_Type.GetAttributeInType<CreateAssetMenuAttribute>(item);
            if (attr != null)
            {
                names.Add(item.Name);
                types.Add(item);
            }
        }
    }

    public void OnGUI()
    {
        for (int i = 0; i < names.Count; i++)
        {
            var item = names[i];
            if (GUILayout.Button(item))
            {
                CreateAsset(types[i]);
            }
        }
    }

    public void CreateAsset(Type atype)
    {
        var asset = CreateInstance(atype.Namespace + "." + atype.Name);

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + atype.Name.ToString() + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }

}
