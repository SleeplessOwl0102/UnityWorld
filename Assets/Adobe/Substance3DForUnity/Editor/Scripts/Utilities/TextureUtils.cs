using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    internal static class TextureUtils
    {
        public static bool IsCompressed(this TextureFormat unityFormat)
        {
            switch (unityFormat)
            {
                case TextureFormat.RGBA32:
                case TextureFormat.RGBA64:
                case TextureFormat.RGB24:
                case TextureFormat.BGRA32:
                case TextureFormat.R8:
                case TextureFormat.R16:
                case TextureFormat.RFloat:
                    return false;

                default:
                    return true;
            }
        }

        public static Texture2D SetReadableFlag(Texture2D pTexture, bool pReadable)
        {
            Texture2D texture = pTexture;

            if (pTexture == null)
                return null;

            string assetPath = AssetDatabase.GetAssetPath(pTexture);

            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter != null)
            {
                if (textureImporter.isReadable == pReadable)
                    return pTexture;

                textureImporter.isReadable = pReadable;
                Debug.LogWarning(string.Format("Setting {0}'s 'Read/Write Enabled' flag to {1}",
                                                pTexture.name, (pReadable ? "true" : "false")));

                EditorUtility.SetDirty(textureImporter);
                AssetDatabase.ImportAsset(assetPath);
                AssetDatabase.Refresh();

                texture = AssetDatabase.LoadMainAssetAtPath(assetPath) as Texture2D;
            }

            return texture;
        }

        public static Texture2D EnsureTextureCorrectness(Texture2D pTexture, bool ensureRGBA, bool enableMipMaps)
        {
            Texture2D texture = pTexture;

            if (pTexture == null)
                return null;

            string assetPath = AssetDatabase.GetAssetPath(pTexture);

            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (textureImporter != null)
            {
                bool changed = false;

                if (textureImporter.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                    Debug.LogWarning(string.Format("Setting {0}'s 'Compression' flag to Uncompressed", pTexture.name));
                    changed = true;
                }

                if (textureImporter.isReadable != true)
                {
                    textureImporter.isReadable = true;
                    Debug.LogWarning(string.Format("Setting {0}'s 'Read/Write Enabled' flag to {1}",
                                                    pTexture.name, (true ? "true" : "false")));
                    changed = true;
                }

                if (textureImporter.maxTextureSize < 4096)
                {
                    textureImporter.maxTextureSize = 4096;
                    changed = true;
                }

                if (enableMipMaps != textureImporter.mipmapEnabled)
                {
                    textureImporter.mipmapEnabled = enableMipMaps;
                    changed = true;
                }

                if (ensureRGBA)
                {
                    var defaultSettings = textureImporter.GetDefaultPlatformTextureSettings();

                    if (defaultSettings.format != TextureImporterFormat.RGBA32)
                    {
                        defaultSettings.format = TextureImporterFormat.RGBA32;
                        textureImporter.SetPlatformTextureSettings(defaultSettings);
                        changed = true;
                    }
                }

                if (changed)
                {
                    AssetDatabase.ImportAsset(assetPath);
                    texture = AssetDatabase.LoadMainAssetAtPath(assetPath) as Texture2D;
                }
            }

            return texture;
        }
    }
}