using UnityEngine;

[System.Serializable]
public class QuadTreeSettings
{
    [Header("Limits")]
    public float minSize;
    [Min(0)]
    public int maxDepth;
    [Min(1)]
    public float heightMultiplier = 25f;
    [Min(1)]
    public float scale;
    
    [Header("Level of Detail")]
    public int resolution = 4;
    public int distanceModifier = 5;
    public Vector2 viewerPosition;
}
