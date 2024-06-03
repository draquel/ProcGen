using UnityEngine;

public class Water : MonoBehaviour
{
    public Vector3 position;
    public Material material;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    public int meshSize = 256;
    public int meshScale = 1;

    public WaterSettings settings;

    public void Init(Vector3 pos, WaterSettings settings)
    {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = new Material(material);
        
        position = pos;
        this.settings = settings;
        meshSize = this.settings.meshSize;
        meshScale = this.settings.meshScale;

        CreateMesh();
        SetShaderProps();
    }

    public void SetShaderProps()
    {
        material.SetFloat("_RippleSpeed",settings.rippleSpeed);
        material.SetFloat("_RippleDensity",settings.rippleDensity);
        material.SetFloat("_RippledSlimness",settings.rippleSlimness);
        material.SetFloat("_WaveSpeed",settings.waveSpeed);
        material.SetFloat("_WaveStrength",settings.waveStrength);
        material.SetFloat("_WaveScale",settings.waveScale);
        material.SetFloat("_Scale",settings.scale);
        material.SetFloat("_Transparency",settings.transparency);
        material.SetFloat("_FoamOffset",settings.foamOffset);
        
        material.SetVector("_Tiling",settings.tiling);
        material.SetColor("_BaseColor",settings.baseColor);
        material.SetColor("_RippleColor",settings.rippleColor);
    }

    public void CreateMesh()
    {
        if (Application.isPlaying) {
            RectMeshGenerator.RequestRectMesh(position, meshSize, meshSize, ApplyMesh, meshSize/meshScale, meshSize/meshScale);
        } else {
            ApplyMesh(RectMeshGenerator.GenerateMeshData(position, meshSize, meshSize, meshSize/meshScale, meshSize/meshScale));
        }
    }

    private void ApplyMesh(MeshData meshData)
    {
        _meshFilter.mesh = meshData.CreateMesh();
    }
}


