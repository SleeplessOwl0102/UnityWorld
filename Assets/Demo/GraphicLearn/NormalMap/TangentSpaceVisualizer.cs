#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public class TangentSpaceVisualizer : MonoBehaviour
{
	public float offset = 0.01f;
	public float scale = 0.1f;

	void OnDrawGizmos()
	{
		MeshFilter filter = GetComponent<MeshFilter>();
		if (filter)
		{
			Mesh mesh = filter.sharedMesh;
			if (mesh)
			{
				ShowTangentSpace(mesh);
			}
		}
	}

	void ShowTangentSpace(Mesh mesh)
	{
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		Vector4[] tangents = mesh.tangents;
		for (int i = 0; i < vertices.Length; i++)
		{
			ShowTangentSpace(
				transform.TransformPoint(vertices[i]),
				transform.TransformDirection(normals[i]),
				transform.TransformDirection(tangents[i]),
				tangents[i].w
			);
		}
	}

	void ShowTangentSpace(Vector3 vertex, Vector3 normal, Vector3 tangent, float binormalSign)
	{

		vertex += normal * offset;
		Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
		Handles.color = Color.green;
		Handles.DrawLine(vertex, vertex + normal * scale);
		Handles.color = Color.red;
		Handles.DrawLine(vertex, vertex + tangent * scale);

		Vector3 binormal = Vector3.Cross(normal, tangent) * binormalSign;
		Handles.color = Color.blue;

		Handles.DrawLine(vertex, vertex + binormal * scale);
	}
}
#endif