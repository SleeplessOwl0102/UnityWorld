using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;

namespace RenLib
{

    public partial class IconMiner
    {
        private const string folderLocation = "Packages/RenLib/Editor/#AutoGenerate/";
        const string scriptFileName = "UnityEditorIcon.cs";

        [MenuItem("RenTool/IconMiner/Generate Icon Path Script", priority = 0)]
        private static void GenerateIconPathScript()
        {
            string folderPath = Application.dataPath.Remove(Application.dataPath.Length - 6) + folderLocation;

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var editorAssetBundle = GetEditorAssetBundle();
            var iconsPath = EditorResources.iconsPath;
            var readmeContents = new StringBuilder();

            readmeContents.AppendLine($"/* -----------------------------------------------------------");
            readmeContents.AppendLine($"Unity Editor Built-in Icons");
            readmeContents.AppendLine($"=================================================");
            readmeContents.AppendLine($"Unity version: {Application.unityVersion}");
            readmeContents.AppendLine($"Icons what can load using");
            readmeContents.AppendLine($"EditorGUIUtility.IconContent( );   to get GUIContent");
            readmeContents.AppendLine($"EditorGUIUtility.FindTexture( );   to get Texture2D");
            readmeContents.AppendLine($"-------------------------------------------------------------*/");
            readmeContents.AppendLine($"");
            readmeContents.AppendLine($"public class UnityEditorIcon");
            readmeContents.AppendLine("{");

            var assetNames = EnumerateIcons(editorAssetBundle, iconsPath).ToArray();

            HashSet<string> repeatDetectHashSet = new HashSet<string>();
            for (var i = 0; i < assetNames.Length; i++)
            {
                var assetPath = assetNames[i];
                var fileName = assetPath.Substring(assetPath.LastIndexOf('/') + 1).Replace(".png", string.Empty);
                var propertyName = fileName.Replace(".", "_").Replace("@", "_").Replace(" ", "_").Replace("-", "_");

                if (repeatDetectHashSet.Contains(fileName))
                    continue;
                repeatDetectHashSet.Add(fileName);

                // \t = Tab
                readmeContents.AppendLine($"\tpublic const string {propertyName} = \"{fileName}\";");
                Debug.Log(propertyName);
            }

            readmeContents.AppendLine("}");
            File.WriteAllText(folderPath + scriptFileName, readmeContents.ToString());
            AssetDatabase.Refresh();
            Debug.Log("Rebuild Tags, Layers, Sorting Layers, Scenes  Complete in" + " Assets/" + folderLocation);
        }

