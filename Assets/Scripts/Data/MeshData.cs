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
        int vertIndex = vertices.Count;
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

    // public void CaclulateDisplacement(Texture2D texture, float size, float displacementMod)
    // {
    //     List<Vector3> displacedVerts = new List<Vector3>(); 
    //     for(int i = 0; i < vertices.Count; i++)
    //     {
    //         float localu = localizeDimension(vertices[i].x,size);
    //         float localv = localizeDimension(vertices[i].x,size);
    //         float h = texture.GetPixelBilinear(localu, localv).r * displacementMod;
    //         
    //         //Debug.Log("uv = ("+vertices[i].x+","+vertices[i].z+")  localUV = ("+localu+","+localv+") h="+h);
    //         
    //         displacedVerts.Add(new Vector3(vertices[i].x, h,vertices[i].z));
    //     }
    //
    //     vertices = displacedVerts;
    // }
    
    public int localizeDimension(float input, float size)
    {
        int res = (int)(input % size);
        if(input < 0){
            res += (int)size;
        } 
        return res;
    }
   
    public Mesh CreateMesh(bool recalculateNormals = true){
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        if(recalculateNormals) {  CalculateNormals(); }

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

public class MeshData2 {
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    public Vector3[] normals;

    public int vertIndex = 0;
    public int triIndex = 0;

    MeshData2(int size)
    {
        Set(size);
    }
    
    public void AddTriangle(Vector3 a, Vector3 b, Vector3 c) {
        AddVertex(a);
        AddVertex(b);
        AddVertex(c);
        AddTriangle(vertIndex-2,vertIndex-1,vertIndex);
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triIndex] = a;
        triangles[triIndex] = b;
        triangles[triIndex] = c;
        triIndex += 3;
    }
   
    public void AddVertex(Vector3 vert, bool computeUV = true)
    {
        vertices[vertIndex] = vert;
        if(computeUV) uvs[vertIndex] = new Vector2(vert.x, vert.z);
        vertIndex++;
    }
    public void AddVertex(Vector3 vert, Vector2 uv)
    {
        vertices[vertIndex] = vert;
        uvs[vertIndex] = uv;
        vertIndex++;
    }
   
    public Vector3[] CalculateNormals()
    {
        Vector3[] normals = new Vector3[vertices.Length];

        for (int i = 0; i < triangles.Length; i += 3) {
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
            normals[i] = normals[i].normalized;
        }

        return normals;
    }

    public int localizeDimension(float input, float size)
    {
        int res = (int)(input % size);
        if(input < 0 && res != 0){
            res += (int)size;
        } 
        return res;
    }
   
    public Mesh CreateMesh(bool recalculateNormals = true){
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        if(recalculateNormals) { mesh.normals = CalculateNormals(); }

        return mesh;
    }

    public void Set(int size)
    {
        vertices = new Vector3[size];
        triangles = new int[size];
        uvs = new Vector2[size];
        normals = new Vector3[size];
    }
    public void Clear()
    {
        Set(vertices.Length);
    }
}
