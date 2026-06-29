using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyButtonManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button deduct50Button;   // Deducts 50
    public Button deduct100Button;  // Deducts 100
    public Button add250Button;     // Adds 250

    [Header("Currency Amounts")]
    public int deduct50Amount = -50;
    public int deduct100Amount = -100;
    public int add250Amount = 250;

    [Header("UI Panels")]
    public GameObject insufficientCurrencyPanel;
    public float insufficientCurrencyDuration = 2f;

    private Coroutine insufficientPanelCoroutine;

    private void Start()
    {
        if (deduct50Button != null)
            deduct50Button.onClick.AddListener(() => ModifyCurrency(deduct50Amount));

        if (deduct100Button != null)
            deduct100Button.onClick.AddListener(() => ModifyCurrency(deduct100Amount));

        if (add250Button != null)
            add250Button.onClick.AddListener(() => ModifyCurrency(add250Amount));
    }

    private void ModifyCurrency(int amount)
    {
        if (amount < 0 && !HasEnoughCurrency(Mathf.Abs(amount)))
        {
            ShowInsufficientCurrencyPanel();
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCurrency(amount);
            Debug.Log($"Currency modified by {amount}. Current: {GameManager.Instance.Currency}");
            return;
        }

        SaveCurrencyToActiveUser(amount);
    }

    public void Add250CurrencyToActiveUser()
    {
        ModifyCurrency(add250Amount);
    }

    private bool HasEnoughCurrency(int cost)
    {
        int currentCurrency = GetActiveUserCurrency();
        return currentCurrency >= cost;
    }

    private int GetActiveUserCurrency()
    {
        if (GameManager.Instance != null)
            return GameManager.Instance.Currency;

        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser))
            return 0;

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        if (!File.Exists(path))
            return 0;

        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));
        if (profile == null)
            return 0;

        return profile.currency;
    }

    private void ShowInsufficientCurrencyPanel()
    {
        if (insufficientCurrencyPanel == null)
            return;

        insufficientCurrencyPanel.SetActive(true);

        if (insufficientPanelCoroutine != null)
            StopCoroutine(insufficientPanelCoroutine);

        insufficientPanelCoroutine = StartCoroutine(HideInsufficientPanelAfterDelay(insufficientCurrencyDuration));
    }

    private IEnumerator HideInsufficientPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (insufficientCurrencyPanel != null)
            insufficientCurrencyPanel.SetActive(false);

        insufficientPanelCoroutine = null;
    }

    private void SaveCurrencyToActiveUser(int amount)
    {
        string activeUser = PlayerPrefs.GetString("ActiveUser", "");
        if (string.IsNullOrEmpty(activeUser))
        {
            Debug.LogWarning("CurrencyButtonManager: No active user to save currency for.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, activeUser + "_profile.json");
        ProfilePlayerData profile;

        if (File.Exists(path))
        {
            profile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));
        }
        else
        {
            profile = new ProfilePlayerData { username = activeUser };
        }

        profile.currency += amount;
        File.WriteAllText(path, JsonUtility.ToJson(profile, true));

        Debug.Log($"CurrencyButtonManager: Added {amount} currency to {activeUser}. New total: {profile.currency}");
    }
}
