using UnityEngine;

public class MotorPart : MonoBehaviour
{
    [Header("Display Info")]
    public string partName;

    [Header("Visual Highlighting")]
    public Color highlightColor = new Color(1f, 0.85f, 0.2f, 1f); // Vibrant amber/yellow

    private Renderer[] renderers;
    private MaterialPropertyBlock propBlock;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    public GameObject labelObject;

    void Awake()
    {
        // Grab renderers on this object AND any child objects
        renderers = GetComponentsInChildren<Renderer>(true);
        propBlock = new MaterialPropertyBlock();
    }

    public void SetHighlight(bool state)
    {
        if (renderers == null || renderers.Length == 0) return;

        foreach (Renderer r in renderers)
        {
            if (r == null) continue;

            for (int i = 0; i < r.sharedMaterials.Length; i++)
            {
                if (state)
                {
                    // Fetch existing block, apply color override
                    r.GetPropertyBlock(propBlock, i);

                    if (r.sharedMaterials[i] != null && r.sharedMaterials[i].HasProperty(BaseColorId))
                    {
                        propBlock.SetColor(BaseColorId, highlightColor);
                    }
                    else
                    {
                        propBlock.SetColor(ColorId, highlightColor);
                    }

                    r.SetPropertyBlock(propBlock, i);
                }
                else
                {
                    // Clear overrides and restore original material colors
                    r.SetPropertyBlock(null, i);
                }
            }
        }
    }
}