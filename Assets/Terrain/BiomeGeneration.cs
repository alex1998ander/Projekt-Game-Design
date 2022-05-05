using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BiomeGeneration : MonoBehaviour {

    int[,] BiomeMap = new int[,] { { 0, 0, 1 }, { 2, 3, 4 }, { 5, 2, 6 } };

    public void Start() {
        StartCoroutine(InitializeBiomeMap());
    }

    public IEnumerator InitializeBiomeMap() {

        //for (int y = 0; y < BiomeMap.GetLength(0); y++) {
        //    for (int x = 0; x < BiomeMap.GetLength(0); x++) {

        //        BiomeMap[y, x] = Random.Range(0, 7);
        //    }
        //}

        DrawDensityMap();
        yield return new WaitForSeconds(1);
        ZoomInBiomeMap();
        DrawDensityMap();
        yield return new WaitForSeconds(1);
        ZoomInBiomeMap();
        DrawDensityMap();
        yield return new WaitForSeconds(1);
        ZoomInBiomeMap();
        DrawDensityMap();
        yield return new WaitForSeconds(1);
        ZoomInBiomeMap();
        DrawDensityMap();
        yield return new WaitForSeconds(1);
        ZoomInBiomeMap();
        DrawDensityMap();
        yield return new WaitForSeconds(1);
        SmoothBiomeMap();
        DrawDensityMap();
        yield return new WaitForSeconds(1);
        SmoothBiomeMap();
        DrawDensityMap();
        yield return new WaitForSeconds(1);
        SmoothBiomeMap();
        DrawDensityMap();
    }

    public void DrawDensityMap() {

        Color32[] colors = new Color32[BiomeMap.GetLength(0) * BiomeMap.GetLength(0)];

        for (int i = 0, y = 0; y < BiomeMap.GetLength(0); y++) {
            for (int x = 0; x < BiomeMap.GetLength(0); x++) {

                colors[i++] = GetColor(BiomeMap[y, x]);

            }
        }

        Texture2D texture = new(BiomeMap.GetLength(0), BiomeMap.GetLength(0));
        texture.SetPixels32(colors);
        texture.filterMode = FilterMode.Point;
        texture.Apply();

        Renderer r = GetComponent<Renderer>();
        r.material.mainTexture = texture;


    }

    private Color GetColor(int value) {

        switch (value) {
            case 0: return Color.blue;
            case 1: return Color.yellow;
            case 2: return Color.green;
            case 3: return Color.white;
            case 4: return Color.black;
            case 5: return Color.red;
            case 6: return Color.cyan;
            default: return Color.gray;
        }

    }

    private void ZoomInBiomeMap() {

        int size = BiomeMap.GetLength(0) * 2 - 1;

        int[,] zoomedBiomeMap = new int[size, size];

        for (int y = 0; y < BiomeMap.GetLength(0) - 1; y++) {
            for (int x = 0; x < BiomeMap.GetLength(0) - 1; x++) {

                zoomedBiomeMap[y * 2, x * 2] = BiomeMap[y, x];
                zoomedBiomeMap[y * 2 + 1, x * 2] = ChooseRandom(BiomeMap[y, x], BiomeMap[y + 1, x]);
                zoomedBiomeMap[y * 2, x * 2 + 1] = ChooseRandom(BiomeMap[y, x], BiomeMap[y, x + 1]);

                //if (Random.Range(0, 2) == 0) ZoomedBiomeMap[y * 2 + 1, x * 2 + 2] = BiomeMap[y, x + 1];
                //else ZoomedBiomeMap[y * 2 + 1, x * 2] = BiomeMap[y + 1, x + 1];

                //if (Random.Range(0, 2) == 0) ZoomedBiomeMap[y * 2 + 2, x * 2 + 1] = BiomeMap[y + 1, x];
                //else ZoomedBiomeMap[y * 2 + 1, x * 2] = BiomeMap[y + 1, x + 1];

                zoomedBiomeMap[y * 2 + 1, x * 2 + 1] = ChooseRandom(BiomeMap[y, x], BiomeMap[y, x + 1], BiomeMap[y + 1, x], BiomeMap[y + 1, x + 1]);

            }

            int _x = BiomeMap.GetLength(0) - 1;

            zoomedBiomeMap[y * 2, _x * 2] = BiomeMap[y, _x];
            zoomedBiomeMap[y * 2 + 1, _x * 2] = ChooseRandom(BiomeMap[y, _x], BiomeMap[y + 1, _x]);

        }

        int _y = BiomeMap.GetLength(0) - 1;

        for (int x = 0; x < BiomeMap.GetLength(0) - 1; x++) {

            zoomedBiomeMap[_y * 2, x * 2] = BiomeMap[_y, x];
            zoomedBiomeMap[_y * 2, x * 2 + 1] = ChooseRandom(BiomeMap[_y, x], BiomeMap[_y, x + 1]);
        }

        zoomedBiomeMap[_y * 2, _y * 2] = BiomeMap[_y, _y];

        BiomeMap = zoomedBiomeMap;

    }

    private void SmoothBiomeMap() {

        int size = BiomeMap.GetLength(0) - 2;

        int[,] smoothedBiomeMap = new int[size, size];

        for (int y = 1; y <= size; y++) {
            for (int x = 1; x <= size; x++) {

                int above = BiomeMap[y - 1, x], below = BiomeMap[y + 1, x], left = BiomeMap[y, x - 1], right = BiomeMap[y, x + 1];

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
                    smoothedBiomeMap[y - 1, x - 1] = BiomeMap[y, x];
                }
            }
        }
        BiomeMap = smoothedBiomeMap;
    }

    private int ChooseRandom(params int[] choices) {

        int[] distinctChoises = choices.Distinct().ToArray();

        return distinctChoises[Random.Range(0, distinctChoises.Length)];

    }

    private string BiomeMapToString() {

        string result = "";

        for (int y = 0; y < BiomeMap.GetLength(0); y++) {
            for (int x = 0; x < BiomeMap.GetLength(0); x++) {

                result += BiomeMap[y, x];
                result += " ";
            }
            result += "\n";
        }

        return result;
    }

}
