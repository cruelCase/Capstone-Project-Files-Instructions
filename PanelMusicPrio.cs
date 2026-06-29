using UnityEngine;

[System.Serializable]
public class PanelAudioPair
{
    public GameObject panel;
    public AudioSource audioSource;
}

public class PanelMusicPrio : MonoBehaviour
{
    [Header("Background Audio")]
    public AudioSource backgroundAudioSource;

    [Header("Panels & Their Audio")]
    public PanelAudioPair[] panelAudioPairs;

    private bool wasBackgroundPlaying = false;

    void Start()
    {
        // Auto-detect background audio if not assigned
        if (backgroundAudioSource == null)
        {
            AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (AudioSource audio in allAudioSources)
            {
                if (audio.playOnAwake && audio.loop)
                {
                    backgroundAudioSource = audio;
                    break;
                }
            }
        }
    }

    void Update()
    {
        UpdateAudioState();
    }

    void UpdateAudioState()
    {
        if (panelAudioPairs == null || panelAudioPairs.Length == 0)
            return;

        bool anyPanelActive = false;
        AudioSource activePanelAudio = null;

        // Check which panel is active and get its audio source
        foreach (PanelAudioPair pair in panelAudioPairs)
        {
            if (pair.panel != null && pair.panel.activeSelf)
            {
                anyPanelActive = true;
                activePanelAudio = pair.audioSource;
                break;
            }
        }

        if (anyPanelActive && activePanelAudio != null)
        {
            // A panel is ACTIVE - play its audio, pause background
            if (!activePanelAudio.isPlaying)
            {
                if (backgroundAudioSource != null && backgroundAudioSource.isPlaying)
                {
                    wasBackgroundPlaying = true;
                    backgroundAudioSource.Pause();
                }

                activePanelAudio.Play();
            }
        }
        else
        {
            // NO panels are active - stop all panel audio, resume background
            foreach (PanelAudioPair pair in panelAudioPairs)
            {
                if (pair.audioSource != null && pair.audioSource.isPlaying)
                {
                    pair.audioSource.Stop();
                }
            }

            // Resume background music
            if (backgroundAudioSource != null && wasBackgroundPlaying && !backgroundAudioSource.isPlaying)
            {
                backgroundAudioSource.Play();
                wasBackgroundPlaying = false;
            }
        }
    }
}

