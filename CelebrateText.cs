using UnityEngine;
using TMPro;

public class CelebrateText : MonoBehaviour
{
    public TMP_Text congratsText;     // Drag your TMP text here
    public float pulseSpeed = 2f;     // Speed of the pulsing
    public float scaleAmount = 0.2f;  // How big the pulse gets
    public float colorSpeed = 2f;     // How fast colors change

    private Vector3 originalScale;

    void Start()
    {
        if (congratsText == null)
            congratsText = GetComponent<TMP_Text>();

        originalScale = congratsText.transform.localScale;
    }

    void Update()
    {
        // Pulse the text scale
        float scaleFactor = 1 + Mathf.Sin(Time.time * pulseSpeed) * scaleAmount;
        congratsText.transform.localScale = originalScale * scaleFactor;

        // Cycle through colors
        congratsText.color = Color.HSVToRGB(Mathf.PingPong(Time.time * colorSpeed, 1f), 1f, 1f);

        // Optional: small rotation for extra flair
        congratsText.transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * pulseSpeed) * 10f);
    }
}
