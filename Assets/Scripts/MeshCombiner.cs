using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCombiner : MonoBehaviour
{
    private void Awake()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];

        Matrix4x4 myTransform = transform.worldToLocalMatrix;
        int index = 0;
        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.gameObject == gameObject)
                continue;

            if (mf.sharedMesh == null)
                continue;

            combine[index].mesh = mf.sharedMesh;
            combine[index].transform = myTransform * mf.transform.localToWorldMatrix;
            index++;

            mf.gameObject.SetActive(false);
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();

        meshFilter.mesh.CombineMeshes(combine, true, true);

        gameObject.SetActive(true);
    }
}