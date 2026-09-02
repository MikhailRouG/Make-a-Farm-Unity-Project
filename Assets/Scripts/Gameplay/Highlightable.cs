using System.Collections.Generic;
using UnityEngine;

// Purely local visuals: nothing here is networked, every client outlines only what
// its own player is looking at.
[DisallowMultipleComponent]
public class Highlightable : MonoBehaviour
{
    private const string MaskName = "Outline Mask";
    private const string HullName = "Outline Hull";

    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");

    // Lowpoly meshes are flat shaded, so their normals are split at every edge and a
    // hull extruded along them tears apart at the seams. The welded copy is cached per
    // source mesh: every carrot in the scene shares one, and the cache outlives the
    // hulls themselves, so rebuilding them costs nothing but a few GameObjects.
    private static readonly Dictionary<Mesh, Mesh> WeldedMeshes = new Dictionary<Mesh, Mesh>();

    [Header("References")]
    // Leave empty for anything that swaps its geometry at runtime - a plant replaces
    // its visual child on every growth stage. An empty list is looked up again while
    // the object is highlighted, a filled one is taken as a fixed set.
    [SerializeField] private Renderer[] _renderers;
    [SerializeField] private Shader _outlineShader;
    [SerializeField] private Shader _maskShader;

    [Header("Settings")]
    [SerializeField] private Color _outlineColor = new Color(1f, 0.85f, 0.3f);
    [SerializeField, Range(0.5f, 16f)] private float _outlineWidth = 4f;

    private readonly List<GameObject> _parts = new List<GameObject>();
    private readonly List<Renderer> _sources = new List<Renderer>();
    private readonly List<Renderer> _scan = new List<Renderer>();
    private Material _outlineMaterial;
    private Material _maskMaterial;
    private bool _isHighlighted;
    private bool _shadersMissing;

    private bool IsDynamic => _renderers == null || _renderers.Length == 0;

    private void OnDisable()
    {
        SetHighlighted(false);
    }

    private void OnDestroy()
    {
        if (_outlineMaterial != null)
            Destroy(_outlineMaterial);

        if (_maskMaterial != null)
            Destroy(_maskMaterial);
    }

    public void SetHighlighted(bool value)
    {
        if (_isHighlighted == value) return;
        _isHighlighted = value;

        if (!value)
        {
            ClearParts();
            _sources.Clear();
            return;
        }

        Scan();
        Rebuild();
    }

    // Plant destroys its visual child and instantiates the next one on every growth
    // stage, so the renderers the hulls hang under stop existing mid-highlight.
    // Re-scanning beats subscribing to anything: it is one allocation-free walk of a
    // small hierarchy and it catches a swap however deep it happens -
    // OnTransformChildrenChanged only fires for direct children, and an event from
    // Plant would only ever cover plants.
    //
    // Driven by PlayerInteraction rather than a LateUpdate of its own: the component
    // sits on every interactable in the farm, and Unity would then call all of them
    // every frame to have all but one return immediately.
    public void RefreshSources()
    {
        if (!_isHighlighted || !IsDynamic) return;

        Scan();

        if (ScanMatchesSources()) return;

        Rebuild();
    }

    private void Scan()
    {
        _scan.Clear();

        if (!IsDynamic)
        {
            foreach (Renderer manual in _renderers)
            {
                if (manual != null)
                    _scan.Add(manual);
            }

            return;
        }

        GetComponentsInChildren(_scan);

        for (int i = _scan.Count - 1; i >= 0; i--)
        {
            if (!IsOutlineable(_scan[i]))
                _scan.RemoveAt(i);
        }
    }

    private bool IsOutlineable(Renderer source)
    {
        if (source == null) return false;

        // The floating TMP timer and anything else on a world space canvas: outlining
        // a label the player is meant to read only makes it harder to read.
        if (source.transform is RectTransform) return false;

        // Our own hulls. Destroy only lands at the end of the frame, so during a
        // rebuild the previous ones are still here, and outlining an outline would
        // nest a level deeper every time.
        return !_parts.Contains(source.gameObject);
    }

    private bool ScanMatchesSources()
    {
        if (_scan.Count != _sources.Count) return false;

        for (int i = 0; i < _scan.Count; i++)
        {
            if (_scan[i] != _sources[i]) return false;
        }

        return true;
    }

    // Builds from whatever Scan just put in _scan.
    private void Rebuild()
    {
        if (!TryCreateMaterials()) return;

        ClearParts();

        _sources.Clear();
        _sources.AddRange(_scan);

        foreach (Renderer source in _sources)
            CreateOutlineParts(source);
    }

