using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]

public class SpriteOutlineController : MonoBehaviour
{
    private static readonly int OutlineEnabledID = Shader.PropertyToID("_OutlineEnabled");
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");

    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private bool visibleOnStart = false;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField][Min(0f)] private float outlineThickness = 1.0f;

    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }

        propertyBlock = new MaterialPropertyBlock();
        ApplyAll();
        SetVisible(visibleOnStart);
    }
    public void SetVisible(bool visible)
    {
        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(OutlineEnabledID, visible ? 1f : 0f);
        targetRenderer.SetPropertyBlock(propertyBlock);
    }

    public void SetOutlineColor(Color color)
    {
        outlineColor = color;

        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(OutlineColorID, outlineColor);
        targetRenderer.SetPropertyBlock(propertyBlock);
    }

    public Color GetOutlineColor()
    {
        return outlineColor;
    }

    public float GetOutlineThickness()
    {
        return outlineThickness;
    }

    private void ApplyAll()
    {
        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(OutlineEnabledID, visibleOnStart ? 1f : 0f);
        propertyBlock.SetColor(OutlineColorID, outlineColor);
        propertyBlock.SetFloat(OutlineThicknessID, outlineThickness);
        targetRenderer.SetPropertyBlock(propertyBlock);
    }

    private void OnDisable()
    {
        if (targetRenderer == null)
        {
            return;
        }
        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(OutlineEnabledID, 0f);
        targetRenderer.SetPropertyBlock(propertyBlock);
    }

}
