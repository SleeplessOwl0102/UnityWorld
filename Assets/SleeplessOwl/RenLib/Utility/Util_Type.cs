using System;
using System.Collections.Generic;

public static class Util_Type
{
    /// <summary>
    /// Get first target attribute in target type
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="targetType"></param>
    /// <returns></returns>
    public static TAttribute GetAttributeInType<TAttribute>(Type targetType) 
        where TAttribute : Attribute
    {
        Attribute[] attrs = Attribute.GetCustomAttributes(targetType);
        foreach (var attr in attrs)
        {
            if (attr is TAttribute)
                return (TAttribute)attr;
        }
        return null;
    }

    /// <summary>
    /// Get all types that derived from some type
    /// </summary>
    /// <param name="basetype"></param>
    /// <returns></returns>
    public static List<Type> GetDerivedTypes_InAppDomain(Type basetype)
    {
        var result = new List<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();
            foreach (var curType in types)
            {
                if (curType.IsSubclassOf(basetype))
                    result.Add(curType);
            }
        }
        return result;
    }

    /// <summary>
    /// Check type whether having implemented this interface
    /// </summary>
    /// <param name="type"></param>
    /// <param name="interfaceType"></param>
    /// <returns></returns>
    public static bool IsImplementedInterface(this Type type, Type interfaceType)
    {
        Type[] intfs = type.GetInterfaces();
        for (int i = 0; i < intfs.Length; i++)
        {
            if (intfs[i] == interfaceType)
            {
                return true;
            }
        }
        return false;
    }

}
