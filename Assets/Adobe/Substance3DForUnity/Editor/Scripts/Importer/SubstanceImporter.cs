using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using Adobe.Substance;

#if UNITY_2020_2_OR_NEWER

using UnityEditor.AssetImporters;

#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace Adobe.SubstanceEditor.Importer
{
    internal class SubstanceAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions rao)
        {
            if (string.IsNullOrEmpty(assetPath))
                return AssetDeleteResult.DidNotDelete;

            if (AssetDatabase.IsValidFolder(assetPath))
                return CanDeleteFolder(assetPath, rao) ? AssetDeleteResult.DidNotDelete : AssetDeleteResult.FailedDelete;

            var substanceInstance = AssetDatabase.LoadAssetAtPath<SubstanceGraphSO>(assetPath);

            if (substanceInstance != null)
            {
                if (substanceInstance.FlagedForDelete || !File.Exists(substanceInstance.AssetPath))
                {
                    return AssetDeleteResult.DidNotDelete;
                }
                else
                {
                    Debug.LogWarning($"The target file cannot be manually deleted because it is associated with {substanceInstance.AssetPath}. In order to delete it, first the .sbsar file must be deleted.");
                    return AssetDeleteResult.FailedDelete;
                }
            }

            if (Path.GetExtension(assetPath.ToLower()) != ".sbsar")
                return AssetDeleteResult.DidNotDelete;

            SubstanceImporter importer = AssetImporter.GetAtPath(assetPath) as SubstanceImporter;

            if (importer != null)
            {
                foreach (var materialInstance in importer._instancesCopy)
                {
                    if (materialInstance == null)
                        continue;

                    SubstanceEditorEngine.instance.ReleaseInstance(materialInstance);
                    materialInstance.FlagedForDelete = true;
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(materialInstance));
                }
            }

            return AssetDeleteResult.DidNotDelete;
        }

        public static AssetMoveResult OnWillMoveAsset(string from, string to)
        {
            if (string.IsNullOrEmpty(from))
                return AssetMoveResult.DidNotMove;

            var substanceInstance = AssetDatabase.LoadAssetAtPath<SubstanceGraphSO>(from);

            if (substanceInstance != null)
            {
                substanceInstance.Move(to);
                return AssetMoveResult.DidNotMove;
            }

            if (Path.GetExtension(from.ToLower()) != ".sbsar")
                return AssetMoveResult.DidNotMove;

            var importer = AssetImporter.GetAtPath(from) as SubstanceImporter;

            if (importer != null)
            {
                var so = new SerializedObject(importer);
                var prop = so.FindProperty("_assetPath");

                if (prop != null && !string.IsNullOrEmpty(prop.stringValue))
                {
                    prop.stringValue = to;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            AssetDatabase.Refresh(ImportAssetOptions.Default);

            var fileObject = AssetDatabase.LoadAssetAtPath<SubstanceFileSO>(from);

            foreach (var materialInstance in fileObject.Instances)
                materialInstance.AssetPath = to;

            return AssetMoveResult.DidNotMove;
        }

        /// <summary>
        /// Checks if the target folder has sbsar file generated assets that can be deleted or not.
        /// </summary>
        /// <param name="assetPath">Path to the target folder.</param>
        /// <param name="rao">Remove asset options.</param>
        /// <returns>True if the folder can be deleted.</returns>
        private static bool CanDeleteFolder(string assetPath, RemoveAssetOptions rao)
        {
            var assetsGUIDs = AssetDatabase.FindAssets($"t:{nameof(SubstanceGraphSO)}", new[] { assetPath });

            foreach (var guid in assetsGUIDs)
            {
                var targetPath = AssetDatabase.GUIDToAssetPath(guid);

                var substanceInstance = AssetDatabase.LoadAssetAtPath<SubstanceGraphSO>(targetPath);

                if (substanceInstance != null)
                {
                    if (!substanceInstance.FlagedForDelete && File.Exists(substanceInstance.AssetPath))
                    {
                        Debug.LogWarning($"The target folder cannot be deleted manually because it has assets associated with {substanceInstance.AssetPath}. In order to delete it, first the .sbsar file must be deleted.");
                        return false;
                    }
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Importer for Substance Material Assets using the .sbsar extension .
    /// </summary>
    [ScriptedImporter(Adobe.Substance.Version.ImporterVersion, "sbsar")]
    public sealed class SubstanceImporter : ScriptedImporter
    {
        [SerializeField]
        public List<SubstanceGraphSO> _instancesCopy;

        [SerializeField]
        public SubstanceFileSO _fileAsset;

        [SerializeField]
        private string _assetPath;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            _assetPath = ctx.assetPath;

            if (_instancesCopy == null)
            {
                CreateSubstanceFile(ctx);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                UpdateSubstanceFile(ctx);
            }
        }

        private void CreateSubstanceFile(AssetImportContext ctx)
        {
            var rawData = AddRawFileData(ctx);

            EditorTools.InitializeSubstanceFile(ctx.assetPath, out int graphCount, out string fileGuid);

            _fileAsset = ScriptableObject.CreateInstance<SubstanceFileSO>();
            _fileAsset.AssetPath = _assetPath;
            _fileAsset.Instances = new List<SubstanceGraphSO>();

            Task task = Task.CompletedTask;

            for (int i = 0; i < graphCount; i++)
            {
                var graphSO = EditorTools.CreateSubstanceInstance(ctx.assetPath, rawData, $"graph_{i}", i, fileGuid, true);
                _fileAsset.Instances.Add(graphSO);

                if (!string.Equals(_assetPath, ctx.assetPath))
                {
                    _assetPath = ctx.assetPath;
                    EditorUtility.SetDirty(this);
                }
            }

            _ = RenderGraphsAsync(_fileAsset.Instances);

            ctx.AddObjectToAsset("Substance File", _fileAsset);
            ctx.SetMainObject(_fileAsset);

            _instancesCopy = _fileAsset.Instances;
        }

        private void UpdateSubstanceFile(AssetImportContext ctx)
        {
            _fileAsset = ScriptableObject.CreateInstance<SubstanceFileSO>();
            _fileAsset.AssetPath = _assetPath;
            _fileAsset.Instances = _instancesCopy;
            ctx.AddObjectToAsset("Substance Material", _fileAsset);
            AddRawFileData(ctx);
            ctx.SetMainObject(_fileAsset);
        }

        private SubstanceFileRawData AddRawFileData(AssetImportContext ctx)
        {
            var rawData = ScriptableObject.CreateInstance<SubstanceFileRawData>();
            rawData.FileContent = File.ReadAllBytes(ctx.assetPath);
            rawData.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            ctx.AddObjectToAsset("Substance Data", rawData);
            return rawData;
        }

        private Task RenderGraphsAsync(IReadOnlyList<SubstanceGraphSO> graph)
        {
            return SubstanceEditorEngine.instance.RenderInstanceAsync(graph);
        }
    }
}