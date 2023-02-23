using UnityEditor;

public class CreateTemplateScript
{
    private const string pathToYourScriptTemplate = "Assets/SleeplessOwl/RenLib/Editor/CreateScript";
    private const int priority = 2000;

    [MenuItem("Assets/Create Monobehaviour", false, priority)]
    public static void Monobehaviour()
    {
        var path = pathToYourScriptTemplate + "/Monobehaviour.cs.txt";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "New Monobehaviour.cs");
    }

    [MenuItem("Assets/Create ScriptableObject", false, priority)]
    public static void Scriptableobject()
    {
        var path = pathToYourScriptTemplate + "/ScriptableObject.cs.txt";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "New ScriptableObject.cs");
    }

    [MenuItem("Assets/Create README.md", false, priority)]
    public static void Readme_md()
    {
        var path = pathToYourScriptTemplate + "/README.md";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "README.md");
    }

    [MenuItem("Assets/Create Shader (.hlsl)", false, priority)]
    public static void Shader()
    {
        var path = pathToYourScriptTemplate + "/ShaderWithHLSL.shader.txt";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "New Shader.shader");
    }

    [MenuItem("Assets/Create HLSL (.shader)", false, priority)]
    public static void HLSL()
    {
        var path = pathToYourScriptTemplate + "/ShaderWithHLSL.hlsl.txt";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "New HLSL.hlsl");
    }

}