using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class SpriteLibraryShopSaver : MonoBehaviour
{
    [Header("UI Slots")]
    public Transform currencyTextSlot; // assign the currency text Transform (TextMeshProUGUI or Text)
    public Transform purchaseNotificationSlot; // assign a UI element to enable/disable on successful purchase
    public Transform notEnoughMoneySlot; // UI element to show when player lacks currency

    private TextMeshProUGUI _currencyTMP;
    private Text _currencyUI;

    private void Start()
    {
        // Try to ensure GameManager singleton exists
        if (GameManager.Instance == null)
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null) GameManager.Instance = gm;
        }

        // Wire currency UI to GameManager if a slot is assigned
        if (currencyTextSlot != null && GameManager.Instance != null)
        {
            _currencyTMP = currencyTextSlot.GetComponent<TextMeshProUGUI>() ?? currencyTextSlot.GetComponentInChildren<TextMeshProUGUI>();
            _currencyUI = currencyTextSlot.GetComponent<Text>() ?? currencyTextSlot.GetComponentInChildren<Text>();

            if (_currencyTMP != null) GameManager.Instance.currencyText = _currencyTMP;
            else if (_currencyUI != null) GameManager.Instance.currencyText = null; // GameManager uses TMP; keep null if using UI.Text

            // Refresh UI from GameManager
            GameManager.Instance.ApplyDataToUI();
        }

        if (purchaseNotificationSlot != null) purchaseNotificationSlot.gameObject.SetActive(false);
        if (notEnoughMoneySlot != null) notEnoughMoneySlot.gameObject.SetActive(false);
    }
    private string GetProfilePath()
    {
        string user = PlayerPrefs.GetString("ActiveUser", "");
        return Path.Combine(Application.persistentDataPath, user + "_profile.json");
    }

    // Costs for each sprite library
    private int originalCost = 0;
    private int villager2Cost = 150;
    private int villager3Cost = 150;

    // Public methods called by buttons
    public void SelectOriginal() => TryBuyLibrary("Original", originalCost);
    public void SelectVillager2() => TryBuyLibrary("Villager2", villager2Cost);
    public void SelectVillager3() => TryBuyLibrary("Villager3", villager3Cost);

    // Attempt to buy and save the selected library
    private void TryBuyLibrary(string libraryName, int cost)
    {
        // Ensure we have a GameManager
        if (GameManager.Instance == null)
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null) GameManager.Instance = gm;
            else
            {
                Debug.LogError("SpriteLibraryShopSaver: No GameManager found in scene.");
                return;
            }
        }

        // If already purchased, just switch (no cost)
        if (IsPurchased(libraryName))
        {
            Save(libraryName);
            Debug.Log($"{libraryName} switched (already purchased).");
            StartCoroutine(ShowPurchaseNotification());
            return;
        }

        // Attempt to spend currency and purchase
        if (GameManager.Instance.SpendCurrency(cost))
        {
            // Mark purchased and save selection to profile
            Save(libraryName, markPurchased: true);
            Debug.Log($"{libraryName} purchased and selected!");
            StartCoroutine(ShowPurchaseNotification());
        }
        else
        {
            // Not enough currency, show message
            Debug.Log($"Cannot select {libraryName}. You need {cost} currency.");
            StartCoroutine(ShowNotEnoughMoneyNotification());
        }
    }

    // Save the selected library to the profile JSON
    private void Save(string value, bool markPurchased = false)
    {
        string path = GetProfilePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning("Profile file not found: " + path);
            return;
        }

        // Load existing profile data
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));

        // Update selected sprite library
        profile.selectedSpriteLibrary = value;

        // Mark purchased flags when applicable
        if (markPurchased)
        {
            if (value == "Villager2") profile.purchasedVillager2 = true;
            if (value == "Villager3") profile.purchasedVillager3 = true;
        }

        // Save back to file
        File.WriteAllText(path, JsonUtility.ToJson(profile, true));
        Debug.Log("Profile saved: " + value);

        // Refresh GameManager in-memory data if present
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RefreshProfileData();
            GameManager.Instance.ApplyDataToUI();
        }
    }

    private bool IsPurchased(string libraryName)
    {
        string path = GetProfilePath();
        if (!File.Exists(path)) return false;
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));
        switch (libraryName)
        {
            case "Villager2": return profile.purchasedVillager2;
            case "Villager3": return profile.purchasedVillager3;
            default: return true; // Original always available
        }
    }

    private IEnumerator ShowPurchaseNotification()
    {
        if (purchaseNotificationSlot == null) yield break;
        GameObject go = purchaseNotificationSlot.gameObject;
        go.SetActive(true);
        yield return new WaitForSeconds(2f);
        go.SetActive(false);
    }

    private IEnumerator ShowNotEnoughMoneyNotification()
    {
        if (notEnoughMoneySlot == null) yield break;
        GameObject go = notEnoughMoneySlot.gameObject;
        go.SetActive(true);
        yield return new WaitForSeconds(2f);
        go.SetActive(false);
    }
}