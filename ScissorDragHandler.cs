using UnityEngine;
using UnityEngine.EventSystems;

public class ScissorDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private MiniGameWiseManager manager;
    private Canvas uiCanvas;
    private RectTransform rectTransform;
    private Vector2 pointerOffset;
    private bool isDragging;
    private float dragStartTime;
    private RectTransform clampArea;
    private Transform originalParent;

    public void Initialize(MiniGameWiseManager manager, Canvas canvas, RectTransform clampArea = null)
    {
        this.manager = manager;
        this.uiCanvas = canvas ?? GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        this.clampArea = clampArea;
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (uiCanvas == null)
            uiCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rectTransform == null)
            return;

        originalParent = rectTransform.parent;
        if (uiCanvas != null)
            rectTransform.SetParent(uiCanvas.transform, true);

        RectTransform canvasRect = uiCanvas != null ? uiCanvas.transform as RectTransform : null;
        if (canvasRect != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition);
            pointerOffset = rectTransform.localPosition - (Vector3)localPointerPosition;
        }
        isDragging = true;
        dragStartTime = Time.time;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null || uiCanvas == null)
            return;

        RectTransform canvasRect = uiCanvas.transform as RectTransform;
        if (canvasRect == null)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            Vector2 baseLocal = localPoint + pointerOffset;

            // Convert desired canvas-local position to world, clamp to clampArea if provided, and apply
            Vector3 desiredWorld = uiCanvas.transform.TransformPoint(baseLocal);
            if (clampArea != null)
            {
                Vector3[] corners = new Vector3[4];
                clampArea.GetWorldCorners(corners);
                float minX = corners[0].x;
                float maxX = corners[2].x;
                float minY = corners[0].y;
                float maxY = corners[2].y;

                desiredWorld.x = Mathf.Clamp(desiredWorld.x, minX, maxX);
                desiredWorld.y = Mathf.Clamp(desiredWorld.y, minY, maxY);
            }

            rectTransform.position = desiredWorld;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        manager?.OnScissorReleased(eventData);
    }
}
