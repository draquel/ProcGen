using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainChunk : MonoBehaviour 
{
    public QuadTree quadTree;

    public Vector2 coord;
    public int size = 1024;
    public Vector3 position;
    public Bounds bounds;

    public TerrainChunkSettings settings;
    
    public Texture2D noise;
    public bool hasNoise = false;
    
    private bool meshRequested = false;
    private bool collisionMeshRequested = false;
    private bool noiseRequested = false;

    private InfiniteTerrain _terrain;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;
    public Material material;

    private List<Vector3> grassPoints = new List<Vector3>();
    public Mesh grassMesh;
    public Material grassMaterial;
    public bool hasGrass = false;
    private List<List<Matrix4x4>> Batches = new List<List<Matrix4x4>>();

    public void Update()
    {
        RenderBatches();
    }

    public void Init(Vector2 chunkCoord,TerrainChunkSettings chunkSettings)
    {
        
        _terrain = transform.parent.GetComponent<InfiniteTerrain>();
        settings = chunkSettings;
        coord = chunkCoord;
        size = this.settings.size;
        position = CoordToPos(chunkCoord, size);
        
        quadTree = new QuadTree(position,new Vector3(size,settings.heightMultiplier,size),settings.quadTreeSettings); 
        bounds = new Bounds(quadTree.center, quadTree.size);
        gameObject.name = "Chunk (" + coord.x + "," + coord.y + ")";

        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshCollider = gameObject.AddComponent<MeshCollider>();

        material = new Material(settings.material);
        ApplyMaterial();

        if(settings.enableWater)
            CreateWater();
        
        //GenerateGrassPoints(); 
    }

    public void UpdateChunk()
    {
        quadTree.GenerateTree();
        
        if(!hasNoise && !noiseRequested)
            GenerateNoise(settings.noiseSettings);

        if(hasNoise && !meshRequested)
            GenerateMesh(); 

        if (ViewerDistanceCheck()) {
            if(hasNoise && !collisionMeshRequested)
                GenerateMesh(true); 
        } else {
            _meshCollider.enabled = false;
        }
        
        setVisibility(true); 
    }
    // public void UpdateChunk(NoiseSettings noiseSettings)
    // {
    //     quadTree.GenerateTree();
    //     
    //     if(_terrain.useNoiseTexture && !hasNoise && !noiseRequested)
    //         GenerateNoise(noiseSettings);
    //
    //     if(!meshRequested)
    //         if(_terrain.useNoiseTexture && !noiseRequested) {
    //             GenerateMesh();
    //         } else {
    //             GenerateMesh(noiseSettings);
    //         }
    //
    //     if (ViewerDistanceCheck()) {
    //         if(!collisionMeshRequested)
    //             if (_terrain.useNoiseTexture && !noiseRequested) {
    //                 GenerateMesh(true);
    //             } else {
    //                 GenerateMesh(noiseSettings, true);
    //             }
    //     } else {
    //         _meshCollider.enabled = false;
    //     }
    //     
    //     setVisibility(true); 
    // } 
    
    private void RenderBatches()
    {
        foreach (var batch in Batches) {
            Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, batch);
        }
    }
    
        //-- Noise
    
    public void GenerateNoise(NoiseSettings noiseSettings)
    {
        // if (Application.isPlaying) {
        //     NoiseTextureGenerator.RequestNoiseTexture(position, new Vector3Int(size, 1, size), noiseSettings,ApplyNoise);
        //     noiseRequested = true;
        // } else {
        //     ApplyNoise(NoiseTextureGenerator.GenerateNoise(position, new Vector3Int(size, 1, size), noiseSettings));
        // }
        
        if (Application.isPlaying) {
            NoiseTextureGenerator.RequestNoiseTexture(position, new Vector3Int(size, 1, size), noiseSettings,ApplyNoise);
            noiseRequested = true;
        } else {
            ApplyNoise(NoiseTextureGenerator.GenerateNoise(position, new Vector3Int(size, 1, size), noiseSettings));
        } 
    }

    private void ApplyNoise(Color[] noise)
    {
        Texture2D texture = new Texture2D(size+4, size+4,TextureFormat.RGBAFloat,false,false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(noise);
        this.noise = texture;
        hasNoise = true;
        noiseRequested = false;
    }
    
        //-- Meshes
        
    public void GenerateMesh(NoiseSettings settings, bool collisionMesh = false)
    {
        Action<MeshData> callback;
        int depth;
        if (collisionMesh) {
            callback = ApplyCollisionMesh;
            depth = quadTree.GetDepth();
        } else {
            callback = ApplyMesh;
            depth = 0;
        }
        if (Application.isPlaying) {
            QuadTreeMeshGenerator.RequestQuadTreeMesh(quadTree,settings,callback,depth);
            if (collisionMesh) { collisionMeshRequested = true; } else { meshRequested = true; }
        } else {
            callback(QuadTreeMeshGenerator.GenerateMeshData(quadTree,settings,depth));
        }
    }
    
    public void GenerateMesh(bool collisionMesh = false)
    {
        Action<MeshData> callback;
        int depth;
        if (collisionMesh) {
            callback = ApplyCollisionMesh;
            depth = quadTree.GetDepth();
        } else {
            callback = ApplyMesh;
            depth = 0;
        }
    
        if (Application.isPlaying) {
            QuadTreeMeshGenerator.RequestQuadTreeMesh(quadTree,settings.noiseSettings,callback,depth);
            if (collisionMesh) { collisionMeshRequested = true; } else { meshRequested = true; }
        } else {
            callback(QuadTreeMeshGenerator.GenerateMeshData(quadTree,settings.noiseSettings,depth));
        }
    
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
 
        //-- Details
        
    public Water CreateWater()
    {
        GameObject waterGO = Instantiate(settings.waterPrefab,new Vector3(0,settings.waterLevel,0),Quaternion.identity,transform);
        Water water = waterGO.GetComponent<Water>();
        water.Init(position,settings.waterSettings);
        return water;
    }
    
    private void GenerateGrassPoints()
    {
        if (Application.isPlaying) {
            //PoissonDiscSampling.RequestPoissonPoints(2, Vector2.one * size, 30, ApplyGrassPoints);
            PoissonDiscSampling.RequestPoissonPoints(position,2,Vector2.one*size, settings.noiseSettings, ApplyGrassPoints,quadTree.settings.heightMultiplier);
        } else {
            //grassPoints = PoissonDiscSampling.GeneratePoints(2, Vector2.one * size);
            grassPoints = PoissonDiscSampling.GeneratePoints(position,2,Vector2.one*size, settings.noiseSettings, quadTree.settings.heightMultiplier);
        }
    }
    
    public void CreateGrass(NoiseSettings noiseSettings)
    {
        int addedMatricies = 0;
        Batches = new List<List<Matrix4x4>>(); 
        Batches.Add(new List<Matrix4x4>());
        foreach (Vector2 point in grassPoints) {
            Vector3 start = new Vector3(point.x, noiseSettings.seed, point.y);
            float minH = 0f * quadTree.settings.heightMultiplier;
            float maxH = 0.75f * quadTree.settings.heightMultiplier;


            Vector3 pos = new Vector3(point.x,0,point.y);
            //Debug.Log(pos);
                // Vector3 pos = position + start;
                // pos.y = Noise.Evaluate((pos + noiseSettings.offset) / noiseSettings.scale, noiseSettings)*quadTree.settings.heightMultiplier;
                if (pos.y > minH && pos.y < maxH && Vector3.Distance(_terrain.viewer.transform.position,pos) < quadTree.size.x*0.6f) {
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

        hasGrass = true;
    }
    
    private void ApplyGrassPoints(List<Vector3> obj)
    {
        grassPoints = obj;
        CreateGrass(settings.noiseSettings);
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

    public bool VisibilityCheck()
    {
        Plane[] planes = FrustrumUtility.ScalePlanes(GeometryUtility.CalculateFrustumPlanes(_terrain.mainCam),1.15f);
        planes = FrustrumUtility.MovePlanes(planes,quadTree.settings.minSize*quadTree.settings.minSize*quadTree.settings.distanceModifier,-quadTree.settings.viewerForward);
        return bounds.Contains(_terrain.viewer.position) || FrustrumUtility.IsPartiallyInFrustum(planes,bounds);
    }
    
    public bool ViewerDistanceCheck()
    {
        return Vector3.Distance(_terrain.viewer.position,quadTree.center) < quadTree.size.x;
    }

    public void OnDrawGizmos()
    {
        quadTree.DrawLeafBounds();
    }

    public void DrawChunkBorder()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center,bounds.size);
    }

    public void DrawPositionMarker()
    {
        Vector3 pos = position + (new Vector3(15f, 0, 15f));
        Vector3 size = new Vector3(30f, quadTree.settings.heightMultiplier, 30f);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(pos,size);
    }
}

public struct TerrainChunkMeshGenerator : IJobParallelFor
{

    private TerrainChunkMeshData data;


    public TerrainChunkMeshGenerator(TerrainChunkMeshData d)
    {
        data = d;
    }
    
    public void Execute(int index)
    {
        // if(depthFilter != 0 && leaf.depth < depthFilter){ continue; }
        //    
        //     //corners
        //     float posX = (leaf.center.x + leaf.size.x * 0.5f);
        //     float negX = (leaf.center.x - leaf.size.x * 0.5f);
        //     float posZ = (leaf.center.z + leaf.size.z * 0.5f);
        //     float negZ = (leaf.center.z - leaf.size.z * 0.5f);
        //
        //     int borderWidth = 2;
        //     //pixel index
        //     int zeroXpi = localizeDimension(leaf.center.x, tree.size.x)+borderWidth;
        //     int zeroZpi = localizeDimension(leaf.center.z, tree.size.z)+borderWidth;
        //     int posXpi = (posX == posCorner.x ? (int)tree.size.x : localizeDimension(posX, tree.size.x))+borderWidth;
        //     int negXpi = (negX == tree.position.x ? 0 : localizeDimension(negX, tree.size.x))+borderWidth;
        //     int posZpi = (posZ == posCorner.z ? (int)tree.size.x : localizeDimension(posZ, tree.size.z))+borderWidth;
        //     int negZpi = (negZ == tree.position.z ? 0 : localizeDimension(negZ, tree.size.z))+borderWidth;
        //     int rowSize = (int)tree.size.x + borderWidth*2;
        //
        //     float y = noise[(zeroZpi * rowSize + zeroXpi)].r * tree.settings.heightMultiplier;
        //     meshData.AddVertex(new Vector3(leaf.center.x,y,leaf.center.z));
        //     int zeroIndex = meshData.vertices.Count - 1;
        //    
        //     float y1 = noise[(posZpi * rowSize + negXpi)].r * tree.settings.heightMultiplier;
        //     meshData.AddVertex(new Vector3(negX, y1, posZ)); //1:
        //     float y3 = noise[posZpi * rowSize + posXpi].r * tree.settings.heightMultiplier;
        //     meshData.AddVertex(new Vector3(posX, y3, posZ)); //3
        //     float y5 = noise[negZpi * rowSize + posXpi].r * tree.settings.heightMultiplier;
        //     meshData.AddVertex(new Vector3(posX, y5, negZ)); //5
        //     float y7 = noise[negZpi * rowSize + negXpi].r * tree.settings.heightMultiplier;
        //     meshData.AddVertex(new Vector3(negX, y7, negZ)); //7
        //     int index = zeroIndex + 4;
        //     
        //     //edges & triangles
        //     if (!leaf.neighbors[Direction.North] || posZpi == tree.size.z+borderWidth) {
        //         float y2 = tree.settings.useInterpolation ? 
        //             interpolate(y1, y3, .5f) : 
        //             noise[(posZpi * rowSize + zeroXpi)].r * tree.settings.heightMultiplier;
        //         meshData.AddVertex(new Vector3(leaf.center.x, y2, posZ)); //2
        //         index++;
        //         meshData.AddTriangle(zeroIndex+1,index,zeroIndex);
        //         meshData.AddTriangle(index,zeroIndex+2,zeroIndex);
        //     }
        //     else {
        //         meshData.AddTriangle(zeroIndex+1,zeroIndex+2,zeroIndex);
        //     }
        //     if (!leaf.neighbors[Direction.East] || posXpi == tree.size.x+borderWidth) {
        //         float y4 = tree.settings.useInterpolation ? 
        //             interpolate(y3, y5, .5f) : 
        //             noise[(zeroZpi * rowSize + posXpi)].r * tree.settings.heightMultiplier;
        //         meshData.AddVertex(new Vector3(posX, y4, leaf.center.z)); //4
        //         index++;
        //         meshData.AddTriangle(zeroIndex+2,index,zeroIndex);
        //         meshData.AddTriangle(index,zeroIndex+3,zeroIndex);
        //     }
        //     else {
        //         meshData.AddTriangle(zeroIndex+2,zeroIndex+3, zeroIndex);
        //     }
        //     if (!leaf.neighbors[Direction.South] || negZpi == 0+borderWidth) {
        //         float y6 = tree.settings.useInterpolation ? 
        //             interpolate(y5, y7, .5f) : 
        //             noise[(negZpi * rowSize + zeroXpi)].r * tree.settings.heightMultiplier;
        //         meshData.AddVertex(new Vector3(leaf.center.x, y6, negZ)); //6
        //         index++;
        //         meshData.AddTriangle(zeroIndex+3,index,zeroIndex);
        //         meshData.AddTriangle(index,zeroIndex+4,zeroIndex);
        //     }else {
        //         meshData.AddTriangle(zeroIndex+3,zeroIndex+4, zeroIndex);
        //     }
        //     if (!leaf.neighbors[Direction.West] || negXpi == 0+borderWidth) {
        //         float y8 = tree.settings.useInterpolation ? 
        //             interpolate(y7, y1, .5f) : 
        //             noise[(zeroZpi * rowSize + negXpi)].r * tree.settings.heightMultiplier;
        //         meshData.AddVertex(new Vector3(negX, y8, leaf.center.z)); //8
        //         index++;
        //         meshData.AddTriangle(zeroIndex+4,index,zeroIndex);
        //         meshData.AddTriangle(index,zeroIndex+1,zeroIndex); 
        //     }
        //     else {
        //         meshData.AddTriangle(zeroIndex+4,zeroIndex+1,zeroIndex);
        //     }
        // }
    }
}

public struct TerrainChunkMeshData
{
    private Vector3 position;
    private Vector3 size;
    private NativeArray<QuadTreeLeaf> leaves;
    private NativeArray<Color> noise;
    private int depthFilter;
    private float heightMultiplier;
    
    public TerrainChunkMeshData(Vector3 position, Vector3 size, NativeArray<QuadTreeLeaf> leaves,NativeArray<Color> noise, int depthFilter, float heightMultiplier)
    {
        this.position = position;
        this.size = size;
        this.leaves = leaves;
        this.noise = noise;
        this.depthFilter = depthFilter;
        this.heightMultiplier = heightMultiplier;
    }
}

public struct TerrainChunkNoiseGenerator : IJobParallelFor
{
    public void Execute(int index)
    {
        throw new NotImplementedException();
    }
}