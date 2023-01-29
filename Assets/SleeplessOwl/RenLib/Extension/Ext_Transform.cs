using UnityEngine;

public static partial class Ext_Transform
{
    #region Position

    public static void SetPosX(this Transform t, float x)
    {
        var pos = t.position;
        t.position = new Vector3(x, pos.y, pos.z);
    }
    public static void SetPosY(this Transform t, float y)
    {
        var pos = t.position;
        t.position = new Vector3(pos.x, y, pos.z);
    }
    public static void SetPosZ(this Transform t, float z)
    {
        var pos = t.position;
        t.position = new Vector3(pos.x, pos.y, z);
    }

    public static void SetLocalPosX(this Transform t, float x)
    {
        var pos = t.localPosition;
        t.localPosition = new Vector3(x, pos.y, pos.z);
    }
    public static void SetLocalPosY(this Transform t, float y)
    {
        var pos = t.localPosition;
        t.localPosition = new Vector3(pos.x, y, pos.z);
    }
    public static void SetLocalPosZ(this Transform t, float z)
    {
        var pos = t.localPosition;
        t.localPosition = new Vector3(pos.x, pos.y, z);
    }

    #endregion

    #region Rotation

    public static void SetLocalRotX(this Transform t, float x)
    {
        var rot = t.localRotation;
        t.localRotation = Quaternion.Euler(x, rot.y, rot.z);
    }
    public static void SetLocalRotY(this Transform t, float y)
    {
        var rot = t.localRotation;
        t.localRotation = Quaternion.Euler(rot.x, y, rot.z);
    }
    public static void SetLocalRotZ(this Transform t, float z)
    {
        var rot = t.localRotation;
        t.localRotation = Quaternion.Euler(rot.x, rot.y, z);
    }

    #endregion

    #region Scale

    public static void SetLocalScaleXY(this GameObject obj, float scale)
    {
        var z = obj.transform.localScale.z;
        obj.transform.localScale = new Vector3(scale, scale, z);
    }

    public static void SetLocalScaleX(this Transform t, float x)
    {
        var scale = t.localScale;
        t.localScale = new Vector3(x, scale.y, scale.z);
    }
    public static void SetLocalScaleY(this Transform t, float y)
    {
        var scale = t.localScale;
        t.localScale = new Vector3(scale.x, y, scale.z);
    }
    public static void SetLocalScaleZ(this Transform t, float z)
    {
        var scale = t.localScale;
        t.localScale = new Vector3(scale.x, scale.y, z);
    }

    #endregion
}