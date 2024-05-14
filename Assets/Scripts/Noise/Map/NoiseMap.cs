using UnityEngine;


public class NoiseMap 
{
    public float[,] map;
    public Vector2Int size;
    public MinMax minMax = new MinMax();
	
    public NoiseMap(Vector2Int size)
    {
        map = new float[size.x, size.y];
        this.size = size;
    }

    public void addValue(Vector2Int pos, float value)
    {
        map[pos.x, pos.y] = value;
        minMax.AddValue(value);
    }

    public void normalize(Vector2Int size,Noise.NormalizeMode mode = Noise.NormalizeMode.Default){
        for (int z = 0; z < size.y; z++) {
            for (int x = 0; x < size.x; x++) {
                map[x,z] = (map[x,z] - minMax.Min) / (minMax.Max - minMax.Min);
            }
        }
    }
}

public class DensityMap 
{
    
    public float[,,] map;
    public Vector3Int size;
    public MinMax minMax = new MinMax();
	
    public DensityMap(Vector3Int size)
    {
        map = new float[size.x, size.y, size.z];
        this.size = size;
    }
	
    public void addValue(Vector3Int pos, float value)
    {
        map[pos.x, pos.y, pos.z] = value;
        minMax.AddValue(value);
    }
	
    public void normalize(Vector3Int size,Noise.NormalizeMode mode = Noise.NormalizeMode.Default){
        for (int z = 0; z < size.z; z++) {
            for (int y = 0; y < size.y; y++) {
                for (int x = 0; x < size.x; x++) {
                    map[x, y, z] = (map[x, y, z] - minMax.Min) / (minMax.Max - minMax.Min); // postitive
                }
            }
        }
    }
}