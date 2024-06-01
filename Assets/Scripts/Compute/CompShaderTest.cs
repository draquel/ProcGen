using UnityEngine;

public class CompShaderTest : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture texture;
    

    public int resolution = 1024;
    private int kernal;
    public NoiseSettings noiseSettings;
    
    void Start()
    {

        //ApplyMaterial(material);
        // float[,] resFloatArray = new float[resolution, resolution];
        // ComputeBuffer resBuffer = new ComputeBuffer(resolution * resolution, sizeof(float) * 1);
        // resBuffer.SetData(resFloatArray);
        
        texture = new RenderTexture(resolution, resolution, 0,RenderTextureFormat.ARGB32);
        texture.enableRandomWrite = true;
        texture.Create();

        kernal = computeShader.FindKernel("Perlin2");
        computeShader.SetTexture(kernal,"Result", texture);
        //computeShader.SetBuffer(0,"resfloat",resBuffer);
        
        SetShaderProps();
        
        int threadGroupsX = Mathf.CeilToInt(resolution / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(resolution / 8.0f); 
        computeShader.Dispatch(kernal,threadGroupsX, threadGroupsY, 1);
        //resBuffer.GetData(resFloatArray);
        
        ApplyTexture(texture);
    }

    private void FixedUpdate()
    {
        SetShaderProps();
        int threadGroupsX = Mathf.CeilToInt(resolution / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(resolution / 8.0f); 
        computeShader.Dispatch(kernal,threadGroupsX, threadGroupsY, 1);
    }

    private void SetShaderProps()
    {
        // computeShader.SetFloat("height",texture.height);
        // computeShader.SetFloat("width",texture.width);
        // computeShader.SetFloat("depth",texture.width);
        computeShader.SetVector("position", transform.position);
        computeShader.SetVector("offset", noiseSettings.offset);
        computeShader.SetFloat("persistence",noiseSettings.persistence);
        computeShader.SetInt("octaves",noiseSettings.octaves);
        computeShader.SetFloat("lacunarity",noiseSettings.lacunarity);
        computeShader.SetFloat("scale",noiseSettings.scale); 
    }

    private void ApplyTexture(RenderTexture texture)
    {
        if (Application.isPlaying) {
            GetComponent<Renderer>().sharedMaterial.SetTexture("_Texture2D",texture);
        } else {
            GetComponent<Renderer>().sharedMaterial.SetTexture("_Texture2D", texture);
        } 
    } 
    
    private void ApplyMaterial(Material material)
    {
        if (Application.isPlaying) {
            GetComponent<Renderer>().material = material;
        } else {
            GetComponent<Renderer>().sharedMaterial = material;
        } 
    }
    
}



// [CustomEditor(typeof(CompShaderTest))]
// public class CompShaderTestEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         CompShaderTest compShader = (CompShaderTest)target;
//        
//         EditorGUILayout.BeginVertical();
//         EditorGUILayout.ObjectField("Compute Shader", compShader.computeShader, typeof(ComputeShader), true);
//         EditorGUILayout.ObjectField("Texture", compShader.texture, typeof(Texture), true);
//         EditorGUILayout.EndVertical();
//     }
// }
