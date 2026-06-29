using UnityEngine;
using UnityEngine.Events;

public class InteractableZone : MonoBehaviour
{
    public UnityEvent onInteract; // assign in inspector

    private PlayerInteractor playerInteractor;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered interactable zone: " + gameObject.name);
            playerInteractor = other.GetComponent<PlayerInteractor>();
            if (playerInteractor != null)
                playerInteractor.SetCurrentInteractable(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited interactable zone: " + gameObject.name);
            if (playerInteractor != null)
                playerInteractor.ClearCurrentInteractable(this);
        }
    }

    public void Interact()
    {
        onInteract?.Invoke();
        Debug.Log("Interact pressed on " + gameObject.name);
    }
}
