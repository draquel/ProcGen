
using System;
using UnityEngine;

public class CSTerrain : MonoBehaviour
{
    public int size = 1024;
    public Material material;
    public ComputeShader shader;
    private int kernalIndex = 0;
    public float heightMultiplier = 25;
    
    public Vector3 position = Vector3.zero;

    public RenderTexture texture;

    private QuadTree _quadTree;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    
    public NoiseSettings noiseSettings;
    public QuadTreeSettings quadTreeSettings;

    private void Start()
    {
        Setup();
    }

    private void FixedUpdate()
    {
        RunShader();
    }

    private void Setup()
    {
        texture = new RenderTexture(size, size, 0,RenderTextureFormat.ARGB32);
        texture.enableRandomWrite = true;
        texture.Create();
        
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        _quadTree = new QuadTree(position, new Vector3(size,quadTreeSettings.heightMultiplier,size), quadTreeSettings);
        _quadTree.GenerateTree();
        MeshData meshData = QuadTreeMeshGenerator.GenerateMeshData(_quadTree);
        _meshFilter.mesh = meshData.CreateMesh(true);
        
        kernalIndex = shader.FindKernel("Perlin2");
        shader.SetTexture(kernalIndex,"Result", texture); 
        
        RunShader();
        
        if (Application.isPlaying) {
            _meshRenderer.material = material;
        } else {
            _meshRenderer.sharedMaterial = material;
        }
    }

    private void RunShader()
    {
        SetShaderProps();
        
        int threadGroupsX = Mathf.CeilToInt(size / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(size / 8.0f); 
        shader.Dispatch(kernalIndex,threadGroupsX, threadGroupsY, 1);
       
        material.SetFloat("_heightModifier",heightMultiplier);
        material.SetTexture("_MainTex",texture);
    }

    private void SetShaderProps()
    {
        shader.SetVector("position", position);
        shader.SetVector("offset", noiseSettings.offset);
        shader.SetFloat("persistence",noiseSettings.persistence);
        shader.SetInt("octaves",noiseSettings.octaves);
        shader.SetFloat("lacunarity",noiseSettings.lacunarity);
        shader.SetFloat("scale",noiseSettings.scale);
    }
}
