using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public AudioClip walkingSFX;
    public float walkingSFXCooldown = 0.3f;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private float walkingSFXTimer = 0f;

    // Button state
    [HideInInspector] public bool moveLeft = false;
    [HideInInspector] public bool moveRight = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        MovePlayer();
        UpdateWalkingSFX();
    }

    void MovePlayer()
    {
        float moveInput = 0f;

        if (moveLeft) moveInput = -1f;
        if (moveRight) moveInput = 1f;

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void UpdateWalkingSFX()
    {
        walkingSFXTimer -= Time.deltaTime;

        // Play walking SFX when player is moving horizontally
        if ((moveLeft || moveRight) && walkingSFXTimer <= 0f && walkingSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(walkingSFX);
            walkingSFXTimer = walkingSFXCooldown;
        }
    }

    // Called by Jump Button
    public void JumpButton()
    {
        // Same logic as your original script
        if (Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // UI Button Events
    public void OnLeftDown()  { moveLeft = true; }
    public void OnLeftUp()    { moveLeft = false; }

    public void OnRightDown() { moveRight = true; }
    public void OnRightUp()   { moveRight = false; }
}
