using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiSlopeNoise : INoiseFunction {
    float INoiseFunction.Noise(int x, int z) { //, Seed seed) {

        float noise = z * -0.25f + Mathf.Cos(x * Mathf.PI / (130.0f)) * 20 + Mathf.PerlinNoise(x * 0.01f, z * 0.01f) * 35;
        return noise;
    }
}
