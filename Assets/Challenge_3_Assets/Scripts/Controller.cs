using UnityEngine;

public class Controller : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 targetOffset = Vector3.zero;

    [Header("Distances")]
    public float distance = 5.0f;
    public float minDistance = 1.0f;
    public float maxDistance = 20.0f;

    [Header("Speeds")]
    public float xSpeed = 120.0f;
    public float ySpeed = 80.0f;
    public float zoomSpeed = 5.0f;
    public float panSpeed = 0.3f;

    [Header("Pitch Limits")]
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    private float x = 0.0f;
    private float y = 0.0f;
    private Vector3 currentTargetPos;

    void Start()
    {
        if (target != null)
        {
            currentTargetPos = target.position + targetOffset;
        }

        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (!target) return;

        // 1. Zoom (Mouse Scroll Wheel)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // 2. Rotate / Orbit (Right Mouse Button or Left Alt + Left Click)
        if (Input.GetMouseButton(1))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            y = ClampAngle(y, yMinLimit, yMaxLimit);
        }

        // 3. Pan (Middle Mouse Button)
        if (Input.GetMouseButton(2))
        {
            float panX = -Input.GetAxis("Mouse X") * panSpeed;
            float panY = -Input.GetAxis("Mouse Y") * panSpeed;

            Vector3 pan = transform.right * panX + transform.up * panY;
            currentTargetPos += pan;
        }

        // Apply Transform
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + currentTargetPos;

        transform.rotation = rotation;
        transform.position = position;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F) angle += 360F;
        if (angle > 360F) angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    // Public method to reset focus or re-target a clicked part
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        currentTargetPos = newTarget.position;
    }
}