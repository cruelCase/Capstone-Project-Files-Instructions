using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreHero : MonoBehaviour
{
    [Header("UI References")]
    public Image[] heroImages;          
    public string[] heroNames;          
    public AudioSource[] heroAudioSources;
    public TMP_Text dialogueText;

    private void Start()
    {
        LoadHero();
    }

    public void LoadHero()
    {
        string username = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("ScoreHero: No active user found!");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, username + "_profile.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("ScoreHero: Profile JSON not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);

        Debug.Log($"ScoreHero: Loading hero: {profile.hero} for user: {username}");

        // Hide all hero images
        if (heroImages != null)
            foreach (var img in heroImages)
                if (img != null) img.gameObject.SetActive(false);

        // Stop all hero voice audio
        if (heroAudioSources != null)
            foreach (var aud in heroAudioSources)
                if (aud != null) aud.Stop();

        // Show the hero that matches profile.hero
        if (string.IsNullOrEmpty(profile.hero))
        {
            Debug.LogWarning($"ScoreHero: No hero selected for user: {username}");
            return;
        }

        string heroValue = profile.hero.Trim().ToLower();
        bool heroFound = false;

        if (heroNames != null)
        {
            for (int i = 0; i < heroNames.Length; i++)
            {
                string heroNameValue = heroNames[i].Trim().ToLower();
                Debug.Log($"ScoreHero: Checking hero {i}: '{heroNames[i]}' against profile.hero: '{profile.hero}'");

                if (heroNameValue == heroValue ||
                    heroNameValue.Contains(heroValue) ||
                    heroValue.Contains(heroNameValue))
                {
                    if (heroImages != null && heroImages.Length > i && heroImages[i] != null)
                        heroImages[i].gameObject.SetActive(true);
                    heroFound = true;

                    // Play hero voice
                    if (heroAudioSources != null && heroAudioSources.Length > i && heroAudioSources[i] != null)
                        heroAudioSources[i].Play();

                    Debug.Log($"ScoreHero: Hero {profile.hero} loaded successfully using match '{heroNames[i]}'");
                    break;
                }
            }
        }

        if (!heroFound)
        {
            Debug.LogWarning($"ScoreHero: Hero '{profile.hero}' not found in heroNames array!");
        }

        // Dialogue text
        if (dialogueText != null)
        {
            dialogueText.text =
                "Muntik ka nang ma-scam, dapat magpasalamat ka sa akin. " +
                "Sa susunod mag-ingat ka kapag bibili ka ng kahit ano. " +
                "Dapat mong matutunan ang konsepto ng pangangailangan vs kagustuhan. " +
                "Marahil ay makakahanap ka ng ilang mga aralin sa lugar.";
        }
    }
}
