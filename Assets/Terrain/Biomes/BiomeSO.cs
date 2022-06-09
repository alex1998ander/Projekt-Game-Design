using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//#if UNITY_EDITOR
//using UnityEditor;
//# endif

[CreateAssetMenu(fileName = "Biome_", menuName = "Procedural Generation/Biome Configuration")]
public class BiomeSO : ScriptableObject {

    public string Name;

    public int Weight;

    [SerializeField] private AnimationCurve spline = AnimationCurve.Linear(0, 1, 1, 1);

    public Material DebugMaterial;

    public float GetSplinePoint(float x) {
        return spline.Evaluate(x);
    }

    //    #region Editor
    //#if UNITY_EDITOR

    //    [CustomEditor(typeof(BiomeSO))]
    //    public class BiomeSOEditorGUI : Editor {

    //        public override void OnInspectorGUI() {
    //            base.OnInspectorGUI();

    //            BiomeSO biomeSO = (BiomeSO)target;

    //            biomeSO.curve = EditorGUILayout.CurveField("test", biomeSO.curve);

    //        }

    //    }

    //#endif
    //    #endregion
}
