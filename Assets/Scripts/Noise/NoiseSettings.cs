using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings{
    public Noise.NoiseMode noiseMode;
    public Noise.FilterMode filterMode;
    public Noise.NormalizeMode normalizeMode = Noise.NormalizeMode.Positive;
    public bool LocalNormalization;

    [Header("General")]
	[Range(-1000000,1000000)]
	public int seed;
    [Range(0.1f,10000f)]
	public float scale = 50f;
    public Vector3 offset;
    [Header("Perlin")]
    [Range(0,10)]
	public int octaves = 3;
    [Range(1,3)]
	public float lacunarity = 2f;
	[Min(0.01f)]
	public float persistence = 1f;
	
	[Header("Simplex")]
	[Range(0.01f,2f)]
	public float gain = 0.5f;
	[Range(1,8)]
	public int layerCount = 1;
	public float strength = 1f;
	public float baseRoughness = 1f;
	public float roughness = 2f;
	public float minValue;
    public float weightMultiplier = 0.8f;

    [Header("Domain Warping")]
	public bool useDomainWarping = false;
	[Range(0,20)]
	public float warpStrength = 10f;

	public void copy(NoiseSettings source, bool disableDomainWarping = true){
        noiseMode = source.noiseMode;
        filterMode = source.filterMode;
        normalizeMode = source.normalizeMode;

		seed = source.seed;
		scale = source.scale;
        gain = source.gain;
		offset = source.offset;
		
		octaves = source.octaves;
		lacunarity = source.lacunarity;

        layerCount = source.layerCount;
        strength = source.strength;
        baseRoughness = source.baseRoughness;
        roughness = source.roughness;
        minValue = source.minValue;
        weightMultiplier = source.weightMultiplier;

		useDomainWarping = (disableDomainWarping ? false : source.useDomainWarping);
		warpStrength = source.warpStrength;
	}
}
