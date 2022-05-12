using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

    private GameObject[] Chunks { get; set; }
    public static int CHUNK_SIZE = 10;
    public static int X_CHUNCK_COUNT = 57;
    public static int Z_CHUNCK_COUNT = 57;
    public static int VERTEX_DISTANCE = 1;
    private Seed seed;

    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private string seedStr = "";
    [SerializeField] private INoiseFunction noiseFunction = new SkiSlopeNoise();

    public void Awake() {

        BiomeGeneration.Instance.InitializeBiomeMap();

        seed = new Seed(seedStr);
        Chunks = new GameObject[X_CHUNCK_COUNT * Z_CHUNCK_COUNT];

        //Für jeden Chunk
        for (int chunkIdx = 0; chunkIdx < Chunks.Length; chunkIdx++) {

            //Positionen der Mesh-Knotenpunkte
            Vector3[] verticies = new Vector3[(CHUNK_SIZE + 1) * (CHUNK_SIZE + 1)];

            for (int i = 0, z = 0; z <= CHUNK_SIZE; z++) {
                for (int x = 0; x <= CHUNK_SIZE; x++) {

                    int xGlobal = (chunkIdx % X_CHUNCK_COUNT) * CHUNK_SIZE + x;
                    int zGlobal = (chunkIdx / X_CHUNCK_COUNT) * CHUNK_SIZE + z;

                    verticies[i] = new Vector3(x * VERTEX_DISTANCE, noiseFunction.Noise(xGlobal * VERTEX_DISTANCE, zGlobal * VERTEX_DISTANCE, seed), z * VERTEX_DISTANCE);
                    i++;
                }
            }

            //Berechnung der Dreiecke vom Mesh
            int[] triangles = new int[CHUNK_SIZE * CHUNK_SIZE * 6];

            int vert = 0;
            int current = 0;

            //Berechnung der Punkte der Dreicke (6 Punkte: 3 Punkte pro Dreieck, 2 Dreiecke pro Kachel)
            for (int z = 0; z < CHUNK_SIZE; z++) {
                for (int x = 0; x < CHUNK_SIZE; x++) {

                    triangles[current] = vert;
                    triangles[current + 1] = vert + CHUNK_SIZE + 1;
                    triangles[current + 2] = vert + CHUNK_SIZE + 2;

                    triangles[current + 5] = vert;
                    triangles[current + 3] = vert + CHUNK_SIZE + 2;
                    triangles[current + 4] = vert + 1;

                    vert++;
                    current += 6;
                }
                vert++;
            }

            Vector3 chunkPos = new Vector3((chunkIdx % X_CHUNCK_COUNT) * CHUNK_SIZE * VERTEX_DISTANCE, 0, (chunkIdx / X_CHUNCK_COUNT) * CHUNK_SIZE * VERTEX_DISTANCE);

            GameObject newChunk = Instantiate(chunkPrefab, chunkPos, Quaternion.identity);

            Chunk chunkData = newChunk.GetComponent<Chunk>();
            chunkData.Biome = BiomeGeneration.GetBiome(new Vector2Int(chunkIdx % X_CHUNCK_COUNT, (int) chunkIdx / X_CHUNCK_COUNT));

            newChunk.GetComponent<Renderer>().material = chunkData.Biome.DebugMaterial;

            newChunk.transform.parent = transform;
            newChunk.name = "Chunk " + chunkIdx;
            newChunk.layer = LayerMask.NameToLayer("Terrain");


            //Erstelllung des Meshs
            Mesh mesh = newChunk.GetComponent<MeshFilter>().mesh;
            mesh.Clear();
            mesh.vertices = verticies;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            MeshCollider meshCollider = newChunk.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;


            Chunks[chunkIdx] = newChunk;
        }
    }
}
