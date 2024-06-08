using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public class GPUInstanceBatchGenerator
    {

        // public List<List<Matrix4x4>> GenerateBatch(Vector2[] positions, NoiseSettings noiseSettings)
        // {
        //     int addedMatricies = 0;
        //     List<List<Matrix4x4>> Batches = new List<List<Matrix4x4>>(); 
        //     Batches.Add(new List<Matrix4x4>());
        //     foreach (Vector2 point in positions) {
        //         Vector3 start = new Vector3(point.x, noiseSettings.seed, point.y);
        //         float minH = 0f * quadTree.settings.heightMultiplier;
        //         float maxH = 0.75f * quadTree.settings.heightMultiplier;
        //
        //         Vector3 pos = start;
        //         pos.y = Noise.Evaluate((pos + noiseSettings.offset) / noiseSettings.scale, noiseSettings)*quadTree.settings.heightMultiplier;
        //         if (pos.y > minH && pos.y < maxH && Vector3.Distance(quadTree.settings.viewerPosition,pos) < quadTree.size.x*0.6f) {
        //             if (addedMatricies < 1000) {
        //                 Batches[Batches.Count - 1].Add(Matrix4x4.TRS(pos, Quaternion.AngleAxis(Random.Range(0,360), Vector3.up), Vector3.one)); 
        //                 addedMatricies += 1;
        //             } else {
        //                 Batches.Add(new List<Matrix4x4>());
        //                 Batches[Batches.Count - 1].Add(Matrix4x4.TRS(pos,  Quaternion.AngleAxis(Random.Range(0,360), Vector3.up), Vector3.one)); 
        //                 addedMatricies = 1;
        //             }
        //         }
        //     }
        // }
    }
}