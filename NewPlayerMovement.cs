using UnityEngine;

public class NewPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Joystick joystick; // assign your joystick here

    [Header("Walking SFX")]
    public AudioClip walkingSFX;
    public float walkingSFXCooldown = 0.3f;

    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;
    private float walkingSFXTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        // Read the joystick input
        Vector2 moveInput = new Vector2(joystick.Horizontal, joystick.Vertical);

        // Move the player
        rb.linearVelocity = moveInput * moveSpeed;

        // Update the Blend Tree parameters
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

        // Walking flag
        bool isWalking = moveInput.magnitude > 0.1f; // deadzone to avoid jitter
        animator.SetBool("isWalking", isWalking);

        // Optional: remember last direction for idle facing
        if (isWalking)
        {
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }

        // Play walking SFX
        UpdateWalkingSFX(isWalking);
    }

    void UpdateWalkingSFX(bool isWalking)
    {
        if (audioSource == null || walkingSFX == null)
            return;

        if (isWalking)
        {
            // Start playing walking SFX when movement begins
            if (!audioSource.isPlaying)
            {
                audioSource.clip = walkingSFX;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            // Stop immediately when movement stops
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
