using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodedView : MonoBehaviour
{
    [System.Serializable]
    public class ExplodePart
    {
        public Transform partTransform;
        public Vector3 explodeOffset; // Direction and distance to move
        [HideInInspector] public Vector3 originalLocalPos;
    }

    [Header("Explode Settings")]
    public List<ExplodePart> parts = new List<ExplodePart>();
    public float animationDuration = 1.0f;

    private bool isExploded = false;
    private Coroutine activeRoutine;

    void Awake()
    {
        // Cache original assembled positions
        foreach (var p in parts)
        {
            if (p.partTransform != null)
            {
                p.originalLocalPos = p.partTransform.localPosition;
            }
        }
    }

    public void ToggleExplode()
    {
        SetExploded(!isExploded);
    }

    public void SetExploded(bool state)
    {
        if (isExploded == state) return;
        isExploded = state;

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(AnimateExplosion(isExploded));
    }

    private IEnumerator AnimateExplosion(bool exploded)
    {
        float elapsed = 0f;

        // Store positions at the moment of triggering to prevent snapping
        Dictionary<Transform, Vector3> startPositions = new Dictionary<Transform, Vector3>();
        foreach (var p in parts)
        {
            if (p.partTransform != null)
                startPositions[p.partTransform] = p.partTransform.localPosition;
        }

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);

            foreach (var p in parts)
            {
                if (p.partTransform == null) continue;

                Vector3 targetPos = exploded ? (p.originalLocalPos + p.explodeOffset) : p.originalLocalPos;
                p.partTransform.localPosition = Vector3.Lerp(startPositions[p.partTransform], targetPos, t);
            }

            yield return null;
        }

        // Snap precisely to target at completion
        foreach (var p in parts)
        {
            if (p.partTransform == null) continue;
            p.partTransform.localPosition = exploded ? (p.originalLocalPos + p.explodeOffset) : p.originalLocalPos;
        }
    }
}