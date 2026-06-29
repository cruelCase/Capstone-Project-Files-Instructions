using System.Collections;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;
    public float messageDuration = 2.5f;
    public string successSceneName = "NewScene1";

    [Header("Error Message Images (Assign in Inspector)")]
    public Image errorEmptyInputImage;
    public Image errorAccountNotFoundImage;
    public Image errorReadingDataImage;
    public Image errorIncorrectPasswordImage;
    public Image successMessageImage;
    public Transform activeUserDisplay;
    public Transform activeUserBackground;
    public Transform logoutButton;

    [Header("Panels and Buttons")]
    public Transform loginPanel;
    public Transform registerPanel;
    public Transform playButton;
    public Transform startButton;
    // Button only usable when a user is logged in. Assign the Button's Transform here.
    public Transform loggedInOnlyButton;
    // Optional overlay Transform that will catch clicks when the above button is visually disabled (assign an invisible Button here).
    public Transform loggedInOnlyButtonCatcher;
    // Popup/message that shows for a short time when the uninteractable button is clicked.
    public Transform blockedClickPopup;
    public float blockedMessageDuration = 2f;

    public void LoginUser()
    {
        string username = usernameInput != null ? usernameInput.text.Trim() : string.Empty;
        string password = passwordInput != null ? passwordInput.text : string.Empty;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowErrorImage(errorEmptyInputImage);
            return;
        }

        string path = GetProfilePath(username);
        if (!File.Exists(path))
        {
            ShowErrorImage(errorAccountNotFoundImage);
            return;
        }

        ProfilePlayerData profile = JsonUtility.FromJson<ProfilePlayerData>(File.ReadAllText(path));
        if (profile == null)
        {
            ShowErrorImage(errorReadingDataImage);
            return;
        }

        if (profile.password != password)
        {
            ShowErrorImage(errorIncorrectPasswordImage);
            return;
        }

        PlayerPrefs.SetString("ActiveUser", username);
        PlayerPrefs.Save();

        if (usernameInput != null)
            usernameInput.text = string.Empty;
        if (passwordInput != null)
            passwordInput.text = string.Empty;

        if (activeUserBackground != null)
            activeUserBackground.gameObject.SetActive(true);

        if (activeUserDisplay != null)
        {
            TMP_Text activeUserText = activeUserDisplay.GetComponent<TMP_Text>();
            if (activeUserText == null)
                activeUserText = activeUserDisplay.GetComponentInChildren<TMP_Text>();

            if (activeUserText != null)
                activeUserText.text = username;

            activeUserDisplay.gameObject.SetActive(true);
        }

        if (logoutButton != null)
            logoutButton.gameObject.SetActive(true);

        // Show the success image first, then perform the UI transition after a short delay
        ShowErrorImage(successMessageImage);
        StartCoroutine(TransitionAfterSuccess());

        // Update any logged-in-only UI
        UpdateLoggedInOnlyButtonState();
    }

    private string GetProfilePath(string username)
    {
        return Path.Combine(Application.persistentDataPath, username + "_profile.json");
    }

    private void ShowErrorImage(Image errorImage)
    {
        if (errorImage != null)
        {
            // don't stop all coroutines here — other coroutines (like transitions) should not be interrupted
            errorImage.gameObject.SetActive(true);
            StartCoroutine(HideImageAfterDelay(errorImage));
        }
    }

    private IEnumerator HideImageAfterDelay(Image errorImage)
    {
        yield return new WaitForSeconds(messageDuration);

        if (errorImage != null)
            errorImage.gameObject.SetActive(false);
    }

    private IEnumerator TransitionAfterSuccess()
    {
        float delay = Mathf.Max(2f, messageDuration);
        yield return new WaitForSeconds(delay);

        if (loginPanel != null)
            loginPanel.gameObject.SetActive(false);

        // Ensure register panel is hidden when showing login transitions
        if (registerPanel != null)
            registerPanel.gameObject.SetActive(false);

        if (playButton != null)
            playButton.gameObject.SetActive(false);

        if (startButton != null)
            startButton.gameObject.SetActive(true);
    }

    // Call this from the registration flow when a user has successfully registered.
    public void OnRegistrationSuccess_ShowLogin()
    {
        if (registerPanel != null)
            registerPanel.gameObject.SetActive(false);

        if (loginPanel != null)
            loginPanel.gameObject.SetActive(true);
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("ActiveUser");
        PlayerPrefs.Save();

        if (logoutButton != null)
            logoutButton.gameObject.SetActive(false);

        if (activeUserDisplay != null)
            activeUserDisplay.gameObject.SetActive(false);

        if (activeUserBackground != null)
            activeUserBackground.gameObject.SetActive(false);

        if (usernameInput != null)
            usernameInput.text = string.Empty;

        if (passwordInput != null)
            passwordInput.text = string.Empty;

        if (loginPanel != null)
            loginPanel.gameObject.SetActive(true);

        if (playButton != null)
            playButton.gameObject.SetActive(true);

        if (startButton != null)
            startButton.gameObject.SetActive(false);

        UpdateLoggedInOnlyButtonState();
    }

    private void Start()
    {
        // Initialize UI based on login state
        bool isLoggedIn = PlayerPrefs.HasKey("ActiveUser");
        
        if (!isLoggedIn)
        {
            // No user logged in — hide logged-in UI
            if (activeUserDisplay != null)
                activeUserDisplay.gameObject.SetActive(false);
            if (activeUserBackground != null)
                activeUserBackground.gameObject.SetActive(false);
            if (logoutButton != null)
                logoutButton.gameObject.SetActive(false);
        }
        
        UpdateLoggedInOnlyButtonState();
        if (blockedClickPopup != null)
            blockedClickPopup.gameObject.SetActive(false);
    }

    private void UpdateLoggedInOnlyButtonState()
    {
        bool isLoggedIn = PlayerPrefs.HasKey("ActiveUser");

        if (loggedInOnlyButton != null)
        {
            Button btn = loggedInOnlyButton.GetComponent<Button>();
            if (btn != null)
                btn.interactable = isLoggedIn;
        }

        // If an overlay catcher is provided, enable it when NOT logged in so clicks can be caught
        if (loggedInOnlyButtonCatcher != null)
            loggedInOnlyButtonCatcher.gameObject.SetActive(!isLoggedIn);
    }

    // Hook this to the overlay catcher's Button onClick, or to the button itself if you prefer to allow clicks
    public void OnLoggedInOnlyButtonClicked()
    {
        bool isLoggedIn = PlayerPrefs.HasKey("ActiveUser");
        if (isLoggedIn)
        {
            // User is logged in — perform the intended action for this button.
            // Implement the action here or wire the real handler in the Inspector.
            return;
        }

        if (blockedClickPopup != null)
            StartCoroutine(ShowBlockedPopup());
    }

    private IEnumerator ShowBlockedPopup()
    {
        blockedClickPopup.gameObject.SetActive(true);
        yield return new WaitForSeconds(blockedMessageDuration);
        blockedClickPopup.gameObject.SetActive(false);
    }
}
