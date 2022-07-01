using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
# endif

[System.Serializable]
public class TerrainHandler : MonoBehaviour {

    #region Konstanten
    //Vertexanzahl pro Chunkseite
    public const int CHUNK_SIZE = 10;
    //Chunkanzahl pro Seite
    private static int X_CHUNCK_COUNT = 25;
    private static int Z_CHUNCK_COUNT = 50;
    //Chunkanzahl gesamt
    private static int CHUNK_COUNT;
    //Terrainskalierung
    public const int TERRAIN_SCALE = 10;

    //Dreiecksanordnung der vertices eines Meshes
    private static int[] TRIANGLES_MESH;
    private static int[] TRIANGLES_COLLIDER;

    //Collidergroesse unterhalb des Spielers (MUSS UNGERADE sein, damit der Spieler in der Mitte bleibt)
    private const int COLLIDER_CHUNK_SIZE = 3;
    #endregion

    #region Variablen
    //Chunk-Array
    private GameObject[] Chunks { get; set; }
    //Spielerposition
    [SerializeField] private Transform Player;
    //Collider fuer den Untergrund in der Umgebung des Spielers
    [SerializeField] private GameObject Collider;

    [SerializeField] private BiomeSO biome_forest;
    [SerializeField] private BiomeSO biome_frozen_lake;
    [SerializeField] private BiomeSO biome_isolated_trees;
    [SerializeField] private BiomeSO biome_open_space;
    [SerializeField] private BiomeSO biome_ski_slope;
    [SerializeField] private BiomeSO biome_village;

    [SerializeField] private BiomeSO[,,] biomeTable;


    //World-Koordinate des Terrainanfangs
    private int terrainOffsetZ = 0;

    //Aktueller Index im Chunk-Array an dem sich der Spieler befindet
    private int currentPlayerChunkIdx = -1;

    //Singleton
    public static TerrainHandler Instance;

    //ChunkPrefab
    [SerializeField] private GameObject chunkPrefab;
    #endregion

    public void Awake() {
        Instance = this;

        CHUNK_COUNT = X_CHUNCK_COUNT * Z_CHUNCK_COUNT;

        TRIANGLES_MESH = CreateMeshTriangleArray(1);
        TRIANGLES_COLLIDER = CreateMeshTriangleArray(COLLIDER_CHUNK_SIZE);

        Chunks = new GameObject[CHUNK_COUNT];

        biomeTable = new BiomeSO[,,] {
            {
                {biome_open_space, biome_isolated_trees},
                {biome_open_space, biome_isolated_trees},
                {biome_open_space, biome_isolated_trees},
                {biome_open_space, biome_isolated_trees}
            },
            {
                {biome_forest, biome_isolated_trees},
                {biome_forest, biome_isolated_trees},
                {biome_forest, biome_isolated_trees},
                {biome_open_space, biome_isolated_trees}
            },
            {
                {biome_frozen_lake, null},
                {biome_forest, biome_open_space},
                {biome_ski_slope, biome_open_space},
                {biome_ski_slope, biome_open_space}
            },
            {
                {biome_village, null },
                {biome_forest, biome_open_space},
                {biome_ski_slope, biome_open_space},
                {biome_ski_slope, biome_open_space}
            }
        };

        biome_forest.Init();
        biome_frozen_lake.Init();
        biome_isolated_trees.Init();
        biome_open_space.Init();
        biome_ski_slope.Init();
        biome_village.Init();

    }

    public void Start() {
        InitializeChunks();
    }


    public void Update() {

        if (Player.position.z > (terrainOffsetZ + 20) * CHUNK_SIZE * TERRAIN_SCALE) {
            ExpandWorld();
        }

        int debug_x = (int)Player.transform.position.x / TERRAIN_SCALE;
        int debug_z = (int)Player.transform.position.z / TERRAIN_SCALE;

        float fluctuation = NoiseHandler.FluctuationNoise(debug_x, debug_z);
        float erosion = NoiseHandler.ErosionNoise(debug_x, debug_z);
        float gradient = NoiseHandler.GradientNoise(debug_x, debug_z);
        float gradient_slope = NoiseHandler.Instance.GetGradientSlopeZ(debug_x, debug_z);

        //Debug.Log("Fluctuation: " + fluctuation
        //    + "\nErosion: " + erosion
        //    + "\n=> " + (fluctuation * erosion)
        //    + "\nGradient: " + gradient
        //    + "\nSteigung: " + gradient_slope
        //    + "\nHöhe: " + GetHeight(new Vector2Int((int)Player.position.x, (int)Player.position.z)));

        int chunkIndex = GetChunkIndexFromWorldPosition(Player.transform.position);

        if (chunkIndex != currentPlayerChunkIdx) {
            UpdateTerrainMeshCollider();
            currentPlayerChunkIdx = chunkIndex;
        }
    }

