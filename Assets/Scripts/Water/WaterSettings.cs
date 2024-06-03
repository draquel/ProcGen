using UnityEngine;

[System.Serializable]
public class WaterSettings
{
   public int meshSize = 64;
   public int meshScale = 1;
   
   public Color baseColor = Color.blue;
   public Color rippleColor = Color.cyan;
   
   public float rippleSpeed = 3;
   public float rippleDensity = 5;
   public float rippleSlimness = 6;

   public float waveSpeed = 1;
   public float waveStrength = 0.5f;
   public float waveScale = 1;
   public float scale = 1;
   public Vector2 tiling = Vector2.one;

   public float transparency = 0.75f;

   public float foamOffset = 5;
}