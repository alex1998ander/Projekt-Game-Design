//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class TestNoise : INoiseFunction
//{
//    float INoiseFunction.Noise(int x, int z, Seed seed) {

//        float noise = Mathf.PerlinNoise(x * 0.005f + seed.Fst, z * 0.005f + seed.Fst) * 200
//            - Mathf.PerlinNoise(x * 0.01f + seed.Snd, z * 0.01f + seed.Snd) * 50
//            - Mathf.PerlinNoise(x * 0.02f + seed.Trd, z * 0.02f + seed.Trd) * 25;

//        return noise;
//    }
//}
