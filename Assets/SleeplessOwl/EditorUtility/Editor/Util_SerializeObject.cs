using System;
using UnityEditor;
using UnityEngine;


namespace SleeplessOwl.EditorUtil
{
    public static class Util_SerializeObject
    {
        public static SerializedObject CreateVirtualSerializeObject<TScriptableObject>()
            where TScriptableObject : UnityEngine.ScriptableObject
        {
            return new SerializedObject(ScriptableObject.CreateInstance<TScriptableObject>());
        }

        public static SerializedObject InitSerializeReference<TSerializableClass>(this SerializedObject serializeObject, string propertyPath)
        {
            var propp = serializeObject.FindProperty(propertyPath);
            var instance = Activator.CreateInstance<TSerializableClass>();

            propp.serializedObject.Update();
            propp.managedReferenceValue = instance;
            propp.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            return serializeObject;
        }
    }
}