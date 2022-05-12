using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {

    [SerializeField] public BiomeSO Biome;

    public BiomeSO GetBiome() {
        return Biome;
    }

}
