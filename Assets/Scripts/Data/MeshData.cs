using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshData
 {
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector3> borderVertices = new List<Vector3>();
    public List<int> borderTriangles = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();
    public List<Vector3> normals = new List<Vector3>(); 
    
    public void AddTriangle(Vector3 a, Vector3 b, Vector3 c) {
        int triIndex = triangles.Count;
        AddVertex(a);
        AddVertex(b);
        AddVertex(c);
        triangles.Add(triIndex);
        triangles.Add(triIndex + 1);
        triangles.Add(triIndex + 2);
    }

    public void AddTriangle(int a, int b, int c, bool border = false)
    {
        if (border) {
            borderTriangles.Add(a);
            borderTriangles.Add(b);
            borderTriangles.Add(c);
        }
        else
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c); 
        }
    }
    
    public void AddVertex(Vector3 vert,bool border = false)
    {
        if (border) {
            borderVertices.Add(vert);
        } else {
            vertices.Add(vert);
            uvs.Add(new Vector2(0f, 0f));
        }
    }
   
    public Vector3[] CalculateNormals()
    {
        Vector3[] normals = new Vector3[vertices.Count];

        for (int i = 0; i < triangles.Count; i += 3) {
            int index1 = triangles[i];
            int index2 = triangles[i + 1];
            int index3 = triangles[i + 2];

            Vector3 side1 = vertices[index2] - vertices[index1];
            Vector3 side2 = vertices[index3] - vertices[index1];
            Vector3 normal = Vector3.Cross(side1, side2).normalized;

            normals[index1] += normal;
            normals[index2] += normal;
            normals[index3] += normal;
        }
        
        for (int i = 0; i < borderTriangles.Count; i += 3) {
            int index1 = borderTriangles[i];
            int index2 = borderTriangles[i + 1];
            int index3 = borderTriangles[i + 2];

            Vector3 side1 = vertices[index2] - vertices[index1];
            Vector3 side2 = vertices[index3] - vertices[index1];
            Vector3 normal = Vector3.Cross(side1, side2).normalized;

            normals[index1] += normal;
            normals[index2] += normal;
            normals[index3] += normal;
        }

        for (int i = 0; i < normals.Length; i++) {
            normals[i] = normals[i].normalized;
        }

        return normals;
    }
    
    public Mesh CreateMesh(bool recalculateNormals = false){
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        if(recalculateNormals) { mesh.normals = CalculateNormals(); }

        return mesh;
    }
    
    public void Clear()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        normals.Clear();
    }
 }