    #region Chunk
    //Initialisert alle Chunks
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

    //Erweitert die Welt, indem Chunk vom hinteren Ende ans Vordere gesetzt und aktualisiert werden
    private void ExpandWorld() {
        for (int x = 0; x < X_CHUNCK_COUNT; x++) {

            Vector2Int chunkPos = new(x, terrainOffsetZ + Z_CHUNCK_COUNT);

            int chunkIdx = (terrainOffsetZ * X_CHUNCK_COUNT + x) % CHUNK_COUNT;

            GameObject currentChunk = Chunks[chunkIdx];

            //Loescht alle Kindobjekte
            foreach (Transform child in currentChunk.transform) {
                Destroy(child.gameObject);
            }

            currentChunk.transform.position = new Vector3(currentChunk.transform.position.x, currentChunk.transform.position.y, chunkPos.y * CHUNK_SIZE * TERRAIN_SCALE);

            SetUpChunk(currentChunk, chunkPos);

        }

        terrainOffsetZ++;
    }

    //Aktualisiert den Chunk passend zur uebergebenen Position
    private void SetUpChunk(GameObject chunk, Vector2Int pos) {

        ChunkData chunkData = chunk.GetComponent<ChunkData>();
        chunkData.Biome = DetermineBiome(pos);
        chunk.GetComponent<Renderer>().material = chunkData.Biome.DebugMaterial;
        chunkData.Preset = ObstaclePlacement.Instance.SelectRandomPlacementPreset();
        chunk.name = "Chunk r" + pos.y + " p" + pos.x;

        //Aktualisiert das Mesh eines Chunk anhand der Noisefunction mit der neunen Chunkposition
        //Positionen der Mesh-Knotenpunkte
        Vector3[] vertices = new Vector3[(CHUNK_SIZE + 1) * (CHUNK_SIZE + 1)];

        for (int i = 0, z = 0; z <= CHUNK_SIZE; z++) {
            for (int x = 0; x <= CHUNK_SIZE; x++) {

                int xGlobal = pos.x * CHUNK_SIZE + x;
                int zGlobal = pos.y * CHUNK_SIZE + z;

                vertices[i] = new Vector3(x * TERRAIN_SCALE, NoiseHandler.Instance.Noise(xGlobal, zGlobal) * TERRAIN_SCALE, z * TERRAIN_SCALE);
                i++;
            }
        }

        Mesh mesh = chunk.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = TRIANGLES_MESH;

        ObstaclePlacement.Instance.SpawnObstacles(chunkData);
    }

    private BiomeSO DetermineBiome(Vector2Int pos) {

        int xGlobal = pos.x * CHUNK_SIZE;
        int zGlobal = pos.y * CHUNK_SIZE;

        float slope = NoiseHandler.Instance.GetGradientSlopeZ(xGlobal, zGlobal);
        float erosion = NoiseHandler.ErosionNoise(xGlobal, zGlobal);

        int slopeIndex;
        int erosionIndex;

        if (slope < 0.5f)
            slopeIndex = 0;
        else if (slope < 1.0f)
            slopeIndex = 1;
        else if (slope < 1.5f)
            slopeIndex = 2;
        else
            slopeIndex = 3;

        if (erosion < 0.25f)
            erosionIndex = 0;
        else if (erosion < 0.5f)
            erosionIndex = 1;
        else if (erosion < 0.75f)
            erosionIndex = 2;
        else
            erosionIndex = 3;

        return biomeTable[erosionIndex, slopeIndex, 0];
    }

    #endregion

    #region Collider
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
        mesh.RecalculateNormals();
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

        Vector3[] vertices = new Vector3[vertexCountX * vertexCountZ];

        for (int z = 0; z < meshs.GetLength(0); z++) {
            for (int x = 0; x < meshs.GetLength(1); x++) {

                Mesh currentMesh = meshs[z, x];

                for (int i = 0; i < currentMesh.vertices.Length; i++) {
                    vertices[i % meshSize + (i / meshSize) * vertexCountX + x * (meshSize - 1) + z * vertexCountX * (meshSize - 1)] = currentMesh.vertices[i] + new Vector3(x * (meshSize - 1) * TERRAIN_SCALE, 0, z * (meshSize - 1) * TERRAIN_SCALE);
                }

            }
        }

        Mesh mesh = new();
        mesh.vertices = vertices;
        mesh.triangles = TRIANGLES_COLLIDER;

