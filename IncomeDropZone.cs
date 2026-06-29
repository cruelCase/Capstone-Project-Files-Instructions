using UnityEngine;
using UnityEngine.EventSystems;

public class IncomeDropZone : MonoBehaviour, IDropHandler
{
    public IncomeCategory zoneCategory;

    public void OnDrop(PointerEventData eventData)
    {
        IncomeItem item = eventData.pointerDrag.GetComponent<IncomeItem>();
        if (item == null) return;

        // Check correctness
        bool correct = item.category == zoneCategory;

        // Tell manager about the attempt
        //MiniGameIncomeSortManager.Instance.OnItemDropped(correct);

        // Destroy the prefab no matter what
        Destroy(item.gameObject);
    }
}
