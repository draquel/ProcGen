using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public static class QuadTreeMeshGenerator
{
    public static readonly Queue<QuadTreeMeshThreadData<MeshData>> QuadTreemeshThreadQueue = new Queue<QuadTreeMeshThreadData<MeshData>>();
    
    // public static MeshData GenerateMeshData(QuadTree tree, NoiseSettings noiseSettings)
    // {
    //     MeshData meshData = new MeshData();
    //     foreach (QuadTreeNode leaf in tree.leaves)
    //     {
    //         int step = Mathf.FloorToInt(leaf.size.x / tree.settings.resolution);
    //
    //         for (int z = 0; z <= tree.settings.resolution; z++)
    //         {
    //             for (int x = 0; x <= tree.settings.resolution; x++)
    //             {
    //                 float vertX = leaf.center.x + (x * step - leaf.size.x * 0.5f);
    //                 float vertZ = leaf.center.z + (z * step - leaf.size.z * 0.5f);
    //                 float vertY =
    //                     Noise.Evaluate((new Vector3(vertX, noiseSettings.seed, vertZ) + noiseSettings.offset) / noiseSettings.scale,
    //                         noiseSettings) * tree.settings.heightMultiplier;
    //
    //                 meshData.AddVertex(new Vector3(vertX, vertY, vertZ));
    //
    //                 if (x == tree.settings.resolution || z == tree.settings.resolution) continue;
    //
    //                 int index = meshData.vertices.Count - 1;
    //                 int nextRowOffset = tree.settings.resolution + 1;
    //
    //                 meshData.AddTriangle(index, index + nextRowOffset, index + 1);
    //                 meshData.AddTriangle(index + 1, index + nextRowOffset, index + nextRowOffset + 1);
    //             }
    //         }
    //     }
    //
    //     return meshData;
    // }


    public static MeshData GenerateMeshData(QuadTree tree, NoiseSettings noiseSettings)
    {
        MeshData meshData = new MeshData();
        foreach (QuadTreeNode leaf in tree.leaves)
        {
            float y = Noise.Evaluate((new Vector3(leaf.center.x, noiseSettings.seed, leaf.center.z) + noiseSettings.offset) / noiseSettings.scale, noiseSettings) * tree.settings.heightMultiplier; 
            meshData.AddVertex(new Vector3(leaf.center.x,y,leaf.center.z));
            int zeroIndex = meshData.vertices.Count - 1;
            
            //corners
            float posX = leaf.center.x + leaf.size.x * 0.5f;
            float negX = leaf.center.x - leaf.size.x * 0.5f;
            float posZ = leaf.center.z + leaf.size.z * 0.5f;
            float negZ = leaf.center.z - leaf.size.z * 0.5f;
           
            float y1 = Noise.Evaluate((new Vector3(negX, noiseSettings.seed, posZ) + noiseSettings.offset) / noiseSettings.scale, noiseSettings) * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(negX, y1, posZ)); //1
            float y3 = Noise.Evaluate((new Vector3(posX, noiseSettings.seed, posZ) + noiseSettings.offset) / noiseSettings.scale, noiseSettings) * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(posX, y3, posZ)); //3
            float y5 = Noise.Evaluate((new Vector3(posX, noiseSettings.seed, negZ) + noiseSettings.offset) / noiseSettings.scale, noiseSettings) * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(posX, y5, negZ)); //5
            float y7 = Noise.Evaluate((new Vector3(negX, noiseSettings.seed, negZ) + noiseSettings.offset) / noiseSettings.scale, noiseSettings) * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(negX, y7, negZ)); //7
            int index = zeroIndex + 4;
            
            //edges & triangles
            if (!leaf.neighbors[Direction.North]) {
                float y2 = tree.settings.useInterpolation ? 
                    interpolate(y1, y3, .5f) : 
                    Noise.Evaluate((new Vector3(leaf.center.x, noiseSettings.seed, posZ) + noiseSettings.offset) / noiseSettings.scale, noiseSettings) * tree.settings.heightMultiplier;
                meshData.AddVertex(new Vector3(leaf.center.x, y2, posZ)); //2
                index++;
                meshData.AddTriangle(zeroIndex+1,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+2,zeroIndex);
            }
            else {
                meshData.AddTriangle(zeroIndex+1,zeroIndex+2,zeroIndex);
            }
            if (!leaf.neighbors[Direction.East]) {
                float y4 = tree.settings.useInterpolation ? 
                    interpolate(y3, y5, .5f) : 
                    Noise.Evaluate((new Vector3(posX, noiseSettings.seed, leaf.center.z) + noiseSettings.offset) / noiseSettings.scale, noiseSettings) * tree.settings.heightMultiplier;
                meshData.AddVertex(new Vector3(posX, y4, leaf.center.z)); //4
                index++;
                meshData.AddTriangle(zeroIndex+2,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+3,zeroIndex);
            }
            else {
                meshData.AddTriangle(zeroIndex+2,zeroIndex+3, zeroIndex);
            }
            if (!leaf.neighbors[Direction.South]) {
                float y6 = tree.settings.useInterpolation ? 
                    interpolate(y5, y7, .5f) : 
                    Noise.Evaluate((new Vector3(leaf.center.x, noiseSettings.seed, negZ) + noiseSettings.offset) / noiseSettings.scale, noiseSettings) * tree.settings.heightMultiplier;
                meshData.AddVertex(new Vector3(leaf.center.x, y6, negZ)); //6
                index++;
                meshData.AddTriangle(zeroIndex+3,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+4,zeroIndex);
            }else {
                meshData.AddTriangle(zeroIndex+3,zeroIndex+4, zeroIndex);
            }
            if (!leaf.neighbors[Direction.West]) {
                float y8 = tree.settings.useInterpolation ? 
                    interpolate(y7, y1, .5f) : 
                    Noise.Evaluate((new Vector3(negX, noiseSettings.seed, leaf.center.z) + noiseSettings.offset) / noiseSettings.scale, noiseSettings) * tree.settings.heightMultiplier;
                meshData.AddVertex(new Vector3(negX, y8, leaf.center.z)); //8
                index++;
                meshData.AddTriangle(zeroIndex+4,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+1,zeroIndex); 
            }
            else {
                meshData.AddTriangle(zeroIndex+4,zeroIndex+1,zeroIndex);
            }
        }

        return meshData;
    }

    public static void RequestQuadTreeMesh(QuadTree tree, NoiseSettings settings, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            QuadTreeMeshThread(tree, settings, callback);
        };
        new Thread(threadStart).Start();
    }

    public static void QuadTreeMeshThread(QuadTree tree, NoiseSettings settings, Action<MeshData> callback)
    {
        MeshData data = GenerateMeshData(tree, settings);
        lock (QuadTreemeshThreadQueue) {
            QuadTreemeshThreadQueue.Enqueue(new QuadTreeMeshThreadData<MeshData>(callback, data));
        }
    }

    public struct QuadTreeMeshThreadData<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public QuadTreeMeshThreadData(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

    public static void ProcessThreadQueue()
    {
        if (QuadTreemeshThreadQueue.Count > 0) {
            for (int i = 0; i < QuadTreemeshThreadQueue.Count; i++) {
                QuadTreeMeshThreadData<MeshData> threadData = QuadTreemeshThreadQueue.Dequeue();
                threadData.callback(threadData.parameter);
            }
        }
    }
    private static float interpolate(float p1, float p2, float fraction) { return p1 + (p2 - p1) * fraction; }
} 
