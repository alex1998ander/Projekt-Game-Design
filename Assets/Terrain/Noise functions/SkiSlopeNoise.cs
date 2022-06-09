using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiSlopeNoise : INoiseFunction {
    float INoiseFunction.Noise(int x, int z, BiomeSO biome) {

        //float perlinNoiseFactor = 0.01f;

        float noise = z * -0.35f + Mathf.Cos(x * Mathf.PI / (135.0f)) * 8 + Mathf.PerlinNoise(x * 0.01f, z * 0.01f) * 35 - Mathf.PerlinNoise(x * 0.05f, z * 0.05f) * 5;
        //float spline = biome.GetSplinePoint(Mathf.PerlinNoise(x * 0.01f, z * 0.01f) - Mathf.PerlinNoise(x * 0.05f, z * 0.05f) * 0.25f);
        //Debug.Log(spline + " " + Mathf.PerlinNoise(x / 270.0f, z / 270.0f));
        //float noise = z * -0.35f + spline * 35;
        //Debug.Log(biome.GetSplinePoint(x / 270.0f % 5) + " "  + x);
        return noise;
    }
}
