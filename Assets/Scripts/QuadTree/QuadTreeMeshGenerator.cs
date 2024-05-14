using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public static class QuadTreeMeshGenerator
{
    public static readonly Queue<QuadTreeMeshThreadData<MeshData>> QuadTreemeshThreadQueue = new Queue<QuadTreeMeshThreadData<MeshData>>();
    
    public static MeshData GenerateMeshData(QuadTree tree, NoiseSettings noiseSettings)
    {
        MeshData meshData = new MeshData();
        foreach (QuadTreeNode leaf in tree.leaves)
        {
            int step = Mathf.FloorToInt(leaf.size.x / tree.settings.resolution);

            for (int z = 0; z <= tree.settings.resolution; z++)
            {
                for (int x = 0; x <= tree.settings.resolution; x++)
                {
                    float vertX = leaf.center.x + (x * step - leaf.size.x * 0.5f);
                    float vertZ = leaf.center.z + (z * step - leaf.size.z * 0.5f);
                    float vertY =
                        Noise.Evaluate((new Vector3(vertX, noiseSettings.seed, vertZ) + noiseSettings.offset) / noiseSettings.scale,
                            noiseSettings) * tree.settings.heightMultiplier;

                    meshData.AddVertex(new Vector3(vertX, vertY, vertZ));

                    if (x == tree.settings.resolution || z == tree.settings.resolution) continue;

                    int index = meshData.vertices.Count - 1;
                    int nextRowOffset = tree.settings.resolution + 1;

                    meshData.AddTriangle(index, index + nextRowOffset, index + 1);
                    meshData.AddTriangle(index + 1, index + nextRowOffset, index + nextRowOffset + 1);
                }
            }
        }

        return meshData;
    }
    
    public static MeshData GenerateMeshData2(QuadTree tree, NoiseSettings noiseSettings)
    {
        MeshData meshData = new MeshData();
        foreach (QuadTreeNode leaf in tree.leaves)
        {
            List<int> boundaryVertices = new List<int>();
            int step = Mathf.FloorToInt(leaf.size.x / tree.settings.resolution);

            for (int z = 0; z <= tree.settings.resolution; z++)
            {
                for (int x = 0; x <= tree.settings.resolution; x++)
                {
                    bool isBoundery = x == 0 || x == tree.settings.resolution || z == 0 || z == tree.settings.resolution;
                    float vertX = leaf.center.x + (x * step - leaf.size.x * 0.5f);
                    float vertZ = leaf.center.z + (z * step - leaf.size.z * 0.5f);
                    float vertY =
                        Noise.Evaluate((new Vector3(vertX, noiseSettings.seed, vertZ) + noiseSettings.offset) / noiseSettings.scale,
                            noiseSettings) * tree.settings.heightMultiplier;

                    meshData.AddVertex(new Vector3(vertX, vertY, vertZ));
                    int index = meshData.vertices.Count - 1;

                    if (isBoundery) { boundaryVertices.Add(index); }
                    if (x == tree.settings.resolution || z == tree.settings.resolution) continue;

                    int nextRowOffset = tree.settings.resolution + 1;

                    meshData.AddTriangle(index, index + nextRowOffset, index + 1);
                    meshData.AddTriangle(index + 1, index + nextRowOffset, index + nextRowOffset + 1);
                }
            }
            for (int i = 0; i < boundaryVertices.Count - 1; i++)
            {
                meshData.AddTriangle(boundaryVertices[i], boundaryVertices[i + 1], boundaryVertices[boundaryVertices.Count - 1]);
            }
        }

        return meshData;
    }
    
    public static void RequestQuadTreeMesh(QuadTree tree, NoiseSettings settings, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            QuadTreeMeshThread(tree, settings, callback);
        };
        
        new Thread(threadStart).Start();
    }

    public static void QuadTreeMeshThread(QuadTree tree, NoiseSettings settings, Action<MeshData> callback)
    {
        MeshData data = GenerateMeshData(tree, settings);
        lock (QuadTreemeshThreadQueue)
        {
            QuadTreemeshThreadQueue.Enqueue(new QuadTreeMeshThreadData<MeshData>(callback, data));
        }
    }

    public struct QuadTreeMeshThreadData<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public QuadTreeMeshThreadData(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

    public static void ProcessThreadQueue()
    {
        if (QuadTreemeshThreadQueue.Count > 0)
        {
            for (int i = 0; i < QuadTreemeshThreadQueue.Count; i++)
            {
                QuadTreeMeshThreadData<MeshData> threadData = QuadTreemeshThreadQueue.Dequeue();
                threadData.callback(threadData.parameter);
            }
        }
    }
} 
