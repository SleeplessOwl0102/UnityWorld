using System;
using SleeplessOwl.EditorUtil.Mono;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SleeplessOwl.EditorUtil.Editor
{
    [CustomPropertyDrawer(typeof(AssetPathAttribute))]
    public class AssetPathDrawer : PropertyDrawer
    {
        // A helper warning label when the user puts the attribute above a non string type.
        private const string m_InvalidTypeLabel = "Attribute invalid for type ";

        private bool m_isCompatibleRepuireCompnent;
        private bool m_isHaveValidObject;
        private Object m_curSelectObj;
        private string m_preAssetPath;

        public AssetPathDrawer()
        {
            m_isCompatibleRepuireCompnent = false;
            m_isHaveValidObject = false;
            m_preAssetPath = string.Empty;
        }

        protected virtual Type SelectObjectType
        {
            get
            {
                AssetPathAttribute attribute = this.attribute as AssetPathAttribute;
                return attribute.Type;
            }
        }

        protected virtual Type RequireComponentType
        {
            get
            {
                AssetPathAttribute attribute = this.attribute as AssetPathAttribute;
                return attribute.ComponentType;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (m_isHaveValidObject == false || m_isCompatibleRepuireCompnent)
                return EditorGUIUtility.singleLineHeight;
            else
                return EditorGUIUtility.singleLineHeight * 3;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.hasMultipleDifferentValues)
            {
                EditorGUI.PropertyField(position, property);
                return;
            }

            //检查Field是否为string类型
            if (property.propertyType != SerializedPropertyType.String)
            {
                Rect labelPosition = position;
                labelPosition.width = EditorGUIUtility.labelWidth;
                GUI.Label(labelPosition, label);
                Rect contentPosition = position;
                contentPosition.x += labelPosition.width;
                contentPosition.width -= labelPosition.width;

                EditorGUI.HelpBox(contentPosition, m_InvalidTypeLabel + this.fieldInfo.FieldType.Name, MessageType.Error);

                return;
            }

            HandleObjectReference(position, property, label);
            ContextMenu(position, property);
        }

        private static Rect ContextMenu(Rect position, SerializedProperty property)
        {
            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 1 && position.Contains(e.mousePosition))
            {
                GenericMenu context = new GenericMenu();

                context.AddItem(new GUIContent("Copy Property Path"), property.isExpanded, () =>
                {
                    EditorGUIUtility.systemCopyBuffer = property.propertyPath;
                });

                context.AddItem(new GUIContent("Copy string path"), property.isExpanded, () =>
                {
                    EditorGUIUtility.systemCopyBuffer = property.stringValue;
                });

                context.AddItem(new GUIContent("Past string path"), property.isExpanded, () =>
                {
                    property.stringValue = EditorGUIUtility.systemCopyBuffer;
                    property.serializedObject.ApplyModifiedProperties();
                });

                context.ShowAsContext();
            }

            return position;
        }

        private void HandleObjectReference(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.UpdateIfRequiredOrScript();
            Type objectType = SelectObjectType;
            string pathValue = property.stringValue;


            if (string.IsNullOrEmpty(pathValue) || m_preAssetPath != pathValue)
            {
                m_curSelectObj = null;
            }

            if (m_curSelectObj == null && string.IsNullOrEmpty(pathValue) == false)
            {
                //开始更新asset cache
                if (pathValue.Contains("@"))
                {
                    //处理SubAsset的情况
                    //使用 @ 做为SubAsset的分隔符号
                    var fileName = pathValue.Substring(pathValue.IndexOf("@") + "@".Length);
                    var assetPath = pathValue.Substring(0, pathValue.IndexOf("@"));

                    var allObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                    foreach (var @object in allObjects)
                    {
                        if (@object.name == fileName)
                        {
                            m_curSelectObj = @object;
                            break;
                        }
                    }
                }
                else
                {
                    m_curSelectObj = AssetDatabase.LoadAssetAtPath(pathValue, objectType);
                }

                //处理有必要的Component的情况
                var requireCompType = RequireComponentType;
                if (requireCompType != null && m_curSelectObj != null && m_curSelectObj is GameObject go)
                {
                    var comp = go.GetComponent(requireCompType);
                    m_isCompatibleRepuireCompnent = comp != null;
                }
                else
                {
                    m_isCompatibleRepuireCompnent = true;
                }

                m_preAssetPath = property.stringValue;
            }


            if (m_curSelectObj != null)
                m_isHaveValidObject = true;
            else
                m_isHaveValidObject = false;

            EditorGUI.BeginChangeCheck();
            {
                m_curSelectObj = EditorGUI.ObjectField(position.SingleLine(), label, m_curSelectObj, objectType, false);
            }
            if (EditorGUI.EndChangeCheck())
            {
                OnSelectionMade(m_curSelectObj, property);
            }

            if (m_curSelectObj != null && m_isCompatibleRepuireCompnent == false)
            {
                EditorGUI.HelpBox(position.NextMultyLine(2), $"Require Component {RequireComponentType}", MessageType.Error);
            }
        }

        protected virtual void OnSelectionMade(Object newSelection, SerializedProperty property)
        {
            string assetPath = string.Empty;

            m_curSelectObj = null;
            if (newSelection != null)
            {
                if (AssetDatabase.IsSubAsset(newSelection))
                {
                    //使用 @ 做为SubAsset的分隔符号
                    assetPath = $"{AssetDatabase.GetAssetPath(newSelection)}@{newSelection.name}";
                }
                else
                {
                    assetPath = AssetDatabase.GetAssetPath(newSelection);
                }
                Debug.Log(assetPath);
            }

            property.stringValue = assetPath;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}