using UnityEngine;

public class SimpleAnimator2D : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] idleFrames;
    public Sprite[] walkFrames;
    public float frameRate = 8f;

    private Sprite[] currentFrames;
    private int frameIndex = 0;
    private float timer = 0f;

    public enum AnimationState { Idle, Walk }
    private AnimationState currentState;

    void Start()
    {
        PlayAnimation(AnimationState.Idle); // ALWAYS start animation
    }

    void OnEnable()
    {
        // Ensures animation works even if object was inactive at Start
        PlayAnimation(AnimationState.Idle);
    }

    void Update()
    {
        if (currentFrames == null || currentFrames.Length == 0)
            return;

        timer += Time.deltaTime;

        if (timer >= 1f / frameRate)
        {
            frameIndex = (frameIndex + 1) % currentFrames.Length;
            spriteRenderer.sprite = currentFrames[frameIndex];
            timer = 0f;
        }
    }

    public void PlayAnimation(AnimationState newState)
    {
        if (newState == currentState && currentFrames != null)
            return;

        currentState = newState;

        if (newState == AnimationState.Idle) currentFrames = idleFrames;
        if (newState == AnimationState.Walk) currentFrames = walkFrames;

        frameIndex = 0;
        timer = 0f;

        if (currentFrames != null && currentFrames.Length > 0)
            spriteRenderer.sprite = currentFrames[0];
    }
}
