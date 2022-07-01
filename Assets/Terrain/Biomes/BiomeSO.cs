using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome_", menuName = "Procedural Generation/Biome Configuration")]
public class BiomeSO : ScriptableObject {

    public string Name;

    public Material DebugMaterial;

    public List<Transform> Obstacles;
    public List<int> Weights;

    public int emptyWeight;

    private int totalWeight;

    public void Init() {

        totalWeight = emptyWeight;

        foreach (int currentWeight in Weights) {
            totalWeight += currentWeight;
        }

    }

    public Transform SelectRandomObstacle() {

        int random = Random.Range(0, totalWeight);

        for (int i = 0; i < Obstacles.Count; i ++) {

            random -= Weights[i];

            if (random < 0) {
                return Obstacles[i];
            }
        }

        return null;
    }
}
