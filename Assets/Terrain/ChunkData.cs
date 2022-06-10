using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkData : MonoBehaviour {

    [SerializeField] public BiomeSO Biome;

    public BiomeSO GetBiome() {
        return Biome;
    }

}
