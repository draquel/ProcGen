using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (NoiseMapRenderer))]
public class NoiseMapRendererEditor : Editor
{
    public override void OnInspectorGUI(){
        NoiseMapRenderer noiseMapRenderer = (NoiseMapRenderer)target;
        if(DrawDefaultInspector()){
             if(noiseMapRenderer.autoUpdate)
             {
                 if(noiseMapRenderer.renderMode == NoiseMapRenderer.RenderMode.Render2D) { noiseMapRenderer.DrawNoiseMap(); }
                 else if(noiseMapRenderer.renderMode == NoiseMapRenderer.RenderMode.MarchingCubes) { noiseMapRenderer.DrawDensityMap(); }
                 else if(noiseMapRenderer.renderMode == NoiseMapRenderer.RenderMode.QuadTree) { noiseMapRenderer.DrawQuads();}
             }
        }

        if(GUILayout.Button("Generate Map")){
            if(noiseMapRenderer.renderMode == NoiseMapRenderer.RenderMode.Render2D) { noiseMapRenderer.DrawNoiseMap(); }
            else if(noiseMapRenderer.renderMode == NoiseMapRenderer.RenderMode.MarchingCubes) { noiseMapRenderer.DrawDensityMap(); }
            else if(noiseMapRenderer.renderMode == NoiseMapRenderer.RenderMode.QuadTree) { noiseMapRenderer.DrawQuads();}
        }
    }
}
