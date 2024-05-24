using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (InfiniteTerrain))]
public class InfiniteTerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        InfiniteTerrain infiniteTerrain = (InfiniteTerrain)target;

        if ((DrawDefaultInspector() && infiniteTerrain.autoUpdate && !Application.isPlaying) || GUILayout.Button("Update"))
        {
            infiniteTerrain.setPlayerPos();
            infiniteTerrain.UpdateChunks();
        }

        if (GUILayout.Button("Clear"))
        {
            infiniteTerrain.ClearChunks();
        }
    }
}
