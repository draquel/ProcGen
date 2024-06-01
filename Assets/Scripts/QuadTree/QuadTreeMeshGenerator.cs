using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public static class QuadTreeMeshGenerator
{
    public static readonly Queue<QuadTreeMeshThreadData<MeshData>> QuadTreemeshThreadQueue = new Queue<QuadTreeMeshThreadData<MeshData>>();
   
    public static MeshData GenerateMeshData(QuadTree tree)
    {
        MeshData meshData = new MeshData();
        foreach (QuadTreeNode leaf in tree.leaves)
        {
            //corners
            float posX = (leaf.center.x + leaf.size.x * 0.5f);
            float negX = (leaf.center.x - leaf.size.x * 0.5f);
            float posZ = (leaf.center.z + leaf.size.z * 0.5f);
            float negZ = (leaf.center.z - leaf.size.z * 0.5f);
            
            float y = 0;
            meshData.AddVertex(new Vector3(leaf.center.x,y,leaf.center.z));
            int zeroIndex = meshData.vertices.Count - 1;
           
            float y1 = 0;
            meshData.AddVertex(new Vector3(negX, y1, posZ)); //1:
            float y3 = 0;
            meshData.AddVertex(new Vector3(posX, y3, posZ)); //3
            float y5 = 0;
            meshData.AddVertex(new Vector3(posX, y5, negZ)); //5
            float y7 = 0;
            meshData.AddVertex(new Vector3(negX, y7, negZ)); //7
            int index = zeroIndex + 4;
    
            
            
            //edges & triangles
            if (!leaf.neighbors[Direction.North]) {
                float y2 = 0;
                meshData.AddVertex(new Vector3(leaf.center.x, y2, posZ)); //2
                index++;
                meshData.AddTriangle(zeroIndex+1,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+2,zeroIndex);
            }
            else {
                meshData.AddTriangle(zeroIndex+1,zeroIndex+2,zeroIndex);
            }
            if (!leaf.neighbors[Direction.East]) {
                float y4 = 0;
                meshData.AddVertex(new Vector3(posX, y4, leaf.center.z)); //4
                index++;
                meshData.AddTriangle(zeroIndex+2,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+3,zeroIndex);
            }
            else {
                meshData.AddTriangle(zeroIndex+2,zeroIndex+3, zeroIndex);
            }
            if (!leaf.neighbors[Direction.South]) {
                float y6 = 0;
                meshData.AddVertex(new Vector3(leaf.center.x, y6, negZ)); //6
                index++;
                meshData.AddTriangle(zeroIndex+3,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+4,zeroIndex);
            }else {
                meshData.AddTriangle(zeroIndex+3,zeroIndex+4, zeroIndex);
            }
            if (!leaf.neighbors[Direction.West]) {
                float y8 = 0;
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
    
    public static MeshData GenerateMeshData(QuadTree tree, NoiseSettings noiseSettings,int depthFilter = 0)
    {
        MeshData meshData = new MeshData();
        foreach (QuadTreeNode leaf in tree.leaves)
        {
            if(depthFilter != 0 && leaf.depth < depthFilter){ continue; }
            
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

    public static MeshData GenerateMeshData(QuadTree tree, Texture2D texture)
    {
        MeshData meshData = new MeshData();
        foreach (QuadTreeNode leaf in tree.leaves)
        {
            //corners
            float posX = (leaf.center.x + leaf.size.x * 0.5f);
            float negX = (leaf.center.x - leaf.size.x * 0.5f);
            float posZ = (leaf.center.z + leaf.size.z * 0.5f);
            float negZ = (leaf.center.z - leaf.size.z * 0.5f);
    
            //pixel index
            int zeroXpi = meshData.localizeDimension(leaf.center.x, tree.size.x);
            int zeroZpi = meshData.localizeDimension(leaf.center.z, tree.size.z);
            int posXpi = meshData.localizeDimension(posX, tree.size.x);
            int negXpi = meshData.localizeDimension(negX, tree.size.x);
            int posZpi = meshData.localizeDimension(posZ, tree.size.z);
            int negZpi = meshData.localizeDimension(negZ, tree.size.z);
            
            float y = texture.GetPixel(zeroXpi,zeroZpi).r * tree.settings.heightMultiplier; 
            meshData.AddVertex(new Vector3(leaf.center.x,y,leaf.center.z));
            int zeroIndex = meshData.vertices.Count - 1;
           
            float y1 = texture.GetPixel(negXpi, posZpi).r * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(negX, y1, posZ)); //1:
            float y3 = texture.GetPixel(posXpi, posZpi).r * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(posX, y3, posZ)); //3
            float y5 = texture.GetPixel(posXpi, negZpi).r * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(posX, y5, negZ)); //5
            float y7 = texture.GetPixel(negXpi, negZpi).r * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(negX, y7, negZ)); //7
            int index = zeroIndex + 4;
            
            //edges & triangles
            if (!leaf.neighbors[Direction.North]) {
                float y2 = tree.settings.useInterpolation ? 
                    interpolate(y1, y3, .5f) : 
                    texture.GetPixel(zeroXpi, posZpi).r * tree.settings.heightMultiplier;
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
                    texture.GetPixel((int)posXpi, zeroZpi).r * tree.settings.heightMultiplier;
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
                    texture.GetPixel(zeroXpi, negZpi).r * tree.settings.heightMultiplier;
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
                    texture.GetPixel(negXpi, zeroZpi).r * tree.settings.heightMultiplier;
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
    
    

    public static void RequestQuadTreeMesh(QuadTree tree, NoiseSettings settings, Action<MeshData> callback, int depthFilter = 0)
    {
        ThreadStart threadStart = delegate {
            QuadTreeMeshThread(tree, settings, callback, depthFilter);
        };
        new Thread(threadStart).Start();
    }

    public static void QuadTreeMeshThread(QuadTree tree, NoiseSettings settings, Action<MeshData> callback, int depthFilter = 0)
    {
        MeshData data = GenerateMeshData(tree, settings, depthFilter);
        lock (QuadTreemeshThreadQueue) {
            QuadTreemeshThreadQueue.Enqueue(new QuadTreeMeshThreadData<MeshData>(callback, data));
        }
    }
    
    public static void RequestQuadTreeMesh(QuadTree tree, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            QuadTreeMeshThread(tree, callback);
        };
        new Thread(threadStart).Start();
    }

    public static void QuadTreeMeshThread(QuadTree tree, Action<MeshData> callback)
    {
        MeshData data = GenerateMeshData(tree);
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
