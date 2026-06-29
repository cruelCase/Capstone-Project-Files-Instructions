using UnityEngine;

public class OutfitChanger : MonoBehaviour
{
    [Header("Animated Outfits")]
    public Animator animator;
    public RuntimeAnimatorController[] outfitAnimators;
    private int outfitIndex = 0;

    [Header("Accessories (Static Images)")]
    public SpriteRenderer hairRenderer;       // Where your hair image goes
    public SpriteRenderer glassesRenderer;    // Where your glasses image goes

    public Sprite[] hairOptions;              // Different hair sprites
    public Sprite[] glassesOptions;           // Different glasses sprites

    private int hairIndex = 0;
    private int glassesIndex = 0;

    // Change the animated outfit
    public void ChangeOutfit()
    {
        outfitIndex = (outfitIndex + 1) % outfitAnimators.Length;
        animator.runtimeAnimatorController = outfitAnimators[outfitIndex];
    }

    // Change hair sprite
    public void ChangeHair()
    {
        hairIndex = (hairIndex + 1) % hairOptions.Length;
        hairRenderer.sprite = hairOptions[hairIndex];
    }

    // Change glasses sprite
    public void ChangeGlasses()
    {
        glassesIndex = (glassesIndex + 1) % glassesOptions.Length;
        glassesRenderer.sprite = glassesOptions[glassesIndex];
    }
}
