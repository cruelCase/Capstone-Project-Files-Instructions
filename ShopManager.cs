using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.IO;

public class ShopManager : MonoBehaviour
{
    [Header("UI Links")]
    public GameObject shopPanel;            // the shop panel to show/hide
    public TextMeshProUGUI infoText;        // text area for item descriptions and welcome message
    public TextMeshProUGUI goodByeText;     // goodbye text to show when exiting
    public TextMeshProUGUI moneyText;       // displays active user's currency
    public GameObject messagePanel;         // panel for warnings and confirmations
    public TextMeshProUGUI messageText;     // text shown inside the message panel

    [Header("Items")]
    public Button item1Button;              // item button (has Button component)
    public TextMeshProUGUI item1PriceText;
    public Button item2Button;
    public TextMeshProUGUI item2PriceText;

    [Header("Purchase")]
    public Button buyButton;                // buy button inside shop
    public GameObject confirmPanel;         // confirmation panel (image panel) with text + yes/no
    public TextMeshProUGUI confirmText;
    public Button confirmYesButton;
    public Button confirmNoButton;

    public Button exitButton;               // closes the shop

    // Item descriptions (can be set in Inspector)
    public string item1Description = "Item1: A useful item that helps your journey.";
    public string item2Description = "Item2: A special item for advanced players.";
    public int item1Cost = 50;
    public int item2Cost = 50;

    private int selectedItem = 0; // 1 or 2

    // typing coroutines
    private Coroutine typingCoroutine;
    private Coroutine goodbyeCoroutine;
    private Coroutine messageCoroutine;

