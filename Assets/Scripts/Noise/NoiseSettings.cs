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
    [Range(0.1f,1000f)]
	public float scale = 50f;
    [Range(0.01f,2f)]
	public float gain = 0.5f;
    public Vector3 offset;
    [Header("Perlin")]
    [Range(0,5)]
	public int octaves = 3;
    [Range(1,3)]
	public float lacunarity = 2f;

    [Header("Simplex")]
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
        this.noiseMode = source.noiseMode;
        this.filterMode = source.filterMode;
        this.normalizeMode = source.normalizeMode;

		this.seed = source.seed;
		this.scale = source.scale;
        this.gain = source.gain;
		this.offset = source.offset;
		
		this.octaves = source.octaves;
		this.lacunarity = source.lacunarity;

        this.layerCount = source.layerCount;
        this.strength = source.strength;
        this.baseRoughness = source.baseRoughness;
        this.roughness = source.roughness;
        this.minValue = source.minValue;
        this.weightMultiplier = source.weightMultiplier;

		this.useDomainWarping = (disableDomainWarping ? false : source.useDomainWarping);
		this.warpStrength = source.warpStrength;
	}
}
