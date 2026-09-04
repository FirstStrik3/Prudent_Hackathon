using UnityEngine;

[ExecuteAlways]
public class BillboardLabel : MonoBehaviour
{
    private Camera targetCamera;

    void Start()
    {
        FindCamera();
    }

    void FindCamera()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindFirstObjectByType<Camera>();
            }
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            FindCamera();
            if (targetCamera == null) return;
        }

        // Align directly with the camera's forward plane to keep it flat to view
        transform.forward = targetCamera.transform.forward;
    }
}