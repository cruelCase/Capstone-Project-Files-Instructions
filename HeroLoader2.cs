using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HeroLoader2 : MonoBehaviour
{
    [Header("UI References")]
    public GameObject heroDialoguePanel;
    public TMP_Text dialogueText;

    [Header("Hero Assets")]
    public Image[] heroImages;          
    public string[] heroNames;          
    public AudioSource[] heroAudioSources;

    //private void Start()
    //{
    //    LoadHero();
    //}

    public void LoadHero()
    {
        string username = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("No active user found!");
            heroDialoguePanel.SetActive(false);
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, username + "_profile.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("Profile JSON not found: " + path);
            heroDialoguePanel.SetActive(false);
            return;
        }

        string json = File.ReadAllText(path);

        // Use ProfilePlayerData
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);

        heroDialoguePanel.SetActive(true);

        // Hide all hero images
        foreach (var img in heroImages)
            img.gameObject.SetActive(false);

        // Stop all hero audio
        foreach (var aud in heroAudioSources)
            if (aud != null) aud.Stop();

        // Show the hero that matches profile.hero
        if (string.IsNullOrEmpty(profile.hero))
        {
            Debug.LogWarning($"HeroLoader2: No hero selected for user: {username}");
            heroDialoguePanel.SetActive(false);
            return;
        }

        string heroValue = profile.hero.Trim().ToLower();
        bool heroFound = false;

        for (int i = 0; i < heroNames.Length; i++)
        {
            string heroNameValue = heroNames[i].Trim().ToLower();
            if (heroNameValue == heroValue ||
                heroNameValue.Contains(heroValue) ||
                heroValue.Contains(heroNameValue))
            {
                heroImages[i].gameObject.SetActive(true);
                heroFound = true;

                if (heroAudioSources.Length > i && heroAudioSources[i] != null)
                    heroAudioSources[i].Play();

                break;
            }
        }

        if (!heroFound)
        {
            Debug.LogWarning($"HeroLoader2: Hero '{profile.hero}' not found in heroNames array!");
        }

        // Dialogue text
        if (dialogueText != null)
        {
            dialogueText.text =
                "Mga kababayan, ang pagkonsumo ay ang paggamit ng mga produkto at serbisyo upang matugunan ang " +
                "ating mga pangangailangan at kagustuhan. Sa bawat paggastos, dapat nating isaalang-alang ang ating limitadong kita at ang tamang pagpili ng bibilhin. Ang pagiging " +
                "matalinong konsyumer ay nangangahulugang inuuna ang mahahalaga, naghahambing ng presyo at kalidad, at" +
                "iniiwasan ang pag-aaksaya. Sa ganitong paraan, nagagamit natin nang wasto ang ating mga yaman at pera para sa mas maayos na pamumuhay.";
        }
    }
}
