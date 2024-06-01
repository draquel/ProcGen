
using UnityEngine;
using Color = UnityEngine.Color;

public static class NoiseMapTextureGenerator
{
    public static Texture2D GenerateTexture(Color[] colorMap,Vector2Int size){
        Texture2D texture = new Texture2D(size.x,size.y);
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }
    
    public static Texture2D GenerateNoiseMapTexture(NoiseMap noiseMap, Gradient gradient){
        return GenerateTexture(NoiseMapGenerator.GenerateColorMap(noiseMap,gradient),noiseMap.size);
    }

    public static Texture2D GenerateDensityMapTexture(DensityMap densityMap, Gradient gradient)
    {
        return GenerateTexture(NoiseMapGenerator.GenerateColorMap(densityMap, gradient),new Vector2Int(densityMap.size.x,densityMap.size.y));
    }

    public static Texture3D GenerateTexture3D(Color[] colorMap,Vector3Int size)
    {
        Texture3D texture = new Texture3D(size.x,size.y,size.z,TextureFormat.RGBA32,false);
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }
}
