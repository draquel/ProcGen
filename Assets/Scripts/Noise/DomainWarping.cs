using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DomainWarping
{
    public static float Warp(Vector3 point, NoiseSettings settings){
        NoiseSettings warpSettings = new NoiseSettings();
        warpSettings.copy(settings);

        Vector3 q = new Vector3(
            Noise.Evaluate(point,warpSettings),
            Noise.Evaluate(point+(Vector3.one*1.3f),warpSettings),
            Noise.Evaluate(point+(Vector3.one*5.2f),warpSettings)
        );

        Vector3 r = new Vector3(
            Noise.Evaluate(point + (warpSettings.warpStrength*q) + (Vector3.one*1.7f),warpSettings),
            Noise.Evaluate(point + (warpSettings.warpStrength*q) + (Vector3.one*2.8f),warpSettings),
            Noise.Evaluate(point + (warpSettings.warpStrength*q) + (Vector3.one*8.3f),warpSettings)
        );

        return Noise.Evaluate(point+(r*warpSettings.warpStrength),warpSettings);
    }
}