    private void Start()
    {
        // Hook listeners (safely)
        if (exitButton != null) exitButton.onClick.AddListener(CloseShop);

        if (item1Button != null) item1Button.onClick.AddListener(() => OnItemPressed(1));
        if (item2Button != null) item2Button.onClick.AddListener(() => OnItemPressed(2));

        if (buyButton != null) buyButton.onClick.AddListener(OnBuyPressed);

        if (confirmYesButton != null) confirmYesButton.onClick.AddListener(OnConfirmYes);
        if (confirmNoButton != null) confirmNoButton.onClick.AddListener(OnConfirmNo);

        // Start state
        if (shopPanel != null) shopPanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);
        if (goodByeText != null) goodByeText.gameObject.SetActive(false);
        if (messagePanel != null) messagePanel.SetActive(false);
    }

    public void OpenShop()
    {
        // Complete Task 2 (Open the shop) if it was accepted
        TaskManager.CompleteTask(1);

        if (shopPanel != null) shopPanel.SetActive(true);
        if (confirmPanel != null) confirmPanel.SetActive(false);

        UpdateMoneyDisplay();
        UpdateItemPriceDisplays();
        UpdateBuyButtonInteractable();

        string welcome = "Maligayang pagdating sa Tindahan! Pumili ng isang gamit upang makita ang mga detalye nito.";
        StartTyping(infoText, welcome, 0.01f);
    }

    void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);

        string goodbye = "Salamat sa pagbisita sa tindahan! Sasusunod ulit.";
        // show and type goodbye, then hide after 2 seconds
        if (goodByeText != null)
        {
            goodByeText.gameObject.SetActive(true);
            if (goodbyeCoroutine != null) StopCoroutine(goodbyeCoroutine);
            goodbyeCoroutine = StartCoroutine(TypeTextCoroutine(goodByeText, goodbye, 0.03f, false, 2f));
        }
    }

    void OnItemPressed(int itemId)
    {
        selectedItem = itemId;
        string desc = itemId == 1 ? item1Description : item2Description;
        string priceLine = itemId == 1 ? $"\nPrice: {item1Cost}" : $"\nPrice: {item2Cost}";
        StartTyping(infoText, desc + priceLine, 0.01f);
        UpdateBuyButtonInteractable();
    }

    void OnBuyPressed()
    {
        if (selectedItem == 0) return; // nothing selected

        if (confirmPanel != null) confirmPanel.SetActive(true);
        string name = selectedItem == 1 ? "Item1" : "Item2";
        StartTyping(confirmText, $"Buy {name}? Are you sure?", 0.01f);
    }

    void OnConfirmYes()
    {
        if (selectedItem == 0)
        {
            if (confirmPanel != null) confirmPanel.SetActive(false);
            return;
        }

        int cost = selectedItem == 1 ? item1Cost : item2Cost;

        bool paymentOk = false;

        // Prefer GameManager for spending (will handle save)
        if (GameManager.Instance != null)
        {
            paymentOk = GameManager.Instance.SpendCurrency(cost);
        }
        else
        {
            // Fallback: adjust profile currency directly
            string path = GetProfilePath();
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                string json = File.ReadAllText(path);
                ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);
                if (profile != null && profile.currency >= cost)
                {
                    profile.currency -= cost;
                    File.WriteAllText(path, JsonUtility.ToJson(profile, true));
                    paymentOk = true;
                }
            }
        }

        if (!paymentOk)
        {
            ShowMessagePanel("Hindi sapat ang pera upang bilhin ang item.", 2f);
            if (confirmPanel != null) confirmPanel.SetActive(false);
            return;
        }

        // Payment succeeded — increment item count in profile
        IncrementActiveUserItem(selectedItem);

        // feedback
        StartTyping(infoText, "Matagumpay ang pagbili!", 0.01f);

        if (confirmPanel != null) confirmPanel.SetActive(false);

        // Update money display
        UpdateMoneyDisplay(true);
        UpdateBuyButtonInteractable();
    }

    void OnConfirmNo()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    void StartTyping(TextMeshProUGUI target, string content, float charDelay)
    {
        if (target == null) return;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeTextCoroutine(target, content, charDelay, false));
    }

    IEnumerator TypeTextCoroutine(TextMeshProUGUI target, string content, float charDelay, bool keepAfter, float hideDelay = 0f)
    {
        target.text = "";
        for (int i = 0; i < content.Length; i++)
        {
            target.text += content[i];
            yield return new WaitForSeconds(charDelay);
        }

        if (hideDelay > 0f)
        {
            yield return new WaitForSeconds(hideDelay);
            target.gameObject.SetActive(false);
        }

        typingCoroutine = null;
    }

    void ShowMessagePanel(string message, float duration = 2f)
    {
        if (messageText != null)
            messageText.text = message;

        if (messagePanel != null)
            messagePanel.SetActive(true);

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(HideMessagePanelAfterDelay(duration));
    }

    IEnumerator HideMessagePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (messagePanel != null)
            messagePanel.SetActive(false);
        messageCoroutine = null;
    }

    // Profile helpers
    private string GetProfilePath()
    {
        string user = PlayerPrefs.GetString("ActiveUser", "");
        return Path.Combine(Application.persistentDataPath, user + "_profile.json");
    }

    void IncrementActiveUserItem(int itemId)
    {
        string path = GetProfilePath();
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Debug.LogWarning("ShopManager: No active profile found to update item count.");
            return;
        }

        string json = File.ReadAllText(path);
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);
        if (profile == null)
        {
            Debug.LogWarning("ShopManager: Failed to parse profile JSON.");
            return;
        }

        if (itemId == 1) profile.item1 += 1;
        else if (itemId == 2) profile.item2 += 1;

        File.WriteAllText(path, JsonUtility.ToJson(profile, true));

        // Refresh GameManager/profile displays if available
        if (GameManager.Instance != null)
            GameManager.Instance.RefreshProfileData();

        // Update displayed money (currency) since profile changed
        UpdateMoneyDisplay();
    }

    void UpdateMoneyDisplay(bool animate = false)
    {
        if (moneyText == null) return;

        string path = GetProfilePath();
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            moneyText.text = "0";
            return;
        }

        string json = File.ReadAllText(path);
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);
        if (profile != null)
        {
            moneyText.text = profile.currency.ToString();
            if (animate)
                AnimateMoneyText();
        }
        else
        {
            moneyText.text = "0";
        }
    }

    public void Convert50PointsTo10Currency()
    {
        const int pointsCost = 50;
        const int currencyReward = 10;

        string path = GetProfilePath();
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            StartTyping(infoText, "Hindi ma-convert ang mga puntos: walang nahanap na aktibong profile.", 0.01f);
            return;
        }

        string json = File.ReadAllText(path);
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);
        if (profile == null)
        {
            StartTyping(infoText, "Hindi ma-convert ang mga puntos: invalid na data ng profile.", 0.01f);
            return;
        }

        if (profile.points < pointsCost)
        {
            StartTyping(infoText, "Hindi sapat ang mga puntos para i-convert. Kailangan mo ng 50 puntos.", 0.01f);
            return;
        }

        profile.points -= pointsCost;
        profile.currency += currencyReward;
        File.WriteAllText(path, JsonUtility.ToJson(profile, true));

        if (GameManager.Instance != null)
            GameManager.Instance.RefreshProfileData();

        if (ProfileManager.Instance != null)
            ProfileManager.Instance.LoadProfile();

        UpdateMoneyDisplay(true);
        StartTyping(infoText, $"Na Convert na ang {pointsCost} pontus sa {currencyReward} na pera!", 0.01f);
    }

    void UpdateItemPriceDisplays()
    {
        if (item1PriceText != null)
            item1PriceText.text = $"Price: {item1Cost}";
        if (item2PriceText != null)
            item2PriceText.text = $"Price: {item2Cost}";
    }

    void UpdateBuyButtonInteractable()
    {
        if (buyButton == null) return;

        if (selectedItem == 0)
        {
            buyButton.interactable = false;
            return;
        }

        string path = GetProfilePath();
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            buyButton.interactable = false;
            return;
        }

        string json = File.ReadAllText(path);
        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(json);
        if (profile == null)
        {
            buyButton.interactable = false;
            return;
        }

        int cost = selectedItem == 1 ? item1Cost : item2Cost;
        buyButton.interactable = profile.currency >= cost;
    }

    void AnimateMoneyText()
    {
        if (moneyText == null) return;
        StartCoroutine(AnimateMoneyCoroutine());
    }

    IEnumerator AnimateMoneyCoroutine()
    {
        Vector3 originalScale = moneyText.rectTransform.localScale;
        float elapsed = 0f;
        float duration = 0.35f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin((elapsed / duration) * Mathf.PI) * 0.2f;
            moneyText.rectTransform.localScale = originalScale * scale;
            yield return null;
        }

        moneyText.rectTransform.localScale = originalScale;
    }

    /// <summary>
    /// Reloads the active user data from ProfileManager and GameManager.
    /// Call this to sync data after purchases or when you need to refresh.
    /// Can be assigned to a button in the Inspector.
    /// </summary>
    public void ReloadActiveUserData()
    {
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.LoadProfile();
            Debug.Log("ShopManager: ProfileManager data reloaded.");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RefreshProfileData();
            Debug.Log("ShopManager: GameManager profile data refreshed.");
        }

        // Optionally update the money display
        UpdateMoneyDisplay();

        Debug.Log("ShopManager: Active user data reloaded successfully.");
    }
}
