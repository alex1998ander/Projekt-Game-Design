using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//#if UNITY_EDITOR
//using UnityEditor;
//# endif

[CreateAssetMenu(fileName = "Biome_", menuName = "Procedural Generation/Biome Configuration")]
public class BiomeSO : ScriptableObject {

    public string Name;

    [Range(0, 100)] public int Weight;

    public Material DebugMaterial;
}
