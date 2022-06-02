using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BiomeGeneration : MonoBehaviour {

    //Liste aller Biome (SO im Editor)
    [SerializeField] private List<BiomeSO> biomes = new();

    //Array zur gewichteten Zufallsauswahl der Biome
    static byte[] WEIGHTED_BIOMES;

    //Array mit Biom-IDs
    private static byte[,] biomeMap = new byte[,] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 2, 0 }, { 2, 2, 2 } };

    //Geglättetes Array
    private static byte[,] smoothedBiomeMap;

    //Arrays der Entwicklungsgenerationen (Stages)
    private static byte[,] fstStage = new byte[3, 3];
    private static byte[,] sndStage = new byte[3, 3];
    private static byte[,] trdStage = new byte[3, 3];
    private static byte[,] forStage = new byte[3, 3];
    private static byte[,] fifStage = new byte[3, 3];

    //Singelton
    public static BiomeGeneration Instance;

    public void Awake() {
        //Zuweisung Singleton
        Instance = this;

        #region Initialisierung eines Arrays zur gewichteten Zufallsauswahl

        //Summe aller Gewichte
        int totalWeight = Instance.biomes.Sum(x => x.Weight);

        //Array mit den Biom-IDs (Anzahl der jeweiligen IDs entsprechen den Wahrscheinlichkeiten)
        WEIGHTED_BIOMES = new byte[totalWeight];

        int idx = 0;

        //Zuweisung der Biome in gewichteter Quantität
        //Je Biome
        for (byte biomeId = 0; biomeId < Instance.biomes.Count; biomeId++) {

            BiomeSO biome = Instance.biomes[biomeId];

            //Wiederholt aufeinanderfolgende gleiche Biomezuweisung je Anzahl
            for (int i = 0; i < biome.Weight; i++) {
                WEIGHTED_BIOMES[idx + i] = biomeId;
            }

            idx += biome.Weight;
        }
        #endregion

    }

    //Setzt alle Arrays Initialzustand (erste BiomeMap, Stages für die Erweiterungen der BiomeMap, SmoothedBiomeMap)
    public static void Init() {
        //Zufällige Füllung der Stages
        InitializeBiomeMap(ref fstStage);
        InitializeBiomeMap(ref sndStage);
        InitializeBiomeMap(ref trdStage);
        InitializeBiomeMap(ref forStage);
        InitializeBiomeMap(ref fifStage);

        //erste Generation
        ZoomInBiomeMap(ref biomeMap);
        ZoomInBiomeMap(ref fstStage);
        ReplaceBiomesInExpansion(ref biomeMap, ref fstStage);
        ZoomInBiomeMap(ref sndStage);
        ReplaceBiomesInExpansion(ref fstStage, ref sndStage);
        ZoomInBiomeMap(ref trdStage);
        ReplaceBiomesInExpansion(ref sndStage, ref trdStage);
        ZoomInBiomeMap(ref forStage);
        ReplaceBiomesInExpansion(ref trdStage, ref forStage);

        //zweite Generation
        ZoomInBiomeMap(ref biomeMap);
        ZoomInBiomeMap(ref fstStage);
        ReplaceBiomesInExpansion(ref biomeMap, ref fstStage);
        ZoomInBiomeMap(ref sndStage);
        ReplaceBiomesInExpansion(ref fstStage, ref sndStage);
        ZoomInBiomeMap(ref trdStage);
        ReplaceBiomesInExpansion(ref sndStage, ref trdStage);

        //dritte Generation
        ZoomInBiomeMap(ref biomeMap);
        ZoomInBiomeMap(ref fstStage);
        ReplaceBiomesInExpansion(ref biomeMap, ref fstStage);
        ZoomInBiomeMap(ref sndStage);
        ReplaceBiomesInExpansion(ref fstStage, ref sndStage);

        //vierte Generation
        ZoomInBiomeMap(ref biomeMap);
        ZoomInBiomeMap(ref fstStage);
        ReplaceBiomesInExpansion(ref biomeMap, ref fstStage);

        smoothedBiomeMap = biomeMap;

        //Glättung
        SmoothBiomeMap(ref smoothedBiomeMap);
        SmoothBiomeMap(ref smoothedBiomeMap);
        SmoothBiomeMap(ref smoothedBiomeMap);

        //Debug
        Instance.Debug_DrawDensityMap(smoothedBiomeMap);

    }

    //Weist dem angegebenen Array zufällige Werte zu und achtet dabei auf die Vorkommenswahrscheinlichkeiten der einzelnen Biome
    private static void InitializeBiomeMap(ref byte[,] _biomeMap) {

        for (int y = 0; y < _biomeMap.GetLength(0); y++) {
            for (int x = 0; x < _biomeMap.GetLength(1); x++) {

                _biomeMap[y, x] = WEIGHTED_BIOMES[Random.Range(0, WEIGHTED_BIOMES.GetLength(0))];
            }
        }
    }

    public static BiomeSO GetBiome(Vector2Int pos) {

        int biomeId = smoothedBiomeMap[pos.y, pos.x];
        return Instance.biomes[biomeId];
    }

    public static int GetWorldWidth() {
        return smoothedBiomeMap.GetLength(1);
    }
    public static int GetWorldLength() {
        return smoothedBiomeMap.GetLength(0);
    }

    //Debug     Zeichnet das Array gefärbt auf eine Plane
    private void Debug_DrawDensityMap(byte[,] _biomeMap) {

        Color32[] colors = new Color32[_biomeMap.GetLength(0) * _biomeMap.GetLength(1)];

        for (int i = 0, y = 0; y < _biomeMap.GetLength(0); y++) {
            for (int x = 0; x < _biomeMap.GetLength(1); x++) {

                colors[i++] = GetColor(_biomeMap[y, x]);

            }
        }

        Texture2D texture = new(_biomeMap.GetLength(1), _biomeMap.GetLength(0));
        texture.SetPixels32(colors);
        texture.filterMode = FilterMode.Point;
        texture.Apply();

        Renderer r = GetComponent<Renderer>();
        r.material.mainTexture = texture;

    }

    //Debug
    private Color GetColor(int value) {

        switch (value) {
            case 0: return Color.white;
            case 1: return Color.cyan;
            case 2: return Color.green;
            case 3: return Color.red;

            default: return Color.gray;
        }

    }

    //Erhöht die Auflösung der übergebenen BiomeMap
    private static void ZoomInBiomeMap(ref byte[,] _biomeMap) {

        //Neue BiomeMap
        byte[,] zoomedBiomeMap = new byte[_biomeMap.GetLength(0) * 2 - 1, _biomeMap.GetLength(1) * 2 - 1];

        //Für jedes Feld
        for (int y = 0; y < _biomeMap.GetLength(0) - 1; y++) {
            for (int x = 0; x < _biomeMap.GetLength(1) - 1; x++) {

                //Jeder zweite Werte wird übernommen
                zoomedBiomeMap[y * 2, x * 2] = _biomeMap[y, x];
                //Zufällige Auswahl der übrigen Felder
                zoomedBiomeMap[y * 2 + 1, x * 2] = ChooseRandom(_biomeMap[y, x], _biomeMap[y + 1, x]);
                zoomedBiomeMap[y * 2, x * 2 + 1] = ChooseRandom(_biomeMap[y, x], _biomeMap[y, x + 1]);
                zoomedBiomeMap[y * 2 + 1, x * 2 + 1] = ChooseRandom(_biomeMap[y, x], _biomeMap[y, x + 1], _biomeMap[y + 1, x], _biomeMap[y + 1, x + 1]);

            }
            //Randfall x
            int _x = _biomeMap.GetLength(1) - 1;
            zoomedBiomeMap[y * 2, _x * 2] = _biomeMap[y, _x];
            zoomedBiomeMap[y * 2 + 1, _x * 2] = ChooseRandom(_biomeMap[y, _x], _biomeMap[y + 1, _x]);

        }

        //Randfall y
        int _y = _biomeMap.GetLength(0) - 1;
        for (int x = 0; x < _biomeMap.GetLength(1) - 1; x++) {
            zoomedBiomeMap[_y * 2, x * 2] = _biomeMap[_y, x];
            zoomedBiomeMap[_y * 2, x * 2 + 1] = ChooseRandom(_biomeMap[_y, x], _biomeMap[_y, x + 1]);
        }
        zoomedBiomeMap[_y * 2, _biomeMap.GetLength(1) - 1] = _biomeMap[_y, _biomeMap.GetLength(1) - 1];

        //Zuweisung der neuen BiomeMap
        _biomeMap = zoomedBiomeMap;

    }

    //Ersetzt zuvor generierte Werte einer übergebenen BiomeMap in einer zweiten übergebene BiomeMap
    //(für einen besseren Übergang)
    private static void ReplaceBiomesInExpansion(ref byte[,] _biomeMap, ref byte[,] expansionMap) {

        int size = _biomeMap.GetLength(1);
        int replacementLength = size / 3 + 2;

        if (expansionMap.GetLength(1) == size) {

            for (int y = 0; y < replacementLength; y++) {
                for (int x = 0; x < size; x++) {
                    expansionMap[y, x] = _biomeMap[_biomeMap.GetLength(0) - replacementLength + y, x];
                }
            }

        }
    }

    //Glättet das übergebene Array
    private static void SmoothBiomeMap(ref byte[,] _biomeMap) {

        int sizeX = _biomeMap.GetLength(1) - 2;
        int sizeY = _biomeMap.GetLength(0) - 2;

        //Neue BiomeMap
        byte[,] smoothedBiomeMap = new byte[sizeY, sizeX];

        //Für jedes Feld
        for (int y = 1; y <= sizeY; y++) {
            for (int x = 1; x <= sizeX; x++) {

                //Angrenzende Werte
                byte above = _biomeMap[y - 1, x], below = _biomeMap[y + 1, x], left = _biomeMap[y, x - 1], right = _biomeMap[y, x + 1];

                //Glättung durch Ersetzung anhand der anliegenden Felder
                if (above == below && left == right) {
                    smoothedBiomeMap[y - 1, x - 1] = ChooseRandom(above, left);
                }
                else if (above == below) {
                    smoothedBiomeMap[y - 1, x - 1] = above;
                }
                else if (left == right) {
                    smoothedBiomeMap[y - 1, x - 1] = left;
                }
                else {
                    smoothedBiomeMap[y - 1, x - 1] = _biomeMap[y, x];
                }
            }
        }

        //Zuweisung der neuen BiomeMap
        _biomeMap = smoothedBiomeMap;
    }

    //Erweitert die aktuelle BiomeMap
    public static void ExpandBiomeMap() {

        byte[,] sixStage = new byte[3, 3];
        InitializeBiomeMap(ref sixStage);

        ZoomInBiomeMap(ref fifStage);
        ReplaceBiomesInExpansion(ref forStage, ref fifStage);

        ZoomInBiomeMap(ref forStage);
        ReplaceBiomesInExpansion(ref trdStage, ref forStage);

        ZoomInBiomeMap(ref trdStage);
        ReplaceBiomesInExpansion(ref sndStage, ref trdStage);

        ZoomInBiomeMap(ref sndStage);
        ReplaceBiomesInExpansion(ref fstStage, ref sndStage);


        int sizeX = biomeMap.GetLength(1);
        int overlapLength = sizeX / 3;
        int newAreaLength = fstStage.GetLength(0) - overlapLength;

        int sizeY = biomeMap.GetLength(0) - newAreaLength;


        for (int y = 0; y < sizeY; y++) {
            for (int x = 0; x < sizeX; x++) {
                biomeMap[y, x] = biomeMap[y + newAreaLength, x];
            }
        }

        for (int y = 0; y < newAreaLength; y++) {
            for (int x = 0; x < sizeX; x++) {
                biomeMap[y + sizeY, x] = fstStage[y + overlapLength, x];
            }
        }


        fstStage = sndStage;
        sndStage = trdStage;
        trdStage = forStage;
        forStage = fifStage;
        fifStage = sixStage;

        smoothedBiomeMap = biomeMap;

        SmoothBiomeMap(ref smoothedBiomeMap);
        SmoothBiomeMap(ref smoothedBiomeMap);
        SmoothBiomeMap(ref smoothedBiomeMap);

        Instance.Debug_DrawDensityMap(smoothedBiomeMap);
    }

    private static byte ChooseRandom(params byte[] choices) {

        byte[] distinctChoises = choices.Distinct().ToArray();

        return distinctChoises[Random.Range(0, distinctChoises.Length)];

    }

    private static string BiomeMapToString(byte[,] _biomeMap) {

        string result = "";

        for (int y = 0; y < _biomeMap.GetLength(0); y++) {
            for (int x = 0; x < _biomeMap.GetLength(1); x++) {

                result += _biomeMap[y, x];
                result += " ";
            }
            result += "\n";
        }

        return result;
    }

}
