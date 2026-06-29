using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MergeTile : MonoBehaviour
{
    public int amount; // 10, 20, 40, 80...
    public Image tileImage; // assign root Image
    public TextMeshProUGUI valueText; // assign child TMP_Text

    // This updates the coin's image and text
    public void UpdateVisual(Sprite newSprite)
    {
        tileImage.sprite = newSprite;          // sets the coin image
        valueText.text = "₱" + amount;        // sets the coin value text
    }
}
