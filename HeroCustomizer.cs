using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.U2D.Animation;

public class HeroCustomizer : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI[] currencyTexts;  // Currency display for each panel - assign all currency texts
    public Image insufficientCurrencyImage;
    public Image successfulSwitchImage;
    public Image equippedImage;  // Shows when costume is already owned/equipped
    
    [Header("Hero Panel References")]
    public GameObject[] heroPanels;  // Panels for each hero in same order as heroNames
    public Transform[] heroSpawnPoints;  // Spawn point for each hero - same order as heroPanels
    
    [Header("Player Reference (Optional - for instant costume update)")]
    public GameObject playerGameObject;  // Assign the active player character here
    
    [Header("Hero Prefabs")]
    public GameObject[] heroPrefabs;  // Hero prefabs in same order as heroPanels and heroNames
    
    [Header("Hero Names (Developer Setup - Match Order with Prefabs & Panels)")]
    public string[] heroNames = new string[] 
    { 
        "Gabriela Silang", 
        "Jose Rizal",
        // Add more hero names here in order
    };
    
    [Header("Hero Costume Data (One set per hero in same order)")]
    public HeroCostumeSet[] heroCostumeSets;  // One for each hero - parallel to heroNames
    
    private ProfilePlayerData activeUserProfile;
    private GameObject currentHeroInstance;
    private int currentHeroIndex = -1;
    private SpriteLibrary currentSpriteLibrary;
    
    void Start()
    {
        LoadActiveUser();
        if (activeUserProfile != null)
        {
            // Don't auto-display, wait for button press to show active user's hero panel
        }
    }
    
    void LoadActiveUser()
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser))
        {
            Debug.LogError("No active user found!");
            return;
        }
        
        string path = System.IO.Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!System.IO.File.Exists(path))
        {
            Debug.LogError($"Profile file not found: {path}");
            return;
        }
        
        string json = System.IO.File.ReadAllText(path);
        activeUserProfile = JsonUtility.FromJson<ProfilePlayerData>(json);
        
        Debug.Log($"Loaded profile for: {activeUserProfile.username}, Hero: {activeUserProfile.hero}, Costume: {activeUserProfile.heroCostume}");
    }
    
    // Attach this to the "Customize Hero" button - shows the panel for active user's current hero
    public void ShowActiveUserHeroPanel()
    {
        // Reload active user profile to get latest currency (in case of conversions or updates)
        LoadActiveUser();

        if (activeUserProfile == null)
        {
            Debug.LogError("No active user profile loaded!");
            return;
        }
        
        DisplayHero(activeUserProfile.hero);
        UpdateCurrencyDisplay();
    }
    
    public void DisplayHero(string heroName)
    {
        // Find the index of this hero in our array
        currentHeroIndex = System.Array.IndexOf(heroNames, heroName);
        
        if (currentHeroIndex < 0)
        {
            Debug.LogWarning($"Hero '{heroName}' not found in heroNames array!");
            return;
        }
        
        // Deactivate all panels
        foreach (GameObject panel in heroPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
        
        // Activate the correct panel
        if (currentHeroIndex < heroPanels.Length && heroPanels[currentHeroIndex] != null)
            heroPanels[currentHeroIndex].SetActive(true);
        
        // Spawn the hero
        SpawnHero(currentHeroIndex);
    }
    
    void SpawnHero(int heroIndex)
    {
        // Destroy previous hero
        if (currentHeroInstance != null)
            Destroy(currentHeroInstance);
        
        // Spawn new hero
        if (heroIndex >= 0 && heroIndex < heroPrefabs.Length && heroPrefabs[heroIndex] != null)
        {
            // Get spawn point for this hero
            Transform spawnPoint = null;
            if (heroIndex < heroSpawnPoints.Length && heroSpawnPoints[heroIndex] != null)
                spawnPoint = heroSpawnPoints[heroIndex];
            
            if (spawnPoint != null)
                currentHeroInstance = Instantiate(heroPrefabs[heroIndex], spawnPoint);
            else
                currentHeroInstance = Instantiate(heroPrefabs[heroIndex]);
            
            if (currentHeroInstance != null)
            {
                currentHeroInstance.transform.localPosition = Vector3.zero;
                currentHeroInstance.transform.localRotation = Quaternion.identity;
                currentHeroInstance.transform.localScale = Vector3.one;
                
                // Get SpriteLibrary component for costume switching
                currentSpriteLibrary = currentHeroInstance.GetComponent<SpriteLibrary>();
                if (currentSpriteLibrary == null)
                    currentSpriteLibrary = currentHeroInstance.GetComponentInChildren<SpriteLibrary>();
                
                // Apply current costume's sprite library
                ApplyCurrentCostume();
            }
        }
    }
    
    void ApplyCurrentCostume()
    {
        if (activeUserProfile == null || currentHeroIndex < 0)
            return;
        
        if (currentHeroIndex >= heroCostumeSets.Length)
        {
            Debug.LogWarning($"No costume set found for hero index {currentHeroIndex}");
            return;
        }
        
        HeroCostumeSet costumeSet = heroCostumeSets[currentHeroIndex];
        
        // Find the costume that matches the current heroCostume
        HeroCostume currentCostume = null;
        foreach (HeroCostume costume in costumeSet.costumes)
        {
            if (costume.costumeName == activeUserProfile.heroCostume)
            {
                currentCostume = costume;
                break;
            }
        }
        
        if (currentCostume != null && currentSpriteLibrary != null && currentCostume.spriteLibraryAsset != null)
        {
            currentSpriteLibrary.spriteLibraryAsset = currentCostume.spriteLibraryAsset;
            Debug.Log($"Applied costume: {currentCostume.costumeName}");
        }
    }
    
    // Attach this to a button to show a specific hero panel
    public void ShowHeroPanel(int heroIndex)
    {
        if (heroIndex >= 0 && heroIndex < heroNames.Length && activeUserProfile != null)
        {
            activeUserProfile.hero = heroNames[heroIndex];
            DisplayHero(heroNames[heroIndex]);
        }
    }
    
    // Attach this to costume buttons
    // Example: costumeSwitcher.SwitchCostume(0) for first costume, SwitchCostume(1) for second, etc.
    public void SwitchCostume(int costumeIndex)
    {
        if (activeUserProfile == null)
        {
            Debug.LogError("No active user profile!");
            return;
        }
        
        if (currentHeroIndex < 0 || currentHeroIndex >= heroCostumeSets.Length)
        {
            Debug.LogError("No valid hero selected!");
            return;
        }
        
        HeroCostumeSet costumeSet = heroCostumeSets[currentHeroIndex];
        
        if (costumeIndex < 0 || costumeIndex >= costumeSet.costumes.Length)
        {
            Debug.LogError($"Invalid costume index: {costumeIndex}");
            return;
        }
        
        HeroCostume costume = costumeSet.costumes[costumeIndex];
        string costumeId = $"{heroNames[currentHeroIndex]}_{costume.costumeName}";
        
        // Check if costume is already owned
        bool isOwned = System.Array.Exists(activeUserProfile.ownedCostumes, element => element == costumeId);
        
        if (isOwned)
        {
            // Already owned - just switch to it if not already equipped
            if (activeUserProfile.heroCostume == costume.costumeName)
            {
                Debug.Log($"Already equipped: {costume.costumeName}");
                ShowFeedbackImage(equippedImage);
                return;
            }
            
            // Switch to owned costume without charging
            activeUserProfile.heroCostume = costume.costumeName;
            
            // Apply the costume immediately
            if (currentSpriteLibrary != null && costume.spriteLibraryAsset != null)
            {
                currentSpriteLibrary.spriteLibraryAsset = costume.spriteLibraryAsset;
                Debug.Log($"Switched to owned costume: {costume.costumeName}");
            }
            
            // Also update the player character
            if (playerGameObject != null)
            {
                SpriteLibrary playerSpriteLibrary = playerGameObject.GetComponent<SpriteLibrary>();
                if (playerSpriteLibrary == null)
                    playerSpriteLibrary = playerGameObject.GetComponentInChildren<SpriteLibrary>();
                
                if (playerSpriteLibrary != null && costume.spriteLibraryAsset != null)
                {
                    playerSpriteLibrary.spriteLibraryAsset = costume.spriteLibraryAsset;
                    Debug.Log($"Player character also switched to: {costume.costumeName}");
                }
            }
            
            SaveActiveUserProfile();
            ShowFeedbackImage(equippedImage);
            return;
        }
        
        // Not owned yet - check currency and purchase
        if (activeUserProfile.currency < costume.cost)
        {
            Debug.LogWarning($"Not enough currency! Need {costume.cost}, have {activeUserProfile.currency}");
            ShowFeedbackImage(insufficientCurrencyImage);
            return;
        }
        
        // Deduct currency
        activeUserProfile.currency -= costume.cost;
        
        // Add costume to owned list
        System.Array.Resize(ref activeUserProfile.ownedCostumes, activeUserProfile.ownedCostumes.Length + 1);
        activeUserProfile.ownedCostumes[activeUserProfile.ownedCostumes.Length - 1] = costumeId;
        
        // Update heroCostume in profile
        activeUserProfile.heroCostume = costume.costumeName;
        
        // Apply the costume immediately to the displayed hero prefab
        if (currentSpriteLibrary != null && costume.spriteLibraryAsset != null)
        {
            currentSpriteLibrary.spriteLibraryAsset = costume.spriteLibraryAsset;
            Debug.Log($"Purchased and switched to costume: {costume.costumeName}, Cost: {costume.cost}, Remaining currency: {activeUserProfile.currency}");
        }
        
        // Also update the player character in the game if assigned
        if (playerGameObject != null)
        {
            SpriteLibrary playerSpriteLibrary = playerGameObject.GetComponent<SpriteLibrary>();
            if (playerSpriteLibrary == null)
                playerSpriteLibrary = playerGameObject.GetComponentInChildren<SpriteLibrary>();
            
            if (playerSpriteLibrary != null && costume.spriteLibraryAsset != null)
            {
                playerSpriteLibrary.spriteLibraryAsset = costume.spriteLibraryAsset;
                Debug.Log($"Player character costume also updated to: {costume.costumeName}");
            }
        }
        
        // Save profile
        SaveActiveUserProfile();
        UpdateCurrencyDisplay();
        
        // Show success feedback
        ShowFeedbackImage(successfulSwitchImage);
    }
    
    void ShowFeedbackImage(Image feedbackImage)
    {
        if (feedbackImage == null)
            return;
        
        // Make sure it's active
        feedbackImage.gameObject.SetActive(true);
        
        // Start coroutine to hide after 2 seconds
        StartCoroutine(HideFeedbackImageAfterDelay(feedbackImage, 2f));
    }
    
    System.Collections.IEnumerator HideFeedbackImageAfterDelay(Image feedbackImage, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (feedbackImage != null)
            feedbackImage.gameObject.SetActive(false);
    }
    
    // Call this when closing the customization panel to finalize changes
    public void CloseCustomizationPanel()
    {
        if (currentHeroInstance != null)
            Destroy(currentHeroInstance);
        currentHeroInstance = null;
        currentHeroIndex = -1;
        
        // All changes are already saved in SwitchCostume, just clean up UI
        foreach (GameObject panel in heroPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }
    
    void UpdateCurrencyDisplay()
    {
        if (activeUserProfile != null && currencyTexts != null)
        {
            foreach (TextMeshProUGUI currencyText in currencyTexts)
            {
                if (currencyText != null)
                    currencyText.text = activeUserProfile.currency.ToString();
            }
        }
    }
    
    void SaveActiveUserProfile()
    {
        if (activeUserProfile == null) return;
        
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser)) return;
        
        string path = System.IO.Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        System.IO.File.WriteAllText(path, JsonUtility.ToJson(activeUserProfile, true));
        Debug.Log($"Profile saved: {activeUser}_profile.json");
    }
    
    // Other scripts (like HeroPanelManager) can call this to get the correct SpriteLibraryAsset
    // based on the active user's current heroCostume
    public static SpriteLibraryAsset GetActiveUserHeroCostumeSpriteLibrary(HeroCustomizer customizer, string heroName, string costumeName)
    {
        if (customizer == null)
            return null;
        
        // Find hero index
        int heroIndex = System.Array.IndexOf(customizer.heroNames, heroName);
        if (heroIndex < 0 || heroIndex >= customizer.heroCostumeSets.Length)
            return null;
        
        // Find costume
        HeroCostumeSet costumeSet = customizer.heroCostumeSets[heroIndex];
        foreach (HeroCostume costume in costumeSet.costumes)
        {
            if (costume.costumeName == costumeName)
                return costume.spriteLibraryAsset;
        }
        
        return null;
    }
}

[System.Serializable]
public class HeroCostumeSet
{
    public HeroCostume[] costumes;
}

[System.Serializable]
public class HeroCostume
{
    public string costumeName;  // e.g., "JoseRizalWarrior", "JoseRizalRock", "DEFAULT"
    public int cost;
    public SpriteLibraryAsset spriteLibraryAsset;  // Assign in inspector
}
