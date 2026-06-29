using System.Collections;
using UnityEngine;

public class ConfettiManager : MonoBehaviour
{
    [Header("Celebration UI")]
    // Assign a Transform that contains an Image with animations (inactive by default). It will be activated when the button is clicked.
    public Transform celebrationImage;
    // Assign a Transform that has an AudioSource with your clapping sound (or any audio). The AudioSource will be played when the button is clicked.
    public Transform clappingAudioSource;

    private Coroutine audioStopCoroutine;

    // Trigger celebration: activate image animation and play clapping audio.
    public void PlayConfetti()
    {
        // Activate celebration image and play its Animator (if assigned)
        if (celebrationImage != null)
        {
            celebrationImage.gameObject.SetActive(true);
            Animator anim = celebrationImage.GetComponent<Animator>();
            if (anim != null)
                anim.Play(0);
        }

        // Play clapping audio if there's an AudioSource on the assigned Transform
        if (clappingAudioSource != null)
        {
            AudioSource a = clappingAudioSource.GetComponent<AudioSource>();
            if (a != null)
            {
                a.Play();
                if (audioStopCoroutine != null)
                    StopCoroutine(audioStopCoroutine);
                audioStopCoroutine = StartCoroutine(StopAudioAfterDelay(a, 5f));
            }
        }
    }

    private IEnumerator StopAudioAfterDelay(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null)
            audioSource.Stop();
        audioStopCoroutine = null;
    }
}
