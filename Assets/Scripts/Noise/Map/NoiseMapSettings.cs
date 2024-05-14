using UnityEngine;

[System.Serializable]
public class NoiseMapSettings
{
    public Vector3Int mapSize;
    public Vector3 position;
    public NoiseMapGenerator.DensityFunction densityFunction;
    [Min(10f)]
    public float sphereRadius = 20f;
    
    public Gradient gradient;
    public NoiseSettings noiseSettings;

    public MarchingCubesSettings marchingCubesSettings;

    public QuadTreeSettings quadTreeSettings;
}