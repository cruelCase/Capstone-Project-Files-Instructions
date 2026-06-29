using UnityEngine;

public class PlayerBoundary : MonoBehaviour
{
    public AreaManager areaManager;
    private bool canTrigger = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canTrigger) return; // Prevent double triggers

        if (collision.gameObject.name == "RightBoundary")
        {
            canTrigger = false;
            areaManager.MoveToNextArea();
            Invoke(nameof(EnableTrigger), 0.3f); // re-enable after short delay
        }
        else if (collision.gameObject.name == "LeftBoundary")
        {
            canTrigger = false;
            areaManager.MoveToPreviousArea();
            Invoke(nameof(EnableTrigger), 0.3f); // re-enable after short delay
        }
    }

    private void EnableTrigger()
    {
        canTrigger = true;
    }
}
