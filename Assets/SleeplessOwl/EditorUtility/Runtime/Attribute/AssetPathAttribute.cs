using System;
using UnityEngine;
namespace SleeplessOwl.EditorUtil.Mono
{
    public enum AssetPathTypes
    {
        /// <summary>
        /// The path will be contained within the 'Asset/*' directory.
        /// </summary>
        Project,
        /// <summary>
        /// The path will be contained within a resources folder.
        /// </summary>
        Resources,
    }

    /// <summary>
    /// We limit this attributes to fields and only allow one. Should
    /// only be applied to string types. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class AssetPathAttribute : PropertyAttribute
    {
        /// <summary>
        /// Gets the type of asset path this attribute is watching.
        /// </summary>
        public AssetPathTypes PathType { get; }

        /// <summary>
        /// Gets the type of asset this attribute is expecting.
        /// </summary>
        public Type Type { get; }

        public Type ComponentType { get; }

        public AssetPathAttribute(Type unityObjectType)
        {
            Type = unityObjectType;
            PathType = AssetPathTypes.Project;
        }

        public AssetPathAttribute(Type unityObjectType, Type requireComponentType)
        {
            Type = unityObjectType;
            ComponentType = requireComponentType;
            PathType = AssetPathTypes.Project;
        }

        /// <summary>
        /// Takes the string from the Asset Path Attribute and converts it into
        /// a usable resources path.
        /// </summary>
        public static string ConvertToResourcesPath(string projectPath)
        {
            string RESOURCES_FOLDER_NAME = "/Resources/";

            if (string.IsNullOrEmpty(projectPath))
            {
                return string.Empty;
            }

            // Get the index of the resources folder
            int folderIndex = projectPath.IndexOf(RESOURCES_FOLDER_NAME);

            // If it's -1 we this asset is not in a resource folder
            if (folderIndex == -1)
            {
                return string.Empty;
            }

            folderIndex += RESOURCES_FOLDER_NAME.Length;
            int length = projectPath.Length - folderIndex;

            length -= projectPath.Length - projectPath.LastIndexOf('.');

            string resourcesPath = projectPath.Substring(folderIndex, length);
            return resourcesPath;
        }
    }

}