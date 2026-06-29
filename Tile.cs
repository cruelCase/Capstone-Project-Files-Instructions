using UnityEngine;

public class Tile : MonoBehaviour
{
    public int valueIndex; // 0 = ₱10, 1 = ₱20 ... 6 = ₱500, -1 = Random tile
    public SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void UpgradeValue(Sprite[] sprites)
    {
        if (valueIndex >= 0 && valueIndex < sprites.Length - 1)
        {
            valueIndex++;
            spriteRenderer.sprite = sprites[valueIndex];
        }
    }

    public void HalveValue(Sprite[] sprites)
    {
        if (valueIndex > 0)
        {
            valueIndex--;
            spriteRenderer.sprite = sprites[valueIndex];
        }
    }
}
