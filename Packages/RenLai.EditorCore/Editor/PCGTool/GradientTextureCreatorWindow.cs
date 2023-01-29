using UnityEngine;
using UnityEditor;
using System.IO;

namespace RenLai.ArtAssetTool
{
    /// <summary>
    /// an editor window for creating gradient texture asset.
    /// </summary>
    public class GradientTextureCreatorWindow : EditorWindow
    {
        [SerializeField] Gradient gradient;
        [SerializeField] int width = 128;
        [SerializeField] int height = 16;
        [SerializeField] string fileName = "Gradient";

        [MenuItem("PCG Tool/Gradient Texure Creator")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(GradientTextureCreatorWindow));
        }

        public static Texture2D CreateTexture(Gradient grad, int width = 32, int height = 1)
        {
            var gradTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            gradTex.filterMode = FilterMode.Bilinear;
            float inv = 1f / (width - 1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var t = x * inv;
                    Color col = grad.Evaluate(t);
                    gradTex.SetPixel(x, y, col);
                }
            }
            gradTex.Apply();
            return gradTex;
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            SerializedObject so = new SerializedObject(this);
            EditorGUILayout.PropertyField(so.FindProperty("gradient"), true, null);
            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("width", GUILayout.Width(80f));
                int.TryParse(GUILayout.TextField(width.ToString(), GUILayout.Width(120f)), out width);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("height", GUILayout.Width(80f));
                int.TryParse(GUILayout.TextField(height.ToString(), GUILayout.Width(120f)), out height);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("name", GUILayout.Width(80f));
                fileName = GUILayout.TextField(fileName, GUILayout.Width(120f));
                GUILayout.Label(".png");
            }

            if (GUILayout.Button("Save"))
            {
                string path = EditorUtility.SaveFolderPanel("Select an output path", "", "");
                if (path.Length > 0)
                {
                    var tex = CreateTexture(gradient, width, height);
                    byte[] pngData = tex.EncodeToPNG();
                    File.WriteAllBytes(path + "/" + fileName + ".png", pngData);
                    AssetDatabase.Refresh();
                }
            }
        }
    }

}