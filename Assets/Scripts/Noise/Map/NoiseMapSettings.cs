using UnityEngine;

[System.Serializable]
public class NoiseMapSettings
{
    public Vector3Int mapSize;
    public Vector3 position;
    public Gradient gradient;
    public NoiseSettings noiseSettings;

    public NoiseMapSettings()
    {
        
    }
    
    public NoiseMapSettings(Vector3Int mapSize, Vector3 position, NoiseSettings noiseSettings)
    {
        this.mapSize = mapSize;
        this.position = position;
        this.noiseSettings = noiseSettings;
    }
}

[System.Serializable]
public class DensityMapSettings
{
    public Vector3Int mapSize;
    public Vector3 position;
    public NoiseMapGenerator.DensityFunction densityFunction;
    [Min(10f)]
    public float sphereRadius = 20f;
    
    public Gradient gradient;
    public NoiseSettings noiseSettings;
    
}