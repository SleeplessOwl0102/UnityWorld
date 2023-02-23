using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RL.EditorCoreRuntime
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class ReferenceTypeSelectorAttribute : PropertyAttribute
    {
        public bool isAbstractBase { get; private set; }
        public bool isValidate { get; private set; }
        public Type baseType { get; private set; }
        public IEnumerable<Type> subTypes { get; private set; }
        string filterMethod;

        public ReferenceTypeSelectorAttribute(Type baseType)
        {
            this.baseType = baseType;
            subTypes = baseType.Assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t) && t != baseType && !t.IsAbstract && !t.IsGenericTypeDefinition);
            isAbstractBase = baseType.IsAbstract;
            isValidate = true;
        }

        public ReferenceTypeSelectorAttribute(Type baseType, string filterMethod)
        {
            this.baseType = baseType;
            this.filterMethod = filterMethod;
            isAbstractBase = baseType.IsAbstract;
        }

#if UNITY_EDITOR
        public void GetSubTypes(SerializedProperty property)
        {
            if (subTypes != null)
            {
                return;
            }

            if (string.IsNullOrEmpty(filterMethod))
            {
                isValidate = false;
                subTypes = Enumerable.Empty<Type>();
                return;
            }

            var obj = property.serializedObject.targetObject as object;
            var objType = obj.GetType();
            var filter = objType.GetMethod(filterMethod, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            if (filter != null && filter.ReturnParameter.ParameterType == typeof(IEnumerable<System.Type>) && filter.GetParameters().Length == 0)
            {
                isValidate = true;
                subTypes = filter.Invoke(null, null) as IEnumerable<System.Type>;
                subTypes = subTypes.Where(t => baseType.IsAssignableFrom(t) && t != baseType && !t.IsAbstract && !t.IsGenericType);
            }
            else
            {
                isValidate = false;
                subTypes = Enumerable.Empty<Type>();
            }

        }
#endif
    }
}
