using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Water))]
public class WaterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Water water = (Water)target;

        // if (GUILayout.Button("Create Mesh"))
        // {
        //     if(!water.isInit)
        //         water.Init();
        //     water.CreateMesh();
        // }
        //
        // if (GUILayout.Button("Rest"))
        // {
        //     water.Reset(); 
        // }

    }
}