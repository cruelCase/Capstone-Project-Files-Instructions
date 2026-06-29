using UnityEngine;

public class CharacterAppearance : MonoBehaviour
{
    [System.Serializable]
    public class AppearanceSet
    {
        public string name;                  // Outfit name / identifier
        public SimpleAnimator2D bodyAnim;    // Body animator (whole outfit)
    }

    [Header("Assign All Outfits Here")]
    public AppearanceSet[] appearances;

    [Header("Optional Extra Sprites")]
    public SpriteRenderer hairRenderer;       // Hair, glasses, accessories
    public Sprite defaultHair;

    [Header("Choose Active Outfit")]
    public int selectedIndex = 0;

    [HideInInspector]
    public SimpleAnimator2D activeBodyAnim;

    private void Start()
    {
        // Load last saved outfit from ProfilePlayerData or PlayerPrefs
        LoadAppearance();
    }

    /// <summary>
    /// Apply the currently selected outfit
    /// </summary>
    public void ApplyAppearance(bool resetToIdle = false)
    {
        if (appearances.Length == 0) return;

        for (int i = 0; i < appearances.Length; i++)
        {
            bool active = (i == selectedIndex);
            if (appearances[i].bodyAnim != null && appearances[i].bodyAnim.spriteRenderer != null)
                appearances[i].bodyAnim.spriteRenderer.enabled = active;
        }

        activeBodyAnim = appearances[selectedIndex].bodyAnim;

        if (resetToIdle && activeBodyAnim != null)
            activeBodyAnim.PlayAnimation(SimpleAnimator2D.AnimationState.Idle);
    }

    /// <summary>
    /// Change outfit at runtime
    /// </summary>
    public void SetOutfit(int index)
    {
        if (index < 0 || index >= appearances.Length) return;

        selectedIndex = index;
        ApplyAppearance();

        // Save the selection to GameManager using SelectedOutfitIndex
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectedOutfitIndex = selectedIndex;
        }
    }

    /// <summary>
    /// Apply hair/accessory sprite
    /// </summary>
    public void SetHair(Sprite newHair)
    {
        if (hairRenderer != null)
            hairRenderer.sprite = newHair ?? defaultHair;
    }

    /// <summary>
    /// Load last saved outfit
    /// </summary>
    private void LoadAppearance()
    {
        // Comment the if statemment below if you want to use the inspector in changing outfits
        if (GameManager.Instance != null)
        {
            selectedIndex = GameManager.Instance.SelectedOutfitIndex;

            // Ensure index is valid
            if (selectedIndex < 0 || selectedIndex >= appearances.Length)
                selectedIndex = 0;
        }

        ApplyAppearance();
    }
}
