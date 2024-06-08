
using UnityEngine;

[System.Serializable]
public class TerrainChunkSettings
{
   public int size;

   public int heightMultiplier;

   public Material material;

   public bool enableWater;
   public float waterLevel;
   public GameObject waterPrefab;

   public NoiseSettings noiseSettings;
   public QuadTreeSettings quadTreeSettings;
   public WaterSettings waterSettings;
   
}
