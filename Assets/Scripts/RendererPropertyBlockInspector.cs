using UnityEngine;
[ExecuteAlways]
public class RendererPropertyBlockInspector : MonoBehaviour
{

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color baseColor = Color.white;

    private MaterialPropertyBlock block;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private void OnEnable()
    {
        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }

    public void Apply()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer == null)
            return;

        if (block == null)
            block = new MaterialPropertyBlock();

        targetRenderer.GetPropertyBlock(block);
        block.SetColor(BaseColorId, baseColor);
        targetRenderer.SetPropertyBlock(block);
    }
}
