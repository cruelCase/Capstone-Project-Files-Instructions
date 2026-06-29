using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HeroLoader : MonoBehaviour
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

        // ⭐ Use ProfilePlayerData (your actual save-file class)
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);

        heroDialoguePanel.SetActive(true);

        Debug.Log($"Loading hero: {profile.hero} for user: {username}");

        // Hide all hero images
        foreach (var img in heroImages)
            img.gameObject.SetActive(false);

        // Stop all hero voice audio
        foreach (var aud in heroAudioSources)
            if (aud != null) aud.Stop();

        // Show the hero that matches profile.hero
        if (string.IsNullOrEmpty(profile.hero))
        {
            Debug.LogWarning($"No hero selected for user: {username}");
            heroDialoguePanel.SetActive(false);
            return;
        }

        string heroValue = profile.hero.Trim().ToLower();
        bool heroFound = false;

        for (int i = 0; i < heroNames.Length; i++)
        {
            string heroNameValue = heroNames[i].Trim().ToLower();
            Debug.Log($"Checking hero {i}: '{heroNames[i]}' against profile.hero: '{profile.hero}'");

            if (heroNameValue == heroValue ||
                heroNameValue.Contains(heroValue) ||
                heroValue.Contains(heroNameValue))
            {
                heroImages[i].gameObject.SetActive(true);
                heroFound = true;

                // Play hero voice
                if (heroAudioSources.Length > i && heroAudioSources[i] != null)
                    heroAudioSources[i].Play();

                Debug.Log($"Hero {profile.hero} loaded successfully using match '{heroNames[i]}'");
                break;
            }
        }

        if (!heroFound)
        {
            Debug.LogWarning($"Hero '{profile.hero}' not found in heroNames array!");
        }

        // Dialogue text
        if (dialogueText != null)
        {
            dialogueText.text =
                "Mga kababayan, tandaan natin na ang matalinong pagdedesisyon ay nagsisimula " +
                "sa pagkilala sa pagkakaiba ng pangangailangan at kagustuhan. Ang pangangailangan ay mga " +
                "bagay na mahalaga sa ating pamumuhay, samantalang ang kagustuhan ay mga bagay na nagbibigay " +
                "lamang ng dagdag na kasiyahan. Kung uunahin natin ang ating mga pangangailangan, " +
                "mas magiging maayos at responsable ang ating paggamit ng salapi at yaman. Oh eto 250 na pera para sa iyo. " +
                "Makakakuha ka ng mga puntos sa bawat minigame. Maaari ka ring makakuha ng puntos sa Rock, Paper, Scissors. Maaari mong ipalit ang mga puntos na ito sa salapi sa tindahan. ";
        }
    }
}
