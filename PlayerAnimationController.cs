using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public CharacterAppearance appearance; // assign in inspector
    public PlayerMovement movement;

    void Update()
    {
        float move = 0f;

        if (movement.moveLeft) move = -1f;
        if (movement.moveRight) move = 1f;

        bool walking = move != 0;
        var state = walking ?
            SimpleAnimator2D.AnimationState.Walk :
            SimpleAnimator2D.AnimationState.Idle;

        // Animate the currently active body animator
        if (appearance != null && appearance.activeBodyAnim != null)
            appearance.activeBodyAnim.PlayAnimation(state);

        // Flip sprite based on movement direction
        bool flip = move < 0;
        if (appearance != null && appearance.activeBodyAnim != null)
            appearance.activeBodyAnim.spriteRenderer.flipX = flip;
    }
}
