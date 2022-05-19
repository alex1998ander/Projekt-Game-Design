using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombineMesh : MonoBehaviour
{
    public string findTag;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            CombineMeshes();
            Debug.Log("Combined");
        }
    }

    private void CombineMeshes()
    {
        GameObject[] test = GameObject.FindGameObjectsWithTag(findTag);
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


        transform.localScale = new Vector3(1, 1, 1);
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;
    }
}
