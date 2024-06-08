using Unity.VisualScripting;
using UnityEngine;

public class NoiseMapRenderer : MonoBehaviour
{
    public enum RenderMode {Render2D, Rect, MarchingCubes, QuadTree}

    [Header("Render Settings")]
    public RenderMode renderMode;
    public GameObject target;
    public Material material;

    [Header("Generator Settings")]
    public NoiseMapSettings noiseMapSettings;
    public DensityMapSettings densityMapSettings;
    public QuadTreeSettings quadTreeSettings;
    public MarchingCubesSettings marchingCubesSettings;
    public bool autoUpdate;

    public GameObject player;
    private Vector3 lastPlayerPos;

    public void Start()
    {
        if (renderMode == RenderMode.QuadTree)
        {
            UpdatePlayerPos();
            DrawQuads(); 
        }
    }

    public void Update()
    {
        if (renderMode == RenderMode.QuadTree)
        {
            UpdatePlayerPos();
            if (Vector3.Distance(lastPlayerPos, quadTreeSettings.viewerPosition) >
                quadTreeSettings.minSize)
            {
                DrawQuads();
            }
        }
    }

    public void UpdatePlayerPos()
    {
        lastPlayerPos = player.transform.position;
        quadTreeSettings.viewerPosition = new Vector2(lastPlayerPos.x, lastPlayerPos.z);
    }

    public void DrawDensityMap()
    {
        DensityMap densityMap = NoiseMapGenerator.GenerateDensityMap(densityMapSettings);
        MeshData meshData = MarchingCubes.March(densityMap, marchingCubesSettings);

        BuildChunk(meshData);
    }
    public void DrawMesh()
    {
        int size = noiseMapSettings.mapSize.x;
        MeshData meshData = RectMeshGenerator.GenerateMeshData(noiseMapSettings.position, size, noiseMapSettings.mapSize.z, size - 1, size - 1,noiseMapSettings.noiseSettings,250);
        BuildChunk(meshData);
    }
    public void DrawQuads()
    {
        QuadTree tree = new QuadTree(noiseMapSettings.position,noiseMapSettings.mapSize,quadTreeSettings);
        tree.GenerateTree();
        MeshData meshData = QuadTreeMeshGenerator.GenerateMeshData(tree,noiseMapSettings.noiseSettings);

        BuildChunk(meshData);
    }

    public void DrawNoiseMap(){
        NoiseMap noiseMap = NoiseMapGenerator.GenerateNoiseMap(noiseMapSettings);
        Texture2D texture = NoiseMapTextureGenerator.GenerateNoiseMapTexture(noiseMap,noiseMapSettings.gradient);
        ApplyMaterial(material,target);
        ApplyTexture(texture,target);
    }

    public GameObject BuildChunk(MeshData meshData)
    {
        GameObject chunk;
        if (target.transform.childCount == 0) {
            chunk = Instantiate(new GameObject("chunk"), Vector3.zero, Quaternion.identity);
            chunk.AddComponent<MeshFilter>();
            chunk.AddComponent<MeshRenderer>(); 
            chunk.transform.SetParent(target.transform);
            if (Application.isPlaying) {
                chunk.GetComponent<Renderer>().material = material;
            }else {
                chunk.GetComponent<Renderer>().sharedMaterial = material;
            }
        }
        else {
            chunk = target.transform.GetChild(0).GameObject();
        }
        meshData.AverageNormals();
        chunk.GetComponent<MeshFilter>().mesh = meshData.CreateMesh(false);

        return chunk;
    } 

    private void ApplyMaterial(Material material, GameObject target)
    {
        if (Application.isPlaying) {
            target.GetComponent<Renderer>().material = material;
        } else {
            target.GetComponent<Renderer>().sharedMaterial = material;
        } 
    }
    
    private void ApplyTexture(Texture2D texture, GameObject target)
    {
        if (Application.isPlaying) {
            target.GetComponent<Renderer>().material.mainTexture = texture;
        } else {
            target.GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
        } 
    }
}