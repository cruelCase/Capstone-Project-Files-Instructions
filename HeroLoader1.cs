using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HeroLoader1 : MonoBehaviour
{
    [Header("UI References")]
    public GameObject heroDialoguePanel;
    public TMP_Text dialogueText;

    [Header("Hero Assets")]
    public Image[] heroImages;
    public string[] heroNames;
    public AudioSource[] heroAudioSources;

    // Do NOT auto-run. Comic will call this.
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

        // Load profile data
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);

        heroDialoguePanel.SetActive(true);

        Debug.Log($"HeroLoader1: Loading hero: '{profile.hero}' for user: {username}");

        // Hide all hero images
        foreach (var img in heroImages)
            img.gameObject.SetActive(false);

        // Stop all hero audio
        foreach (var aud in heroAudioSources)
            if (aud != null) aud.Stop();

        // Show hero + play matching audio
        if (string.IsNullOrEmpty(profile.hero))
        {
            Debug.LogWarning($"HeroLoader1: No hero selected for user: {username}");
            heroDialoguePanel.SetActive(false);
            return;
        }

        string heroValue = profile.hero.Trim().ToLower();
        bool heroFound = false;

        for (int i = 0; i < heroNames.Length; i++)
        {
            string heroNameValue = heroNames[i].Trim().ToLower();
            Debug.Log($"HeroLoader1: Checking hero {i}: '{heroNames[i]}' against profile.hero: '{profile.hero}'");

            if (heroNameValue == heroValue ||
                heroNameValue.Contains(heroValue) ||
                heroValue.Contains(heroNameValue))
            {
                heroImages[i].gameObject.SetActive(true);
                heroFound = true;

                if (heroAudioSources.Length > i && heroAudioSources[i] != null)
                    heroAudioSources[i].Play();

                Debug.Log($"HeroLoader1: Hero '{profile.hero}' loaded successfully using match '{heroNames[i]}'");
                break;
            }
        }

        if (!heroFound)
        {
            Debug.LogWarning($"HeroLoader1: Hero '{profile.hero}' not found in heroNames array!");
        }

        // Dialogue text for HeroLoader1
        if (dialogueText != null)
        {
            dialogueText.text =
                "Mga kababayan, ang kita ang salaping ating natatanggap mula sa ating hanapbuhayo negosyo. Mula sa kitang ito, " +
                "maaari tayong gumastos para sa ating mga pangangailangan at kagustuhan, na tinatawag na pagkonsumo." +
                "Kung may matitirang salapi matapos ang paggastos, ito ay maaaring itabi bilang pag-iimpok. " +
                "Kaya naman, may mahalagang ugnayan ang kita, pag-iimpok, at pagkonsumo: habang tumataas ang kita," +
                "mas nagkakaroon tayo ng kakayahang gumastos at mag-ipon. Ang wastong pagbabalanse ng mga ito ay susi sa matatag na pananalapi at mas magandang kinabukasan.";
        }
    }
}
