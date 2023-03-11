using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateWheleMesh : MonoBehaviour
{
    public int segments = 64;
    public float radius = 1f;
    public  float width = 0.1f;

    private void OnValidate()
    {
        if(TryGetComponent(out MeshFilter meshFilter))
        {
            meshFilter.mesh = BuildMesh();
        }
    }

    public Mesh BuildMesh()
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();

        var angle = 0f;
        var angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            var x = Mathf.Cos(angle * Mathf.Deg2Rad);
            var y = Mathf.Sin(angle * Mathf.Deg2Rad);

            vertices.Add(new Vector3(x, y, 0) * radius);
            vertices.Add(new Vector3(x, y, 0) * (radius + width));

            normals.Add(new Vector3(x, y, 0));
            normals.Add(new Vector3(x, y, 0));

            uvs.Add(new Vector2(i/(float)segments, 0));
            uvs.Add(new Vector2(i/(float)segments, 1));

            angle += angleStep;
        }

        for (int i = 0; i < segments; i++)
        {
            var i0 = i * 2;
            var i1 = i0 + 1;
            var i2 = (i0 + 2) % (segments * 2);
            var i3 = (i1 + 2) % (segments * 2);

            triangles.Add(i0);
            triangles.Add(i1);
            triangles.Add(i2);

            triangles.Add(i1);
            triangles.Add(i3);
            triangles.Add(i2);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);

        return mesh;
    }

}
