using Adobe.Substance;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    internal static class EditorTools
    {
        /// <summary>
        /// Makes an object editable. (Usefull for object managed by Importers)
        /// </summary>
        /// <param name="pObject"></param>
        public static void OverrideReadOnlyFlag(Object unityObject)
        {
            unityObject.hideFlags &= ~HideFlags.NotEditable;
        }

        internal static void InitializeSubstanceFile(string assetPath, out int graphCount, out string guid)
        {
            SubstanceEditorEngine.instance.InitializeSubstanceFile(assetPath, out graphCount, out guid);
        }

        public static SubstanceGraphSO CreateSubstanceInstance(string assetPath, SubstanceFileRawData fileData, string name, int index, string guid, bool isRoot = false, SubstanceGraphSO copy = null)
        {
            var instanceAsset = ScriptableObject.CreateInstance<SubstanceGraphSO>();
            instanceAsset.AssetPath = assetPath;
            instanceAsset.RawData = fileData;
            instanceAsset.Name = name;
            instanceAsset.IsRoot = isRoot;
            instanceAsset.GUID = guid;
            instanceAsset.OutputPath = CreateGraphFolder(assetPath, name);
            instanceAsset.Index = index;
            instanceAsset.GenerateAllMipmaps = true;
            var instancePath = MakeRootGraphAssetPath(instanceAsset);
            SubstanceEditorEngine.instance.InitializeInstance(instanceAsset, instancePath);
            SubstanceEditorEngine.instance.CreateGraphObject(instanceAsset, copy);
            AssetDatabase.CreateAsset(instanceAsset, instancePath);
            return instanceAsset;
        }

        public static void Rename(this SubstanceGraphSO substanceMaterial, string name)
        {
            var oldFolder = substanceMaterial.OutputPath;

            if (substanceMaterial.Name == name)
                return;

            substanceMaterial.Name = name;

            var dir = Path.GetDirectoryName(substanceMaterial.AssetPath);
            var assetName = Path.GetFileNameWithoutExtension(substanceMaterial.AssetPath);
            var newFolder = Path.Combine(dir, $"{assetName}_{name}");
            substanceMaterial.OutputPath = newFolder;

            FileUtil.MoveFileOrDirectory(oldFolder, substanceMaterial.OutputPath);
            File.Delete($"{oldFolder}.meta");

            EditorUtility.SetDirty(substanceMaterial);
            AssetDatabase.Refresh();

            var oldPath = AssetDatabase.GetAssetPath(substanceMaterial);
            var error = AssetDatabase.RenameAsset(oldPath, $"{name}.asset");

            if (!string.IsNullOrEmpty(error))
                Debug.LogError(error);

            var materialOldName = AssetDatabase.GetAssetPath(substanceMaterial.OutputMaterial);
            var materialNewName = Path.GetFileName(substanceMaterial.GetAssociatedAssetPath($"{name}_material", "mat"));
            error = AssetDatabase.RenameAsset(materialOldName, materialNewName);
            EditorUtility.SetDirty(substanceMaterial.OutputMaterial);

            if (!string.IsNullOrEmpty(error))
                Debug.LogError(error);

            AssetDatabase.Refresh();
        }

        public static void Move(this SubstanceGraphSO substanceMaterial, string to)
        {
            substanceMaterial.OutputPath = Path.GetDirectoryName(to);

            var oldMaterialPath = AssetDatabase.GetAssetPath(substanceMaterial.OutputMaterial);
            AssetDatabase.MoveAsset(oldMaterialPath, Path.Combine(substanceMaterial.OutputPath, Path.GetFileName(oldMaterialPath)));

            foreach (var output in substanceMaterial.Output)
            {
                var textureAssetPath = AssetDatabase.GetAssetPath(output.OutputTexture);
                var textureFileName = Path.GetFileName(textureAssetPath);
                var newTexturePath = Path.Combine(substanceMaterial.OutputPath, textureFileName);
                AssetDatabase.MoveAsset(textureAssetPath, newTexturePath);
            }

            EditorUtility.SetDirty(substanceMaterial);
            AssetDatabase.Refresh();
        }

        private static string CreateGraphFolder(string assetPath, string graphName)
        {
            var dir = Path.GetDirectoryName(assetPath);
            var assetName = Path.GetFileNameWithoutExtension(assetPath);

            var newFolder = Path.Combine(dir, $"{assetName}_{graphName}");

            if (Directory.Exists(newFolder))
                return newFolder;

            string guid = AssetDatabase.CreateFolder(dir, $"{assetName}_{graphName}");
            return AssetDatabase.GUIDToAssetPath(guid);
        }

        private static string MakeRootGraphAssetPath(SubstanceGraphSO substanceMaterial)
        {
            return Path.Combine(substanceMaterial.OutputPath, $"{substanceMaterial.Name}.asset");
        }
    }
}