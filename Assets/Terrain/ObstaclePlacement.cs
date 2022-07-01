using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePlacement : MonoBehaviour {

    public static ObstaclePlacement Instance;
    [SerializeField] private PlacementPresetSO[] presets;

    private float yPlacementOffset = 1f;

    private void Awake() {
        Instance = this;
    }

    public PlacementPresetSO SelectRandomPlacementPreset() {
        return presets[Random.Range(0, presets.Length)];
    }

    public void SpawnObstacles(ChunkData chunk) {

        foreach (Vector2 pos in chunk.Preset.Pos) {

            Vector3 worldPos = new(chunk.gameObject.transform.position.x + pos.x * TerrainHandler.TERRAIN_SCALE, GetHeight(chunk, pos) - yPlacementOffset, chunk.gameObject.transform.position.z + pos.y * TerrainHandler.TERRAIN_SCALE);

            Transform obstaclePrefab = chunk.Biome.SelectRandomObstacle();

            if (obstaclePrefab != null) {
                Transform obstacle = Instantiate(obstaclePrefab, worldPos, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
                obstacle.parent = chunk.gameObject.transform;
            }
        }
    }

    private float GetHeight(ChunkData chunk, Vector2 pos) {

        Vector3[] vertices = chunk.gameObject.GetComponent<MeshFilter>().mesh.vertices;

        return vertices[(int) pos.y * (TerrainHandler.CHUNK_SIZE + 1) + (int) pos.x].y;

    }
}
