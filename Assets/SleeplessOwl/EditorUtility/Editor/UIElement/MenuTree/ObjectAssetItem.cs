using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SleeplessOwl.EditorUtil.UIElement
{
    /// <summary>
    /// Unity.Object类型的 MenuTreeItem 类别，右键点击时MenuTreeWindow会提供一些的基础unity资产操作功能
    /// </summary>
    public class ObjectAssetItem : MenuTreeItemBase
    {
        private string m_assetPath;
        private bool m_readOnly;
        private bool m_canDelete;

        private UnityEditor.Editor m_scriptableObjectEditor;
        private VisualElement m_visualElement;

        public ObjectAssetItem(string menuPath, string assetPath, bool readOnly = false) : base(menuPath)
        {
            m_assetPath = assetPath;
            m_readOnly = readOnly;
        }

        public Object GetObject => AssetDatabase.LoadAssetAtPath<ScriptableObject>(m_assetPath);

        public string GetObjectPath => m_assetPath;

        public override VisualElement GetVisualElement
        {
            get
            {
                if (m_visualElement == null)
                {
                    m_visualElement = new VisualElement();

                    if (m_scriptableObjectEditor == null)
                    {
                        var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(m_assetPath);
                        m_scriptableObjectEditor = UnityEditor.Editor.CreateEditor(so);
                    }

                    var ve = m_scriptableObjectEditor.CreateInspectorGUI();
                    bool isDrawByVisualElement = ve != null;
                    if (isDrawByVisualElement)
                    {
                        m_visualElement.Add(new IMGUIContainer(DrawAssetObjectField));
                        m_visualElement.Add(ve);
                    }
                    else
                    {
                        m_visualElement.Add(new IMGUIContainer(DrawAssetObjectField));
                        m_visualElement.Add(new IMGUIContainer(DrawIMGUIEditor));
                    }
                }

                return m_visualElement;
            }
        }

        private void DrawAssetObjectField()
        {
            EditorGUILayout.BeginHorizontal();
            if (GetObject != null)
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField("源文件", GetObject, typeof(ScriptableObject), false);
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawIMGUIEditor()
        {
            if (m_scriptableObjectEditor == null)
            {
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(m_assetPath);
                m_scriptableObjectEditor = UnityEditor.Editor.CreateEditor(so);
            }

            EditorGUI.BeginDisabledGroup(m_readOnly);
            m_scriptableObjectEditor.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
        }

        public override void OnDeselect()
        {
            m_visualElement?.Clear();
            m_visualElement = null;
            m_scriptableObjectEditor = null;
        }
    }
}