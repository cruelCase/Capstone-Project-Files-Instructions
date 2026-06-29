using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public float speed = 1f;          // How fast the background moves
    public float resetHeight = 20f;   // The total height after which it resets

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // Save the initial position
    }

    void Update()
    {
        // Move the background upward
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        // Reset position to start when it reaches resetHeight
        if (transform.position.y >= startPosition.y + resetHeight)
        {
            transform.position = startPosition;
        }
    }
}
