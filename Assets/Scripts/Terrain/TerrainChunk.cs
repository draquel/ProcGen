using System;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainChunk 
{
    private QuadTree quadTree;
    public MeshData meshData;

    public Vector2 coord;
    public Vector3 position;

    public GameObject chunk;

    public bool visible;
    public bool meshRequested = false;
    
    public TerrainChunk(Vector2 chunkCoord, int size, QuadTreeSettings settings)
    {
        coord = chunkCoord;
        position = CoordToPos(chunkCoord, size);
        quadTree = new QuadTree(position,new Vector3(size,100,size),settings);
    }

    public void Build(Transform parent, Material material)
    {
        chunk = new GameObject("Chunk ("+coord.x+","+coord.y+")");
        chunk.AddComponent<MeshFilter>();
        chunk.AddComponent<MeshRenderer>();
        chunk.AddComponent<MeshCollider>();
        
        ApplyMaterial(material);

        chunk.transform.SetParent(parent);
        chunk.transform.localScale = Vector3.one; 
    }

    public void UpdateChunk(NoiseSettings noiseSettings)
    {
        if(meshRequested){ return; }
        quadTree.GenerateTree();

        if (Application.isPlaying) {
            QuadTreeMeshGenerator.RequestQuadTreeMesh(quadTree,noiseSettings,AppyMesh);
            meshRequested = true; 
        } else {
            meshData = QuadTreeMeshGenerator.GenerateMeshData(quadTree,noiseSettings);
            chunk.GetComponent<MeshFilter>().mesh = meshData.CreateMesh(true); 
        }
    }

    public void AppyMesh(MeshData meshData)
    {
        meshRequested = false;
        this.meshData = meshData;
        chunk.GetComponent<MeshFilter>().mesh = meshData.CreateMesh(true);
    }

    public Vector3 CoordToPos(Vector2 coord, int size)
    {
        return new Vector3(coord.x * size, 0, coord.y * size);
    }
    
    private void ApplyMaterial(Material material)
    {
        if (Application.isPlaying) {
            chunk.GetComponent<Renderer>().material = material;
        } else {
            chunk.GetComponent<Renderer>().sharedMaterial = material;
        } 
    }

    public void setVisibility(bool visible)
    {
        chunk.SetActive(visible);
        this.visible = visible;
    }

    public void OnDrawGizmos()
    {
        //Chunk Borders
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(quadTree.center, quadTree.size);
        
        
    }
    
}