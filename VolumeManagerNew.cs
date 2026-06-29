using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VolumeManagerNew : MonoBehaviour
{
    [Header("Audio References")]
    public AudioSource musicSource;
    public Slider volumeSlider;

    [Header("UI Buttons")]
    public UnityEngine.UI.Button volumeOnButton;
    public UnityEngine.UI.Button volumeOffButton;

    [Header("Default Settings")]
    [Range(0f, 1f)]
    public float defaultVolume = 1f;
    public bool startMuted = false;

    [Header("Scene Loading")]
    public string sceneToLoad;

    private bool musicOn = true;

    private void Awake()
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(defaultVolume);
            musicOn = !startMuted;

            if (musicOn)
            {
                if (!musicSource.isPlaying)
                    musicSource.Play();

                musicSource.mute = false;
            }
            else
            {
                musicSource.Pause();
                musicSource.mute = true;
            }
        }
    }

    private void Start()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = musicSource != null ? musicSource.volume : defaultVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (volumeOnButton != null)
            volumeOnButton.onClick.AddListener(() => SetMusicEnabled(true));

        if (volumeOffButton != null)
            volumeOffButton.onClick.AddListener(() => SetMusicEnabled(false));
    }

    public void SetVolume(float value)
    {
        float volume = Mathf.Clamp01(value);

        if (musicSource != null)
        {
            musicSource.volume = volume;
            if (volume > 0f && musicOn == false)
            {
                SetMusicEnabled(true);
            }
            else if (volume == 0f)
            {
                SetMusicEnabled(false);
            }
        }
    }

    public void ToggleMusic()
    {
        SetMusicEnabled(!musicOn);
    }

    public void VolumeOn()
    {
        SetMusicEnabled(true);
    }

    public void VolumeOff()
    {
        SetMusicEnabled(false);
    }

    public void SetMusicEnabled(bool enabled)
    {
        musicOn = enabled;

        if (musicSource == null)
            return;

        if (enabled)
        {
            musicSource.mute = false;
            if (!musicSource.isPlaying)
                musicSource.Play();
        }
        else
        {
            musicSource.mute = true;
            musicSource.Pause();
        }
    }

    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("VolumeManagerNew: sceneToLoad is empty. Set the scene name in the inspector.");
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("VolumeManagerNew: LoadScene called with empty sceneName.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
