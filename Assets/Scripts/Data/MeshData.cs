using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshData {
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();
    public List<Vector3> normals = new List<Vector3>();

    public void AddTriangle(Vector3 a, Vector3 b, Vector3 c) {
        AddVertex(a);
        AddVertex(b);
        AddVertex(c);
        int vertIndex = vertices.Count-1;
        AddTriangle(vertIndex-2,vertIndex-1,vertIndex);
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c); 
    }
   
    public void AddVertex(Vector3 vert)
    {
        vertices.Add(vert);
        uvs.Add(new Vector2(vert.x, vert.z));
    }
   
    public void CalculateNormals()
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
        
        for (int i = 0; i < normals.Length; i++) {
            this.normals.Add(normals[i].normalized);
        }
    }
    
    public void AverageNormals()
    {
        if (normals.Count < vertices.Count) {
            CalculateNormals();
        }
        
        Dictionary<Vector3, Vector3> normalSumDict = new Dictionary<Vector3, Vector3>();
        Dictionary<Vector3, int> countDict = new Dictionary<Vector3, int>();

        for (int i = 0; i < vertices.Count(); i++) {
            Vector3 position = vertices[i];
            Vector3 normal = normals[i];

            if (normalSumDict.ContainsKey(position)) {
                normalSumDict[position] += normal;
                countDict[position]++;
            } else {
                normalSumDict[position] = normal;
                countDict[position] = 1;
            }
        }

        normals.Clear();
        for (int i = 0; i < vertices.Count(); i++) {
            normals.Add(normalSumDict[vertices[i]]);
        }
    }

    public Mesh CreateMesh(bool recalculateNormals = true){
        if(recalculateNormals) {  CalculateNormals(); }
        
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();

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
