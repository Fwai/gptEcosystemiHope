using UnityEngine;

/// <summary>
/// Simple orbital camera controller. Allows orbiting around a target, zooming
/// in/out with the scroll wheel, and panning with the middle mouse button.
/// </summary>
[RequireComponent(typeof(Camera))]
public class SimpleOrbitCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 45f;
    public Vector2 orbit = new Vector2(25f, 45f); // x = pitch, y = yaw
    public float zoomSpeed = 10f;
    public float orbitSpeed = 4f;
    public float panSpeed = 0.5f;

    void Start()
    {
        if (!target)
        {
            GameObject t = new GameObject("CamTarget");
            t.transform.position = Vector3.zero;
            target = t.transform;
        }
        UpdateTransform();
    }

    void Update()
    {
        // Right mouse drag: orbit
        if (Input.GetMouseButton(1))
        {
            orbit.y += Input.GetAxis("Mouse X") * orbitSpeed * 10f;
            orbit.x -= Input.GetAxis("Mouse Y") * orbitSpeed * 10f;
            orbit.x = Mathf.Clamp(orbit.x, 5f, 85f);
        }
        // Scroll wheel: zoom
        distance -= Input.mouseScrollDelta.y * zoomSpeed;
        distance = Mathf.Clamp(distance, 10f, 220f);
        // Middle mouse drag: pan
        if (Input.GetMouseButton(2))
        {
            Vector3 right = transform.right;
            Vector3 up = Vector3.up;
            target.position -= right * Input.GetAxis("Mouse X") * panSpeed;
            target.position -= up    * Input.GetAxis("Mouse Y") * panSpeed;
        }
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        Quaternion rot = Quaternion.Euler(orbit.x, orbit.y, 0f);
        Vector3 dir = rot * Vector3.forward;
        transform.position = target.position - dir * distance + Vector3.up * 5f;
        transform.rotation = rot;
    }
}