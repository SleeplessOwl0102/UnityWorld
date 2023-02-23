//#undef UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SleeplessOwl.EditorUtil.Mono
{
    
    /// <summary>
    /// 可提供透过SerializeReference序列化的字段，选取序列化类别的GUI
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class ReferenceTypeSelectorAttribute : PropertyAttribute
    {
        public enum SelectTypeMethod
        { 
            BaseType,

            /// <summary>
            /// 除要属于特定类别外,该类型定义还必须被包含于当前序列化物件类别中
            /// </summary>
            BelongSerializeObjectNamespace,
        }

        public Type m_baseType { get; private set; }

        public SelectTypeMethod m_selectTypeMethod { get; private set; }

        public IEnumerable<Type> m_subTypes { get; private set; }

        public string m_filterMethodName { get; private set; }

        
        public ReferenceTypeSelectorAttribute(Type baseType, string filterMethod)
        {
            this.m_baseType = baseType;
            this.m_filterMethodName = filterMethod;
        }

        public ReferenceTypeSelectorAttribute(Type baseType, SelectTypeMethod filterType = SelectTypeMethod.BaseType)
        {
            m_baseType = baseType;
            m_subTypes = GetSubTypes(baseType);
            m_selectTypeMethod = filterType;
            this.m_filterMethodName = string.Empty;
        }

        private List<Type> GetSubTypes(Type baseType)
        {
            List<Type> subTypes = new List<Type>();

            //self assembly
            subTypes.AddRange(baseType.Assembly.GetTypes().Where(
                    t => baseType.IsAssignableFrom(t) && t != baseType && !t.IsAbstract && !t.IsGenericTypeDefinition));

            //dependency assembly
            foreach (var item in GetDependentAssemblies(baseType.Assembly))
            {
                subTypes.AddRange(item.GetTypes().Where(
                    t => baseType.IsAssignableFrom(t) && t != baseType && !t.IsAbstract && !t.IsGenericTypeDefinition));
            }

            return subTypes;
        }

        private IEnumerable<Assembly> GetDependentAssemblies(Assembly analyzedAssembly)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => GetNamesOfAssembliesReferencedBy(a)
                                    .Contains(analyzedAssembly.FullName));
        }

        public IEnumerable<string> GetNamesOfAssembliesReferencedBy(Assembly assembly)
        {
            return assembly.GetReferencedAssemblies()
                .Select(assemblyName => assemblyName.FullName);
        }


#if UNITY_EDITOR
        public void UpdateSubTypesWithSerializeObjectNamespace(SerializedProperty property)
        {
            if (m_subTypes != null)
            {
                if(m_selectTypeMethod == SelectTypeMethod.BelongSerializeObjectNamespace)
                {
                    var serializeObjectType = property.serializedObject.targetObject.GetType();
                    m_subTypes = m_subTypes.Where(x => { 
                        return x.FullName.Contains(serializeObjectType.FullName); 
                    });
                }
                return;
            }
        }

        public void UpdateSubTypesWithCustomMethod(SerializedProperty property)
        {
            if (string.IsNullOrEmpty(m_filterMethodName))
            {
                m_subTypes = Enumerable.Empty<Type>();
                return;
            }

            var obj = property.serializedObject.targetObject as object;
            var objType = obj.GetType();
            var filter = objType.GetMethod(m_filterMethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            if (filter != null &&
                filter.ReturnParameter.ParameterType == typeof(IEnumerable<System.Type>) &&
                filter.GetParameters().Length == 0)
            {
                m_subTypes = filter.Invoke(null, null) as IEnumerable<System.Type>;
                m_subTypes = m_subTypes.Where(t => m_baseType.IsAssignableFrom(t) && t != m_baseType && !t.IsAbstract && !t.IsGenericType);
            }
            else
            {
                m_subTypes = Enumerable.Empty<Type>();
            }
        }

#endif
    }

}
