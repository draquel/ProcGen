using UnityEngine;

public class TerrainChunk : MonoBehaviour 
{
    public QuadTree quadTree;
    public MeshData meshData;

    public Vector2 coord;
    public Vector3 position;

    public bool collidable = false;
    public bool meshRequested = false;

    private InfiniteTerrain _terrain;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;
    
    public void Init(Vector2 chunkCoord, int size, QuadTreeSettings settings)
    {
        coord = chunkCoord;
        position = CoordToPos(chunkCoord, size);
        quadTree = new QuadTree(position,new Vector3(size,100,size),settings); 
    }
    
    public void Build(Transform parent, Material material)
    {
        gameObject.name = "Chunk (" + coord.x + "," + coord.y + ")";
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshCollider = gameObject.AddComponent<MeshCollider>();
        
        ApplyMaterial(material);

        gameObject.transform.SetParent(parent);
        gameObject.transform.localScale = Vector3.one;

        _terrain = parent.GetComponent<InfiniteTerrain>();
    }

    public void UpdateChunk(NoiseSettings noiseSettings)
    {
        if(meshRequested){ return; }
        quadTree.GenerateTree();

        if (Application.isPlaying) {
            QuadTreeMeshGenerator.RequestQuadTreeMesh(quadTree,noiseSettings,ApplyMesh);
            meshRequested = true; 
        } else {
            meshData = QuadTreeMeshGenerator.GenerateMeshData(quadTree,noiseSettings);
            _meshFilter.mesh = meshData.CreateMesh(true); 
        }
    }

    public void ApplyCollisionMesh(Vector3 viewerPos)
    {
        if (Vector3.Distance(viewerPos, quadTree.center) < quadTree.size.x * .8)
        {
            _meshCollider.sharedMesh = meshData.CreateMesh();
            _meshCollider.enabled = true;
            collidable = true;
        }else if (collidable)
        {
            _meshCollider.enabled = false;
            collidable = false;
        }
    }

    public void ApplyMesh(MeshData meshData)
    {
        meshRequested = false;
        this.meshData = meshData;
        _meshFilter.mesh = meshData.CreateMesh(true);
        
        ApplyCollisionMesh(_terrain.viewer.position);
    }

    public Vector3 CoordToPos(Vector2 coord, int size)
    {
        return new Vector3(coord.x * size, 0, coord.y * size);
    }
    
    private void ApplyMaterial(Material material)
    {
        if (Application.isPlaying) {
            _meshRenderer.material = material;
        } else {
            _meshRenderer.sharedMaterial = material;
        } 
    }

    public void setVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void OnDrawGizmos()
    {
        //Chunk Borders
        Vector3 gsize = quadTree.size;
        gsize.y *= quadTree.settings.heightMultiplier/2;

        Vector3 gcenter = quadTree.center;
        gcenter.y *= quadTree.settings.heightMultiplier/2; 
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(gcenter, gsize);
        
        //LODs
        quadTree.OnDrawGizmos();
    }
    
}