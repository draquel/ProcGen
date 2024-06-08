using UnityEngine;

[System.Serializable]
public class MarchingCubesSettings 
{
    public bool interpolation;
    [Range(0.001f,1f)]
    public float isoLevel = 0.5f;

    public bool showSides = false;
}
