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

    private bool meshRequested = false;
    private bool collisionMeshRequested = false;
    // public bool noiseRequested = false;
    // public bool hasNoise = false;
    // public bool hasCollisionMesh = false;

    private InfiniteTerrain _terrain;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;
    public Material material;

    //private ComputeShader noiseShader;
    //private GameObject GrassPrefab;
    //private bool hasGrass = false;
    
    private NoiseMapSettings _noiseMapSettings;
    private NoiseMap _noiseMap;

    // private RenderTexture _texture;
    // public Texture2D noise;

    private List<Vector2> grassPoints = new List<Vector2>();
    public Mesh grassMesh;
    public Material grassMaterial;
    private List<List<Matrix4x4>> Batches = new List<List<Matrix4x4>>();

    public void Update()
    {
        RenderBatches();
    }

    public void Init(Vector2 chunkCoord)
    {
        _terrain = transform.parent.GetComponent<InfiniteTerrain>();
        
        coord = chunkCoord;
        size = _terrain.chunkSize;
        position = CoordToPos(chunkCoord, size);
        
        quadTree = new QuadTree(position,new Vector3(size,_terrain.quadTreeSettings.heightMultiplier*2,size),_terrain.quadTreeSettings); 
        bounds = new Bounds(quadTree.center, quadTree.size);
        gameObject.name = "Chunk (" + coord.x + "," + coord.y + ")";

        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshCollider = gameObject.AddComponent<MeshCollider>();

        // _texture = new RenderTexture(size+100, size+100, 0,RenderTextureFormat.ARGBFloat);
        // _texture.enableRandomWrite = true;
        // _texture.wrapMode = TextureWrapMode.Clamp;
        // _texture.filterMode = FilterMode.Bilinear;
        // _texture.Create();
        //
        // noise = new Texture2D(size+100, size+100,TextureFormat.ARGB32,false);
        // noise.wrapMode = TextureWrapMode.Clamp;
        // noise.filterMode = FilterMode.Bilinear;
        //
        // noiseShader = _terrain.noiseComputeShader;
        // int kernel = noiseShader.FindKernel("Perlin2");
        // noiseShader.SetTexture(kernel,"Result", _texture);
        
        material = new Material(_terrain.chunkMaterial);
        ApplyMaterial();

        CreateWater();
        
        // if (Application.isPlaying) {
        //     PoissonDiscSampling.RequestPoissonPoints(2, Vector2.one * size, 30, Callback);
        // } else {
        //     grassPoints = PoissonDiscSampling.GeneratePoints(2, Vector2.one * size);
        // }
    }

    private void Callback(List<Vector2> obj)
    {
        grassPoints = obj;
    }

    // public void GenerateNoise(NoiseSettings noiseSettings)
    // {
    //     int kernel = noiseShader.FindKernel("Perlin2");
    //     noiseShader.SetVector("position", new Vector3(position.x-50,position.y,position.z-50));
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
    
    // Texture2D RenderTextureToTexture2D(RenderTexture rt)
    // {
    //     Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
    //     texture.wrapMode = TextureWrapMode.Clamp;
    //     texture.filterMode = FilterMode.Bilinear;
    //
    //     RenderTexture currentRT = RenderTexture.active;
    //     RenderTexture.active = rt;
    //
    //     texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
    //     texture.Apply();
    //
    //     RenderTexture.active = currentRT;
    //
    //     return texture;
    // }

    public Water CreateWater()
    {
        GameObject waterGO = Instantiate(_terrain.WaterPrefab,new Vector3(0,_terrain.waterLevel,0),Quaternion.identity,transform);
        Water water = waterGO.GetComponent<Water>();
        water.Init(position,_terrain.waterSettings);
        return water;
    }

    private void RenderBatches()
    {
        foreach (var batch in Batches) {
            Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, batch);
        }
    }
    
    public void CreateGrass()
    {
        int addedMatricies = 0;
        Batches = new List<List<Matrix4x4>>(); 
        Batches.Add(new List<Matrix4x4>());
        foreach (Vector2 point in grassPoints) {
            RaycastHit hit;
            Vector3 start = new Vector3(point.x, 500, point.y);
            float minH = 0.25f * quadTree.settings.heightMultiplier;
            float maxH = 0.66f * quadTree.settings.heightMultiplier;
 
            if (Physics.Raycast(position+start,transform.TransformDirection(Vector3.down),out hit,Mathf.Infinity)) {
                Vector3 pos = position + start - new Vector3(0, hit.distance, 0);
                if (pos.y > minH && pos.y < maxH) {
                    if (addedMatricies < 1000) {
                        Batches[Batches.Count - 1].Add(Matrix4x4.TRS(pos, Quaternion.AngleAxis(Random.Range(0,360), Vector3.up), Vector3.one)); 
                        addedMatricies += 1;
                    } else {
                        Batches.Add(new List<Matrix4x4>());
                        Batches[Batches.Count - 1].Add(Matrix4x4.TRS(pos,  Quaternion.AngleAxis(Random.Range(0,360), Vector3.up), Vector3.one)); 
                        addedMatricies = 1;
                    }
                }
            }
        }
    }

    public void GenerateMesh(NoiseSettings settings, bool collisionMesh = false)
    {
        if (Application.isPlaying) {
            if (collisionMesh) {
                QuadTreeMeshGenerator.RequestQuadTreeMesh(quadTree,settings,ApplyCollisionMesh,quadTree.GetDepth());
                collisionMeshRequested = true;
            } else {
                QuadTreeMeshGenerator.RequestQuadTreeMesh(quadTree,settings,ApplyMesh);
                meshRequested = true;
            }
        } else {
            if (collisionMesh) {
                ApplyCollisionMesh(QuadTreeMeshGenerator.GenerateMeshData(quadTree, settings, quadTree.GetDepth()));
            } else {
                ApplyMesh(QuadTreeMeshGenerator.GenerateMeshData(quadTree,settings));
            }
        }
    }
    
    // public void GenerateMesh(bool collisionMesh = false)
    // {
    //     Action<MeshData> callback = ApplyMesh;
    //     int depth = 0;
    //     if (collisionMesh) {
    //         callback = ApplyCollisionMesh;
    //         depth = quadTree.GetDepth();
    //     }
    //
    //     if (Application.isPlaying) {
    //         QuadTreeMeshGenerator.RequestQuadTreeMesh(quadTree,noise,callback,depth);
    //         if (collisionMesh) {
    //             collisionMeshRequested = true; 
    //         } else {
    //             meshRequested = true;
    //         }
    //     } else {
    //         callback(QuadTreeMeshGenerator.GenerateMeshData(quadTree,noise,depth));
    //     }
    //
    // }
   
    public void UpdateChunk(NoiseSettings noiseSettings)
    {
        quadTree.GenerateTree();

        if(!meshRequested)
            GenerateMesh(noiseSettings);

        bool collidable = ViewerDistanceCheck();
        if (collidable) {
            if(!collisionMeshRequested)
                GenerateMesh(noiseSettings, true);
        } else {
            _meshCollider.enabled = false;
        }
        
        setVisibility(true); 

        // if (collidable) {
        //     CreateGrass();
        // }
    }
    
    public bool ViewerDistanceCheck()
    {
        return Vector3.Distance(_terrain.viewer.position,quadTree.center) < quadTree.size.x;
    }

    public void ApplyMesh(MeshData meshData)
    {
        meshData.AverageNormals();
        _meshFilter.mesh = meshData.CreateMesh(false);
        meshRequested = false;
    }
    
    public void ApplyCollisionMesh(MeshData meshData)
    {
        _meshCollider.sharedMesh = meshData.CreateMesh(false);
        _meshCollider.enabled = true;
        collisionMeshRequested = false;
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
        Vector3 pos = position + (new Vector3(15f, 0, 15f));
        Vector3 size = new Vector3(30f, quadTree.settings.heightMultiplier*2, 30f);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(pos,size);
    }
    
    // void OnDestroy()
    // {
    //     //_texture.Release(); 
    // }
    //
    // private void OnDisable()
    // {
    //     
    // }
}