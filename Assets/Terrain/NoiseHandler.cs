using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseHandler : MonoBehaviour {

    [SerializeField] private AnimationCurve erosionSpline = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve gradientSpline = AnimationCurve.Linear(0, 0, 1, -1);
    [SerializeField] private AnimationCurve fluctuationSpline = AnimationCurve.Linear(0, 0, 1, 100);

    //Singleton
    public static NoiseHandler Instance;

    public void Awake() {
        Instance = this;
    }

    public float ErosionNoise(int x, int y) {
        return erosionSpline.Evaluate(AccumulatedPerlinNoise(x, y, 1, 800.0f));
    }

    public float GradientNoise(int x, int y) {
        return gradientSpline.Evaluate(AccumulatedPerlinNoise(x, y, 1, 800.0f, 1500.0f));
    }

    public float FluctuationNoise(int x, int y) {
        return fluctuationSpline.Evaluate(AccumulatedPerlinNoise(x, y, 3, 100.0f));
    }

    private float AccumulatedPerlinNoise(int x, int y, int octaves, float frequency) {
        return AccumulatedPerlinNoise(x, y, octaves, frequency, frequency);
    }

    private float AccumulatedPerlinNoise(int x, int y, int octaves, float frequencyX, float frequencyZ) {

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
}
