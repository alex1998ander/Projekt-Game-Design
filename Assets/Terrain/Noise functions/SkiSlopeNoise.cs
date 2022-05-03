using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiSlopeNoise : INoiseFunction
{
    float INoiseFunction.Noise(int x, int z, Seed seed)
    {
        float noise = z * -0.15f + Mathf.Cos(x * Mathf.PI / (250.0f)) * 40 + Mathf.PerlinNoise(x * 0.01f, z * 0.01f) * 35;

        return noise;
    }
}
