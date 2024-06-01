using UnityEngine;

public static class Noise
{
    public enum NoiseMode {Perlin, Simplex}
    public enum FilterMode {Simple, Rigid, None}
    public enum NormalizeMode {Default, Positive, Negative}

    public static float Evaluate(Vector3 point,NoiseSettings settings){
        float value;
        if(settings.useDomainWarping){
            value = DomainWarping.Warp(point,settings);
        }else{
            switch(settings.noiseMode){
                case NoiseMode.Simplex:
                    value = SimplexEvaluate(point,settings);
                break;
                case NoiseMode.Perlin:
                default:
                    value = PerlinEvaluate(point,settings);
                break;
            }
        }
        return value;
    }

    //PERLIN

    public static float PerlinEvaluate(Vector3 point,NoiseSettings settings){
        float value;
        switch(settings.filterMode){
            case FilterMode.Rigid:
                value = PerlinRigidEvaluate(point,settings);
            break;
            case FilterMode.Simple:
                value = PerlinSimpleEvaluate(point,settings);
            break;
            case FilterMode.None:
            default:
                value = Perlin.Fbm(point, settings.octaves, settings.lacunarity, settings.persistence); 
            break;
        }
        return value;
    }

    public static float PerlinSimpleEvaluate(Vector3 point,NoiseSettings settings){
        return normalize(Perlin.Fbm(point,settings.octaves,settings.lacunarity, settings.persistence),settings.normalizeMode);
    }

    private static float PerlinRigidEvaluate(Vector3 point, NoiseSettings settings)
    {
        return normalizeRigid(Perlin.Fbm(point,settings.octaves,settings.lacunarity, settings.persistence),settings.normalizeMode);
    }

    //SIMPLEX

    public static float SimplexEvaluate(Vector3 point,NoiseSettings settings){
        float value;
        switch(settings.filterMode){
            case FilterMode.Rigid:
                value = SimplexRigidEvaluate(point,settings);
            break;
            case FilterMode.Simple:
            case FilterMode.None:
            default:
                value = SimplexSimpleEvaluate(point,settings);
            break;
        }
        return value;
    }

    public static float SimplexSimpleEvaluate(Vector3 point,NoiseSettings settings){
        Simplex simplex = new Simplex();
        float value = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;

        for(int i = 0; i < settings.layerCount; i++){
            float v = simplex.Evaluate(point * frequency + settings.offset);
            value += (settings.filterMode == Noise.FilterMode.None ? v : normalize(v,settings.normalizeMode)) * amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.gain;
        }
        value = value - settings.minValue;
        return value * settings.strength;
    }

    public static float SimplexRigidEvaluate(Vector3 point,NoiseSettings settings){
        Simplex simplex = new Simplex();
        float value = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;
        float weight = 1;

        for(int i = 0; i < settings.layerCount; i++){
            float v = normalizeRigid(simplex.Evaluate(point * frequency + settings.offset),settings.normalizeMode);
            v *= v;
            v*= weight;
            weight = v;

            value += v * amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.gain;
        }
        value = value - settings.minValue;
        return value * settings.strength;
    }

    //Normalize
    private static float normalize(float value, NormalizeMode mode = NormalizeMode.Default){
        switch(mode){
            case NormalizeMode.Positive:
                //value = (value - (-1)) / (1 - (-1));
                value = (value + 1) * 0.5f;
            break;
            case NormalizeMode.Negative:
                value = (value - 1) * 0.5f;
            break;
            case NormalizeMode.Default:
            default:
            break;
        }
        return value;
    }

    private static float normalizeRigid(float value, NormalizeMode mode = NormalizeMode.Default){
        switch(mode){
            case NormalizeMode.Positive:
                value = 1-Mathf.Abs(value);
                break;
            case NormalizeMode.Negative:
                value = 1 - (1 - Mathf.Abs(value));
                break;
            case NormalizeMode.Default:
            default:
                break;
        }
        return value;
    }
}
