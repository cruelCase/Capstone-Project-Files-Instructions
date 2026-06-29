using UnityEngine;
using System.Collections;

public class SwipeItem : MonoBehaviour
{
    public enum ItemType { Need, Want, Green, Gold }
    public ItemType itemType;
    public float fallSpeed = 3f;
    public float destroyY = -10f;

    private bool isDestroyed = false;

    void Start()
    {
        // Start the lifetime countdown using itemLifetime from MiniQuizManager2
        if (MiniQuizManager2.Instance != null)
        {
            StartCoroutine(DestroyAfterLifetime(MiniQuizManager2.Instance.itemLifetime));
        }
        else
        {
            // Fallback if manager not available
            StartCoroutine(DestroyAfterLifetime(2.5f));
        }
    }

    IEnumerator DestroyAfterLifetime(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (!isDestroyed)
            DestroyItem();
    }

    void Update()
    {
        if (isDestroyed)
            return;

        // Use world-space translation so UI parent transforms don't interfere with fall direction
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        // Destroy if it falls too far
        if (transform.position.y < destroyY)
        {
            DestroyItem();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (MiniQuizManager2.Instance == null || MiniQuizManager2.Instance.catcherTransform == null)
            return;

        if (other.transform != MiniQuizManager2.Instance.catcherTransform)
            return;

        CollectItem();
    }

    void CollectItem()
    {
        if (MiniQuizManager2.Instance == null)
            return;

        switch (itemType)
        {
            case ItemType.Need:
                MiniQuizManager2.Instance.CollectNeedItem(gameObject);
                break;
            case ItemType.Want:
                MiniQuizManager2.Instance.CollectWantItem(gameObject);
                break;
            case ItemType.Green:
                MiniQuizManager2.Instance.CollectGreenItem(gameObject);
                break;
            case ItemType.Gold:
                MiniQuizManager2.Instance.CollectGoldItem(gameObject);
                break;
        }
    }

    void DestroyItem()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;
        if (MiniQuizManager2.Instance != null)
            MiniQuizManager2.Instance.RemoveSpawnedItem(gameObject);

        Destroy(gameObject);
    }
}
