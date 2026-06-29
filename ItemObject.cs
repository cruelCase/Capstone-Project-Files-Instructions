using UnityEngine;
using UnityEngine.EventSystems;

public class ItemObject : MonoBehaviour, IPointerClickHandler
{
    [Header("Set if this is a NEED or WANT")]
    public bool isNeed;  // true = NEED, false = WANT

    private float lifetime = 2f;

    private void Start()
    {
        // Destroy after 2 seconds
        Destroy(gameObject, lifetime);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        MiniQuizManager1.Instance.ItemClicked(this);
    }
}
