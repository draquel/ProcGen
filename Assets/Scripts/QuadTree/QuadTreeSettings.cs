using UnityEngine;

[System.Serializable]
public class QuadTreeSettings
{
    [Header("Limits")]
    public int minSize;
    [Min(0)]
    public int maxDepth;
    [Min(1)]
    public float heightMultiplier = 25f;

    [Header("Level of Detail")]
    public Vector2 viewerPosition;
    public int distanceModifier = 5;
    public bool useInterpolation = false;
    
    [Header("Occlusion Culling")]
    public bool enableOcclusion = false;
    public Vector3 viewerForward;
    public Camera camera;
}
