using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public static class QuadTreeMeshGenerator
{
    public static readonly Queue<QuadTreeMeshThreadData<MeshData>> QuadTreeMeshThreadQueue = new Queue<QuadTreeMeshThreadData<MeshData>>();
   
    public static MeshData GenerateMeshData(QuadTree tree, int depthFilter = 0)
    {
        MeshData meshData = new MeshData();
        foreach (QuadTreeNode leaf in tree.leaves) {
            if(depthFilter != 0 && leaf.depth < depthFilter){ continue; }
            
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
            if (!leaf.neighbors[Direction.North] && negX % tree.size.x != 0) {
                float y2 = 0;
                meshData.AddVertex(new Vector3(leaf.center.x, y2, posZ)); //2
                index++;
                meshData.AddTriangle(zeroIndex+1,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+2,zeroIndex);
            }
            else {
                meshData.AddTriangle(zeroIndex+1,zeroIndex+2,zeroIndex);
            }
            if (!leaf.neighbors[Direction.East] && negX % tree.size.x != 0) {
                float y4 = 0;
                meshData.AddVertex(new Vector3(posX, y4, leaf.center.z)); //4
                index++;
                meshData.AddTriangle(zeroIndex+2,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+3,zeroIndex);
            }
            else {
                meshData.AddTriangle(zeroIndex+2,zeroIndex+3, zeroIndex);
            }
            if (!leaf.neighbors[Direction.South] && negX % tree.size.x != 0) {
                float y6 = 0;
                meshData.AddVertex(new Vector3(leaf.center.x, y6, negZ)); //6
                index++;
                meshData.AddTriangle(zeroIndex+3,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+4,zeroIndex);
            }else {
                meshData.AddTriangle(zeroIndex+3,zeroIndex+4, zeroIndex);
            }
            if (!leaf.neighbors[Direction.West] && negX % tree.size.x != 0) {
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
        foreach (QuadTreeNode leaf in tree.leaves) {
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
            if (!leaf.neighbors[Direction.North] || posZ % tree.size.z == 0) {
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
            if (!leaf.neighbors[Direction.East] || posX % tree.size.x == 0) {
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
            if (!leaf.neighbors[Direction.South] || negZ % tree.size.z == 0) {
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
            if (!leaf.neighbors[Direction.West] || negX % tree.size.x == 0) {
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

    public static MeshData GenerateMeshData(QuadTree tree, Texture2D texture, int depthFilter = 0)
    {
        MeshData meshData = new MeshData();
        Vector3 posCorner = tree.position + tree.size;
        foreach (QuadTreeNode leaf in tree.leaves) {
            if(depthFilter != 0 && leaf.depth < depthFilter){ continue; }
           
            //corners
            float posX = (leaf.center.x + leaf.size.x * 0.5f);
            float negX = (leaf.center.x - leaf.size.x * 0.5f);
            float posZ = (leaf.center.z + leaf.size.z * 0.5f);
            float negZ = (leaf.center.z - leaf.size.z * 0.5f);
    
            int borderWidth = 2;
            //pixel index
            int zeroXpi = localizeDimension(leaf.center.x, tree.size.x)+borderWidth;
            int zeroZpi = localizeDimension(leaf.center.z, tree.size.z)+borderWidth;
            int posXpi = (posX == posCorner.x ? (int)tree.size.x : localizeDimension(posX, tree.size.x))+borderWidth;
            int negXpi = (negX == tree.position.x ? 0 : localizeDimension(negX, tree.size.x))+borderWidth;
            int posZpi = (posZ == posCorner.z ? (int)tree.size.x : localizeDimension(posZ, tree.size.z))+borderWidth;
            int negZpi = (negZ == tree.position.z ? 0 : localizeDimension(negZ, tree.size.z))+borderWidth;

            float y = texture.GetPixel(zeroXpi,zeroZpi,0).r * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(leaf.center.x,y,leaf.center.z));
            int zeroIndex = meshData.vertices.Count - 1;
           
            float y1 = texture.GetPixel(negXpi, posZpi, 0).r * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(negX, y1, posZ)); //1:
            float y3 = texture.GetPixel(posXpi, posZpi, 0).r * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(posX, y3, posZ)); //3
            float y5 = texture.GetPixel(posXpi, negZpi, 0).r * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(posX, y5, negZ)); //5
            float y7 = texture.GetPixel(negXpi, negZpi, 0).r * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(negX, y7, negZ)); //7
            int index = zeroIndex + 4;
            
            //edges & triangles
            if (!leaf.neighbors[Direction.North] || posZpi == tree.size.z+borderWidth) {
                float y2 = tree.settings.useInterpolation ? 
                    interpolate(y1, y3, .5f) : 
                    texture.GetPixel(zeroXpi, posZpi, 0).r * tree.settings.heightMultiplier;
                meshData.AddVertex(new Vector3(leaf.center.x, y2, posZ)); //2
                index++;
                meshData.AddTriangle(zeroIndex+1,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+2,zeroIndex);
            }
            else {
                meshData.AddTriangle(zeroIndex+1,zeroIndex+2,zeroIndex);
            }
            if (!leaf.neighbors[Direction.East] || posXpi == tree.size.x+borderWidth) {
                float y4 = tree.settings.useInterpolation ? 
                    interpolate(y3, y5, .5f) : 
                    texture.GetPixel(posXpi, zeroZpi, 0).r * tree.settings.heightMultiplier;
                meshData.AddVertex(new Vector3(posX, y4, leaf.center.z)); //4
                index++;
                meshData.AddTriangle(zeroIndex+2,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+3,zeroIndex);
            }
            else {
                meshData.AddTriangle(zeroIndex+2,zeroIndex+3, zeroIndex);
            }
            if (!leaf.neighbors[Direction.South] || negZpi == 0+borderWidth) {
                float y6 = tree.settings.useInterpolation ? 
                    interpolate(y5, y7, .5f) : 
                    texture.GetPixel(zeroXpi, negZpi, 0).r * tree.settings.heightMultiplier;
                meshData.AddVertex(new Vector3(leaf.center.x, y6, negZ)); //6
                index++;
                meshData.AddTriangle(zeroIndex+3,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+4,zeroIndex);
            }else {
                meshData.AddTriangle(zeroIndex+3,zeroIndex+4, zeroIndex);
            }
            if (!leaf.neighbors[Direction.West] || negXpi == 0+borderWidth) {
                float y8 = tree.settings.useInterpolation ? 
                    interpolate(y7, y1, .5f) : 
                    texture.GetPixel(negXpi, zeroZpi, 0).r * tree.settings.heightMultiplier;
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
    
    public static MeshData GenerateMeshData(QuadTree tree, Color[] noise, int depthFilter = 0)
    {
        MeshData meshData = new MeshData();
        Vector3 posCorner = tree.position + tree.size;
        foreach (QuadTreeNode leaf in tree.leaves) {
            if(depthFilter != 0 && leaf.depth < depthFilter){ continue; }
           
            //corners
            float posX = (leaf.center.x + leaf.size.x * 0.5f);
            float negX = (leaf.center.x - leaf.size.x * 0.5f);
            float posZ = (leaf.center.z + leaf.size.z * 0.5f);
            float negZ = (leaf.center.z - leaf.size.z * 0.5f);

            int borderWidth = 2;
            //pixel index
            int zeroXpi = localizeDimension(leaf.center.x, tree.size.x)+borderWidth;
            int zeroZpi = localizeDimension(leaf.center.z, tree.size.z)+borderWidth;
            int posXpi = (posX == posCorner.x ? (int)tree.size.x : localizeDimension(posX, tree.size.x))+borderWidth;
            int negXpi = (negX == tree.position.x ? 0 : localizeDimension(negX, tree.size.x))+borderWidth;
            int posZpi = (posZ == posCorner.z ? (int)tree.size.x : localizeDimension(posZ, tree.size.z))+borderWidth;
            int negZpi = (negZ == tree.position.z ? 0 : localizeDimension(negZ, tree.size.z))+borderWidth;
            int rowSize = (int)tree.size.x + borderWidth*2;

            float y = noise[(zeroZpi * rowSize + zeroXpi)].r * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(leaf.center.x,y,leaf.center.z));
            int zeroIndex = meshData.vertices.Count - 1;
           
            float y1 = noise[(posZpi * rowSize + negXpi)].r * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(negX, y1, posZ)); //1:
            float y3 = noise[posZpi * rowSize + posXpi].r * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(posX, y3, posZ)); //3
            float y5 = noise[negZpi * rowSize + posXpi].r * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(posX, y5, negZ)); //5
            float y7 = noise[negZpi * rowSize + negXpi].r * tree.settings.heightMultiplier;
            meshData.AddVertex(new Vector3(negX, y7, negZ)); //7
            int index = zeroIndex + 4;
            
            //edges & triangles
            if (!leaf.neighbors[Direction.North] || posZpi == tree.size.z+borderWidth) {
                float y2 = tree.settings.useInterpolation ? 
                    interpolate(y1, y3, .5f) : 
                    noise[(posZpi * rowSize + zeroXpi)].r * tree.settings.heightMultiplier;
                meshData.AddVertex(new Vector3(leaf.center.x, y2, posZ)); //2
                index++;
                meshData.AddTriangle(zeroIndex+1,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+2,zeroIndex);
            }
            else {
                meshData.AddTriangle(zeroIndex+1,zeroIndex+2,zeroIndex);
            }
            if (!leaf.neighbors[Direction.East] || posXpi == tree.size.x+borderWidth) {
                float y4 = tree.settings.useInterpolation ? 
                    interpolate(y3, y5, .5f) : 
                    noise[(zeroZpi * rowSize + posXpi)].r * tree.settings.heightMultiplier;
                meshData.AddVertex(new Vector3(posX, y4, leaf.center.z)); //4
                index++;
                meshData.AddTriangle(zeroIndex+2,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+3,zeroIndex);
            }
            else {
                meshData.AddTriangle(zeroIndex+2,zeroIndex+3, zeroIndex);
            }
            if (!leaf.neighbors[Direction.South] || negZpi == 0+borderWidth) {
                float y6 = tree.settings.useInterpolation ? 
                    interpolate(y5, y7, .5f) : 
                    noise[(negZpi * rowSize + zeroXpi)].r * tree.settings.heightMultiplier;
                meshData.AddVertex(new Vector3(leaf.center.x, y6, negZ)); //6
                index++;
                meshData.AddTriangle(zeroIndex+3,index,zeroIndex);
                meshData.AddTriangle(index,zeroIndex+4,zeroIndex);
            }else {
                meshData.AddTriangle(zeroIndex+3,zeroIndex+4, zeroIndex);
            }
            if (!leaf.neighbors[Direction.West] || negXpi == 0+borderWidth) {
                float y8 = tree.settings.useInterpolation ? 
                    interpolate(y7, y1, .5f) : 
                    noise[(zeroZpi * rowSize + negXpi)].r * tree.settings.heightMultiplier;
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
   
    public static int localizeDimension(float input, float size)
    {
        int res = (int)(input % size);
        if(input < 0){
            res += (int)size;
        } 
        return res;
    }
    
    private static float interpolate(float p1, float p2, float fraction) { return p1 + (p2 - p1) * fraction; } 

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
        lock (QuadTreeMeshThreadQueue) {
            QuadTreeMeshThreadQueue.Enqueue(new QuadTreeMeshThreadData<MeshData>(callback, data));
        }
    }
   
    public static void RequestQuadTreeMesh(QuadTree tree, Color[] noise, Action<MeshData> callback, int depthFilter = 0)
    {
        ThreadStart threadStart = delegate {
            QuadTreeMeshThread(tree, noise, callback, depthFilter);
        };
        new Thread(threadStart).Start();
    }

    public static void QuadTreeMeshThread(QuadTree tree, Color[] noise, Action<MeshData> callback, int depthFilter = 0)
    {
        MeshData data = GenerateMeshData(tree, noise, depthFilter);
        lock (QuadTreeMeshThreadQueue) {
            QuadTreeMeshThreadQueue.Enqueue(new QuadTreeMeshThreadData<MeshData>(callback, data));
        }
    }
    
    public static void RequestQuadTreeMesh(QuadTree tree, Action<MeshData> callback,int depthFilter = 0)
    {
        ThreadStart threadStart = delegate {
            QuadTreeMeshThread(tree, callback, depthFilter);
        };
        new Thread(threadStart).Start();
    }

    public static void QuadTreeMeshThread(QuadTree tree, Action<MeshData> callback,int depthFilter = 0)
    {
        MeshData data = GenerateMeshData(tree, depthFilter);
        lock (QuadTreeMeshThreadQueue) {
            QuadTreeMeshThreadQueue.Enqueue(new QuadTreeMeshThreadData<MeshData>(callback, data));
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
        if (QuadTreeMeshThreadQueue.Count > 0) {
            for (int i = 0; i < (QuadTreeMeshThreadQueue.Count > 2 ? 2 : QuadTreeMeshThreadQueue.Count); i++) {
                QuadTreeMeshThreadData<MeshData> threadData = QuadTreeMeshThreadQueue.Dequeue();
                threadData.callback(threadData.parameter);
            }
        }
    }
    
} 
