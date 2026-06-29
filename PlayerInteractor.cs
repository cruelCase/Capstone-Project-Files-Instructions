using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public GameObject interactButton;
    private InteractableZone currentInteractable;
    private Image buttonImage;

    private void Start()
    {
        interactButton.SetActive(true);
        buttonImage = interactButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color color = buttonImage.color;
            color.a = 100f / 255f;
            buttonImage.color = color;
        }
    }

    public void SetCurrentInteractable(InteractableZone zone)
    {
        currentInteractable = zone;
        if (buttonImage != null)
        {
            Color color = buttonImage.color;
            color.a = 1f;
            buttonImage.color = color;
        }
    }

    public void ClearCurrentInteractable(InteractableZone zone)
    {
        if (currentInteractable == zone)
        {
            currentInteractable = null;
            if (buttonImage != null)
            {
                Color color = buttonImage.color;
                color.a = 100f / 255f;
                buttonImage.color = color;
            }
        }
    }

    public void OnInteractPressed()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact(); // UnityEvent triggers the correct NPC StartDialogue
        }
    }
}