        [MenuItem("RenTool/IconMiner/Export All Icon", priority = 1)]
        private static void ExportIcons()
        {
            EditorUtility.DisplayProgressBar("Export Icons", "Exporting...", 0.0f);
            try
            {
                var editorAssetBundle = GetEditorAssetBundle();
                var iconsPath = EditorResources.iconsPath;
                var count = 0;
                foreach (var assetName in EnumerateIcons(editorAssetBundle, iconsPath))
                {
                    var icon = editorAssetBundle.LoadAsset<Texture2D>(assetName);
                    if (icon == null)
                        continue;

                    var readableTexture = new Texture2D(icon.width, icon.height, icon.format, icon.mipmapCount > 1);

                    Graphics.CopyTexture(icon, readableTexture);

                    var folderPath = Path.GetDirectoryName(Path.Combine("icons/original/", assetName.Substring(iconsPath.Length)));
                    if (Directory.Exists(folderPath) == false)
                        Directory.CreateDirectory(folderPath);

                    var iconPath = Path.Combine(folderPath, icon.name + ".png");
                    File.WriteAllBytes(iconPath, readableTexture.EncodeToPNG());

                    count++;
                }

                Debug.Log($"{count} icons has been exported!");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem("RenTool/IconMiner/Generate README.md", priority = 2)]
        private static void GenerateREADME()
        {
            var guidMaterial = new Material(Shader.Find("Unlit/Texture"));
            var guidMaterialId = "Assets/Editor/GuidMaterial.mat";
            AssetDatabase.CreateAsset(guidMaterial, guidMaterialId);

            EditorUtility.DisplayProgressBar("Generate README.md", "Generating...", 0.0f);
            try
            {
                var editorAssetBundle = GetEditorAssetBundle();
                var iconsPath = EditorResources.iconsPath;
                var readmeContents = new StringBuilder();

                readmeContents.AppendLine($"Unity Editor Built-in Icons");
                readmeContents.AppendLine($"==============================");
                readmeContents.AppendLine($"Unity version: {Application.unityVersion}");
                readmeContents.AppendLine($"Icons what can load using `EditorGUIUtility.IconContent`");
                readmeContents.AppendLine();
                readmeContents.AppendLine($"File ID");
                readmeContents.AppendLine($"-------------");
                readmeContents.AppendLine($"You can change script icon by file id");
                readmeContents.AppendLine($"1. Open `*.cs.meta` in Text Editor");
                readmeContents.AppendLine($"2. Modify line `icon: {{instanceID: 0}}` to `icon: {{fileID: <FILE ID>, guid: 0000000000000000d000000000000000, type: 0}}`");
                readmeContents.AppendLine($"3. Save and focus Unity Editor");
                readmeContents.AppendLine();
                readmeContents.AppendLine($"| Icon | Name | File ID |");
                readmeContents.AppendLine($"|------|------|---------|");

                var assetNames = EnumerateIcons(editorAssetBundle, iconsPath).ToArray();
                for (var i = 0; i < assetNames.Length; i++)
                {
                    var assetName = assetNames[i];
                    var icon = editorAssetBundle.LoadAsset<Texture2D>(assetName);
                    if (icon == null)
                        continue;

                    EditorUtility.DisplayProgressBar("Generate README.md", $"Generating... ({i + 1}/{assetNames.Length})", (float)i / assetNames.Length);

                    var readableTexture = new Texture2D(icon.width, icon.height, icon.format, icon.mipmapCount > 1);

                    Graphics.CopyTexture(icon, readableTexture);

                    var folderPath = Path.GetDirectoryName(Path.Combine("icons/small/", assetName.Substring(iconsPath.Length)));
                    if (Directory.Exists(folderPath) == false)
                        Directory.CreateDirectory(folderPath);

                    var iconPath = Path.Combine(folderPath, icon.name + ".png");
                    File.WriteAllBytes(iconPath, readableTexture.EncodeToPNG());

                    //
                    guidMaterial.mainTexture = icon;
                    EditorUtility.SetDirty(guidMaterial);
                    AssetDatabase.SaveAssets();
                    var fileId = GetFileId(guidMaterialId);

                    var escapedUrl = iconPath.Replace(" ", "%20").Replace('\\', '/');
                    readmeContents.AppendLine($"| ![]({escapedUrl}) | `{icon.name}` | `{fileId}` |");
                }

                File.WriteAllText("README.md", readmeContents.ToString());

                Debug.Log($"'READMD.md' has been generated.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.DeleteAsset(guidMaterialId);
            }
        }

        private static IEnumerable<string> EnumerateIcons(AssetBundle editorAssetBundle, string iconsPath)
        {
            foreach (var assetName in editorAssetBundle.GetAllAssetNames())
            {
                if (assetName.StartsWith(iconsPath, StringComparison.OrdinalIgnoreCase) == false)
                    continue;
                if (assetName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) == false &&
                    assetName.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) == false)
                    continue;

                yield return assetName;
            }
        }

        private static string GetFileId(string proxyAssetPath)
        {
            var serializedAsset = File.ReadAllText(proxyAssetPath);
            var index = serializedAsset.IndexOf("_MainTex:", StringComparison.Ordinal);
            if (index == -1)
                return string.Empty;

            const string FileId = "fileID:";
            var startIndex = serializedAsset.IndexOf(FileId, index) + FileId.Length;
            var endIndex = serializedAsset.IndexOf(',', startIndex);
            return serializedAsset.Substring(startIndex, endIndex - startIndex).Trim();
        }

        private static AssetBundle GetEditorAssetBundle()
        {
            var editorGUIUtility = typeof(EditorGUIUtility);
            var getEditorAssetBundle = editorGUIUtility.GetMethod(
                "GetEditorAssetBundle",
                BindingFlags.NonPublic | BindingFlags.Static);

            return (AssetBundle)getEditorAssetBundle.Invoke(null, new object[] { });
        }

    }
}