using UnityEngine;

public class PartVisibilityManager : MonoBehaviour
{
    [Header("References")]
    public PartSelector selector;
    public Transform motorRoot;

    private MotorPart[] allParts;

    void Start()
    {
        CacheParts();
    }

    public void CacheParts()
    {
        if (motorRoot != null)
        {
            allParts = motorRoot.GetComponentsInChildren<MotorPart>(true);
        }
    }

    // 1. Hides only the visual mesh & collider of the selected part
    public void HideSelected()
    {
        if (selector == null || selector.currentSelected == null) return;

        SetPartMeshVisible(selector.currentSelected, false);
    }

    // 2. Restores visibility to every part
    public void ShowAll()
    {
        if (allParts == null || allParts.Length == 0) CacheParts();

        foreach (var part in allParts)
        {
            if (part != null)
            {
                SetPartMeshVisible(part, true);
            }
        }
    }

    // 3. Shows only the selected part's mesh and hides all other parts' meshes
    public void IsolateSelected()
    {
        if (selector == null || selector.currentSelected == null) return;
        if (allParts == null || allParts.Length == 0) CacheParts();

        MotorPart active = selector.currentSelected;

        foreach (var part in allParts)
        {
            if (part != null)
            {
                SetPartMeshVisible(part, part == active);
            }
        }
    }

    // Helper: toggles renderers and colliders without disabling the GameObject or UI
    private void SetPartMeshVisible(MotorPart part, bool visible)
    {
        Renderer[] renderers = part.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            r.enabled = visible;
        }

        Collider[] colliders = part.GetComponentsInChildren<Collider>(true);
        foreach (Collider c in colliders)
        {
            c.enabled = visible;
        }
    }
}