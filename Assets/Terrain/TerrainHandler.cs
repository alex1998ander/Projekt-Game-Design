using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHandler : MonoBehaviour {

    #region Konstanten
    //Vertexanzahl pro Chunkseite
    private const int CHUNK_SIZE = 10;
    //Chunkanzahl pro Seite
    private static int X_CHUNCK_COUNT;
    private static int Z_CHUNCK_COUNT;
    //Chunkanzahl gesamt
    private static int CHUNK_COUNT;

    private const int TERRAIN_SCALE = 10;

    //Y-Groesse jeder neu generierten Terrainerweiterung
    private static int NEW_AREA_LENGTH;

    //Dreiecksanordnung der Verticies eines Meshes
    private static int[] TRIANGLES_MESH;
    private static int[] TRIANGLES_COLLIDER;

    //Collidergroesse unterhalb des Spielers (MUSS UNGERADE sein, damit der Spieler in der Mitte bleibt)
    private const int COLLIDER_CHUNK_SIZE = 5;
    #endregion

    #region Variablen
    //Chunk-Array
    private GameObject[] Chunks { get; set; }
    //Spielerposition
    public Transform Player;
    //Collider fuer den Untergrund in der Umgebung des Spielers
    public GameObject Collider;

    //World-Koordinate des Terrainanfangs
    private int terrainOffsetZ = 0;
    //Startindex des Chunkarrays
    //Die neugenerierten Chunks werden im Array an der Stelle ersetzt, an denen die alten zu entfernenden Chunks liegen.
    //Nach dem Entfernen der alten Chunks, ist der neue "erste" Chunk an der Stelle 'chunkArrayStartIndex' im Array)
    private int chunkArrayStartIndex = 0;
    //Aktueller Index im Chunk-Array an dem sich der Spieler befindet
    private int currentPlayerChunkIdx = -1;

    //Singleton
    public static TerrainHandler Instance;

    //ChunkPrefab
    [SerializeField] private GameObject chunkPrefab;
    //Noise-Funktion
    [SerializeField] private INoiseFunction noiseFunction = new SkiSlopeNoise();
    #endregion

    public void Start() {
        Instance = this;

        BiomeGeneration.Init();

        X_CHUNCK_COUNT = BiomeGeneration.GetWorldWidth();
        Z_CHUNCK_COUNT = BiomeGeneration.GetWorldLength();

        CHUNK_COUNT = X_CHUNCK_COUNT * Z_CHUNCK_COUNT;

        //+4 durchs 3-fache Smoothing (+6 -2 fuer generellen Offset von 2)
        NEW_AREA_LENGTH = X_CHUNCK_COUNT - (X_CHUNCK_COUNT / 3) + 4;

        TRIANGLES_MESH = CreateMeshTriangleArray(1);
        TRIANGLES_COLLIDER = CreateMeshTriangleArray(COLLIDER_CHUNK_SIZE);


        Chunks = new GameObject[CHUNK_COUNT];

        InitializeChunks();
    }

    public void Update() {

        if (Player.position.z > (terrainOffsetZ + (Z_CHUNCK_COUNT * 0.5f)) * CHUNK_SIZE * TERRAIN_SCALE) {
            ExpandWorld();
        }

        int chunkIndex = GetChunkIndexFromWorldPosition(Player.transform.position);

        int debug_x = (int)Player.transform.position.x / TERRAIN_SCALE;
        int debug_z = (int)Player.transform.position.z / TERRAIN_SCALE;

        Debug.Log("Fluctuation: " + NoiseHandler.Instance.FluctuationNoise(debug_x, debug_z) 
            + "\nErosion: " + NoiseHandler.Instance.ErosionNoise(debug_x, debug_z) 
            + "\nGradient: " + NoiseHandler.Instance.GradientNoise(debug_x, debug_z));

        if (chunkIndex != currentPlayerChunkIdx) {
            UpdateTerrainMeshCollider();
        }
    }

    private void InitializeChunks() {
        //Fuer jeden Chunk
        for (int chunkIdx = 0; chunkIdx < Chunks.Length; chunkIdx++) {

            Vector2Int chunkPos = new(chunkIdx % X_CHUNCK_COUNT, chunkIdx / X_CHUNCK_COUNT);
            Vector3 chunkPosWorld = new(chunkPos.x * CHUNK_SIZE * TERRAIN_SCALE, 0, chunkPos.y * CHUNK_SIZE * TERRAIN_SCALE);

            GameObject newChunk = Instantiate(chunkPrefab, chunkPosWorld, Quaternion.identity);

            newChunk.layer = LayerMask.NameToLayer("Terrain");
            newChunk.transform.parent = transform;

            SetUpChunk(newChunk, chunkPos);

            Chunks[chunkIdx] = newChunk;
        }
    }

    private void ExpandWorld() {

        BiomeGeneration.ExpandBiomeMap();

        terrainOffsetZ += NEW_AREA_LENGTH;

        for (int z = 0; z < NEW_AREA_LENGTH; z++) {

            for (int x = 0; x < X_CHUNCK_COUNT; x++) {

                Vector2Int chunkPos = new(x, terrainOffsetZ + Z_CHUNCK_COUNT - NEW_AREA_LENGTH + z);

                int chunkIdx = (z * X_CHUNCK_COUNT + x + chunkArrayStartIndex) % CHUNK_COUNT;

                GameObject currentChunk = Chunks[chunkIdx];

                currentChunk.transform.position = new Vector3(currentChunk.transform.position.x, currentChunk.transform.position.y, chunkPos.y * CHUNK_SIZE * TERRAIN_SCALE);

                SetUpChunk(currentChunk, chunkPos);

            }
        }

        chunkArrayStartIndex = (chunkArrayStartIndex + NEW_AREA_LENGTH * X_CHUNCK_COUNT) % Chunks.GetLength(0);
    }

    //Gibt den Chunk an dem uebergebenen Index zurck
    //Funktioniert auch fuer Index, die groesser sind als das Array (fuer die fortlaufende Welt koennen also auch fortlaufende Indexe benutzt werden)
    private GameObject GetChunk(int index) {

        return Chunks[(index + Chunks.GetLength(0)) % Chunks.GetLength(0)];
    }

    //Aktualisiert das Mesh eines Chunk anhand der Noisefunction mit der neunen Chunkposition
    private void UpdateMesh(GameObject chunk, Vector2Int pos) {

        //Positionen der Mesh-Knotenpunkte
        Vector3[] verticies = new Vector3[(CHUNK_SIZE + 1) * (CHUNK_SIZE + 1)];

        for (int i = 0, z = 0; z <= CHUNK_SIZE; z++) {
            for (int x = 0; x <= CHUNK_SIZE; x++) {

                int xGlobal = pos.x * CHUNK_SIZE + x;
                int zGlobal = pos.y * CHUNK_SIZE + z;

                verticies[i] = new Vector3(x * TERRAIN_SCALE, noiseFunction.Noise(xGlobal, zGlobal) * TERRAIN_SCALE, z * TERRAIN_SCALE);
                i++;
            }
        }

        Mesh mesh = chunk.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = verticies;
        mesh.triangles = TRIANGLES_MESH;
        mesh.RecalculateBounds();
    }

    //Aktualisiert den Collider unterhalb des Spieler anhand der Spielerposition
    private void UpdateTerrainMeshCollider() {

        int oneSideSize = COLLIDER_CHUNK_SIZE / 2;

        int playerChunkIdx = GetChunkIndexFromWorldPosition(Player.position);

        if ((playerChunkIdx % X_CHUNCK_COUNT) + oneSideSize >= X_CHUNCK_COUNT) {
            playerChunkIdx -= (playerChunkIdx + oneSideSize + 1) % X_CHUNCK_COUNT;
        }
        else if ((playerChunkIdx % X_CHUNCK_COUNT) - oneSideSize < 0) {
            playerChunkIdx += X_CHUNCK_COUNT - ((playerChunkIdx - oneSideSize) % X_CHUNCK_COUNT);
        }

        Mesh[,] meshs = SelectColliderMeshes(playerChunkIdx);

        Mesh mesh = CombineColliderMeshes(meshs);
        Collider.GetComponent<MeshCollider>().sharedMesh = mesh;
        Collider.transform.position = GetChunk(playerChunkIdx - oneSideSize - (oneSideSize * X_CHUNCK_COUNT)).transform.position;

    }

    private Mesh[,] SelectColliderMeshes(int playerChunkIdx) {

        int oneSideSize = COLLIDER_CHUNK_SIZE / 2;

        Mesh[,] meshSelection = new Mesh[COLLIDER_CHUNK_SIZE, COLLIDER_CHUNK_SIZE];

        for (int y = 0; y < COLLIDER_CHUNK_SIZE; y++) {
            for (int x = 0; x < COLLIDER_CHUNK_SIZE; x++) {
                meshSelection[y, x] = GetChunk(playerChunkIdx - oneSideSize + x - (oneSideSize * X_CHUNCK_COUNT) + (y * X_CHUNCK_COUNT)).GetComponent<MeshFilter>().mesh;
            }
        }

        return meshSelection;

    }

    //gleichgrosse quadrateische meshs
    private Mesh CombineColliderMeshes(Mesh[,] meshs) {

        int meshSize = (int)Mathf.Sqrt(meshs[0, 0].vertexCount);
        int vertexCountX = meshSize * meshs.GetLength(0) - meshs.GetLength(0) + 1;
        int vertexCountZ = meshSize * meshs.GetLength(1) - meshs.GetLength(0) + 1;

        Vector3[] verticies = new Vector3[vertexCountX * vertexCountZ];

        for (int z = 0; z < meshs.GetLength(0); z++) {
            for (int x = 0; x < meshs.GetLength(1); x++) {

                Mesh currentMesh = meshs[z, x];

                for (int i = 0; i < currentMesh.vertices.Length; i++) {
                    verticies[i % meshSize + (i / meshSize) * vertexCountX + x * (meshSize - 1) + z * vertexCountX * (meshSize - 1)] = currentMesh.vertices[i] + new Vector3(x * (meshSize - 1) * TERRAIN_SCALE, 0, z * (meshSize - 1) * TERRAIN_SCALE);
                }

            }
        }

        Mesh mesh = new();
        mesh.vertices = verticies;
        mesh.triangles = TRIANGLES_COLLIDER;
        mesh.RecalculateBounds();

        return mesh;

    }

    private void SetUpChunk(GameObject chunk, Vector2Int pos) {

        ChunkData chunkData = chunk.GetComponent<ChunkData>();
        chunkData.Biome = BiomeGeneration.GetBiome(new Vector2Int(pos.x, pos.y - terrainOffsetZ));

        chunk.GetComponent<Renderer>().material = chunkData.Biome.DebugMaterial;
        chunk.name = "Chunk r" + pos.y + " p" + pos.x;

        UpdateMesh(chunk, pos);
    }

    private int GetChunkIndexFromWorldPosition(Vector3 worldPos) {

        Vector2Int pos = new((int)worldPos.x / (CHUNK_SIZE * TERRAIN_SCALE), (int)worldPos.z / (CHUNK_SIZE * TERRAIN_SCALE));

        return (pos.y % Z_CHUNCK_COUNT) * X_CHUNCK_COUNT + pos.x;

    }

    private int[] CreateMeshTriangleArray(int chunkCount) {

        int vertexCountPerSide = chunkCount * CHUNK_SIZE;

        //Berechnung der Dreiecke vom Mesh
        int[] triangles = new int[vertexCountPerSide * vertexCountPerSide * 6];

        int vert = 0;
        int current = 0;

        //Berechnung der Punkte der Dreicke (6 Punkte: 3 Punkte pro Dreieck, 2 Dreiecke pro Kachel)
        for (int z = 0; z < vertexCountPerSide; z++) {
            for (int x = 0; x < vertexCountPerSide; x++) {

                triangles[current] = vert;
                triangles[current + 1] = vert + vertexCountPerSide + 1;
                triangles[current + 2] = vert + vertexCountPerSide + 2;

                triangles[current + 5] = vert;
                triangles[current + 3] = vert + vertexCountPerSide + 2;
                triangles[current + 4] = vert + 1;

                vert++;
                current += 6;
            }
            vert++;
        }
        return triangles;
    }



}
