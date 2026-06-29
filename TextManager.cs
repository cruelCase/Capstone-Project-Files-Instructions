using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class TextManager : MonoBehaviour
{
    public TMP_Text displayText;

    [TextArea]
    public string startingText;

    public float typeDelay = 0.04f;
    // Delay before typing starts (seconds)
    public float typingStartDelay = 2.5f;
    // Delay before invoking onNext after the Next button is pressed
    public float nextActionDelay = 0.5f;

    // Button Transform that acts as the "Next" button (assign the button GameObject here)
    public Transform nextButton;

    // Optional UnityEvent invoked when the Next button is pressed (after typing completes)
    public UnityEvent onNext;

    private Coroutine typingCoroutine;
    private string currentFullText = string.Empty;

    private void Start()
    {
        if (displayText != null)
            displayText.text = string.Empty;

        // Show next button by default
        if (nextButton != null)
            nextButton.gameObject.SetActive(true);

        if (!string.IsNullOrEmpty(startingText))
            ShowText(startingText);
    }

    public void ShowText(string text)
    {
        currentFullText = text ?? string.Empty;
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        // wait a short delay before beginning to type
        if (typingStartDelay > 0f)
            yield return new WaitForSeconds(typingStartDelay);

        if (displayText != null)
            displayText.text = string.Empty;

        // Keep next button visible throughout
        if (nextButton != null)
            nextButton.gameObject.SetActive(true);

        for (int i = 0; i < currentFullText.Length; i++)
        {
            if (displayText != null)
                displayText.text += currentFullText[i];

            yield return new WaitForSeconds(typeDelay);
        }

        typingCoroutine = null;
    }

    // Hook this to the Next button's onClick (assign the Next button to `nextButton` and wire its Button.onClick to this)
    public void OnNextButtonPressed()
    {
        if (typingCoroutine != null)
        {
            // typing is still in progress: skip animation and show full text immediately
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            if (displayText != null)
                displayText.text = currentFullText;
            return;
        }

        // typing is complete: proceed with the original next action
        if (nextActionDelay > 0f)
        {
            StartCoroutine(InvokeOnNextAfterDelay());
        }
        else
        {
            onNext?.Invoke();
        }
    }

    private IEnumerator InvokeOnNextAfterDelay()
    {
        yield return new WaitForSeconds(nextActionDelay);
        onNext?.Invoke();
    }
}
