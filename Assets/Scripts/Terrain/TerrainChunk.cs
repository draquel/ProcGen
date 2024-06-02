using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainChunk : MonoBehaviour 
{
    public QuadTree quadTree;

    public Vector2 coord;
    public int size = 2048;
    public Vector3 position;
    public Bounds bounds;

    public bool meshRequested = false;
    public bool collisionMeshRequested = false;
    // public bool noiseRequested = false;
    // public bool hasNoise = false;
    public bool hasCollisionMesh = false;

    private InfiniteTerrain _terrain;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;
    public Material material;

    private ComputeShader noiseShader;
    private GameObject GrassPrefab;
    //private bool hasGrass = false;
    
    private NoiseMapSettings _noiseMapSettings;
    private NoiseMap _noiseMap;

    // public RenderTexture _texture;
    // public Texture2D noise;

    private List<Vector2> grassPoints = new List<Vector2>();
    public Mesh grassMesh;
    public Material grassMaterial;
    private List<List<Matrix4x4>> Batches = new List<List<Matrix4x4>>();

    public void Update()
    {
        RenderBatches();
    }

    public void Init(Vector2 chunkCoord, int chunkSize, Material chunkMaterial, QuadTreeSettings settings)
    {
        coord = chunkCoord;
        size = chunkSize;
        position = CoordToPos(chunkCoord, size);
        quadTree = new QuadTree(position,new Vector3(size,settings.heightMultiplier,size),settings); 
        bounds = new Bounds(quadTree.center, quadTree.size);
        gameObject.name = "Chunk (" + coord.x + "," + coord.y + ")";

        _terrain = gameObject.transform.parent.GetComponent<InfiniteTerrain>();
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshCollider = gameObject.AddComponent<MeshCollider>();

        grassPoints = PoissonDiscSampling.GeneratePoints(2, Vector2.one * size);
        CreateGrass();
        // _texture = new RenderTexture(size+10, size+10, 0,RenderTextureFormat.ARGBFloat);
        // _texture.enableRandomWrite = true;
        // _texture.wrapMode = TextureWrapMode.Repeat;
        // _texture.filterMode = FilterMode.Point;
        // _texture.Create();

        // noise = new Texture2D(size+10, size+10,TextureFormat.ARGB32,false);
        // noise.wrapMode = TextureWrapMode.Repeat;
        // noise.filterMode = FilterMode.Point;
        //
        // noiseShader = _terrain.noiseComputeShader;
        // int kernel = noiseShader.FindKernel("Perlin2");
        // noiseShader.SetTexture(kernel,"Result", _texture);
        
        material = new Material(chunkMaterial);
        ApplyMaterial();
    }

    // public void GenerateNoise(NoiseSettings noiseSettings)
    // {
    //     int kernel = noiseShader.FindKernel("Perlin2");
    //     noiseShader.SetVector("position", position);
    //     noiseShader.SetVector("offset", noiseSettings.offset);
    //     noiseShader.SetFloat("persistence",noiseSettings.persistence);
    //     noiseShader.SetInt("octaves",noiseSettings.octaves);
    //     noiseShader.SetFloat("lacunarity",noiseSettings.lacunarity);
    //     noiseShader.SetFloat("scale",noiseSettings.scale);
    //     
    //     int threadGroupsX = Mathf.CeilToInt(_texture.width / 8.0f);
    //     int threadGroupsY = Mathf.CeilToInt(_texture.height / 8.0f); 
    //     noiseShader.Dispatch(kernel,threadGroupsX, threadGroupsY, 1);
    //
    //     material.SetFloat("_chunkSize",size);
    //     material.SetVector("_chunkOffset",position);
    //     material.SetFloat("_heightModifier",quadTree.settings.heightMultiplier);
    //     material.SetTexture("_MainTex",_texture);
    //     
    //     noise = RenderTextureToTexture2D(_texture);
    //     hasNoise = true;
    // }
    
    Texture2D RenderTextureToTexture2D(RenderTexture rt)
    {
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = rt;

        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        RenderTexture.active = currentRT;

        return texture;
    }

    public void CreateWater()
    {
        
    }

    private void RenderBatches()
    {
        foreach (var batch in Batches)
        {
            Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, batch);
        }
    }
    
    public void CreateGrass()
    {
        // ClearGrass();
        int addedMatricies = 0;
        Batches = new List<List<Matrix4x4>>(); 
        Batches.Add(new List<Matrix4x4>());
        foreach (Vector2 point in grassPoints)
        {
            RaycastHit hit;
            Vector3 start = new Vector3(point.x, 500, point.y);
            float minH = 0.33f * quadTree.settings.heightMultiplier;
            float maxH = 0.66f * quadTree.settings.heightMultiplier;
 
            if (Physics.Raycast(position+start,transform.TransformDirection(Vector3.down),out hit,Mathf.Infinity))
            {
                Vector3 pos = position + start - new Vector3(0, hit.distance, 0);
                if (pos.y > minH && pos.y < maxH)
                {
                    if (addedMatricies < 1000)
                    {
                        Batches[Batches.Count - 1].Add(Matrix4x4.TRS(pos, Quaternion.AngleAxis(Random.Range(0,360), Vector3.up), Vector3.one)); 
                        addedMatricies += 1;
                    } else {
                        Batches.Add(new List<Matrix4x4>());
                        Batches[Batches.Count - 1].Add(Matrix4x4.TRS(pos,  Quaternion.AngleAxis(Random.Range(0,360), Vector3.up), Vector3.one)); 
                        addedMatricies = 1;
                    }
                    //Instantiate(_terrain.GrassPrefab,pos,Quaternion.identity,transform);
                }
            }
        }
    }

    public void ClearGrass()
    {
        var children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        if (Application.isPlaying) {
            children.ForEach(child => Destroy(child));
        } else {
            children.ForEach(child => DestroyImmediate(child)); 
        }
    }

    public void GenerateMesh(NoiseSettings settings)
    {
        quadTree.GenerateTree();
        if (Application.isPlaying) {
            QuadTreeMeshGenerator.RequestQuadTreeMesh(quadTree,settings,ApplyMesh);
            meshRequested = true; 
        } else {
            ApplyMesh(QuadTreeMeshGenerator.GenerateMeshData(quadTree,settings));
        }
    }
    
    // public void GenerateMesh()
    // {
    //     quadTree.GenerateTree();
    //     if (Application.isPlaying) {
    //         QuadTreeMeshGenerator.RequestQuadTreeMesh(quadTree,ApplyMesh);
    //         meshRequested = true; 
    //     } else {
    //         ApplyMesh(QuadTreeMeshGenerator.GenerateMeshData(quadTree));
    //     }
    //
    // }
    
    public void UpdateChunk(ComputeShader shader, NoiseSettings noiseSettings)
    {
        if(meshRequested){ return; }
        GenerateMesh(noiseSettings);
        
        //if (!hasNoise || !_terrain.enableCaching) { GenerateNoise(noiseSettings); }
        setVisibility(true); 
    
        if (ViewerDistanceCheck()) {
            GenerateCollisionMesh(noiseSettings);
        } else {
            _meshCollider.enabled = false;
        }

        if (hasCollisionMesh) {
            CreateGrass();
        }
    }

    public void GenerateCollisionMesh(NoiseSettings noiseSettings)
    {
        // int resolution = size / (quadTree.settings.minSize/2);
        // _meshCollider.sharedMesh = RectMeshGenerator.GenerateMeshData(noise,position,size,size,resolution,resolution,noiseSettings,quadTree.settings.heightMultiplier).CreateMesh();
        //_meshCollider.sharedMesh = QuadTreeMeshGenerator.GenerateMeshData(quadTree, noise).CreateMesh();
        //_meshCollider.sharedMesh = _meshFilter.mesh;

        if (Application.isPlaying) {
            QuadTreeMeshGenerator.RequestQuadTreeMesh(quadTree,noiseSettings,ApplyCollisionMesh,quadTree.GetDepth());
        } else {
            ApplyCollisionMesh(QuadTreeMeshGenerator.GenerateMeshData(quadTree,noiseSettings,quadTree.GetDepth()));
        }
    }

    public bool ViewerDistanceCheck()
    {
        return Vector3.Distance(_terrain.viewer.position,quadTree.center) < quadTree.size.x * 0.9f;
    }

    public void ApplyMesh(MeshData meshData)
    {
        meshData.AverageNormals();
        _meshFilter.mesh = meshData.CreateMesh(false);
        meshRequested = false;
    }
    
    public void ApplyCollisionMesh(MeshData meshData)
    {
        _meshCollider.sharedMesh = meshData.CreateMesh(true);
        _meshCollider.enabled = true;
        collisionMeshRequested = false;
        hasCollisionMesh = true;
    }

    private void ApplyMaterial()
    {
        if (Application.isPlaying) {
            _meshRenderer.material = material;
        } else {
            _meshRenderer.sharedMaterial = material;
        } 
    }
    
    public Vector3 CoordToPos(Vector2 coord, int size)
    {
        return new Vector3(coord.x * size, 0, coord.y * size);
    }

    public void setVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public bool isVisible()
    {
        Plane[] planes = FrustrumUtility.ScalePlanes(GeometryUtility.CalculateFrustumPlanes(_terrain.mainCam),1.15f);
        planes = FrustrumUtility.MovePlanes(planes,quadTree.settings.minSize*quadTree.settings.minSize*quadTree.settings.distanceModifier,-quadTree.settings.viewerForward);
        return bounds.Contains(_terrain.viewer.position) || FrustrumUtility.IsPartiallyInFrustum(planes,bounds);
    }

    public void OnDrawGizmos()
    {
        //LODs
        quadTree.OnDrawGizmos();
    }

    public void DrawChunkBorder()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center,bounds.size);
    }

    public void DrawPositionMarker()
    {
        Vector3 pos = position + (new Vector3(15f, quadTree.settings.heightMultiplier/2, 15f));
        Vector3 size = new Vector3(30f, quadTree.settings.heightMultiplier, 30f);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(pos,size);
    }
    
    void OnDestroy()
    {
        //_texture.Release(); 
    }

    private void OnDisable()
    {
        
    }
}