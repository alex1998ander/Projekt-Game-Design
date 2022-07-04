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

            Vector3 worldPos = new(
                chunk.gameObject.transform.position.x + pos.x * TerrainHandler.TERRAIN_SCALE,
                GetHeight(chunk, pos) - yPlacementOffset,
                chunk.gameObject.transform.position.z + pos.y * TerrainHandler.TERRAIN_SCALE);

            Transform obstaclePrefab = chunk.Biome.SelectRandomObstacle();

            if (obstaclePrefab != null) {
                Transform obstacle = Instantiate(obstaclePrefab, worldPos, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
                obstacle.parent = chunk.gameObject.transform;

                foreach (Transform child in obstacle.transform) {

                    Vector3 childWorldSpace = child.transform.TransformPoint(Vector3.zero);

                    float y = GetHeight(chunk, new Vector2(
                        (childWorldSpace.x - chunk.transform.position.x) / TerrainHandler.TERRAIN_SCALE,
                        (childWorldSpace.z - chunk.transform.position.z) / TerrainHandler.TERRAIN_SCALE)
                    );

                    Debug.Log((childWorldSpace.x - chunk.transform.position.x) / TerrainHandler.TERRAIN_SCALE);

                    child.position = new Vector3(child.position.x, y - yPlacementOffset + child.localPosition.y, child.position.z);
                }
            }
        }
    }
    private float GetHeight(ChunkData chunk, Vector2 pos) {

        Vector3[] vertices = chunk.gameObject.GetComponent<MeshFilter>().mesh.vertices;

        //Interpolation
        int x_int = (int)pos.x;
        int y_int = (int)pos.y;

        float corner_ul = vertices[y_int * (TerrainHandler.CHUNK_SIZE + 1) + x_int].y;
        float corner_ur = vertices[y_int * (TerrainHandler.CHUNK_SIZE + 1) + (x_int + 1)].y;
        float corner_ll = vertices[(y_int + 1) * (TerrainHandler.CHUNK_SIZE + 1) + x_int].y;
        float corner_lr = vertices[(y_int + 1) * (TerrainHandler.CHUNK_SIZE + 1) + (x_int + 1)].y;

        float delta_xr = x_int + 1 - pos.x;
        float delta_xl = pos.x - x_int;

        float delta_yu = pos.y - y_int;
        float delta_yl = y_int + 1 - pos.y;

        return corner_ul * delta_xr * delta_yl +
             corner_ur * delta_xl * delta_yl +
             corner_ll * delta_xr * delta_yu +
             corner_lr * delta_xl * delta_yu;

    }
}
