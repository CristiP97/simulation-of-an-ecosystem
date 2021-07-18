using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CombineMeshes : MonoBehaviour
{
    public void Combine()
    {
        MeshFilter[] initialMeshFilters = GetComponentsInChildren<MeshFilter>();
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        int i;

        for (i = 1; i < initialMeshFilters.Length; ++i)
        {
            meshFilters.Add(initialMeshFilters[i]);
        }

        Debug.Log(meshFilters.Count);
        CombineInstance[] combine = new CombineInstance[meshFilters.Count];

        i = 0;
        while (i < meshFilters.Count)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        transform.gameObject.SetActive(true);

        //transform.localScale = new Vector3(1, 1, 1);
        //transform.rotation = Quaternion.identity;
        //transform.position = Vector3.zero;
    }
}
