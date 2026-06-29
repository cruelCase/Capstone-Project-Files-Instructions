using System.IO;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NewHeroLoader2 : MonoBehaviour
{
    [Header("UI References")]
    public GameObject heroDialoguePanel;
    public TMP_Text dialogueText;

    [Header("Hero Assets")]
    public GameObject[] heroPrefabs;          
    public string[] heroNames;          
    public AudioSource[] heroAudioSources;
    public Transform heroSpawnPoint;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f; // Delay between characters

    private Coroutine typingCoroutine;
    private GameObject activeHeroInstance;

    private void Start()
    {
        // Activate panel immediately
        if (heroDialoguePanel != null)
            heroDialoguePanel.SetActive(true);
        
        // Automatically load hero when this component initializes
        LoadHero();
    }

    public void LoadHero()
    {
        // Ensure panel is active
        if (heroDialoguePanel == null)
        {
            Debug.LogError("[LoadHero] heroDialoguePanel is not assigned in inspector!");
            return;
        }

        string heroName = null;
        ProfilePlayerData profile = LoadActiveUserProfile();
        if (profile != null)
            heroName = profile.hero;
        else if (GameManager.Instance != null)
            heroName = GameManager.Instance.SelectedCharacter;

        Debug.Log($"[LoadHero] Hero name from profile: {heroName}");

        if (string.IsNullOrEmpty(heroName))
        {
            Debug.LogWarning("[LoadHero] No hero selected!");
            return;
        }

        heroDialoguePanel.SetActive(true);

        // Destroy previous hero instance if it exists
        if (activeHeroInstance != null)
        {
            Destroy(activeHeroInstance);
            activeHeroInstance = null;
        }

        // Stop all hero voice audio
        foreach (var aud in heroAudioSources)
            if (aud != null) aud.Stop();

        // Find hero prefab index by name
        int heroIndex = FindHeroIndexByName(heroName);
        if (heroIndex < 0 || heroIndex >= heroPrefabs.Length)
        {
            Debug.LogWarning($"[LoadHero] Hero '{heroName}' not found in heroPrefabs array!");
            return;
        }

        // Instantiate hero prefab
        Transform targetParent = heroSpawnPoint != null ? heroSpawnPoint : (heroDialoguePanel != null ? heroDialoguePanel.transform : null);
        if (targetParent != null)
            activeHeroInstance = Instantiate(heroPrefabs[heroIndex], targetParent);
        else
            activeHeroInstance = Instantiate(heroPrefabs[heroIndex]);

        if (activeHeroInstance != null)
        {
            activeHeroInstance.transform.localPosition = Vector3.zero;
            activeHeroInstance.transform.localRotation = Quaternion.identity;
            activeHeroInstance.transform.localScale = Vector3.one;
        }

        Debug.Log($"[LoadHero] Hero '{heroName}' instantiated successfully!");

        // Play hero voice
        if (heroAudioSources.Length > heroIndex && heroAudioSources[heroIndex] != null)
            heroAudioSources[heroIndex].Play();

        // Dialogue text with typing effect
        if (dialogueText != null)
        {
            string dialogue =
                "Maligayang pagdating sa iyong huling Pagsusulit! " +
                "Tulad ng nakaraang pagkakataon, lahat ng mga tanong dito ay mula sa parehong paksa ng iyong nakaraang mundo na mundo 3. " +
                "Galingang Mo!";
            
            Debug.Log("[LoadHero] Starting dialogue typing effect");
            
            // Stop previous typing if any
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            
            // Start new typing effect
            typingCoroutine = StartCoroutine(TypeDialogue(dialogue));
        }
        else
        {
            Debug.LogWarning("[LoadHero] dialogueText is null!");
        }
    }

    private ProfilePlayerData LoadActiveUserProfile()
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        Debug.Log($"[LoadActiveUserProfile] ActiveUser: {activeUser}");
        
        if (string.IsNullOrEmpty(activeUser))
        {
            Debug.LogWarning("[LoadActiveUserProfile] No active user found!");
            return null;
        }

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        Debug.Log($"[LoadActiveUserProfile] Looking for profile at: {path}");
        
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[LoadActiveUserProfile] Profile file not found at: {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        Debug.Log($"[LoadActiveUserProfile] Profile JSON: {json}");
        
        return JsonUtility.FromJson<ProfilePlayerData>(json);
    }

    private int FindHeroIndexByName(string heroName)
    {
        if (string.IsNullOrEmpty(heroName) || heroNames == null)
        {
            Debug.LogWarning("[FindHeroIndexByName] heroName is empty or heroNames is null!");
            return -1;
        }

        for (int i = 0; i < heroNames.Length; i++)
        {
            if (!string.IsNullOrEmpty(heroNames[i]) && 
                string.Equals(heroNames[i].Trim(), heroName.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"[FindHeroIndexByName] Found hero '{heroName}' at index {i}");
                return i;
            }
        }

        Debug.LogWarning($"[FindHeroIndexByName] Hero '{heroName}' not found in heroNames array!");
        return -1;
    }

    private IEnumerator TypeDialogue(string text)
    {
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
