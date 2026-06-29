using UnityEngine;

public class OneTimeImage : MonoBehaviour
{
    [Header("Image Objects")]
    public Transform[] images; // Assign your image(s) here in the Inspector

    // ⭐ Static flag ensures this image shows only once
    private static bool hasShown = false;

    void Start()
    {
        if (hasShown)
        {
            HideImages();
            gameObject.SetActive(false); // optional: hide manager object
            return;
        }

        hasShown = true; // mark as shown
        ShowImages();
    }

    void ShowImages()
    {
        foreach (Transform t in images)
        {
            if (t != null)
                t.gameObject.SetActive(true);
        }
    }

    void HideImages()
    {
        foreach (Transform t in images)
        {
            if (t != null)
                t.gameObject.SetActive(false);
        }
    }
}
