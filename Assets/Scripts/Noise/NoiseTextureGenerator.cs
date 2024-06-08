using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public static class NoiseTextureGenerator
{
    public static readonly Queue<NoiseTextureThreadData<Color[]>> NoiseTextureThreadQueue = new Queue<NoiseTextureThreadData<Color[]>>();
    
    public static Texture2D GenerateNoiseTexture(Vector3 position, Vector3Int mapSize, NoiseSettings settings, int step = 1)
    {
        Vector2Int borderSize = Vector2Int.one * 4;
        Vector2Int size = new Vector2Int(mapSize.x, mapSize.z)+borderSize;
        Texture2D texture = new Texture2D(size.x, size.y,TextureFormat.RGBAFloat,false,false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;

        for (int z = 0; z < size.y; z+=step) {
            for (int x = 0; x < size.x; x+=step) {
                Vector3 mapPos =  position + new Vector3(x,settings.seed,z);
                Vector3 sample = (mapPos + settings.offset) / settings.scale;
                float n = Noise.Evaluate(sample, settings);
                texture.SetPixel(x,z,new Color(n,0,0,0),0);
            }
        }
        
        return texture;
    }
    
    public static Color[] GenerateNoise(Vector3 position, Vector3Int mapSize, NoiseSettings settings, int step = 1)
    {
        Vector2Int borderSize = Vector2Int.one * 4;
        Vector2Int size = new Vector2Int(mapSize.x, mapSize.z)+borderSize;

        Color[] colors = new Color[size.x * size.y];
        for (int z = 0; z < size.y; z+=step) {
            for (int x = 0; x < size.x; x+=step) {
                Vector3 mapPos =  position + new Vector3(x,settings.seed,z);
                Vector3 sample = (mapPos + settings.offset) / settings.scale;
                float n = Noise.Evaluate(sample, settings);
                colors[z * size.x + x] = new Color(n, 0, 0, 0);
            }
        }
        
        return colors;
    }
    
    public static void RequestNoiseTexture(Vector3 position, Vector3Int mapSize, NoiseSettings settings, Action<Color[]> callback, int step = 1)
    {
        ThreadStart threadStart = delegate {
            NoiseTextureThread(position, mapSize, settings,callback,step);
        };
        new Thread(threadStart).Start();
    }

    public static void NoiseTextureThread(Vector3 position, Vector3Int mapSize, NoiseSettings settings, Action<Color[]> callback, int step = 1)
    {
        Color[] data = GenerateNoise(position, mapSize, settings,step);
        lock (NoiseTextureThreadQueue) {
            NoiseTextureThreadQueue.Enqueue(new NoiseTextureThreadData<Color[]>(callback, data));
        }
    }
    
    public struct NoiseTextureThreadData<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public NoiseTextureThreadData(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

    public static void ProcessThreadQueue()
    {
        if (NoiseTextureThreadQueue.Count > 0) {
            for (int i = 0; i < (NoiseTextureThreadQueue.Count > 2 ? 2 : NoiseTextureThreadQueue.Count); i++) {
                NoiseTextureThreadData<Color[]> threadData = NoiseTextureThreadQueue.Dequeue();
                threadData.callback(threadData.parameter);
            }
        }
    }
}

