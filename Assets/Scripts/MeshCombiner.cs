using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCombiner : MonoBehaviour
{
    private void Awake()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        Matrix4x4 myTransform = transform.worldToLocalMatrix;

        int i = 0;
        while (i < meshFilters.Length)
        {
            if (meshFilters[i].gameObject == gameObject) { i++; continue; }

            combine[i].mesh = meshFilters[i].sharedMesh;

            combine[i].transform = myTransform * meshFilters[i].transform.localToWorldMatrix;

            meshFilters[i].gameObject.SetActive(false);

            i++;
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();

        meshFilter.mesh.CombineMeshes(combine, true, true);

        gameObject.SetActive(true);
    }
}