using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SleeplessOwl.EditorUtil.UIElement
{
    public class PopUpAssetWindow : EditorWindow
    {
        private Object m_asset;
        private UnityEditor.Editor m_assetEditor;

        public static PopUpAssetWindow Create(Object asset, EditorWindow editorWindow)
        {
            var window = CreateWindow<PopUpAssetWindow>(typeof(PopUpAssetWindow));
            window.titleContent = new GUIContent() { 
                text = asset.name,
                tooltip = $"{asset.name} | {asset.GetType().Name}" 
            };

            window.Init(asset, UnityEditor.Editor.CreateEditor(asset));
            return window;
        }

        public void Init(Object asset, UnityEditor.Editor assetEditor)
        {
            m_asset = asset;
            m_assetEditor = assetEditor;

            var ve = assetEditor.CreateInspectorGUI();
            rootVisualElement.style.SetBorderWidthAll();

            if (ve != null)
            {
                rootVisualElement.Add(new IMGUIContainer(DrawHeader));
                rootVisualElement.Add(m_assetEditor.CreateInspectorGUI());
            }
            else
            {
                rootVisualElement.Add(new IMGUIContainer(DrawHeader));
                rootVisualElement.Add(new IMGUIContainer(DrawIMGUIContent));
            }
        }

        private void DrawHeader()
        {
            GUI.enabled = false;
            m_asset = EditorGUILayout.ObjectField("源文件", m_asset, m_asset.GetType(), false);
            GUI.enabled = true;
        }

        private void DrawIMGUIContent()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            m_assetEditor.OnInspectorGUI();
            EditorGUILayout.EndVertical();
        }
    }
}