using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseHandler : MonoBehaviour {

    [SerializeField] private AnimationCurve erosionSpline = AnimationCurve.Linear(0, 1, 1, 0);
    [SerializeField] private AnimationCurve gradientSpline = AnimationCurve.Linear(0, 0, 1, -1);
    [SerializeField] private AnimationCurve fluctuationSpline = AnimationCurve.Linear(0, 0, 1, 100);

    //Singleton
    public static NoiseHandler Instance;
    private float offset = 0.0f;
    private float descent = 0.15f;

    public void Awake() {
        Instance = this;
        offset = Noise(125, 0);
    }

    public float Noise(int x, int z) {

        float noise = 0
            + (fluctuationSpline.Evaluate(FluctuationNoise(x, z)) * erosionSpline.Evaluate(ErosionNoise(x, z)))
            + (gradientSpline.Evaluate(GradientNoise(x, z)) - z) * descent
            //+ (-Mathf.Cos(Mathf.PI * Mathf.Pow((x - 125) / 125.0f, 2.0f)) + 1) * 25
            - offset
            ;

        return noise;
    }

    public static float ErosionNoise(int x, int y) {
        return NormalizeRange(AccumulatedPerlinNoise(x, y, 1, 600.0f), 0.0f, 0.7f);
    }

    public static float GradientNoise(int x, int y) {
        return NormalizeRange(AccumulatedPerlinNoise(x, y, 1, 1500.0f), 0.0f, 0.7f);
    }

    public static float FluctuationNoise(int x, int y) {
        return NormalizeRange(AccumulatedPerlinNoise(x, y, 3, 150.0f), 0.0f, 0.7f);
    }

    private static float AccumulatedPerlinNoise(int x, int y, int octaves, float frequency) {
        return AccumulatedPerlinNoise(x, y, octaves, frequency, frequency);
    }

    private static float AccumulatedPerlinNoise(int x, int y, int octaves, float frequencyX, float frequencyZ) {

        frequencyX = 1 / frequencyX;
        frequencyZ = 1 / frequencyZ;

        float noiseValue = Mathf.PerlinNoise(x * frequencyX, y * frequencyZ);
        float amplitude = 1.0f;
        float normalizingValue = 1.0f;

        for (int i = 0; i < octaves; i++) {
            frequencyX *= 2;
            frequencyZ *= 2;
            amplitude /= 2;
            normalizingValue += amplitude;

            noiseValue += Mathf.PerlinNoise(x * frequencyX, y * frequencyZ) * amplitude;
        }

        return noiseValue / normalizingValue;
    }

    private static float NormalizeRange(float value, float min, float max) {
        return (value - min) / (max - min);
    }

    public float GetGradientSlopeZ(int x, int y) {
        return ((gradientSpline.Evaluate(GradientNoise(x, y - 1)) - y + 1) - (gradientSpline.Evaluate(GradientNoise(x, y + 1)) - y - 1)) * 0.5f;
    }
}
