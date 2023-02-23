using System;
using SleeplessOwl.EditorUtil.Mono;
using UnityEditor;
using UnityEngine;

namespace SleeplessOwl.EditorUtil.Editor
{

    [CustomPropertyDrawer(typeof(ReferenceTypeSelectorAttribute))]
    public class ReferenceTypeSelectorDrawer : PropertyDrawer
    {
        public bool isInit;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUI.GetPropertyHeight(property, label, true);
            if (property.hasVisibleChildren && property.isExpanded)
            {
                var targetObject = property.managedReferenceValue;
                if (targetObject != null)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var typeSelector = attribute as ReferenceTypeSelectorAttribute;
            var targetObject = property.managedReferenceValue;

            if (isInit == false)
            {
                if (string.IsNullOrEmpty(typeSelector.m_filterMethodName) == false)
                {
                    typeSelector.UpdateSubTypesWithCustomMethod(property);
                }
                else if (typeSelector.m_selectTypeMethod == ReferenceTypeSelectorAttribute.SelectTypeMethod.BelongSerializeObjectNamespace)
                {
                    typeSelector.UpdateSubTypesWithSerializeObjectNamespace(property);
                }
                isInit = true;
            }

            if (targetObject != null)
            {
                label.text = property.name + $" ({targetObject.GetType().Name})";
                if (property.hasVisibleChildren == false)
                {
                    ShowSingleLine(position, property, label, typeSelector, targetObject);
                }
                else
                {
                    ShowFoldout(position, property, label, typeSelector, targetObject);
                }
            }
            else
            {
                label.text = property.managedReferenceValue == null ?
                    property.name + " (Null Value)" :
                    property.name;
                ShowSingleLine(position, property, label, typeSelector, targetObject);
            }
        }

        void ShowSingleLine(Rect position, SerializedProperty property, GUIContent label, ReferenceTypeSelectorAttribute typeSelector, object targetObject)
        {
            var buttonPosition = new Rect(position.x + EditorGUIUtility.labelWidth + 2, position.y, position.width - EditorGUIUtility.labelWidth - 2, position.height);

            EditorGUI.PropertyField(position, property, label, false);
            ShowInstanceMenu(buttonPosition, property, typeSelector, targetObject);
        }

        void ShowFoldout(Rect position, SerializedProperty property, GUIContent label, ReferenceTypeSelectorAttribute typeSelector, object targetObject)
        {
            var buttonPosition = new Rect(position.x + EditorGUIUtility.labelWidth + 2, position.y, position.width - EditorGUIUtility.labelWidth - 2, EditorGUIUtility.singleLineHeight);
            ShowInstanceMenu(buttonPosition, property, typeSelector, targetObject);
            EditorGUI.PropertyField(position, property, label, true);
        }

        void ShowInstanceMenu(Rect position, SerializedProperty property, ReferenceTypeSelectorAttribute typeSelector, object targetObject)
        {
            var baseType = typeSelector.m_baseType;
            var buttonText = (targetObject == null) ? "Select Type" : "Change Type";

            if (GUI.Button(position, buttonText))
            {
                var context = new GenericMenu();

                string cache = string.Empty;
                foreach (var subType in typeSelector.m_subTypes)
                {
                    if (subType.FullName.Replace(subType.Name, "") != cache)
                    {
                        context.AddSeparator(string.Empty);
                        context.AddDisabledItem(new GUIContent(subType.FullName.Replace(subType.Name, "")), false);
                    }
                    cache = subType.FullName.Replace(subType.Name, "");

                    context.AddItem(new GUIContent(subType.Name), false, () =>
                    {
                        property.managedReferenceValue = Activator.CreateInstance(subType);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
                context.AddSeparator("");
                context.AddItem(new GUIContent("Set null"), false, () =>
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                });

                if (typeSelector.m_baseType.IsAbstract)
                {
                    context.AddDisabledItem(new GUIContent(baseType.Name));
                }
                else
                {
                    context.AddItem(new GUIContent(baseType.Name), false, () =>
                    {
                        property.managedReferenceValue = Activator.CreateInstance(baseType);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }

                var pos = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, 0, 0);
                context.DropDown(pos);
            }
        }
    }
}