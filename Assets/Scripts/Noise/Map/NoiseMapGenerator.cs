using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public static class NoiseMapGenerator
{
	public enum DensityFunction
	{
		Default, Floor, Sphere
	}
	
	public static readonly Queue<MapGeneratorThreadData<NoiseMap>> NoiseThreadQueue = new Queue<MapGeneratorThreadData<NoiseMap>>();
	public static readonly Queue<MapGeneratorThreadData<DensityMap>> DensityThreadQueue = new Queue<MapGeneratorThreadData<DensityMap>>();
	
	//2D Maps
    public static NoiseMap GenerateNoiseMap(NoiseMapSettings settings,int step = 1)
    {
	    Vector2Int size = new Vector2Int(settings.mapSize.x, settings.mapSize.z);
	    NoiseMap noiseMap = new NoiseMap(size);
		for (int z = 0; z < settings.mapSize.z; z+=step) {
			for (int x = 0; x < settings.mapSize.x; x+=step) {
                Vector3 mapPos = new Vector3(x,settings.noiseSettings.seed,z);
                Vector3 sample = (settings.position + settings.noiseSettings.offset + mapPos) / settings.noiseSettings.scale;
                noiseMap.addValue(new Vector2Int(x,z),Noise.Evaluate(sample,settings.noiseSettings));
			}
        }
		if (settings.noiseSettings.LocalNormalization) { noiseMap.normalize(size,settings.noiseSettings.normalizeMode); }
        return noiseMap;
	}
    
    public static NoiseMap GenerateNoiseMap(Vector3 position, Vector3Int mapSize, NoiseSettings settings, int step = 1)
    {
	    Vector2Int size = new Vector2Int(mapSize.x, mapSize.z);
	    NoiseMap noiseMap = new NoiseMap(size);
	    for (int z = 0; z < mapSize.z; z+=step) {
		    for (int x = 0; x < mapSize.x; x+=step) {
			    Vector3 mapPos = new Vector3(x,settings.seed,z);
			    Vector3 sample = (position + settings.offset + mapPos) / settings.scale;
			    noiseMap.addValue(new Vector2Int(x,z),Noise.Evaluate(sample,settings));
		    }
	    }
	    if (settings.LocalNormalization) { noiseMap.normalize(size,settings.normalizeMode); }
	    return noiseMap;
    }
    
    
    
    
    //3D Maps
    public static DensityMap GenerateDensityMap(DensityMapSettings settings) {
	    DensityMap densityMap = new DensityMap(settings.mapSize);
	    for (int z = 0; z < settings.mapSize.z; z++) {
		    for (int y = 0; y < settings.mapSize.y; y++) {
			    for (int x = 0; x < settings.mapSize.x; x++) {
				    Vector3 mapPos = new Vector3(x,y,z);
				    Vector3 sample = (settings.position + settings.noiseSettings.offset + mapPos) / settings.noiseSettings.scale;
				    float value;
				    switch (settings.densityFunction)
				    {
					    case DensityFunction.Floor:
						    value = -sample.y + Noise.Evaluate(sample, settings.noiseSettings);
					    break;
					    case DensityFunction.Sphere:
							value = settings.sphereRadius - (settings.position + mapPos).magnitude + Noise.Evaluate(sample,settings.noiseSettings);
					    break;
					    case DensityFunction.Default:
					    default:
							value = Noise.Evaluate(sample,settings.noiseSettings);
						break;
				    }
					densityMap.addValue(new Vector3Int(x,y,z),value); 
			    }
		    }
	    }
		if (settings.noiseSettings.LocalNormalization) { densityMap.normalize(settings.mapSize,settings.noiseSettings.normalizeMode); }
	    return densityMap;
    }
    
	//Color Maps
    public static Color[] GenerateColorMap(NoiseMap noiseMap, Gradient gradient){
	    Color[] colorMap = new Color[noiseMap.size.x*noiseMap.size.y];
	    for (int y = 0; y < noiseMap.size.y; y++) {
		    for (int x = 0; x < noiseMap.size.x; x++) {
			    colorMap[y*noiseMap.size.x+x] = gradient.Evaluate(noiseMap.map[x,y]);
		    }
	    }
	    return colorMap;
    }
    
    public static Color[] GenerateColorMap(DensityMap densityMap, Gradient gradient){
	    Color[] colorMap = new Color[densityMap.size.x*densityMap.size.y*densityMap.size.z];
	    for (int z = 0; z < densityMap.size.z; z++) {
		    for (int y = 0; y < densityMap.size.y; y++) {
			    for (int x = 0; x < densityMap.size.x; x++) {
				    colorMap[z * (densityMap.size.y + y) * (densityMap.size.x + x)] = gradient.Evaluate(densityMap.map[x,y,z]);
			    }
		    }
	    }
	    return colorMap;
    }

    //Thread Handling
	public static void RequestNoiseMap(NoiseMapSettings settings, Action<NoiseMap> callback)
	{
		ThreadStart start = delegate
		{
			NoiseMapGeneratorThread(settings, callback);
		};
		new Thread(start).Start();
	}
	
	public static void NoiseMapGeneratorThread(NoiseMapSettings settings,Action<NoiseMap> callback)
	{
		NoiseMap map = GenerateNoiseMap(settings);
		lock (NoiseThreadQueue)
		{
			NoiseThreadQueue.Enqueue(new MapGeneratorThreadData<NoiseMap>(callback,map));
		}
	}
	
	public static void RequestNoiseMap(Vector3 position, Vector3Int mapSize,NoiseSettings settings, Action<NoiseMap> callback)
	{
		ThreadStart start = delegate
		{
			NoiseMapGeneratorThread( position, mapSize,settings, callback);
		};
		new Thread(start).Start();
	}
	
	public static void NoiseMapGeneratorThread(Vector3 position, Vector3Int mapSize,NoiseSettings settings,Action<NoiseMap> callback)
	{
		NoiseMap map = GenerateNoiseMap(position, mapSize,settings);
		lock (NoiseThreadQueue)
		{
			NoiseThreadQueue.Enqueue(new MapGeneratorThreadData<NoiseMap>(callback,map));
		}
	}
	
	public static void RequestDensityMap(DensityMapSettings settings, Action<DensityMap> callback)
	{
		ThreadStart start = delegate
		{
			DensityMapGeneratorThread(settings, callback);
		};
		new Thread(start).Start();
	}
	
	public static void DensityMapGeneratorThread(DensityMapSettings settings,Action<DensityMap> callback)
	{
		DensityMap map = GenerateDensityMap(settings);
		lock (DensityThreadQueue)
		{
			DensityThreadQueue.Enqueue(new MapGeneratorThreadData<DensityMap>(callback,map));
		}
	}	

	public struct MapGeneratorThreadData<T>
	{
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapGeneratorThreadData(Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
	}

	public static void ProcessThreadQueue()
	{
		if (NoiseThreadQueue.Count > 0)
		{
			for (int i = 0; i < NoiseThreadQueue.Count; i++)
			{
				MapGeneratorThreadData<NoiseMap> threadData = NoiseThreadQueue.Dequeue();
				threadData.callback(threadData.parameter);
			}
		}
		if (DensityThreadQueue.Count > 0)
		{
			for (int i = 0; i < DensityThreadQueue.Count; i++)
			{
				MapGeneratorThreadData<DensityMap> threadData = DensityThreadQueue.Dequeue();
				threadData.callback(threadData.parameter);
			}
		}
	}
}