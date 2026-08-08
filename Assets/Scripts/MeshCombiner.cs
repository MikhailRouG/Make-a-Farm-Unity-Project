using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCombiner : MonoBehaviour
{
    private void Awake()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        Matrix4x4 myTransform = transform.worldToLocalMatrix;

        // Built as a list rather than sized meshFilters.Length - 1: children
        // without a sharedMesh are skipped, and a fixed array kept trailing
        // entries with mesh == null, which CombineMeshes chokes on.
        List<CombineInstance> combine = new List<CombineInstance>(meshFilters.Length);

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.gameObject == gameObject)
                continue;

            if (mf.sharedMesh == null)
                continue;

            combine.Add(new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = myTransform * mf.transform.localToWorldMatrix
            });

            mf.gameObject.SetActive(false);
        }

        if (combine.Count == 0)
            return;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh combined = new Mesh();
        combined.CombineMeshes(combine.ToArray(), true, true);
        meshFilter.mesh = combined;

        gameObject.SetActive(true);
    }
}
