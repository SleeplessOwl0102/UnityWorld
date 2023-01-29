using UnityEngine;
using UnityEngine.Rendering;

public static class Util_URP
{

    public static void SetKeyWord(this Material mat, string keyWord, bool active)
    {
        if (active)
            mat.EnableKeyword(keyWord);
        else
            mat.DisableKeyword(keyWord);
    }

}