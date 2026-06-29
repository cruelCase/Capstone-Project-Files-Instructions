using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;   // Player

    [Header("Movement")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Map Bounds")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float clampedX = Mathf.Clamp(
            desiredPosition.x,
            minBounds.x + camWidth,
            maxBounds.x - camWidth
        );

        float clampedY = Mathf.Clamp(
            desiredPosition.y,
            minBounds.y + camHeight,
            maxBounds.y - camHeight
        );

        transform.position = new Vector3(
            clampedX,
            clampedY,
            transform.position.z
        );
    }
}