        return mesh;

    }

    #endregion

    #region Getter
    //Gibt den Chunk an dem uebergebenen Index zurueck
    //Funktioniert auch fuer Index, die groesser sind als das Array (fuer die fortlaufende Welt koennen also auch fortlaufende Indexe benutzt werden)
    private GameObject GetChunk(int index) {

        return Chunks[(index + Chunks.GetLength(0)) % Chunks.GetLength(0)];
    }

    //Gibt den Index des Chunks an der uebergebenen Position im Chunk-Array an
    private int GetChunkIndexFromWorldPosition(Vector3 worldPos) {

        Vector2Int pos = new((int)worldPos.x / (CHUNK_SIZE * TERRAIN_SCALE), (int)worldPos.z / (CHUNK_SIZE * TERRAIN_SCALE));

        return (pos.y % Z_CHUNCK_COUNT) * X_CHUNCK_COUNT + pos.x;

    }

    #endregion

    #region Util
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
    #endregion

    #region Editor
    //#if UNITY_EDITOR

    //    [CustomEditor(typeof(TerrainHandler))]
    //    public class BiomeTableEditor : Editor {

    //        bool showBiomeTable = true;
    //        SerializedProperty p_biomeTable;

    //        private void OnEnable() {
    //            p_biomeTable = serializedObject.FindProperty("biomeTable");
    //        }


    //        public override void OnInspectorGUI() {
    //            base.OnInspectorGUI();

    //            TerrainHandler terrainHandler = (TerrainHandler)target;

    //            System.Type biomeType = typeof(BiomeSO);
    //            showBiomeTable = EditorGUILayout.Foldout(showBiomeTable, "Biome Table");

    //            if (showBiomeTable) {
    //                GUILayout.BeginHorizontal();
    //                GUILayout.BeginVertical();
    //                GUILayout.Label(" ");
    //                GUILayout.Label("E □□□");
    //                GUILayout.Label(" ");
    //                GUILayout.Space(5);
    //                GUILayout.Label("E ■□□");
    //                GUILayout.Label(" ");
    //                GUILayout.Space(5);
    //                GUILayout.Label("E ■■□");
    //                GUILayout.Label(" ");
    //                GUILayout.Space(5);
    //                GUILayout.Label("E ■■■");
    //                GUILayout.EndVertical();

    //                GUILayout.BeginVertical();
    //                GUILayout.Label("S □□□");
    //                for (int i = 0; i < 4; i++) {
    //                    terrainHandler.biomeTable[i, 0, 0] = (BiomeSO)EditorGUILayout.ObjectField(terrainHandler.biomeTable[i, 0, 0], biomeType, false);
    //                    terrainHandler.biomeTable[i, 0, 1] = (BiomeSO)EditorGUILayout.ObjectField(terrainHandler.biomeTable[i, 0, 1], biomeType, false);

    //                   // EditorGUILayout.PropertyField(p_biomeTable);
    //                    GUILayout.Space(5);
    //                }
    //                GUILayout.EndVertical();

    //                GUILayout.BeginVertical();
    //                GUILayout.Label("S ■□□");
    //                for (int i = 0; i < 4; i++) {
    //                    terrainHandler.biomeTable[i, 1, 0] = (BiomeSO)EditorGUILayout.ObjectField(terrainHandler.biomeTable[i, 1, 0], biomeType, false);
    //                    terrainHandler.biomeTable[i, 1, 1] = (BiomeSO)EditorGUILayout.ObjectField(terrainHandler.biomeTable[i, 1, 1], biomeType, false);
    //                    GUILayout.Space(5);
    //                }
    //                GUILayout.EndVertical();

    //                GUILayout.BeginVertical();
    //                GUILayout.Label("S ■■□");
    //                for (int i = 0; i < 4; i++) {
    //                    terrainHandler.biomeTable[i, 2, 0] = (BiomeSO)EditorGUILayout.ObjectField(terrainHandler.biomeTable[i, 2, 0], biomeType, false);
    //                    terrainHandler.biomeTable[i, 2, 1] = (BiomeSO)EditorGUILayout.ObjectField(terrainHandler.biomeTable[i, 2, 1], biomeType, false);
    //                    GUILayout.Space(5);
    //                }
    //                GUILayout.EndVertical();

    //                GUILayout.BeginVertical();
    //                GUILayout.Label("S ■■■");
    //                for (int i = 0; i < 4; i++) {
    //                    terrainHandler.biomeTable[i, 3, 0] = (BiomeSO)EditorGUILayout.ObjectField(terrainHandler.biomeTable[i, 3, 0], biomeType, false);
    //                    terrainHandler.biomeTable[i, 3, 1] = (BiomeSO)EditorGUILayout.ObjectField(terrainHandler.biomeTable[i, 3, 1], biomeType, false);
    //                    GUILayout.Space(5);
    //                }
    //                GUILayout.EndVertical();

    //                GUILayout.EndHorizontal();
    //            }
    //        }
    //    }

    //#endif
    #endregion

}