    private bool TryCreateMaterials()
    {
        if (_outlineMaterial != null && _maskMaterial != null) return true;
        if (_shadersMissing) return false;

        if (_outlineShader == null)
            _outlineShader = Shader.Find("Custom/URP/OutlineHull");

        if (_maskShader == null)
            _maskShader = Shader.Find("Custom/URP/OutlineMask");

        if (_outlineShader == null || _maskShader == null)
        {
            // Latched, or the warning would repeat every frame the object is looked at.
            _shadersMissing = true;
            Debug.LogWarning($"{name}: outline shaders are missing, no outline will be drawn.", this);
            return false;
        }

        _outlineMaterial = new Material(_outlineShader) { name = "OutlineHull (Runtime)" };
        _outlineMaterial.SetColor(OutlineColorId, _outlineColor);
        _outlineMaterial.SetFloat(OutlineWidthId, _outlineWidth);

        _maskMaterial = new Material(_maskShader) { name = "OutlineMask (Runtime)" };

        return true;
    }

    private void ClearParts()
    {
        for (int i = 0; i < _parts.Count; i++)
        {
            if (_parts[i] != null)
                Destroy(_parts[i]);
        }

        _parts.Clear();
    }

    private void CreateOutlineParts(Renderer source)
    {
        Mesh mesh = source is SkinnedMeshRenderer skinnedSource
            ? skinnedSource.sharedMesh
            : source.TryGetComponent(out MeshFilter filter) ? filter.sharedMesh : null;

        if (mesh == null) return;

        // The mask stamps the pixels the object covers into the stencil buffer and the
        // hull is discarded there, so only the rim survives. The mask runs on the raw
        // mesh: welding it would move the silhouette it is supposed to describe.
        _parts.Add(CreatePart(source, mesh, _maskMaterial, MaskName));
        _parts.Add(CreatePart(source, GetWeldedMesh(mesh), _outlineMaterial, HullName));
    }

    // Plain constants rather than names built from the source: a rebuild would then
    // allocate two strings per renderer, and the part is a child of the renderer it
    // wraps, so the hierarchy already says which object it belongs to.
    private GameObject CreatePart(Renderer source, Mesh mesh, Material material, string partName)
    {
        GameObject part = new GameObject(partName);
        part.transform.SetParent(source.transform, false);
        part.layer = source.gameObject.layer;

        Renderer partRenderer;

        if (source is SkinnedMeshRenderer skinned)
        {
            SkinnedMeshRenderer copy = part.AddComponent<SkinnedMeshRenderer>();
            copy.sharedMesh = mesh;
            copy.bones = skinned.bones;
            copy.rootBone = skinned.rootBone;
            copy.localBounds = skinned.localBounds;
            partRenderer = copy;
        }
        else
        {
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            partRenderer = part.AddComponent<MeshRenderer>();
        }

        // One material per submesh, otherwise only the first submesh is drawn.
        Material[] materials = new Material[mesh.subMeshCount];
        for (int i = 0; i < materials.Length; i++)
            materials[i] = material;

        partRenderer.sharedMaterials = materials;
        partRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        partRenderer.receiveShadows = false;
        partRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        partRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

        return part;
    }

    private static Mesh GetWeldedMesh(Mesh source)
    {
        if (WeldedMeshes.TryGetValue(source, out Mesh cached) && cached != null)
            return cached;

        // Read/Write Disabled on the model import: the vertices cannot be touched, so
        // the raw mesh is used and the outline may crack on hard edges.
        if (!source.isReadable)
        {
            Debug.LogWarning($"Mesh '{source.name}' is not readable, its outline may show seams. " +
                             "Enable Read/Write on the model import settings to fix it.");
            WeldedMeshes[source] = source;
            return source;
        }

        Mesh welded = Instantiate(source);
        welded.name = $"{source.name} (Welded)";

        Vector3[] vertices = welded.vertices;
        Vector3[] normals = welded.normals;

        if (normals.Length == vertices.Length)
        {
            Dictionary<Vector3, Vector3> sums = new Dictionary<Vector3, Vector3>(vertices.Length);

            for (int i = 0; i < vertices.Length; i++)
            {
                sums.TryGetValue(vertices[i], out Vector3 sum);
                sums[vertices[i]] = sum + normals[i];
            }

            for (int i = 0; i < vertices.Length; i++)
                normals[i] = sums[vertices[i]].normalized;

            welded.normals = normals;
        }

        WeldedMeshes[source] = welded;
        return welded;
    }
}
