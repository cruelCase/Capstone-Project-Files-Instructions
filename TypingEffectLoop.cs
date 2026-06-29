using UnityEngine;
using TMPro;
using System.Collections;

public class TypingEffectLoop : MonoBehaviour
{
    public TMP_Text sentenceText;      // Drag your TMP text here
    public string fullSentence = "Your sentence here";
    public float typingSpeed = 0.05f;  // Time between each character
    public float blinkDuration = 0.3f; // How long each blink lasts
    public int blinkTimes = 3;         // Number of blinks before restarting

    private void Start()
    {
        if (sentenceText == null)
            sentenceText = GetComponent<TMP_Text>();

        StartCoroutine(TypeAndBlinkLoop());
    }

    private IEnumerator TypeAndBlinkLoop()
    {
        while (true)
        {
            // Typing animation
            sentenceText.text = "";
            foreach (char c in fullSentence)
            {
                sentenceText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }

            // Blink animation
            for (int i = 0; i < blinkTimes; i++)
            {
                sentenceText.enabled = false;
                yield return new WaitForSeconds(blinkDuration);
                sentenceText.enabled = true;
                yield return new WaitForSeconds(blinkDuration);
            }

            // Optional small pause before restarting
            yield return new WaitForSeconds(0.5f);
        }
    }
}
