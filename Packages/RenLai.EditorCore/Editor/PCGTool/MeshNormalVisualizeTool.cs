#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorTool("Show Mesh Normal", typeof(MeshFilter))]
public class MeshNormalVisualizeTool : EditorTool
{
    GUIContent _toolbarIcon;

    float offset = 0.01f;
    float scale = 0.1f;

    Vector3[] vertices;
    Vector3[] normals;
    Vector4[] tangents;

    bool showNormal = true;
    bool showTangent = true;
    bool showBinormal = true;

    public override GUIContent toolbarIcon
    {
        get
        {
            if (_toolbarIcon == null)
            {
                _toolbarIcon = EditorGUIUtility.IconContent("meshfilter icon.asset");
                _toolbarIcon.tooltip = "Vertex Visualization Tool";
            }
            return _toolbarIcon;
        }
    }

    public void OnEnable()
    {
        Selection.selectionChanged += RebuildVertexPositions;
    }

    public void OnDisable()
    {
        Selection.selectionChanged -= RebuildVertexPositions;
    }

    MeshFilter filter;

    void RebuildVertexPositions()
    {
        filter = Selection.activeGameObject?.GetComponent<MeshFilter>();

        if (filter == null)
            return;

        var mesh = filter.sharedMesh;
        vertices = mesh.vertices;
        normals = mesh.normals;
        tangents = mesh.tangents;
    }

    public override void OnToolGUI(EditorWindow window)
    {
        GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
        Handles.BeginGUI();

        GUILayout.BeginArea(new Rect(10, 10, 200, 130), "Draw Normal", GUI.skin.window);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Offset", GUILayout.Width(40));
        offset = EditorGUILayout.Slider(offset, 0.01f, 0.1f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Scale", GUILayout.Width(40));
        scale = EditorGUILayout.Slider(scale, 0.1f, 1f);
        GUILayout.EndHorizontal();

        showNormal = GUILayout.Toggle(showNormal, "Normal");
        showTangent = GUILayout.Toggle(showTangent, "Tangent");
        showBinormal = GUILayout.Toggle(showBinormal, "Binormal");

        GUILayout.EndArea();
        Handles.EndGUI();

        if (filter == null)
            return;

        for (int i = 0; i < vertices.Length; i++)
        {
            ShowTangentSpace(
                filter.transform.TransformPoint(vertices[i]),
                filter.transform.TransformDirection(normals[i]),
                filter.transform.TransformDirection(tangents[i]),
                tangents[i].w
            );
        }
    }

    void ShowTangentSpace(Vector3 vertex, Vector3 normal, Vector3 tangent, float binormalSign)
    {
        vertex += normal * offset;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

        if (showNormal)
        {
            Handles.color = Color.green;
            Handles.DrawLine(vertex, vertex + normal * scale);
        }
        if (showTangent)
        {
            Handles.color = Color.red;
            Handles.DrawLine(vertex, vertex + tangent * scale);
        }
        if (showBinormal)
        {
            Vector3 binormal = Vector3.Cross(normal, tangent) * binormalSign;
            Handles.color = Color.blue;
            Handles.DrawLine(vertex, vertex + binormal * scale);
        }
    }
}
#endif