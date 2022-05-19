using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(MeshFilter))]
//[RequireComponent(typeof(MeshRenderer))]

public class TreeGeneration : MonoBehaviour
{
    public List<GameObject> trees = new();

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.J))
        //{
        //    CombineMesh();
        //    Debug.Log("Combined");
        //}
    }

    public GameObject GetTree2(int index)
    {
        return trees[index];
    }

    private void CombineMesh()
    {
        GameObject[] test = GameObject.FindGameObjectsWithTag("Trunk");
        //MeshFilter[] meshFilters = GameObject.FindGameObjectsWithTag("Trunk").GetComponents<MeshFilter>();
        MeshFilter[] meshFilters = new MeshFilter[test.Length];
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int j = 0; j < test.Length; j++)
        {
            meshFilters[j] = test[j].GetComponent<MeshFilter>();
        }

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }
        var meshFilter = transform.GetComponent<MeshFilter>();
        //meshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.mesh.CombineMeshes(combine);
        GetComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
        transform.gameObject.SetActive(true);


        //transform.localScale = new Vector3(1, 1, 1);
        //transform.rotation = Quaternion.identity;
        //transform.position = Vector3.zero;
    }
}
