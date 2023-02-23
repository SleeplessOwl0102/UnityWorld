using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Assertions;

public static class SerializedPropertyUtil
{
    public static bool CompareSerializedPropertyValue(this SerializedProperty source1, SerializedProperty source2)
    {
        Assert.IsNotNull(source1);
        Assert.IsNotNull(source2);

        switch (source1.propertyType)
        {
            case SerializedPropertyType.String:
                if(string.IsNullOrEmpty(source1.stringValue))
                    return true;
                return source2.stringValue.Contains(source1.stringValue, System.StringComparison.OrdinalIgnoreCase);

            case SerializedPropertyType.Integer:
                return source1.intValue == source2.intValue;
            case SerializedPropertyType.Boolean:
                return source1.boolValue == source2.boolValue;
            case SerializedPropertyType.Float:
                return source1.floatValue == source2.floatValue;
            case SerializedPropertyType.Color:
                return source1.colorValue == source2.colorValue;
            case SerializedPropertyType.Enum:
                return source1.enumValueIndex == source2.enumValueIndex;
            case SerializedPropertyType.Vector2:
                return source1.vector2Value == source2.vector2Value;
            case SerializedPropertyType.Vector3:
                return source1.vector3Value == source2.vector3Value;
            case SerializedPropertyType.Vector4:
                return source1.vector4Value == source2.vector4Value;
            case SerializedPropertyType.Rect:
                return source1.rectValue == source2.rectValue;
            case SerializedPropertyType.ArraySize:
                return source1.arraySize == source2.arraySize;
            case SerializedPropertyType.Bounds:
                return source1.boundsValue == source2.boundsValue;
            case SerializedPropertyType.Gradient:
                return source1.gradientValue == source2.gradientValue;
            case SerializedPropertyType.Quaternion:
                return source1.quaternionValue == source2.quaternionValue;
            case SerializedPropertyType.Vector2Int:
                return source1.vector2IntValue == source2.vector2IntValue;
            case SerializedPropertyType.Vector3Int:
                return source1.vector3IntValue == source2.vector3IntValue;
            case SerializedPropertyType.Hash128:
                return source1.hash128Value == source2.hash128Value;


            case SerializedPropertyType.RectInt:
            case SerializedPropertyType.BoundsInt:
            case SerializedPropertyType.ManagedReference:
            case SerializedPropertyType.ObjectReference:
            case SerializedPropertyType.LayerMask:
            case SerializedPropertyType.Character:
            case SerializedPropertyType.AnimationCurve:
            case SerializedPropertyType.ExposedReference:
            case SerializedPropertyType.FixedBufferSize:
            case SerializedPropertyType.Generic:
            default:
                break;
        }

        return false;
    }

}
#endif