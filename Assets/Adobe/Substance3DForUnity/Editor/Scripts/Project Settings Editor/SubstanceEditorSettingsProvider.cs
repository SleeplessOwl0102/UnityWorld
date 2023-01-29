using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Adobe.SubstanceEditor.ProjectSettings
{
    /// <summary>
    /// Settings provider for the Adobe Substance tab in the Unity project settings UI.
    /// </summary>
    internal class SubstanceEditorSettingsProvider : SettingsProvider
    {
        private const string substanceURL = "https://substance3d.adobe.com/assets/";

        private SerializedObject _editorSettings;

        private SerializedProperty _generateAllTextureProp;

        private SerializedProperty _targetResolutionProp;

        private readonly GUIStyle _richTextStyle = new GUIStyle() { richText = true };

        private class Styles
        {
            public static GUIContent generateAllTexturesGUI = new GUIContent("Generate all outputs", "Generate all output textures for the substance graphs.");
            public static GUIContent textureResoltuionGUI = new GUIContent("Texture resolution", "Texture resolution for all graphs outputs.");
        }

        public SubstanceEditorSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _editorSettings = SubstanceEditorSettingsSO.GetSerializedSettings();
            _generateAllTextureProp = _editorSettings.FindProperty("_generateAllTexture");
            _targetResolutionProp = _editorSettings.FindProperty("_targetResolution");
        }

        public override void OnGUI(string searchContext)
        {
            _editorSettings.Update();

            EditorGUILayout.Space();

            EditorGUI.indentLevel++;
            {
                if (_generateAllTextureProp != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(_generateAllTextureProp, Styles.generateAllTexturesGUI, GUILayout.Width(100));
                }

                if (_targetResolutionProp != null)
                {
                    EditorGUILayout.Space();
                    EditorDrawUtilities.DrawResolutionSelection(_targetResolutionProp, Styles.textureResoltuionGUI);
                }

                DrawURLText($"<a href=\"{substanceURL}\"> Substance 3D</a>", substanceURL);
                DrawAboutText();
            }
            EditorGUI.indentLevel--;

            _editorSettings.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the "About" hyperlink text and handles click events.
        /// </summary>
        private void DrawAboutText()
        {
            EditorGUILayout.Space();

            var labelRect = EditorGUILayout.GetControlRect();

            if (Event.current.type == EventType.MouseUp && labelRect.Contains(Event.current.mousePosition))
                Extensions.DrawAboutWindow();

            GUI.Label(labelRect, "<a href=\"\"> About</a>", _richTextStyle);
        }

        /// <summary>
        /// Draws a hyperlink label that should redirect the user to a URL.
        /// </summary>
        /// <param name="text">Label text.</param>
        /// <param name="url">Redirect URL.</param>
        private void DrawURLText(string text, string url)
        {
            EditorGUILayout.Space();

            var labelRect = EditorGUILayout.GetControlRect();

            if (Event.current.type == EventType.MouseUp && labelRect.Contains(Event.current.mousePosition))
                Application.OpenURL(url);

            GUI.Label(labelRect, text, _richTextStyle);
        }

        #region Registration

        [SettingsProvider]
        public static SettingsProvider CreateSubstanceSettingsProvider()
        {
            if (SubstanceEditorSettingsSO.IsSettingsAvailable())
            {
                return new SubstanceEditorSettingsProvider("Project/Adobe Substance 3D", SettingsScope.Project)
                {
                    label = "Adobe Substance 3D",
                    keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
                };
            }

            return null;
        }

        #endregion Registration
    }
}