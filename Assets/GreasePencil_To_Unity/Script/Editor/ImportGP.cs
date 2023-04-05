using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Collections.Generic;
 using System.Globalization;

public class ImportGP : EditorWindow
{
    public Object source;
    public float deltaTime = 1f;
    public bool loop;
    public Shader shader;

    [MenuItem("GameObject/Import Grease Pencil")]
    public static void ShowWindow()
    {
        GetWindow<ImportGP>(false, "Grease Pencil Importer", true);
    }

    void OnGUI()
    {
        var style = GUI.skin.GetStyle("label");
        style.fontSize = 14; // whatever you set
        style.contentOffset = new Vector2(10f, 0f);




        GUILayout.Label("Grease Pencil Object", style);
        source = EditorGUILayout.ObjectField(source, typeof(Object), true);


        GUILayout.Label("Create Animation");
        if (GUILayout.Button("Create Animation"))
        {
            string objPath = AssetDatabase.GetAssetPath(source);
            objPath = objPath.Substring(0, objPath.Length - 4);

            CreateAnimation(objPath);

        }

        GUILayout.Label("Create Corresponding Materials");
        shader = EditorGUILayout.ObjectField(shader, typeof(Shader), true) as Shader;

        if (GUILayout.Button("Create Materials"))
        {
            string objPath = AssetDatabase.GetAssetPath(source);
            objPath = objPath.Substring(0, objPath.Length - 4); ;

            CreateMaterials(objPath);

        }

    }

    void CreateAnimation(string pathID)
    {
        // get txt file (ID + "Keys")
        // get Data

        string path = pathID + "_Keys.txt";
        StreamReader reader = new StreamReader(path);
        string read = reader.ReadToEnd();
        reader.Close();

        string[] arr = read.Split('#');

        AnimationClip clip = new AnimationClip();

        // Set Loop
        if (loop)
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

        }

        int i = 0;
        // create array for layers
        foreach (string ar in arr)
        {
            Debug.Log("Ar  " + ar);

            i += 1;

            string[] ark = ar.Split('&');
            string layerName = ark[0];
            List<string> arkList = new List<string>(ark);
            // remove ark[0] from ark
            arkList.RemoveAt(0);

            int j = 0;
            //for each key frame : add keyframe visibility
            foreach (string str in arkList)
            {
                bool setKeyStart = true;
                bool setKeyEnd = true;

                AnimationCurve curve;
                Keyframe[] keys;

                int prevKeyTime = 0;
                int nextKeyTime = 0;                

 
                if (j > 0) {
                    string prevKey = arkList[j - 1];
                    bool res1 = int.TryParse(prevKey, out prevKeyTime);
                }
                else
                {
                    setKeyStart = false;
                }


                if (j < arkList.Count-1)
                {
                    string nextKey = arkList[j + 1];
                    bool res2 = int.TryParse(nextKey, out nextKeyTime);
                }
                else
                {
                    setKeyEnd = false;
                }


                keys = new Keyframe[3];
                int keyTime = 0;
                bool res = int.TryParse(str, out keyTime);
                    
                if (setKeyStart == true) {
                    keys[0] = new Keyframe((prevKeyTime) * deltaTime, 0f);
                }
                else
                {
                    keys[0] = new Keyframe((keyTime - 1) * deltaTime, 1f);
                }

                keys[1] = new Keyframe((keyTime) * deltaTime, 1f);

                if (setKeyEnd == true)
                {
                    keys[2] = new Keyframe((nextKeyTime) * deltaTime, 0f);
                }
                else
                {
                    keys[2] = new Keyframe((keyTime + 1) * deltaTime, 1f);
                }



                curve = new AnimationCurve(keys);

                // Calculate constant tangent for animation curve
                setTangent(curve);
                clip.SetCurve(layerName + "." + str, typeof(GameObject), "m_IsActive", curve);


                j += 1;

            }

        }

        AssetDatabase.CreateAsset(clip, pathID + ".anim");
        AssetDatabase.SaveAssets();


    }

    void setTangent(AnimationCurve curve)
    {
        for (int i = 0; i < curve.keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
        }

    }

    void CreateMaterials(string pathID)
    {

        string path = pathID + "_Materials.txt";
        // get txt file (ID + "Materials")
        StreamReader reader = new StreamReader(path);
        string read = reader.ReadToEnd();
        Debug.Log(read);
        reader.Close();

        string[] arr = read.Split('#');
        string textureName = "";

        // create material if they doesn't exist

        foreach (string ar in arr)
        {
            if (ar != "") {

                string[] arm = ar.Split('&');
                //Debug.Log( arm[1] + "__" + arm[2]);
                string materialName = arm[0];
                float materialColR = float.Parse(arm[1], CultureInfo.InvariantCulture);
                float materialColG = float.Parse(arm[2], CultureInfo.InvariantCulture);
                float materialColB = float.Parse(arm[3], CultureInfo.InvariantCulture);
                float materialColA = float.Parse(arm[4], CultureInfo.InvariantCulture);

                if (arm.Length > 5) {
                    textureName = arm[5];
                }
                bool materialExist = false;
                //check if it esist
                string[] materialsguid = AssetDatabase.FindAssets(materialName + ".mat" + " t:material");

                foreach (string materialguid in materialsguid)
                {

                    Debug.Log("Material  " + materialguid + "  already exists, use refresh to update it");
                    materialExist = true;

                }



                //Create Material if it doesnt
                if (materialExist == false)
                {
                    Material material = new Material(shader);

                    Color col = new Color(materialColR, materialColG, materialColB, materialColA);

                    material.SetColor("_Color", col);

                    if (arm.Length > 5)
                    {
                        textureName = textureName.Substring(0, textureName.Length - 4);
                        string[] texturesguid = AssetDatabase.FindAssets(textureName + " t:texture");
                        Debug.Log("textures " + texturesguid[0] + "__" + textureName);


                        if (texturesguid[0] != null) {

                            string texturePath = AssetDatabase.GUIDToAssetPath(texturesguid[0]);
                            Texture materialTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));

                            Debug.Log(materialTexture);
                            material.SetTexture("_MainTex", materialTexture);
                        }
                    }

                    AssetDatabase.CreateAsset(material, pathID + materialName + ".mat");
                }

            }
        }


    }


    void RefreshMaterials(string pathID)
    {

        Debug.Log("Refresh");

    }
}