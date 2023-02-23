using Adobe.Substance;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    [CustomEditor(typeof(SubstanceGraphSO))]
    public class SubstanceGraphSOEditor : UnityEditor.Editor
    {
        private GraphInputsGroupingHelper _inputGroupingHelper;

        private GraphOutputAlphaChannelsHelper _outputChannelsHelper;

        private bool _propertiesChanged = false;

        private bool _showOutput = true;

        private bool _showExportPresentationHandler = false;

        private SubstanceGraphSO _target = null;

        private SubstanceNativeHandler _handler = null;

        // Scrollview handling:
        private Rect lastRect;

        private Texture2D _backgroundImage;

        private MaterialEditor _materialPreviewEditor;

        private Vector2 _textureOutputScrollView;

        private SerializedProperty _generateAllOutputsProperty;
        private SerializedProperty _generateAllMipmapsProperty;
        private SerializedProperty _runtimeOnlyProperty;
        private SerializedProperty _outputRemapedProperty;
        private SerializedProperty _graphOutputs;
        private SerializedProperty _presetProperty;

        public void OnEnable()
        {
            if (!IsSerializedObjectReady())
                return;

            _target = serializedObject.targetObject as SubstanceGraphSO;
            _textureOutputScrollView = Vector2.zero;
            _propertiesChanged = false;

            if (_inputGroupingHelper == null)
                _inputGroupingHelper = new GraphInputsGroupingHelper(_target, serializedObject);

            if (_outputChannelsHelper == null)
                _outputChannelsHelper = new GraphOutputAlphaChannelsHelper(_target);

            float c = (EditorGUIUtility.isProSkin) ? 0.35f : 0.65f;

            if (_backgroundImage == null)
                _backgroundImage = Globals.CreateColoredTexture(16, 16, new Color(c, c, c, 1));

            EditorApplication.projectWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
            _generateAllOutputsProperty = serializedObject.FindProperty("GenerateAllOutputs");
            _generateAllMipmapsProperty = serializedObject.FindProperty("GenerateAllMipmaps");
            _runtimeOnlyProperty = serializedObject.FindProperty("IsRuntimeOnly");
            _outputRemapedProperty = serializedObject.FindProperty("OutputRemaped");
            _graphOutputs = serializedObject.FindProperty("Output");
            _presetProperty = serializedObject.FindProperty("CurrentStatePreset");

            SubstanceEditorEngine.instance.TryGetHandlerFromInstance(_target, out _handler);
        }

        public void OnDisable()
        {
            if (_materialPreviewEditor != null)
            {
                _materialPreviewEditor.OnDisable();
                _materialPreviewEditor = null;
            }

            SaveEditorChanges();
            EditorApplication.projectWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
        }

        public void SaveEditorChanges()
        {
            if (_propertiesChanged)
            {
                SaveTGAFiles();
                UpdateGraphMaterialLabel();
                AssetDatabase.Refresh();
            }

            _propertiesChanged = false;
        }

        public override void OnInspectorGUI()
        {
            if(_handler == null)
                if(!SubstanceEditorEngine.instance.TryGetHandlerFromInstance(_target, out _handler))
                    return;

            if (_materialPreviewEditor == null)
            {
                var material = _target.OutputMaterial;

                if (material != null)
                    _materialPreviewEditor = MaterialEditor.CreateEditor(material) as MaterialEditor;
            }

            serializedObject.Update();

            if (DrawGraph())
            {
                serializedObject.ApplyModifiedProperties();
                _propertiesChanged = true;
            }
        }

        /// <summary>
        /// Callback for GUI events to block substance files from been duplicated.
        /// </summary>
        /// <param name="guid">Asset guid.</param>
        /// <param name="rt">GUI rect.</param>
        protected static void OnHierarchyWindowItemOnGUI(string guid, Rect rt)
        {
            var currentEvent = Event.current;

            if ("Duplicate" == currentEvent.commandName && currentEvent.type == EventType.ExecuteCommand)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var instanceObject = AssetDatabase.LoadAssetAtPath<SubstanceGraphSO>(assetPath);

                if (instanceObject != null)
                {
                    Debug.LogWarning("Substance graph can not be manually duplicated.");
                    currentEvent.Use();
                }
            }
        }

        #region Material Preview

        public override bool HasPreviewGUI()
        {
            return _materialPreviewEditor != null;
        }

        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("Material", null, "");
        }

        public override void OnPreviewSettings()
        {
            if (_materialPreviewEditor)
                _materialPreviewEditor.OnPreviewSettings();
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (_materialPreviewEditor)
                _materialPreviewEditor.OnPreviewGUI(r, background);
        }

        #endregion Material Preview

        #region Draw

        private bool DrawGraph()
        {
            bool valuesChanged = false;

            if (DrawTextureGenerationSettings(_generateAllOutputsProperty, _generateAllMipmapsProperty, _runtimeOnlyProperty))
            {
                _outputRemapedProperty.boolValue = true;
                valuesChanged = true;
            }

            GUILayout.Space(8);

            if (DrawInputs())
            {
                var newPreset = _handler.CreatePresetFromCurrentState(_target.Index);
                _presetProperty.stringValue = newPreset;
                SubstanceEditorEngine.instance.SubmitAsyncRenderWork(_handler, _target);
                valuesChanged = true;
                UpdateInputsVisibilty();
            }

            DrawPresentExport(_target);

            EditorGUILayout.Space();

            _showOutput = EditorGUILayout.Foldout(_showOutput, "Generated textures");

            if (_showOutput)
            {
                if (DrawGeneratedTextures(_graphOutputs, _generateAllOutputsProperty.boolValue))
                {
                    _outputRemapedProperty.boolValue = true;
                    valuesChanged = true;
                }
            }

            return valuesChanged;
        }

        private void UpdateInputsVisibilty()
        {
        }

        #region Texture Generation Settings

        private bool DrawTextureGenerationSettings(SerializedProperty generateAllOutputsProperty, SerializedProperty generateAllMipmapsProperty, SerializedProperty runtimeOnlyProperty)
        {
            bool changed = false;

            GUILayout.Space(4);

            var boxWidth = EditorGUIUtility.currentViewWidth;
            var boxHeight = (3 * EditorGUIUtility.singleLineHeight) + 16;
            var padding = 16;

            DrawHighlightBox(boxWidth, boxHeight, padding);

            if (DrawGenerateAllOutputs(generateAllOutputsProperty) ||
                DrawGenerateAllMipmaps(generateAllMipmapsProperty) ||
                DrawRuntimeOnlyToggle(runtimeOnlyProperty))
            {
                changed = true;
            }

            return changed;
        }

        private static readonly GUIContent _GenerateAllOutputsGUI = new GUIContent("Generate All Outputs", "Force the generation of all Substance outputs");

        private bool DrawGenerateAllOutputs(SerializedProperty generateAllOutputsProperty)
        {
            var oldValue = generateAllOutputsProperty.boolValue;
            generateAllOutputsProperty.boolValue = EditorGUILayout.Toggle(_GenerateAllOutputsGUI, generateAllOutputsProperty.boolValue);
            return oldValue != generateAllOutputsProperty.boolValue;
        }

        private static readonly GUIContent _GenerateAllMipMapsGUI = new GUIContent("Generate Mip Maps", "Enable MipMaps when generating textures");

        private bool DrawGenerateAllMipmaps(SerializedProperty generateAllMipmapsProperty)
        {
            var oldValue = generateAllMipmapsProperty.boolValue;
            generateAllMipmapsProperty.boolValue = EditorGUILayout.Toggle(_GenerateAllMipMapsGUI, generateAllMipmapsProperty.boolValue);
            return oldValue != generateAllMipmapsProperty.boolValue;
        }

        private static readonly GUIContent _RuntimeOnlyGUI = new GUIContent("Runtime only", "If checked this instance will not generate TGA texture files");

        private bool DrawRuntimeOnlyToggle(SerializedProperty runtimeOnlyProperty)
        {
            var oldValue = runtimeOnlyProperty.boolValue;
            runtimeOnlyProperty.boolValue = EditorGUILayout.Toggle(_RuntimeOnlyGUI, runtimeOnlyProperty.boolValue);
            return oldValue != runtimeOnlyProperty.boolValue;
        }

        #endregion Texture Generation Settings

        #region Input draw

        /// <summary>
        /// Draws substance file inputs.
        /// </summary>
        /// <returns>True if any input value has changed.</returns>
        private bool DrawInputs()
        {
            bool changed = false;

            EditorGUILayout.Space();

            if (DrawGrouplessInputs(_inputGroupingHelper.GrouplessInputs))
                changed = true;

            EditorGUILayout.Space();

            foreach (var groupInfo in _inputGroupingHelper.InputGroups)
            {
                if (DrawInputGroup(groupInfo))
                    changed = true;
                EditorGUILayout.Space();
            }

            return changed;
        }

        /// <summary>
        /// Draws the inputs that are not part of any input group.
        /// </summary>
        /// <param name="inputsInfo">Inputs info</param>
        /// <returns>True if any input has changed.</returns>
        private bool DrawGrouplessInputs(SubstanceInputGroupCachedInfo inputsInfo)
        {
            var indexArray = inputsInfo.Inputs;

            bool changed = false;

            for (int i = 0; i < indexArray.Count; i++)
            {
                var property = indexArray[i].InputProperty;
                var guiContent = indexArray[i].GUIContent;
                var index = indexArray[i].Index;

                if (_handler.IsInputVisible(_target.Index, index))
                {
                    if (SubstanceInputDrawer.DrawInput(property, guiContent, _handler, _target.Index, index))
                        changed = true;
                }
            }

            return changed;
        }

        /// <summary>
        /// Draws inputs from a input group.
        /// </summary>
        /// <param name="groupInfo"></param>
        /// <returns></returns>
        private bool DrawInputGroup(SubstanceInputGroupCachedInfo groupInfo)
        {
            var groupName = groupInfo.Name;
            var indexArray = groupInfo.Inputs;

            groupInfo.ShowGroup = EditorGUILayout.Foldout(groupInfo.ShowGroup, groupName);

            if (!groupInfo.ShowGroup)
                return false;

            bool changed = false;

            for (int i = 0; i < indexArray.Count; i++)
            {
                EditorGUI.indentLevel++;

                var property = indexArray[i].InputProperty;
                var guiContent = indexArray[i].GUIContent;
                var index = indexArray[i].Index;

                if (_handler.IsInputVisible(_target.Index, index))
                {
                    if (SubstanceInputDrawer.DrawInput(property, guiContent, _handler, _target.Index, index))
                        changed = true;
                }

                EditorGUI.indentLevel--;
            }

            return changed;
        }

        #endregion Input draw

        #region Output draw

        private static readonly GUIContent _GeneratedTextureGUI = new GUIContent();

        private bool DrawGeneratedTextures(SerializedProperty outputList, bool generateAllTextures)
        {
            bool valueChanged = false;
            EditorGUILayout.Space(4);

            using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(_textureOutputScrollView, false, false))
            {
                scrollViewScope.handleScrollWheel = false;
                _textureOutputScrollView = scrollViewScope.scrollPosition;

                EditorGUILayout.BeginHorizontal();
                {
                    var outputsCount = outputList.arraySize;

                    for (int i = 0; i < outputsCount; i++)
                    {
                        var outputProperty = outputList.GetArrayElementAtIndex(i);
                        var isStandard = outputProperty.FindPropertyRelative("IsStandardOutput").boolValue;
                        var outputTexture = _target.Output[i];

                        if (generateAllTextures || isStandard)
                            valueChanged |= DrawOutputTexture(outputProperty, _GeneratedTextureGUI, outputTexture);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            return valueChanged;
        }

        private bool DrawOutputTexture(SerializedProperty output, GUIContent content, SubstanceOutputTexture substanceOutput)
        {
            var valueChanged = false;

            EditorGUILayout.BeginVertical(GUILayout.Width(120));
            {
                var texture = output.FindPropertyRelative("OutputTexture").objectReferenceValue as Texture2D;
                var label = output.FindPropertyRelative("Description.Channel").stringValue;
                var sRGB = output.FindPropertyRelative("sRGB");
                var alpha = output.FindPropertyRelative("AlphaChannel");
                var inverAlpha = output.FindPropertyRelative("InvertAssignedAlpha");
                var isAlphaAssignable = output.FindPropertyRelative("IsAlphaAssignable").boolValue;

                //Draw texture preview.
                if (texture != null)
                {
                    if (texture != null)
                    {
                        content.text = null;

                        var thumbnail = EditorUtility.IsDirty(texture) ? AssetPreview.GetMiniThumbnail(texture) : AssetPreview.GetAssetPreview(texture);

                        if (thumbnail == null)
                            thumbnail = AssetPreview.GetAssetPreview(texture);

                        content.image = thumbnail;
                        content.tooltip = texture.name;

                        if (GUILayout.Button(content, //style,
                                         GUILayout.Width(70),
                                         GUILayout.Height(70)))
                        {
                            // Highlight object in project browser:
                            EditorGUIUtility.PingObject(texture);
                        }
                    }
                }

                GUILayout.Label(label);

                if (substanceOutput.IsBaseColor() || substanceOutput.IsDiffuse() || substanceOutput.IsEmissive())
                {
                    var oldsRGB = sRGB.boolValue;
                    var newsRGB = GUILayout.Toggle(oldsRGB, "sRGB");

                    if (newsRGB != oldsRGB)
                    {
                        sRGB.boolValue = newsRGB;
                        valueChanged = true;
                    }
                }

                //Draw alpha remapping.
                EditorGUILayout.BeginHorizontal(GUILayout.Width(80), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                {
                    if (isAlphaAssignable)
                    {
                        var option = _outputChannelsHelper.GetAlphaChannels(label);
                        var index = 0;

                        if (!string.IsNullOrEmpty(alpha.stringValue))
                            index = Array.IndexOf(option, alpha.stringValue);

                        EditorGUILayout.LabelField("A", GUILayout.Width(10));

                        var newIndex = EditorGUILayout.Popup(index, option, GUILayout.Width(70));

                        if (newIndex != index)
                        {
                            alpha.stringValue = newIndex != 0 ? option[newIndex] : string.Empty;
                            valueChanged = true;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                //Draw inver alpha.
                EditorGUILayout.BeginHorizontal(GUILayout.Width(80), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                {
                    if (!string.IsNullOrEmpty(alpha.stringValue))
                    {
                        var oldValue = inverAlpha.boolValue;
                        var newValue = GUILayout.Toggle(oldValue, "Invert alpha");

                        if (newValue != oldValue)
                        {
                            inverAlpha.boolValue = newValue;
                            valueChanged = true;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            return valueChanged;
        }

        #endregion Output draw

        #region Presets draw

        private static readonly GUIContent _PresetExportGUIContent = new GUIContent("Export Preset...", "Save Preset");
        private static readonly GUIContent _PresetImportGUIContent = new GUIContent("Import Preset...", "Fetch Preset");
        private static readonly GUIContent _PresetResetGUIContent = new GUIContent("Reset Preset to Default", "Restore input defaults");

        private void DrawPresentExport(SubstanceGraphSO graph)
        {
            int labelWidth = (int)EditorGUIUtility.labelWidth - 15;

            _showExportPresentationHandler = EditorGUILayout.Foldout(_showExportPresentationHandler, "Preset Handling", true);

            if (_showExportPresentationHandler)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(" ", GUILayout.Width(labelWidth)); // Used to position the next button

                    if (GUILayout.Button(_PresetExportGUIContent))
                        HandleExportPresets(graph);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(" ", GUILayout.Width(labelWidth)); // Used to position the next button

                    if (GUILayout.Button(_PresetImportGUIContent))
                        HandleImportPresets(graph);
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(6);

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(" ", GUILayout.Width(labelWidth)); // Used to position the next button

                    if (GUILayout.Button(_PresetResetGUIContent))
                        HandleResetPresets(graph);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void HandleExportPresets(SubstanceGraphSO graph)
        {
            string savePath = EditorUtility.SaveFilePanel("Save Preset as...", graph.AssetPath, graph.GetAssetFileName(), "sbsprs");

            if (savePath != "")
            {
                string savePreset = "<sbspresets count=\"1\" formatversion=\"1.1\">\n "; //formatting line needed by other integrations
                savePreset += SubstanceEditorEngine.instance.ExportGraphPresetXML(_target, graph.Index);
                savePreset += "</sbspresets>";
                File.WriteAllText(savePath, savePreset);
            }
        }

        private void HandleImportPresets(SubstanceGraphSO graph)
        {
            string loadPath = EditorUtility.OpenFilePanel("Select Preset", graph.AssetPath, "sbsprs");

            if (loadPath != "")
            {
                string presetFile = System.IO.File.ReadAllText(loadPath);

                int startIndex = presetFile.IndexOf("<sbspreset ");
                int endIndex = presetFile.IndexOf("sbspreset>") + 10;
                var presetXML = presetFile.Substring(startIndex, endIndex - startIndex);

                SubstanceEditorEngine.instance.LoadPresetsToGraph(_target, presetXML);
            }
        }

        private void HandleResetPresets(SubstanceGraphSO graph)
        {
            SubstanceEditorEngine.instance.LoadPresetsToGraph(_target, graph.DefaultPreset);
        }

        #endregion Presets draw

        #region Thumbnail preview

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            if (_target.HasThumbnail)
                return _target.GetThumbnailTexture();

            var icon = UnityPackageInfo.GetSubstanceIcon(width, height);

            if (icon != null)
            {
                Texture2D tex = new Texture2D(width, height);
                EditorUtility.CopySerialized(icon, tex);
                return tex;
            }

            return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        #endregion Thumbnail preview

        #endregion Draw

        #region Scene Drag

        public void OnSceneDrag(SceneView sceneView, int index)
        {
            Event evt = Event.current;

            if (evt.type == EventType.Repaint)
                return;

            var materialIndex = -1;
            var go = HandleUtility.PickGameObject(evt.mousePosition, out materialIndex);

            if (_target.OutputMaterial != null)
            {
                if (go && go.GetComponent<Renderer>())
                {
                    HandleRenderer(go.GetComponent<Renderer>(), materialIndex, _target.OutputMaterial, evt.type, evt.alt);

                    if (_target.IsRuntimeOnly)
                    {
                        var runtimeComponent = go.GetComponent<Adobe.Substance.Runtime.SubstanceRuntimeGraph>();

                        if (runtimeComponent == null)
                            runtimeComponent = go.AddComponent<Adobe.Substance.Runtime.SubstanceRuntimeGraph>();

                        runtimeComponent.AttachGraph(_target);
                    }
                }
            }
        }

        internal static void HandleRenderer(Renderer r, int materialIndex, Material dragMaterial, EventType eventType, bool alt)
        {
            var applyMaterial = false;
            switch (eventType)
            {
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    applyMaterial = true;
                    break;

                case EventType.DragPerform:
                    DragAndDrop.AcceptDrag();
                    applyMaterial = true;
                    break;
            }
            if (applyMaterial)
            {
                var materials = r.sharedMaterials;

                bool isValidMaterialIndex = (materialIndex >= 0 && materialIndex < r.sharedMaterials.Length);
                if (!alt && isValidMaterialIndex)
                {
                    materials[materialIndex] = dragMaterial;
                }
                else
                {
                    for (int q = 0; q < materials.Length; ++q)
                        materials[q] = dragMaterial;
                }

                r.sharedMaterials = materials;
            }
        }

        #endregion Scene Drag

        #region Utilities

        private void SaveTGAFiles()
        {
            if (_target == null)
                return;

            if (_target.IsRuntimeOnly)
                return;

            _target.OutputRemaped = true;
            _target.RenderTextures = true;
            EditorUtility.SetDirty(_target);
        }

        private Rect DrawHighlightBox(float width, float height, float xPadding)
        {
            float bx, by, bw, bh;

            bx = xPadding;
            by = GetPosition();
            bw = width - xPadding;
            bh = height;

            var boxRect = new Rect(bx, by, bw, bh);

            var backgroundStyle = new GUIStyle();
            backgroundStyle.normal.background = _backgroundImage;
            GUI.Box(boxRect, GUIContent.none, backgroundStyle);
            return boxRect;
        }

        private int GetPosition()
        {
            Rect rect = GUILayoutUtility.GetLastRect();

            if ((rect.x != 0) || (rect.y != 0))
                lastRect = rect;

            return (int)lastRect.y;
        }

        /// This is a workaround a bug in the Unity asset database for generating materials previews.
        /// It basically generated a previews image whenever a property changes in the material, but it is now considering changes in the
        /// textures assign to the material itself. By adding a random label we ensure that the asset preview image will be updated.
        private void UpdateGraphMaterialLabel()
        {
            if (_target == null)
                return;

            const string tagPrefix = "sb_";

            var material = _target.OutputMaterial;

            if (material != null)
            {
                var labels = AssetDatabase.GetLabels(material);
                var newLabels = labels.Where(a => !a.Contains(tagPrefix)).ToList();
                newLabels.Add($"{tagPrefix}{Guid.NewGuid().ToString("N")}");
                AssetDatabase.SetLabels(material, newLabels.ToArray());
            }
        }

        #endregion Utilities

        /// Work around Unity SerializedObjectNotCreatableException during script compilation.
        private bool IsSerializedObjectReady()
        {
            try
            {
                if (serializedObject.targetObject == null)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}